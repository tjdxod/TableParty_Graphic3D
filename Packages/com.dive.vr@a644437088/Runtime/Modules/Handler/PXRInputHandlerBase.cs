using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dive.VRModule
{
    /// <summary>
    /// 컨트롤러의 버튼상태 (Down, Stay, Up, 값 저장)
    /// </summary>
    [Serializable]
    public struct ButtonState
    {
        #region Public Fields
        
        /// <summary>
        /// 버튼을 누른 상태인 경우 true, 그렇지 않은 경우 false
        /// </summary>
        public bool isStay;
        
        /// <summary>
        /// 버튼이 터치 된 경우 true, 그렇지 않은 경우 false
        /// </summary>
        public bool isTouch;
        
        /// <summary>
        /// 버튼이 내려간 경우 true, 그렇지 않은 경우 false
        /// </summary>
        public bool isDown;
        
        /// <summary>
        /// 버튼이 올라온 경우 true, 그렇지 않은 경우 false
        /// </summary>
        public bool isUp;
        
        /// <summary>
        /// 버튼의 눌림 정도
        /// </summary>
        public double value;
        
        #endregion
    }

    /// <summary>
    /// 컨트롤러의 입력 상태를 관리하는 추상 클래스
    /// </summary>
    public abstract class PXRInputHandlerBase : MonoBehaviour
    {
        #region Public Fields
        
        /// <summary>
        /// 컨트롤러의 손 방향
        /// </summary>
        [SerializeField, Tooltip("사용되는 손")]
        public HandSide handSide;
        
        /// <summary>
        /// 스틱의 데드존 설정
        /// 스틱축값이 해당 일정값 아래로 내려가면 0으로 바꾸기 위한 값
        /// </summary>
        public Vector2 StickDeadZone { get; private set; } = new Vector2(0.15f, 0.15f);     
        
        #endregion
        
        #region Private Fields
        
        [SerializeField]
        private PXRBaseController controller;      
        
        /// <summary>
        /// 컨트롤러 버튼들의 상태를 담아둠
        /// </summary>
        protected readonly Dictionary<Buttons, ButtonState> buttonStates = new Dictionary<Buttons, ButtonState>();

        /// <summary>
        /// 트리거 버튼 상태
        /// </summary>
        [Header("버튼 상태"), SerializeField]
        protected ButtonState triggerState;

        /// <summary>
        /// 그립 버튼 상태
        /// </summary>
        [SerializeField]
        protected ButtonState gripState;

        /// <summary>
        /// Primary 방향 상태
        /// </summary>
        [SerializeField]
        protected ButtonState primaryAxisState;

        /// <summary>
        /// Primary 버튼 상태
        /// </summary>
        [SerializeField]
        protected ButtonState primaryButtonState;

        /// <summary>
        /// Secondary 방향 상태
        /// </summary>
        [SerializeField]
        protected ButtonState secondaryAxisState;
        
        /// <summary>
        /// Secondary 버튼 상태
        /// </summary>
        [SerializeField]
        protected ButtonState secondaryButtonState;

        /// <summary>
        /// 메뉴 버튼 상태
        /// </summary>
        [SerializeField]
        protected ButtonState menuButtonState;

        /// <summary>
        /// 컨트롤러 진동을 사용하는 경우 true, 그렇지 않은 경우 false
        /// </summary>
        protected bool useHaptic = true;
        
        /// <summary>
        /// 버튼 클릭 임계값
        /// </summary>
        [Header("Controller Pressed Threshold"), SerializeField]
        protected float pressedThreshold = 0.8f;
        
        /// <summary>
        /// 버튼 터치 임계값
        /// </summary>
        [Header("Controller Touched Threshold"), SerializeField]
        protected float touchedThreshold = 0.01f;

        /// <summary>
        /// 버튼 클릭 임계값
        /// </summary>
        [Header("Controller Button Click Threshold"), SerializeField]
        protected float clickThreshold = 0.5f;        
        
        #endregion

        #region Public Properties

        public PXRBaseController Controller => controller;

        #endregion
        
        #region Public Methods

        /// <summary>
        /// 버튼의 상태를 반환
        /// </summary>
        /// <param name="buttons">버튼</param>
        /// <returns>버튼의 상태</returns>
        public ButtonState GetButtonState(Buttons buttons)
        {
            return buttonStates[buttons];
        }

        /// <summary>
        /// 컨트롤러의 진동 활성화
        /// </summary>
        public void ActivateHaptic()
        {
            useHaptic = true;
        }

        /// <summary>
        /// 컨트롤러의 진동 비활성화
        /// </summary>
        public void DeactivateHaptic()
        {
            useHaptic = false;
        }        
        
        /// <summary>
        /// Axis 컨트롤러의 방향을 반환
        /// </summary>
        /// <param name="axis">Axis 컨트롤러 enum</param>
        /// <returns>Axis 컨트롤러의 방향</returns>
        public abstract Vector2 GetAxisValue(ControllerAxis axis);
        
        /// <summary>
        /// 컨트롤러 진동 실행
        /// </summary>
        /// <param name="amplitude">진동 세기</param>
        /// <param name="duration">진동 시간</param>
        public virtual void Haptic(float amplitude, float duration)
        {
            if (!useHaptic)
                return;

            var correctIntensity = amplitude * PXRInputBridge.HapticIntensity;
            controller.SendHapticImpulse(correctIntensity, duration);
        }        
        
        #endregion

        #region Private Methods

        /// <summary>
        /// 버튼 변수 등록
        /// </summary>
        protected virtual void Awake()
        {
            buttonStates.Add(Buttons.Trigger, triggerState);
            buttonStates.Add(Buttons.Grip, gripState);
            buttonStates.Add(Buttons.PrimaryAxis, primaryAxisState);
            buttonStates.Add(Buttons.Primary, primaryButtonState);
            buttonStates.Add(Buttons.SecondaryAxis, secondaryAxisState);
            buttonStates.Add(Buttons.Secondary, secondaryButtonState);
            buttonStates.Add(Buttons.Menu, menuButtonState);
        }        
        
        /// <summary>
        /// 버튼의 상태를 업데이트
        /// </summary>
        protected virtual void Update()
        {
            CheckButtonState(Buttons.Trigger, ref triggerState);
            CheckButtonState(Buttons.Grip, ref gripState);
            CheckButtonState(Buttons.PrimaryAxis, ref primaryAxisState);
            CheckButtonState(Buttons.Primary, ref primaryButtonState);
            CheckButtonState(Buttons.SecondaryAxis, ref secondaryAxisState);
            CheckButtonState(Buttons.Secondary, ref secondaryButtonState);
            CheckButtonState(Buttons.Menu, ref menuButtonState);
        }       
        
        /// <summary>
        /// Input Action을 활성화
        /// </summary>
        /// <param name="inputAction">활성화하고자하는 InputAction</param>
        protected virtual void EnableAction(InputActionReference inputAction)
        {
            if(!inputAction.action.enabled)
                inputAction.action.Enable();
        }        
        
        /// <summary>
        /// 버튼의 입력 상태를 감지
        /// </summary>
        /// <param name="button">버튼</param>
        /// <param name="buttonState">버튼의 상태</param>
        protected abstract void CheckButtonState(Buttons button, ref ButtonState buttonState);        
        
        /// <summary>
        /// 버튼의 Down, Up, Stay값 저장
        /// </summary>
        /// <param name="buttonState">버튼의 상태</param>
        /// <param name="pressed">누른 상태인 경우 true, 그렇지 않은 경우 false</param>
        protected void SetButtonState(ref ButtonState buttonState, bool pressed)
        {
            if (pressed)
            {
                if (!buttonState.isStay)
                {
                    buttonState.isDown = true;
                    buttonState.isStay = true;
                }
            }
            else
            {
                if (buttonState.isStay)
                {
                    buttonState.isUp = true;
                    buttonState.isStay = false;
                }
            }
        }

        /// <summary>
        /// 버튼의 Stay 값을 제외하고 초기화
        /// </summary>
        /// <param name="buttonState">버튼의 상태</param>
        protected void ResetButtonState(ref ButtonState buttonState)
        {
            buttonState.isDown = false;
            buttonState.isUp = false;
            buttonState.value = 0f;
        }        
        
        #endregion
    }
}