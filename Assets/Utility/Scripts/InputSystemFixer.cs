using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

public class InputSystemFixer : MonoBehaviour
{
    public void Awake()
    {
        // Fix: Stop subsystems and deinitialize the loader if it's already active
        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }

        // Initialize and start the loader
        XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
        XRGeneralSettings.Instance.Manager.StartSubsystems();
    }
}