using UnityEngine;

namespace Dive.VRModule
{
    public class PXRValveGrabbable : PXRGrabbable
    {
        #region Private Fields

        [SerializeField]
        private PXRTurnableValve valve;

        [SerializeField]
        private int index = 0;

        private Vector3 originLocalPosition;
        private Quaternion originLocalRotation;

        #endregion

        #region Public Properties

        public Vector3 OriginLocalPosition => originLocalPosition;

        public Quaternion OriginLocalRotation => originLocalRotation;        
        
        public int Index => index;        
        
        #endregion
        
        #region Private Methods

        protected override void Awake()
        {
            base.Awake();

            originLocalPosition = transform.localPosition;
            // ReSharper disable once Unity.InefficientPropertyAccess
            originLocalRotation = transform.localRotation;

            GrabbedEvent += OnGrabbedEvent;
            ReleasedEvent += OnReleasedEvent;
        }

        private void OnGrabbedEvent(PXRGrabber grabber, PXRGrabbable grabbable)
        {
            var previousPosition = valve.HeadTransform.InverseTransformPoint(transform.position);
            previousPosition.y = 0;

            if (grabbable.CurrentHandSide == HandSide.Left)
                valve.PreviousLeftPosition = previousPosition;
            else
                valve.PreviousRightPosition = previousPosition;

            if (valve.GrabbedCount == 0)
            {
                valve.SetHandAngle(index);
                valve.SetCanInteract(index);
                valve.LeftFakeHand.SetActive(true);
                valve.RightFakeHand.SetActive(true);
            }
            else if (valve.GrabbedCount == 1)
            {
                valve.LeftFakeHand.SetActive(true);
                valve.RightFakeHand.SetActive(true);
            }

            valve.AddGrabbable(grabber, index);
        }

        private void OnReleasedEvent(PXRGrabber grabber, PXRGrabbable grabbable, HandSide releaseHandSide)
        {
            if (valve.GrabbedCount == 1)
            {
                valve.LeftFakeHand.SetActive(false);
                valve.RightFakeHand.SetActive(false);
                valve.ResetCanInteract();
            }

            valve.ReturnToOrigin(this);
            valve.RemoveGrabbable(grabber, index);
        }        
        
        #endregion
    }
}