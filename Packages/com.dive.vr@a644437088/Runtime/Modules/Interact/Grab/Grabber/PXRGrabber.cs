using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;
using UnityEngine.Serialization;

namespace Dive.VRModule
{
    /// <summary>
    /// Grabbable를 잡을 수 있는 클래스 (컨트롤러)
    /// </summary>
    public partial class PXRGrabber : MonoBehaviour
    {
        #region Private Fields

        /// <summary>
        /// 반대쪽 Grabber
        /// </summary>
        [Tooltip("반대쪽 손의 Grabber를 할당")]
        [SerializeField]
        protected PXRGrabber otherGrabber;

        [Tooltip("손의 애니메이터를 할당")]
        [SerializeField]
        private Animator handAnimator;


        [Tooltip("손 모양을 저장하는 Poser를 할당")]
        [SerializeField]
        private HandPoser handPoser;
        

        [Tooltip("손 모양을 적용하는 CustomHand를 할당")]
        [SerializeField]
        private List<PXRPoser> pxrPosers;
        

        [Tooltip("물건을 잡을 때 손 모양을 자동으로 잡아주는 AutoPoser를 할당")]
        [SerializeField]
        private AutoPoser autoPoser;


        /// <summary>
        /// Input Handler
        /// </summary>
        protected PXRInputHandlerBase inputHandler;

        /// <summary>
        /// Player Controller
        /// </summary>
        protected PXRPlayerController playerController;

        /// <summary>
        /// 방향 피봇
        /// </summary>
        protected Transform directionPivot;

        /// <summary>
        /// 회전 피봇
        /// </summary>
        protected Transform rotationPivot;

        private GameObject handModel;
        private Transform targetParent;

        // 앞뒤 이동 속도
        private readonly float moveSpeed = 1.5f;

        // 회전 속도
        private readonly float rotateSpeed = 100f;

        private bool isGrabbing = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// 손의 방향
        /// </summary>
        [field: Tooltip("손의 방향 (왼손 or 오른손)")]
        [field: SerializeField]
        public HandSide HandSide { get; private set; }

        /// <summary>
        /// 그랩의 상태
        /// </summary>
        public GrabState GrabState { get; private set; }

        #endregion

        #region Public Methods
        
        public void SetPoser(PXRPoser poser)
        {
            pxrPosers.Add(poser);
        }

        public void DeletePoser(PXRPoser poser)
        {
            pxrPosers.Remove(poser);
        }
        
        #endregion
        
        #region Private Methods

        /// <summary>
        /// 변수 초기화
        /// </summary>
        protected virtual void Awake()
        {
            playerController = GetComponentInParent<PXRPlayerController>();
            grabbableRing = GetComponentInChildren<PXRGrabbableRing>(true);
            maxMoveDistance = PXRInputModule.MaxRaycastLength;
            inputHandler = PXRInputBridge.GetXRController(HandSide);

            if(handPoser != null)
               handModel = handPoser.gameObject;

            var sphereColl = GetComponent<SphereCollider>();
            if (sphereColl)
                colliderRadius = sphereColl.radius;

            directionPivot = CreateChildEmptyPivot("DirectionPivot");
            rotationPivot = CreateChildEmptyPivot("RotationPivot");
        }

        /// <summary>
        /// 그립 상태 감지
        /// </summary>
        protected virtual void Update()
        {
            if (PXRRig.IsHandTrackingMode)
            {
#if DIVE_HANDTRACKING
                
                // 손 추적 모드일 때
                if (RecognizeHandGesture.GetGripState(HandSide).isDown)
                {
                    TryGrab();
                }
                else if (RecognizeHandGesture.GetGripState(HandSide).isUp)
                {
                    TryRelease();
                }

#endif
            }
            else
            {
                if (inputHandler.GetButtonState(Buttons.Grip).isDown)
                {
                    TryGrab();
                }
                else if (inputHandler.GetButtonState(Buttons.Grip).isUp)
                {
                    TryRelease();
                }
            }
        }

        /// <summary>
        /// 피봇으로 사용할 빈 게임 오브젝트를 자식으로 생성
        /// </summary>
        /// <param name="objName">빈 게임 오브젝트 이름</param>
        /// <returns>생성된 빈 게임 오브젝트의 Transform</returns>
        private Transform CreateChildEmptyPivot(string objName)
        {
            var tr = new GameObject(objName).transform;
            tr.transform.parent = transform;
            tr.localPosition = Vector3.zero;
            tr.localRotation = Quaternion.identity;
            tr.gameObject.SetActive(false);

            return tr;
        }

        /// <summary>
        /// 손 모델 모양 변경 (경우에따라 AutoPose기능을 사용할지 정해진 손모양으로 할지 판단)
        /// </summary>
        private void ChangeHandPose()
        {
            if (handPoser == null || pxrPosers == null)
                return;            
            
            handAnimator.enabled = false;
            
            if (GrabbedGrabbable.GrabbedHandPose == null && GrabbedGrabbable.GrabbedPXRPose == null)
            {
                handPoser.CurrentPose = null;

                foreach (var pxrPoser in pxrPosers)
                {
                    pxrPoser.CurrentPose = null;
                }
                
                autoPoser.UpdateAutoPoseOnce();
            }
            else
            {
                handPoser.CurrentPose = GrabbedGrabbable.GrabbedHandPose;
                
                foreach (var pxrPoser in pxrPosers)
                {
                    pxrPoser.CurrentPose = GrabbedGrabbable.GrabbedPXRPose;
                }
            }                        
        }

