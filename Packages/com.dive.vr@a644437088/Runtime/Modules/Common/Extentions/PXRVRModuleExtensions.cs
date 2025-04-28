using UnityEngine;
using UnityEngine.UI;

namespace Dive.VRModule
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once IdentifierTypo
    public static class PXRVRModuleExtensions
    {
        public static HandSide Other(this HandSide handSide)
        {
            return handSide == HandSide.Left ? HandSide.Right : HandSide.Left;
        }
        
        public static PXRGrabbable GetGrabbable(this GameObject target)
        {
            if (!target)
                return null;

            var component = target.GetComponent<IGrabbable>();
            return component?.GetGrabbable();
        }

        public static void SetLayerRecursively(this GameObject target, int layer)
        {
            target.layer = layer;

            foreach (Transform child in target.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
        
        public static void ClearAlpha(this Image image)
        {
            var color = image.color;
            color.a = 0f;
            image.color = color;
        }
    }
}
