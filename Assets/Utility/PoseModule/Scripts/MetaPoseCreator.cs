using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dive.VRModule;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class MetaPoseCreator : MonoBehaviour
{
    [SerializeField]
    private Transform targetHand;
    
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

    [Space(10)]
        
    [SerializeField, LabelText("저장된 포즈"), Tooltip("마지막으로 저장한 Pose를 할당")]
    private PXRPose savedPose = null;
    
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private readonly string defaultPath = "Assets/CombineMesh";
    private readonly int needLength = 3;
    

    [Button("Transform 자동 할당")]
    private async void AutoHandAssign()
    {
        var isLeft = handSide == HandSide.Left;

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
    
    [Button("현재 포즈 상태를 저장")]
    public void SavePose(string objName, string posePath, string meshPath, string prefabPath, bool afterCreated, bool noObjectPose, string poseName)
    {
        if (!IsValid(ThumbJoints, 4) || !IsValid(IndexJoints, needLength) || !IsValid(MiddleJoints, needLength) || !IsValid(RingJoints, needLength) || !IsValid(PinkyJoints, 4))
            return;
            
        var pose = ScriptableObject.CreateInstance<PXRPose>();
        
        pose.Joints = new PXRPoseDefinition()
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

        if(Directory.Exists(posePath) == false)
        {
            Directory.CreateDirectory(posePath);
        }
        
        var modelName = noObjectPose ? poseName : objName;
        pose.PoseName = $"{modelName}";
        var side = handSide == HandSide.Left ? "Left" : "Right";
        var date = DateTime.Now.ToString("yyMMdd");
        
        var savedName = $"{modelName}_{side}_{date}_01";
        
        var savePosePath = Path.Combine(posePath, $"{savedName}.asset");

        var index = 2;
        
        while(File.Exists(savePosePath))
        {
            var toString = index < 10 ? $"0{index}" : index.ToString();

            savedName = $"{modelName}_{side}_{date}_{toString}";
            
            index++;
            savePosePath = Path.Combine(posePath, $"{savedName}.asset");
            
            if (index >= 100)
            {
                Debug.LogError("Too many attempts to find a unique file name.");
                return;
            }
        }
        
        AssetDatabase.CreateAsset(pose, savePosePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ConvertSkinnedMeshToMesh(savedName, meshPath, prefabPath, afterCreated);
        
        savedPose = pose;
    }
    
    public void ConvertSkinnedMeshToMesh(string savedName, string meshPath, string prefabPath, bool afterCreated)
    {
        if (skinnedMeshRenderer == null)
        {
            skinnedMeshRenderer = targetHand.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        Mesh mesh = null;
        if (skinnedMeshRenderer != null)
        {
            mesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(mesh);
        }
        else
        {
            Debug.LogError("No SkinnedMeshRenderer found on this GameObject.");
            return;
        }

        GameObject obj = new GameObject("BakedMesh");
        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.materials = skinnedMeshRenderer.sharedMaterials;

        obj.transform.position = skinnedMeshRenderer.transform.position;
        obj.transform.rotation = skinnedMeshRenderer.transform.rotation;
        
        var grabber = new GameObject("Grabber");
        grabber.transform.SetParent(obj.transform);

        grabber.transform.localPosition = new Vector3(-0.082031f, -0.046758f, -0.008599f);
        grabber.transform.localRotation = Quaternion.Euler(2.754f, -102.733f, 80.009f);
        
        if(Directory.Exists(meshPath) == false)
        {
            Directory.CreateDirectory(meshPath);
        }
        
        // save mesh
        string savedMeshPath = Path.Combine(meshPath, $"{savedName}.asset");
        
        AssetDatabase.CreateAsset(mesh, savedMeshPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        if(Directory.Exists(prefabPath) == false)
        {
            Directory.CreateDirectory(prefabPath);
        }
        
        // save prefab
        string savedPrefabPath = Path.Combine(prefabPath, $"{savedName}.prefab");

        PrefabUtility.SaveAsPrefabAsset(obj, savedPrefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        if (!afterCreated)
        {
            DestroyImmediate(obj);
        }
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