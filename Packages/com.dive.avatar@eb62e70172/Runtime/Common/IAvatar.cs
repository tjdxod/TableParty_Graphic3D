using System;
using UnityEngine;

namespace Dive.Avatar
{
    public interface IAvatar
    {
        public static event Action<(int, string) , IAvatar> InstanceCallback;
        public static event Action<(int, string) , IAvatar> DestroyCallback;
        public bool IsMine { get; }
        public bool AddColliderCompleted { get; }
        public SupportedPlatform Platform { get; }
        public GameObject InstanceObject { get; }
        
        public PXRAvatarBridgeComponent GetBridgeComponent();
        
        public Transform GetTransform();

        public AudioSource GetAudioSource();
        
        public void SetVolume(float volume);
        
        public static void OnInstanceCallback((int, string) identifier, IAvatar avatar)
        {
            InstanceCallback?.Invoke(identifier, avatar);
        }
        
        public static void OnDestroyCallback((int, string) identifier, IAvatar avatar)
        {
            DestroyCallback?.Invoke(identifier, avatar);
        }
    }
}


