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

namespace Oculus.Avatar2
{
    internal static class OvrAvatarLogFilter
    {
        internal static string[] KnownAssetWarnings = new string[]
        {
            "RigGraphNodeHelpers::Variable cache_muzzle_head_joint_offset is initialized by another node and referenced by current node cache_muzzle_joint_head_joint_offset",
            "Render::Could not find node LOD00_combined_1stPerson_geometry to re-root",
        };
    }
}
