#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Oculus.Avatar2;
using Oculus.Platform;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using CAPI = Oculus.Avatar2.CAPI;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Dive.Avatar.Meta
{
    [Serializable]
    public struct OnJointLoadedPair
    {
        public CAPI.ovrAvatar2JointType Joint;
        public Transform TargetToSetAsChild;
        public UnityEvent<Transform> OnLoaded;
    }

    /// <summary>
    /// 메타 아바타 엔티티 베이스 클래스
    /// </summary>
    public class PXRMetaAvatarEntityBase : OvrAvatarEntity, IAvatar
    {
        #region Public Fields

        public event Action NetworkInitializeEvent;
        public List<OnJointLoadedPair> JointLoadedEvents = new();

        #endregion

        #region Private Variables

        protected bool isCreated = false;
        protected bool isOwner = false;
        protected bool useNetwork = false;
        protected bool isFirstPerson = false;
        protected string uniqueId = string.Empty;
        protected string nickname = string.Empty;

        protected float alpha = 0f;
        protected PXRAvatarBridgeComponent bridge;
        
        private TweenerCore<float, float, FloatOptions> invadeTween;
        
        private const string LOGScope = "PXRMetaAvatarEntityBase";
        private readonly Stopwatch loadTime = new Stopwatch();
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

        [field: SerializeField]
        public Shader OpaqueShader { get; private set; }

        [field: SerializeField]
        public Shader TransparentShader { get; private set; }

        public Transform GetJointTransform(CAPI.ovrAvatar2JointType jointType) => GetSkeletonTransformByType(jointType);

        public TweenerCore<float, float, FloatOptions> InvadeTween => invadeTween;
        
        #endregion

        #region Private Properties

        [field: SerializeField, Space(10), Header("PXR Meta Avatar Entity Settings")]
        protected bool UseUserAvatar { get; set; } = true;

        protected bool OpponentUseUserAvatar => !UseUserAvatar;

        [field: SerializeField]
        protected bool UseCreateAtStartMethod { get; set; } = false;

        [field: SerializeField, ShowIf(nameof(OpponentUseUserAvatar), true), Range(-1, 31)]
        protected int PresetAvatarIndex { get; set; } = -1;

        [field: SerializeField, ShowIf(nameof(UseUserAvatar), true)]
        protected bool UseAutoRetryAvatarLoad { get; set; } = true;

        [field: SerializeField, ShowIf(nameof(UseUserAvatar), true)]
        protected bool UseAutoCheckChangeAvatar { get; set; } = false;

        [field: SerializeField, ShowIf(nameof(UseAutoCheckChangeAvatar), true), Range(4.0f, 320.0f)]
        protected float CheckChangeAvatarInterval { get; set; } = 8.0f;

        [field: SerializeField, Range(0.01f, 3f)]
        public float FadeDuration { get; private set; } = 0.75f;
        
        private AudioSource AudioSource => audioSource ??= bridge.GetComponent<AudioSource>();

        #endregion

        #region Public Methods

        public Transform GetTransform()
        {
            try
            {
                return transform;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public AudioSource GetAudioSource()
        {   
            try
            {
                return AudioSource;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public PXRAvatarBridgeComponent GetBridgeComponent()
        {
            try
            {
                return bridge;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void SetVolume(float volume)
        {
            if (AudioSource == null)
                return;

            AudioSource.volume = volume;
        }

        public IEnumerator Initialize(ulong userId)
        {
            Teardown();

            yield return new WaitUntil(() => isCreated);

            SetIsLocal(isOwner);

            if (isOwner)
            {
                if (useNetwork)
                {
                    SetOwnerAvatar();
                }
                else
                {
                    if (isFirstPerson)
                    {
                        SetOwnerAvatar();
                    }
                    else
                    {
                        SetThirdPersonOwnerAvatar();
                    }
                }
            }
            else
            {
                SetOpponentAvatar();
            }

            CreateEntity();
            SetActiveView(_creationInfo.renderFilters.viewFlags);

            SetUpAccessTokenAsync();

            NetworkInitializeEvent?.Invoke();

#if DIVE_PLATFORM_META

#if UNITY_ANDROID

            if (IsLocal)
            {
                UseUserAvatar = PresetAvatarIndex < 0;
                yield return CreateAvatar();
            }
            else
            {
                UseUserAvatar = true;
                _userId = userId;
                yield return CreateAvatar(userId);
            }

#elif UNITY_STANDALONE
            UseUserAvatar = false;
            yield return CreateAvatar();

#endif

#elif DIVE_PLATFORM_STEAM
            UseUserAvatar = false;
            yield return CreateAvatar();

#else
            OvrAvatarLog.LogError("지원하지 않는 플랫폼입니다.", LOGScope, this);

#endif
        }

        public void SetPresets(int index, bool immediateLoad = false)
        {
            PresetAvatarIndex = index;
            if (immediateLoad)
            {
                LoadPresetAvatar();
            }
        }

        public long GetLoadTimeMs()
        {
            return loadTime.ElapsedMilliseconds;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }
        
#pragma warning disable CS0618 // Type or member is obsolete
        public void FadeAvatar(bool isFade, Action callback = null)
        {
            if(InvadeTween != null || InvadeTween.IsActive())
                InvadeTween.Kill();
            
            Material.SetShader(TransparentShader);
            
            // SetMaterialShader(TransparentShader);

            primitiveMeshRenderer.enabled = true;

            var endAlpha = isFade ? 0f : 1f;
            invadeTween = DOTween.To(() => alpha, x => alpha = x, endAlpha, FadeDuration).OnUpdate(() =>
                {
                    Material.SetFloat("_Alpha", alpha);
                })
                .OnComplete(() =>
                {
                    Material.SetFloat("_Alpha", endAlpha);

                    primitiveMeshRenderer.enabled = !isFade;
                    // SetMaterialShader(OpaqueShader);
                    Material.SetShader(OpaqueShader);

                    callback?.Invoke();
                }).Play();
        }
#pragma warning restore CS0618 // Type or member is obsolete

        #endregion

        #region Private Methods

        protected override void Awake()
        {
            base.Awake();

            OVRPlugin.StartFaceTracking();
            OVRPlugin.StartEyeTracking();
        }

        protected IEnumerator Start()
        {
            // isCreated가 true일때까지 대기
            yield return new WaitUntil(() => isCreated);
            yield return Initialize(_userId);
        }

        private IEnumerator CreateAvatar()
        {
            if (UseUserAvatar)
            {
                yield return RecognizeUserAvatarAsync();
            }
            else
            {
                LoadPresetAvatar();
            }
        }

        private IEnumerator CreateAvatar(ulong userId)
        {
            _userId = userId;
            yield return LoadUserAvatar();
        }

        private async void SetUpAccessTokenAsync()
        {
#if DIVE_PLATFORM_META

#if UNITY_ANDROID

            var accessToken = Oculus.Platform.Users.GetAccessToken().Gen();
            await UniTask.WaitUntil(() => accessToken.IsCompleted);
            OvrAvatarEntitlement.SetAccessToken(accessToken.Result.Data);

#elif UNITY_STANDALONE
            var accessToken = await GetFederateAccessToken();
                
            if(accessToken == string.Empty)
            {
                OvrAvatarLog.LogError("Failed to get access token.", LOGScope, this);
                return;
            }
                
            OvrAvatarEntitlement.SetAccessToken(accessToken);

#endif


#elif DIVE_PLATFORM_STEAM
            // TODO : Steam Platform Access Token -> Federated Authentication 기능으로 추후 대체

            if (Platform == SupportedPlatform.Steam)
            {
                var accessToken = await GetFederateAccessToken();
                
                if(accessToken == string.Empty)
                {
                    OvrAvatarLog.LogError("Failed to get access token.", LOGScope, this);
                    return;
                }
                
                OvrAvatarEntitlement.SetAccessToken(accessToken);
            }

#else
            return;

#endif
        }

        protected virtual async UniTask<string> GetFederateAccessToken()
        {
            var tokenCallback = await PXRMetaAvatarPlatformInit.FederateGeneratedUserAccessToken(uniqueId, nickname);

            if (tokenCallback.IsSuccess)
            {
                var accessToken = tokenCallback.Response.access_token;
                return accessToken;
            }

            var createToken = await PXRMetaAvatarPlatformInit.FederateCreateUser(uniqueId, nickname);

            if (createToken.IsSuccess)
            {
                var reTokenCallback = await PXRMetaAvatarPlatformInit.FederateGeneratedUserAccessToken(uniqueId, nickname);
                return reTokenCallback.Response.access_token;
            }

            OvrAvatarLog.LogError($"Failed to generate user access token. [{tokenCallback.Message}]", LOGScope, this);
            return string.Empty;
        }

        protected void SetOwnerAvatar()
        {
            _creationInfo.features |= Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Animation;
            var inputManager = OvrAvatarManager.Instance.GetComponent<PXRMetaAvatarInputManager>();

            SetInputManager(inputManager);
            AvatarLODManager.Instance.firstPersonAvatarLod = AvatarLOD;
            _creationInfo.renderFilters.viewFlags = photoAvatar ? CAPI.ovrAvatar2EntityViewFlags.ThirdPerson : CAPI.ovrAvatar2EntityViewFlags.FirstPerson;
        }

        protected void SetThirdPersonOwnerAvatar()
        {
            _creationInfo.features |= Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Animation;
            var inputManager = OvrAvatarManager.Instance.GetComponent<PXRMetaAvatarInputManager>();

            SetInputManager(inputManager);
            AvatarLODManager.Instance.firstPersonAvatarLod = AvatarLOD;
            _creationInfo.renderFilters.viewFlags = CAPI.ovrAvatar2EntityViewFlags.ThirdPerson;
        }

        protected void SetOpponentAvatar()
        {
            _creationInfo.features &= ~Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Animation;

            SetInputManager(null);
            SetFacePoseProvider(null);
            SetEyePoseProvider(null);
            SetLipSync(null);

            _creationInfo.renderFilters.viewFlags = CAPI.ovrAvatar2EntityViewFlags.ThirdPerson;
        }

        private IEnumerator RecognizeUserAvatarAsync()
        {
            if (Platform == SupportedPlatform.Meta)
            {
                if (PXRMetaAvatarPlatformInit.status != MetaAvatarPlatformInitStatus.Succeeded)
                {
                    PXRMetaAvatarPlatformInit.InitializeMetaAvatarPlatform();
                }

                while (PXRMetaAvatarPlatformInit.status != MetaAvatarPlatformInitStatus.Succeeded)
                {
                    if (PXRMetaAvatarPlatformInit.status == MetaAvatarPlatformInitStatus.Failed)
                    {
                        OvrAvatarLog.LogError($"메타 플랫폼 초기화에 실패하였습니다. 프리셋 아바타로 대체합니다.", LOGScope);
                        LoadPresetAvatar();
                        yield break;
                    }

                    yield return null;
                }

                if (_userId == 0)
                {
                    var getUserIdComplete = false;
                    Oculus.Platform.Users.GetLoggedInUser().OnComplete(message =>
                    {
                        if (!message.IsError)
                        {
                            _userId = message.Data.ID;
                        }
                        else
                        {
                            var e = message.GetError();
                            OvrAvatarLog.LogError($"아바타를 로드하는데 실패하였습니다. [{e.Message}]. 프리셋 아바타로 대체합니다.", LOGScope);
                        }

                        getUserIdComplete = true;
                    });

                    while (!getUserIdComplete)
                    {
                        yield return null;
                    }

                    yield return LoadUserAvatar();
                }
                else
                {
                    yield return RetryHasAvatarRequest();
                }
            }
            else if (Platform == SupportedPlatform.Steam)
            {
                if (PXRMetaAvatarPlatformInit.status != MetaAvatarPlatformInitStatus.Succeeded)
                {
                    PXRMetaAvatarPlatformInit.InitializeMetaAvatarStandAlonePlatform(uniqueId, nickname);
                }

                LoadPresetAvatar();
                yield return null;
            }
        }

        private IEnumerator LoadUserAvatar()
        {
#if DIVE_PLATFORM_META

#if UNITY_ANDROID

            yield return RetryHasAvatarRequest();
            yield break;

#elif UNITY_STANDALONE
            LoadPresetAvatar();
            yield return null;

#endif

#elif DIVE_PLATFORM_STEAM
            LoadPresetAvatar();
            yield return null;

#endif
        }

        private void LoadPresetAvatar()
        {
#if DIVE_PLATFORM_META

#if UNITY_ANDROID

            var assetPostfix = GetZipAssetPostfix();
            var assetPath = $"{PresetAvatarIndex}{assetPostfix}";

            LoadAssets(new[] {assetPath}, AssetSource.Zip);

#elif UNITY_STANDALONE
            var assetPostfix = GetZipAssetPostfix();
            var assetPath = $"{PresetAvatarIndex}{assetPostfix}";

            LoadAssets(new[] {assetPath}, AssetSource.Zip);

#endif

#elif DIVE_PLATFORM_STEAM
            var assetPostfix = GetZipAssetPostfix();
            var assetPath = $"{PresetAvatarIndex}{assetPostfix}";

            LoadAssets(new[] {assetPath}, AssetSource.Zip);

#endif
        }

        private string GetZipAssetPostfix()
        {
            var assetPostfix =
                $"_{OvrAvatarManager.Instance.GetPlatformGLBPostfix(_creationInfo.renderFilters.quality, true)}" +
                $"{OvrAvatarManager.Instance.GetPlatformGLBVersion(_creationInfo.renderFilters.quality, true)}" +
                $"{OvrAvatarManager.Instance.GetPlatformGLBExtension(true)}";

            return assetPostfix;
        }

        private string GetStreamingAssetPostfix()
        {
            var assetPostfix =
                $"_{OvrAvatarManager.Instance.GetPlatformGLBPostfix(_creationInfo.renderFilters.quality, false)}" +
                $"{OvrAvatarManager.Instance.GetPlatformGLBVersion(_creationInfo.renderFilters.quality, false)}" +
                $"{OvrAvatarManager.Instance.GetPlatformGLBExtension(false)}";

            return assetPostfix;
        }


        // 해당 유저의 아바타 데이터가 없는 경우 프리셋 아바타를 생성
        private void UserHasNoAvatarFallback()
        {
            OvrAvatarLog.LogError(
                $"UserId가 {_userId}인 아바타 데이터를 찾을 수 없습니다. 프리셋 아바타로 대체합니다.", LOGScope, this);

            LoadPresetAvatar();
        }

        private IEnumerator RetryHasAvatarRequest()
        {
            const float hasAvatarRetryWaitTime = 4.0f;
            const int hasAvatarRetryAttempts = 12;

            var totalAttempts = UseAutoRetryAvatarLoad ? hasAvatarRetryAttempts : 1;
            var continueRetries = UseAutoRetryAvatarLoad;
            var retriesRemaining = totalAttempts;
            var hadFoundAvatarData = false;
            var requestComplete = false;

            do
            {
                var hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);
                while (!hasAvatarRequest.IsCompleted)
                {
                    yield return null;
                }

                switch (hasAvatarRequest.Result)
                {
                    case OvrAvatarManager.HasAvatarRequestResultCode.HasAvatar:
                        hadFoundAvatarData = true;
                        requestComplete = true;
                        continueRetries = false;

                        yield return AutoRetryLoadUser(true);
                        break;
                    case OvrAvatarManager.HasAvatarRequestResultCode.BadParameter:
                        continueRetries = false;
                        OvrAvatarLog.LogError("유효하지 않은 UserId에 접근을 시도하였습니다.", LOGScope, this);
                        break;
                    case OvrAvatarManager.HasAvatarRequestResultCode.SendFailed:
                        OvrAvatarLog.LogError("아바타 상태 요청을 전송할 수 없습니다.", LOGScope, this);
                        break;
                    case OvrAvatarManager.HasAvatarRequestResultCode.RequestFailed:
                        OvrAvatarLog.LogError("아바타 상태 Query에서 문제가 발생하였습니다", LOGScope, this);
                        break;
                    case OvrAvatarManager.HasAvatarRequestResultCode.RequestCancelled:
                        continueRetries = false;
                        OvrAvatarLog.LogInfo("HasAvatar 요청이 취소되었습니다.", LOGScope, this);
                        break;
                    case OvrAvatarManager.HasAvatarRequestResultCode.HasNoAvatar:
                        requestComplete = true;
                        continueRetries = false;
                        OvrAvatarLog.LogDebug("유저가 아바타 데이터를 가지고 있지 않습니다. 프리셋 아바타로 대체합니다.", LOGScope, this);
                        break;
                    case OvrAvatarManager.HasAvatarRequestResultCode.UnknownError:
                    default:
                        OvrAvatarLog.LogError(
                            $"알수 없는 오류가 발생하였습니다. [{hasAvatarRequest.Result}]. 프리셋 아바타로 대체합니다.", LOGScope, this);
                        break;
                }

                continueRetries &= --retriesRemaining > 0;
                if (continueRetries)
                {
                    yield return new WaitForSecondsRealtime(hasAvatarRetryWaitTime);
                }
            } while (continueRetries);

            if (!requestComplete)
            {
                OvrAvatarLog.LogError($"{totalAttempts}번의 시도를 하였지만 Query를 보낼 수 없습니다.", LOGScope, this);
            }

            if (!hadFoundAvatarData)
            {
                UserHasNoAvatarFallback();
            }
        }

        private IEnumerator AutoRetryLoadUser(bool loadFallbackOnFailure)
        {
            const float loadUserPollingInterval = 4.0f;
            const float loadUserBackoffFactor = 1.618033988f;
            const int cdnRetryAttempts = 13;

            var totalAttempts = UseAutoRetryAvatarLoad ? cdnRetryAttempts : 1;
            var remainingAttempts = totalAttempts;
            var didLoadAvatar = false;
            var currentPollingInterval = loadUserPollingInterval;

            do
            {
                LoadUser();

                Oculus.Avatar2.CAPI.ovrAvatar2Result status;
                do
                {
                    yield return new WaitForSecondsRealtime(currentPollingInterval);

                    status = entityStatus;
                    if (status.IsSuccess() || HasNonDefaultAvatar)
                    {
                        didLoadAvatar = true;
                        remainingAttempts = 0;

                        OvrAvatarLog.LogDebug(
                            "재시도 결과 아바타를 정상적으로 다운로드하였습니다. 루틴을 종료합니다.", LOGScope, this);
                        break;
                    }

                    currentPollingInterval *= loadUserBackoffFactor;
                } while (status == Oculus.Avatar2.CAPI.ovrAvatar2Result.Pending);
            } while (--remainingAttempts > 0);

            if (loadFallbackOnFailure && !didLoadAvatar)
            {
                OvrAvatarLog.LogError(
                    $"{totalAttempts}번의 시도를 하였지만 아바타 데이터를 다운로드 할 수 없습니다.",
                    LOGScope, this);

                UserHasNoAvatarFallback();
            }
        }

        protected override void OnSkeletonLoaded()
        {
            base.OnSkeletonLoaded();

            foreach (var evt in JointLoadedEvents)
            {
                var jointTransform = GetJointTransform(evt.Joint);
                if (evt.TargetToSetAsChild != null)
                {
                    evt.TargetToSetAsChild.SetParent(jointTransform, false);
                }

                evt.OnLoaded?.Invoke(jointTransform);
            }
        }

        #endregion
    }
}

#endif