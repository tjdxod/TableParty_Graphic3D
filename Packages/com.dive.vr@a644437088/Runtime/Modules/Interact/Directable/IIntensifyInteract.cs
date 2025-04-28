using UnityEngine;

namespace Dive.VRModule
{
    public interface IIntensifyInteract
    {
        public InteractMode ModeType { get; }
        public int Type { get; }

        public void Initialize(PXRInteractAddon addon);
        public PXRDirectableBase[] GetDirectableArray();
        public (Collider[], int) OverlapAtPoint(InteractMode mode);
        
        public (Vector3, Vector3) GetPosition(bool isWorld = true);
        public (Quaternion, Quaternion) GetRotation(bool isWorld = true);
        public Vector3 GetDirection(bool isWorld = true);
        public PXRIntensifyInteractBase GetIntensifyInteractBase();
    }
}
