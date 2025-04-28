using UnityEngine;

namespace Dive.VRModule
{
    public partial class PXRRigMovementCommon : PXRRigMovementBase
    {
        protected override void HandleCharacterXRInput()
        {
            var isTeleporting = CurrentState == CharacterState.Teleport;
            var isClicking = PXRRig.PlayerController.IsClicking;
            
            currentInputs = new CharacterXRInputs
            {
                MoveAxis = isTeleporting || isClicking ? Vector2.zero : PXRInputBridge.LeftController.GetAxisValue(ControllerAxis.Primary)
            };
            
            SetInputs(ref currentInputs);
        }
    }
}
