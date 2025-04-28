using UnityEngine;

namespace Dive.VRModule
{
    public class PXRPullablePumpVariable : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        // ReSharper disable once NotAccessedField.Local
        private PXRPullablePump pump;

        [SerializeField]
        private PXRPumpGrabbable leftGrabbable;
        
        [SerializeField]
        private PXRPumpGrabbable rightGrabbable;
        
        [SerializeField]
        private Transform leftHandleTransform;

        [SerializeField]
        private Transform rightHandleTransform;

        [SerializeField]
        private Transform headTransform;

        [SerializeField]
        private float pumpHeight = 0.45f;

        [SerializeField]
        private float pumpWidth = 0.105f;
        
        [SerializeField]
        private float preRatio = 0f;

        #endregion

        #region Public Properties

        public PXRPumpGrabbable LeftGrabbable => leftGrabbable;
        public PXRPumpGrabbable RightGrabbable => rightGrabbable;
        public Transform LeftHandleTransform => leftHandleTransform;
        public Transform RightHandleTransform => rightHandleTransform;
        public Transform HeadTransform => headTransform;
        public float PumpHeight => pumpHeight;
        public float PumpWidth => pumpWidth;
        
        public float PreRatio
        {
            get => preRatio;
            set => preRatio = value;
        }

        #endregion
    }
}