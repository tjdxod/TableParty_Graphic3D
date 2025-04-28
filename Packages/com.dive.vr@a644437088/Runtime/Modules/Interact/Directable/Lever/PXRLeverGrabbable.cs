using UnityEngine;

namespace Dive.VRModule
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class PXRLeverGrabbable : PXRGrabbable
    {
        [SerializeField]
        private PXRPullableLever lever;

        [SerializeField]
        private GameObject objLeftHand;
        
        [SerializeField]
        private GameObject objRightHand;
        
        protected override void Awake()
        {
            base.Awake();
            
            GrabbedEvent += OnGrabbedEvent;
            ReleasedEvent += OnReleasedEvent;
        }
        
        private void OnGrabbedEvent(PXRGrabber grabber, PXRGrabbable grabbable)
        {
            grabbable.FakeHand.SetActive(false);
            grabbable.FakeHand = grabbable.CurrentHandSide == HandSide.Left ? objLeftHand : objRightHand;
            grabbable.FakeHand.SetActive(true);
        }
        
        protected virtual void OnReleasedEvent(PXRGrabber grabber, PXRGrabbable grabbable, HandSide releaseHandSide)
        {
            if (lever.LeverType == LeverType.None)
                return;
            
            objLeftHand.SetActive(false);
            objRightHand.SetActive(false);
            
            lever.ReturnToOrigin(false);

            if (Rigid.isKinematic) 
                return;
            
            Rigid.velocity = Vector3.zero;
            Rigid.angularVelocity = Vector3.zero;
        }
    }
}