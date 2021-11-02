using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace alisahanyalcin
{
    public class ATPCInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        [SerializeField] private Vector2 move;
        [SerializeField] private Vector2 look;
        [SerializeField] private bool jump;
        [SerializeField] private bool crouch;
        [SerializeField] private bool sprint;
        [SerializeField] private bool roll;

        [Header("Movement Settings")]
        [SerializeField] private bool analogMovement;

        [Header("Mouse Cursor Settings")]
        [SerializeField] private bool cursorInputForLook = true;

        public void OnMove(InputAction.CallbackContext value)
        {
            MoveInput(value.action.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext value)
        {
            if(cursorInputForLook)
            {
                LookInput(value.action.ReadValue<Vector2>());
            }
        }

        public void OnJump(InputAction.CallbackContext value)
        {
            JumpInput(value.action.triggered);
        }

        public void OnRoll(InputAction.CallbackContext value)
        {
            RollInput(value.action.triggered);
        }

        public void OnCrouch(InputAction.CallbackContext value)
        {
            CrouchInput(value.action.triggered);
        }

        public void OnSprint(InputAction.CallbackContext value)
        {
            SprintInput(value.action.ReadValue<float>() == 1f);
        }

        private void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        private void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        private void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        private void RollInput(bool newRollState)
        {
            roll = newRollState;
        }

        private void CrouchInput(bool newCrouchState)
        {
            crouch = newCrouchState;
        }

        private void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public Vector2 GetMove()
        {
            return move;
        }

        public Vector2 GetLook()
        {
            return look;
        }

        public bool IsJumping()
        {
            return jump;
        }

        public bool IsRolling()
        {
            return roll;
        }

        public bool IsCrouching()
        {
            return crouch;
        }

        public bool IsSprinting()
        {
            return sprint;
        }

        public bool IsAnalog()
        {
            return analogMovement;
        }
    }
}