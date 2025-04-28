using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Dive.VRModule
{
    public class PXRInputHandlerPC : PXRInputHandlerBase
    {
        #region Private Fields

        [Header("포인터"), SerializeField]
        // ReSharper disable once NotAccessedField.Local
        private PXRPointerVR myPointer;

        #region InputActionReferences

        [Header("좌측 Primary 버튼"), SerializeField]
        private InputActionReference leftPrimaryReference;

        [Header("좌측 Secondary 버튼"), SerializeField]
        private InputActionReference leftSecondaryReference;

        [Header("우측 Primary 버튼"), SerializeField]
        private InputActionReference rightPrimaryReference;

        [Header("우측 Secondary 버튼"), SerializeField]
        private InputActionReference rightSecondaryReference;

        [FormerlySerializedAs("grabRotationAction")]
        [Header("그랩 로테이션 버튼"), SerializeField]
        private InputActionReference grabRotationReference;

        [Header("메뉴 버튼"), SerializeField]
        private InputActionReference menuReference;

        #endregion

        private ButtonState leftClickMouseState;
        private ButtonState rightClickMouseState;
        private Buttons currentMouseButton;

        private const float ScrollSpeed = 3.5f;

        #endregion

        #region Public Methods

        public override Vector2 GetAxisValue(ControllerAxis axis)
        {
            Vector2 newVec = grabRotationReference.action.ReadValue<Vector2>();
            newVec.y = Mouse.current.scroll.ReadValue().normalized.y * ScrollSpeed;

            return newVec;
        }

        public override void Haptic(float amplitude, float duration)
        {
        }

        #endregion

        #region Private Methods

        protected override void Awake()
        {
            base.Awake();
            EnableActions();
        }

        protected override void Update()
        {
            CheckButtonState(Buttons.PrimaryAxis, ref primaryAxisState);
            CheckButtonState(Buttons.Primary, ref primaryButtonState);
            CheckButtonState(Buttons.Secondary, ref secondaryButtonState);
            CheckButtonState(Buttons.Menu, ref menuButtonState);
            CheckButtonState(Buttons.LeftMouse, ref leftClickMouseState);
            CheckButtonState(Buttons.RightMouse, ref rightClickMouseState);
        }

        private void EnableActions()
        {
            EnableAction(leftPrimaryReference);
            EnableAction(leftSecondaryReference);

            EnableAction(rightPrimaryReference);
            EnableAction(rightSecondaryReference);

            EnableAction(grabRotationReference);

            EnableAction(menuReference);
        }

        protected override void CheckButtonState(Buttons button, ref ButtonState buttonState)
        {
            ResetButtonState(ref buttonState);

            InputActionReference inputActionReference = null;

            switch (button)
            {
                case Buttons.Primary:
                    inputActionReference = handSide == HandSide.Left ? leftPrimaryReference : rightPrimaryReference;
                    buttonState.value = inputActionReference.action.ReadValue<float>() > 0.5f ? 1 : 0;
                    SetButtonState(ref buttonState, buttonState.value > pressedThreshold);
                    buttonStates[button] = buttonState;
                    break;

                case Buttons.Secondary:
                    inputActionReference = handSide == HandSide.Left ? leftSecondaryReference : rightSecondaryReference;
                    buttonState.value = inputActionReference.action.ReadValue<float>() > 0.5f ? 1 : 0;
                    SetButtonState(ref buttonState, buttonState.value > pressedThreshold);
                    buttonStates[button] = buttonState;
                    break;

                case Buttons.Menu:
                    buttonState.value = menuReference.action.ReadValue<float>() > 0.5f ? 1f : 0f;
                    SetButtonState(ref buttonState, buttonState.value > pressedThreshold);
                    buttonStates[button] = buttonState;
                    break;

                case Buttons.LeftMouse:
                    buttonState.value = Mouse.current.leftButton.isPressed ? 1 : 0;
                    SetButtonState(ref buttonState, buttonState.value > pressedThreshold);
                    buttonStates[button] = buttonState;
                    break;

                case Buttons.RightMouse:
                    buttonState.value = Mouse.current.rightButton.isPressed ? 1 : 0;
                    SetButtonState(ref buttonState, buttonState.value > pressedThreshold);
                    buttonStates[button] = buttonState;
                    break;
            }
        }

        #endregion
    }
}