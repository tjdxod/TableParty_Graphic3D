using System;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public class PXRPullablePump : PXRDirectableBase
    {
        #region Public Fields
        
        public event Action<float> ProgressEvent;
        public event Action<float> AccumulatedEvent;

        #endregion
        
        #region Private Fields

        [SerializeField, ReadOnly, Space, LabelText("눌림 정도 (0~1)")]
        private float ratio = 1;
        
        [SerializeField, LabelText("펌프의 Y축 최대값")]
        private Vector3 maxLocalPosition = new Vector3(0, 0.45f, 0);

        [SerializeField, LabelText("펌프의 Y축 최소값")]
        private Vector3 minLocalPosition = new Vector3(0, 0.25f, 0);
        
        [SerializeField, LabelText("내부 변수")]
        private PXRPullablePumpVariable internalVariable;
        
        [SerializeField, ReadOnly]
        // ReSharper disable once NotAccessedField.Local
        private float accumulatedRatio;
        
        private bool isCompletedGrabbed;
        private PXRLeverGrabbable leverGrabbable;        
        
        #endregion
        
        #region Public Properties

        public Transform LeftHandleTransform => internalVariable.LeftHandleTransform;
        public Transform RightHandleTransform => internalVariable.RightHandleTransform;
        public Transform HeadTransform => internalVariable.HeadTransform;
        public Vector3 RatioPosition => Vector3.Lerp(minLocalPosition, maxLocalPosition, ratio);
        public PXRPumpGrabbable LeftGrabbable => internalVariable.LeftGrabbable;
        public PXRPumpGrabbable RightGrabbable => internalVariable.RightGrabbable;
        public float PumpHeight => internalVariable.PumpHeight;
        public float PumpWidth => internalVariable.PumpWidth;        
        
        #endregion

        #region Private Properties
        
        private float PreRatio
        {
            get => internalVariable.PreRatio;
            set => internalVariable.PreRatio = value;
        }        
        
        #endregion

        #region Public Methods

        public void ReturnToOrigin(HandSide handSide)
        {
            if (handSide == HandSide.Left)
            {
                var leftGrabbableTransform = LeftGrabbable.transform;
                leftGrabbableTransform.localPosition = LeftHandleTransform.localPosition;
                leftGrabbableTransform.localRotation = LeftHandleTransform.localRotation;
            }
            else
            {
                var rightGrabbableTransform = RightGrabbable.transform;
                rightGrabbableTransform.localPosition = RightHandleTransform.localPosition;
                rightGrabbableTransform.localRotation = RightHandleTransform.localRotation;
            }
        }        
        
        public override void ForceRelease(bool useEvent = true)
        {
            
        }

        public override void ForcePress(bool useEvent = true)
        {
            
        }
        
        #endregion
        
        #region Private Methods


        protected override void Awake()
        {
            base.Awake();

            PreRatio = ratio;
        }
        
        /// <summary>
        /// 정해진 시간마다 펌프의 위치를 갱신
        /// </summary>
        /// <example>
        /// 상속하여 사용할때는 다음과 같이 override하여 변경하여야 함.
        /// <code>
        /// protected override void FixedUpdate()
        /// {
        ///     if (!IsEnablePump())
        ///     {
        ///         accumulatedRatio = 0;
        ///         return;
        ///     }
        /// 
        ///     if (photonView.IsMine)
        ///     {
        ///         ratio = SetRatio();
        ///     }
        ///     else
        ///     {
        ///         ratio = ratio;
        ///     }
        ///
        ///     OnAccumulated(ratio);
        ///     OnProgress();
        /// }
        /// </code>
        /// </example>
        protected virtual void FixedUpdate()
        {
            if (!IsEnablePump())
            {
                accumulatedRatio = 0;
                return;
            }
            
            ratio = SetRatio();
            OnAccumulated(ratio);
            OnProgress();
        }

        private float SetRatio()
        {
            var centerPosition = Vector3.Lerp(LeftGrabbable.transform.position, RightGrabbable.transform.position, 0.5f);
            var centerLocalPosition = WorldToLocalPosition(centerPosition, transform);
            var y = Mathf.Clamp(centerLocalPosition.y, minLocalPosition.y, maxLocalPosition.y);

            var distance = Mathf.Abs(maxLocalPosition.y - minLocalPosition.y);
            var yRatio = Mathf.Abs(y - minLocalPosition.y) / distance;

            var equal = Mathf.Approximately(PreRatio, yRatio);

            return equal ? PreRatio : yRatio;
        }
        
        private void OnAccumulated(float yRatio)
        {
            var compare = 0f;
            
            var equal = Mathf.Approximately(PreRatio, yRatio);
            if (equal)
                return;
            
            if (PreRatio - yRatio > 0)
            {
                // 펌프가 내려가는 중
                compare = PreRatio - yRatio;
            }
            else
            {
                // 펌프가 올라가는 중
                compare = yRatio - PreRatio;
            }
            
            AccumulatedEvent?.Invoke(compare);
            accumulatedRatio += compare;
        }
        
        private void OnProgress()
        {
            PreRatio = ratio;
            
            HeadTransform.localPosition = Vector3.Lerp(HeadTransform.localPosition, RatioPosition, Time.deltaTime * 10f);
            ProgressEvent?.Invoke(ratio);
        }

        protected virtual bool IsEnablePump()
        {
            if (!CanInteract)
                return false;
            
            if(LeftGrabbable == null || RightGrabbable == null)
                return false;
            
            if(!LeftGrabbable.IsGrabbed || !RightGrabbable.IsGrabbed)
                return false;
            
            return true;
        }        
        
        #endregion
    }
}
