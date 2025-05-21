using System.Collections;
using System.Collections.Generic;
using Dive.VRModule;
using Oculus.Avatar2;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(PXRGrabbable))]
public class GrabbableDataSaver : MonoBehaviour
{
    private PXRGrabbable grabbable;
    private Renderer rend;

    private GameObject newObj;
    
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => FindObjectOfType<SampleAvatarEntity>().IsCreated);
        
        var avatar = FindObjectOfType<SampleAvatarEntity>();
        
        yield return new WaitUntil(() => avatar.GetSkeletonTransform(CAPI.ovrAvatar2JointType.RightHandWrist) != null);
        
        var wrist = avatar.GetSkeletonTransform(CAPI.ovrAvatar2JointType.RightHandWrist);
        
        newObj = new GameObject("Grabber");
        newObj.transform.SetParent(wrist);
        
        newObj.transform.localPosition = new Vector3(-0.08203185f, -0.04675849f, -0.008599499f);
        newObj.transform.localRotation = Quaternion.Euler(2.754f, -102.733f, 80.009f);
        newObj.transform.localScale = Vector3.one;
    }
    
    [BoxGroup("데이터 저장"), Button("오브젝트 생성")]
    public void CreateRenderObject(GameObject renderObject)
    {
        if (renderObject == null)
        {
            Debug.LogError("RenderObject가 null입니다.");
            return;
        }        
        
        var childCount = transform.childCount;

        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        
        var newObject = Instantiate(renderObject, transform);
        newObject.transform.localPosition = Vector3.zero;
        newObject.transform.localRotation = Quaternion.identity;
        
        var meshCollider = newObject.AddComponent<MeshCollider>();
        var meshFilter = newObject.GetComponent<MeshFilter>();
        newObject.AddComponent<PXRGrabbableChild>();
        
        if (meshFilter != null)
        {
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }
        else
        {
            Debug.LogError("MeshFilter가 null입니다.");
            return;
        }
    }

    [BoxGroup("데이터 저장"), Button("차일드 화")]
    public void ChildNewObj()
    {
        if (newObj == null)
        {
            Debug.LogError("newObj가 null입니다.");
            return;
        }
        
        // 하이어라키에서 오브젝트 선택
        transform.SetParent(newObj.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    [BoxGroup("데이터 저장"), Button("차일드화 해제")]
    public void UnchildNewObj() 
    {
        if (newObj == null)
        {
            Debug.LogError("newObj가 null입니다.");
            return;
        }
        
        // 하이어라키에서 오브젝트 선택
        transform.SetParent(null);
    }
    
    [BoxGroup("데이터 저장"), Button("포즈 적용")]
    public void SetPose(PXRPose pose)
    {
        var avatar = FindObjectOfType<SampleAvatarEntity>();
        var customPoses = avatar.GetComponentsInChildren<PXRMetaCustomHand>(true);
        var rightPose = customPoses.Find(x => x.HandSide == HandSide.Right);
        
        if (rightPose == null)
        {
            Debug.LogError("RightHandPose가 null입니다.");
            return;
        }
        
        if (pose == null)
        {
            Debug.LogError("Pose가 null입니다.");
            return;
        }
        
        var parent = rightPose.transform.parent;

        if (parent.name == "MyAvatar")
        {
            rightPose.transform.SetParent(parent.GetChild(2));
            rightPose.transform.localPosition = Vector3.zero;
            rightPose.transform.localRotation = Quaternion.Euler(0, 180, 0);
            rightPose.testPose = pose;
            rightPose.gameObject.SetActive(true);
        }
        
        rightPose.SetPose(pose);

        grabbable = GetComponent<PXRGrabbable>();
        
        grabbable.GrabbedPXRPose = pose;
    }
    
    [BoxGroup("데이터 저장"), Button("오브젝트 제거")]
    public void DeleteRenderObject()
    {
        var childCount = transform.childCount;

        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
    
    
    [BoxGroup("데이터 저장"), Button("데이터 적용")]
    public void ApplyGrabbableData()
    {
        grabbable = GetComponent<PXRGrabbable>();

        if (grabbable == null)
        {
            Debug.LogError("PXRGrabbable이 null입니다.");
            return;
        }
        
        grabbable.OverrideLocalPosition = transform.localPosition;
        grabbable.OverrideLocalRotation = transform.localEulerAngles;
    }
    
    
    [BoxGroup("데이터 저장"), Button("데이터 파일로 저장")]
    public void SaveGrabbableData()
    {
        Debug.Log("Grabbable data saved.");

        grabbable = GetComponent<PXRGrabbable>();
        
        if (grabbable.AttachGrabbableState != AttachGrabbableState.AttachedWithPose)
        {
            Debug.LogWarning($"그랩 부착 상태가 {grabbable.AttachGrabbableState}입니다.");
            return;
        }

        if (grabbable.OverrideTransformState != OverrideTransformState.Both)
        {
            Debug.LogWarning($"지정 위치 이동 및 회전 상태가 {grabbable.OverrideTransformState}입니다.");
            return;
        }
        
        if (grabbable.GrabbedPXRPose == null)
        {
            Debug.LogWarning("Pose가 null입니다.");
            return;
        }
        
        var grabbableData = ScriptableObject.CreateInstance<GrabbableData>();
        grabbableData.Pose = grabbable.GrabbedPXRPose;
        grabbableData.nativePosition = grabbable.OverrideLocalPosition;
        grabbableData.nativeRotation = grabbable.OverrideLocalRotation;
        grabbableData.metaPosition = grabbable.MetaOverrideLocalPosition;
        grabbableData.metaRotation = grabbable.MetaOverrideLocalRotation;
        grabbableData.picoPosition = grabbable.PicoOverrideLocalPosition;
        grabbableData.picoRotation = grabbable.PicoOverrideLocalRotation;
        
        var path = EditorUtility.SaveFilePanelInProject("Save GrabbableData", "GrabbableData", 
            "asset", "Please enter a file name to save the GrabbableData");

        if (path.Length == 0)
        {
            Debug.LogWarning("파일 경로가 비어있습니다.");
            return;
        }
        
        AssetDatabase.CreateAsset(grabbableData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    [BoxGroup("데이터 확인"), Button("그래버블 데이터로 위치 적용")]
    public void SetGrabbableData(GrabbableData grabbableData)
    {
        if (grabbableData == null)
        {
            Debug.LogError("GrabbableData가 null입니다.");
            return;
        }
        
        grabbable = GetComponent<PXRGrabbable>();
        
        if (grabbable == null)
        {
            Debug.LogError("PXRGrabbable이 null입니다.");
            return;
        }
        
        var pose = grabbableData.Pose;
        if (pose == null)
        {
            Debug.LogError("Pose가 null입니다.");
            return;
        }
        
        grabbable.GrabbedPXRPose = pose;
        
        grabbable.OverrideLocalPosition = grabbableData.nativePosition;
        grabbable.OverrideLocalRotation = grabbableData.nativeRotation;
    }

    [BoxGroup("데이터 확인"), Button("그래버블 데이터 수치 리셋")]
    public void ResetGrabbableData()
    {
        grabbable = GetComponent<PXRGrabbable>();
        
        if (grabbable == null)
        {
            Debug.LogError("PXRGrabbable이 null입니다.");
            return;
        }
        
        grabbable.GrabbedPXRPose = null;
        grabbable.OverrideLocalPosition = Vector3.zero;
        grabbable.OverrideLocalRotation = Vector3.zero;
    }
}