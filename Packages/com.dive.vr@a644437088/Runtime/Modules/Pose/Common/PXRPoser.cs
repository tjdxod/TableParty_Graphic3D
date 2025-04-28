using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dive.VRModule
{
    public abstract class PXRPoser : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private HandSide handSide = HandSide.Left;

        [SerializeField]
        protected Transform[] thumbJoints;

        [SerializeField]
        protected Transform[] indexJoints;

        [SerializeField]
        protected Transform[] middleJoints;

        [SerializeField]
        protected Transform[] ringJoints;

        [SerializeField]
        protected Transform[] pinkyJoints;

        private PXRGrabber grabber = null;
        private PXRPose currentPose = null;
        private PXRPose previousPose = null;

        #endregion

        #region Public Properties

        public PXRPose CurrentPose
        {
            get => currentPose;
            set
            {
                previousPose = currentPose;
                currentPose = value;
                SetPose(value);
            }
        }

        public PXRPose PreviousPose
        {
            get => previousPose;
            set
            {
                previousPose = value;
                SetPreviousPose(value);
            }
        }

        public HandSide HandSide => handSide;
        public string CurrentPoseName => CurrentPose == null ? "" : CurrentPose.PoseName;

        public Transform[] ThumbJoints => thumbJoints;
        public Transform[] IndexJoints => indexJoints;
        public Transform[] MiddleJoints => middleJoints;
        public Transform[] RingJoints => ringJoints;
        public Transform[] PinkyJoints => pinkyJoints;
        
        #endregion

        #region Private Properties

        private PXRGrabber Grabber
        {
            get
            {
                if (grabber == null)
                {
                    grabber = HandSide == HandSide.Left ? PXRRig.LeftGrabber : PXRRig.RightGrabber;
                }

                return grabber;
            }
        }

        #endregion


        #region Public Methods

        public abstract void SetPose(PXRPose pose);

        public abstract void SetPreviousPose(PXRPose pose);

        #endregion

        #region Private Methods

        protected virtual void Awake()
        {
            Grabber.SetPoser(this);
        }

        protected virtual void OnDestroy()
        {
            Grabber.DeletePoser(this);
        }
        
        #endregion
    }
}