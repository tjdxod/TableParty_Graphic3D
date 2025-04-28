#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using Dive.VRModule;
using Meta.XR.MultiplayerBlocks.Shared;
using Oculus.Avatar2;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public struct PoseVariable
{
    public HandPoseData.HandMode HandMode;
    public string posePath;
    public string meshPath;
    public string prefabPath;
    public bool afterCreated;
    public bool noObjectPose;
    public string poseName;
    public bool useMultiplePhoto;
    public float interval;
    public int photoCount;
    public float duration;
}

public class HandPoseData : MonoBehaviour
{
    public enum ColorMode
    {
        Ghost,
        Black,
        Green
    }

    public enum HandMode
    {
        Left,
        Right,
        Both
    }

    [SerializeField]
    private PoseAvatarEntity avatarEntity;

    [SerializeField]
    private Material[] materials;

    [SerializeField]
    private Renderer leftHandRenderer;

    [SerializeField]
    private Renderer rightHandRenderer;

    [SerializeField]
    private MetaPoser leftMetaPoser;

    [SerializeField]
    private MetaPoser rightMetaPoser;

    [SerializeField]
    private Transform target;

    [SerializeField, ReadOnly]
    private GameObject createdModel = null;

    [SerializeField]
    private MetaPoseCreator leftMetaPoseCreator;

    [SerializeField]
    private MetaPoseCreator rightMetaPoseCreator;

    [SerializeField]
    private OvrAvatarCustomHandPose leftCustomHandPose;

    [SerializeField]
    private OvrAvatarCustomHandPose rightCustomHandPose;

    private string currentAssetGuid;
    private IEnumerator routineSavePose = null;

    private void Awake()
    {
        avatarEntity.OnUserAvatarLoadedEvent.AddListener(ShowWindow);
    }

    private void OnDestroy()
    {
        avatarEntity.OnUserAvatarLoadedEvent.RemoveAllListeners();

        HandTrackingEditor.CloseWindow();
    }

    private void ShowWindow(OvrAvatarEntity entity)
    {
        HandTrackingEditor.ShowWindow();
    }

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
                leftHandRenderer.material = materials[0];
                rightHandRenderer.material = materials[0];
                break;
            case ColorMode.Black:
                leftHandRenderer.material = materials[1];
                rightHandRenderer.material = materials[1];
                break;
            case ColorMode.Green:
                leftHandRenderer.material = materials[2];
                rightHandRenderer.material = materials[2];
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

    public void SavePose(PoseVariable poseVariable)
    {
        if (!leftMetaPoser || !rightMetaPoseCreator)
        {
            Debug.LogError("MetaPoser or MetaPoseCreator is not assigned.");
            return;
        }

        if (routineSavePose != null)
        {
            StopCoroutine(routineSavePose);
            routineSavePose = null;
        }

        routineSavePose = SavePoseCoroutine(poseVariable);
        StartCoroutine(routineSavePose);
    }

