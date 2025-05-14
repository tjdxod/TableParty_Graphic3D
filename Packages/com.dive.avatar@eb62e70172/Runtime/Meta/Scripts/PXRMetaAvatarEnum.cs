#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

namespace Dive.Avatar.Meta
{
    public enum MetaAvatarPlatformInitStatus
    {
        NotStarted = 0,
        Initializing,
        Succeeded,
        Failed
    }
}

#endif