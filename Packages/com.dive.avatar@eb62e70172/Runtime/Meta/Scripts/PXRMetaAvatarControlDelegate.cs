#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using Dive.VRModule;
using Oculus.Avatar2;

namespace Dive.Avatar.Meta
{
    /// <summary>
    /// 메타 아바타와 컨트롤러 입력 값을 동기화하는 클래스
    /// </summary>
    public class PXRMetaAvatarControlDelegate : OvrAvatarInputControlDelegate
    {
        /// <summary>
        /// 컨트롤러의 입력상태를 업데이트
        /// </summary>
        /// <param name="inputControlState">현재 입력 상태</param>
        /// <returns>입력 값이 정상적으로 받아지는지</returns>
        public override bool GetInputControlState(out OvrAvatarInputControlState inputControlState)
        {
            inputControlState = new OvrAvatarInputControlState
            {
                type = GetControllerType()
            };

            UpdateControllerInput(ref inputControlState.leftControllerState, OVRInput.Controller.LTouch);
            UpdateControllerInput(ref inputControlState.rightControllerState, OVRInput.Controller.RTouch);

            return true;
        }

        /// <summary>
        /// OvrAvatarControllerState를 업데이트
        /// </summary>
        /// <param name="controllerState">업데이트하고자 하는 OvrAvatarControllerState</param>
        /// <param name="controller">컨트롤러 enum</param>
        private static void UpdateControllerInput(ref OvrAvatarControllerState controllerState, OVRInput.Controller controller)
        {
            controllerState.buttonMask = 0;
            controllerState.touchMask = 0;

            var isLeft = controller == OVRInput.Controller.LTouch;

            var controllerBase = isLeft ? PXRInputBridge.LeftController : PXRInputBridge.RightController;
            
            if(controllerBase == null)
                return;
            
            if (controllerBase.GetButtonState(Buttons.Primary).isStay)
            {
                controllerState.buttonMask |= CAPI.ovrAvatar2Button.One;
            }

            if (controllerBase.GetButtonState(Buttons.Secondary).isStay)
            {
                controllerState.buttonMask |= CAPI.ovrAvatar2Button.Two;
            }

            if (controllerBase.GetButtonState(Buttons.PrimaryAxis).isStay)
            {
                controllerState.buttonMask |= CAPI.ovrAvatar2Button.Joystick;
            }

            if (controllerBase.GetButtonState(Buttons.Primary).isTouch)
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.One;
            }

            if (controllerBase.GetButtonState(Buttons.Secondary).isTouch)
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.Two;
            }

            if (controllerBase.GetButtonState(Buttons.PrimaryAxis).isTouch)
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.Joystick;
            }

            controllerState.indexTrigger = (float)controllerBase.GetButtonState(Buttons.Trigger).value;
            if (controllerBase.GetButtonState(Buttons.Trigger).isTouch)
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.Index;
            }
            else if (controllerBase.GetButtonState(Buttons.Trigger).value <= 0f)
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.Pointing;
            }

            controllerState.handTrigger = (float)controllerBase.GetButtonState(Buttons.Grip).value;

            if ((controllerState.touchMask & (CAPI.ovrAvatar2Touch.One | CAPI.ovrAvatar2Touch.Two | CAPI.ovrAvatar2Touch.Joystick)) == 0)
            {
                controllerState.touchMask |= CAPI.ovrAvatar2Touch.ThumbUp;
            }
        }
    }
}

#endif