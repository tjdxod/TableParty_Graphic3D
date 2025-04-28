using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

namespace Dive.VRModule
{
    public interface IAdditionalPressableButton
    {
        public Collider DetectCollider { get; }
        
        public Collider DefaultCollider { get; }
        
        public PXRPose TargetPXRPose { get; }
        
        public HandPose TargetHandPose { get; }
        
        public PXRAdditionalPressableButton GetPressableButton();
    }
}
