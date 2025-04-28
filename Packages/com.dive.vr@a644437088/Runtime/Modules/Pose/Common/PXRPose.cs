using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    public class PXRPose : ScriptableObject
    {
        [Header("Pose Name")]
        public string PoseName;

        [SerializeField, Header("Joint Definitions")]
        public PXRPoseDefinition Joints;
    }
}
