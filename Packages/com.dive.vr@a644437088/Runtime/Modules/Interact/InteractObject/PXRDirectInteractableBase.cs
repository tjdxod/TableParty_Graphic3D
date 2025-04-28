using System;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    /// <summary>
    /// 직접 상호작용 오브젝트 클래스
    /// </summary>
    public class PXRDirectInteractableBase : MonoBehaviour
    {
        #region Public Fields

        /// <summary>
        /// 누르고 있는 상태의 이벤트
        /// </summary>
        public event Action HoverEvent;

        /// <summary>
        /// 떼고 있는 상태의 이벤트
        /// </summary>
        public event Action UnHoverEvent;

        /// <summary>
        /// 눌러진 상태의 액션
        /// </summary>
        public Action PressEvent;

        /// <summary>
        /// 누르기 활성화 비활성화 제어
        /// </summary>
        public bool CanInteract = true;

        #endregion

        #region Private Fields

        /// <summary>
        /// Direct Interactor
        /// </summary>
        protected PXRDirectInteractor interactor;

        /// <summary>
        /// 현재 상호작용중인 손의 방향
        /// </summary>
        protected HandSide handSide;

        /// <summary>
        /// 누르고 있는 상태인지 판단
        /// </summary>
        protected bool isPressing = false;

        [Tooltip("버튼 딜레이가 필요한 경우 설정")]
        [SerializeField]
        protected bool useDelay = true;

        [ShowIf(nameof(UseDelay), true)]
        [Tooltip("버튼 딜레이 시간"), SerializeField, Range(0.1f, 1f)]
        protected float delayTime = 0.5f;

        #endregion

        #region Public Properties

        /// <summary>
        /// 버튼 딜레이 사용 유무
        /// </summary>
        public bool UseDelay => useDelay;

        /// <summary>
        /// 버튼 딜레이 시간
        /// </summary>
        public float DelayTime => UseDelay ? delayTime : 0f;

        #endregion

        #region Public Methods

        /// <summary>
        /// 누르기
        /// </summary>
        /// <param name="interactor"></param>
        // ReSharper disable once ParameterHidesMember
        public virtual void Hover(PXRDirectInteractor interactor)
        {
            if (!CanInteract)
                return;

            isPressing = true;

            HoverEvent?.Invoke();
            this.interactor = interactor;
        }

        /// <summary>
        /// 떼기
        /// </summary>
        public virtual void UnHover()
        {
            if (!CanInteract)
                return;

            isPressing = false;

            UnHoverEvent?.Invoke();
            interactor = null;
        }

        /// <summary>
        /// 현재 상호작용중인 손을 설정
        /// </summary>
        /// <param name="handSide">손의 방향</param>
        // ReSharper disable once ParameterHidesMember
        public void SetInteractHand(HandSide handSide)
        {
            if (!CanInteract)
                return;

            this.handSide = handSide;
        }

        #endregion
    }
}