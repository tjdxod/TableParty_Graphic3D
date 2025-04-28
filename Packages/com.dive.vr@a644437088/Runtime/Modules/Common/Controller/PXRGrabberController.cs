using UnityEngine;

namespace Dive.VRModule
{
    // PXRGrabberController.cs
    public partial class PXRPlayerController
    {
        #region Private Fields

        private PXRGrabber leftGrabber;
        private PXRGrabber rightGrabber;        
        
        private bool isLeftGrabberActive = false;
        private bool isRightGrabberActive = false;
        
        #endregion
        
        #region Public Properties

        /// <summary>
        /// Rig의 양손 Grabber
        /// </summary>
        public PXRGrabber[] Grabbers { get; private set; }

        /// <summary>
        /// 왼손 Grabber
        /// </summary>
        public PXRGrabber LeftGrabber
        {
            get
            {
                if (leftGrabber == null)
                {
                    leftGrabber = Grabbers.Find(p => p.HandSide == HandSide.Left);
                    
                    if (leftGrabber == null)
                        Debug.LogWarning("LeftGrabber is null");
                }

                return leftGrabber;
            }
        }
        
        /// <summary>
        /// 오른손 Grabber
        /// </summary>
        public PXRGrabber RightGrabber
        {
            get
            {
                if (rightGrabber == null)
                {
                    rightGrabber = Grabbers.Find(p => p.HandSide == HandSide.Right);
                    
                    if (rightGrabber == null)
                        Debug.LogWarning("RightGrabber is null");
                }

                return rightGrabber;
            }
        }        
        
        #endregion
        
        #region Public Methods

        /// <summary>
        /// Grabber 활성화
        /// </summary>
        /// <param name="handSide">손 방향</param>
        public void ActivateGrabber(HandSide handSide)
        {
            foreach (var grabber in Grabbers)
            {
                if (grabber.HandSide == handSide)
                    grabber.enabled = true;
            }
            
            if (handSide == HandSide.Left)
                isLeftGrabberActive = true;
            else
                isRightGrabberActive = true;
        }

        /// <summary>
        /// Grabber 비활성화
        /// </summary>
        /// <param name="handSide">손 방향</param>
        public void DeactivateGrabber(HandSide handSide)
        {
            foreach (var grabber in Grabbers)
            {
                if (grabber.HandSide == handSide)
                    grabber.enabled = false;
            }
            
            if (handSide == HandSide.Left)
                isLeftGrabberActive = false;
            else
                isRightGrabberActive = false;
        }

        /// <summary>
        /// 모든 Grabber 활성화
        /// </summary>
        public void ActivateAllGrabber()
        {
            foreach (var grabber in Grabbers)
            {
                grabber.enabled = true;
            }
            
            isLeftGrabberActive = true;
            isRightGrabberActive = true;
        }

        /// <summary>
        /// 모든 Grabber 비활성화
        /// </summary>
        public void DeactivateAllGrabber()
        {
            foreach (var grabber in Grabbers)
            {
                grabber.enabled = false;
            }
            
            isLeftGrabberActive = false;
            isRightGrabberActive = false;
        }

        /// <summary>
        /// 모든 Grabber가 잡고있는 물체를 강제로 내려놓음.
        /// </summary>
        public void ForceReleaseAllGrabber()
        {
            foreach (var grabber in Grabbers)
            {
                grabber.ForceRelease();
            }
        }

        /// <summary>
        /// 현재 손의 상태를 변경
        /// </summary>
        /// <param name="side">손 방향</param>
        /// <param name="state">손 상태</param>
        public void ChangeHandState(HandSide side, HandInteractState state)
        {
            if (side == HandSide.Left)
                leftHandState = state;
            else
                rightHandState = state;
        }

        /// <summary>
        /// 손의 Interact 상태를 반환
        /// </summary>
        /// <param name="side">손 방향</param>
        /// <returns>손의 Interact 상태</returns>
        public HandInteractState GetHandState(HandSide side)
        {
            return side == HandSide.Left ? leftHandState : rightHandState;
        }

        /// <summary>
        /// 반대쪽 손의 Interact 상태를 반환
        /// </summary>
        /// <param name="side">손 방향</param>
        /// <returns>손의 Interact 상태</returns>
        public HandInteractState GetOtherHandState(HandSide side)
        {
            return side == HandSide.Left ? rightHandState : leftHandState;
        }        
        
        #endregion
    }
}