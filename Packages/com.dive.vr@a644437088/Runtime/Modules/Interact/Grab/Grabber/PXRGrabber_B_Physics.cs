using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public partial class PXRGrabber
    {
        #region Private Fields

        /// <summary>
        /// 포인터 클래스
        /// </summary>
        [Tooltip("해당 손의 PVR 포인터 클래스를 할당")]
        [SerializeField]
        protected PXRPointerVR pointer;

        /// <summary>
        /// 상호작용 레이어
        /// </summary>
        [Tooltip("상호작용할 레이어를 적용")]
        [SerializeField]
        private LayerMask interactableLayerMask;

        [SerializeField]
        private BoxCollider additionalCollider;

        [Tooltip("포인터로 잡을 수 있는 경우 나타내는 마크를 나타낼지")]
        [SerializeField]
        private bool isPointerGrabDetect;

        [ShowIf(nameof(isPointerGrabDetect), true)]
        [Tooltip("포인터로 잡을 수 있는 경우 나타내는 마크")]
        [SerializeField]
        private GameObject objPointerGrabDetect;

        [ShowIf(nameof(isPointerGrabDetect), true)]
        [Tooltip("Hand Anchor 트랜스폼")]
        [SerializeField]
        private Transform handAnchor;

        [ShowIf(nameof(isPointerGrabDetect), true)]
        [Tooltip("이미지 위치 조절")]
        [SerializeField]
        private Vector3 grabDetectOffset = Vector3.up * 0.1f;

        private readonly List<IGrabbable> attachedGrabbableList = new List<IGrabbable>();

        private IEnumerator routineMoveGrabbable;
        private IEnumerator routineCheckAttach;
        private PXRGrabbable prevNearestGrabbable;
        private PXRGrabbable currentAttachedGrabbable;
        private PXRGrabbableRing grabbableRing;

        /// <summary>
        /// 그랩 된 오브젝트와의 거리
        /// </summary>
        protected float targetDistance;

        // 그랩 된 오브젝트의 최대 이동 거리 (최대 레이캐스트 거리와 같음)
        private float maxMoveDistance;

        // 회전이 시작되는 조이스틱의 x값
        private const float RotateThreshold = 0.2f;

        // 들어와서 그랩 되는 거리
        private const float HandGrabbedDistance = 0.05f;

        // 컬라이더 그랩을 위한 범위값
        private float colliderRadius;

        private bool isAttachingGrabbable;

        #endregion

        #region Public Properties

        /// <summary>
        /// 잡고 있는 grabbable
        /// </summary>
        public PXRGrabbable GrabbedGrabbable { get; protected set; }

        /// <summary>
        /// 물건을 잡고 있는지
        /// </summary>
        public bool IsGrabbing
        {
            get => isGrabbing;
            private set
            {
                isGrabbing = value;
                playerController.ChangeHandState(HandSide, value ? HandInteractState.Grabbing : HandInteractState.None);
            }
        }

        #endregion

        #region Private Properties

        private bool IsAttachingGrabbable
        {
            get => isAttachingGrabbable;
            set
            {
                isAttachingGrabbable = value;

                if (value)
                {
                    grabbableRing.DeactivateRing();
                }
                else
                {
                    if (IsGrabbing && GrabbedGrabbable.CanRotating)
                    {
                        grabbableRing.ActivateRing();
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public void ShowPointerGrabMark()
        {
            if (!isPointerGrabDetect)
                return;

            if (objPointerGrabDetect == null)
                return;

            if (!objPointerGrabDetect.activeInHierarchy)
                objPointerGrabDetect.SetActive(true);

            if (handAnchor != null)
                objPointerGrabDetect.transform.position = handAnchor.position + grabDetectOffset;
        }

        public void HidePointerGrabMark()
        {
            if (!isPointerGrabDetect)
                return;

            if (objPointerGrabDetect == null)
                return;

            if (objPointerGrabDetect.activeInHierarchy)
                objPointerGrabDetect.SetActive(false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 리스트의 Grabbable 중 target(손)과 가장 가까운 Grabbable을 찾음
        /// </summary>
        /// <param name="target">타겟 Transform</param>
        /// <param name="list">잡을수 있는 grabbable의 리스트</param>
        /// <param name="isCheckCanInteract">잡을 수 있는 상태인지 판단</param>
        /// <returns></returns>
        private PXRGrabbable FindNearestGrabbable(Transform target, List<IGrabbable> list)
        {
            if (list.Count == 0)
                return null;

            var exceptNone = list.Count > 1;
            
            var minDist = 1000f;
            PXRGrabbable nearest = null;
            PXRGrabbable enableGrabbable = null;

            foreach (var iGrabbable in list)
            {
                if (iGrabbable == null)
                    continue;

                var grabbable = iGrabbable.GetGrabbable();

                if (exceptNone && grabbable.TransferState == TransferState.None || (grabbable.TransferState == TransferState.One && grabbable.IsGrabbed))
                    continue;

                var dist = Vector3.SqrMagnitude(grabbable.transform.position - target.position);
                if (!(dist < minDist))
                    continue;

                nearest = grabbable;
                minDist = dist;
            }

            return nearest;
        }

        private IAdditionalPressableButton FindNearestPressableButton(Transform target, bool isCheckCanInteract)
        {
            var targetTransform = additionalCollider.transform;

            var directableLayerMask = 1 << PXRNameToLayer.Directable;
            var results = new Collider[10];

            var worldCenter = additionalCollider.transform.TransformPoint(additionalCollider.center);
            var worldHalfExtents = Vector3.Scale(additionalCollider.size, targetTransform.lossyScale) * 0.5f;
            var size = Physics.OverlapBoxNonAlloc(worldCenter, worldHalfExtents, results, targetTransform.rotation, directableLayerMask);

            if (size == 0)
                return null;

            IAdditionalPressableButton nearest = null;
            var minDist = 1000f;

            for (var i = 0; i < size; i++)
            {
                var buttonChild = results[i].GetComponent<PXRPressableButtonChild>();

                if (buttonChild == null)
                    continue;

                var additionalPressableButton = buttonChild.GetAdditionalPressableButton();

                if (additionalPressableButton == null)
                    continue;

                var button = additionalPressableButton.GetPressableButton();


                if (isCheckCanInteract && !button.CanInteract)
                    continue;

                var dist = Vector3.SqrMagnitude(button.ButtonRender.position - target.position);
                if (!(dist < minDist))
                    continue;

                nearest = additionalPressableButton;
                minDist = dist;
            }

            return nearest;
        }

        /// <summary>
        /// 손에 닿고 있는 오브젝트 그랩
        /// </summary>
        private void GrabInHandCollider()
        {
            GrabState = GrabState.Collider;

            HidePointerGrabMark();

            targetDistance = 0f;
            IsAttachingGrabbable = true;

            if (GrabbedGrabbable.AttachGrabbableState == AttachGrabbableState.AttachedWithPose)
            {
                MoveGrabbableToHand();
            }
            else
            {
                ChangeHandPose();
            }
        }

        /// <summary>
        /// 포인터에 닿은 오브젝트 그랩
        /// </summary>
        protected void GrabOnPointer()
        {
            GrabState = GrabState.Distance;

            HidePointerGrabMark();

            var grabbedPosition = GrabbedGrabbable.transform.position;
            var targetTransform = directionPivot.transform;

            targetDistance = Vector3.Distance(transform.position, grabbedPosition);

            targetTransform.position = grabbedPosition;
            targetTransform.localRotation = Quaternion.identity;


            // 손에 바로 올 경우
            if (GrabbedGrabbable.IsAttachGrabbable)
            {
                targetDistance = 0f;
                grabbableRing.ResetRotation();
            }

            if (!GrabbedGrabbable.IsAttachGrabbable && GrabbedGrabbable.CanRotating)
            {
                grabbableRing.ActivateRing();
                grabbableRing.ResetRotation();
            }


            if (routineMoveGrabbable != null)
            {
                StopCoroutine(routineMoveGrabbable);
                routineMoveGrabbable = null;
            }

            routineMoveGrabbable = CoroutineMoveGrabbable();
            StartCoroutine(routineMoveGrabbable);
        }

        /// <summary>
        /// 닿은 Grabbable를 잡을 수 있는지 판단
        /// </summary>
        /// <param name="other">닿은 Grabbable 콜라이더</param>
        private void OnTriggerEnter(Collider other)
        {
            if(playerController.GetHandState(HandSide) != HandInteractState.None)
                return;
            
            CheckColliderGrab(other.gameObject);
        }

        /// <summary>
        /// 닿은 물체를 잡을 수 있는 경우 리스트에 추가
        /// </summary>
        /// <param name="target">닿은 Grabbable 오브젝트</param>
        private void CheckColliderGrab(GameObject target)
        {
            if (IsGrabbing)
                return;

            if (!interactableLayerMask.Includes(target.gameObject.layer))
                return;

            var grabbable = target.GetComponent<IGrabbable>();

            if (grabbable == null)
                return;

            if (attachedGrabbableList.Contains(grabbable))
                return;

            if (attachedGrabbableList.Count == 0)
            {
                if (routineCheckAttach != null)
                {
                    StopCoroutine(routineCheckAttach);
                    routineCheckAttach = null;
                }

                routineCheckAttach = CoroutineCheckAttach();
                StartCoroutine(routineCheckAttach);
            }

            attachedGrabbableList.Add(grabbable);

            if (pointer.CanProcess)
            {
                pointer.CanProcess = false;
            }
        }

        #endregion
    }
}