using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace alisahanyalcin
{
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("References")]
        public Animator animator;
        public CharacterController controller;
        public ATPCInputs input;

        [Header("Player")]
        [SerializeField] private float walkSpeed = 3.0f;
        [SerializeField] private float crouchSpeed = 2.0f;
        [SerializeField] private float sprintSpeed = 6.0f;
        [SerializeField] private float rotationSmoothTime = 0.005f;
        [SerializeField] private float smoothSpeed = 0.15f;
        [SerializeField] private float speedChangeRate = 10.0f;
        [SerializeField] private bool isPlayerDied = false;

        [Header("Jump")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -15.0f;
        [SerializeField] private float jumpTimeout = 0.5f;
        [SerializeField] private float fallTimeout = 0.15f;

        [Header("Player Grounded")]
        [SerializeField] private bool grounded = true;
        [SerializeField] private float groundedOffset = -0.14f;
        [SerializeField] private float groundedRadius = 0.28f;
        public GameObject spherePosition;
        public LayerMask groundLayers;

        [Header("Cinemachine")]
        public GameObject cinemachineCameraTarget;
        [SerializeField] private float topClamp = 70.0f;
        [SerializeField] private float bottomClamp = -30.0f;

        [Header("Ragdoll")]
        [SerializeField] private Rigidbody[] ragdollBodies;
        [SerializeField] private float explosionForce;
        [SerializeField] private float explosionRadius;

        [Header("Cinemachine")]
        public Camera mainCamera;
        [SerializeField] private float cameraAngleOverride = 0.0f;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private const float TerminalVelocity = 53.0f;

        private Vector2 _currentVector;
        private Vector2 _smoothInputVelocity;

        // timeout
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation Hash
        private int _animHashSpeed;
        private int _animHashGrounded;
        private int _animHashJump;
        private int _animHashRoll;
        private int _animHashCrouch;
        private int _animHashFreeFall;
        private int _animHashMotionSpeed;
        private float _targetRotation = 0.0f;
        private const float SpeedOffset = 0.1f;

        private const float Threshold = 0.01f;
        private void Start()
        {
            AssignAnimationHash();
            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;

            ragdollBodies = GetComponentsInChildren<Rigidbody>();
            ToggleRagdoll(false);
        }

        private void Update()
        {
            switch (isPlayerDied)
            {
                case true:
                    Die();
                    break;
                case false:
                    JumpAndGravity();
                    GroundedCheck();
                    Crouch();
                    Roll();
                    Move();
                    CameraRotation();
                    break;
            }
        }

        private void AssignAnimationHash()
        {
            _animHashSpeed = Animator.StringToHash("Speed");
            _animHashGrounded = Animator.StringToHash("Grounded");
            _animHashJump = Animator.StringToHash("Jump");
            _animHashRoll = Animator.StringToHash("Roll");
            _animHashCrouch = Animator.StringToHash("Crouch");
            _animHashFreeFall = Animator.StringToHash("FreeFall");
            _animHashMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void CameraRotation()
        {
            Vector2 look = input.GetLook();
            if (look.sqrMagnitude >= Threshold)
            {
                _cinemachineTargetPitch += look.y * Time.deltaTime;
                _rotationVelocity = look.x * Time.deltaTime;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

            cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            Vector2 getMove = input.GetMove();
            _currentVector = Vector2.SmoothDamp(_currentVector, getMove, ref _smoothInputVelocity, smoothSpeed) * Time.deltaTime;

            float targetSpeed = walkSpeed;

            if (input.IsSprinting())
                targetSpeed = sprintSpeed;

            if (input.IsCrouching())
                targetSpeed = crouchSpeed;

			if (getMove == Vector2.zero) targetSpeed = 0.0f;

            var velocity = controller.velocity;
            float currentHorizontalSpeed = new Vector3(velocity.x, 0.0f, velocity.z).magnitude;

			float inputMagnitude = input.IsAnalog() ? getMove.magnitude : 1f;

			if (currentHorizontalSpeed < targetSpeed - SpeedOffset || currentHorizontalSpeed > targetSpeed + SpeedOffset)
			{
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);

				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
                _speed = targetSpeed;

			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);

			Vector3 inputDirection = new Vector3(_currentVector.x, 0.0f, _currentVector.y).normalized;

			if (_currentVector != Vector2.zero)
			{
				_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
				float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmoothTime);

				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}

			Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

			controller.Move(targetDirection * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			animator.SetFloat(_animHashSpeed, _animationBlend);
			animator.SetFloat(_animHashMotionSpeed, inputMagnitude);
		}

        private void Crouch()
        {
            animator.SetBool(_animHashCrouch, input.IsCrouching());
        }

        private void Roll()
        {
            if (controller.velocity != Vector3.zero)
                animator.SetBool(_animHashRoll, input.IsRolling());
        }

        private void JumpAndGravity()
        {
            if (grounded)
            {
                _fallTimeoutDelta = fallTimeout;

                animator.SetBool(_animHashJump, false);
                animator.SetBool(_animHashFreeFall, false);

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (input.IsJumping() && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                    animator.SetBool(_animHashJump, true);
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = jumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    _jumpTimeoutDelta = jumpTimeout;

                    animator.SetBool(_animHashFreeFall, true);

                    if (_fallTimeoutDelta >= 0.0f)
                    {
                        _fallTimeoutDelta -= Time.deltaTime;
                    }
                }
            }

            if (_verticalVelocity < TerminalVelocity)
                _verticalVelocity += gravity * Time.deltaTime;
        }

        private void GroundedCheck()
        {
	        grounded = Physics.CheckSphere(spherePosition.transform.position, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
	        animator.SetBool(_animHashGrounded, grounded);
        }

        private void Die()
        {
            ToggleRagdoll(true);
            foreach (Rigidbody rb in ragdollBodies)
            {
                rb.AddExplosionForce(explosionForce, new Vector3(-1f, 0.5f, -1f), explosionRadius, 0f, ForceMode.Impulse);
            }
        }
        private void ToggleRagdoll(bool state)
        {
            animator.enabled = !state;
            foreach (Rigidbody rb in ragdollBodies)
            {
                rb.isKinematic = !state;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = grounded ? transparentGreen : transparentRed;

            var position = transform.position;
            Gizmos.DrawSphere(new Vector3(position.x, position.y - groundedOffset, position.z), groundedRadius);
        }
    }
}
