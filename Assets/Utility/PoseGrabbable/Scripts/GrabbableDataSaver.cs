using System.Collections;
using System.Collections.Generic;
using Dive.VRModule;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(PXRGrabbable))]
public class GrabbableDataSaver : MonoBehaviour
{
    private PXRGrabbable grabbable;
    private Renderer rend;
    
    [Button("오브젝트 생성")]
    public void CreateRenderObject(GameObject renderObject)
    {
        if (renderObject == null)
        {
            Debug.LogError("RenderObject가 null입니다.");
            return;
        }        
        
        var child = transform.GetChild(0);

        var childCount = child.childCount;

        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(child.GetChild(i).gameObject);
            }
        }
        
        var newObject = Instantiate(renderObject, child);
        newObject.transform.localPosition = Vector3.zero;
        newObject.transform.localRotation = Quaternion.identity;

        var meshCollider = newObject.AddComponent<MeshCollider>();
        var meshFilter = newObject.GetComponent<MeshFilter>();
        
        if (meshFilter != null)
        {
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }
    }
    
    [Button("오브젝트 제거")]
    public void DeleteRenderObject()
    {
        var child = transform.GetChild(0);

        var childCount = child.childCount;

        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(child.GetChild(i).gameObject);
            }
        }
    }
    
    [Button("데이터 저장")]
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
}