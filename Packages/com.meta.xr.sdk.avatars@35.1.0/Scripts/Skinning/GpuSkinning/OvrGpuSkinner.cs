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

using System;

using Oculus.Avatar2;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Oculus.Skinning.GpuSkinning
{
    // Missing C++ template metaprogramming to avoiding needing
    // separate classes for this
    internal class OvrGpuSkinner : OvrGpuSkinnerBase<OvrGpuSkinnerDrawCall>, IOvrGpuJointSkinner
    {
        public OvrSkinningTypes.SkinningQuality Quality
        {
            get => _skinningQuality;
            set
            {
                if (_skinningQuality != value)
                {
                    _skinningQuality = value;
                    UpdateDrawCallQuality(value);
                }
            }
        }

        public OvrGpuSkinner(
            int width,
            int height,
            GraphicsFormat texFormat,
            FilterMode texFilterMode,
            int depthTexelsPerSlice,
            OvrExpandableTextureArray neutralPoseTexture,
            OvrExpandableTextureArray jointsTexture,
            OvrSkinningTypes.SkinningQuality quality,
            OvrExpandableTextureArray indirectionTexture,
            OvrGpuMorphTargetsCombiner combiner,
            Shader skinningShader) : base(
            $"morphJointSkinnerOutput({combiner.name}+{jointsTexture.name})",
            width,
            height,
            texFormat,
            texFilterMode,
            depthTexelsPerSlice,
            neutralPoseTexture,
            skinningShader)

        {
            _jointsTex = jointsTexture;
            _skinningQuality = quality;
            _indirectionTex = indirectionTexture;
            _combiner = combiner;
        }

        public OvrSkinningTypes.Handle AddBlock(
            int widthInOutputTex,
            int heightInOutputTex,
            CAPI.ovrTextureLayoutResult layoutInNeutralPoseTex,
            CAPI.ovrTextureLayoutResult layoutInJointsTex,
            int numJoints,
            CAPI.ovrTextureLayoutResult layoutInIndirectionTex)
        {
            OvrSkinningTypes.Handle packerHandle = PackBlockAndExpandOutputIfNeeded(widthInOutputTex, heightInOutputTex);
            if (!packerHandle.IsValid())
            {
                return packerHandle;
            }

            var layoutInOutputTexture = GetLayoutInOutputTex(packerHandle);
            OvrGpuSkinnerDrawCall drawCallThatCanFit = GetDrawCallThatCanFit(
                (int)layoutInOutputTexture.texSlice,
                drawCall => drawCall.CanAdditionalQuad() && drawCall.CanFitAdditionalJoints(numJoints),
                () => new OvrGpuSkinnerDrawCall(
                    _skinningShader,
                    _outputScaleBias,
                    _neutralPoseTex,
                    _jointsTex,
                    _skinningQuality,
                    _combiner,
                    _indirectionTex));

            OvrSkinningTypes.Handle drawCallHandle = drawCallThatCanFit.AddBlock(
                new RectInt(layoutInOutputTexture.x, layoutInOutputTexture.y, layoutInOutputTexture.w, layoutInOutputTexture.h),
                Width,
                Height,
                layoutInNeutralPoseTex,
                layoutInJointsTex,
                numJoints,
                layoutInIndirectionTex);

            if (!drawCallHandle.IsValid())
            {
                RemoveBlock(packerHandle);
                return OvrSkinningTypes.Handle.kInvalidHandle;
            }

            AddBlockDataForHandle(layoutInOutputTexture, packerHandle, drawCallThatCanFit, drawCallHandle);
            return packerHandle;
        }

        public override IntPtr GetJointTransformMatricesArray(OvrSkinningTypes.Handle handle)
        {
            BlockData dataForBlock = GetBlockDataForHandle(handle);
            if (dataForBlock != null)
            {
                return dataForBlock.skinnerDrawCall.GetJointTransformMatricesArray(dataForBlock.handleInDrawCall);
            }
            else { return IntPtr.Zero; }
        }

        public override bool HasJoints => true;

        private readonly OvrExpandableTextureArray _jointsTex;
        private readonly OvrExpandableTextureArray _indirectionTex;
        private readonly OvrGpuMorphTargetsCombiner _combiner;

        private OvrSkinningTypes.SkinningQuality _skinningQuality;
    }
}
