#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Dive.Avatar;
using Dive.Avatar.Meta;
using Newtonsoft.Json;
using Dive.Platform;
using Dive.Utility.UnityExtensions;
using Dive.VRModule;
using Oculus.Avatar2;
using Oculus.Avatar2.Experimental;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;
using CAPI = Oculus.Avatar2.CAPI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using SupportedPlatform = Dive.Avatar.SupportedPlatform;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable CS0618 // Type or member is obsolete
public class PXRMetaAvatarNetworking : PXRMetaAvatarEntityBase
{
    private static readonly int MinYValue = Shader.PropertyToID("_MinYValue");

    [Serializable]
    private struct LodFrequency
    {
        public StreamLOD LOD;
        public float UpdateFrequency;
    }

    [SerializeField]
    private bool isSendDebug = false;
    
    [SerializeField]
    private bool isReceiveDebug = false;

    #region Private Fields

    [SerializeField]
    private GameObject portalPrefab;

    [SerializeField]
    private float sitOffset = 0.5f;

    [SerializeField]
    private float footOffset = 0.5f;
    
    [SerializeField]
    private List<LodFrequency> updateFrequencySecondsByLodList;

    private OvrAvatarCustomHandPose[] customHandPoses;
    private float currentStreamDelay;

    private Dictionary<StreamLOD, float> updateFrequencySecondsByLod;
    private Dictionary<StreamLOD, double> lastUpdateTime = new();

    [SerializeField, ReadOnly]
    private int createActorNumber = -1;

    private Transform[] transforms;
    private PXRAvatarBridge avatarBridge = null;
    private PXRAvatarBridge.AvatarData currentAvatarData;
    
    private bool isChangedAvatarCheck = false;
    private bool firstDefaultStream = false;

    private GameObject portal;
    private Transform hipTransform;
    private Transform leftAnkleTransform;
    private Transform rightAnkleTransform;

    private float floorHeight = 0f;
    private float timer = 0.0f;
    private const string LOGScope = "PXRMetaAvatarNetworking";
    private string streamDataPath = "Stand_Medium";
    private byte[] defaultPose;
    private Camera MainCam => Camera.main;

    #endregion

    #region Public Properties

    public int CreateActorNumber
    {
        get => createActorNumber;
        set => createActorNumber = value;
    }

    public float AnklePositionY =>
        (leftAnkleTransform.position.y + rightAnkleTransform.position.y) / 2.0f;
    
    public PXRAvatarBridge AvatarBridge => avatarBridge;
    public bool FirstDefaultStream => firstDefaultStream;
    
    #endregion

    #region Public Methods

    public void Initialize(PXRAvatarBridge bridge, PXRAvatarBridge.AvatarData avatarData)
    {
        currentAvatarData = avatarData;

        createActorNumber = avatarData.actorNumber;
        Platform = avatarData.platform;
        customHandPoses = GetComponentsInChildren<OvrAvatarCustomHandPose>();
        InstanceObject = gameObject;
        useNetwork = true;

        if (createActorNumber == PhotonNetwork.LocalPlayer.ActorNumber && !photoAvatar)
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

        if (Platform == SupportedPlatform.Meta)
        {
            var id = avatarData.userId;
            _userId = Convert.ToUInt64(id);
        }
        else
        {
            _userId = 0;
            SetPresets(Random.Range(0, 32));
        }

        isCreated = true;
        avatarBridge = bridge;
        base.bridge = bridge.GetComponent<PXRAvatarBridgeComponent>();

        if(!photoAvatar)
            IAvatar.OnInstanceCallback((CreateActorNumber, currentAvatarData.nakamaId), this);
    }

    #endregion

    #region Private Methods

    protected override async void Awake()
    {
        await UniTask.WaitUntil(() => avatarBridge != null);

        uniqueId = PlatformManager.Platform == Dive.Platform.Platform.Steam ? currentAvatarData.uniqueId : currentAvatarData.nakamaId;
        nickname = currentAvatarData.nickname;

        base.Awake();
        NetworkInitializeEvent += SyncInitialize;

        if (PXRRig.LeftTeleporter != null)
            PXRRig.LeftTeleporter.ExecuteTeleportEvent += OnTeleport;

        if (PXRRig.RightTeleporter != null)
            PXRRig.RightTeleporter.ExecuteTeleportEvent += OnTeleport;

        OnUserAvatarLoadedEvent.AddListener(ChangeAvatarMaterial);

        if (!photoAvatar)
        {
            portal = Instantiate(portalPrefab);
            portal.transform.position = new Vector3(0, -100, 0);
        }
        
        if (createActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            OnUserAvatarLoadedEvent.AddListener(ChangeAvatarBodyLayer);
            return;
        }

        OnUserAvatarLoadedEvent.AddListener(OtherAvatarLoaded);
        OnSkeletonLoadedEvent.AddListener(AddCollider);
    }

