using System;
using Dive.Utility;
using UnityEngine;
using UnityEngine.XR;

namespace Dive.VRModule
{
    /// <summary>
    /// 컨트롤러와 SDK를 연결하는 클래스
    /// </summary>
    public class PXRInputBridge : MonoBehaviour
    {
        #region Private Fields

        private static StaticVar<PXRInputHandlerBase> leftController;
        private static StaticVar<PXRInputHandlerBase> rightController;
        
        #endregion

        #region Public Properties

        public static bool IsTrackingHMD
        {
            get
            {
                var headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);

                if (!headDevice.isValid)
                    return false;
                
                headDevice.TryGetFeatureValue(CommonUsages.userPresence, out var userPresent);
                return userPresent;
            }
        }
        
        public static PXRInputHandlerBase LeftController => leftController.Value;

        public static ButtonState LeftTrigger => LeftController.GetButtonState(Buttons.Trigger);
        public static ButtonState LeftGrip => LeftController.GetButtonState(Buttons.Grip);
        public static ButtonState LeftMenu => LeftController.GetButtonState(Buttons.Menu);
        public static ButtonState LeftPrimaryAxis => LeftController.GetButtonState(Buttons.PrimaryAxis);
        public static ButtonState LeftSecondaryAxis => LeftController.GetButtonState(Buttons.SecondaryAxis);
        public static ButtonState LeftPrimaryButton => LeftController.GetButtonState(Buttons.Primary);
        public static ButtonState LeftSecondaryButton => LeftController.GetButtonState(Buttons.Secondary);
        
        public static PXRInputHandlerBase RightController => rightController.Value;

        public static ButtonState RightTrigger => RightController.GetButtonState(Buttons.Trigger);
        public static ButtonState RightGrip => RightController.GetButtonState(Buttons.Grip);
        public static ButtonState RightMenu => RightController.GetButtonState(Buttons.Menu);
        public static ButtonState RightPrimaryAxis => RightController.GetButtonState(Buttons.PrimaryAxis);
        public static ButtonState RightSecondaryAxis => RightController.GetButtonState(Buttons.SecondaryAxis);
        public static ButtonState RightPrimaryButton => RightController.GetButtonState(Buttons.Primary);
        public static ButtonState RightSecondaryButton => RightController.GetButtonState(Buttons.Secondary);
        
        #endregion

        #region Private Properties

        /// <summary>
        /// 진동을 사용하는 경우 true, 그렇지 않은 경우 false
        /// </summary>        
        internal static bool UseHaptic { get; private set; }
        
        internal static float HapticIntensity { get; private set; }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// 손 방향의 InputHandler를 반환
        /// </summary>
        /// <param name="side">손 방향</param>
        /// <returns>손 방향에 맞는 InputHandlerBase</returns>
        public static PXRInputHandlerBase GetXRController(HandSide side)
        {
            if (leftController == null || rightController == null)
                return null;
            
            return side == HandSide.Left ? LeftController : RightController;
        }

        /// <summary>
        /// 모든 컨트롤러의 진동 활성화
        /// </summary>
        public static void ActivateHaptic()
        {
            UseHaptic = true;
            leftController.Value.ActivateHaptic();
            rightController.Value.ActivateHaptic();
        }


        /// <summary>
        /// 모든 컨트롤러의 진동 비활성화
        /// </summary>
        public static void DeactivateHaptic()
        {
            UseHaptic = false;
            leftController.Value.DeactivateHaptic();
            rightController.Value.DeactivateHaptic();
        }

        public static void SetHapticIntensity(float intensity)
        {
            if(intensity is < 0 or > 2)
            {
                Debug.LogWarning("진동 세기는 0과 2사이의 값을 가질 수 있습니다.");
                return;
            }

            HapticIntensity = intensity;
        }
        
        #endregion

        #region Private Methods

        private void Awake()
        {
            var controllers = GetComponentsInChildren<PXRInputHandlerBase>(true);

            if (controllers[0].handSide == HandSide.Left)
            {
                leftController = new StaticVar<PXRInputHandlerBase>(controllers[0]);
                rightController = new StaticVar<PXRInputHandlerBase>(controllers[1]);
            }
            else
            {
                rightController = new StaticVar<PXRInputHandlerBase>(controllers[0]);
                leftController = new StaticVar<PXRInputHandlerBase>(controllers[1]);
            }
        }

        /// <summary>
        /// 모든 컨트롤러 진동 활성화 및 비활성화를 관리 (옵션?) 
        /// </summary>
        /// <param name="isVibrationEffect">진동을 활성화 할 경우 true, 그렇지 않은 경우 false</param>
        private void OnVibration(bool isVibrationEffect)
        {
            if (isVibrationEffect)
            {
                ActivateHaptic();
            }
            else
            {
                DeactivateHaptic();
            }
        }

        #endregion
    }
}