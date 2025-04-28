using System.Collections;
using System.Collections.Generic;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes.Utility;
#endif

using UnityEngine;

namespace Dive.Utility.Localizer
{
    public partial class Localizer
    {
#if !ODIN_INSPECTOR
        [HorizontalLine(10f, color: EColor.White)]
#endif
        [FoldoutGroup("Texture Localizer"), ShowIf(nameof(localizerType), Type.Texture), SerializeField]
        private Renderer rendererComponent;

        [FoldoutGroup("Texture Localizer"), ShowIf(nameof(localizerType), Type.Texture), SerializeField]
        private string currentTextureKey;

        [FoldoutGroup("Texture Localizer"), ShowIf(nameof(localizerType), Type.Texture), SerializeField]
        private Language modifiedTextureLanguage;

        private bool HasTexture()
        {
            return (localizerType & Type.Texture) == Type.Texture;
        }

        private void RendererChanged()
        {
        }
    }
}