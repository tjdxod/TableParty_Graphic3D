using System.Collections;
using System.Collections.Generic;
using Dive.Avatar.Meta;
using Dive.VRModule;
using Oculus.Avatar2;
using UnityEditor;
using UnityEngine;

public class PXRMetaCustomHand : PXRPoser
{
    [SerializeField]
    private OvrAvatarCustomHandPose customHandPose;

    public PXRPose testPose;
    
    [Sirenix.OdinInspector.Button]
    private void Test()
    {
        SetPose(testPose);
        
        customHandPose.setHandPose = false;
        customHandPose.setWristOffset = false;
        
        customHandPose.setHandPose = true;
        customHandPose.setWristOffset = false;
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

    [Sirenix.OdinInspector.Button("저장(덮어쓰기)")]
    public void SavePose()
    {
        for (var i = 0; i < 3; i++)
        {
            testPose.Joints.IndexJoints[i].LocalRotation = indexJoints[i].localRotation;
            testPose.Joints.MiddleJoints[i].LocalRotation = middleJoints[i].localRotation;
            testPose.Joints.RingJoints[i].LocalRotation = ringJoints[i].localRotation;
        }
        for (var i = 0; i < 4; i++)
        {
            testPose.Joints.ThumbJoints[i].LocalRotation = thumbJoints[i].localRotation;
            testPose.Joints.PinkyJoints[i].LocalRotation = pinkyJoints[i].localRotation;
        }
        
        EditorUtility.SetDirty(testPose);
        AssetDatabase.SaveAssets();
    }
}
