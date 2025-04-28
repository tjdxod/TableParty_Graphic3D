using System;
using System.Collections;
using System.Collections.Generic;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#else

using NaughtyAttributes.Utility;

#endif

using TMPro;
using UnityEngine;

namespace Dive.Utility.Localizer
{
    public partial class Localizer : MonoBehaviour
    {
#if UNITY_EDITOR
        [OnValueChanged(nameof(OnChangeLocalizerType))]
#endif
        [SerializeField]
        private Type localizerType;

        private void OnEnable()
        {
            if ((localizerType & Type.Text) == Type.Text)
                TextLocalizerManager.Instance.LocalizeChangedEvent += OnLocalizeChanged;

            if ((localizerType & Type.Image) == Type.Image)
                return;

            if ((localizerType & Type.Texture) == Type.Texture)
                return;
        }

        private void OnDisable()
        {
            if ((localizerType & Type.Text) == Type.Text)
                TextLocalizerManager.Instance.LocalizeChangedEvent -= OnLocalizeChanged;

            if ((localizerType & Type.Image) == Type.Image)
                return;

            if ((localizerType & Type.Texture) == Type.Texture)
                return;
        }

        private void OnLocalizeChanged()
        {
            if ((localizerType & Type.Text) == Type.Text)
            {
                if (textComponent != null)
                {
                    TextChanged();
                }
            }

            if ((localizerType & Type.Image) == Type.Image)
            {
                if (imageComponent != null)
                {
                    ImageChanged();
                }

                if (spriteRendererComponent != null)
                {
                    SpriteChanged();
                }
            }

            if ((localizerType & Type.Texture) == Type.Texture)
            {
                if (rendererComponent != null)
                {
                    RendererChanged();
                }
            }
        }
    }
}