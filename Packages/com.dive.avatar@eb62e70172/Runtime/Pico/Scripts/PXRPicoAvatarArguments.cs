#if DIVE_PLATFORM_PICO

namespace Dive.Avatar.Pico
{
    public class PicoAvatarAppConfigUtils
    {
        public const string PicoPlatformAppID = "d2c1fcda903176953e81e35588bcf912";
    }
    
    public class PicoAvatarPlatformInfoTestModelUtils
    {
        public const string UserID = "01df319b-2963-4627-bffa-9234478b11fa";
        /// <summary>
        /// The AvatarSpecification is used to load Avatar directly 
        /// </summary>
        public const string AvatarSpecification = "";
    }
    
    public enum LogState
    {
        None,
        Log,
        Error
    }
}

#endif