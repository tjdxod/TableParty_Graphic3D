#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using EasyButtons;

#endif

namespace Dive.VRModule
{
    public class PXRPoserCreator : MonoBehaviour
    {
        [SerializeField, LabelText("지원 플랫폼")]
        private SupportedPlatform supportPlatform = SupportedPlatform.None;

        [SerializeField, LabelText("손 위치")] 
        private HandSide handSide = HandSide.Left;

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

        private const string DefaultPoseName = "DefaultPose";
        private readonly int needLength = 3;

        
        [Space(10)]
        
        [SerializeField, LabelText("저장된 포즈"), Tooltip("마지막으로 저장한 Pose를 할당")]
        private PXRPose savedPose = null;
        
        [Button("Transform 자동 할당 (피코 Hand와 메타 Hand 전용")]
        private async void AutoHandAssign()
        {
            if (supportPlatform != SupportedPlatform.Meta && supportPlatform != SupportedPlatform.Pico)
                return;

            var isLeft = handSide == HandSide.Left;

            var transforms = GetComponentsInChildren<Transform>();

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

        [Button("현재 포즈 상태를 저장")]
        private void SavePose()
        {
            if (!IsValid(ThumbJoints, 4) || !IsValid(IndexJoints, needLength) || !IsValid(MiddleJoints, needLength) || !IsValid(RingJoints, needLength) || !IsValid(PinkyJoints, 4))
                return;
            
            var pose = ScriptableObject.CreateInstance<PXRPose>();

            pose.PoseName = DefaultPoseName;
            pose.Joints = new PXRPoseDefinition
            {
                ThumbJoints = new PXRFingerJoint[4],
                IndexJoints = new PXRFingerJoint[3],
                MiddleJoints = new PXRFingerJoint[3],
                RingJoints = new PXRFingerJoint[3],
                PinkyJoints = new PXRFingerJoint[4]
            };

            for (var i = 0; i < 3; i++)
            {
                pose.Joints.IndexJoints[i] = new PXRFingerJoint(IndexJoints[i]);
                pose.Joints.MiddleJoints[i] = new PXRFingerJoint(MiddleJoints[i]);
                pose.Joints.RingJoints[i] = new PXRFingerJoint(RingJoints[i]);
            }
            for (var i = 0; i < 4; i++)
            {
                pose.Joints.ThumbJoints[i] = new PXRFingerJoint(ThumbJoints[i]);
                pose.Joints.PinkyJoints[i] = new PXRFingerJoint(PinkyJoints[i]);
            }

            var path = EditorUtility.SaveFilePanelInProject("Save PXRPose", DefaultPoseName, "asset", "Please enter a file name to save the PXRPose");
            if (path.Length == 0)
                return;

            AssetDatabase.CreateAsset(pose, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            savedPose = pose;
        }

        private bool IsValid(Transform[] transforms, int length)
        {
            if (transforms == null)
                return false;

            if (transforms.Length != length)
                return false;

            return true;
        }
    }
}

#endif