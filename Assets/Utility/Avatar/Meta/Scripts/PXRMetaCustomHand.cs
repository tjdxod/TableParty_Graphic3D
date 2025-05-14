using System.Collections;
using System.Collections.Generic;
using Dive.Avatar.Meta;
using Dive.VRModule;
using Oculus.Avatar2;
using UnityEngine;

public class PXRMetaCustomHand : PXRPoser
{
    [SerializeField]
    private OvrAvatarCustomHandPose customHandPose;

    [SerializeField]
    private PXRPose testPose;
    
    [Sirenix.OdinInspector.Button]
    private void Test()
    {
        SetPose(testPose);
    }

    protected override void Awake()
    {
        gameObject.SetActive(false);
        base.Awake();
    }

    public override void SetPose(PXRPose pose)
    {
        if (pose == null)
        {
            customHandPose.setHandPose = false;
            customHandPose.setWristOffset = false;
        }
        else
        {
            for (var i = 0; i < 3; i++)
            {
                indexJoints[i].localRotation = pose.Joints.IndexJoints[i].LocalRotation;
                middleJoints[i].localRotation = pose.Joints.MiddleJoints[i].LocalRotation;
                ringJoints[i].localRotation = pose.Joints.RingJoints[i].LocalRotation;
            }
            for (var i = 0; i < 4; i++)
            {
                thumbJoints[i].localRotation = pose.Joints.ThumbJoints[i].LocalRotation;
                pinkyJoints[i].localRotation = pose.Joints.PinkyJoints[i].LocalRotation;
            }
            
            customHandPose.setHandPose = true;
            customHandPose.setWristOffset = false;
        }
    }

    public override void SetPreviousPose(PXRPose pose)
    {
        
    }
}
