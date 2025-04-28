using System;
using System.Collections;
using BNG;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public class PXRAdditionalPressableButton : PXRDirectableBase, IAdditionalPressableButton
    {
        #region Public Fields

        public event Action PressedEvent;
        public event Action ReleasedEvent;
        public event Action<float> ProgressEvent;

        #endregion

        #region Private Fields

        [SerializeField, ReadOnly, Space, LabelText("눌림 정도 (0~1)")]
        private float ratio = 1;

        [SerializeField, LabelText("모델의 Y축 오프셋")]
        private float modelYOffset = 0.025f;

        [SerializeField, LabelText("눌림상태 임계값 (0.01~0.99)")]
        private float completedPressThreshold = 0.01f;

        [SerializeField, LabelText("버튼으로 사용되는 렌더러")]
        private Transform buttonRender;

        [SerializeField, LabelText("렌더러의 Y축 최대값")]
        private Vector3 maxLocalPosition;

        [SerializeField, LabelText("렌더러의 Y축 최소값")]
        private Vector3 minLocalPosition;

        [SerializeField, LabelText("버튼 눌린상태 고정")]
        private bool useFixedPress;

        private IEnumerator routineReturnToOrigin;
        private bool isCompletedPress;
        private bool isCompletedRelease;
        private Collider detectRange;
        private HandPose targetPose;

        #endregion

        #region Public Properties

        [field: SerializeField, LabelText("감지 범위 Collider")]
        public Collider DetectCollider { get; private set; }

        [field: SerializeField, LabelText("기존 Collider")]
        public Collider DefaultCollider { get; private set; }
        
        [field: SerializeField, LabelText("변경할 Pose")]
        public PXRPose TargetPXRPose { get; private set; }

        [field: SerializeField, LabelText("변경할 Pose")]
        public HandPose TargetHandPose { get; private set; }
        
        public Transform ButtonRender => buttonRender;
        public bool UseFixedPress => useFixedPress;
        public float Ratio => ratio;
        
        #endregion

        #region Private Properties

        private Vector3 RatioPosition => Vector3.Lerp(minLocalPosition, maxLocalPosition, ratio);

        #endregion

        #region Public Methods
        
        public override void ForceRelease(bool useEvent = true)
        {
            if (routineReturnToOrigin != null)
            {
                StopCoroutine(routineReturnToOrigin);
                routineReturnToOrigin = null;
            }

            routineReturnToOrigin ??= CoroutineReturnToOrigin();
            StartCoroutine(routineReturnToOrigin);
        }

        public override void ForcePress(bool useEvent = true)
        {
            
        }        
        
        public PXRAdditionalPressableButton GetPressableButton()
        {
            return this;
        }
        
        #endregion

        #region Private Methods



        protected override void Awake()
        {
            base.Awake();
            buttonRender.gameObject.layer = PXRNameToLayer.Directable;
            
            Physics.IgnoreCollision(DetectCollider, DefaultCollider);
        }

        /// <summary>
        /// 정해진 시간마다 버튼의 위치를 갱신
        /// </summary>
        /// <example>
        /// 상속하여 사용할때는 다음과 같이 override하여 변경하여야 함.
        /// <code>
        /// protected override void FixedUpdate()
        /// {
        ///     if (!CanInteract)
        ///         return;
        ///
        ///     if (useFixedPress &amp;&amp; isCompletedPress)
        ///     {
        ///         return;
        ///     }
        /// 
        ///     if (photonView.IsMine)
        ///     {
        ///         ratio = SetRatio();
        ///     }
        ///     else
        ///     {
        ///         ratio = ratio;
        ///     }
        ///
        ///     OnPress();
        ///     OnProgress(ratio);
        /// }
        /// </code>
        /// </example>
        protected virtual void FixedUpdate()
        {
            if (!CanInteract)
                return;
            
            if (useFixedPress && isCompletedPress)
            {
                return;
            }

            ratio = SetRatio();

            OnPress();
            OnProgress(ratio);
        }

        protected float SetRatio()
        {
            if (InteractList.Count == 0)
            {
                if (isCompletedRelease)
                    return 1f;

                if (routineReturnToOrigin != null)
                    return ratio;

                routineReturnToOrigin ??= CoroutineReturnToOrigin();
                StartCoroutine(routineReturnToOrigin);

                return ratio;
            }

            if (routineReturnToOrigin != null)
            {
                StopCoroutine(routineReturnToOrigin);
                routineReturnToOrigin = null;
            }

            var count = InteractList.Count;
            isCompletedRelease = false;

            var minRatio = 1f;

            for (var i = 0; i < count; i++)
            {
                var interact = InteractList[i];
                var (head, tail) = WorldToLocalPosition(interact.GetPosition(), transform);

                head.y -= modelYOffset;
                head.y -= interact.Radius;
                tail.y -= modelYOffset;
                tail.y -= interact.Radius;

                var absTotalDistance = Mathf.Abs(maxLocalPosition.y - minLocalPosition.y);
                var absHeadDistance = Mathf.Abs(head.y - minLocalPosition.y);
                var headRatio = absHeadDistance / absTotalDistance;

                float targetRatio;

                if (head.y < minLocalPosition.y)
                {
                    var compare = Mathf.Abs(tail.y - head.y);
                    var absTailDistance = Mathf.Abs(tail.y - minLocalPosition.y) - compare;
                    var tailRatio = absTailDistance / absTotalDistance;

                    targetRatio = tailRatio;
                }
                else
                {
                    targetRatio = headRatio;
                }

                if (targetRatio > 1f)
                    targetRatio = 1;

                if (targetRatio < 0)
                    targetRatio = 0;

                if (minRatio > targetRatio)
                    minRatio = targetRatio;
            }

            return minRatio;
        }

        protected void OnPress()
        {
            if (ratio < completedPressThreshold && !isCompletedPress)
            {
                isCompletedPress = true;
                ratio = 0f;
                buttonRender.localPosition = minLocalPosition;

                PressedEvent?.Invoke();
            }
            else if (ratio >= completedPressThreshold && isCompletedPress)
            {
                isCompletedPress = false;
            }
        }

        protected void OnProgress(float r)
        {
            ratio = r;
            buttonRender.localPosition = Vector3.MoveTowards(buttonRender.localPosition, RatioPosition, Time.deltaTime * 10f);

            ProgressEvent?.Invoke(ratio);
        }
        
        private IEnumerator CoroutineReturnToOrigin()
        {
            var originPosition = buttonRender.localPosition;
            var targetPosition = maxLocalPosition;
            var distance = Vector3.Distance(originPosition, targetPosition);
            var speed = distance / 0.1f;
            var time = 0f;

            while (time < 0.1f)
            {
                var headLocalPosition = buttonRender.localPosition;
                var absTotalDistance = Mathf.Abs(maxLocalPosition.y - minLocalPosition.y);
                var absPointDistance = Mathf.Abs(headLocalPosition.y - minLocalPosition.y);
                ratio = absPointDistance / absTotalDistance;

                time += Time.deltaTime;
                headLocalPosition = Vector3.MoveTowards(headLocalPosition, targetPosition, speed * Time.deltaTime);
                buttonRender.localPosition = headLocalPosition;

                yield return null;
            }

            isCompletedPress = false;
            isCompletedRelease = true;

            ReleasedEvent?.Invoke();

            buttonRender.localPosition = targetPosition;
            ratio = 1f;
            routineReturnToOrigin = null;
        }

        #endregion
    }
}