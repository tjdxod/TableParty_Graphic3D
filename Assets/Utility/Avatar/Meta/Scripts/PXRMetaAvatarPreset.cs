#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dive.Avatar.Meta;
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.Rendering;

#pragma warning disable CS0618 // Type or member is obsolete
public class PXRMetaAvatarPreset : PXRMetaAvatarEntityBase
{
    protected override void Awake()
    {
        base.Awake();

        isOwner = false;
        _userId = 0;
        
        SetPresets(MetaPresetAvatarCreator.Instance.PresetAvatarIndex);
        
        isCreated = true;
        
        OnUserAvatarLoadedEvent.AddListener(ChangeAvatarMaterial);
    }

    protected override void OnDestroyCalled()
    {
        OnUserAvatarLoadedEvent.RemoveAllListeners();
    }

    private void ChangeAvatarMaterial(OvrAvatarEntity entity)
    {
        // var currentSceneIndex = TablePartyController.Instance.CurrentRoomIndex;
        // var avatarMaterial = Setting.Room.GetAvatarMaterial(currentSceneIndex);

        ChangeLightProbe().Forget();
        
        // entity.SetMaterialProperties((callback) =>
        // {
        //     callback.SetFloat("u_Exposure", avatarMaterial.materialExposure);
        //     callback.SetFloat("u_OcclusionStrength", avatarMaterial.occlusionStrength);
        // });
    }
    
    private async UniTask ChangeLightProbe()
    {
        await UniTask.WaitUntil(() => gameObject.GetComponentsInChildren<MeshRenderer>(true).Length > 0);
        
        var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
        
        foreach (var meshRenderer in meshRenderers)
        {
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        }
    }
}

#endif
