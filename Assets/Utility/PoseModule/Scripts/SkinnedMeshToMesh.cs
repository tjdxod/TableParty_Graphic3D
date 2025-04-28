#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class SkinnedMeshToMesh : MonoBehaviour
{
    private readonly string defaultPath = "Assets/CombineMesh";
    
    private SkinnedMeshRenderer skinnedMeshRenderer;

    [Button]
    public void ConvertSkinnedMeshToMesh(string targetPath = "")
    {
        if (skinnedMeshRenderer == null)
        {
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
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
        
        targetPath = string.IsNullOrEmpty(targetPath) ? defaultPath : targetPath;
        
        if(Directory.Exists(targetPath) == false)
        {
            Directory.CreateDirectory(targetPath);
        }
        
        // save mesh
        string path = Path.Combine(targetPath, $"{gameObject.name}_BakedMesh.asset");
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

#endif