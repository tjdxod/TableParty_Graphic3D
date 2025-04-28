using BNG;
using UnityEngine;

namespace Dive.VRModule
{
    /// <summary>
    /// 클릭 Down 이벤트
    /// </summary>
    public delegate void DownEvent(PXRClickable clickable, HandSide handSide);

    /// <summary>
    /// 클릭 Up 이벤트
    /// </summary>
    public delegate void UpEvent(PXRClickable clickable, HandSide handSide);
    
    public class PXRClicker : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private PXRPointerVR pointer;
        
        /// <summary>
        /// 반대쪽 Grabber
        /// </summary>
        [Tooltip("반대쪽 손의 Clicker 할당")]
        [SerializeField]
        protected PXRClicker otherClicker;

        [Tooltip("손의 애니메이터를 할당")]
        [SerializeField]
        private Animator handAnimator;

        [Tooltip("손 모양을 저장하는 Poser를 할당")]
        [SerializeField]
        private HandPoser handPoser;

        [SerializeField]
        private PXRTubeRendererController tubeRendererController;
        
        /// <summary>
        /// Input Handler
        /// </summary>
        protected PXRInputHandlerBase inputHandler;

        /// <summary>
        /// Player Controller
        /// </summary>
        protected PXRPlayerController playerController;

        private bool isClicking = false;
        
        private PXRClickable currentClickedClickable;

        #endregion

        #region Public Properties

        /// <summary>
        /// 손의 방향
        /// </summary>
        [field: Tooltip("손의 방향 (왼손 or 오른손)")]
        [field: SerializeField]
        public HandSide HandSide { get; private set; }

        public PXRClickable ClickedClickable { get; protected set; }

        public Vector3 LinePosition => pointer.LineRenderer.transform.position;
        public Vector3 LineUpDirection => pointer.LineRenderer.transform.up;
        
        public PXRTubeRendererController TubeRendererController => tubeRendererController;
        
        /// <summary>
        /// 클릭의 상태
        /// </summary>
        public ClickState ClickState { get; private set; }

        public bool IsClicking
        {
            get => isClicking;
            private set
            {
                isClicking = value;
                playerController.ChangeHandState(HandSide, value ? HandInteractState.Clicking : HandInteractState.None);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 변수 초기화
        /// </summary>
        protected virtual void Awake()
        {
            playerController = PXRRig.PlayerController;
            inputHandler = PXRInputBridge.GetXRController(HandSide);
        }

        internal void Clicked(PXRClickable clickable)
        {
            if (isClicking)
            {
                InitializeClicker();
                return;
            }
            
            if(clickable.IsClicked)
                return;
            
            ClickedClickable = null;

            if (playerController.GetHandState(HandSide) != HandInteractState.None)
                return;

            playerController.ChangeHandState(HandSide, HandInteractState.Clicking);
            
            if(HandSide == HandSide.Right)
            {
                PXRRig.RightPointer.HideLineRenderer();
                PXRRig.RightPointer.HideEndpointCircle();
            }
            else if(HandSide == HandSide.Left)
            {
                PXRRig.LeftPointer.HideLineRenderer();
                PXRRig.LeftPointer.HideEndpointCircle();
            }

            ClickedClickable = clickable;
            currentClickedClickable = ClickedClickable;
            IsClicking = true;
            ClickState = ClickState.Click;

            ClickedClickable.Clicked(HandSide);
        }
        
        internal void Released(PXRClickable clickable)
        {
            if (!isClicking)
                return;

            playerController.ChangeHandState(HandSide, HandInteractState.None);
            
            if(HandSide == HandSide.Right)
            {
                PXRRig.RightPointer.ShowLineRenderer();
                PXRRig.RightPointer.ShowEndpointCircle();
            }
            else if(HandSide == HandSide.Left)
            {
                PXRRig.LeftPointer.ShowLineRenderer();
                PXRRig.LeftPointer.ShowEndpointCircle();
            }

            currentClickedClickable.Released(HandSide);
            IsClicking = false;
            ClickState = ClickState.None;
            ClickedClickable = null;
            currentClickedClickable = null;
        }

        internal void ForceReleased()
        {
            playerController.ChangeHandState(HandSide, HandInteractState.None);
            
            if(HandSide == HandSide.Right)
            {
                PXRRig.RightPointer.ShowLineRenderer();
                PXRRig.RightPointer.ShowEndpointCircle();
            }
            else if(HandSide == HandSide.Left)
            {
                PXRRig.LeftPointer.ShowLineRenderer();
                PXRRig.LeftPointer.ShowEndpointCircle();
            }
            
            IsClicking = false;
            ClickState = ClickState.None;
            ClickedClickable = null;
            currentClickedClickable = null;
        }
        
        protected void InitializeClicker()
        {
            isClicking = false;

            ClickedClickable = null;
            ClickState = ClickState.None;

            if (handPoser)
            {
                handPoser.PreviousPose = null;
                handPoser.CurrentPose = null;
                handAnimator.enabled = true;
            }

            currentClickedClickable = null;
        }

        #endregion
    }
}