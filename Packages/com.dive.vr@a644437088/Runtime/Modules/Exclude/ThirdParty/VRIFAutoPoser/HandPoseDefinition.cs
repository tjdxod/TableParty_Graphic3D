using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {
    [System.Serializable]
    public class HandPoseDefinition {
        [SerializeField]
        [Header("Wrist")]
        public FingerJoint WristJoint;

        [SerializeField]
        [Header("Thumb")]
        public FingerJoint[] ThumbJoints;

        [SerializeField]
        [Header("Index")]
        public FingerJoint[] IndexJoints;

        [SerializeField]
        [Header("Middle")]
        public FingerJoint[] MiddleJoints;

        [SerializeField]
        [Header("Ring")]
        public FingerJoint[] RingJoints;

        [SerializeField]
        [Header("Pinky")]
        public FingerJoint[] PinkyJoints;

        [SerializeField]
        [Header("Other")]
        public FingerJoint[] OtherJoints;
    }
}

