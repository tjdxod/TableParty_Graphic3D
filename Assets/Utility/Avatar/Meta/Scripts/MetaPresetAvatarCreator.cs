#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using Dive.Avatar.Meta;
using Dive.Utility;
using UnityEngine;

public class MetaPresetAvatarCreator : MonoBehaviour
{
    private static StaticVar<MetaPresetAvatarCreator> instance;

    public static MetaPresetAvatarCreator Instance
    {
        get
        {
            if (instance != null && instance.Value != null)
                return instance.Value;

            var creator = FindObjectOfType<MetaPresetAvatarCreator>();

            if (creator == null)
                return null;

            instance = new StaticVar<MetaPresetAvatarCreator>(creator);

            return instance.Value;
        }
    }
    
    [SerializeField, Range(0, 31)]
    private int presetAvatarIndex = 0;
    
    [SerializeField]
    private PXRMetaPresetAvatarBase aiAvatarPrefab;
    
    public PXRMetaPresetAvatarBase AiAvatarPrefab => aiAvatarPrefab;
    public int PresetAvatarIndex => presetAvatarIndex;
    
    public void SetAvatarIndex(int index)
    {
        if(index is < 0 or > 31)
        {
            Debug.LogError("Invalid index");
            return;
        }
        
        presetAvatarIndex = index;
    }
}

#endif