#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System.Reflection;
using Dive.VRModule;
using Oculus.Avatar2;
using UnityEngine;

namespace Dive.Avatar.Meta
{
    /// <summary>
    /// 컨트롤러 및 HMD 입력과 메타 아바타를 동기화하는 클래스
    /// </summary>
    public class PXRMetaAvatarInputManager : OvrAvatarInputManager
    {
        private bool useRig;

        protected void Awake()
        {
            useRig = PXRRig.Current != null;
        }

        private void Start()
        {
            if (useRig)
                return;
            
            Debug.LogError($"PXRRig is null");
        }

        protected override void OnTrackingInitialized()
        {
            OvrPluginInvoke("StartFaceTracking");
            OvrPluginInvoke("StartEyeTracking");
            
            IOvrAvatarInputTrackingDelegate? inputTrackingDelegate = null;
            
            inputTrackingDelegate = new PXRMetaAvatarTrackingDelegate(PXRRig.Current);
            
            var inputControlDelegate = new PXRMetaAvatarControlDelegate();
            _inputTrackingProvider = new OvrAvatarInputTrackingDelegatedProvider(inputTrackingDelegate);
            _inputControlProvider = new OvrAvatarInputControlDelegatedProvider(inputControlDelegate);
        }
        
        private static void OvrPluginInvoke(string method)
        {
            typeof(OVRPlugin).GetMethod(method, BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
        }
    }
}

#endif