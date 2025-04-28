using UnityEngine;

namespace Dive.VRModule
{
    public class PXRInteractAddon : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private HandSide handSide = HandSide.Left;
        private PXRHandAnimationController controller;
        private PXRGrabber grabber;
        private PXRIntensifyInteractBase[] intensifyInteracts;

        #endregion

        #region Public Methods

        public bool IsPressable(IIntensifyInteract intensifyInteract)
        {
            if (intensifyInteract.ModeType == InteractMode.Finger)
                return IsPressable(intensifyInteract.Type);

            if (intensifyInteract.ModeType == InteractMode.Palm)
                return true;

            return false;
        }

        #endregion

        #region Private Methods

        private void Awake()
        {
            grabber = handSide == HandSide.Left ? PXRRig.LeftGrabber : PXRRig.RightGrabber;
            controller = handSide == HandSide.Left ? PXRRig.LeftHandAnimationController : PXRRig.RightHandAnimationController;
            intensifyInteracts = transform.parent.GetComponentsInChildren<PXRIntensifyInteractBase>();

            foreach (var intensifyInteract in intensifyInteracts)
            {
                intensifyInteract.Initialize(this);
            }
        }

        private void FixedUpdate()
        {
            if (grabber.IsGrabbing)
            {
                foreach (var intensifyInteract in intensifyInteracts)
                {
                    var interact = intensifyInteract.GetIntensifyInteractBase();
                    interact.ForceAllRelease();
                }
                
                return;
            }

            ExecuteDirectable();
        }

        private void ExecuteDirectable()
        {
            foreach (var interact in intensifyInteracts)
            {
                var mode = interact.ModeType;

                if (mode == InteractMode.None)
                    continue;

                interact.OverlapAtPoint(mode);
                // var overlap = interact.OverlapAtPoint(mode);
                // var length = overlap.Item2;
            }
        }

        private bool IsPressable(int fingerType)
        {
            var finger = (FingerType)fingerType;

            if(grabber.IsGrabbing)
                return false;
            
            switch (finger)
            {
                case FingerType.Thumb:
                    return !controller.IsThumbTouch;
                case FingerType.Index:
                    if (!controller.IsIndexTouch)
                        return true;

                    if (!controller.IsThumbTouch)
                        return controller.FlexParam <= 0.25;

                    if (controller.FlexParam == 0 && controller.PinchParam <= 0.6f)
                        return true;

                    return controller.FlexParam < 0.2f && controller.PinchParam <= 0.35f;

                case FingerType.Middle:
                    return controller.FlexParam <= 0.1f;
                case FingerType.Ring:
                    return controller.FlexParam <= 0.1f;
                case FingerType.Pinky:
                    return controller.FlexParam == 0;
                case FingerType.All:
                case FingerType.None:
                default:
                    return false;
            }
        }

        #endregion
    }
}