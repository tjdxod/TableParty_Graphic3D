using UnityEngine;

namespace Dive.VRModule
{
    public interface ITeleportSpace
    {
        public SpaceType SpaceType { get; }
        public bool UseActiveOnlyTeleporting { get; }
        public bool CanTeleport { get; }
        public bool CanVisible { get; }
        public bool UseFixedDirection { get; }
        public bool UseFixedHeight { get; }
        public bool IsEnteredPlayer { get; }
        public int CurrentInsidePlayerCount { get; }
        
        public bool UseForceTransform { get; }
        public Transform ForceTransform { get; }

        public PXRTeleportSpaceBase GetTeleportSpace();
        public PXRTeleportSpaceBase GetOriginalTeleportSpace();
        public float GetSpaceMaxY();
        public float GetSpaceMinY();
        public void IncreaseInsidePlayerCount();
        public void DecreaseInsidePlayerCount();
    }
}
