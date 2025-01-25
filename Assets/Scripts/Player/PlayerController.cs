using System;
using System.Linq;
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
    public class PlayerController : MonoBehaviour
    {
        private const float ALLOW_PLAYER_ROTATION = 0.1f;
        private const float DESIRED_ROTATION_SPEED = 0.1f;
        
        [Header("General")]
        [SerializeField] private int playerIndex = 0;
        
        [Header("Setting - Normal Movement")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float gravity = -9.81f;
        
        [Header("Setting - Knockback Movement")]
        [SerializeField] private KnockBackSetting[] knockBackSettings;
        
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
        
        // Dash Status
        private bool IsDash => (Time.time <= dashTimer);
        private bool CanDash => characterState.isMoveable && characterState.isGrounded && !IsKnockBack && !IsImmobilize;
        private float dashTimer = 0.0f;
        private float dashCooldownTimer = 0.0f;
        
        // Immobilize Status
        private bool IsImmobilize => false; // TODO
        
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
            lastNonZeroDesiredMoveDirection = Vector3.forward;
            characterSkinControllerLite.ChangeMaterialSettings(playerIndex);
            characterState.isMoveable = true;
            animator.SetTrigger("angry");
        }

        private void Update()
        {
            AnimationHandler();
            MoveHandler();

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
                Dash(Vector3.forward);
            }
            #endif
        }

        private void LateUpdate()
        {
            RotateHandler();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            inputVector = context.ReadValue<Vector2>();
            
            if (inputVector.magnitude > 1)
            {
                inputVector.Normalize();
            }
        }

        // TODO : for respawn
        public void ResetCharacterState()
        {
            
        }

        private void AnimationHandler()
        {
            bool shouldBlendAnimationMoveSpeed = (characterState.isMoveable && !IsKnockBack);

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
            
            if (shouldUseKnockback)
            {
                currentMoveDirection = currentKnockbackSetting.knockbackDirection;
                currentMoveSpeed = currentKnockbackSetting.knockbackSpeed;
                currentMoveSpeedMultiplier = currentKnockbackSetting.knockbackSpeedMultipiler;
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

        private void RotateHandler()
        {
            Quaternion lookDirection = Quaternion.LookRotation(lastNonZeroDesiredMoveDirection);
            transform.rotation = Quaternion.Slerp (transform.rotation, lookDirection, DESIRED_ROTATION_SPEED);
        }

        public void Knockback(Vector3 knockbackDirection, KnockBackType knockbackType)
        {
            currentKnockBackType = knockbackType;
            currentKnockbackSetting = knockBackSettings.FirstOrDefault(x => x.knockBackType == knockbackType);
            currentKnockbackSetting.knockbackDirection = knockbackDirection;
            knockbackTimer = (Time.time + currentKnockbackSetting.knockbackDuration);
        }

        // TODO
        public void Dash(Vector3 dashDirection)
        {
            if (!CanDash)
            {
                return;
            }
            
            // TODO
        }
    }
}
