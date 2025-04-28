using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using UnityEngine.Serialization;

namespace Dive.VRModule
{
    public partial class PXRRigMovementBase
    {
        [field: SerializeField, ReadOnly]
        public CharacterState CurrentState { get; private set; } = CharacterState.None;
        
        public float MaxStableMoveSpeed = 10f / 5;
        public float StableMovementSharpness = 15f / 5;
        
        public float MaxAirMoveSpeed = 15f / 5;
        public float AirAccelerationSpeed = 15f / 5;
        public float Drag = 0.1f;
        
        public Vector3 Gravity = new Vector3(0, -30f / 5, 0);

        public List<Collider> IgnoredColliders = new List<Collider>();

        [SerializeField, ReadOnly]
        private PXRTeleportSpaceBase currentTeleportSpaceArea;

        public PXRTeleportSpaceBase CurrentTeleportSpaceArea => currentTeleportSpaceArea;
        
        public void SetCharacterState(CharacterState state)
        {
            if(CurrentState == state)
                return;
            
            var preState = CurrentState;
            CurrentState = state;
            OnStateExit(preState, state);
            OnStateEnter(preState, state);
        }

        private void OnStateEnter(CharacterState exitState, CharacterState enterState)
        {
            switch (enterState)
            {
                case CharacterState.None:
                    break;
                case CharacterState.Default:
                    break;
                case CharacterState.Movement:
                    break;
                case CharacterState.Teleport:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(enterState), enterState, null);
            }
        }

        private void OnStateExit(CharacterState exitState, CharacterState enterState)
        {
            switch (exitState)
            {
                case CharacterState.None:
                    break;
                case CharacterState.Default:
                    break;
                case CharacterState.Movement:
                    break;
                case CharacterState.Teleport:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exitState), exitState, null);
            }
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (CurrentState)
            {
                case CharacterState.Default:
                case CharacterState.Movement:
                    
                    // currentVelocity 방향으로 특정 레이어가 있는 경우 이동할 수 없음.
                    
                    var teleportMask = 1 << PXRNameToLayer.TeleportSpace;
                    
                    if (Physics.Raycast(motor.Transform.position, moveInputVector, out var hit, 0.5f, teleportMask))
                    {
                        var tp = hit.collider.GetComponent<PXRTeleportOwnerArea>();

                        if (tp != null && currentTeleportSpaceArea != null && tp.GetHashCode() != currentTeleportSpaceArea.GetHashCode() && tp.IsEnteredPlayer)
                        {
                            currentVelocity = Vector3.zero;
                            moveInputVector = Vector3.zero;
                            return;
                        }
                    }

                    var avatarMask = 1 << PXRNameToLayer.IgnoreAvatar | 1 << PXRNameToLayer.AvatarBody;

                    var height = motor.Capsule.height / 2;
                    
                    if (Physics.Raycast(motor.Transform.position + Vector3.up * height, moveInputVector, out var hit2, 0.5f, avatarMask))
                    {
                        currentVelocity = Vector3.zero;
                        moveInputVector = Vector3.zero;
                        return;
                    }
                    
                    if (motor.GroundingStatus.IsStableOnGround)
                    {
                        var currentVelocityMagnitude = currentVelocity.magnitude;
                        var effectiveGroundNormal = motor.GroundingStatus.GroundNormal;
                        
                        currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                        var inputRight = Vector3.Cross(moveInputVector, motor.CharacterUp);
                        var reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveInputVector.magnitude;
                        var targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
                    }
                    else
                    {
                        if (moveInputVector.sqrMagnitude > 0f)
                        {
                            var addedVelocity = moveInputVector * (AirAccelerationSpeed * deltaTime);
                            var currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
                    
                            if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
                            {
                                Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, MaxAirMoveSpeed);
                                addedVelocity = newTotal - currentVelocityOnInputsPlane;
                            }
                            else
                            {
                                if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                                {
                                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                                }
                            }
                            
                            if (motor.GroundingStatus.FoundAnyGround)
                            {
                                if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                                {
                                    var perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(motor.CharacterUp, motor.GroundingStatus.GroundNormal), motor.CharacterUp).normalized;
                                    addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                                }
                            }
                            
                            currentVelocity += addedVelocity;
                        }
                        
                        // Gravity
                        currentVelocity += Gravity * deltaTime;
                    
                        // Drag
                        currentVelocity *= (1f / (1f + (Drag * deltaTime)));
                    }
                    
                    break;
                case CharacterState.None:
                case CharacterState.Teleport:
                    currentVelocity = Vector3.zero;
                    break;
            }
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void AfterCharacterUpdate(float deltaTime)
        {

        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            var grabbable = coll.GetComponent<PXRGrabbable>();
            if (grabbable != null)
            {
                return false;
            }
            
            if (IgnoredColliders.Count == 0)
            {
                return true;
            }

            if (IgnoredColliders.Contains(coll))
            {
                return false;
            }

            return true;
        }

        public void SetTeleportSpaceArea(PXRTeleportSpaceBase spaceBase)
        {
            currentTeleportSpaceArea = spaceBase;
        }
        
        private void RefreshTeleportSpace(Collider hitCollider)
        {
            var teleportSpace = hitCollider.GetComponent<PXRTeleportSpaceBase>();

            if (teleportSpace == null)
                return;

            var ts = teleportSpace.GetTeleportSpace();
            
            if(currentTeleportSpaceArea == null)
            {
                currentTeleportSpaceArea = ts;
                ts.OnEnterPlayer();
                return;
            }

            if (ts.GetHashCode() == currentTeleportSpaceArea.GetHashCode())
                return;
            
            currentTeleportSpaceArea.OnExitPlayer();
            
            currentTeleportSpaceArea = ts;
            
            ts.OnEnterPlayer();
        }
        
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            RefreshTeleportSpace(hitCollider);
        }
        
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation,
            ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }
    }
}