using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dive.VRModule
{
    public class PVRSliderPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        #region Private Fields

        private PXRSlider slider;

        private PXRInputModule inputModule;
        private PXRPointerBase currentPointer;
        private IEnumerator routineMoveSlider;        
        
        #endregion
        
        #region Public Methods

        public void OnPointerDown(PointerEventData eventData)
        {
            if (slider.IsMoving)
                return;

            slider.DeactivateOutline();
            slider.IsMoving = true;

            currentPointer = inputModule.CurrentPointer;

            if (routineMoveSlider != null)
            {
                StopCoroutine(routineMoveSlider);
                routineMoveSlider = null;
            }

            routineMoveSlider = CoroutineMoveSlider();
            StartCoroutine(routineMoveSlider);
        }


        public void OnPointerUp(PointerEventData eventData)
        {
            slider.IsMoving = false;

            if (routineMoveSlider != null)
            {
                StopCoroutine(routineMoveSlider);
                routineMoveSlider = null;
            }

            slider.ResetPosition();
        }        
        
        #endregion

        #region Private Methods

        private void Awake()
        {
            slider = GetComponentInParent<PXRSlider>();
            inputModule = FindObjectOfType<PXRInputModule>();
        }        
        
        private IEnumerator CoroutineMoveSlider()
        {
            while (true)
            {
                if (!isActiveAndEnabled)
                    yield break;

                var pointerPosition = currentPointer.transform.position;
                var sliderPosition = slider.transform.position;
                
                // ReSharper disable once Unity.InefficientPropertyAccess
                var ray = new Ray(pointerPosition, currentPointer.transform.forward);
                // ReSharper disable once Unity.InefficientPropertyAccess
                var plane = new Plane(slider.transform.rotation * Vector3.up, sliderPosition);

                var dist = 0f;
                var dot = Vector3.Dot(Vector3.Normalize(sliderPosition - pointerPosition), plane.normal);

                if (!Mathf.Approximately(dot, 0f) && !plane.Raycast(ray, out dist))
                    yield return null;

                var worldPos = ray.GetPoint(dist);
                var localPos = slider.transform.InverseTransformPoint(worldPos);
                localPos.y = 0f;

                slider.ChangeSliderValue(localPos.x);

                yield return null;
            }
        }        
        
        #endregion
    }
}
