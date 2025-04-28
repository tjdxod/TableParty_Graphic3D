using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dive.VRModule
{
    /// <summary>
    /// 클릭 상호작용이 가능한 오브젝트
    /// </summary>
    public class PXRClickable : PXRInteractableBase, IPointerDownHandler, IPointerUpHandler
    {
        #region Public Fields

        /// <summary>
        /// 클릭 Down 이벤트
        /// </summary>
        public event UpEvent UpEvent;
        
        /// <summary>
        /// 클릭 Up 이벤트
        /// </summary>
        public event DownEvent DownEvent;
        
        
        public Action PointerStayEvent;
        
        #endregion

        #region Private Fields

        [Space(10f)]
        [Header("Clickable")]
        [Space(10f)]
        private IEnumerator routineDoStayEvent;
        
        private readonly Dictionary<HandSide, PXRClicker> clickers = new Dictionary<HandSide, PXRClicker>();
        
        #endregion

        #region Public Properties

        public bool IsClicked { get; private set; } = false;
        public PXRClicker Clicker { get; private set; } = null;

        #endregion
        
        #region Public Methods

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (routineDoStayEvent != null)
            {
                StopCoroutine(routineDoStayEvent);
                routineDoStayEvent = null;
            }

            if (PointerStayEvent == null) 
                return;
            
            routineDoStayEvent = CoroutineDoPointerStay();
            StartCoroutine(routineDoStayEvent);
        }
        
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (routineDoStayEvent != null)
                StopCoroutine(routineDoStayEvent);
        }
        
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if(transferState == TransferState.None)
                return;
            
            var cam = eventData.pointerPressRaycast.gameObject;
            if(cam == null)
                return;

            var handSide = eventData.GetHandSide(leftPointer, rightPointer);
            clickers[handSide].Released(this);
        }
        
        public virtual void OnPointerDown(PointerEventData eventData) 
        {
            if(transferState == TransferState.None)
                return;
            
            var cam = eventData.pointerPressRaycast.gameObject;
            if(cam == null)
                return;
            
            var handSide = eventData.GetHandSide(leftPointer, rightPointer);
            clickers[handSide].Clicked(this);
        }
        
        internal void Clicked(HandSide handSide)
        {
            IsClicked = true;
            Clicker = clickers[handSide];
            DownEvent?.Invoke(this, handSide);
        }
        
        internal void Released(HandSide handSide)
        {
            IsClicked = false;
            Clicker = null;
            UpEvent?.Invoke(this, handSide);
        }
        
        #endregion

        #region Private Methods

        protected override void Awake()
        {
            base.Awake();
            
            clickers.Add(HandSide.Left, PXRRig.LeftClicker);
            clickers.Add(HandSide.Right, PXRRig.RightClicker);
        }
        
        // ReSharper disable once RedundantOverriddenMember
        protected override void Start()
        {
            base.Start();
        }

        protected virtual void OnDestroy()
        {
            if(Clicker != null)
                Clicker.ForceReleased();
        }

        protected virtual void OnDisable()
        {
            if(Clicker != null)
                Clicker.ForceReleased();
        }
        
        private IEnumerator CoroutineDoPointerStay()
        {
            while (true)
            {
                PointerStayEvent?.Invoke();
                yield return null;
            }
        
            // ReSharper disable once IteratorNeverReturns
        }         
        
        #endregion
    }
}
