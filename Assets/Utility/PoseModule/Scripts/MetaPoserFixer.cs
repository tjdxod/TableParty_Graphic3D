using System.Collections;
using System.Collections.Generic;
using Dive.VRModule;
using UnityEngine;

public class MetaPoserFixer : MonoBehaviour
{
    [SerializeField]
    private MetaPoser leftMetaPoser;
    
    [SerializeField]
    private MetaPoser rightMetaPoser;
    
    [SerializeField]
    private PXRPose leftPose;
    
    [SerializeField]
    private PXRPose rightPose;
    
    [SerializeField]
    private MetaPoserCopy leftMetaPoserCopy;
    
    [SerializeField]
    private MetaPoserCopy rightMetaPoserCopy;

    public void SetCopyPose(HandPoseData.HandMode handMode)
    {
        if (handMode == HandPoseData.HandMode.Left)
        {
            leftMetaPoser.SetPose(leftPose);
        }
        else if (handMode == HandPoseData.HandMode.Right)
        {
            rightMetaPoser.SetPose(rightPose);
        }
    }
}
