#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using BNG;
using DG.Tweening;
using Dive.Utility.UnityExtensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class PoseData : MonoBehaviour
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

        if(createdModel != null)
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

    public void SavePose()
    {
        HandPoseSaveAs window = (HandPoseSaveAs)EditorWindow.GetWindow(typeof(HandPoseSaveAs));
        window.ShowWindow(handPoser);
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

    public void CombinePose(string path)
    {
        var toMesh = FindObjectOfType<SkinnedMeshToMesh>();
        if (toMesh == null)
        {
            Debug.LogError("SkinnedMeshToMesh is null");
            return;
        }
        
        toMesh.ConvertSkinnedMeshToMesh(path);
    }

    public void UseAutoHand(bool useAutoHand)
    {
        autoPoser.UpdateContinuously = useAutoHand;
    }
}

#endif
