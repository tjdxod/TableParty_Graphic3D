/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


#nullable enable

using System.Runtime.InteropServices;

namespace Oculus.Avatar2.Experimental
{
    using ovrAvatar2Result = Avatar2.CAPI.ovrAvatar2Result;
    using ovrAvatar2EntityId = Avatar2.CAPI.ovrAvatar2EntityId;
    using ovrAvatar2EntityViewFlags = Avatar2.CAPI.ovrAvatar2EntityViewFlags;

    public static partial class CAPI
    {
        /// Enable behavior system driven animation for this entity
        /// \param entityId Id of the entity on which to enable the behavior system
        /// \param enabled whether to enable or disable
        /// \return result code
        [DllImport(Avatar2.CAPI.LibFile, CallingConvention = CallingConvention.Cdecl)]
        public static extern ovrAvatar2Result ovrAvatar2Behavior_SetBehaviorSystemEnabled(ovrAvatar2EntityId entityId, bool enabled);

        /// Set this entity's main behavior.
        /// \param entityId Id of the entity on which to set the main behavior
        /// \param behaviorName Name of the main behavior (default "main")
        /// \return result code
        [DllImport(Avatar2.CAPI.LibFile, CallingConvention = CallingConvention.Cdecl)]
        public static extern ovrAvatar2Result ovrAvatar2Behavior_SetMainBehavior(ovrAvatar2EntityId entityId, string behaviorName);

        /// Assigns an output pose to an entity view of the currently active behavior graph.
        /// By default, only the first-person view is assigned with the output pose name "pose".
        /// \param entityId Id of the entity
        /// \param views All view modes to which the output pose should apply to
        /// \param outputPoseName Name of the output pose
        /// \return result code
        [DllImport(Avatar2.CAPI.LibFile, CallingConvention = CallingConvention.Cdecl)]
        public static extern ovrAvatar2Result ovrAvatar2Behavior_SetOutputPose(ovrAvatar2EntityId entityId, ovrAvatar2EntityViewFlags views, string outputPoseName);

        /// Link one graph's output with another's input.
        /// \param entityId Id of the entity this applies to
        /// \param outputExpr Output being linked, eg "path/to/graph:output"
        /// \param inputExpr Input being linked, eg "path/to/graph:input"
        /// \return return code
        [DllImport(Avatar2.CAPI.LibFile, CallingConvention = CallingConvention.Cdecl)]
        public static extern ovrAvatar2Result ovrAvatar2Behavior_Link(ovrAvatar2EntityId entityId, string outputExpr, string inputExpr);

        /// Unlink graph's input.
        /// \param entityId Id of the entity this applies to
        /// \param inputExpr Input being unlinked, eg "path/to/graph:input"
        /// \return result code
        [DllImport(Avatar2.CAPI.LibFile, CallingConvention = CallingConvention.Cdecl)]
        public static extern ovrAvatar2Result ovrAvatar2Behavior_UnlinkInput(ovrAvatar2EntityId entityId, string inputExpr);

        // Unlink graph's output.
        /// \param entityId Id of the entity this applies to
        /// \param outputExpr output being unlinked, eg "path/to/graph:output"
        /// \return result code
        [DllImport(Avatar2.CAPI.LibFile, CallingConvention = CallingConvention.Cdecl)]
        public static extern ovrAvatar2Result ovrAvatar2Behavior_UnlinkOutput(ovrAvatar2EntityId entityId, string outputExpr);

        /// Get event definitions registered for a specific Behavior
        /// \param behaviorName - Name of Behavior being inspected
        /// \param definitions - output array which will contain event definitions
        /// \param definitionsCapacity - maximum capacity of definitions output array (eventDef instances)
        /// \param totalDefinitions - output variable recording unique event definitions on `entityId`
        ///     NOTE: May be greater than `definitionsCapacity`
        /// \return ovrAvatar2Result_Success if all event definitions fit into `definitions`
        ///         ovrAvatar2Result_BufferTooSmall if `definitionsCapacity` was not large enough
        [DllImport(Avatar2.CAPI.LibFile, CallingConvention = CallingConvention.Cdecl)]
        internal static extern unsafe ovrAvatar2Result ovrAvatarXBehavior_GetEventDefinitions(
            ovrAvatar2StringView behaviorName,
            ovrAvatar2EventDefinition* definitions,
            uint definitionsCapacity,
            uint* totalDefinitions);
    }
}
