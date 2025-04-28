using System;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public class PXRPullableLeverVariable : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private Transform jointTransform;

        [SerializeField]
        private Transform axisTransform;

        [SerializeField]
        private Transform staticCenterTransform;
        
        [SerializeField]
        private float leverLength = 0.5f;

        [SerializeField, Range(0.01f, 0.8f)]
        private float stateChangeThreshold = 0.1f;

        [SerializeField, Range(0.01f, 0.8f)]
        private float deadZoneRatio = 0.1f;

        [SerializeField]
        private Vector2 frameSize = new Vector2(0.2f, 0.2f);

        [SerializeField, ReadOnly]
        private float preGrabAxis = 0;

        [SerializeField]
        private int angle = 20;
        
        #endregion

        #region Public Properties

        public Transform JointTransform => jointTransform;
        public Transform AxisTransform => axisTransform;
        public Transform StaticCenterTransform => staticCenterTransform;
        public float LeverLength => leverLength;
        public float StateChangeThreshold => stateChangeThreshold;
        public Vector2 FrameSize => frameSize;
        public float DeadZoneRatio => deadZoneRatio;
        public int Angle => angle;

        public float PreGrabAxis
        {
            get => preGrabAxis;
            set => preGrabAxis = value;
        }

        #endregion
    }
}