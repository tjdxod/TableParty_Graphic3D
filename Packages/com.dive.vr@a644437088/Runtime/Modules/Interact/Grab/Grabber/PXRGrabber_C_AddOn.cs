using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public partial class PXRGrabber
    {
        [SerializeField]
        private bool isNeedChangeTweezerPosition = false;

        public bool IsNeedChangeTweezerPosition => isNeedChangeTweezerPosition;
        
        [field: SerializeField, ShowIf(nameof(isNeedChangeTweezerPosition), true)]
        private Vector3 TweezerPosition { get; set; } = Vector3.zero;
        
        public static Vector3 GetGrabberPosition(HandSide handSide)
        {
            switch (handSide)
            {
                case HandSide.Left:
                    return new Vector3(0.0028f, -0.0351f, -0.0261f);
                case HandSide.Right:
                    return new Vector3(-0.0028f, -0.0351f, -0.0261f);
                case HandSide.Unknown:
                default:
                    return Vector3.zero;
            }
        }

        public static Vector3 GetGrabberTweezerPosition(HandSide handSide)
        {
            switch (handSide)
            {
                case HandSide.Left:
                    return new Vector3(-0.0222f, -0.0337f, 0.042f);
                case HandSide.Right:
                    return new Vector3(0.0222f, -0.0337f, 0.042f);
                case HandSide.Unknown:
                default:
                    return Vector3.zero;
            }
        }
        
        public void SetGrabberPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetGrabberTweezer()
        {
            if(isNeedChangeTweezerPosition)
                transform.localPosition = TweezerPosition;
            else
                transform.localPosition = GetGrabberTweezerPosition(HandSide);
        }
        
        public void ResetGrabberPosition()
        {
            transform.localPosition = GetGrabberPosition(HandSide);
        }
    }
}
