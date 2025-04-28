using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dive.VRModule
{
    [RequireComponent(typeof(Camera))]
    public class PXRPointerVR : PXRPointerBase
    {
        #region Private Fields

        [SerializeField]
        private HandSide handSide;

        [SerializeField]
        private Transform endpointCircle;
        
        [SerializeField]
        private PXRLineRendererController lineRendererController;
        private PXRGrabber grabber;
        private readonly float lineMinWidth = 0.01f;
        private readonly float lineMaxWidth = 0.015f;
        private readonly float defaultLineLength = 3.0f;
        private readonly float endPointInterval = 0f;

        private PXRPlayerController playerController;

        #endregion

        #region Public Properties

        public HandSide HandSide => handSide;

        public new bool CanProcess
        {
            get => canProcess;
            set
            {
                canProcess = value;
                if (value)
                {
                    if (PlayerController.LineRendererState == LineRendererState.Always && LineRendererController != null)
                        LineRendererController.EnableLineRenderer();

                    endpointCircle.gameObject.SetActive(true);
                }
                else
                {
                    PointerEventData.pointerCurrentRaycast = new RaycastResult {gameObject = null};

                    if (LineRendererController != null)
                        LineRendererController.DisableLineAndClearAllPosition(transform.position);

                    endpointCircle.gameObject.SetActive(false);
                }
            }
        }
        
        public LineRenderer LineRenderer => LineRendererController.LineRenderer;
        
        #endregion

        #region Private Properties

        private PXRPlayerController PlayerController
        {
            get
            {
                if (playerController == null)
                    playerController = FindObjectOfType<PXRPlayerController>();

                return playerController;
            }
        }

        private PXRLineRendererController LineRendererController
        {
            get
            {
                if (lineRendererController == null)
                    lineRendererController = GetComponentInChildren<PXRLineRendererController>();

                return lineRendererController;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// PVRInputModule의 Process 함수에서 실행
        /// </summary>
        /// <param name="isValid">유효한 상태 체크</param>
        public override void Process(out bool isValid)
        {
            isValid = canProcess;

            if (!canProcess)
                return;

            PointerEventData.Reset();
            PointerEventData.position = new Vector2(EventCamera.pixelWidth / 2f, EventCamera.pixelHeight / 2f);
            PointerEventData.scrollDelta = Vector2.zero;
        }

        public float GetDistanceOnPointedObject()
        {
            return Vector3.Distance(transform.position, CurrentObjectOnPointer.transform.position);
        }

        public Vector3 GetPointedWorldPosition()
        {
            return PointerEventData.pointerCurrentRaycast.worldPosition;
        }

        public void ShowLineRenderer()
        {
            if (LineRendererController == null)
                return;

            LineRendererController.EnableLineRenderer();
        }

        public void HideLineRenderer()
        {
            if (LineRendererController == null)
                return;

            LineRendererController.DisableLineRenderer();
        }

        public void SetMoveGradient()
        {
            if(LineRendererController == null)
                return;
            
            LineRendererController.SetMoveGradient();
        }
        
        public void SetDefaultGradient()
        {
            if(LineRendererController == null)
                return;
            
            LineRendererController.SetDefaultGradient();
        }
        
        public void ShowEndpointCircle()
        {
            endpointCircle.gameObject.SetActive(true);
        }
        
        public void HideEndpointCircle()
        {
            endpointCircle.gameObject.SetActive(false);
        }

        /// <summary>
        /// 강제로 PointerEnter 이벤트 실행
        /// </summary>
        public void ForcePointerEnterInGrabbable()
        {
            if (!CurrentObjectOnPointer)
                return;

            var pointerGrabbable = CurrentObjectOnPointer.GetGrabbable();
            if (pointerGrabbable)
            {
                pointerGrabbable.ExecuteOnPointerEnter();
            }
        }

        /// <summary>
        /// 강제로 PointerExit 이벤트 실행
        /// </summary>
        public void ForcePointerExitInGrabbable()
        {
            if (!CurrentObjectOnPointer)
                return;

            var pointerGrabbable = CurrentObjectOnPointer.GetGrabbable();
            if (pointerGrabbable)
            {
                pointerGrabbable.OnPointerExit(PointerEventData);
            }
        }

        #endregion

        #region Private Methods

        protected override void Awake()
        {
            grabber = PlayerController.Grabbers.FirstOrDefault(g => g.HandSide == handSide);
            base.Awake();
        }

        private void OnEnable()
        {
            Application.onBeforeRender += SetLineRenderer;
        }

        private void OnDisable()
        {
            Application.onBeforeRender -= SetLineRenderer;
        }

        private void SetLineRenderer()
        {
            if (!CanProcess || !LineRendererController)
                return;

            Vector3 startPoint;
            Vector3 endPoint;
            
            if (PointerEventData.pointerCurrentRaycast.gameObject != null)
            {
                var obj = PointerEventData.pointerCurrentRaycast.gameObject;

                if (PlayerController.LineRendererState == LineRendererState.OnHover)
                {
                    var layer = obj.layer;
                    var isUI = obj.GetComponent<UIBehaviour>() != null;

                    if (layer == PXRNameToLayer.Interactable || layer == PXRNameToLayer.Directable || isUI)
                    {
                        ShowLineRenderer();
                    }
                    else
                    {
                        HideLineRenderer();
                    }
                }

                var grabbableChild = obj.GetComponent<PXRGrabbableChild>();
                var grabbable = grabbableChild != null ? grabbableChild.ParentGrabbable : obj.GetComponent<PXRGrabbable>();

                if (grabbable != null)
                {
                    if (grabbable.IsEnableDistanceGrab)
                    {
                        grabber.ShowPointerGrabMark();
                    }
                    else
                    {
                        grabber.HidePointerGrabMark();
                    }
                }
                else
                {
                    grabber.HidePointerGrabMark();
                }

                startPoint = LineRendererController.transform.position;
                
                if (PXRRig.RigMovement.CurrentState == CharacterState.Movement)
                {
                    endPoint = transform.position + transform.forward * 0.2f;
                }
                else
                {
                    endPoint = PointerEventData.pointerCurrentRaycast.worldPosition;
                    // 부딪치는 지점에서 약간의 간격을 둠
                    Vector3 interval = (startPoint - endPoint).normalized;
                    interval *= endPointInterval;
                    endPoint += interval;
                }
            }
            // 닿는 오브젝트가 없을 때
            else
            {
                if (PlayerController.LineRendererState == LineRendererState.OnHover || PlayerController.LineRendererState == LineRendererState.None)
                {
                    HideLineRenderer();
                }

                grabber.HidePointerGrabMark();
                startPoint = LineRendererController.transform.position;
                
                // ReSharper disable once Unity.InefficientPropertyAccess

                if (PXRRig.RigMovement.CurrentState == CharacterState.Movement)
                    endPoint = transform.position + transform.forward * 0.2f;
                else
                    endPoint = transform.position + transform.forward * defaultLineLength;
            }

            DrawLineRenderer(startPoint, endPoint);
        }

        private void DrawLineRenderer(Vector3 startPoint, Vector3 endPoint)
        {
            if (LineRendererController == null)
                return;

            endpointCircle.position = endPoint;

            // World to Local
            startPoint = LineRendererController.transform.InverseTransformPoint(startPoint);
            endPoint = LineRendererController.transform.InverseTransformPoint(endPoint);

            LineRendererController.SetTwoPoints(startPoint, endPoint);
        }

        private float CalculateWidth(Vector3 startPoint, Vector3 endPoint)
        {
            var maxLength = PXRInputModule.MaxRaycastLength;
            maxLength.Value = Mathf.Min(maxLength, 5f);

            var sqrMaxLength = maxLength * maxLength;
            var distance = Vector3.SqrMagnitude(endPoint - startPoint);

            var ratio = 1 - (distance / sqrMaxLength);
            ratio = Mathf.Max(ratio, 0f);

            return lineMinWidth + ((lineMaxWidth - lineMinWidth) * ratio);
        }

        #endregion
    }
}