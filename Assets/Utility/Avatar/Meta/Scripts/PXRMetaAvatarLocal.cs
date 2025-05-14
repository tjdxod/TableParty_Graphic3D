#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dive.Avatar;
using Dive.Avatar.Meta;
using Dive.Platform;
using Dive.VRModule;
using DiveVR;
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using SupportedPlatform = Dive.Avatar.SupportedPlatform;

#pragma warning disable CS0108, CS0114
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable CS0618 // Type or member is obsolete
public class PXRMetaAvatarLocal : PXRMetaAvatarEntityBase
{
    // 마이룸에서 생성되는 아바타를 위한 클래스
    [Serializable]
    public struct AssetData
    {
        public AssetSource source;
        public string path;
    }

    [Header("Assets")]
    [SerializeField]
    public AssetData assetData = new AssetData {source = AssetSource.Zip, path = "0"};

    [Serializable]
    private struct LodFrequency
    {
        public StreamLOD LOD;
        public float UpdateFrequency;
    }

    [SerializeField]
    private List<LodFrequency> updateFrequencySecondsByLodList;

    private OvrAvatarCustomHandPose[] customHandPoses;
    private float currentStreamDelay;
    
    private Dictionary<StreamLOD, float> updateFrequencySecondsByLod;
    private Dictionary<StreamLOD, double> lastUpdateTime = new();
    
    private PXRAvatarBridge avatarBridge = null;
    private PXRAvatarBridge.AvatarData currentAvatarData;

    private Transform[] transforms;
    private bool isChangedAvatarCheck = false;
    private bool isInsideCustom = false;
    private const string LOGScope = "PXRMetaAvatarNetworking";

    private Camera mirrorCamera;

    public PXRAvatarBridge AvatarBridge => avatarBridge;

    public void Initialize(PXRAvatarBridge bridge, PXRAvatarBridge.AvatarData avatarData)
    {
        currentAvatarData = avatarData;
        Platform = avatarData.platform;
        customHandPoses = GetComponentsInChildren<OvrAvatarCustomHandPose>();
        InstanceObject = gameObject;
        useNetwork = true;
        
        if (!photoAvatar)
        {
            IsMine = true;
            isOwner = true;
            foreach (var custom in customHandPoses)
            {
                custom.enabled = true;
            }
        }
        else
        {
            IsMine = false;
        }
        
        if(photoAvatar)
            SetIsLocal(false);        
        
        updateFrequencySecondsByLod = new Dictionary<StreamLOD, float>();
        foreach (var val in updateFrequencySecondsByLodList)
        {
            updateFrequencySecondsByLod[val.LOD] = val.UpdateFrequency;
            lastUpdateTime[val.LOD] = 0;
        }
        
        if (avatarData.platform == SupportedPlatform.Meta)
        {
            var id = avatarData.userId;
            _userId = Convert.ToUInt64(id);
        }
        else
        {
            _userId = 0;
            // SetPresets(avatarData.presetAvatar);
        }

        isCreated = true;
        avatarBridge = bridge;
        base.bridge = bridge.GetComponent<PXRAvatarBridgeComponent>();
    }

    protected override async void Awake()
    {
        await UniTask.WaitUntil(() => avatarBridge != null);
        
        uniqueId = PlatformManager.Platform == Dive.Platform.Platform.Steam ? currentAvatarData.uniqueId : currentAvatarData.nakamaId;
        nickname = currentAvatarData.nickname;
        
        base.Awake();
        PXRRig.LeftTeleporter.ExecuteTeleportEvent += OnTeleport;
        PXRRig.RightTeleporter.ExecuteTeleportEvent += OnTeleport;

        mirrorCamera = FindObjectOfType<PXRAvatarCustom>().GetComponentInChildren<Camera>(true);

        OnSkeletonLoadedEvent.AddListener(AddCollider);
        OnUserAvatarLoadedEvent.AddListener(ChangeAvatarMaterial);
        OnUserAvatarLoadedEvent.AddListener(ChangeAvatarBodyLayer);

        avatarBridge = transform.parent.GetComponent<PXRAvatarBridge>();
    }

