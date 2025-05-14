#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System.Collections;
using System.Collections.Generic;
using Dive.VRModule;
using UnityEngine;
using UnityEngine.Serialization;

public class PXRMetaCenterEye : MonoBehaviour
{
    [SerializeField]
    private bool isWorld;
    
    [SerializeField]
    private Transform originCenterEye;

    private Vector3 prePosition;
    private Quaternion preRotation;
    
    void Update()
    {
        if (originCenterEye == null)
            return;

        if (isWorld)
        {
            var position = originCenterEye.localPosition;
            var rotation = originCenterEye.localRotation;

            if (!OVRManager.hasVrFocus)
            {
                position = prePosition;
                rotation = preRotation;
            }
            else
            {
                position.y += PXRRig.PlayerController.transform.position.y;
                
                prePosition = position;
                preRotation = rotation;
            }
            
            transform.localPosition = position;
            transform.localRotation = rotation;
        }
        else
        {
            var position = new Vector3(0, originCenterEye.localPosition.y, 0);
            var rotation = originCenterEye.localRotation;

            transform.localPosition = position;
            transform.localRotation = rotation;
        }
    }
}

#endif
