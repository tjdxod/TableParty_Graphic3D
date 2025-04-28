using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    public class PXRBaseControllerScriptableObject : ScriptableObject
    {
        [System.Serializable]
        public struct OpenXRRuntime
        {
            public OpenXRRuntimeType openXRRuntime;
            public List<DevicePosRot> devicePosRots;
        }
        
        [System.Serializable]
        public struct DevicePosRot
        {
            public SupportOffsetDevice supportOffsetDevice;
            public Vector3 positionOffset;
            public Vector3 rotationOffset;
        }
        
        [SerializeField]
        private List<OpenXRRuntime> openXRRuntimes = new List<OpenXRRuntime>();
        
        public Vector3 GetPositionOffset(OpenXRRuntimeType runtime, SupportOffsetDevice supportOffsetDevice)
        {
            foreach (var openXRRuntime in openXRRuntimes)
            {
                if(openXRRuntime.openXRRuntime != runtime)
                    continue;
                
                foreach (var devicePosRot in openXRRuntime.devicePosRots)
                {
                    if (devicePosRot.supportOffsetDevice == supportOffsetDevice)
                    {
                        return devicePosRot.positionOffset;
                    }
                }
            }

            return Vector3.zero;
        }
        
        public Vector3 GetRotationOffset(OpenXRRuntimeType runtime, SupportOffsetDevice supportOffsetDevice)
        {
            foreach (var openXRRuntime in openXRRuntimes)
            {
                if(openXRRuntime.openXRRuntime != runtime)
                    continue;
                
                foreach (var devicePosRot in openXRRuntime.devicePosRots)
                {
                    if (devicePosRot.supportOffsetDevice == supportOffsetDevice)
                    {
                        return devicePosRot.rotationOffset;
                    }
                }
            }

            return Vector3.zero;
        }
        
        public (Vector3, Vector3) GetPosRotOffset(OpenXRRuntimeType runtime, SupportOffsetDevice supportOffsetDevice)
        {
            foreach (var openXRRuntime in openXRRuntimes)
            {
                if(openXRRuntime.openXRRuntime != runtime)
                    continue;
                
                foreach (var devicePosRot in openXRRuntime.devicePosRots)
                {
                    if (devicePosRot.supportOffsetDevice == supportOffsetDevice)
                    {
                        return (devicePosRot.positionOffset, devicePosRot.rotationOffset);
                    }
                }
            }

            return (Vector3.zero, Vector3.zero);
        }
    }
}
