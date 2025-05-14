using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.Avatar.Meta
{
    public class PXRMetaFederateSetting : ScriptableObject
    {
        [SerializeField]
        private string federateAppId = "";
        
        [SerializeField]
        private string federateAppAccessToken = "";
        
        public string FederateAppId => federateAppId;
        public string FederateAppAccessToken => federateAppAccessToken;
    }
}