    protected new IEnumerator Start()
    {
        StartCoroutine(base.Start());
        yield return null;

        OVRManager.InputFocusAcquired += ReturnToApp;

        if (Camera.main != null)
        {
            if (Camera.main.GetComponent<PXRRigHeadRotator>() == null)
                Camera.main.gameObject.AddComponent<PXRRigHeadRotator>();
        }
    }

    private void Update()
    {
        if (!isCreated)
            return;

        if (!IsLocal)
            return;

        if (!AddColliderCompleted)
            return;

        if (photoAvatar)
            return;

        UpdateDataStream();
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();

        if (PXRRig.LeftTeleporter != null)
            PXRRig.LeftTeleporter.ExecuteTeleportEvent -= OnTeleport;

        if (PXRRig.RightTeleporter != null)
            PXRRig.RightTeleporter.ExecuteTeleportEvent -= OnTeleport;

        OnSkeletonLoadedEvent.RemoveAllListeners();
        OnUserAvatarLoadedEvent.RemoveAllListeners();
        OVRManager.InputFocusAcquired -= ReturnToApp;
    }

    public void ChangeAvatarBodyLayer(OvrAvatarEntity entity)
    {
        transforms = gameObject.GetComponentsInChildren<Transform>(true);

        foreach (var child in transforms)
        {
            if (child.GetComponent<Collider>())
                continue;

            child.gameObject.layer = photoAvatar ? PXRNameToLayer.PhotoAvatar : PXRNameToLayer.AvatarBody;
        }
    }

    public void ChangeIgnoreAvatarLayer(OvrAvatarEntity entity)
    {
        transforms = gameObject.GetComponentsInChildren<Transform>(true);

        foreach (var child in transforms)
        {
            if (child.GetComponent<Collider>())
                continue;

            child.gameObject.layer = PXRNameToLayer.IgnoreAvatar;
        }
    }

    public void ChangeAvatarMaterial(OvrAvatarEntity entity)
    {
        ChangeLightProbe().Forget();
    }

