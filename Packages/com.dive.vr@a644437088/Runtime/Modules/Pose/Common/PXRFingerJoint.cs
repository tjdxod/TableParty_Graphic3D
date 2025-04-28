using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    [System.Serializable]
    public struct PXRFingerJoint 
    {       
        [SerializeField]
        public string TransformName;

        [SerializeField]
        public Quaternion LocalRotation;

        public PXRFingerJoint(Transform target)
        {
            TransformName = target.name;
            LocalRotation = target.localRotation;
        }
    }
}
