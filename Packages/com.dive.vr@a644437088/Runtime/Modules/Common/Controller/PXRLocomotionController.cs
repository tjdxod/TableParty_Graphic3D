using System;
using Dive.VRModule.Locomotion;
using UnityEngine;

namespace Dive.VRModule
{
    // PXRLocomotionController.cs
    public partial class PXRPlayerController
    {
        #region Public Fields

        public event Action AfterMovementEvent;

        #endregion
        
        #region Private Fields

        private PXRSnapTurn leftSnapTurn;
        private PXRSnapTurn rightSnapTurn;
        
        private PXRTeleporter leftTeleporter;
        private PXRTeleporter rightTeleporter;
        
        private bool isLeftSnapTurnActive = false;
        private bool isRightSnapTurnActive = false;
        
        private bool isLeftTeleporterActive = false;
        private bool isRightTeleporterActive = false;
        
        private bool isMovementActive = false;
        
        #endregion
        
        #region Public Properties

        [field: SerializeField]
        public bool UseMovement { get; private set; } = true;
        
        /// <summary>
        /// Teleport, SnapTurn으로 이동중인지 확인
        /// </summary>
        public bool IsMoving { get; set; } 
        
        /// <summary>
        /// Rig의 양손 SnapTurn
        /// </summary>
        public PXRSnapTurn[] SnapTurns { get; private set; }

        /// <summary>
        /// Rig의 양손 Teleporter
        /// </summary>
        public PXRTeleporter[] Teleporters { get; private set; }
        
        /// <summary>
        /// 텔레포트 위치 피봇
        /// </summary>
        [field: SerializeField]
        public Transform TeleportPivot { get; private set; }
        
        /// <summary>
        /// Teleport, SnapTurn으로 이동중인지 확인
        /// </summary>
        /// <summary>
        /// 이전에 위치한 TeleportSpace
        /// </summary>
        public PXRTeleportSpaceBase PrevTeleportSpace { get; private set; }
        
        /// <summary>
        /// 왼손 컨트롤러 회전 관리
        /// </summary>
        public PXRSnapTurn LeftSnapTurn
        {
            get
            {
                if (leftSnapTurn == null)
                {
                    leftSnapTurn = SnapTurns.Find(p => p.HandSide == HandSide.Left);

                    if (leftSnapTurn == null)
                        Debug.LogWarning("LeftSnapTurn is null");
                }

                return leftSnapTurn;
            }
        }
        
        /// <summary>
        /// 오른손 컨트롤러 회전 관리
        /// </summary>
        public PXRSnapTurn RightSnapTurn
        {
            get
            {
                if (rightSnapTurn == null)
                {
                    rightSnapTurn = SnapTurns.Find(p => p.HandSide == HandSide.Right);
                    
                    if (rightSnapTurn == null)
                        Debug.LogWarning("RightSnapTurn is null");
                }

                return rightSnapTurn;
            }       
        }
        
        /// <summary>
        /// 왼손 컨트롤러 텔레포트 관리
        /// </summary>
        public PXRTeleporter LeftTeleporter
        {
            get
            {
                if (leftTeleporter == null)
                {
                    leftTeleporter = Teleporters.Find(p => p.HandSide == HandSide.Left);
                }

                return leftTeleporter;
            }
        }
        
        /// <summary>
        /// 오른손 컨트롤러 텔레포트 관리
        /// </summary>
        public PXRTeleporter RightTeleporter
        {
            get
            {
                if (rightTeleporter == null)
                {
                    rightTeleporter = Teleporters.Find(p => p.HandSide == HandSide.Right);
                }

                return rightTeleporter;
            }
        }
        
        #endregion
        
        #region Public Methods

        public void OnAfterMovement()
        {
            AfterMovementEvent?.Invoke();
        }
        
        /// <summary>
        /// 텔레포터를 활성화
        /// </summary>
        /// <param name="handSide">손 방향</param>
        public void ActivateTeleporter(HandSide handSide)
        {
            foreach (var teleporter in Teleporters)
            {
                if (teleporter.HandSide == handSide)
                {
                    teleporter.enabled = true;
                }
            }
            
            if (handSide == HandSide.Left)
                isLeftTeleporterActive = true;
            else
                isRightTeleporterActive = true;
        }

        /// <summary>
        /// 텔레포터 비활성화
        /// </summary>
        /// <param name="handSide">손 방향</param>
        public void DeactivateTeleporter(HandSide handSide)
        {
            foreach (var teleporter in Teleporters)
            {
                if (teleporter.HandSide == handSide)
                {
                    teleporter.CancelTeleport();
                    teleporter.enabled = false;
                }
            }
            
            if (handSide == HandSide.Left)
                isLeftTeleporterActive = false;
            else
                isRightTeleporterActive = false;
        }

        /// <summary>
        /// Scene 이동 시 OwnerArea 초기화
        /// </summary>
        public void SetOwnerPoint(PXRTeleportSpaceBase space)
        {
            PXRRig.Current.OwnerArea = space;
        }
        
