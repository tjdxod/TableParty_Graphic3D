using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.Management;

namespace Dive.VRModule
{
    // Deprecated
    /*
    public class PXRRecenter : MonoBehaviour
    {
        public event Action RecenterEvent;

        [SerializeField]
        private bool allowRecentering = true;

        [SerializeField]
        private float floorOffset = 1.5f;

        [SerializeField]
        private bool isShowDebug = false;

        private PXRInputHandlerBase inputHandlerBase; // right controller

        private float recenterTime = 2f;
        private float pressTime = 0f;

        private bool isRecentering = false;

        private void Awake()
        {
#if DIVE_PLATFORM_META

            if (OVRManager.display != null)
            {
                OVRManager.display.RecenteredPose += OnRecenterPose;
            }

#endif

            inputHandlerBase = PXRInputBridge.RightController;

            var xrSettings = XRGeneralSettings.Instance;

            if (xrSettings == null)
                return;

            var xrLoader = xrSettings.Manager.activeLoader;

            if (xrLoader == null)
                return;

            var subsystem = xrLoader.GetLoadedSubsystem<XRInputSubsystem>();

            if (subsystem == null)
                return;

            subsystem.trackingOriginUpdated += OnTrackingOriginUpdated;
            
            OpenXRSettings.SetAllowRecentering(allowRecentering, floorOffset);

            RecenterEvent += OpenXRSettings.RefreshRecenterSpace;
        }

        private void OnDestroy()
        {
#if DIVE_PLATFORM_META

            if (OVRManager.display != null)
            {
                OVRManager.display.RecenteredPose -= OnRecenterPose;
            }
            
#endif

            var xrSettings = XRGeneralSettings.Instance;

            if (xrSettings == null)
                return;

            var xrLoader = xrSettings.Manager.activeLoader;

            if (xrLoader == null)
                return;

            var subsystem = xrLoader.GetLoadedSubsystem<XRInputSubsystem>();

            if (subsystem == null)
                return;

            subsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
            
            RecenterEvent -= OpenXRSettings.RefreshRecenterSpace;
        }

#if DIVE_PLATFORM_META

        private void OnRecenterPose()
        {
            if (isShowDebug)
                Debug.Log("OnRecenterPose");
            
            RecenterEvent?.Invoke();
        }

#endif

        private void OnTrackingOriginUpdated(XRInputSubsystem xrInputSubsystem)
        {
            if (isShowDebug)
                Debug.Log("Tracking Origin Updated");

            RecenterEvent?.Invoke();
        }

        private void Update()
        {
            // 2초 이상 누르면 리센터링
        
            if (inputHandlerBase.GetButtonState(Buttons.Menu).isStay)
            {
                pressTime += Time.deltaTime;
        
                if (!(pressTime >= recenterTime) || isRecentering)
                    return;
        
                OpenXRSettings.RefreshRecenterSpace();
                pressTime = 0f;
                isRecentering = true;
            }
            else
            {
                pressTime = 0f;
                isRecentering = false;
            }
        }
    }*/
}