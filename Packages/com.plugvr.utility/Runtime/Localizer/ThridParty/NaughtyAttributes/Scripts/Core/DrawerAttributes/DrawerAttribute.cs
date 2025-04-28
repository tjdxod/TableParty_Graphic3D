#if !ODIN_INSPECTOR

using UnityEngine;

namespace NaughtyAttributes.Utility
{
    /// <summary>
    /// Base class for all drawer attributes
    /// </summary>
    public class DrawerAttribute : PropertyAttribute, INaughtyAttribute
    {
    }
}

#endif