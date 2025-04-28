using UnityEngine;
using UnityEngine.InputSystem;

namespace Dive.VRModule
{
    public class PXRInputHandlerPico : PXRInputHandlerBase
    {
        #region Private Fields

        [Header("트리거"), SerializeField]
        private InputActionReference triggerReference; // Trigger

        [SerializeField]
        private InputActionReference triggerTouchReference;

        [Header("그립"), SerializeField]
        private InputActionReference gripReference; // Grip

        [Header("메뉴"), SerializeField]
        private InputActionReference menuReference;

        [Header("Primary 버튼"), SerializeField]
        private InputActionReference primaryButtonClickReference;

        [SerializeField]
        private InputActionReference primaryButtonTouchReference;

        [Header("Secondary 버튼"), SerializeField]
        private InputActionReference secondaryButtonClickReference;

        [SerializeField]
        private InputActionReference secondaryButtonTouchReference;

        [Header("PrimaryAxis"), SerializeField]
        private InputActionReference primaryAxisReference; // Oculus : 스틱, Odyssey : 스틱

        [SerializeField]
        private InputActionReference primaryAxisClickReference;

        [SerializeField]
        private InputActionReference primaryAxisTouchReference;

        [Header("SecondAxis"), SerializeField]
        private InputActionReference secondaryAxisReference; // Oculus : X, Odyssey : X

        [SerializeField]
        private InputActionReference secondaryAxisClickReference;

        [SerializeField]
        private InputActionReference secondaryAxisTouchReference;

        [SerializeField]
        private InputActionReference secondaryAxisForceReference;

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Axis 컨트롤러의 방향을 반환
        /// </summary>
        /// <param name="axis">Axis 컨트롤러 enum</param>
        /// <returns>Axis 컨트롤러의 방향</returns>
        public override Vector2 GetAxisValue(ControllerAxis axis)
        {
            var result = Vector2.zero;

            if (axis == ControllerAxis.Primary)
                result = primaryAxisReference.action.ReadValue<Vector2>();
            else if (axis == ControllerAxis.Secondary)
                result = secondaryAxisReference.action.ReadValue<Vector2>();

            if (Mathf.Abs(result.x) < StickDeadZone.x) result.x = 0f;
            if (Mathf.Abs(result.y) < StickDeadZone.y) result.y = 0f;

            return result;
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// InputAction 및 버튼 상태 초기화
        /// </summary>
        protected override void Awake()
        {
            EnableActions();
            base.Awake();
        }

        /// <summary>
        /// 모든 Input Action들을 활성화 (OpenXR)
        /// </summary>
        private void EnableActions()
        {
            EnableAction(triggerReference);
            EnableAction(triggerTouchReference);

            EnableAction(gripReference);

            EnableAction(menuReference);

            EnableAction(primaryButtonClickReference);
            EnableAction(primaryButtonTouchReference);

            EnableAction(secondaryButtonClickReference);
            EnableAction(secondaryButtonTouchReference);

            EnableAction(primaryAxisReference);
            EnableAction(primaryAxisClickReference);
            EnableAction(primaryAxisTouchReference);

            EnableAction(secondaryAxisReference);
            EnableAction(secondaryAxisClickReference);
            EnableAction(secondaryAxisTouchReference);
            EnableAction(secondaryAxisForceReference);
        }

        /// <summary>
        /// 버튼의 입력 상태를 감지
        /// </summary>
        /// <param name="button">버튼</param>
        /// <param name="buttonState">버튼의 상태</param>
        protected override void CheckButtonState(Buttons button, ref ButtonState buttonState)
        {
            ResetButtonState(ref buttonState);
            var clickValue = 0f;
            var touchValue = 0f;

            switch (button)
            {
                case Buttons.Trigger:
                    touchValue = triggerTouchReference.action.ReadValue<float>();
                    buttonState.isTouch = touchValue > touchedThreshold;

                    buttonState.value = triggerReference.action.ReadValue<float>();

                    SetButtonState(ref buttonState, buttonState.value > pressedThreshold);
                    buttonStates[button] = buttonState;
                    break;
                case Buttons.Grip:
                    buttonState.value = gripReference.action.ReadValue<float>();
                    SetButtonState(ref buttonState, buttonState.value > pressedThreshold);
                    buttonStates[button] = buttonState;
                    break;
                case Buttons.PrimaryAxis:
                    touchValue = primaryAxisTouchReference.action.ReadValue<float>();
                    buttonState.isTouch = touchValue > touchedThreshold;

                    clickValue = primaryAxisClickReference.action.ReadValue<float>();
                    buttonState.value = clickValue > 0.5f ? 1f : 0f;

                    SetButtonState(ref buttonState, clickValue > 0.5f);
                    buttonStates[button] = buttonState;
                    break;
                case Buttons.Primary:
                    touchValue = primaryButtonTouchReference.action.ReadValue<float>();
                    buttonState.isTouch = touchValue > touchedThreshold;

                    clickValue = primaryButtonClickReference.action.ReadValue<float>();
                    buttonState.value = clickValue > 0.5f ? 1f : 0f;

                    SetButtonState(ref buttonState, clickValue > 0.5f);
                    buttonStates[button] = buttonState;
                    break;
                case Buttons.SecondaryAxis:
                    touchValue = secondaryAxisTouchReference.action.ReadValue<float>();
                    buttonState.isTouch = touchValue > touchedThreshold;

                    clickValue = secondaryAxisClickReference.action.ReadValue<float>();
                    buttonState.value = clickValue > 0.5f ? 1f : 0f;

                    SetButtonState(ref buttonState, clickValue > 0.5f);
                    buttonStates[button] = buttonState;
                    break;
                case Buttons.Secondary:
                    touchValue = secondaryButtonTouchReference.action.ReadValue<float>();
                    buttonState.isTouch = touchValue > touchedThreshold;

                    clickValue = secondaryButtonClickReference.action.ReadValue<float>();
                    buttonState.value = clickValue > 0.5f ? 1f : 0f;
                    SetButtonState(ref buttonState, clickValue > 0.5f);
                    buttonStates[button] = buttonState;
                    break;
                case Buttons.Menu:
                    clickValue = menuReference.action.ReadValue<float>();
                    buttonState.value = clickValue > 0.5f ? 1f : 0f;
                    SetButtonState(ref buttonState, clickValue > 0.5f);
                    buttonStates[button] = buttonState;
                    break;
                case Buttons.LeftMouse:
                case Buttons.Default:
                default:
                    break;
            }
        }

        #endregion
    }
}