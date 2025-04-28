using System;
using System.Collections;
using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 그랩 이벤트
    /// </summary>
    public delegate void GrabEvent(PXRGrabber grabber, PXRGrabbable grabbable);

    /// <summary>
    /// 릴리즈 이벤트
    /// </summary>
    public delegate void ReleaseEvent(PXRGrabber grabber, PXRGrabbable grabbable, HandSide releaseHandSide);
    
    
    
    public partial class PXRGrabber
    {
        #region Public Fields

        /// <summary>
        /// 오브젝트를 잡는 경우 실행되는 이벤트
        /// </summary>
        public event GrabEvent GrabbedEvent;

        /// <summary>
        /// 오브젝트를 놓은 경우 실행되는 이벤트
        /// </summary>
        public event ReleaseEvent ReleasedEvent;

        /// <summary>
        /// 오브젝트를 강제로 잡은 경우 실행되는 이벤트
        /// </summary>
        public event Action AfterForceGrabEvent;
        
        /// <summary>
        /// 오브젝트를 강제로 놓은 경우 실행되는 이벤트
        /// </summary>
        public event Action AfterForceReleaseEvent;

        #endregion

        #region Public Methods

        /// <summary>
        /// 특정 Grabbable를 강제로 잡는 경우
        /// 잡고있는 Grabbable이 있는 경우 강제로 내려놓고 잡음
        /// </summary>
        /// <param name="grabbable">잡으려는 Grabbable</param>
        /// <param name="isAttached">손에 붙는 플래그</param>
        public void ForceGrab(PXRGrabbable grabbable,bool isAttached)
        {
            if (IsGrabbing)
            {
                if(!GrabbedGrabbable)
                    InitializeGrabber();
                else
                    ForceRelease();
            }
            
            GrabbedGrabbable = null;
            
            if (playerController.GetHandState(HandSide) != HandInteractState.None)
                return;
            
            CompletedTryGrab(grabbable, isAttached);
            
            AfterForceGrabEvent?.Invoke();
        }
        
        #endregion
        
        #region Private Methods

        /// <summary>
        /// 그랩이 실행되기 전 그랩을 할 수 있는지 판단
        /// </summary>
        protected virtual void TryGrab()
        {
            // 잡고 있는 오브젝트가 강제로 삭제된 상황에 그랩 기능 초기화
            if (IsGrabbing && !GrabbedGrabbable)
            {
                InitializeGrabber();
                return;
            }

            GrabbedGrabbable = null;

            // 이미 그랩중이거나 텔레포트중일 땐 리턴
            if (playerController.GetHandState(HandSide) != HandInteractState.None)
                return;

            var grabbable = FindNearestGrabbable(transform, attachedGrabbableList);
            var isAttached = true;

            // 포인터에 닿는것 찾기
            if (!grabbable)
            {
                grabbable = pointer.CurrentObjectOnPointer.GetGrabbable();

                if (grabbable && !grabbable.IsEnableDistanceGrab)
                    return;

                isAttached = false;
            }
            else
            {
                if (!grabbable.IsEnableColliderGrab)
                    return;
            }

            if (!grabbable || !grabbable.enabled || grabbable.TransferState == TransferState.None)
                return;

            CompletedTryGrab(grabbable, isAttached);
        }


        
        /// <summary>
        /// 그랩이 될 오브젝트 바꿔치기 여부 판단
        /// </summary>
        /// <param name="grabbable">바꿔칠 grabbable</param>
        protected void BeforeGrab(ref PXRGrabbable grabbable)
        {
            grabbable.OnBeforeGrab(ref grabbable);

            if (!grabbable)
                return;

            IsGrabbing = true;

            targetParent = grabbable.ParentOverride;

            GrabbedGrabbable = grabbable;
            GrabbedGrabbable.transform.parent = transform;
            GrabbedGrabbable.OnPointerExit(pointer.PointerEventData);

            PXRInputBridge.GetXRController(HandSide).Haptic(0.2f, 0.1f);
        }

        /// <summary>
        /// 현재 그랩상태인지 판단하고 그랩 실행
        /// </summary>
        /// <param name="grabbable"></param>
        /// <param name="isAttached"></param>
        private void CompletedTryGrab(PXRGrabbable grabbable, bool isAttached)
        {
            // 반대쪽 손에 있는 오브젝트 가져올 경우
            if (grabbable.IsGrabbed)
            {
                if (grabbable.TransferState == TransferState.One)
                    return;

                if (grabbable.Grabber == otherGrabber)
                {
                    grabbable.ChangeGrabbedHand(this);
                    otherGrabber.ForceRelease();
                }
                else
                    return;
            }

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

            rotationPivot.transform.rotation = Quaternion.identity;

            BeforeGrab(ref grabbable);

            if (!grabbable)
                return;

            if (isAttached)
                GrabInHandCollider();
            else
                GrabOnPointer();
            
            AfterGrab();
        }

        /// <summary>
        /// 그랩이 완료된 후 실행
        /// </summary>
        protected virtual void AfterGrab()
        {
            pointer.CanProcess = false;

            if (GrabbedGrabbable.UseFakeHand)
            {
                handModel.SetActive(false);
            }
            
            GrabbedGrabbable.Grabbed(this);
            GrabbedEvent?.Invoke(this, GrabbedGrabbable);

            // GrabbedEvent에서 ForceRelease가 실행되는 경우
            if (GrabbedGrabbable == null)
                return;
            
            // 손으로 바로 오는지 판단
            if (GrabbedGrabbable.IsAttachGrabbable)
            {
                GrabbedGrabbable.ResetVelocity();
                targetDistance = 0f;
            }
        }

        /// <summary>
        /// Grabbed 이벤트 실행
        /// </summary>
        protected void OnGrabbed()
        {
            GrabbedEvent?.Invoke(this, GrabbedGrabbable);
        }

        /// <summary>
        /// 릴리즈가 실행되기 전 실행
        /// </summary>
        protected void TryRelease()
        {
            if (!GrabbedGrabbable)
                return;

            Release();
        }

        /// <summary>
        /// 강제로 릴리즈 실행
        /// </summary>
        public void ForceRelease()
        {
            if (!GrabbedGrabbable)
                return;

            Release();

            AfterForceReleaseEvent?.Invoke();
        }

        /// <summary>
        /// 릴리즈 실행
        /// </summary>
        protected virtual void Release()
        {
            GrabbedGrabbable.transform.parent = targetParent;

            if (GrabbedGrabbable.UseFakeHand)
            {
                handModel.SetActive(true);
            }
            
            GrabbedGrabbable.Released(this);
            ReleasedEvent?.Invoke(this, GrabbedGrabbable, HandSide);

            PXRInputBridge.GetXRController(HandSide).Haptic(0.5f, 0.1f);

            InitializeGrabber();
        }


        /// <summary>
        /// PointerExit 대신 사용되는 코루틴
        /// FixedUpdate에서 현재 컬라이더에 닿지 않는 Grabbable을 삭제
        /// </summary>
        /// <returns></returns>
        private IEnumerator CoroutineCheckAttach()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();

                var colliders = Physics.OverlapSphere(transform.position, colliderRadius, interactableLayerMask);

                attachedGrabbableList.Clear();

                if (colliders.Length == 0 || IsGrabbing)
                {
                    if (GrabState == GrabState.None)
                    {
                        if (playerController.GetHandState(HandSide) == HandInteractState.None)
                        {
                            pointer.CanProcess = true;
                        }
                    }

                    if (currentAttachedGrabbable != null)
                    {
                        currentAttachedGrabbable.ExecuteOnPointerExit();
                        currentAttachedGrabbable.ExecuteOnGrabberExit();

                        currentAttachedGrabbable = null;
                    }

                    if (prevNearestGrabbable == null) 
                        yield break;
                    
                    prevNearestGrabbable.ExecuteOnPointerExit();
                    prevNearestGrabbable.ExecuteOnGrabberExit();

                    prevNearestGrabbable = null;

                    yield break;
                }

                foreach (var item in colliders)
                {
                    var grabbable = item.GetComponent<IGrabbable>();

                    if (grabbable != null)
                        attachedGrabbableList.Add(grabbable);
                }

                pointer.ForcePointerExitInGrabbable();

                currentAttachedGrabbable = FindNearestGrabbable(transform, attachedGrabbableList);

                if (!currentAttachedGrabbable || currentAttachedGrabbable.IsGrabbed) 
                    continue;
                
                if (prevNearestGrabbable == null)
                {
                    currentAttachedGrabbable.ExecuteOnPointerEnter();
                    currentAttachedGrabbable.ExecuteOnGrabberEnter();

                    prevNearestGrabbable = currentAttachedGrabbable;
                }
                else if (prevNearestGrabbable != currentAttachedGrabbable)
                {
                    prevNearestGrabbable.ExecuteOnPointerExit();
                    prevNearestGrabbable.ExecuteOnGrabberExit();

                    currentAttachedGrabbable.ExecuteOnPointerEnter();
                    currentAttachedGrabbable.ExecuteOnGrabberEnter();

                    prevNearestGrabbable = currentAttachedGrabbable;
                }
            }
        }

        #endregion
    }
}