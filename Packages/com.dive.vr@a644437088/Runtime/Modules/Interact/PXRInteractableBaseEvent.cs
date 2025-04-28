using System;
using UnityEngine.EventSystems;

namespace Dive.VRModule
{
    public partial class PXRInteractableBase
    {
        #region Public Fields

        /// <summary>
        /// 포인터가 오브젝트에 들어왔을 때의 이벤트
        /// </summary>
        public event Action PointerEnterEvent;

        /// <summary>
        /// 포인터가 오브젝트를 나갔을 때 이벤트
        /// </summary>
        public event Action PointerExitEvent;

        /// <summary>
        /// 포인터로 오브젝트를 클릭했을 때 이벤트
        /// </summary>
        public event Action<HandSide> PointerClickEvent;
        

        /// <summary>
        /// Grabber로 오브젝트에 들어왔을 때 이벤트
        /// </summary>
        public event Action GrabberEnterEvent;

        /// <summary>
        /// Grabber로 오브젝트에 나갔을 때 이벤트
        /// </summary>
        public event Action GrabberExitEvent;

        #endregion

        #region Public Methods

        /// <summary>
        /// 포인터가 오브젝트에 들어왔을 때의 이벤트 함수
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnterEvent?.Invoke();
        }

        /// <summary>
        /// 포인터가 오브젝트를 나갔을을 때의 이벤트 함수
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            PointerExitEvent?.Invoke();
        }

        /// <summary>
        /// 포인터로 오브젝트를 클릭했을 때의 이벤트 함수
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            var handSide = eventData.GetHandSide(leftPointer, rightPointer);
            PointerClickEvent?.Invoke(handSide);
        }
        
        /// <summary>
        /// OnPointerEnter 이벤트를 강제로 실행
        /// </summary>
        public virtual void ExecuteOnPointerEnter()
        {
            PointerEnterEvent?.Invoke();
        }

        /// <summary>
        /// OnPointerExit 이벤트를 강제로 실행
        /// </summary>
        public virtual void ExecuteOnPointerExit()
        {
            PointerExitEvent?.Invoke();
        }
        
        /// <summary>
        /// OnPointerClick 이벤트를 강제로 실행
        /// </summary>
        public virtual void ExecuteOnPointerClick(HandSide handSide)
        {
            PointerClickEvent?.Invoke(handSide);
        }

        /// <summary>
        /// OnGrabberEnter 이벤트를 강제로 실행
        /// </summary>
        public virtual void ExecuteOnGrabberEnter()
        {
            GrabberEnterEvent?.Invoke();
        }

        /// <summary>
        /// OnGrabberExit 이벤트를 강제로 실행
        /// </summary>
        public virtual void ExecuteOnGrabberExit()
        {
            GrabberExitEvent?.Invoke();
        }
        
        #endregion


    }
}