using System.Collections;
using System.Collections.Generic;
using Dive.VRModule;
using Oculus.Avatar2;
using UnityEngine;

public class MetaPoser : MonoBehaviour
{
    [SerializeField]
    private OvrAvatarEntity avatarEntity;
    
    [SerializeField]
    private OvrAvatarCustomHandPose customHandPose;

    [SerializeField]
    private PXRPose testPose;
    
    [SerializeField]
    private HandSide handSide = HandSide.Left;

    [SerializeField]
    protected Transform[] thumbJoints;

    [SerializeField]
    protected Transform[] indexJoints;

    [SerializeField]
    protected Transform[] middleJoints;

    [SerializeField]
    protected Transform[] ringJoints;

    [SerializeField]
    protected Transform[] pinkyJoints;

    private PXRPose currentPose = null;
    private PXRPose previousPose = null;

    public PXRPose CurrentPose
    {
        get => currentPose;
        set
        {
            previousPose = currentPose;
            currentPose = value;
            SetPose(value);
        }
    }

    public PXRPose PreviousPose
    {
        get => previousPose;
        set
        {
            previousPose = value;
            SetPreviousPose(value);
        }
    }

    public HandSide HandSide => handSide;
    public string CurrentPoseName => CurrentPose == null ? "" : CurrentPose.PoseName;

    public Transform[] ThumbJoints => thumbJoints;
    public Transform[] IndexJoints => indexJoints;
    public Transform[] MiddleJoints => middleJoints;
    public Transform[] RingJoints => ringJoints;
    public Transform[] PinkyJoints => pinkyJoints;

    [Sirenix.OdinInspector.Button]
    public void SetTestPose(PXRPose pose)
    {
        SetPose(pose);
    }
    
    public void SetPose(PXRPose pose)
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

    public void SetPreviousPose(PXRPose pose)
    {
    }
}