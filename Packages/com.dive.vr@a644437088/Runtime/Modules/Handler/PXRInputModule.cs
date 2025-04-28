using System.Collections.Generic;
using System.Linq;
using Dive.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Dive.VRModule
{
    /// <summary>
    /// 포인터 이벤트를 관리하는 클래스
    /// </summary>
    public class PXRInputModule : PointerInputModule
    {
        #region Private Fields

        [SerializeField]
        private LayerMask ignoreRaycastLayerMask; // 레이캐스트가 무시할 레이어

        [SerializeField]
        private PXRPointerMouse mousePointer;

        [SerializeField]
        private float scrollSpeed = 5f;
        
        private const float AngleDragThreshold = 1f; // 드래그 이벤트 발생할 때 각도 조절값

        private List<Canvas> uiCanvases = new List<Canvas>(); // UI 이벤트가 발생되는 모든 캔버스

        private PXRPointerVR leftPointerVR;
        private PXRPointerVR rightPointerVR;

        #endregion

        #region Public Properties

        /// <summary>
        /// VR 포인터를 사용하는 경우 true, PC 포인터 (마우스)를 사용하는 경우 false
        /// </summary>
        public static StaticVar<bool> UseVRPointer = new StaticVar<bool>(true);

        /// <summary>
        /// 레이캐스트 최대 거리
        /// </summary>
        public static StaticVar<float> MaxRaycastLength = new StaticVar<float>(3f);

        /// <summary>
        /// 컨트롤러의 Pointer
        /// </summary>
        public List<PXRPointerVR> VRPointers { get; private set; }

        /// <summary>
        /// 현재 포인터
        /// </summary>
        public PXRPointerBase CurrentPointer { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// 레이캐스트의 최대 거리 변경
        /// </summary>
        /// <param name="length">변경 거리</param>
        public void ChangeMaxRaycastLength(float length)
        {
            MaxRaycastLength.Value = length;
        }

        /// <summary>
        /// FindObjectType을 실행 후 추가 된 캔버스 있을 시 실행
        /// </summary>
        /// <param name="canvas">타겟 캔버스</param>
        public void AddCanvas(Canvas canvas)
        {
            if (uiCanvases.Contains(canvas))
                return;

            uiCanvases.Add(canvas);
        }

        /// <summary>
        /// 이벤트를 발생시키지 않을 캔버스 삭제
        /// </summary>
        /// <param name="canvas">타겟 캔버스</param>
        public void RemoveCanvas(Canvas canvas)
        {
            if (!uiCanvases.Contains(canvas))
                return;

            uiCanvases.Remove(canvas);
        }

        /// <summary>
        /// 포인터 이벤트 업데이트
        /// </summary>
        public override void Process()
        {
            if (UseVRPointer)
                ProcessVRPointer();
            else
                ProcessMousePointer();
        }

        /// <summary>
        /// PC 환경일 때 VR포인터(크로스헤어의 포인터) 활성화
        /// </summary>
        public void EnableVRPointer()
        {
            UseVRPointer.Value = true;

            foreach (var pointer in VRPointers)
            {
                pointer.CanProcess = true;
            }
        }

        /// <summary>
        /// PC 환경일 때 VR포인터 비활성화
        /// </summary>
        public void DisableVRPointer()
        {
            UseVRPointer.Value = false;

            foreach (var pointer in VRPointers)
            {
                pointer.CanProcess = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 변수 초기화
        /// </summary>
        protected override void Awake()
        {
            VRPointers = FindObjectsOfType<PXRPointerVR>().ToList();

            leftPointerVR = VRPointers.FirstOrDefault(pxr => pxr.HandSide == HandSide.Left);
            rightPointerVR = VRPointers.FirstOrDefault(pxr => pxr.HandSide == HandSide.Right);

            uiCanvases = FindObjectsOfType<Canvas>().ToList();

            ignoreRaycastLayerMask = ~ignoreRaycastLayerMask;
        }

        /// <summary>
        /// PC의 마우스 이벤트(실제 마우스 포인터 감지)
        /// </summary>
        private void ProcessMousePointer()
        {
            CurrentPointer = mousePointer;

            mousePointer.Process(out bool isValid);

            if (!isValid)
                return;

            SendUpdateEventToSelectedObject();
            eventSystem.RaycastAll(mousePointer.PointerEventData, m_RaycastResultCache);

            var graphicResult = FindFirstRaycast(m_RaycastResultCache);

            mousePointer.PointerEventData.pointerCurrentRaycast = graphicResult;

            m_RaycastResultCache.Clear();
            mousePointer.CurrentObjectOnPointer = mousePointer.PointerEventData.pointerCurrentRaycast.gameObject;

            var delta = Mouse.current.position.ReadValue() - mousePointer.PointerEventData.position;
            mousePointer.PointerEventData.delta = delta;

            HandlePointerExitAndEnter(mousePointer.PointerEventData, mousePointer.CurrentObjectOnPointer);

            // 트리거 버튼의 상태에 따라 구분
            var buttonState = PXRInputBridge.GetXRController(HandSide.Right).GetButtonState(Buttons.LeftMouse);

            // Down
            if (buttonState.isDown)
            {
                ProcessPress(mousePointer, HandSide.Right);
            }
            // Stay
            else if (buttonState.isStay)
            {
                ProcessDrag(mousePointer.PointerEventData, HandSide.Right);
            }
            // Up
            else if (buttonState.isUp)
            {
                ProcessRelease(mousePointer, HandSide.Right);
            }

            ProcessScroll(mousePointer.PointerEventData);
        }

        /// <summary>
        /// VR 포인터 이벤트
        /// </summary>
        private void ProcessVRPointer()
        {
            // 양손 사용할 경우 각각 포인터 이벤트 발생시킴
            for (int i = 0; i < VRPointers.Count; i++)
            {
                var vrPointer = VRPointers[i];
                if (!vrPointer || !vrPointer.isActiveAndEnabled)
                    continue;
                // 캔버스에 포인터의 이벤트 카메라 추가
                foreach (var canvas in uiCanvases)
                {
                    if (!canvas)
                        continue;

                    canvas.worldCamera = vrPointer.EventCamera;
                }

                CurrentPointer = vrPointer;

                vrPointer.Process(out bool isValid);

                if (!isValid)
                    continue;

                //m_RaycastResultCache.Clear();

                var isGraphic = false;
                
                SendUpdateEventToSelectedObject();
                eventSystem.RaycastAll(vrPointer.PointerEventData, m_RaycastResultCache);

                RaycastResult graphicResult = FindFirstRaycast(m_RaycastResultCache);
                RaycastResult physicsResult = FindFirstPhysicsRaycast(vrPointer);

                vrPointer.PointerEventData.pointerCurrentRaycast = GetEmptyRaycastResult();

                // UI만 닿은 경우
                if (graphicResult.gameObject != null && physicsResult.gameObject == null)
                {
                    if (graphicResult.distance < MaxRaycastLength)
                    {
                        isGraphic = true;
                        vrPointer.PointerEventData.pointerCurrentRaycast = graphicResult;
                    }
                }
                // 3D 오브젝트만 닿은 경우
                else if (graphicResult.gameObject == null && physicsResult.gameObject != null)
                {
                    if (physicsResult.distance < MaxRaycastLength)
                        vrPointer.PointerEventData.pointerCurrentRaycast = physicsResult;
                }
                // UI, 3D 오브젝트 둘 다 닿은 경우
                else if (graphicResult.gameObject != null && physicsResult.gameObject != null)
                {
                    RaycastResult result = graphicResult.distance < physicsResult.distance ? graphicResult : physicsResult;

                    isGraphic = graphicResult.distance < physicsResult.distance;
                    
                    if (result.distance < MaxRaycastLength)
                        vrPointer.PointerEventData.pointerCurrentRaycast = result;
                }
                else
                {
                }


                m_RaycastResultCache.Clear();
                vrPointer.CurrentObjectOnPointer = vrPointer.PointerEventData.pointerCurrentRaycast.gameObject;

                var screenPosition = (Vector2)vrPointer.EventCamera.WorldToScreenPoint(vrPointer.PointerEventData.pointerCurrentRaycast.worldPosition);
                var delta = screenPosition - vrPointer.PointerEventData.position;
                vrPointer.PointerEventData.position = screenPosition;
                vrPointer.PointerEventData.delta = delta;

                HandlePointerExitAndEnter(vrPointer.PointerEventData, vrPointer.CurrentObjectOnPointer);

                // 트리거 버튼의 상태에 따라 구분

                VRModule.ButtonState buttonState;

                if (PXRRig.IsVRPlay)
                    buttonState = PXRInputBridge.GetXRController(vrPointer.HandSide).GetButtonState(Buttons.Trigger);
                else
                {
                    buttonState = i == 0
                        ? PXRInputBridge.GetXRController(leftPointerVR.HandSide).GetButtonState(Buttons.LeftMouse)
                        : PXRInputBridge.GetXRController(rightPointerVR.HandSide).GetButtonState(Buttons.RightMouse);
                    // buttonState = PXRInputBridge.GetXRController(vrPointer.HandSide).GetButtonState(Buttons.LeftMouse);
                }

                // Down
                if (buttonState.isDown)
                {
                    ProcessPress(vrPointer, vrPointer.HandSide);
                }
                // Stay
                else if (buttonState.isStay)
                {
                    ProcessDrag(vrPointer.PointerEventData, vrPointer.HandSide);
                }
                // Up
                else if (buttonState.isUp)
                {
                    ProcessRelease(vrPointer, vrPointer.HandSide);
                }

                if (PXRRig.IsVRPlay && isGraphic)
                {
                    #if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM
                    
                    var primaryState = PXRInputBridge.GetXRController(vrPointer.HandSide).GetButtonState(Buttons.PrimaryAxis);
                    
                    if (primaryState.isTouch)
                    {
                        var scrollDelta = PXRInputBridge.GetXRController(vrPointer.HandSide).GetAxisValue(ControllerAxis.Primary);
                        
                        if (scrollDelta.magnitude > 0.01f)
                        {
                            if (vrPointer.PointerEventData != null)
                                vrPointer.PointerEventData.scrollDelta = scrollDelta * scrollSpeed;
                        }
                    }                    
                    
                    #elif DIVE_PLATFORM_PICO
                    
                    var scrollDelta = PXRInputBridge.GetXRController(vrPointer.HandSide).GetAxisValue(ControllerAxis.Primary);
                        
                    if (scrollDelta.magnitude > 0.01f)
                    {
                        if (vrPointer.PointerEventData != null)
                            vrPointer.PointerEventData.scrollDelta = scrollDelta * scrollSpeed;
                    }
                    
                    #endif
                }
                
                ProcessScroll(vrPointer.PointerEventData);
            }

            // 다른 포인터를 사용하기 위해 캔버스의 이벤트 카메라 초기화
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var canvas in uiCanvases)
            {
                if (!canvas)
                    continue;

                canvas.worldCamera = null;
            }
        }

        /// <summary>
        /// 빈 RaycastResult 반환
        /// </summary>
        /// <returns>감지한 오브젝트가 없는 RaycastResult</returns>
        private RaycastResult GetEmptyRaycastResult()
        {
            return new RaycastResult
            {
                gameObject = null,
            };
        }


        /// <summary>
        /// 컬라이더를 감지하는 물리 레이캐스트에 닿는 첫번째 오브젝트 반환
        /// </summary>
        /// <param name="pointer">포인터</param>
        /// <returns>물리 레이캐스트에 닿은 첫번째 오브젝트</returns>
        private RaycastResult FindFirstPhysicsRaycast(PXRPointerVR pointer)
        {
            var pointerTransform = pointer.transform;
            var ray = new Ray(pointerTransform.position, pointerTransform.forward);
            var dist = pointer.EventCamera.farClipPlane - pointer.EventCamera.nearClipPlane;

            RaycastResult result;

            if (Physics.Raycast(ray, out var hit, dist, ignoreRaycastLayerMask))
            {
                result = new RaycastResult
                {
                    gameObject = hit.collider.gameObject,
                    distance = hit.distance,
                    index = 0,
                    worldPosition = hit.point,
                    worldNormal = hit.normal,
                };
            }
            else
            {
                result = GetEmptyRaycastResult();
            }

            return result;
        }

        /// <summary>
        /// Scroll 이벤트
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        private static void ProcessScroll(PointerEventData eventData)
        {
            if (!Mathf.Approximately(eventData.scrollDelta.sqrMagnitude, 0.0f))
            {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, eventData, ExecuteEvents.scrollHandler);
            }
        }

        /// <summary>
        /// 현재 선택된 오브젝트를 업데이트
        /// </summary>
        /// <returns>선택된 오브젝트가 변경된 경우 true, 그렇지 않은 경우 false</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        protected bool SendUpdateEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        /// <summary>
        /// Down 이벤트
        /// </summary>
        /// <param name="pointer">포인터</param>
        private void ProcessPress(PXRPointerBase pointer, HandSide handSide)
        {
            var eventData = pointer.PointerEventData;
            eventData.eligibleForClick = true;
            eventData.delta = Vector2.zero;
            eventData.dragging = false;
            eventData.useDragThreshold = true;
            eventData.pressPosition = eventData.position;
            eventData.pointerPressRaycast = eventData.pointerCurrentRaycast;
            eventData.rawPointerPress = pointer.gameObject;

            DeselectIfSelectionChanged(pointer.CurrentObjectOnPointer, eventData);

            var pressed = ExecuteEvents.ExecuteHierarchy(pointer.CurrentObjectOnPointer, eventData, ExecuteEvents.pointerDownHandler);

            if (pressed == null)
            {
                pressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointer.CurrentObjectOnPointer);
            }

            if (pressed != null && PXRRig.IsVRPlay)
            {
                PXRInputBridge.GetXRController(((PXRPointerVR)pointer).HandSide).Haptic(0.2f, 0.1f);
            }

            var time = Time.unscaledTime;

            if (pressed == eventData.lastPress)
            {
                var diffTime = time - eventData.clickTime;
                if (diffTime < 0.3f)
                    ++eventData.clickCount;
                else
                    eventData.clickCount = 1;

                eventData.clickTime = time;
            }
            else
            {
                eventData.clickCount = 1;
            }

            eventData.pointerPress = pressed;
            eventData.rawPointerPress = pointer.CurrentObjectOnPointer;
            eventData.clickTime = Time.unscaledTime;

            //eventData.pointerDrag = ExecuteEvents.GetEventHandler<IPointerDownHandler>(pointer.CurrentObjectOnPointer);

            eventData.button = PointerEventData.InputButton.Left;
            eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(pointer.CurrentObjectOnPointer);

            if (eventData.pointerDrag != null)
            {
                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.initializePotentialDrag);
            }
        }

        /// <summary>
        /// Drag 이벤트
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        protected void ProcessDrag(PointerEventData eventData, HandSide handSide)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            if (!eventData.dragging)
            {
                var startDrag = ShouldStartDrag(eventData);

                if (startDrag)
                {
                    ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.beginDragHandler);
                    eventData.dragging = true;
                }
                else
                {
                    return;
                }
            }

            if (eventData.dragging)
            {
                if (eventData.pointerPress != eventData.pointerDrag)
                {
                    ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

                    eventData.eligibleForClick = false;
                    eventData.pointerPress = null;
                    eventData.rawPointerPress = null;
                }

                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.dragHandler);
            }
        }

        /// <summary>
        /// Drag 이벤트를 발생 시킬 수 있는지 체크
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        /// <returns>Drag 이벤트를 실행할 수 있는 경우 true, 그렇지 않은 경우 false</returns>
        private bool ShouldStartDrag(PointerEventData eventData)
        {
            if (!eventData.useDragThreshold)
                return true;

            var cam = eventData.pressEventCamera ? eventData.pressEventCamera : CurrentPointer.EventCamera;
            var cameraPos = cam.transform.position;
            var pressDir = (eventData.pointerPressRaycast.worldPosition - cameraPos).normalized;
            var currentDir = (eventData.pointerCurrentRaycast.worldPosition - cameraPos).normalized;
            return Vector3.Dot(pressDir, currentDir) < Mathf.Cos(Mathf.Deg2Rad * AngleDragThreshold);
        }


        /// <summary>
        /// Up 이벤트 실행
        /// </summary>
        /// <param name="pointer">포인터</param>
        private void ProcessRelease(PXRPointerBase pointer, HandSide handSide)
        {
            var eventData = pointer.PointerEventData;
            eventData.rawPointerPress = pointer.gameObject;

            ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerUpHandler);

            var handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointer.CurrentObjectOnPointer);

            if (eventData.pointerPress == handler && eventData.eligibleForClick)
            {
                ExecuteEvents.Execute(eventData.pointerPress, eventData, ExecuteEvents.pointerClickHandler);
            }
            else if (eventData.pointerDrag != null && eventData.dragging)
            {
                ExecuteEvents.ExecuteHierarchy(pointer.CurrentObjectOnPointer, eventData, ExecuteEvents.dropHandler);
            }

            eventData.eligibleForClick = false;
            eventData.pointerPress = null;
            eventData.rawPointerPress = null;

            if (eventData.pointerDrag != null && eventData.dragging)
            {
                // 스크롤뷰 인식 개선 ()
                GameObject obj = ExecuteEvents.GetEventHandler<IDragHandler>(pointer.CurrentObjectOnPointer);
                if (eventData.pointerDrag == obj)
                {
                    ExecuteEvents.Execute(pointer.CurrentObjectOnPointer, eventData, ExecuteEvents.pointerClickHandler);
                }

                ExecuteEvents.Execute(eventData.pointerDrag, eventData, ExecuteEvents.endDragHandler);
            }

            eventData.dragging = false;
            eventData.pointerDrag = null;
            //eventData.pressPosition = Vector2.zero;

            if (pointer.CurrentObjectOnPointer != eventData.pointerEnter)
            {
                HandlePointerExitAndEnter(eventData, null);
                HandlePointerExitAndEnter(eventData, pointer.CurrentObjectOnPointer);
            }
        }

        #endregion
    }
}