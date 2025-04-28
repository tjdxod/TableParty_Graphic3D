using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    // PXRClickerController.cs
    public partial class PXRPlayerController
    {
        #region Private Fields

        private PXRClicker leftClicker;
        private PXRClicker rightClicker;        
        
        #endregion
        
        #region Public Properties

        /// <summary>
        /// Rig의 양손 Grabber
        /// </summary>
        public PXRClicker[] Clickers { get; private set; }

        /// <summary>
        /// 왼손 Grabber
        /// </summary>
        public PXRClicker LeftClicker
        {
            get
            {
                if (leftClicker == null)
                {
                    leftClicker = Clickers.Find(p => p.HandSide == HandSide.Left);
                    
                    if (leftClicker == null)
                        Debug.LogWarning("LeftClicker is null");
                }

                return leftClicker;
            }
        }
        
        /// <summary>
        /// 오른손 Grabber
        /// </summary>
        public PXRClicker RightClicker
        {
            get
            {
                if (rightClicker == null)
                {
                    rightClicker = Clickers.Find(p => p.HandSide == HandSide.Right);
                    
                    if (rightClicker == null)
                        Debug.LogWarning("RightClicker is null");
                }

                return rightClicker;
            }
        }

        public bool IsClicking => LeftClicker.IsClicking || RightClicker.IsClicking;

        #endregion
    }
}