        /// <summary>
        /// 그랩된 오브젝트를 앞뒤 이동, 좌우 회전
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoroutineMoveGrabbable()
        {
            while (IsGrabbing && GrabbedGrabbable)
            {
                var controllerAxis = inputHandler.GetAxisValue(ControllerAxis.Primary);

                // 상하(앞뒤 이동), 좌우(회전) 중 하나만 실행
                if (Mathf.Abs(controllerAxis.x) > Mathf.Abs(controllerAxis.y))
                {
                    controllerAxis.y = 0f;
                    ChangeGrabbableRotation(controllerAxis);
                }
                else
                {
                    controllerAxis.x = 0f;
                    ChangeGrabbablePosition(controllerAxis);
                }

                yield return null;
            }
        }

        /// <summary>
        /// 그랩을 할 수 있는 초기 상태로 돌리기
        /// </summary>
        protected void InitializeGrabber()
        {
            IsGrabbing = false;
            IsAttachingGrabbable = false;

            if (attachedGrabbableList.Count == 0)
                pointer.CanProcess = true;

            GrabState = GrabState.None;
            GrabbedGrabbable = null;

            if (handPoser)
            {
                handPoser.PreviousPose = null;
                handPoser.CurrentPose = null;
                handAnimator.enabled = true;
            }

            if (pxrPosers != null && pxrPosers.Count > 0)
            {
                foreach (var pxrPoser in pxrPosers)
                {
                    pxrPoser.PreviousPose = null;
                    pxrPoser.CurrentPose = null;
                }
                
                handAnimator.enabled = true;
            }
            
            grabbableRing.DeactivateRing();

            if (currentAttachedGrabbable != null)
            {
                currentAttachedGrabbable.ExecuteOnPointerExit();
                currentAttachedGrabbable.ExecuteOnGrabberExit();

                currentAttachedGrabbable = null;

                if (prevNearestGrabbable)
                {
                    prevNearestGrabbable.ExecuteOnPointerExit();
                    prevNearestGrabbable.ExecuteOnGrabberExit();

                    prevNearestGrabbable = null;
                }
            }

            if (routineCheckAttach != null)
            {
                StopCoroutine(routineCheckAttach);
                routineCheckAttach = null;
            }

            // 코루틴 실행시키기
            routineCheckAttach = CoroutineCheckAttach();
            StartCoroutine(routineCheckAttach);
        }

        /// <summary>
        /// 잡고있는 Grabbable의 위치를 변경
        /// </summary>
        /// <param name="controllerAxis">패드의 Axis</param>
        private void ChangeGrabbablePosition(Vector2 controllerAxis)
        {
            // 일정 값 이하일 땐 위치, 회전 고정            
            if (targetDistance < HandGrabbedDistance)
            {
                if (!IsAttachingGrabbable)
                {
                    MoveGrabbableToHand();
                }
            }
            // 일정 값 이상, 최대 값 이하일 때는 위치, 회전 계속 바뀜
            else
            {
                if (IsAttachingGrabbable)
                {
                    IsAttachingGrabbable = false;

                    GrabbedGrabbable.DetachToHand();
                    PXRInputBridge.GetXRController(HandSide).Haptic(0.5f, 0.1f);
                }

                // 피봇방향으로 당길 때
                var position = transform.position;
                var direction = (directionPivot.position - position).normalized;
                GrabbedGrabbable.transform.position = position + direction * targetDistance;
            }

            if (PXRRig.IsVRPlay)
            {
                if (GrabbedGrabbable.IsAttachGrabbable && (!GrabbedGrabbable.IsAttachGrabbable || !GrabbedGrabbable.CanMoving))
                    return;

                targetDistance += controllerAxis.y * moveSpeed * Time.deltaTime;
                targetDistance = Mathf.Clamp(targetDistance, 0.01f, maxMoveDistance - 0.1f);
            }
            // PC 버전은 조건 추가
            else
            {
                if (GrabbedGrabbable.IsAttachGrabbable &&
                    (!GrabbedGrabbable.IsAttachGrabbable || !GrabbedGrabbable.IsMoveGrabbable) &&
                    (!GrabbedGrabbable.IsAttachGrabbable || !GrabbedGrabbable.IsPCMoveGrabbable))
                    return;

                targetDistance += controllerAxis.y * moveSpeed * Time.deltaTime;
                targetDistance = Mathf.Clamp(targetDistance, 0.01f, maxMoveDistance - 0.1f);
            }
        }

        /// <summary>
        /// 손으로 바로 오게함
        /// </summary>
        private void MoveGrabbableToHand()
        {
            IsAttachingGrabbable = true;

            GrabbedGrabbable.AttachToHand();
            PXRInputBridge.GetXRController(HandSide).Haptic(0.5f, 0.1f);

            ChangeHandPose();
        }

        /// <summary>
        /// 컨트롤러 좌,우 입력에 따른 회전
        /// </summary>
        /// <param name="controllerAxis">패드의 Axis</param>
        private void ChangeGrabbableRotation(Vector2 controllerAxis)
        {
            if (!GrabbedGrabbable.CanRotating)
                return;

            if (IsAttachingGrabbable)
                return;

            var sign = 0f;

            if (Mathf.Abs(controllerAxis.x) > RotateThreshold)
                sign = Mathf.Sign(controllerAxis.x) * -1f;

            GrabbedGrabbable.transform.RotateAround(GrabbedGrabbable.transform.position, rotationPivot.transform.up, sign * rotateSpeed * Time.deltaTime);
        }

        #endregion
    }
}