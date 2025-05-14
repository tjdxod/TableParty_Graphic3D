using System.Collections;
using System.Collections.Generic;
using Dive.VRModule;
using UnityEngine;

#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

public class PXRMetaAnchorFollow : MonoBehaviour
{
    [SerializeField]
    private Transform originTargetAnchor;

    [SerializeField]
    private bool isUpdate = true;
    
    private Vector3 prePosition;
    private Quaternion preRotation;
    
    void Update()
    {
        if (originTargetAnchor == null)
            return;
        
        if(!isUpdate)
            return;
        
        var position = originTargetAnchor.localPosition;
        var rotation = originTargetAnchor.localRotation;

        if (!OVRManager.hasVrFocus)
        {
            position = prePosition;
            rotation = preRotation;
        }
        else
        {
            position.y += PXRRig.Current.transform.position.y + PXRRig.PlayerController.transform.position.y;
            
            prePosition = position;
            preRotation = rotation;
        }
        
        transform.localPosition = position;
        transform.localRotation = rotation;
    }
}

#endif