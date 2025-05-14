#if DIVE_PLATFORM_META || DIVE_PLATFORM_STEAM

using System;
using Newtonsoft.Json;
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dive.Avatar.Meta
{
    public class PXRMetaPresetAvatarBase : OvrAvatarEntity
    {
        [System.Serializable]
        public struct AssetData
        {
            public AssetSource source;
            public string path;
        }

        [Header("Assets")]
        [SerializeField]
        public AssetData assetData = new AssetData {source = AssetSource.Zip, path = "0"};

        [SerializeField]
        private bool createWithStartMethod = false;

        private bool isCreated = false;
        private bool underscorePostfix = true;
        private string overridePostfix = string.Empty;
        
        [SerializeField]
        private string streamStandDataPath = "Stand_Medium";

        [SerializeField]
        private string streamSitDataPath = "Sit_Medium";

        private string currentDataPath = "Stand_Medium";
        
        protected override void Awake()
        {
            base.Awake();
            CreateEntity();
            OnSkeletonLoadedEvent.AddListener(SetStreamData);
            OnSkeletonLoadedEvent.AddListener(AddCollider);
        }

        protected virtual void Start()
        {
            if (createWithStartMethod)
            {
                LoadLocalAvatar();
            }
        }

        private void AddCollider(OvrAvatarEntity entity)
        {
            var joints = entity.GetCriticalJoints();

            foreach (var joint in joints)
            {
                var skeleton = entity.GetSkeletonTransform(joint);
                skeleton.gameObject.layer = LayerMask.NameToLayer("Ignore Avatar");

                switch (joint)
                {
                    case CAPI.ovrAvatar2JointType.Head:
                        var head = skeleton.gameObject.AddComponent<CapsuleCollider>();
                        head.direction = 0;
                        head.center = new Vector3(0.07304f, 0.01928f, 0);
                        head.radius = 0.1272f;
                        head.height = 0.3382258f;
                        break;
                    case CAPI.ovrAvatar2JointType.Chest:
                        var chest = skeleton.gameObject.AddComponent<CapsuleCollider>();
                        chest.direction = 0;
                        chest.center = new Vector3(-0.09529f, 0f, 0.01235f);
                        chest.radius = 0.19061f;
                        chest.height = 0.56031f;
                        break;
                    case CAPI.ovrAvatar2JointType.LeftHandWrist:
                        var leftHandWrist = skeleton.gameObject.AddComponent<CapsuleCollider>();
                        leftHandWrist.direction = 0;
                        leftHandWrist.center = new Vector3(0.07593f, 0.01549f, -0.0012f);
                        leftHandWrist.radius = 0.04546f;
                        leftHandWrist.height = 0.16433f;
                        break;
                    case CAPI.ovrAvatar2JointType.LeftArmLower:
                        var leftArmLower = skeleton.gameObject.AddComponent<CapsuleCollider>();
                        leftArmLower.direction = 0;
                        leftArmLower.center = new Vector3(0.12349f, 0.00594f, 0.00821f);
                        leftArmLower.radius = 0.06732f;
                        leftArmLower.height = 0.26424f;
                        break;
                    case CAPI.ovrAvatar2JointType.LeftArmUpper:
                        var leftArmUpper = skeleton.gameObject.AddComponent<CapsuleCollider>();
                        leftArmUpper.direction = 0;
                        leftArmUpper.center = new Vector3(0.12349f, 0, 0);
                        leftArmUpper.radius = 0.07f;
                        leftArmUpper.height = 0.26424f;
                        break;
                    case CAPI.ovrAvatar2JointType.RightHandWrist:
                        var rightHandWrist = skeleton.gameObject.AddComponent<CapsuleCollider>();
                        rightHandWrist.direction = 0;
                        rightHandWrist.center = new Vector3(-0.07593f, -0.01549f, 0.0012f);
                        rightHandWrist.radius = 0.04546f;
                        rightHandWrist.height = 0.16433f;
                        break;
                    case CAPI.ovrAvatar2JointType.RightArmLower:
                        var rightArmLower = skeleton.gameObject.AddComponent<CapsuleCollider>();
                        rightArmLower.direction = 0;
                        rightArmLower.center = new Vector3(-0.12349f, -0.00594f, -0.00821f);
                        rightArmLower.radius = 0.06732f;
                        rightArmLower.height = 0.26424f;
                        break;
                    case CAPI.ovrAvatar2JointType.RightArmUpper:
                        var rightArmUpper = skeleton.gameObject.AddComponent<CapsuleCollider>();
                        rightArmUpper.direction = 0;
                        rightArmUpper.center = new Vector3(-0.12349f, 0, 0);
                        rightArmUpper.radius = 0.07f;
                        rightArmUpper.height = 0.26424f;
                        break;
                    default:
                        break;
                }
            }
        }

        public void CreatePreset()
        {
            LoadLocalAvatar();
        }
        
        public void SetStreamDataPath(bool isStand)
        {
            currentDataPath = isStand ? streamStandDataPath : streamSitDataPath;
        }

        private void LoadLocalAvatar()
        {
            if (isCreated)
                return;

            isCreated = true;

            var isFromZip = (assetData.source == AssetSource.Zip);

            var assetPostfix = GetAssetPostfix(isFromZip);

            var assetPath = $"{assetData.path}{assetPostfix}";
            LoadAssets(new[] {assetPath}, assetData.source);
        }

        private string GetAssetPostfix(bool isFromZip)
        {
            var assetPostfix = (underscorePostfix ? "_" : "")
                               + OvrAvatarManager.Instance.GetPlatformGLBPostfix(_creationInfo.renderFilters.quality, isFromZip)
                               + OvrAvatarManager.Instance.GetPlatformGLBVersion(_creationInfo.renderFilters.quality, isFromZip)
                               + OvrAvatarManager.Instance.GetPlatformGLBExtension(isFromZip);
            if (!String.IsNullOrEmpty(overridePostfix))
            {
                assetPostfix = overridePostfix;
            }

            return assetPostfix;
        }

        public bool LoadPreset(int preset, string namePrefix = "")
        {
            const bool isFromZip = true;
            var assetPostfix = GetAssetPostfix(isFromZip);

            var assetPath = $"{namePrefix}{preset}{assetPostfix}";
            return LoadAssets(new[] {assetPath}, AssetSource.Zip);
        }

        private void SetStreamData(OvrAvatarEntity entity)
        {
            var loadBytes = Resources.Load<TextAsset>(currentDataPath).text;
            var toString = JsonConvert.DeserializeObject<byte[]>(loadBytes);

            ApplyStreamData(toString);
        }
    }
}

#endif