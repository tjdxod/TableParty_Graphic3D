using System.Collections;
using System.Collections.Generic;
using Dive.VRModule;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class MetaPoserCopy : MonoBehaviour
{
    public PXRPose currentPose;

    [SerializeField]
    private Transform targetHand;

    [FormerlySerializedAs("handSide")]
    [SerializeField, LabelText("손 위치")]
    private HandPoseData.HandMode handMode = HandPoseData.HandMode.Left;

    [Space(10)]
    [SerializeField, LabelText("엄지 손가락 관절")]
    private Transform[] ThumbJoints;

    [SerializeField, LabelText("검지 손가락 관절")]
    private Transform[] IndexJoints;

    [SerializeField, LabelText("중지 손가락 관절")]
    private Transform[] MiddleJoints;

    [SerializeField, LabelText("약지 손가락 관절")]
    private Transform[] RingJoints;

    [SerializeField, LabelText("소지 손가락 관절")]
    private Transform[] PinkyJoints;

    [Button("Transform 자동 할당")]
    private async void AutoHandAssign()
    {
        var isLeft = handMode == HandPoseData.HandMode.Left;

        var transforms = targetHand.GetComponentsInChildren<Transform>();

        ThumbJoints = new Transform[4];
        IndexJoints = new Transform[3];
        MiddleJoints = new Transform[3];
        RingJoints = new Transform[3];
        PinkyJoints = new Transform[4];

        ThumbJoints[0] = transforms.Find(t => t.name == (isLeft ? "b_l_thumb0" : "b_r_thumb0"));
        ThumbJoints[1] = transforms.Find(t => t.name == (isLeft ? "b_l_thumb1" : "b_r_thumb1"));
        ThumbJoints[2] = transforms.Find(t => t.name == (isLeft ? "b_l_thumb2" : "b_r_thumb2"));
        ThumbJoints[3] = transforms.Find(t => t.name == (isLeft ? "b_l_thumb3" : "b_r_thumb3"));

        IndexJoints[0] = transforms.Find(t => t.name == (isLeft ? "b_l_index1" : "b_r_index1"));
        IndexJoints[1] = transforms.Find(t => t.name == (isLeft ? "b_l_index2" : "b_r_index2"));
        IndexJoints[2] = transforms.Find(t => t.name == (isLeft ? "b_l_index3" : "b_r_index3"));

        MiddleJoints[0] = transforms.Find(t => t.name == (isLeft ? "b_l_middle1" : "b_r_middle1"));
        MiddleJoints[1] = transforms.Find(t => t.name == (isLeft ? "b_l_middle2" : "b_r_middle2"));
        MiddleJoints[2] = transforms.Find(t => t.name == (isLeft ? "b_l_middle3" : "b_r_middle3"));

        RingJoints[0] = transforms.Find(t => t.name == (isLeft ? "b_l_ring1" : "b_r_ring1"));
        RingJoints[1] = transforms.Find(t => t.name == (isLeft ? "b_l_ring2" : "b_r_ring2"));
        RingJoints[2] = transforms.Find(t => t.name == (isLeft ? "b_l_ring3" : "b_r_ring3"));

        PinkyJoints[0] = transforms.Find(t => t.name == (isLeft ? "b_l_pinky0" : "b_r_pinky0"));
        PinkyJoints[1] = transforms.Find(t => t.name == (isLeft ? "b_l_pinky1" : "b_r_pinky1"));
        PinkyJoints[2] = transforms.Find(t => t.name == (isLeft ? "b_l_pinky2" : "b_r_pinky2"));
        PinkyJoints[3] = transforms.Find(t => t.name == (isLeft ? "b_l_pinky3" : "b_r_pinky3"));
    }
    
    [Button("포즈 적용")]
    private void SetPose()
    {
        if (currentPose == null)
            return;

        for (var i = 0; i < 3; i++)
        {
            IndexJoints[i].localRotation = currentPose.Joints.IndexJoints[i].LocalRotation;
            MiddleJoints[i].localRotation = currentPose.Joints.MiddleJoints[i].LocalRotation;
            RingJoints[i].localRotation = currentPose.Joints.RingJoints[i].LocalRotation;
        }

        for (var i = 0; i < 4; i++)
        {
            ThumbJoints[i].localRotation = currentPose.Joints.ThumbJoints[i].LocalRotation;
            PinkyJoints[i].localRotation = currentPose.Joints.PinkyJoints[i].LocalRotation;
        }
    }
    
    [PropertySpace(50)]
    [Button("포즈 저장 (되돌릴수 없으니 주의)")]
    private void SavePose()
    {
        if (currentPose == null)
            return;
        
        currentPose.Joints = new PXRPoseDefinition
        {
            ThumbJoints = new PXRFingerJoint[4],
            IndexJoints = new PXRFingerJoint[3],
            MiddleJoints = new PXRFingerJoint[3],
            RingJoints = new PXRFingerJoint[3],
            PinkyJoints = new PXRFingerJoint[4]
        };
        
        for (var i = 0; i < 3; i++)
        {
            currentPose.Joints.IndexJoints[i] = new PXRFingerJoint(IndexJoints[i]);
            currentPose.Joints.MiddleJoints[i] = new PXRFingerJoint(MiddleJoints[i]);
            currentPose.Joints.RingJoints[i] = new PXRFingerJoint(RingJoints[i]);
        }
        
        for (var i = 0; i < 4; i++)
        {
            currentPose.Joints.ThumbJoints[i] = new PXRFingerJoint(ThumbJoints[i]);
            currentPose.Joints.PinkyJoints[i] = new PXRFingerJoint(PinkyJoints[i]);
        }
    }
}