    private async UniTask ChangeLightProbe()
    {
        await UniTask.WaitUntil(() => gameObject.GetComponentsInChildren<MeshRenderer>(true).Length > 0);

        var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);

        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        }
    }

    public void SetInsideCustom(bool isInside)
    {
        isInsideCustom = isInside;
    }

    public async void LayerChangeTask(OvrAvatarEntity entity)
    {
        transforms = gameObject.GetComponentsInChildren<Transform>(true);

        foreach (var child in transforms)
        {
            child.gameObject.layer = photoAvatar ? PXRNameToLayer.PhotoAvatar : PXRNameToLayer.AvatarBody;
        }
    }

    private async void ReturnToApp()
    {
        if (isChangedAvatarCheck)
            return;

        isChangedAvatarCheck = true;
        await ForceCheckForAvatarChangeAsync();
    }

    private async UniTask ForceCheckForAvatarChangeAsync()
    {
        var checkTask = HasAvatarChangedAsync();
        await checkTask;

        switch (checkTask.Result)
        {
            case OvrAvatarManager.HasAvatarChangedRequestResultCode.UnknownError:
                OvrAvatarLog.LogError("아바타 변경 중 알 수 없는 오류가 발생하였습니다. 작업을 중단합니다.", LOGScope, this);
                break;

            case OvrAvatarManager.HasAvatarChangedRequestResultCode.BadParameter:
                OvrAvatarLog.LogError("아바타 변경이 유효하지 않습니다. 작업을 중단합니다.", LOGScope, this);
                break;

            case OvrAvatarManager.HasAvatarChangedRequestResultCode.SendFailed:
                OvrAvatarLog.LogWarning("아바타 변경을 전송할 수 없습니다.", LOGScope, this);
                break;

            case OvrAvatarManager.HasAvatarChangedRequestResultCode.RequestFailed:
                OvrAvatarLog.LogError("아바타 변경 요청에 실패하였습니다.", LOGScope, this);
                break;

            case OvrAvatarManager.HasAvatarChangedRequestResultCode.RequestCancelled:
                OvrAvatarLog.LogInfo("아바타 변경 요청이 취소되었습니다.", LOGScope, this);
                break;

            case OvrAvatarManager.HasAvatarChangedRequestResultCode.AvatarHasNotChanged:
                OvrAvatarLog.LogVerbose("아바타 변경 사항이 없습니다..", LOGScope, this);
                break;

            case OvrAvatarManager.HasAvatarChangedRequestResultCode.AvatarHasChanged:
                OvrAvatarLog.LogInfo("아바타를 변경하였습니다. 새로운 아바타 상태로 변경합니다.", LOGScope, this);
                mirrorCamera.cullingMask = 0;
                PXRAvatarSync.DestroyPrivateAvatarAndReload(isInsideCustom); // Load new avatar!
                // isInAvatarChange == true ? 3인칭
                // isInAvatarChange == false ? 1인칭
                break;
        }

        isChangedAvatarCheck = false;
    }

    private void AddCollider(OvrAvatarEntity entity)
    {
        var joints = entity.GetCriticalJoints();

        foreach (var joint in joints)
        {
            var skeleton = entity.GetSkeletonTransform(joint);
            skeleton.gameObject.layer = PXRNameToLayer.AvatarItem;
            var avatarTrigger = skeleton.gameObject.AddComponent<PXRAvatarTrigger>();

            switch (joint)
            {
                case CAPI.ovrAvatar2JointType.Head:
                    var head = skeleton.gameObject.AddComponent<CapsuleCollider>();
                    head.direction = 0;
                    head.center = new Vector3(0.07304f, 0.01928f, 0);
                    head.radius = 0.1272f;
                    head.height = 0.3382258f;
                    head.isTrigger = true;
                    avatarTrigger.IsHead = true;

                    var headRigidBody = skeleton.gameObject.AddComponent<Rigidbody>();
                    headRigidBody.useGravity = false;
                    headRigidBody.isKinematic = true;
                    break;
                case CAPI.ovrAvatar2JointType.Chest:
                    var chest = skeleton.gameObject.AddComponent<CapsuleCollider>();
                    chest.direction = 0;
                    chest.center = new Vector3(-0.09529f, 0f, 0.01235f);
                    chest.radius = 0.19061f;
                    chest.height = 0.56031f;
                    chest.isTrigger = true;
                    avatarTrigger.IsBody = true;

                    var chestRigidBody = skeleton.gameObject.AddComponent<Rigidbody>();
                    chestRigidBody.useGravity = false;
                    chestRigidBody.isKinematic = true;
                    break;
                case CAPI.ovrAvatar2JointType.LeftHandWrist:
                    var leftHandWrist = skeleton.gameObject.AddComponent<CapsuleCollider>();
                    leftHandWrist.direction = 0;
                    leftHandWrist.center = new Vector3(0.07593f, 0.01549f, -0.0012f);
                    leftHandWrist.radius = 0.04546f;
                    leftHandWrist.height = 0.16433f;
                    leftHandWrist.isTrigger = true;

                    var leftHandWristRigidBody = skeleton.gameObject.AddComponent<Rigidbody>();
                    leftHandWristRigidBody.useGravity = false;
                    leftHandWristRigidBody.isKinematic = true;
                    break;
                case CAPI.ovrAvatar2JointType.LeftArmLower:
                    var leftArmLower = skeleton.gameObject.AddComponent<CapsuleCollider>();
                    leftArmLower.direction = 0;
                    leftArmLower.center = new Vector3(0.12349f, 0.00594f, 0.00821f);
                    leftArmLower.radius = 0.06732f;
                    leftArmLower.height = 0.26424f;
                    leftArmLower.isTrigger = true;

                    var leftArmLowerRigidBody = skeleton.gameObject.AddComponent<Rigidbody>();
                    leftArmLowerRigidBody.useGravity = false;
                    leftArmLowerRigidBody.isKinematic = true;
                    break;
                case CAPI.ovrAvatar2JointType.LeftArmUpper:
                    var leftArmUpper = skeleton.gameObject.AddComponent<CapsuleCollider>();
                    leftArmUpper.direction = 0;
                    leftArmUpper.center = new Vector3(0.12349f, 0, 0);
                    leftArmUpper.radius = 0.07f;
                    leftArmUpper.height = 0.26424f;
                    leftArmUpper.isTrigger = true;

                    var leftArmUpperRigidBody = skeleton.gameObject.AddComponent<Rigidbody>();
                    leftArmUpperRigidBody.useGravity = false;
                    leftArmUpperRigidBody.isKinematic = true;
                    break;
                case CAPI.ovrAvatar2JointType.RightHandWrist:
                    var rightHandWrist = skeleton.gameObject.AddComponent<CapsuleCollider>();
                    rightHandWrist.direction = 0;
                    rightHandWrist.center = new Vector3(-0.07593f, -0.01549f, 0.0012f);
                    rightHandWrist.radius = 0.04546f;
                    rightHandWrist.height = 0.16433f;
                    rightHandWrist.isTrigger = true;

                    var rightHandWristRigidBody = skeleton.gameObject.AddComponent<Rigidbody>();
                    rightHandWristRigidBody.useGravity = false;
                    rightHandWristRigidBody.isKinematic = true;
                    break;
                case CAPI.ovrAvatar2JointType.RightArmLower:
                    var rightArmLower = skeleton.gameObject.AddComponent<CapsuleCollider>();
                    rightArmLower.direction = 0;
                    rightArmLower.center = new Vector3(-0.12349f, -0.00594f, -0.00821f);
                    rightArmLower.radius = 0.06732f;
                    rightArmLower.height = 0.26424f;
                    rightArmLower.isTrigger = true;

                    var rightArmLowerRigidBody = skeleton.gameObject.AddComponent<Rigidbody>();
                    rightArmLowerRigidBody.useGravity = false;
                    rightArmLowerRigidBody.isKinematic = true;
                    break;
                case CAPI.ovrAvatar2JointType.RightArmUpper:
                    var rightArmUpper = skeleton.gameObject.AddComponent<CapsuleCollider>();
                    rightArmUpper.direction = 0;
                    rightArmUpper.center = new Vector3(-0.12349f, 0, 0);
                    rightArmUpper.radius = 0.07f;
                    rightArmUpper.height = 0.26424f;
                    rightArmUpper.isTrigger = true;

                    var rightArmUpperRigidBody = skeleton.gameObject.AddComponent<Rigidbody>();
                    rightArmUpperRigidBody.useGravity = false;
                    rightArmUpperRigidBody.isKinematic = true;
                    break;
            }
        }

        AddColliderCompleted = true;
    }

    private void UpdateDataStream()
    {
        if (isActiveAndEnabled)
        {
            if (IsCreated && HasJoints)
            {
                var now = Time.unscaledTimeAsDouble;
                var lod = StreamLOD.Low;
                double timeSinceLastUpdate = default;
                foreach (var lastUpdateKvp in lastUpdateTime)
                {
                    var lastLod = lastUpdateKvp.Key;
                    var time = now - lastUpdateKvp.Value;
                    var frequency = updateFrequencySecondsByLod[lastLod];
                    if (time >= frequency)
                    {
                        if (time > timeSinceLastUpdate)
                        {
                            timeSinceLastUpdate = time;
                            lod = lastLod;
                        }
                    }
                }

                if (timeSinceLastUpdate != 0.0d)
                {
                    // act like every lower frequency lod got updated too
                    var lodFrequency = updateFrequencySecondsByLod[lod];
                    foreach (var lodFreqKvp in updateFrequencySecondsByLod)
                    {
                        if (lodFreqKvp.Value <= lodFrequency)
                        {
                            lastUpdateTime[lodFreqKvp.Key] = now;
                        }
                    }

                    SendAvatarData(lod);
                }
            }
        }
    }

    private void SendAvatarData(StreamLOD lod)
    {
        var bytes = RecordStreamData(lod);
        
        AvatarBridge.PhotoAvatarEntityBase.ApplyStreamData(bytes);
    }

    private void OnTeleport()
    {
        transform.localPosition = Vector3.zero;
    }
}

#endif