namespace Dive.VRModule
{
    public interface IDirectable
    {
        public PXRDirectableBase GetInteractableBase();

        public void ForceRelease(bool useEvent = true);
        public void ForcePress(bool useEvent = true);
        
    }
}
