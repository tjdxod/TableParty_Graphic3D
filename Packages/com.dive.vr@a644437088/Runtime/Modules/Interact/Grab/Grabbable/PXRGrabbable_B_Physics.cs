using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public partial class PXRGrabbable
    {
        #region Private Fields

        /// <summary>
        /// 오브젝트가 던져지는 힘
        /// </summary>
        [Tooltip("오브젝트가 던져지는 힘, 기본값은 1f"), Space]
        [SerializeField, ShowIf(nameof(IsEnableGrab), true), LabelText("던져지는 힘")]
        protected float throwForce = 1f;

        /// <summary>
        /// 손에 잡혔을 때 트리거 컬라이더 활성화 기능
        /// </summary>
        [Tooltip("손에 잡혔을 때 트리거 콜라이더 활성화 기능")]
        [SerializeField, ShowIf(nameof(IsEnableGrab), true), LabelText("트리거 콜라이더 활성화")]
        protected bool isActiveAttachHandCollider = false;

        /// <summary>
        /// Rigidbody 컴포넌트 Collision Detection 속성 중 Continuous값을 사용하는 경우 true, 그렇지 않은 경우 false 
        /// </summary>
        [Tooltip("Rigidbody 컴포넌트 Collision Detection 속성 중 Continuous값을 사용하는 경우 true, 그렇지 않은 경우 false")]
        [SerializeField, ShowIf(nameof(IsEnableGrab), true), LabelText("Rigidbody Continuous 기능 활성화")]
        protected bool isRigidContinuous = false;

        /// <summary>
        /// 자신과 자식들의 모든 Collider
        /// </summary>
        protected Collider[] colliderArray;
        
        /// <summary>
        /// 자신과 자식들의 Collider 중에 isTrigger의 값이 true가 아닌 Collider 리스트
        /// </summary>
        protected List<Collider> notTriggerList;

        /// <summary>
        /// Rigidbody의 Velocity와 AngularVelocity 값을 계산하는 코루틴을 저장하는 변수
        /// </summary>
        protected IEnumerator routineCalculateRigid;

        /// <summary>
        /// Release가 된 후에 Velocity가 일정 값 이하로 떨어지면 ResetVelocity를 호출하는 코루틴
        /// </summary>
        protected IEnumerator routineCalculateAfterRigid;

        /// <summary>
        /// 오브젝트를 잡았을 때 Rigidbody의 기존 useGravity값을 저장하는 변수
        /// </summary>
        protected bool wasUseGravity = false;

        /// <summary>
        /// 오브젝트를 잡았을 때 Rigidbody의 기존 isKinematic값을 저장하는 변수
        /// </summary>
        protected bool wasIsKinematic = false;

        /// <summary>
        /// Velocity 보정 변수
        /// </summary>
        protected readonly float velocityForce = 20f;

        /// <summary>
        /// 오브젝트를 던졌을 때 Velocity의 제한 값
        /// </summary>
        protected readonly float maxVelocityMagnitude = 15f;

        #endregion

        #region Public Properties

        /// <summary>
        /// 오브젝트의 Rigidbody
        /// </summary>
        public Rigidbody Rigid { get; private set; } = null;

        #endregion

        #region Private Properties

        /// <summary>
        /// 현재 Rigidbody의 Velocity
        /// </summary>
        public Vector3 CurrentVelocity { get; private set; } = Vector3.zero;

        /// <summary>
        /// 현재 Rigidbody의 AngularVelocity
        /// </summary>
        public Vector3 CurrentAngularVelocity { get; private set; } = Vector3.zero;

        private bool IsEnableGrab => IsEnableColliderGrab || IsEnableDistanceGrab;

        #endregion

        #region Public Methods

        /// <summary>
        /// 강제로 Rigidbody의 Velocity와 AngularVelocity 값 변경
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="angularVelocity"></param>
        public void ForceChangeVelocity(Vector3 velocity, Vector3 angularVelocity)
        {
            CurrentVelocity = velocity;
            CurrentAngularVelocity = angularVelocity;
        }

        /// <summary>
        /// Rigidbody의 Velocity와 AngularVelocity 값 초기화
        /// </summary>
        public void ResetVelocity()
        {
            CurrentVelocity = Vector3.zero;
            CurrentAngularVelocity = Vector3.zero;
        }

        /// <summary>
        /// 최초 생성 시 트리거가 아닌 오브젝트의 모든 트리거 활성화
        /// </summary>
        public virtual void EnableTriggerOfColliders()
        {
            foreach (var coll in notTriggerList)
            {
                coll.isTrigger = true;
            }
        }

        /// <summary>
        /// 최초 생성 시 트리거가 아닌 오브젝트의 모든 트리거 비활성화
        /// </summary>
        public virtual void DisableTriggerOfColliders()
        {
            foreach (var coll in notTriggerList)
            {
                coll.isTrigger = false;
            }
        }

        /// <summary>
        /// Use Gravity, IsKinematic 덮어쓰기
        /// </summary>
        /// <param name="useGravity"></param>
        /// <param name="isKinematic"></param>
        public void ChangeWasPhysics(bool useGravity, bool isKinematic)
        {
            wasUseGravity = useGravity;
            wasIsKinematic = isKinematic;
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        /// Rigidbody의 Velocity와 AngularVelocity 값을 업데이트하는 코루틴
        /// </summary>
        /// <returns></returns>
        private IEnumerator CalculateRigid()
        {
            while (isActiveAndEnabled)
            {
                CalculateVelocity();
                CalculateAngularVelocity();

                yield return null;
            }
        }

        /// <summary>
        /// Rigidbody의 Velocity와 AngularVelocity 값을 업데이트하는 코루틴
        /// </summary>
        /// <returns></returns>
        private IEnumerator CalculateAfterRigid()
        {
            while (isActiveAndEnabled)
            {
                if (CurrentVelocity.magnitude < 0.1f)
                {
                    ResetVelocity();
                    yield break;
                }

                yield return null;
            }
        }
        
        /// <summary>
        /// 현재 Rigidbody의 Velocity를 계산하는 함수
        /// </summary>
        protected void CalculateVelocity()
        {
            var targetPosition = transform.position;
            var currentFrameVelocity = (targetPosition - preFramePosition) / Time.deltaTime;
            CurrentVelocity = Vector3.Lerp(CurrentVelocity, currentFrameVelocity, velocityForce * Time.deltaTime);
            preFramePosition = targetPosition;
        }

        /// <summary>
        /// 현재 Rigidbody의 AngularVelocity를 계산하는 함수
        /// </summary>
        protected void CalculateAngularVelocity()
        {
            var targetRotation = transform.rotation;
            var deltaRotation = targetRotation * Quaternion.Inverse(preFrameRotation);
            preFrameRotation = targetRotation;
            
            deltaRotation.ToAngleAxis(out var angle, out var axis);

            angle *= Mathf.Deg2Rad;
            var angularVelocity = (1.0f / Time.deltaTime) * angle * axis;

            CurrentAngularVelocity = angularVelocity;
        }

        #endregion
    }
}