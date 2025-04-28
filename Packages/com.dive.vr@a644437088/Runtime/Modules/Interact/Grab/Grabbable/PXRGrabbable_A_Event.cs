using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dive.VRModule
{
    public partial class PXRGrabbable
    {
        #region Public Fields

        /// <summary>
        /// 그랩한 경우 실행되는 이벤트
        /// </summary>
        public event GrabEvent GrabbedEvent;

        /// <summary>
        /// 그랩을 놓은 경우 실행되는 이벤트
        /// </summary>
        public event ReleaseEvent ReleasedEvent;

        /// <summary>
        /// 오브젝트가 원래 위치로 복귀할 때 실행되는 이벤트
        /// </summary>
        public event Action<Vector3, Quaternion> ReturnToOriginEvent;

        /// <summary>
        /// 반대쪽 손으로 그랩한 경우 실행되는 이벤트
        /// </summary>
        public event GrabEvent ChangeGrabbedHandEvent;
        
        /// <summary>
        /// 오브젝트가 손에 부착된 경우 실행되는 이벤트
        /// </summary>
        public event Action AttachToHandEvent;

        /// <summary>
        /// 오브젝트가 손에서 떨어진 경우 실행되는 이벤트
        /// </summary>
        public event Action DetachToHandEvent;

        /// <summary>
        /// 그랩을 놓은 경우 오브젝트가 손에서 떨어지기 전 실행되는 이벤트
        /// </summary>
        public event Action BeforeReleasedEvent;

        /// <summary>
        /// PC Rig 사용 시 마우스 클릭 이벤트
        /// </summary>
        public Action MouseClickEvent;

        /// <summary>
        /// 오브젝트가 그랩되기 전에 실행되는 델리게이트
        /// </summary>
        /// <typeparam name="PXRGrabbable">그랩 가능한 오브젝트</typeparam>
#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
        public delegate void BeforeGrabDelegate<PXRGrabbable>(ref PXRGrabbable item);
#pragma warning restore CS0693 // Type parameter has the same name as the type parameter from outer type

        /// <summary>
        /// 오브젝트가 그랩되기 전에 실행되는 이벤트
        /// </summary>
        public event BeforeGrabDelegate<PXRGrabbable> BeforeGrabEvent;

        #endregion

        #region Private Fields

        protected IEnumerator routineReturnToOrigin;
        protected bool notStopped;

        #endregion

        #region Public Methods

        /// <summary>
        /// 그랩 이벤트 함수 (grabber에 null을 할당하는 경우 grabHandSide를 사용)
        /// </summary>
        /// <param name="grabber">물건을 잡고있는 손 방향의 그랩 관리 클래스</param>
        /// <param name="grabHandSide">잡은 손 방향 - grabber에 null을 넣는 경우 사용</param>
        public virtual void Grabbed(PXRGrabber grabber, HandSide grabHandSide = HandSide.Left)
        {
            IsGrabbed = true;
            this.Grabber = grabber;

            currentHandSide = grabber != null ? grabber.HandSide : grabHandSide;

            if (UseFakeHand && FakeHand != null)
                FakeHand.SetActive(true);
            
            if (Rigid)
            {
                wasUseGravity = Rigid.useGravity;
                wasIsKinematic = Rigid.isKinematic;

                if (isRigidContinuous)
                {
                    Rigid.collisionDetectionMode = CollisionDetectionMode.Discrete;
                }

                Rigid.useGravity = false;
                Rigid.isKinematic = true;
            }

            if (grabber)
            {
                if (grabber.GrabState == GrabState.Distance && AttachGrabbableState == AttachGrabbableState.None)
                {
                    EnableTriggerOfColliders();
                }
                else if (grabber.GrabState == GrabState.Collider && isActiveAttachHandCollider)
                {
                    EnableTriggerOfColliders();
                }
            }
            
            GrabbedEvent?.Invoke(grabber, this);

            if(routineCalculateAfterRigid != null)
            {
                StopCoroutine(routineCalculateAfterRigid);
                routineCalculateAfterRigid = null;
            }
            
            if (routineCalculateRigid != null)
            {
                StopCoroutine(routineCalculateRigid);
                routineCalculateRigid = null;
            }

            ResetVelocity();
            routineCalculateRigid = CalculateRigid();
            StartCoroutine(routineCalculateRigid);

            StopReturnToOrigin();
        }

        /// <summary>
        /// 그랩을 놓았을 때 이벤트 함수
        /// </summary>
        /// <param name="grabber">물건을 잡고있는 손 방향의 그랩 관리 클래스</param>
        public virtual void Released(PXRGrabber grabber)
        {
            IsGrabbed = false;

            BeforeReleasedEvent?.Invoke();

            if (routineCalculateRigid != null)
                StopCoroutine(routineCalculateRigid);

            if (UseFakeHand && FakeHand != null)
                FakeHand.SetActive(false);
            
            if (Rigid)
            {
                Rigid.useGravity = wasUseGravity;
                Rigid.isKinematic = wasIsKinematic;

                if (wasIsKinematic)
                {
                    if (isRigidContinuous)
                    {
                        Rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    }

                    if (!Rigid.isKinematic)
                    {
                        Rigid.velocity = Vector3.zero;
                        Rigid.angularVelocity = Vector3.zero;
                    }
                }
                else
                {
                    if (isRigidContinuous)
                    {
                        Rigid.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    }

                    CurrentVelocity = Vector3.ClampMagnitude(CurrentVelocity, maxVelocityMagnitude);

#if DIVE_PLATFORM_STEAM
                    Rigid.velocity = CurrentVelocity * throwForce * 1.25f;
#else
                    Rigid.velocity = CurrentVelocity * throwForce;
#endif

                    Rigid.angularVelocity = CurrentAngularVelocity;
                }
            }
            
            if(routineCalculateAfterRigid != null)
            {
                StopCoroutine(routineCalculateAfterRigid);
                routineCalculateAfterRigid = null;
            }

            routineCalculateAfterRigid = CalculateAfterRigid();

            DisableTriggerOfColliders();
            
            var releaseHandSide = currentHandSide;
            ReleasedEvent?.Invoke(grabber, this, releaseHandSide);
            this.Grabber = null;
            this.currentHandSide = HandSide.Unknown;
            
            if(UseReturnToOrigin)
                StartReturnToOrigin();
        }

        /// <summary>
        /// 반대쪽 손으로 그랩한 경우 실행되는 이벤트 함수
        /// </summary>
        /// <param name="changedGrabber">변경된 Grabber</param>
        public void ChangeGrabbedHand(PXRGrabber changedGrabber)
        {
            ChangeGrabbedHandEvent?.Invoke(changedGrabber, this);
        }
        
        /// <summary>
        /// 오브젝트가 손에 부착된 경우 실행되는 이벤트 함수
        /// 손에 잡힐 때의 위치와 회전을 오른손을 기준으로 만들고, 왼손은 오른손에서 반대로 뒤집기
        /// </summary>
        public void AttachToHand()
        {
            AttachToHandEvent?.Invoke();

            if (!isActiveAttachHandCollider)
                DisableTriggerOfColliders();
            else
                EnableTriggerOfColliders();

            ChangePosAndRot();
        }

        /// <summary>
        /// 손(Grabber)에서 떨어졌을 때 실행
        /// </summary>
        public void DetachToHand()
        {
            DetachToHandEvent?.Invoke();
            EnableTriggerOfColliders();
        }

        /// <summary>
        /// 그랩되기 전에 실행되는 이벤트 함수
        /// </summary>
        /// <param name="grabbable"></param>
        public void OnBeforeGrab(ref PXRGrabbable grabbable)
        {
            BeforeGrabEvent?.Invoke(ref grabbable);
        }

        /// <summary>
        /// 컨트롤러 포인터가 오브젝트에 Enter 된 경우 실행되는 이벤트 함수
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (IsGrabbed)
                return;

            base.OnPointerEnter(eventData);
        }

        /// <summary>
        /// 컨트롤러 포인터가 오브젝트에 Exit 된 경우 실행되는 이벤트 함수
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerExit(PointerEventData eventData)
        {
            if (IsGrabbed)
                return;

            base.OnPointerExit(eventData);
        }

        #endregion

        #region Private Methods

        protected virtual void StartReturnToOrigin()
        {
            if (routineReturnToOrigin != null)
                StopCoroutine(routineReturnToOrigin);

            notStopped = false;

            routineReturnToOrigin = CoroutineReturnToOrigin();
            StartCoroutine(routineReturnToOrigin);
        }

        protected virtual void StopReturnToOrigin()
        {
            if (notStopped)
                return;

            if (routineReturnToOrigin != null)
                StopCoroutine(routineReturnToOrigin);
        }

        private IEnumerator CoroutineReturnToOrigin()
        {
            var time = 0f;
            var moveTime = 0.5f;
            var limitTime = 10f;
            var preTransferState = TransferState;

            // 움직임이 없을때까지 대기 후 시간 계산 : 10초가 넘어가는 경우 강제로 다음 단계로
            if (IsCalculateTimeFromNoMovement)
            {
                var isReturn = false;
                
                while (time < limitTime)
                {
                    if (Rigid.velocity.magnitude < 0.01f && Rigid.angularVelocity.magnitude < 0.01f)
                        break;

                    if (IsReturnToOriginY && (transform.position.y > ReturnToOriginYRange.y || transform.position.y < ReturnToOriginYRange.x))
                    {
                        isReturn = true;
                        break;
                    }

                    time += Time.deltaTime;
                    yield return null;
                }

                if (!isReturn)
                    yield return new WaitForSeconds(ReturnToOriginTime);
            }
            else
            {
                while (time < ReturnToOriginTime)
                {
                    if (IsReturnToOriginY && (transform.position.y > ReturnToOriginYRange.y || transform.position.y < ReturnToOriginYRange.x))
                        break;

                    time += Time.deltaTime;
                    yield return null;
                }
            }
            
            TransferState = TransferState.None;
            notStopped = true;
            Rigid.useGravity = false;
            Rigid.isKinematic = true;

            if (!Rigid.isKinematic)
            {
                Rigid.velocity = Vector3.zero;
                Rigid.angularVelocity = Vector3.zero;
            }
            
            foreach (var coll in colliderArray)
            {
                coll.enabled = false;
            }

            time = 0f;

            while (time < moveTime)
            {
                time += Time.deltaTime;

                transform.position = Vector3.Lerp(transform.position, originPosition, time / moveTime);
                // ReSharper disable once Unity.InefficientPropertyAccess
                transform.rotation = Quaternion.Lerp(transform.rotation, originRotation, time / moveTime);

                yield return null;
            }

            transform.position = originPosition;
            // ReSharper disable once Unity.InefficientPropertyAccess
            transform.rotation = originRotation;
            
            foreach (var coll in colliderArray)
            {
                coll.enabled = true;
            }

            Rigid.isKinematic = wasIsKinematic;
            Rigid.useGravity = wasUseGravity;
            
            TransferState = preTransferState;
            notStopped = false;
            ReturnToOriginEvent?.Invoke(originPosition, originRotation);
        }

        #endregion
    }
}