    private IEnumerator SavePoseCoroutine(PoseVariable poseVariable)
    {
        if (!poseVariable.useMultiplePhoto)
        {
            var toInt = Mathf.FloorToInt(poseVariable.duration);
            var text = FindObjectOfType<TMP_Text>();

            var initInt = toInt;

            text.text = initInt.ToString();

            for (var i = 0; i < toInt; i++)
            {
                yield return new WaitForSeconds(1f);

                text.text = (initInt - i - 1).ToString();
            }

            text.text = "0";

            yield return new WaitForSeconds(0.5f);

            string modelName = string.Empty;

            if (!poseVariable.noObjectPose)
                modelName = createdModel.name.Replace("(Clone)", "");

            if (poseVariable.HandMode == HandMode.Left)
            {
                leftMetaPoseCreator.SavePose(modelName, poseVariable.posePath, poseVariable.meshPath, poseVariable.prefabPath,
                    poseVariable.afterCreated, poseVariable.noObjectPose, poseVariable.poseName);
            }
            else if (poseVariable.HandMode == HandMode.Right)
            {
                rightMetaPoseCreator.SavePose(modelName, poseVariable.posePath, poseVariable.meshPath, poseVariable.prefabPath,
                    poseVariable.afterCreated, poseVariable.noObjectPose, poseVariable.poseName);
            }
            else if (poseVariable.HandMode == HandMode.Both)
            {
                leftMetaPoseCreator.SavePose(modelName, poseVariable.posePath, poseVariable.meshPath, poseVariable.prefabPath,
                    poseVariable.afterCreated, poseVariable.noObjectPose, poseVariable.poseName);

                rightMetaPoseCreator.SavePose(modelName, poseVariable.posePath, poseVariable.meshPath, poseVariable.prefabPath,
                    poseVariable.afterCreated, poseVariable.noObjectPose, poseVariable.poseName);
            }
        }
        else
        {
            for (var i = 0; i < poseVariable.photoCount; i++)
            {
                var toInt = Mathf.FloorToInt(poseVariable.interval);
                var text = FindObjectOfType<TMP_Text>();

                var initInt = toInt;

                text.text = initInt.ToString();

                for (var j = 0; j < toInt; j++)
                {
                    yield return new WaitForSeconds(1f);

                    text.text = (initInt - j - 1).ToString();
                }

                text.text = "0";

                yield return new WaitForSeconds(0.5f);

                string modelName = string.Empty;

                if (!poseVariable.noObjectPose)
                    modelName = createdModel.name.Replace("(Clone)", "");

                if (poseVariable.HandMode == HandMode.Left)
                {
                    leftMetaPoseCreator.SavePose(modelName, poseVariable.posePath, poseVariable.meshPath, poseVariable.prefabPath,
                        poseVariable.afterCreated, poseVariable.noObjectPose, poseVariable.poseName);
                }
                else if (poseVariable.HandMode == HandMode.Right)
                {
                    rightMetaPoseCreator.SavePose(modelName, poseVariable.posePath, poseVariable.meshPath, poseVariable.prefabPath,
                        poseVariable.afterCreated, poseVariable.noObjectPose, poseVariable.poseName);
                }
                else if (poseVariable.HandMode == HandMode.Both)
                {
                    leftMetaPoseCreator.SavePose(modelName, poseVariable.posePath, poseVariable.meshPath, poseVariable.prefabPath,
                        poseVariable.afterCreated, poseVariable.noObjectPose, poseVariable.poseName);

                    rightMetaPoseCreator.SavePose(modelName, poseVariable.posePath, poseVariable.meshPath, poseVariable.prefabPath,
                        poseVariable.afterCreated, poseVariable.noObjectPose, poseVariable.poseName);
                }


                if (i == poseVariable.photoCount - 1)
                {
                    text.text = string.Empty;
                }
            }
        }
    }

    public void SetTestPose(PXRPose pose, HandMode handMode)
    {
        if (!leftMetaPoser || !leftCustomHandPose)
        {
            Debug.LogError("MetaPoser or CustomHandPose is not assigned.");
            return;
        }

        if (handMode == HandMode.Left)
        {
            leftCustomHandPose.setHandPose = pose;
            leftMetaPoser.SetTestPose(pose);
        }
        else if (handMode == HandMode.Right)
        {
            rightCustomHandPose.setHandPose = pose;
            rightMetaPoser.SetTestPose(pose);
        }
        else if (handMode == HandMode.Both)
        {
            leftCustomHandPose.setHandPose = pose;
            rightCustomHandPose.setHandPose = pose;
            leftMetaPoser.SetTestPose(pose);
            rightMetaPoser.SetTestPose(pose);
        }
    }

    public void ResetPose()
    {
        if (!leftMetaPoser || !leftCustomHandPose)
        {
            Debug.LogError("MetaPoser or CustomHandPose is not assigned.");
            return;
        }

        leftCustomHandPose.setHandPose = false;
        leftMetaPoser.SetTestPose(null);
        rightCustomHandPose.setHandPose = false;
        rightMetaPoser.SetTestPose(null);
    }
}

#endif