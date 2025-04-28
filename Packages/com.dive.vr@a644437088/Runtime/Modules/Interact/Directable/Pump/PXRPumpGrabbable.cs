using UnityEngine;

namespace Dive.VRModule
{
    public class PXRPumpGrabbable : PXRGrabbable
    {
        #region Private Fields

        [SerializeField]
        private HandSide handSide;

        [SerializeField]
        private PXRPullablePump pump;

        // ReSharper disable once NotAccessedField.Local
        private Vector3 originLocalPosition;

        // ReSharper disable once NotAccessedField.Local
        private Quaternion originLocalRotation;

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
            if (handSide == grabber.HandSide)
                return;

            grabber.ForceRelease();
        }

        private void OnReleasedEvent(PXRGrabber grabber, PXRGrabbable grabbable, HandSide releaseHandSide)
        {
            pump.ReturnToOrigin(handSide);

            if (!Rigid.isKinematic)
            {
                Rigid.velocity = Vector3.zero;
                Rigid.angularVelocity = Vector3.zero;
            }
        }

        #endregion
    }
}