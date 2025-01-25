using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private const float ALLOW_PLAYER_ROTATION = 0.1f;
        private const float DESIRED_ROTATION_SPEED = 0.1f;
        
        [Header("Setting")]
        [SerializeField] private int playerIndex = 0;
        [SerializeField] private float velocity = 5.0f;
        
        [Header("Animation Smoothing")]
        [Range(0, 1f)] [SerializeField] private float horizontalAnimSmoothTime = 0.2f;
        [Range(0, 1f)] [SerializeField] private float verticalAnimTime = 0.2f;
        [Range(0,1f)] [SerializeField] private float startAnimTime = 0.3f;
        [Range(0, 1f)] [SerializeField] private float stopAnimTime = 0.15f;
        
        private bool isGrouned;
        private Vector3 inputVector;
        private Vector3 moveVector;
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
        }

        private void Update()
        {
            AnimationHandler();
            MoveHandler();
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

        private void AnimationHandler()
        {
            float speed = new Vector2(inputVector.x, inputVector.y).sqrMagnitude;
            
            if (speed > ALLOW_PLAYER_ROTATION) {
                animator.SetFloat ("Blend", speed, startAnimTime, Time.deltaTime);
            } else if (speed < ALLOW_PLAYER_ROTATION) {
                animator.SetFloat ("Blend", speed, stopAnimTime, Time.deltaTime);
            }
        }
        
        private void MoveHandler()
        {
            isGrouned = characterController.isGrounded;

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
            
            characterController.Move(desiredMoveDirection * (Time.deltaTime * velocity));
        }

        private void RotateHandler()
        {
            Quaternion lookDirection = Quaternion.LookRotation(lastNonZeroDesiredMoveDirection);
            transform.rotation = Quaternion.Slerp (transform.rotation, lookDirection, DESIRED_ROTATION_SPEED);
        }
    }
}
