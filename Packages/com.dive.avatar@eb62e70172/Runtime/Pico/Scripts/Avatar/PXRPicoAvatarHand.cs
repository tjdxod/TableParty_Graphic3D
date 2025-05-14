using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.Avatar.Pico
{
    public class PXRPicoAvatarHand : MonoBehaviour
    {
        [SerializeField]
        private GameObject leftHandPostSkeleton;
    
        [SerializeField]
        private GameObject rightHandPostSkeleton;

        [SerializeField]
        private GameObject leftHandPostGo;
    
        [SerializeField]
        private GameObject rightHandPostGo;
    
        public GameObject LeftHandPostSkeleton => leftHandPostSkeleton;
        public GameObject RightHandPostSkeleton => rightHandPostSkeleton;
        public GameObject LeftHandPostGo => leftHandPostGo;
        public GameObject RightHandPostGo => rightHandPostGo;
    }
}
