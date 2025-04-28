#if UNITY_2019_2_OR_NEWER
using System.Reflection;
#else
using System.IO;
#endif
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class Version
    {
#if !UNITY_2019_2_OR_NEWER
        struct PackageInfo
        {
#pragma warning disable 649
            public string version;
#pragma warning restore 649
        }
#endif
    }
}
