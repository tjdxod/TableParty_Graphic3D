using System;

namespace Dive.VRModule
{
    public static class PXRPlayerControllerExtensions
    {
        public static void BeforeTeleport(this PXRPlayerController controller, Action<HandSide> action)
        {
            if (controller == null)
                return;

            if (controller.Teleporters is not {Length: 2})
                return;

            // if (controller.Teleporters is not {Length: 2})
            //      return;

            if (controller.Teleporters[0] == null || controller.Teleporters[1] == null)
                return;

            // if (controller.Teleporters[0] == null || controller.Teleporters[1] == null)
            //      return;


            controller.Teleporters[0].BeforeTeleportEvent += () => { action?.Invoke(controller.Teleporters[0].HandSide); };

            // controller.Teleporters[0].BeforeTeleportEvent += () => { action?.Invoke(controller.Teleporters[0].HandSide); };

            controller.Teleporters[1].BeforeTeleportEvent += () => { action?.Invoke(controller.Teleporters[1].HandSide); };

            // controller.Teleporters[1].BeforeTeleportEvent += () => { action?.Invoke(controller.Teleporters[1].HandSide); };
        }

        public static void AfterTeleport(this PXRPlayerController controller, Action<HandSide> action)
        {
            if (controller == null)
                return;

            if (controller.Teleporters is not {Length: 2})
                return;

            // if (controller.Teleporters is not {Length: 2})
            //      return;

            if (controller.Teleporters[0] == null || controller.Teleporters[1] == null)
                return;

            // if (controller.Teleporters[0] == null || controller.Teleporters[1] == null)
            //      return;

            controller.Teleporters[0].AfterTeleportEvent += () => { action?.Invoke(controller.Teleporters[0].HandSide); };

            // controller.Teleporters[0].AfterTeleportEvent += () => { action?.Invoke(controller.Teleporters[0].HandSide); };

            controller.Teleporters[1].AfterTeleportEvent += () => { action?.Invoke(controller.Teleporters[1].HandSide); };

            // controller.Teleporters[1].AfterTeleportEvent += () => { action?.Invoke(controller.Teleporters[1].HandSide); };
        }

        public static void CancelTeleport(this PXRPlayerController controller, Action<HandSide> action)
        {
            if (controller == null)
                return;

            if (controller.Teleporters is not {Length: 2})
                return;

            // if (controller.Teleporters is not {Length: 2})
            //      return;

            if (controller.Teleporters[0] == null || controller.Teleporters[1] == null)
                return;

            // if (controller.Teleporters[0] == null || controller.Teleporters[1] == null)
            //      return;

            controller.Teleporters[0].CancelTeleportEvent += () => { action?.Invoke(controller.Teleporters[0].HandSide); };

            // controller.Teleporters[0].CancelTeleportEvent += () => { action?.Invoke(controller.Teleporters[0].HandSide); };

            controller.Teleporters[1].CancelTeleportEvent += () => { action?.Invoke(controller.Teleporters[1].HandSide); };

            // controller.Teleporters[1].CancelTeleportEvent += () => { action?.Invoke(controller.Teleporters[1].HandSide); };
        }

        public static void BeforeSnapTurn(this PXRPlayerController controller, Action<HandSide> action)
        {
            if (controller == null)
                return;

            if (controller.Teleporters is not {Length: 2})
                return;

            // if (controller.Teleporters is not {Length: 2})
            //      return;

            if (controller.Teleporters[0] == null || controller.Teleporters[1] == null)
                return;

            // if (controller.Teleporters[0] == null || controller.Teleporters[1] == null)
            //      return;

            controller.SnapTurns[0].BeforeSnapTurnEvent += () => { action?.Invoke(controller.SnapTurns[0].HandSide); };

            controller.SnapTurns[1].BeforeSnapTurnEvent += () => { action?.Invoke(controller.SnapTurns[1].HandSide); };
        }

        public static void AfterSnapTurn(this PXRPlayerController controller, Action<HandSide> action)
        {
            if (controller == null)
                return;

            if (controller.Teleporters is not {Length: 2})
                return;
            
            // if (controller.Teleporters is not {Length: 2})
                // return;

            if (controller.Teleporters[0] == null || controller.Teleporters[1] == null)
                return;
                
            // if (controller.Teleporters[0] == null || controller.Teleporters[1] == null)
                // return;

            controller.SnapTurns[0].AfterSnapTurnEvent += () => { action?.Invoke(controller.SnapTurns[0].HandSide); };

            controller.SnapTurns[1].AfterSnapTurnEvent += () => { action?.Invoke(controller.SnapTurns[1].HandSide); };
        }
    }
}