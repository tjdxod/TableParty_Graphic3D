using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    public partial class PXRRig
    {
        public static void SetLayerAllHand(int layer)
        {
            if (LeftHandAnimationController != null)
            {
                LeftHandAnimationController.HandRenderer.gameObject.layer = layer;
            }

            if (RightHandAnimationController != null)
            {
                RightHandAnimationController.HandRenderer.gameObject.layer = layer;
            }
        }
        
        public static void SetLayerHand(int layer, HandSide handSide)
        {
            if (handSide == HandSide.Left)
            {
                if (LeftHandAnimationController == null)
                    return;
                
                LeftHandAnimationController.HandRenderer.gameObject.layer = layer;
            }
            else
            {
                if (RightHandAnimationController == null)
                    return;
                
                RightHandAnimationController.HandRenderer.gameObject.layer = layer;
            }
        }
        
        public static void SetLayerAllHandSleeve(int layer)
        {
            if (LeftHandAnimationController != null)
            {
                LeftHandAnimationController.HandSleeveRenderer.gameObject.layer = layer;
            }

            if (RightHandAnimationController != null)
            {
                RightHandAnimationController.HandSleeveRenderer.gameObject.layer = layer;
            }
        }
        
        public static void SetLayerHandSleeve(int layer, HandSide handSide)
        {
            if (handSide == HandSide.Left)
            {
                if (LeftHandAnimationController == null)
                    return;
                
                LeftHandAnimationController.HandSleeveRenderer.gameObject.layer = layer;
            }
            else
            {
                if (RightHandAnimationController == null)
                    return;
                
                RightHandAnimationController.HandSleeveRenderer.gameObject.layer = layer;
            }
        }

        public static void SetHandRenderer(Material handMaterial)
        {
            if (LeftHandAnimationController != null)
            {
                LeftHandAnimationController.HandRenderer.sharedMaterial = handMaterial;
            }

            if (RightHandAnimationController != null)
            {
                RightHandAnimationController.HandRenderer.sharedMaterial = handMaterial;
            }
        }

        public static void SetHandSleeveRenderer(Mesh handSleeveMesh, Material handSleeveMaterial)
        {
            if (LeftHandAnimationController != null)
            {
                LeftHandAnimationController.HandSleeveMeshFilter.sharedMesh = handSleeveMesh;
                LeftHandAnimationController.HandSleeveRenderer.sharedMaterial = handSleeveMaterial;
            }

            if (RightHandAnimationController != null)
            {
                RightHandAnimationController.HandSleeveMeshFilter.sharedMesh = handSleeveMesh;
                RightHandAnimationController.HandSleeveRenderer.sharedMaterial = handSleeveMaterial;
            }
        }
    }
}