    protected new IEnumerator Start()
    {
        yield return base.Start();

        if (createActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            yield break;

        OVRManager.InputFocusAcquired += ReturnToApp;

        if (Camera.main != null)
        {
            if (Camera.main.GetComponent<PXRRigHeadRotator>() == null)
                Camera.main.gameObject.AddComponent<PXRRigHeadRotator>();
        }
    }

    protected new void OnDestroy()
    {
        if(portal != null)
            Destroy(portal);
        
        base.OnDestroy();
        
        NetworkInitializeEvent -= SyncInitialize;

        if (PXRRig.LeftTeleporter != null)
            PXRRig.LeftTeleporter.ExecuteTeleportEvent -= OnTeleport;

        if (PXRRig.RightTeleporter != null)
            PXRRig.RightTeleporter.ExecuteTeleportEvent -= OnTeleport;

        IAvatar.OnDestroyCallback((CreateActorNumber, currentAvatarData.nakamaId), this);

        OnSkeletonLoadedEvent.RemoveAllListeners();
        OnUserAvatarLoadedEvent.RemoveAllListeners();

        if (createActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            return;

        OVRManager.InputFocusAcquired -= ReturnToApp;
    }

    private void Update()
    {
        if (!isCreated)
            return;
        
        if (!IsLocal)
        {
            SetPortal();
            return;
        }

        if (!AddColliderCompleted)
            return;
        
        if (photoAvatar)
        {
            transform.localPosition = new Vector3(0, -PXRRig.PlayerController.transform.localPosition.y, 0);
            return;
        }
            
        UpdateDataStream();
    }

    private void SetPortal()
    {
        if (portal == null)
            return;
        
        if (leftAnkleTransform == null || rightAnkleTransform == null || hipTransform == null)
            return;
        
        if (PXRRoomSittingManager.Instance == null)
            return;

        var layerMask = 1 << PXRNameToLayer.Floor | 1 << PXRNameToLayer.TeleportSpace;

        if (Physics.Raycast(hipTransform.transform.position, Vector3.down, out var hit, 100.0f, layerMask))
        {
            var hitPoint = hit.point;

            if (PXRRoomSittingManager.Instance.IsSit(AvatarBridge.ActorNumber))
            {
                var hipPos = hipTransform.transform.position;
                if (hipPos.y > sitOffset)
                {
                    portal.SetActive(false);
                    portal.transform.position = new Vector3(0, -100, 0);
                    return;
                }

                var forward = hipTransform.up;
                forward.y = 0;
                var footPos = hitPoint + forward * footOffset;
                portal.transform.position = footPos;
                portal.SetActive(true);
                return;
            }
            
            if(hit.point.y < AnklePositionY)
            {
                portal.SetActive(false);
                portal.transform.position = new Vector3(0, -100, 0);
                return;
            }
            
            portal.transform.position = hitPoint + new Vector3(0, 0.01f, 0);
            portal.SetActive(true);
        }
        else
        {
            portal.SetActive(false);
            portal.transform.position = new Vector3(0, -100, 0);
        }
    }
    
    private async void ReturnToApp()
    {
        Debug.Log("OVRManager.VrFocusAcquired : ReturnToApp");

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
                PXRAvatarSync.DestroyNetworkAvatarAndReload(); // Load new avatar!
                break;
        }

        isChangedAvatarCheck = false;
    }

    private async void ChangeAvatarBodyLayer(OvrAvatarEntity entity)
    {
        transforms = gameObject.GetComponentsInChildren<Transform>(true);

        foreach (var child in transforms)
        {
            if (child.GetComponent<Collider>())
                continue;

            child.gameObject.layer = photoAvatar ? PXRNameToLayer.PhotoAvatar : PXRNameToLayer.AvatarBody;
        }

        var head = entity.GetSkeletonTransform(CAPI.ovrAvatar2JointType.Head);
        var headTrigger = head.AddComponent<PXRAvatarTrigger>();
        headTrigger.IsHead = true;

        var body = entity.GetSkeletonTransform(CAPI.ovrAvatar2JointType.Chest);
        var bodyTrigger = body.AddComponent<PXRAvatarTrigger>();
        bodyTrigger.IsBody = true;
        
        leftAnkleTransform = entity.GetSkeletonTransform(CAPI.ovrAvatar2JointType.LeftFootAnkle);
        rightAnkleTransform = entity.GetSkeletonTransform(CAPI.ovrAvatar2JointType.RightFootAnkle);
        hipTransform = entity.GetSkeletonTransform(CAPI.ovrAvatar2JointType.Hips);
        
        AddColliderCompleted = true;
    }

    private async void ChangeAvatarMaterial(OvrAvatarEntity entity)
    {
        if (!IsLocal)
            await UniTask.WaitUntil(() => firstDefaultStream);

        ChangeLightProbe().Forget();
        
        if (IsLocal)
        {
            alpha = 1.0f;

            if (photoAvatar)
            {
                FadeAvatar(false, null);
            }

            return;
        }

        alpha = 0.0f;
        FadeAvatar(false, null);
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

    private void OtherAvatarLoaded(OvrAvatarEntity entity)
    {
        if (defaultPose == null)
        {
            var text = Resources.Load<TextAsset>(streamDataPath).text;
            defaultPose = JsonConvert.DeserializeObject<byte[]>(text);
        }

        ReceiveAvatarData(defaultPose);

        firstDefaultStream = true;
    }

    private void AddCollider(OvrAvatarEntity entity)
    {
        var joints = entity.GetCriticalJoints();

        foreach (var joint in joints)
        {
            var skeleton = entity.GetSkeletonTransform(joint);
            skeleton.gameObject.layer = PXRNameToLayer.AvatarItem;
            var avatarTrigger = skeleton.gameObject.AddComponent<PXRAvatarTrigger>();

            avatarTrigger.SetAvatarBridge(avatarBridge);

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
                    avatarTrigger.IsOther = true;
                    avatarTrigger.IsLeftHand = true;

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
                    avatarTrigger.IsOther = true;

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
                    avatarTrigger.IsOther = true;

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
                    avatarTrigger.IsOther = true;
                    avatarTrigger.IsRightHand = true;

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
                    avatarTrigger.IsOther = true;

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
                    avatarTrigger.IsOther = true;

                    var rightArmUpperRigidBody = skeleton.gameObject.AddComponent<Rigidbody>();
                    rightArmUpperRigidBody.useGravity = false;
                    rightArmUpperRigidBody.isKinematic = true;
                    break;
                case CAPI.ovrAvatar2JointType.LeftFootAnkle:
                    avatarTrigger.IsLeftAnkle = true;
                    break;
                case CAPI.ovrAvatar2JointType.RightFootAnkle:
                    avatarTrigger.IsRightAnkle = true;
                    break;
            }
        }
        
        leftAnkleTransform = entity.GetSkeletonTransform(CAPI.ovrAvatar2JointType.LeftFootAnkle);
        rightAnkleTransform = entity.GetSkeletonTransform(CAPI.ovrAvatar2JointType.RightFootAnkle);
        hipTransform = entity.GetSkeletonTransform(CAPI.ovrAvatar2JointType.Hips);

        floorHeight = Setting.Room.GetFloorHeight(TablePartyController.Instance.CurrentRoomIndex);
        
        if(floorHeight != 0f)
            Material.SetFloat("_MinYValue", floorHeight);
        
        AddColliderCompleted = true;
    }
    
    private void SyncInitialize()
    {
        updateFrequencySecondsByLod = new Dictionary<StreamLOD, float>();
        foreach (var val in updateFrequencySecondsByLodList)
        {
            updateFrequencySecondsByLod[val.LOD] = val.UpdateFrequency;
            lastUpdateTime[val.LOD] = 0;
        }
    }

    private void OnTeleport()
    {
        transform.localPosition = Vector3.zero;
    }

    private void UpdateDataStream()
    {
        if (isActiveAndEnabled)
        {
            if (IsCreated && HasJoints)
            {
                var now = Time.unscaledTimeAsDouble;
                var lod = StreamLOD.Medium;
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
        
        if(isSendDebug)
            Debug.LogError($"SendAvatarData: {Convert.ToBase64String(bytes)}");
        
        var emptyBytes = Array.Empty<byte>();
        
        AvatarBridge.SpreadAvatarData(bytes, emptyBytes);
    }

    [Button]
    private void SaveByte(StreamLOD lod)
    {
        var bytes = RecordStreamData(lod);
        
        Debug.LogError($"SaveByte: {Convert.ToBase64String(bytes)}");
        
        
        System.IO.File.WriteAllBytes(Application.dataPath + $"/Resources/{streamDataPath}.json", bytes);
    }
    
    #endregion
    
    public void ReceiveAvatarData(byte[] data)
    {
        if(isReceiveDebug)
            Debug.LogError($"ReceiveAvatarData: {Convert.ToBase64String(data)}");
        
        ApplyStreamData(data);
    }
}

#endif