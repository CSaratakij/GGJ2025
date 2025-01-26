using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public enum KnockBackType
    {
        Normal,
        High
    }
    
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, IKnockback, IImmobilize
    {
        public event Action OnDead;
        
        private const float ALLOW_PLAYER_ROTATION = 0.1f;
        private const float DESIRED_ROTATION_SPEED = 0.1f;
        
        [Header("General")]
        [SerializeField] private int playerIndex = 0;
        [SerializeField] private float respawnInvincibleDuration = 1.0f;
        
        [Header("Setting - Normal Movement")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float gravity = -9.81f;
        
        [Header("Setting - Knockback Movement")]
        [SerializeField] private KnockBackSetting[] knockBackSettings;
        
        [Header("Setting - Dash Movement")]
        [SerializeField] private float dashMoveSpeed = 5.0f;
        [SerializeField] float dashMoveSpeedMultipiler = 1.0f;
        [SerializeField] float dashDuration = 1.0f;
        [SerializeField] float dashCooldown = 1.0f;

        [Header("Setting - Punch")]
        [SerializeField, Required] private Transform punchHitboxOrigin;
        [SerializeField, Required] private float punchHitboxRadius = 1.0f;
        [SerializeField] private float punchDuration = 1.0f;
        [SerializeField] private float punchCooldown = 1.0f;
        [SerializeField] private LayerMask punchMask;
        
        [Header("Setting - Shoot")]
        [SerializeField, Required, AssetsOnly] private BubbleBullet bulletPrefab;
        [SerializeField, Required] private Transform shootMuzzleOrigin;
        [SerializeField] private float shootDuration = 1.0f;
        [SerializeField] private float shootCooldown = 1.0f;
        [SerializeField] private float bulletMoveSpeed = 0.25f;
        [SerializeField] private float bulletLifeTime = 3.0f;
        
        [Header("Setting - Immobilize")]
        [SerializeField] private float immobilizeDuration = 1.0f;
        [SerializeField, Required] private Transform immobilizeVisualParent;
        
        [Header("Animation Smoothing")]
        [Range(0, 1f)] [SerializeField] private float horizontalAnimSmoothTime = 0.2f;
        [Range(0, 1f)] [SerializeField] private float verticalAnimTime = 0.2f;
        [Range(0,1f)] [SerializeField] private float startAnimTime = 0.3f;
        [Range(0, 1f)] [SerializeField] private float stopAnimTime = 0.15f;

        [Serializable]
        public struct CharacterState
        {
            public bool isGrounded;
            public bool isMoveable;
        }

        [Serializable]
        public struct KnockBackSetting
        {
            public KnockBackType knockBackType;
            public float knockbackSpeed;
            public float knockbackSpeedMultipiler;
            public float knockbackDuration;
            public Vector3 knockbackDirection;
        }

        // Knockback Status
        private bool IsKnockBack => (Time.time <= knockbackTimer);
        private float knockbackTimer = 0.0f;
        private KnockBackType currentKnockBackType = KnockBackType.Normal;
        private KnockBackSetting currentKnockbackSetting;
        
        // Immobilize Status
        private bool IsImmobilize => (Time.time <= immobilizeTimer);
        private float immobilizeTimer = 0.0f;
        
        // Dash Status
        private bool IsDash => (Time.time <= dashTimer);
        private bool CanDash => (Time.time >= dashCooldownTimer) && characterState.isMoveable && characterState.isGrounded && !IsKnockBack && !IsImmobilize && !IsDash && !IsPunch;
        private bool isPressedDash = false;
        private float dashTimer = 0.0f;
        private float dashCooldownTimer = 0.0f;
        private Vector3 dashDirection = Vector3.forward;
        
        // Punch
        private bool IsPunch => (Time.time <= punchTimer);
        private bool CanPunch => (Time.time >= punchCooldownTimer) && characterState.isGrounded && !IsKnockBack && !IsImmobilize && !IsDash && !IsPunch;
        private bool isPressedPunch = false;
        private float punchTimer = 0.0f;
        private float punchCooldownTimer = 0.0f;
        
        // Shoot
        private bool IsShoot => (Time.time <= shootTimer);
        private bool CanShoot => (Time.time >= shootCooldownTimer) && characterState.isGrounded && !IsKnockBack && !IsImmobilize && !IsDash && !IsPunch && !IsShoot;
        private bool isPressedShoot = false;
        private float shootTimer = 0.0f;
        private float shootCooldownTimer = 0.0f;
        
        // Invincible Status
        private bool IsInvincible => (Time.time <= respawnInvincibleTimer) || IsDash;
        private float respawnInvincibleTimer = 0.0f;
        
        private Vector3 originalSpawnPosition;
        private CharacterState characterState;
        private Vector2 gravityVector;
        private Vector3 inputVector;
        private Vector3 desiredMoveDirection;
        private Vector3 lastNonZeroDesiredMoveDirection;
        private Animator animator;
        private Camera mainCamera;
        private CharacterController characterController;
        private CharacterSkinControllerLite characterSkinControllerLite;
        private PlayerInput playerInput;
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (punchHitboxOrigin)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(punchHitboxOrigin.position, punchHitboxRadius);
            }
        }
        #endif
        
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            characterSkinControllerLite = GetComponent<CharacterSkinControllerLite>();
            animator = GetComponent<Animator>();
            playerInput = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            mainCamera = Camera.main;
            originalSpawnPosition = transform.position;
            lastNonZeroDesiredMoveDirection = Vector3.forward;
            characterSkinControllerLite.ChangeMaterialSettings(playerIndex);
            characterState.isMoveable = true;
            animator.SetTrigger("angry");
        }

        private void Update()
        {
            AnimationHandler();
            MoveHandler();
            ActionActivationHandler();

            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //characterState.isMoveable = !characterState.isMoveable;
                Knockback(-Vector3.forward, KnockBackType.Normal);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Knockback(-Vector3.forward, KnockBackType.High);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Dash(transform.forward);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Immobilize();
            }
            #endif
        }
        
        private void LateUpdate()
        {
            RotateHandler();

            if (immobilizeVisualParent)
            {
                immobilizeVisualParent.gameObject.SetActive(IsImmobilize);
            }
        }

        private void OnDestroy()
        {
            OnDead = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bullet"))
            {
                BubbleBullet bullet = other.GetComponent<BubbleBullet>();
                bool shouldForceBulletToStop = bullet.IsStartOperate && (bullet.Owner != this.gameObject) && IsInvincible;

                if (shouldForceBulletToStop)
                {
                    bullet.ForceStopOperation();
                }
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            inputVector = context.ReadValue<Vector2>();
            
            if (inputVector.magnitude > 1)
            {
                inputVector.Normalize();
            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (!isPressedDash)
            {
                isPressedDash = true;
            }
        }

        public void OnPunch(InputAction.CallbackContext context)
        {
            if (!isPressedPunch)
            {
                isPressedPunch = true;
            }
        }

        public void OnShoot(InputAction.CallbackContext context)
        {
            if (!isPressedShoot)
            {
                isPressedShoot = true;
            }
        }

        public void ForceDead()
        {
            OnDead?.Invoke();
            Respawn();
        }
        
        public void Respawn()
        {
            characterController.enabled = false;
            transform.position = originalSpawnPosition;
            characterController.enabled = true;
            knockbackTimer = 0.0f;
            dashTimer = 0.0f;
            respawnInvincibleTimer = (Time.time + respawnInvincibleDuration);
        }

        private void AnimationHandler()
        {
            bool shouldBlendAnimationMoveSpeed = (characterState.isMoveable && !IsKnockBack && !IsPunch);

            if (shouldBlendAnimationMoveSpeed)
            {
                float speed = new Vector2(inputVector.x, inputVector.y).sqrMagnitude;
                
                if (speed > ALLOW_PLAYER_ROTATION)
                {
                    animator.SetFloat("Blend", speed, startAnimTime, Time.deltaTime);
                }
                else if (speed < ALLOW_PLAYER_ROTATION)
                {
                    animator.SetFloat("Blend", speed, stopAnimTime, Time.deltaTime);
                }
            }
            else
            {
                animator.SetFloat("Blend", 0.0f, 0.0f, Time.deltaTime);
            }
        }
        
        private void MoveHandler()
        {
            characterState.isGrounded = characterController.isGrounded;

            bool shouldResetGravity = characterState.isGrounded && (gravityVector.y < 0.0f);
            
            if (shouldResetGravity)
            {
                gravityVector = Vector3.zero;
            }
            
            gravityVector.y = (gravity * Time.deltaTime);

            Vector3 currentMoveDirection = desiredMoveDirection;
            float currentMoveSpeed = moveSpeed;
            float currentMoveSpeedMultiplier = 1.0f;
            
            bool shouldUseKnockback = (Time.time <= knockbackTimer);
            bool shouldUseDash = (Time.time <= dashTimer);
            bool shouldStandStill = (IsImmobilize && !IsKnockBack) || IsPunch;
            
            if (shouldStandStill)
            {
                 currentMoveDirection = Vector3.zero;
                 currentMoveSpeed = 0.0f;
                 currentMoveSpeedMultiplier = 1.0f;               
            }
            else if (shouldUseKnockback)
            {
                currentMoveDirection = currentKnockbackSetting.knockbackDirection;
                currentMoveSpeed = currentKnockbackSetting.knockbackSpeed;
                currentMoveSpeedMultiplier = currentKnockbackSetting.knockbackSpeedMultipiler;
            }
            else if (shouldUseDash)
            {
                 currentMoveDirection = dashDirection;
                 currentMoveSpeed = dashMoveSpeed;
                 currentMoveSpeedMultiplier = dashMoveSpeedMultipiler;               
            }
            else if (characterState.isMoveable)
            {
                Vector3 forwardBasis = mainCamera.transform.forward;
                Vector3 rightBasis = mainCamera.transform.right;

                forwardBasis.y = 0.0f;
                rightBasis.y = 0.0f;
                
                forwardBasis.Normalize();
                rightBasis.Normalize();

                desiredMoveDirection = (forwardBasis * inputVector.y) + (rightBasis * inputVector.x);

                if (desiredMoveDirection.sqrMagnitude > 0.0f)
                {
                    lastNonZeroDesiredMoveDirection = desiredMoveDirection;
                }

                currentMoveDirection = desiredMoveDirection;
            }
            else
            {
                desiredMoveDirection = Vector3.zero;
                currentMoveDirection = Vector3.zero;
            }
            
            Vector3 velocity = currentMoveDirection * (Time.deltaTime * currentMoveSpeed * currentMoveSpeedMultiplier);
            velocity.y = gravityVector.y;
            
            characterController.Move(velocity);
        }

        private void ActionActivationHandler()
        {
            // Dash
            if (isPressedDash)
            {
                isPressedDash = false;
                Dash(transform.forward);
            }
            
            // Punch
            if (isPressedPunch)
            {
                isPressedPunch = false;
                Punch();
            }
            
            // Shoot
            if (isPressedShoot)
            {
                isPressedShoot = false;
                Shoot();
            }
        }
        
        private void RotateHandler()
        {
            Quaternion lookDirection = Quaternion.LookRotation(lastNonZeroDesiredMoveDirection);
            transform.rotation = Quaternion.Slerp (transform.rotation, lookDirection, DESIRED_ROTATION_SPEED);
        }

        public void Knockback(Vector3 knockbackDirection)
        {
            var knockbackType = IsImmobilize ? KnockBackType.High : KnockBackType.Normal;
            Knockback(knockbackDirection, knockbackType);
        }

        private void Knockback(Vector3 knockbackDirection, KnockBackType knockbackType)
        {
            currentKnockBackType = knockbackType;
            currentKnockbackSetting = knockBackSettings.FirstOrDefault(x => x.knockBackType == knockbackType);
            currentKnockbackSetting.knockbackDirection = knockbackDirection;
            knockbackTimer = (Time.time + currentKnockbackSetting.knockbackDuration);
        }

        public void Immobilize()
        {
            if (IsImmobilize)
            {
                return;
            }

            immobilizeTimer = (Time.time + immobilizeDuration);
        }

        public bool CanImmobilize()
        {
            return !IsInvincible;
        }
        
        private void Dash(Vector3 dashDirection)
        {
            if (CanDash)
            {
                this.dashDirection = dashDirection;
                dashTimer = (Time.time + dashDuration);
                dashCooldownTimer = (Time.time + dashCooldown);
            }
        }

        public void Punch()
        {
            if (!CanPunch)
            {
                return;
            }
            
            punchTimer = (Time.time + punchDuration);
            punchCooldown = (Time.time + punchCooldown);
            
            animator.ResetTrigger("punch");
            animator.SetTrigger("punch");
            
            // Hit Checks
            Collider[] hits = Physics.OverlapSphere(punchHitboxOrigin.transform.position, punchHitboxRadius, punchMask, QueryTriggerInteraction.Collide);
            
            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    bool isSelf = hits[i].gameObject == this.gameObject;

                    if (isSelf)
                    {
                        continue;
                    }

                    Collider target = hits[i];
                    IKnockback knockbackAffector = target.gameObject.GetComponent<IKnockback>();

                    if (knockbackAffector != null)
                    {
                        Vector3 knockbackDirection = (target.gameObject.transform.position - transform.position);
                        knockbackDirection.y = 0;

                        if (knockbackDirection.magnitude > 1.0f)
                        {
                            knockbackDirection.Normalize();
                        }
                        
                        knockbackAffector.Knockback(knockbackDirection);
                    }
                    
                    Debug.Log($"Punch : {target.gameObject.name}", target.gameObject);
                }
            }
        }

        private void Shoot()
        {
            if (!CanShoot)
            {
                Debug.Log($"Cannot shoot");
                return;
            }
            
            shootTimer = (Time.time + shootDuration);
            shootCooldownTimer = (Time.time + shootCooldown);
            
            animator.ResetTrigger("shoot");
            animator.SetTrigger("shoot");

            bool isValid = (shootMuzzleOrigin != null) && (bulletPrefab != null);

            if (isValid)
            {
                var bullet = Instantiate(bulletPrefab, shootMuzzleOrigin.transform.position, Quaternion.identity);
                bullet.StartOperation(moveDirection: transform.forward.normalized, moveSpeed: bulletMoveSpeed, lifeTime: bulletLifeTime, owner: this.gameObject);
            }
        }
    }
}
