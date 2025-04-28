using UnityEngine;

namespace Dive.VRModule
{
    // PXRAnimationController.cs
    public partial class PXRPlayerController
    {
        #region Private Fields

        private PXRHandAnimationController leftHandAnimationController;
        private PXRHandAnimationController rightHandAnimationController;        
        
        private bool isLeftHandRendererActive = false;
        private bool isRightHandRendererActive = false;
        
        #endregion
        
        #region Public Properties

        /// <summary>
        /// 양손의 애니매이션 컨트롤러
        /// </summary>
        public PXRHandAnimationController[] HandAnimationController { get; private set; }
        
        /// <summary>
        /// 왼손 애니매이션 컨트롤러
        /// </summary>
        public PXRHandAnimationController LeftHandAnimationController
        {
            get
            {
                if (leftHandAnimationController == null)
                {
                    leftHandAnimationController = HandAnimationController.Find(hac => hac.controller == HandSide.Left);
                    
                    if (leftHandAnimationController == null)
                        Debug.LogWarning("LeftHandAnimationController is null");
                }

                return leftHandAnimationController;
            }
        }
        
        /// <summary>
        /// 오른손 애니매이션 컨트롤러
        /// </summary>
        public PXRHandAnimationController RightHandAnimationController
        {
            get
            {
                if (rightHandAnimationController == null)
                {
                    rightHandAnimationController = HandAnimationController.Find(hac => hac.controller == HandSide.Right);
                    
                    if (rightHandAnimationController == null)
                        Debug.LogWarning("RightHandAnimationController is null");
                }

                return rightHandAnimationController;
            }
        }        
        
        #endregion
        
        #region Public Methods

        /// <summary>
        /// 손의 LineRenderer을 활성화
        /// </summary>
        /// <param name="handSide">손 방향</param>
        /// <param name="isActive">활성화인 경우 true, 그렇지 않은 경우 false</param>
        public void ActiveHandRenderer(HandSide handSide, bool isActive)
        {
            var hand = HandAnimationController.Find(hac => hac.controller == handSide);
            
            if(hand == null)
                return;
            
            if(HandSide.Left == handSide)
                isLeftHandRendererActive = isActive;
            else
                isRightHandRendererActive = isActive;
            
            var renderers = hand.GetComponentsInChildren<Renderer>();

            foreach (var rend in renderers)
            {
                rend.enabled = isActive;
            }
        }        
        
        #endregion
    }
}