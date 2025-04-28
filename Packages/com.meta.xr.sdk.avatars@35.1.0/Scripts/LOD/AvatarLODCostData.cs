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

using Debug = UnityEngine.Debug;

namespace Oculus.Avatar2
{
    public struct AvatarLODCostData
    {
        /// Number of vertices in avatar mesh.
        public readonly uint meshVertexCount;
        // TODO: Deprecate, use triCount instead
        /// Number of vertices in the morph targets.
        public readonly uint morphVertexCount;
        /// Number of triangles in the avatar mesh.
        public readonly uint renderTriangleCount;
        /// Skinning cost heuristic from SDK.
        public readonly uint skinningCost;

        private AvatarLODCostData(uint meshVertCount, uint morphVertCount, uint triCount, uint skinCost)
        {
            meshVertexCount = meshVertCount;
            morphVertexCount = morphVertCount;
            renderTriangleCount = triCount;
            skinningCost = skinCost;
        }
        internal AvatarLODCostData(OvrAvatarPrimitive prim)
            : this(prim.meshVertexCount, prim.morphVertexCount, prim.triCount, prim.skinningCost) { }
        ///
        /// Add the second LOD cost to the first and return
        /// the combined cost of both LODs.
        ///
        /// @param total    first LodCostData to add.
        /// @param add      second LodCostData to add.
        /// @returns LodCostData with total cost of both LODs.
        // TODO: inplace Increment/Decrement would be useful
        public static AvatarLODCostData Sum(in AvatarLODCostData total, in AvatarLODCostData add)
        {
            return new AvatarLODCostData(
                total.meshVertexCount + add.meshVertexCount,
                total.morphVertexCount + add.morphVertexCount,
                total.renderTriangleCount + add.renderTriangleCount,
                total.skinningCost + add.skinningCost
            );
        }

        ///
        /// Subtract the second LOD cost from the first and return
        /// the difference between the LODs.
        ///
        /// @param total    LodCostData to subtract from.
        /// @param sub      LodCostData to subtract.
        /// @returns LodCostData with different between LODs.
        public static AvatarLODCostData Subtract(in AvatarLODCostData total, in AvatarLODCostData sub)
        {
            Debug.Assert(total.meshVertexCount >= sub.meshVertexCount);
            Debug.Assert(total.skinningCost >= sub.skinningCost);
            return new AvatarLODCostData(
                total.meshVertexCount - sub.meshVertexCount,
                total.morphVertexCount - sub.morphVertexCount,
                total.renderTriangleCount - sub.renderTriangleCount,
                total.skinningCost - sub.skinningCost);
        }
    }
}
