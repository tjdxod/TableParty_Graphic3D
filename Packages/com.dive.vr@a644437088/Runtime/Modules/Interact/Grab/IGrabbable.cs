namespace Dive.VRModule
{
    /// <summary>
    /// Grabbable을 반환하는 인터페이스
    /// </summary>
    public interface IGrabbable
    {
        /// <summary>
        /// Grabbable을 반환
        /// </summary>
        /// <returns>Grabbable 클래스</returns>
        public PXRGrabbable GetGrabbable();
    }
}