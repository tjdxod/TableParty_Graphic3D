using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    public partial class PXRTeleportOwnerArea : PXRTeleportArea
    {
        public event Action EnterOwnerEvent;
        public event Action ExitOwnerEvent;

        [SerializeField]
        protected Vector2 areaSize = new Vector2(1, 1);

        protected BoxCollider areaCollider;
        protected PXRTeleportPoint areaPoint;
        
#if UNITY_EDITOR

        private void OnValidate()
        {
            if(areaCollider == null)
            {
                areaCollider = GetComponent<BoxCollider>();
                
                if(areaCollider == null)
                    areaCollider = gameObject.AddComponent<BoxCollider>();
            }
            
            areaCollider.center = new Vector3(0, 0.01f, 0);
            areaCollider.size = new Vector3(areaSize.x, 0.01f, areaSize.y);
        }

        private void Reset()
        {
            areaCollider = GetComponent<BoxCollider>();
            
            if(areaCollider == null)
                areaCollider = gameObject.AddComponent<BoxCollider>();
            
            areaCollider.center = new Vector3(0, 0.01f, 0);
            areaCollider.size = new Vector3(areaSize.x, 0.01f, areaSize.y);
        }
        
#endif

        protected override void Awake()
        {
            base.Awake();
            areaPoint = GetComponentInChildren<PXRTeleportPoint>(true);
        }

        public void ActiveAreaPoint()
        {
            if (areaPoint == null) 
                return;
            
            areaPoint.EnteredPlayerToSpace();
        }
        
        public void InactiveAreaPoint()
        {
            if (areaPoint == null) 
                return;
            
            areaPoint.ExitedPlayerToSpace();
        }
        
        public override void OnEnterPlayer()
        {
            base.OnEnterPlayer();
            EnterOwnerEvent?.Invoke();
        }
        
        public override void OnExitPlayer()
        {
            base.OnExitPlayer();
            ExitOwnerEvent?.Invoke();
        }
    }
}
