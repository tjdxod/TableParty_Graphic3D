using System;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Dive.VRModule
{
    public class PXRSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Public Fields

        public event Action<float> ValueChangedEvent;

        #endregion

        #region Private Fields

        [SerializeField]
        private Transform sliderBox;

        [SerializeField]
        private Transform leftPos;

        [SerializeField]
        private Transform rightPos;

        private PVRSliderPointer sliderPointer;
        private PVRSliderGrabbable sliderGrabbable;

        private bool isMoving;

        #endregion

        #region Public Properties

        public bool IsMoving
        {
            get => isMoving;
            set
            {
                isMoving = value;

                if (!sliderGrabbable.Grabbable)
                    return;

                sliderGrabbable.Grabbable.TransferState = value ? TransferState.None : TransferState.Both;
            }
        }

        public PVRSliderGrabbable SliderGrabbable => sliderGrabbable;

        public PVRSliderPointer SliderPointer => sliderPointer;

        #endregion

        #region Public Methods

        public void OnPointerEnter(PointerEventData eventData)
        {
            ActivateOutline();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DeactivateOutline();
        }

        /// <summary>
        /// 아웃라인 활성화
        /// </summary>
        public void ActivateOutline()
        {
            if (IsMoving)
                return;

            sliderBox.GetComponent<LuxOutline>().enabled = true;
        }

        /// <summary>
        /// 아웃라인 비활성화
        /// </summary>
        public void DeactivateOutline()
        {
            if (IsMoving)
                return;

            sliderBox.GetComponent<LuxOutline>().enabled = false;
        }

        /// <summary>
        /// Slider의 값 변경 (0 ~ 1)
        /// </summary>
        /// <param name="xValue">(0 ~ 1)사이의 값</param>
        public void ChangeSliderValue(float xValue)
        {
            xValue = Mathf.Clamp(xValue, leftPos.localPosition.x, rightPos.localPosition.x);
            sliderBox.transform.localPosition = new Vector3(xValue, 0f, 0f);

            // 값을 0~1로 변경
            // ReSharper disable once Unity.InefficientPropertyAccess
            var sliderValue = xValue / rightPos.localPosition.x;
            sliderValue = (sliderValue + 1) * 0.5f;

            ValueChangedEvent?.Invoke(sliderValue);
        }


        /// <summary>
        /// 초기 값으로 되돌리기
        /// </summary>
        public void ResetSliderValue()
        {
            var xValue = Mathf.Lerp(leftPos.localPosition.x, rightPos.localPosition.x, 0.5f);
            sliderBox.transform.localPosition = new Vector3(xValue, 0f, 0f);

            // ReSharper disable once Unity.InefficientPropertyAccess
            var sliderValue = xValue / rightPos.localPosition.x;
            sliderValue = (sliderValue + 1) * 0.5f;

            ValueChangedEvent?.Invoke(sliderValue);
        }

        /// <summary>
        /// 초기 위치로 되돌리기
        /// </summary>
        public void ResetPosition()
        {
            if (sliderPointer == null || sliderGrabbable == null)
                return;
            
            var boxPosition = sliderBox.transform.position;
            // ReSharper disable once Unity.InefficientPropertyAccess
            var boxRotation = sliderBox.transform.rotation;
            
            var sliderPointerTransform = sliderPointer.transform;
            sliderPointerTransform.position = boxPosition;
            sliderPointerTransform.rotation = boxRotation;

            var sliderGrabbableTransform = sliderGrabbable.transform;
            sliderGrabbableTransform.position = boxPosition;
            sliderGrabbableTransform.rotation = boxRotation;
        }

        #endregion

        #region Private Methods

        private void Awake()
        {
            sliderPointer = GetComponentInChildren<PVRSliderPointer>();
            sliderGrabbable = GetComponentInChildren<PVRSliderGrabbable>();
        }

        private void OnEnable()
        {
            IsMoving = false;

            DeactivateOutline();
            ResetPosition();
        }

        #endregion
    }
}