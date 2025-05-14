#if DIVE_PLATFORM_PICO

using System;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pico.Avatar;
using Pico.Platform;
using UnityEngine;
using UnityEngine.Networking;

namespace Dive.Avatar.Pico
{
    public abstract class PXRPicoAvatarLaunchBase : MonoBehaviour
    {
        private enum LogState
        {
            None,
            Log,
            Error
        }
        
        #region Private Fields
        
        [SerializeField]
        private LogState logState = LogState.None;
        
        private System.Action<bool> platformLoginFinish = null;        
        
        /// <summary>
        ///Pico developer AppID
        /// </summary>
        private string platformAppID = "";
        /// <summary>
        /// get platform userID by platformSDK UserService
        /// </summary>
        private string userServiceUserID = "";
        /// <summary>
        /// get platform accessToken by platformSDK UserService
        /// </summary>
        private string userServiceAccessToken = "";
        /// <summary>
        /// get platform nativeCode by platformSDK UserService
        /// </summary>
        private string userServiceStoreRegion = "";
        
        #endregion

        #region Public Properties

        public virtual string PlatformAppID
        {
            get
            {
                if (string.IsNullOrEmpty(platformAppID))
                {
                    return PicoAvatarAppConfigUtils.PicoPlatformAppID;
                }
                return platformAppID;
            }
        }
        public virtual string UserServiceUserID
        {
            get => userServiceUserID;
            set => userServiceUserID = value;
        }
        public virtual string UserServiceAccessToken
        {
            get => userServiceAccessToken;
            set => userServiceAccessToken = value;
        }
        public virtual string UserServiceStoreRegion
        {
            get => userServiceStoreRegion;
            set => userServiceStoreRegion = value;
        }

        #endregion

        #region Public Methods

        public virtual void StartPicoAvatarApp()
        {
            var avatarApp = PicoAvatarApp.instance;
            avatarApp.loginSettings.accessToken = this.UserServiceAccessToken;
       
            avatarApp.StartAvatarManager();
        }

        public virtual void LoginPlatform(Action<bool> infoCallback)
        {
            platformLoginFinish = infoCallback;

            if (string.IsNullOrEmpty(PlatformAppID))
            {
                LogError($"Platform App ID의 값이 존재하지 않습니다.");
                OnPlatformResultCall(false);
                return;
            }

            try
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                StartCoroutine(SampleAccessToken()); // sample token
#else
                CoreService.Initialize(PlatformAppID);
                PlatformPermissions();
#endif
            }
            catch (Exception e)
            {
                LogError($"Pico Platform SDK 실행 중 오류가 발생하였습니다. (message: {e.Message})");
                OnPlatformResultCall(false);
            }
        }
        
        #endregion

        #region Private Methods

        protected void PXRCheck()
        {
            Unity.XR.CoreUtils.XROrigin xrGo = null;
            {
                var origin = GameObject.Find("XR Origin");
                if (origin == null)
                {
                    LogError("XR Origin not find");
                    return;
                }
                xrGo = origin.GetComponent<Unity.XR.CoreUtils.XROrigin>();
            }
            if (xrGo == null)
            {
                LogError("XR Origin not find");
                return;
            }

            // PXR 
            var pxrMgr = xrGo.GetComponent<Unity.XR.PXR.PXR_Manager>();
            
            if (pxrMgr == null)
                xrGo.gameObject.AddComponent<Unity.XR.PXR.PXR_Manager>();
        }
        
        protected virtual void OnPicoAvatarAppStartTestModel()
        {
            Log("테스트 모델로 시작합니다.");
        }
        
        protected void OnPlatformResultCall(bool finish)
        {
            if(platformLoginFinish != null)
                platformLoginFinish.Invoke(finish);
        }
        
        protected void PlatformPermissions()
        {
            Log($"유저 권한 요청을 실행합니다.");
            //, "avatar" 
            UserService.RequestUserPermissions("user_info", "avatar").OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    LogError($"유저 권한 요청중 오류가 발생하였습니다. (code: {msg.Error.Code}, message: {msg.Error.Message})");
                    OnPlatformResultCall(false);
                    return;
                }

                Log($"유저 권한 요청에 성공하였습니다. :{msg.Data}");
                Log($"유저 권한 요청을 종료합니다.");
                PlatformUserID();
            });
        }

        protected virtual void PlatformUserID()
        {
            Log($"로그인된 유저 정보를 가져옵니다.");
            UserService.GetLoggedInUser().OnComplete((user) =>
            {
                if (user == null || user.IsError)
                {
                    LogError($"유저 정보를 가져오는 과정에 오류가 발생하였습니다.");
                    OnPlatformResultCall(false);
                    return;
                }
               
                Log($"User Id: {user.Data.ID}, 사용자 이름 = {user.Data.DisplayName}");
                
                userServiceUserID = user.Data.ID;
                userServiceStoreRegion = ApplicationService.GetSystemInfo().IsCnDevice?"oidc-pico-cn":"oidc-pico-global";
                
                if (string.IsNullOrEmpty(this.userServiceStoreRegion))
                    userServiceStoreRegion = "oidc-pico-cn";
                
                Log($"유저의 상점 지역: {userServiceStoreRegion}");
                
                PlatformAccessToken();
                
                // StartCoroutine(SampleAccessToken()); // sample token
            });
            
        }
        
        protected void PlatformAccessToken()
        {
            Log($"유저의 AccessToken을 가져옵니다.");
            UserService.GetAccessToken().OnComplete(delegate (Message<string> message)
            {
                if (message.IsError)
                {
                    var err = message.Error;
                    LogError($"유저의 AccessToken을 가져오는 과정에서 오류가 발생하였습니다.(code: {err.Code}, message: {err.Message})");
                    OnPlatformResultCall(false);
                    return;
                }

                var credential = message.Data;
                Log($"AccessToken 요청이 성공하였습니다.: {credential}");

                userServiceAccessToken = (credential);

                OnPlatformResultCall(true);
            });
        }
        
        protected IEnumerator SampleAccessToken()
        {
            Log($"샘플 토큰을 가져옵니다.");
            yield return new WaitUntil(() => PicoAvatarApp.isWorking);
            OnPicoAvatarAppStartTestModel();
            UnityWebRequest webRequest = UnityWebRequest.Get(NetEnvHelper.GetFullRequestUrl(NetEnvHelper.SampleTokenApi));
            webRequest.timeout = 30;
            webRequest.SetRequestHeader("Content-Type", "application/json"); 
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Log($"샘플 토큰을 가져오는 과저에서 오류가 발생하였습니다. (message: {webRequest.error}");
                OnPlatformResultCall(false);
                yield break;
            }
            UserServiceAccessToken = JsonConvert.DeserializeObject<JObject>(webRequest.downloadHandler.text)?.Value<string>("data");
            userServiceUserID = PicoAvatarPlatformInfoTestModelUtils.UserID;
            OnPlatformResultCall(true);
        }
        
        protected void Log(string content)
        {
            if (logState == LogState.None)
                return;
            
            Debug.Log($"PXRPicoAvatarLaunchBase Log : {content}");
        }
        protected void LogError(string content)
        {
            if(logState is LogState.None or LogState.Log)
                return;
            
            Debug.LogError($"PXRPicoAvatarLaunchBase Error : {content}");
        }
        
        #endregion
        
    }
}

#endif