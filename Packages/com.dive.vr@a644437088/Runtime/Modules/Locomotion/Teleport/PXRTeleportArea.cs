using UnityEngine;

namespace Dive.VRModule
{
    public class PXRTeleportArea : PXRTeleportSpaceBase
    {
        #region Public Methods

        public override void AdditionalActive()
        {
        }

        public override void AdditionalInactive()
        {
        }

        public override PXRTeleportSpaceBase GetTeleportSpace()
        {
            return this;
        }

        public override PXRTeleportSpaceBase GetOriginalTeleportSpace()
        {
            return this;
        }

        #endregion
        
        #region Private Methods
        
        protected override void Awake()
        {
            base.Awake();
            
            if(SpaceType != SpaceType.Area)
                SpaceType = SpaceType.Area;
        }

        private void Start()
        {
            RegisterActiveOnlyTeleport();
        }

        private void OnDestroy()
        {
            UnregisterActiveOnlyTeleport();
        }

        public override void EnteredPlayerToSpace()
        {
            
        }
        
        
        public override void ExitedPlayerToSpace()
        {
            
        }        
        
        #endregion
    }
}
