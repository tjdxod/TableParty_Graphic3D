using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

namespace Dive.VRModule
{
    public abstract partial class PXRRigMovementBase : MonoBehaviour, ICharacterController
    {
        public struct CharacterXRInputs
        {
            public Vector2 MoveAxis;
        }

        // Kinematic Character Controller 사용 예정
        [SerializeField]
        private KinematicCharacterMotor motor;

        [SerializeField]
        private float deadZoneY = -2f;
        
        protected CharacterXRInputs currentInputs;

        private CharacterState previousState = CharacterState.None;
        private Vector3 moveInputVector = Vector3.zero;
        private PXRPointerVR Pointer => PXRRig.LeftPointer;
        
        [field: SerializeField]
        protected Vector3 ResetPosition { get; set; }
        
        [field: SerializeField]
        protected Vector3 ResetDirection { get; set; }

        protected virtual void Awake()
        {
            motor.CharacterController = this;
            SetCharacterState(CharacterState.Default);
        }

        private void Update()
        {
            if (!motor.GroundingStatus.IsStableOnGround)
            {
                var currentPosY = transform.position.y;

                if (deadZoneY > currentPosY)
                {
                    // 강제로 이동시켜야 함.
                    
                    PXRRig.KinematicCharacterMotor.SetPositionAndRotation(ResetPosition, Quaternion.Euler(ResetDirection));
                    return;
                }
            }
            
            if (!PXRRig.PlayerController.UseMovement)
            {
                currentInputs = new CharacterXRInputs
                {
                    MoveAxis = Vector2.zero
                };
            
                SetInputs(ref currentInputs);
                return;
            }

            if (PXRRig.RightTeleporter.IsTeleporting)
            {
                SetCharacterState(CharacterState.Teleport);
                moveInputVector = Vector3.zero;
                return;
            }
            else
            {
                SetCharacterState(CharacterState.Default);
            }

            if (Pointer.PointerEventData != null)
            {
                var pointerEventData = Pointer.PointerEventData;
                var scrollDelta = Vector2.zero;
                var magnitude = 0f;
                
                if(pointerEventData != null)
                    scrollDelta = pointerEventData.scrollDelta;
            
                magnitude = scrollDelta.magnitude;

                var isScrollView = magnitude > 0.01f;

                if (isScrollView)
                {
                    currentInputs = new CharacterXRInputs
                    {
                        MoveAxis = Vector2.zero
                    };
            
                    SetInputs(ref currentInputs);
                }
            }
            
            HandleCharacterXRInput();
        }

        protected abstract void HandleCharacterXRInput();
        
        public virtual void SetResetPosition(Vector3 position)
        {
            ResetPosition = position;
        }
        
        public virtual void SetResetDirection(Vector3 direction)
        {
            ResetDirection = direction;
        }

        public void SetInputs(ref CharacterXRInputs inputs)
        {
            var inputVector = new Vector3(inputs.MoveAxis.x, 0, inputs.MoveAxis.y);
            var cam = Camera.main;

            if (cam == null)
                return;

            SetCharacterState(inputVector.sqrMagnitude > 0.01f ? CharacterState.Movement : CharacterState.Default);

            var cameraPlanarDirection = Vector3.ProjectOnPlane(cam.transform.rotation * Vector3.forward, motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(cam.transform.rotation * Vector3.up, motor.CharacterUp).normalized;
            }

            var cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, motor.CharacterUp);
            
            switch (CurrentState)
            {
                case CharacterState.None:
                case CharacterState.Teleport:
                    moveInputVector = Vector3.zero;
                    
                    break;
                case CharacterState.Default:
                    if (previousState == CharacterState.Movement)
                    {
                        PXRRig.PlayerController.OnAfterMovement();
                        PXRRig.LeftPointer.SetDefaultGradient();
                        PXRRig.RightPointer.SetDefaultGradient();
                    }
                    
                    moveInputVector = cameraPlanarRotation * inputVector;
                    break;
                case CharacterState.Movement:
                    if (previousState != CharacterState.Movement)
                    {
                        PXRRig.LeftPointer.SetMoveGradient();
                        PXRRig.RightPointer.SetMoveGradient();
                    }
                    
                    moveInputVector = cameraPlanarRotation * inputVector;
                    break;
            }
            
            previousState = CurrentState;
        }
    }
}