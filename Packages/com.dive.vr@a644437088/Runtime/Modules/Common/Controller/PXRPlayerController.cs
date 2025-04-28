using System;
using Dive.VRModule.Locomotion;
using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 플레이어의 움직임을 관리하는 매니저
    /// </summary>
    public partial class PXRPlayerController : MonoBehaviour
    {
        #region Public Fields

        /// <summary>
        /// Rig의 스케일 변경 이벤트
        /// </summary>
#pragma warning disable CS0067 // Event is never used
        public event Action<float> AfterChangeRigScaleEvent;
#pragma warning restore CS0067 // Event is never used

        /// <summary>
        /// 비활성화 이벤트
        /// </summary>
        public event Action AfterDeactivatedAllEvent;
        
        #endregion

        #region Private Fields

        /// <summary>
        /// VR Rig 메인 카메라의 트랜스폼
        /// </summary>
        [SerializeField, Tooltip("Main Camera")]
        private Transform centerEye;        
        
        [SerializeField, Tooltip("Left Anchor")]
        private Transform leftHandAnchor;
        
        [SerializeField, Tooltip("Right Anchor")]
        private Transform rightHandAnchor;
        
        #endregion

        #region Public Properties

        public Transform CenterEye => centerEye;
        public Transform LeftHandAnchor => leftHandAnchor;
        public Transform RightHandAnchor => rightHandAnchor;
        
        public bool IsBothSnapTurnActive => isLeftSnapTurnActive && isRightSnapTurnActive;
        public bool IsBothPointerActive => isLeftPointerActive && isRightPointerActive;
        public bool IsBothTeleporterActive => isLeftTeleporterActive && isRightTeleporterActive;
        public bool IsBothGrabberActive => isLeftGrabberActive && isRightGrabberActive;
        public bool IsHeightChangeActive => isHeightChangeActive;
        public bool IsMovementActive => isMovementActive;
        
        #endregion
        
        #region Public Methods

        /// <summary>
        /// 모든 움직임을 활성화
        /// </summary>
        public void ActivateAll()
        {
            ActivateAllPointer();
            ActivateAllTeleporter();
            ActivateAllGrabber();
            ActivateAllSnapTurn();
            ActivateChangeEyeHeight();
            ActivateMovement();
        }

        /// <summary>
        /// 모든 움직임을 비활성화
        /// </summary>
        public void DeactivateAll()
        {
            DeactivateAllPointer();
            DeactivateAllTeleporter();
            DeactivateAllGrabber();
            DeactivateAllSnapTurn();
            DeactivateChangeEyeHeight();
            DeactivateMovement();

            AfterDeactivatedAllEvent?.Invoke();
        }        
        
        #endregion

        #region Private Methods

        private void Awake()
        {
            SnapTurns = GetComponentsInChildren<PXRSnapTurn>();
            Grabbers = GetComponentsInChildren<PXRGrabber>();
            HandAnimationController = GetComponentsInChildren<PXRHandAnimationController>();
            Pointers = GetComponentsInChildren<PXRPointerVR>();
            EyeHeightChanger = GetComponent<PXREyeHeightChanger>();
            Clickers = PXRRig.Current.GetComponentsInChildren<PXRClicker>();
            
            Teleporters = GetComponentsInChildren<PXRTeleporter>();
            
            CurrentHeight = transform.position.y;
            originTrackingSpaceY = standardHeight;
        }        
        
        #endregion
    }
}