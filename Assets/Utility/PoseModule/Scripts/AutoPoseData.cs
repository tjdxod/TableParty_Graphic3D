#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BNG;
using DG.Tweening;
using Dive.Utility.UnityExtensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class AutoPoseData : MonoBehaviour
{
    public enum ColorMode
    {
        Ghost,
        Black,
        Green
    }

    public enum RotateMode
    {
        X,
        Y,
        Z
    }

    [SerializeField]
    private Material[] materials;

    [SerializeField]
    private Renderer handRenderer;

    [SerializeField]
    private Transform target;

    [SerializeField, ReadOnly]
    private GameObject createdModel = null;

    [SerializeField]
    private Transform rotate;

    [SerializeField]
    private HandPoser handPoser;

    [SerializeField]
    private AutoPoser autoPoser;

    [SerializeField]
    private HandPose openOriginalPose;

    [SerializeField]
    private HandPose closedOriginalPose;

    [SerializeField]
    private HandPose idleOriginalPose;

    private string currentAssetGuid;

    public (bool, string) CheckInit()
    {
        var hasTarget = GameObject.FindGameObjectWithTag("Target");

        createdModel = hasTarget != null ? hasTarget : null;

        if (hasTarget == null)
            return (false, "");

        var path = AssetDatabase.GUIDToAssetPath(currentAssetGuid);

        return (true, path);
    }

    public void SetColorMaterial(ColorMode mode)
    {
        switch (mode)
        {
            case ColorMode.Ghost:
                handRenderer.material = materials[0];
                break;
            case ColorMode.Black:
                handRenderer.material = materials[1];
                break;
            case ColorMode.Green:
                handRenderer.material = materials[2];
                break;
        }
    }

    public void CreateModel(GameObject model)
    {
        if (model == null)
        {
            Debug.LogError("Model is null");
            return;
        }

        if (createdModel != null)
        {
            Destroy(createdModel);
            createdModel = null;
        }

        currentAssetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(model));

        createdModel = Instantiate(model, target);
        createdModel.transform.localPosition = Vector3.zero;
        createdModel.transform.localRotation = Quaternion.identity;
        createdModel.transform.localScale = model.transform.localScale;

        createdModel.tag = "Target";

        var transforms = createdModel.GetComponentsInChildren<Transform>();

        foreach (var tr in transforms)
        {
            tr.gameObject.layer = LayerMask.NameToLayer("Interactable");
        }

        var renderers = createdModel.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            var meshCollider = renderer.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
        }

        createdModel.transform.parent = null;

        // selected created model
        Selection.activeGameObject = createdModel;
    }

    public void DeleteModel()
    {
        if (createdModel == null)
            return;

        DestroyImmediate(createdModel);
        createdModel = null;
        currentAssetGuid = "";
    }

    public void SavePose(string posePath, string meshPath, string prefabPath)
    {
        if (Directory.Exists(posePath) == false)
        {
            Directory.CreateDirectory(posePath);
        }
        
        var poseObject = handPoser.GetHandPoseScriptableObject();

        var modelName = createdModel.name.Replace("(Clone)", "");
        
        var saveName = $"{modelName}_Right_{DateTime.Now:yyMMdd}_01";

        var path = Path.Combine(posePath, $"{saveName}.asset");

        var index = 2;

        // Check if the file already exists
        while (File.Exists(path))
        {
            // If it does, increment the number and try again
            var toString = index < 10 ? $"0{index}" : index.ToString();

            saveName = $"{modelName}_Right_{DateTime.Now:yyMMdd}_{toString}";
            
            path = Path.Combine(posePath, $"{saveName}.asset");

            index++;

            // You can also add a check to limit the number of attempts to avoid an infinite loop
            if (index >= 100)
            {
                Debug.LogError("Too many attempts to find a unique file name.");
                return;
            }
        }

        poseObject.PoseName = modelName;
        
        // Creates the file in the folder path
        AssetDatabase.CreateAsset(poseObject, path);
        AssetDatabase.SaveAssets();

        // As we are saving to the asset folder, tell Unity to scan for modified or new assets
        AssetDatabase.Refresh();

        // Set as active in the hierarchy
        // Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        handPoser.CurrentPose = poseObject;

        var skinned = autoPoser.GetComponentInChildren<SkinnedMeshRenderer>();
        ConvertSkinnedMeshToMesh(skinned, meshPath, prefabPath, saveName);
    }

    public void SetPose(HandPose handPose)
    {
        autoPoser.OpenHandPose = handPose;
        autoPoser.ClosedHandPose = handPose;
        autoPoser.IdleHandPose = handPose;
    }

    public void ResetPose()
    {
        if (openOriginalPose != null)
        {
            autoPoser.OpenHandPose = openOriginalPose;
        }

        if (closedOriginalPose != null)
        {
            autoPoser.ClosedHandPose = closedOriginalPose;
        }

        if (idleOriginalPose != null)
        {
            autoPoser.IdleHandPose = idleOriginalPose;
        }
    }

    public void UseAutoHand(bool useAutoHand)
    {
        autoPoser.UpdateContinuously = useAutoHand;
    }

    private SkinnedMeshRenderer skinnedMeshRenderer;

    [Button]
    public void ConvertSkinnedMeshToMesh(SkinnedMeshRenderer skinned, string meshPath, string prefabPath, string saveName)
    {
        Mesh mesh = null;
        if (skinned != null)
        {
            mesh = new Mesh();
            skinned.BakeMesh(mesh);
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
        meshRenderer.materials = skinned.sharedMaterials;

        obj.transform.position = skinned.transform.position;
        obj.transform.rotation = skinned.transform.rotation;

        if (Directory.Exists(meshPath) == false)
        {
            Directory.CreateDirectory(meshPath);
        }

        // save mesh
        string savedMeshPath = Path.Combine(meshPath, $"{saveName}.asset");
        AssetDatabase.CreateAsset(mesh, savedMeshPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (Directory.Exists(prefabPath) == false)
        {
            Directory.CreateDirectory(prefabPath);
        }
        
        // save prefab
        string savedPrefabPath = Path.Combine(prefabPath, $"{saveName}.prefab");
        PrefabUtility.SaveAsPrefabAsset(obj, savedPrefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

#endif