using Dive.VRModule.Locomotion;
using Dive.Utility;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dive.VRModule
{
    /// <summary>
    /// 플레이어 Rig
    /// </summary>
    public partial class PXRRig : MonoBehaviour
    {
        #region Private Fields

        private static StaticVar<PXRRig> instance;

        [field: SerializeField]
        private bool isVRPlay;

        [field: SerializeField]
        private bool isHandTrackingMode;
        
        [field: SerializeField, Range(0.1f, 2.0f)]
        private float handClampDistance = 1.5f;
        
        [field: SerializeField, Range(0.1f, 2.0f)]
        private float handDisableDistance = 1.75f;
        
        private PXRPlayerController pxrPlayerController;
        
        // Deprecate
        /*
        private PXRRecenter pxrRecenter;
        */
        private KinematicCharacterMotor motor;
        private PXRRigMovementBase movement;

        #endregion

        #region Public Properties

        /// <summary>
        /// VR 플레이인 경우 true, 그렇지 않은 경우 false
        /// PC 플레이를 지원할 시 사용
        /// </summary>
        public static bool IsVRPlay => Instance != null && Instance.isVRPlay;

        /// <summary>
        /// 손 추적 모드인지 체크
        /// </summary>
        public static bool IsHandTrackingMode => Instance != null && Instance.isHandTrackingMode;
        
        public static float HandClampDistance => Instance == null ? 0 : Instance.handClampDistance;
        public static float HandDisableDistance => Instance == null ? 0 : Instance.handDisableDistance;

        /// <summary>
        /// Rig를 반환
        /// </summary>
        public static PXRRig Current => Instance;
        
        [field: SerializeField, ReadOnly]
        public PXRTeleportSpaceBase OwnerArea { get; internal set; }

        [field: SerializeField]
        public float OutlineWidth { get; private set; }

        /// <summary>
        /// 상호작용 가능한 색상
        /// </summary>
        [field: SerializeField]
        public Color CanInteractColor { get; private set; }

        /// <summary>
        /// 상호작용 불가능한 색상
        /// </summary>
        [field: SerializeField]
        public Color CanNotInteractColor { get; private set; }

        /// <summary>
        /// 잡은 오브젝트의 색상
        /// </summary>
        [field: SerializeField]
        public Color GrabbedColor { get; private set; }

        /// <summary>
        /// Rig이 존재하는지 확인
        /// </summary>
        public static bool IsValid => Instance != null;

        /// <summary>
        /// Rig의 PlayerController를 반환
        /// </summary>
        public static PXRPlayerController PlayerController => Instance == null ? null : Instance.PxrPlayerController;

        /// <summary>
        /// Rig의 왼손 SnapTurn
        /// </summary>
        public static PXRSnapTurn LeftSnapTurn => Instance == null ? null : Instance.PxrPlayerController.LeftSnapTurn;

        /// <summary>
        /// Rig의 오른손 SnapTurn
        /// </summary>
        public static PXRSnapTurn RightSnapTurn => Instance == null ? null : Instance.PxrPlayerController.RightSnapTurn;

        /// <summary>
        /// Rig의 왼손 Grabber
        /// </summary>
        public static PXRGrabber LeftGrabber => Instance == null ? null : Instance.PxrPlayerController.LeftGrabber;

        /// <summary>
        /// Rig의 오른손 Grabber
        /// </summary>
        public static PXRGrabber RightGrabber => Instance == null ? null : Instance.PxrPlayerController.RightGrabber;

        /// <summary>
        /// Rig의 왼손 Teleporter
        /// </summary>
        public static PXRTeleporter LeftTeleporter => Instance == null ? null : Instance.PxrPlayerController.LeftTeleporter;

        /// <summary>
        /// Rig의 오른손 Teleporter
        /// </summary>
        public static PXRTeleporter RightTeleporter => Instance == null ? null : Instance.PxrPlayerController.RightTeleporter;

        /// <summary>
        /// Rig의 왼손 포인터
        /// </summary>
        public static PXRPointerVR LeftPointer => Instance == null ? null : Instance.PxrPlayerController.LeftPointer;

        /// <summary>
        /// Rig의 오른손 포인터
        /// </summary>
        public static PXRPointerVR RightPointer => Instance == null ? null : Instance.PxrPlayerController.RightPointer;

        /// <summary>
        /// Rig의 왼손 Clicker
        /// </summary>
        public static PXRClicker LeftClicker => Instance == null ? null : Instance.PxrPlayerController.LeftClicker;
        
        /// <summary>
        /// Rig의 오른손 Clicker
        /// </summary>
        public static PXRClicker RightClicker => Instance == null ? null : Instance.PxrPlayerController.RightClicker;
        
        // Deprecated
        /*
        public static PXRRecenter Recenter => Instance == null ? null : Instance.PxrRecenter;
        */
        
        /// <summary>
        /// Rig의 EyeHeightChanger
        /// </summary>
        public static PXREyeHeightChanger EyeHeightChanger => Instance == null ? null : Instance.PxrPlayerController.EyeHeightChanger;

        /// <summary>
        /// Rig의 왼손 HandAnimationController
        /// </summary>
        public static PXRHandAnimationController LeftHandAnimationController => Instance == null ? null : Instance.PxrPlayerController.LeftHandAnimationController;

        /// <summary>
        /// Rig의 오른손 HandAnimationController
        /// </summary>
        public static PXRHandAnimationController RightHandAnimationController => Instance == null ? null : Instance.PxrPlayerController.RightHandAnimationController;

        /// <summary>
        /// KinematicCharacterMotor 컴포넌트
        /// </summary>
        public static KinematicCharacterMotor KinematicCharacterMotor => Instance == null ? null : Instance.Motor;

        public static PXRRigMovementBase RigMovement => Instance == null ? null : Instance.Movement;
        
        #endregion

        #region Private Properties

        private static PXRRig Instance
        {
            get
            {
                if (instance != null && instance.Value != null)
                    return instance.Value;

                var rig = FindObjectOfType<PXRRig>();

                if (rig == null)
                    return null;

                instance = new StaticVar<PXRRig>(rig);

                return instance.Value;
            }
        }

        private PXRPlayerController PxrPlayerController
        {
            get
            {
                if (Instance == null)
                    return null;

                if (pxrPlayerController == null)
                {
                    pxrPlayerController = Instance.GetComponentInChildren<PXRPlayerController>();
                }

                return pxrPlayerController;
            }
        }

        // Deprecated
        /*
        private PXRRecenter PxrRecenter
        {
            get
            {
                if (Instance == null)
                    return null;

                if (pxrRecenter == null)
                {
                    pxrRecenter = Instance.GetComponentInChildren<PXRRecenter>();
                }

                return pxrRecenter;
            }
        }
        */
        
        private KinematicCharacterMotor Motor
        {
            get
            {
                if(Instance == null)
                    return null;
                
                if (motor == null)
                {
                    motor = GetComponentInChildren<KinematicCharacterMotor>();
                }

                return motor;
            }
        }

        private PXRRigMovementBase Movement
        {
            get
            {
                if(Instance == null)
                    return null;
                
                if (movement == null)
                {
                    movement = GetComponentInChildren<PXRRigMovementBase>();
                }

                return movement;
            }
        }
        
        #endregion
    }
}