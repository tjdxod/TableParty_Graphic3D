using System.Collections;
using UnityEngine;

namespace Dive.VRModule
{
    public class PVRSliderGrabbable : MonoBehaviour
    {
        #region Private Fields

        private PXRSlider slider;
        private IEnumerator routineMoveSlider;        
        
        #endregion
        
        #region Public Properties

        public PXRGrabbable Grabbable { get; private set; }        
        
        #endregion

        #region Public Methods

        public void StartMoveSlider()
        {
            if (routineMoveSlider != null)
            {
                StopCoroutine(routineMoveSlider);
                routineMoveSlider = null;
            }

            routineMoveSlider = CoroutineMoveSlider();
            StartCoroutine(routineMoveSlider);
        }


        public void StopMoveSlider()
        {
            if (routineMoveSlider == null) 
                return;
            
            StopCoroutine(routineMoveSlider);
            routineMoveSlider = null;
        }        
        
        #endregion

        #region Private Methods

        private void Awake()
        {
            slider = GetComponentInParent<PXRSlider>();
            Grabbable = GetComponent<PXRGrabbable>();
        }

        private void Start()
        {
            Grabbable.GrabbedEvent += OnGrabbed;
            Grabbable.ReleasedEvent += OnReleased;
        }        
        
        private void OnGrabbed(PXRGrabber grabber, PXRGrabbable grabbable)
        {
            if (slider.IsMoving)
                return;

            slider.IsMoving = true;

            StartMoveSlider();
        }
        
        private void OnReleased(PXRGrabber grabber, PXRGrabbable grabbable, HandSide releaseHandSide)
        {
            StopMoveSlider();

            slider.IsMoving = false;
            slider.ResetPosition();
        }        
        
        private IEnumerator CoroutineMoveSlider()
        {
            while (true)
            {
                if (!isActiveAndEnabled)
                    yield break;

                var vec = Grabbable.transform.position - slider.transform.position;

                vec = slider.transform.InverseTransformDirection(vec);
                vec.y = 0f;
                vec.z = 0f;

                var sign = Mathf.Sign(vec.x);
                var distance = Vector3.Magnitude(vec);
                var sliderValue = sign * distance;

                slider.ChangeSliderValue(sliderValue);

                yield return null;
            }
        }        
        
        #endregion
    }
}

