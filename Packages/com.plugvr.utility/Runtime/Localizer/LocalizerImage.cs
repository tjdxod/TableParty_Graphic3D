using System.Collections;
using System.Collections.Generic;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using NaughtyAttributes.Utility;
#endif

using UnityEngine;
using UnityEngine.UI;

namespace Dive.Utility.Localizer
{
    public partial class Localizer
    {
#if !ODIN_INSPECTOR
        [HorizontalLine(10f, color: EColor.White)]
#endif
        [FoldoutGroup("Image Localizer"), ShowIf(nameof(localizerType), Type.Image), SerializeField]
        private bool isSpriteRenderer = false;

#if ODIN_INSPECTOR
        [FoldoutGroup("Image Localizer"), ShowIf(nameof(HasUIImage)), SerializeField]
#else
        [FoldoutGroup("Image Localizer"), ShowIf(EConditionOperator.And, nameof(HasUIImage)), SerializeField]
#endif

        private Image imageComponent;

#if ODIN_INSPECTOR
        [FoldoutGroup("Image Localizer"), ShowIf(nameof(HasSpriteRenderer)), SerializeField]
#else
        [FoldoutGroup("Image Localizer"), ShowIf(EConditionOperator.And, nameof(HasSpriteRenderer)), SerializeField]
#endif
        
        private SpriteRenderer spriteRendererComponent;

        [FoldoutGroup("Image Localizer"), ShowIf(nameof(localizerType), Type.Image), SerializeField]
        private string currentImageKey;

        [FoldoutGroup("Image Localizer"), ShowIf(nameof(localizerType), Type.Image), SerializeField]
        private Language modifiedImageLanguage;

        public string CurrentImageKey
        {
            get { return currentImageKey; }
            set
            {
                currentImageKey = value;
                OnLocalizeChanged();
            }
        }

        public Image ImageComponent
        {
            get { return imageComponent; }
        }

        public SpriteRenderer SpriteRendererComponent
        {
            get { return spriteRendererComponent; }
        }

        private bool HasImage()
        {
            return (localizerType & Type.Image) == Type.Image;
        }

        private bool HasUIImage()
        {
            return (localizerType & Type.Image) == Type.Image && !isSpriteRenderer;
        }

        private bool HasSpriteRenderer()
        {
            return (localizerType & Type.Image) == Type.Image && isSpriteRenderer;
        }

        private void ImageChanged()
        {
            if (CurrentImageKey.Equals(string.Empty))
                return;

            var language = ImageLocalizerManager.GetCurrentLanguage();

            ChangeImage(language);
        }

        private void SpriteChanged()
        {
            if (CurrentImageKey.Equals(string.Empty))
                return;

            var language = ImageLocalizerManager.GetCurrentLanguage();

            ChangeSpriteRenderer(language);
        }

        private void ChangeImage(Language language)
        {
        }

        private void ChangeSpriteRenderer(Language language)
        {
        }
    }
}