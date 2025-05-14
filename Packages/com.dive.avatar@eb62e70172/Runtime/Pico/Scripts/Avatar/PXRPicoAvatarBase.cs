#if DIVE_PLATFORM_PICO

using System;
using System.Collections.Generic;
using Pico.Avatar;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Dive.Avatar.Pico
{
    public class PXRPicoAvatarBase : MonoBehaviour, IAvatar
    {
        #region Public Fields

        public event Action NetworkInitializeEvent;

        [Tooltip("로컬 아바타 여부")]
        public bool isMainAvatar = false;

        [Tooltip("네트워크 연결 상태가 좋지 않을 때 이전에 캐시해둔 아바타 메타 정보를 사용할 것인지 여부")]
        public bool allowAvatarMetaFromCache = false;

        [Tooltip("아바타의 렌더링 부위")]
        public AvatarManifestationType avatarManifestationType = AvatarManifestationType.Half;

        [Tooltip("1인칭, 3인칭 아바타 여부")]
        public AvatarHeadShowType headShowType = AvatarHeadShowType.Normal;

        [Tooltip("아바타 패킷 레벨")]
        public RecordBodyAnimLevel recordBodyAnimLevel = RecordBodyAnimLevel.Invalid;

        [Tooltip("메시 클리핑 사용 여부, 메시 클리핑을 사용하면 일부 메시가 렌더링되지 않을 수 있습니다.")]
        public bool bodyCulling = false;

        [Tooltip("입모양 추적 사용 여부")]
        public bool useLipSync = false;

        [Tooltip("오브젝트를 생성할 관절 부위")]
        public JointType[] criticalJoints;

        [Tooltip("아바타 생성 전 미리보기 아바타 여부")]
        public bool enablePlaceHolder = true;

        [Tooltip("아바타 IK 세팅")]
        public PXRPicoAvatarIKSettings ikSettings = null;

        [Tooltip("입력장치 타입")]
        public DeviceInputReaderBuilderInputType deviceInputReaderType = DeviceInputReaderBuilderInputType.Invalid;

        [Tooltip("XR 입력 장치를 대신하는 경우 사용")]
        public InputActionProperty[] buttonActions;

        [Tooltip("아바타 로드 완료 시 실행되는 액션")]
        public Action<PXRPicoAvatarBase> loadedFinishCall;

        // public float positionRange = 0.0f;        

        #endregion

        #region Private Fields

        [SerializeField]
        private List<string> presetAvatarIDList = new List<string>();

        private AvatarBodyAnimController bodyAnimController;
        private GameObject leftSkeletonGo;
        private GameObject rightSkeletonGo;
        private GameObject leftHandPose;
        private GameObject rightHandPose;

        private Vector3 cameraOffsetPosition = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 defaultXrScale = Vector3.one;
        private Vector3 defaultXrPosition = Vector3.zero;

        private Action<bool> loadedCallback;

        private string jsonAvatar;

        private float maxControllerDistance = 1.0f;

        private bool isHeightAutoFitInitialized = false;
        private bool cameraTracking = false;
        private bool enableExpression = true;
        protected bool isCreated = false;

        protected PXRAvatarBridgeComponent bridgeComponent;
        private AudioSource audioSource;

        #endregion

        #region Public Properties

        [field: SerializeField]
        public bool IsMine { get; protected set; }

        [field: SerializeField]
        public bool AddColliderCompleted { get; protected set; }

        [field: SerializeField]
        public SupportedPlatform Platform { get; protected set; }

        [field: SerializeField]
        public GameObject InstanceObject { get; protected set; }

        public string UserID { get; protected set; }

        public string AvatarID { get; protected set; }

        public PicoAvatar Avatar { get; protected set; }

        #endregion

        #region Private Properties

        private PicoAvatarManager AvatarManager => PicoAvatarManager.instance;

        private AudioSource AudioSource => audioSource ??= transform.parent.GetComponent<AudioSource>();

        #endregion

        #region Public Methods

        public Transform GetTransform()
        {
            return transform;
        }

        public AudioSource GetAudioSource()
        {
            return AudioSource;
        }

        public PXRAvatarBridgeComponent GetBridgeComponent()
        {
            return bridgeComponent;
        }
        
        public void SetVolume(float volume)
        {
            if (AudioSource == null)
                return;

            AudioSource.volume = volume;
        }

        public virtual void StartAvatar(string userID, InputActionProperty[] btnActions = null, string avatarID = "", bool usePreset = false)
        {
            if (this.Avatar != null)
            {
                Dispose();
            }

            jsonAvatar = "";
            UserID = userID;
            AvatarID = avatarID;
            buttonActions = btnActions;

            CreateAvatar(usePreset);

            if (!isMainAvatar)
                return;

            var avatarApp = PicoAvatarApp.instance;

            var avatarHand = avatarApp.GetComponent<PXRPicoAvatarHand>();
            if (avatarHand == null)
                return;

            SetCustomHandPose(avatarHand.LeftHandPostSkeleton, avatarHand.RightHandPostSkeleton, avatarHand.LeftHandPostGo, avatarHand.RightHandPostGo);
            Avatar.entity.leftCustomHandPose.syncWristTransform = false;
            Avatar.entity.rightCustomHandPose.syncWristTransform = false;

            Avatar.entity.leftCustomHandPose.fingerPoseSyncMode = FingerPoseSyncMode.SyncRotation;
            Avatar.entity.rightCustomHandPose.fingerPoseSyncMode = FingerPoseSyncMode.SyncRotation;
        }

        public virtual void StartJsonAvatar(string userID, string jsonData, InputActionProperty[] btnActions = null)
        {
            jsonAvatar = jsonData;
            UserID = userID;
            buttonActions = btnActions;

            CreateAvatar();

            if (!isMainAvatar)
                return;

            var avatarApp = PicoAvatarApp.instance;

            var avatarHand = avatarApp.GetComponent<PXRPicoAvatarHand>();
            if (avatarHand == null)
                return;

            SetCustomHandPose(avatarHand.LeftHandPostSkeleton, avatarHand.RightHandPostSkeleton, avatarHand.LeftHandPostGo, avatarHand.RightHandPostGo);
            Avatar.entity.leftCustomHandPose.syncWristTransform = false;
            Avatar.entity.rightCustomHandPose.syncWristTransform = false;

            Avatar.entity.leftCustomHandPose.fingerPoseSyncMode = FingerPoseSyncMode.SyncRotation;
            Avatar.entity.rightCustomHandPose.fingerPoseSyncMode = FingerPoseSyncMode.SyncRotation;
        }

        public void StartPresetAvatar(string userId, int presetIndex)
        {
            if (presetIndex < 0 || presetIndex >= presetAvatarIDList.Count)
            {
                Debug.LogError($"유효하지 않은 presetIndex 입니다. : {presetIndex}");
                return;
            }

            StartAvatar(userID: userId, avatarID: presetAvatarIDList[presetIndex], usePreset: true);
        }

        public void ReloadAvatar()
        {
            Debug.Log("아바타 리로드");
            if (Avatar != null)
            {
                AvatarManager.UnloadAvatar(Avatar);
                Avatar = null;
            }

            CreateAvatar();

            if (!isMainAvatar)
                return;

            var avatarApp = PicoAvatarApp.instance;

            var avatarHand = avatarApp.GetComponent<PXRPicoAvatarHand>();
            if (avatarHand == null)
                return;

            SetCustomHandPose(avatarHand.LeftHandPostSkeleton, avatarHand.RightHandPostSkeleton, avatarHand.LeftHandPostGo, avatarHand.RightHandPostGo);
            Avatar.entity.leftCustomHandPose.syncWristTransform = false;
            Avatar.entity.rightCustomHandPose.syncWristTransform = false;

            Avatar.entity.leftCustomHandPose.fingerPoseSyncMode = FingerPoseSyncMode.SyncRotation;
            Avatar.entity.rightCustomHandPose.fingerPoseSyncMode = FingerPoseSyncMode.SyncRotation;
        }

        public void SetCustomHandPose(GameObject leftHandSkeleton, GameObject rightHandSkeleton, GameObject leftHandPose, GameObject rightHandPose)
        {
            this.leftSkeletonGo = leftHandSkeleton;
            this.rightSkeletonGo = rightHandSkeleton;
            this.leftHandPose = leftHandPose;
            this.rightHandPose = rightHandPose;
        }

        public void SetEntityLoadedCall(System.Action<bool> call)
        {
            loadedCallback = call;
        }

        public void SetEnableExpression(bool enableExpress)
        {
            this.enableExpression = enableExpress;
        }

        public void SetAsMainAvatar()
        {
            this.isMainAvatar = true;
        }

        public void Dispose()
        {
            if (bodyAnimController != null)
            {
                var autoFitController = bodyAnimController.autoFitController;
                autoFitController?.ClearAvatarOffsetChangedCallback(OnAvatarOffsetChangedCallBack);
            }

            AvatarManager.UnloadAvatar(Avatar);
            Avatar = null;
        }

        public void AlignAvatarArmSpan()
        {
            if (Avatar.entity == null)
                return;

            Avatar.entity.AlignAvatarArmSpan();
        }

        public void ResetXRRoot()
        {
            var xrRoot = ikSettings != null ? ikSettings.XRRoot : null;
            if (xrRoot == null)
                return;

            xrRoot.localScale = defaultXrScale;
            xrRoot.localPosition = defaultXrPosition;
            if (bodyAnimController != null && bodyAnimController.bipedIKController != null)
            {
                bodyAnimController.bipedIKController.controllerScale = xrRoot.localScale.x;
            }
        }

        public void ResetIKTargets()
        {
            var bipedIKController = bodyAnimController?.bipedIKController;
            if (bipedIKController == null)
                return;

            bipedIKController.ResetEffector(IKEffectorType.Head);
            bipedIKController.ResetEffector(IKEffectorType.LeftHand);
            bipedIKController.ResetEffector(IKEffectorType.RightHand);
            bipedIKController.ResetEffector(IKEffectorType.LeftFoot);
            bipedIKController.ResetEffector(IKEffectorType.RightFoot);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        #endregion

        #region Private Methods

        private void Update()
        {
            if (Avatar == null)
                return;

            var avatarEntity = Avatar.entity;

            if (avatarEntity == null || ikSettings == null)
                return;

            if (ikSettings.isDirty)
            {
                if (!isHeightAutoFitInitialized && ikSettings.heightAutoFit.enableAutoFitHeight)
                {
                    InitAutoFitController();
                }

                ikSettings.UpdateAvatarIKSettings(avatarEntity);
            }

            var autoFitController = bodyAnimController?.autoFitController;
            if (autoFitController == null || ikSettings.heightAutoFit.cameraOffsetTarget == null || ikSettings.heightAutoFit.enableAutoFitHeight != true)
                return;

            var pos = ikSettings.heightAutoFit.cameraOffsetTarget.transform.position;
            if (!(Vector3.SqrMagnitude(pos - cameraOffsetPosition) > 1e-6))
                return;

            autoFitController.SetCurrentAvatarOffset(pos);
            cameraOffsetPosition = pos;
        }

        private void InitAvatarCustomData()
        {
            if (!isMainAvatar || (leftSkeletonGo == null && rightSkeletonGo == null) || Avatar == null)
                return;
            // Right
            var rightUp = new Vector3(0.0f, 0.0f, -1.0f);
            var rightForward = new Vector3(-1.0f, 0.0f, 0.0f);
            var rightOffset = Vector3.zero; // new Vector3(0.15f,-0.05f,0.05f);

            // Left
            var leftUp = new Vector3(0.0f, 0.0f, -1.0f);
            var leftForward = new Vector3(1.0f, 0.0f, 0.0f);
            var leftOffset = Vector3.zero; // new Vector3(-0.059f, 0, 00.052f);
            var state = Avatar.entity.SetCustomHand(HandSide.Right, rightSkeletonGo, rightHandPose, rightUp, rightForward, rightOffset);
            state = Avatar.entity.SetCustomHand(HandSide.Left, leftSkeletonGo, leftHandPose, leftUp, leftForward, leftOffset);
            if (state)
            {
                Debug.Log($"아바타 커스텀 데이터 초기화 완료");
            }
        }

        private void CreateAvatar(bool usePreset = false)
        {
            if (isMainAvatar)
            {
                if (ikSettings != null)
                    ikSettings.SetFindXROrigin();
            }

            var capability = new AvatarCapabilities();
            capability.manifestationType = AvatarManifestationType.Half;
            capability.controlSourceType = isMainAvatar ? ControlSourceType.MainPlayer : ControlSourceType.OtherPlayer;

            if (deviceInputReaderType == DeviceInputReaderBuilderInputType.Invalid)
                capability.inputSourceType = isMainAvatar ? DeviceInputReaderBuilderInputType.PicoXR : DeviceInputReaderBuilderInputType.RemotePackage;
            else
                capability.inputSourceType = deviceInputReaderType;

            capability.bodyCulling = bodyCulling;
            capability.recordBodyAnimLevel = recordBodyAnimLevel;
            capability.enablePlaceHolder = enablePlaceHolder;
            capability.autoStopIK = !ikSettings || ikSettings.autoStopIK;
            capability.ikMode = ikSettings ? ikSettings.ikMode : AvatarIKMode.None;
            capability.headShowType = headShowType;
            capability.enableExpression = enableExpression;
            if (avatarManifestationType is AvatarManifestationType.HeadHands or AvatarManifestationType.Hands)
            {
                // not full mode must set handAssetId
                capability.handAssetId = "1550582586916995072";
            }

            if (allowAvatarMetaFromCache)
            {
                capability.flags |= (uint)AvatarCapabilities.Flags.AllowAvatarMetaFromCache;
            }

            Action<PicoAvatar, AvatarEntity> callback = (avatar, avatarEntity) =>
            {
                if (avatarEntity == null)
                {
                    loadedCallback?.Invoke(false);
                    return;
                }

                if (!isMainAvatar)
                {
                    avatar.PlayAnimation("idle", 0, "BottomLayer");
                }
                else
                {
                    bodyAnimController = avatarEntity.bodyAnimController;
                    InitBodyAnimControllerIK();
                    InitAvatarCustomData();
                    InitAutoFitController();
                    if (useLipSync)
                    {
#if !UNITY_EDITOR && UNITY_ANDROID
                         bodyAnimController.StartFaceTrack(true, true);
#endif
                    }
                }

                loadedCallback?.Invoke(true);
                loadedFinishCall?.Invoke(this);
            };

            if (usePreset)
            {
                Avatar = AvatarManager.LoadAvatar(AvatarLoadContext.CreateByAvatarId(string.Empty,
                    UserID, capability), callback);
            }
            else
            {
                Avatar = AvatarManager.LoadAvatar(!string.IsNullOrEmpty(this.jsonAvatar)
                    ? new AvatarLoadContext(UserID, AvatarID, this.jsonAvatar, capability)
                    : new AvatarLoadContext(UserID, AvatarID, null, capability), callback);
            }


            Avatar.criticalJoints = this.criticalJoints;

            var avatarEntity = Avatar.entity;
            var avatarTransform = Avatar.transform;

            avatarTransform.SetParent(transform);
            avatarTransform.localPosition = Vector3.zero;
            avatarTransform.localRotation = Quaternion.identity;
            avatarTransform.localScale = Vector3.one;

            avatarEntity.buttonActions = buttonActions;

            InitEntityXRTarget(avatarEntity);

            if (cameraTracking && isMainAvatar && PicoAvatarManager.instance.avatarCamera != null)
            {
                AvatarManager.avatarCamera.trakingAvatar = Avatar;
            }

            var xrRoot = ikSettings != null ? ikSettings.XRRoot : null;
            if (xrRoot != null)
            {
                defaultXrScale = xrRoot.localScale;
                defaultXrPosition = xrRoot.localPosition;
            }

            //
            // currently around origin.
            // if (positionRange > 0.0f)
            // {
            //     var dir = this.transform.localPosition;
            //     var dist = dir.magnitude;
            //     this.transform.localPosition = dir.normalized * (positionRange + dist);
            // }
        }

        private void InitEntityXRTarget(AvatarEntity avatarEntity)
        {
            if (ikSettings == null || avatarEntity == null)
            {
                return;
            }

            ikSettings.UpdateIKTargetsConfig(avatarEntity.avatarIKTargetsConfig);
        }

        private void InitBodyAnimControllerIK()
        {
            if (bodyAnimController is not {bipedIKController: not null})
                return;

            var avatarEntity = bodyAnimController.owner;

            bodyAnimController.bipedIKController.SetValidHipsHeightRange(0.0f, 3.0f);

            bodyAnimController.bipedIKController.SetIKAutoStopModeEnable(IKAutoStopMode.ControllerDisconnect, true);
            bodyAnimController.bipedIKController.SetIKAutoStopModeEnable(IKAutoStopMode.ControllerIdle, true);
#if UNITY_EDITOR
            bodyAnimController.bipedIKController.SetIKAutoStopModeEnable(IKAutoStopMode.ControllerIdle, false);
#endif
            bodyAnimController.bipedIKController.SetIKAutoStopModeEnable(IKAutoStopMode.ControllerLoseTracking, true);
            bodyAnimController.bipedIKController.SetIKAutoStopModeEnable(IKAutoStopMode.ControllerFarAway, true);
        }

        private void InitAutoFitController()
        {
            if (bodyAnimController == null || ikSettings == null)
                return;

            if (!ikSettings.heightAutoFit.enableAutoFitHeight || ikSettings.heightAutoFit.cameraOffsetTarget == null)
                return;

            var autoFitController = bodyAnimController.autoFitController;
            if (autoFitController == null)
                return;

            autoFitController.localAvatarHeightFittingEnable = true;
            autoFitController.ClearAvatarOffsetChangedCallback(OnAvatarOffsetChangedCallBack);
            autoFitController.AddAvatarOffsetChangedCallback(OnAvatarOffsetChangedCallBack);

            var trigger = Avatar.entity.gameObject.GetComponent<PicoAvatarAutoFitTrigger>();
            if (trigger == null)
                trigger = Avatar.entity.gameObject.AddComponent<PicoAvatarAutoFitTrigger>();
            trigger.SetTriggerCallback(OnAppAutoFitTrigger);

            var initPos = ikSettings.heightAutoFit.cameraOffsetTarget.transform.position;
            autoFitController.SetCurrentAvatarOffset(initPos);
            autoFitController.UpdateAvatarHeightOffset();
            Debug.Log("UpdateAvatarHeightOffset 함수 실행 완료");

            isHeightAutoFitInitialized = true;
        }

        private void OnAvatarOffsetChangedCallBack(AvatarAutoFitController cotroller, Vector3 cameraOffsetPos)
        {
            if (ikSettings == null || !ikSettings.heightAutoFit.enableAutoFitHeight)
                return;

            RefreshCameraOffsetTargetPos(cameraOffsetPos);
            cotroller.SetCurrentAvatarOffset(cameraOffsetPos);
        }

        private void OnAppAutoFitTrigger()
        {
            Debug.Log("OnAppAutoFitTrigger 함수 실행:");
            var autoFitController = bodyAnimController.autoFitController;
            if (autoFitController == null || ikSettings == null || ikSettings.heightAutoFit.cameraOffsetTarget == null)
                return;

            var initPos = ikSettings.heightAutoFit.cameraOffsetTarget.transform.position;
            autoFitController.SetCurrentAvatarOffset(initPos);
            autoFitController.UpdateAvatarHeightOffset();
        }

        private void RefreshCameraOffsetTargetPos(Vector3 offset)
        {
            if (ikSettings == null || ikSettings.heightAutoFit.cameraOffsetTarget == null)
                return;

            ikSettings.heightAutoFit.cameraOffsetTarget.transform.position = offset;
            cameraOffsetPosition = offset;
        }

        #endregion
    }
}

#endif