        /// <summary>
        /// 모든 텔레포터 활성화
        /// </summary>
        public void ActivateAllTeleporter()
        {
            foreach (var teleporter in Teleporters)
            {
                teleporter.enabled = true;
            }
            
            isLeftTeleporterActive = true;
            isRightTeleporterActive = true;
        }

        /// <summary>
        /// 모든 텔레포트 비활성화
        /// </summary>
        public void DeactivateAllTeleporter()
        {
            foreach (var teleporter in Teleporters)
            {
                teleporter.CancelTeleport();
                teleporter.enabled = false;
            }
            
            isLeftTeleporterActive = false;
            isRightTeleporterActive = false;
        }

        /// <summary>
        /// 강제로 특정 위치 / 특정 방향을 바라보며 텔레포트
        /// </summary>
        /// <param name="position">이동할 월드 좌표</param>
        /// <param name="direction">바라보는 방향</param>
        public void ForceTeleport(Vector3 position, Vector3 direction)
        {
            var teleporter = LeftTeleporter.enabled ? LeftTeleporter : RightTeleporter;
            teleporter.ForceTeleport(position, direction);
        }
        
        /// <summary>
        /// 입력받은 방향으로 회전 (Camera의 위치 잘 확인할것)
        /// 이동과 회전을 동시에 할 시 회전이 먼저되야함
        /// </summary>
        /// <param name="direction">회전 방향</param>
        public void TurnToDirection(Vector3 direction)
        {
            direction.y = 0f;

            var start = centerEye.forward;
            start.y = 0f;

            var angle = Vector3.Angle(start, direction);

            var cross = Vector3.Cross(start, direction);
            if (cross.y < 0)
                angle *= -1f;

            TurnToAngle(angle);

            TeleportPivot.forward = direction;
        }

        /// <summary>
        /// 고정된 위치로 이동
        /// </summary>
        /// <param name="destination">이동 위치</param>
        public void MoveToFixedDestination(Vector3 destination)
        {
            TeleportPivot.position = destination;

            var vec = PXRRig.Current.transform.position - centerEye.position;
            vec.y = 0f;
            vec += destination;

            // 높이는 그대로 유지하기 위해
            //vec.y = destination.y + CurrentLocalHeight;

            var noY = vec;
            
            // TH 수정 코드 : 22.04.20 텔레포트한 높이가 다른 경우
            // vec.y = destination.y + CurrentHeight;
            vec.y = CurrentHeight;

            // ReSharper disable once Unity.InefficientPropertyAccess
            // transform.position = vec;
            PXRRig.KinematicCharacterMotor.SetPositionAndRotation(noY, PXRRig.Current.transform.rotation);
            transform.position = vec;
        }

        /// <summary>
        /// SnapTurn을 반환
        /// </summary>
        /// <param name="handSide">손 방향</param>
        /// <returns>손의 SnapTurn</returns>
        public PXRSnapTurn GetSnapTurn(HandSide handSide)
        {
            foreach (var snapTurn in SnapTurns)
            {
                if (snapTurn.HandSide == handSide)
                    return snapTurn;
            }

            return null;
        }

        /// <summary>
        /// 모든 SnapTurn을 활성화
        /// </summary>
        public void ActivateAllSnapTurn()
        {
            foreach (var snapTurn in SnapTurns)
            {
                snapTurn.enabled = true;
            }
            
            isLeftSnapTurnActive = true;
            isRightSnapTurnActive = true;
        }

        /// <summary>
        /// 모든 SnapTurn을 비활성화
        /// </summary>
        public void DeactivateAllSnapTurn()
        {
            foreach (var snapTurn in SnapTurns)
            {
                snapTurn.CancelSnapTurn();
                snapTurn.enabled = false;
            }
            
            isLeftSnapTurnActive = false;
            isRightSnapTurnActive = false;
        }

        /// <summary>
        /// 입력받은 각도만큼 회전
        /// </summary>
        /// <param name="angle">회전 각도</param>
        public void TurnToAngle(float angle)
        {
            var rig = PXRRig.Current.transform;
            PXRRig.KinematicCharacterMotor.SetPositionAndRotation(rig.position, rig.rotation * Quaternion.Euler(0f, angle, 0f));
        }
        
        public void ChangePrevTeleportPoint(PXRTeleportSpaceBase space)
        {
            PrevTeleportSpace = space;
        }
        
        /// <summary>
        /// 컨트롤러 이동을 활성화
        /// </summary>
        public void ActivateMovement()
        {
            UseMovement = true;
            
            isMovementActive = true;
        }
        
        /// <summary>
        /// 컨트롤러 이동을 비활성화
        /// </summary>
        public void DeactivateMovement()
        {
            UseMovement = false;
            
            isMovementActive = false;
        }
        
        #endregion
    }
}