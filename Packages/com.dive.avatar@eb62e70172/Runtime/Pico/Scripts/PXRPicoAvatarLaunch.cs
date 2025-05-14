#if DIVE_PLATFORM_PICO

using System;
using Cysharp.Threading.Tasks;
using Pico.Avatar;

namespace Dive.Avatar.Pico
{
    public class PXRPicoAvatarLaunch : PXRPicoAvatarLaunchBase
    {
        public event Action ReadyToLoadAvatarEvent;
        
        private bool loginPlatformSDK = false;
        
        private void Awake()
        {
            Action<bool> loginCallback = (state) =>
            {
                loginPlatformSDK = state;
            };
            
            LoginPlatform(loginCallback);
        }

        private async void Start()
        {
            PXRCheck();
            
            await UniTask.WaitUntil(() => loginPlatformSDK);

            await UniTask.WaitUntil(() => PicoAvatarApp.isWorking);

            StartPicoAvatarApp();
            
            await UniTask.WaitUntil(() => PicoAvatarManager.canLoadAvatar);

            ReadyToLoadAvatarEvent?.Invoke();
        }
        
#if UNITY_EDITOR
        protected override void OnPicoAvatarAppStartTestModel()
        {
            base.OnPicoAvatarAppStartTestModel();
            this.UserServiceUserID = PicoAvatarPlatformInfoTestModelUtils.UserID;
        }
#endif
    }
}

#endif