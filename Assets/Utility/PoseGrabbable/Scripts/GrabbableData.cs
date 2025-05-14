using System.Collections;
using System.Collections.Generic;
using Dive.VRModule;
using Sirenix.OdinInspector;
using UnityEngine;

public class GrabbableData : SerializedScriptableObject
{
    [Header("Pose")]
    public PXRPose Pose;
    
    [Space]
    
    [Header("Native Data")]
    public Vector3 nativePosition;
    public Vector3 nativeRotation;
    
    [Space]
    
    [Header("Meta Data")]
    public Vector3 metaPosition;
    public Vector3 metaRotation;
    
    [Space]
    
    [Header("Pico Data")]
    public Vector3 picoPosition;
    public Vector3 picoRotation;
}
