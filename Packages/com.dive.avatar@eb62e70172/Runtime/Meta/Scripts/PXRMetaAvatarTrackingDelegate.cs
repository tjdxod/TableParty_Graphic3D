#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using Dive.VRModule;
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.XR;
using Node = UnityEngine.XR.XRNode;

namespace Dive.Avatar.Meta
{
    /// <summary>
    /// 메타 아바타와 트래킹을 동기화하는 클래스
    /// </summary>
    public class PXRMetaAvatarTrackingDelegate : OvrAvatarInputTrackingDelegate
    {
        private PXRRig rig;

        public PXRMetaAvatarTrackingDelegate(PXRRig rig)
        {
            this.rig = rig;
        }

        public override bool GetRawInputTrackingState(out OvrAvatarInputTrackingState inputTrackingState)
        {
            inputTrackingState = default;

            bool leftControllerActive = false;
            bool rightControllerActive = false;
            if (OVRInput.GetActiveController() != OVRInput.Controller.Hands)
            {
                leftControllerActive = OVRInput.GetControllerOrientationTracked(OVRInput.Controller.LTouch);
                rightControllerActive = OVRInput.GetControllerOrientationTracked(OVRInput.Controller.RTouch);
            }
            
            if (rig is not null)
            {
                inputTrackingState.headsetActive = true;
                inputTrackingState.leftControllerActive = leftControllerActive;
                inputTrackingState.rightControllerActive = rightControllerActive;
                inputTrackingState.leftControllerVisible = false;
                inputTrackingState.rightControllerVisible = false;
                inputTrackingState.headset = (CAPI.ovrAvatar2Transform)PXRRig.PlayerController.CenterEye;
                inputTrackingState.leftController = (CAPI.ovrAvatar2Transform)PXRRig.PlayerController.LeftHandAnchor;
                inputTrackingState.rightController = (CAPI.ovrAvatar2Transform)PXRRig.PlayerController.RightHandAnchor;
                return true;
            }


            if (!OVRNodeStateProperties.IsHmdPresent())
                return false;

            inputTrackingState.headsetActive = true;
            inputTrackingState.leftControllerActive = leftControllerActive;
            inputTrackingState.rightControllerActive = rightControllerActive;
            inputTrackingState.leftControllerVisible = true;
            inputTrackingState.rightControllerVisible = true;

            if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.CenterEye, NodeStatePropertyType.Position,
                    OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var headPos))
            {
                inputTrackingState.headset.position = headPos;
            }
            else
            {
                inputTrackingState.headset.position = Vector3.zero;
            }

            if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.CenterEye, NodeStatePropertyType.Orientation,
                    OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var headRot))
            {
                inputTrackingState.headset.orientation = headRot;
            }
            else
            {
                inputTrackingState.headset.orientation = Quaternion.identity;
            }

            inputTrackingState.headset.scale = Vector3.one;

            inputTrackingState.leftController.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            inputTrackingState.rightController.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            inputTrackingState.leftController.orientation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
            inputTrackingState.rightController.orientation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            inputTrackingState.leftController.scale = Vector3.one;
            inputTrackingState.rightController.scale = Vector3.one;
            return true;
        }

        protected override void FilterInput(ref OvrAvatarInputTrackingState inputTracking)
        {
            ClampHandPositions(ref inputTracking, out var handDistances, PXRRig.HandClampDistance);
            DisableDistantControllers(ref inputTracking, in handDistances, PXRRig.HandDisableDistance);

            HideInactiveControllers(ref inputTracking);
        }

        protected new static void ClampHandPositions(ref OvrAvatarInputTrackingState tracking, out InputHandDistances distances, float clampDistanceSquared)
        {
            ref readonly var hmdPos = ref tracking.headset.position;
            ref var leftHandPos = ref tracking.leftController.position;
            ref var rightHandPos = ref tracking.rightController.position;

            var leftHandDist = ClampHand(in hmdPos, ref leftHandPos, clampDistanceSquared);
            var rightHandDist = ClampHand(in hmdPos, ref rightHandPos, clampDistanceSquared);

            distances = new InputHandDistances(leftHandDist, rightHandDist);
        }

        protected new static void DisableDistantControllers(ref OvrAvatarInputTrackingState inputTracking, in InputHandDistances handDistances, float disableDistanceSquared)
        {
            DisableDistantController(ref inputTracking.leftControllerActive, handDistances.leftSquared, disableDistanceSquared);
            DisableDistantController(ref inputTracking.rightControllerActive, handDistances.rightSquared, disableDistanceSquared);
        }

        protected new static void HideInactiveControllers(ref OvrAvatarInputTrackingState inputTracking)
        {
            inputTracking.leftControllerVisible &= inputTracking.leftControllerActive;
            inputTracking.rightControllerVisible &= inputTracking.rightControllerActive;
        }
    }
}

#endif