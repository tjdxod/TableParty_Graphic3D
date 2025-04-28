using Dive.VRModule;

namespace UnityEngine.EventSystems
{
    public static class PXRPointerEventDataExtensions
    {
        public static HandSide GetHandSide(this PointerEventData eventData, PXRPointerVR leftController, PXRPointerVR rightController)
        {
            var rawPointerPress = eventData.rawPointerPress;
            
            if (rawPointerPress == null)
                return HandSide.Left;
            
            var pointer = rawPointerPress.GetComponent<PXRPointerVR>();
            return pointer.HandSide;
        }
    }
}
