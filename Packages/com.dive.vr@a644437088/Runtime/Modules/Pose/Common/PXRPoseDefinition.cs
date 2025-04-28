using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    [System.Serializable]
    public class PXRPoseDefinition
    {
        [Header("엄지")]
        public PXRFingerJoint[] ThumbJoints;

        [Header("검지")]
        public PXRFingerJoint[] IndexJoints;

        [Header("중지")]
        public PXRFingerJoint[] MiddleJoints;

        [Header("약지")]
        public PXRFingerJoint[] RingJoints;

        [Header("소지")]
        public PXRFingerJoint[] PinkyJoints;
    }
}
