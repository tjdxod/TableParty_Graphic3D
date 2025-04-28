using UnityEngine;

namespace Dive.VRModule
{
    // ReSharper disable once CommentTypo
    // PXRVRPointerController.cs
    public partial class PXRPlayerController
    {
        #region Private Fields

        private HandInteractState leftHandState;
        private HandInteractState rightHandState;
        private LineRendererState lineRendererState;

        private PXRPointerVR leftPointer;
        private PXRPointerVR rightPointer;        
        
        private bool isLeftPointerActive = false;
        private bool isRightPointerActive = false;
        
        #endregion
        
        #region Public Properties

        /// <summary>
        /// Rig의 양손 포인터
        /// </summary>
        public PXRPointerVR[] Pointers { get; private set; }

        /// <summary>
        /// 왼손 포인터
        /// </summary>
        public PXRPointerVR LeftPointer
        {
            get
            {
                if (leftPointer == null)
                {
                    leftPointer = Pointers.Find(p => p.HandSide == HandSide.Left);
                    
                    if (leftPointer == null)
                        Debug.LogWarning("LeftPointer is null");
                }

                return leftPointer;
            }
        }

        /// <summary>
        /// 오른손 포인터
        /// </summary>
        public PXRPointerVR RightPointer
        {
            get
            {
                if (rightPointer == null)
                {
                    rightPointer = Pointers.Find(p => p.HandSide == HandSide.Right);
                    
                    if (rightPointer == null)
                        Debug.LogWarning("RightPointer is null");
                }

                return rightPointer;
            }
        }

        /// <summary>
        /// 포인터 라인렌더러 활성화 여부
        /// </summary>
        public LineRendererState LineRendererState
        {
            get => lineRendererState;
            set
            {
                lineRendererState = value;
                SetPointerLineRendererState(value);
            }
        }        
        
        #endregion

        #region Public Methods

        /// <summary>
        /// 모든 포인터를 활성화
        /// </summary>
        public void ActivateAllPointer()
        {
            foreach (var tmp in Pointers)
            {
                tmp.CanProcess = true;
            }
            
            isLeftPointerActive = true;
            isRightPointerActive = true;
        }

        /// <summary>
        /// 모든 포인터를 비활성화
        /// </summary>
        public void DeactivateAllPointer()
        {
            foreach (var tmp in Pointers)
            {
                tmp.CanProcess = false;
            }
            
            isLeftPointerActive = false;
            isRightPointerActive = false;
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// 라인렌더러 상태를 변경
        /// </summary>
        /// <param name="renderState">라인렌더러 상태</param>
        private void SetPointerLineRendererState(LineRendererState renderState)
        {
            foreach (var tmp in Pointers)
            {
                if (renderState == LineRendererState.None)
                {
                    tmp.HideLineRenderer();
                }
                else if (renderState == LineRendererState.Always)
                {
                    tmp.ShowLineRenderer();
                }
            }
        }        
        
        #endregion
    }
}