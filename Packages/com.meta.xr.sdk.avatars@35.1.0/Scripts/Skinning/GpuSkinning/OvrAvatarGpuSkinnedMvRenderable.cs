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

/// @file OvrAvatarGpuSkinnedRenderable.cs

using System;
using Oculus.Avatar2;
using UnityEngine;

namespace Oculus.Skinning.GpuSkinning
{
    /**
     * Component that encapsulates the meshes of a skinned avatar.
     * This component implements skinning using the Avatar SDK
     * and uses the GPU. It performs skinning on every avatar
     * at each frame. It is used when the skinning configuration
     * is set to SkinningConfig.OVR_UNITY_GPU_FULL, motion smoothing
     * is *not* enabled, and "App Space Warp" is enabled in the GPU skinning configuration.
     *
     * @see OvrAvatarSkinnedRenderable
     * @see OvrAvatarGpuSkinnedRenderable
     * @see OvrGpuSkinningConfiguration.MotionSmoothing
     * @see OvrAvatarGpuInterpolatedSkinnedRenderable
     */
    public class OvrAvatarGpuSkinnedMvRenderable : OvrAvatarGpuSkinnedRenderableBase
    {
        protected override string LogScope => "OvrAvatarGpuSkinnedMvRenderable";

        // Only 1 "animation frame" is required for this renderable
        private bool _hasValidPreviousRenderFrame;

        private SkinningOutputFrame _prevRenderFrameSlice = SkinningOutputFrame.FrameZero;

        // 2 "output depth texels" per "atlas packer" slice to have a current and previous animation frame
        protected override int SkinnerOutputDepthTexelsPerSlice => 2;
        protected override int NumAnimationFramesBeforeValidData => 1;

        protected override void OnAnimationEnabledChanged(bool isNowEnabled)
        {
            if (isNowEnabled)
            {
                NumValidAnimationFrames = 0;
                SkinnerWriteDestination = SkinningOutputFrame.FrameOne;
            }
        }

        protected virtual void OnEnable()
        {
            // Reset the previous render frame slice and render frame count
            _hasValidPreviousRenderFrame = false;
            _prevRenderFrameSlice = SkinnerWriteDestination;
        }

        internal override void AnimationFramePreUpdate()
        {
            // ASSUMPTION: This call will always be followed by calls to update morphs and/or skinning.
            // With that assumption, new data will be written by the morph target combiner and/or skinner, so there
            // will be valid data at end of frame.
            IncrementValidAnimationFramesIfNeeded();

            SwapWriteDestination();
        }

        internal override void RenderFrameUpdate()
        {
            // Need at least 1 "previous render frame" to have a previous
            // render frame slice
            if (!_hasValidPreviousRenderFrame)
            {
                // Not enough render frames, just make the motion vectors "previous frame" the same
                // as the current one
                _prevRenderFrameSlice = SkinnerWriteDestination;
                _hasValidPreviousRenderFrame = true;
            }

            SetRenderFrameTextureSlices();

            // Update "previous frame" value for next frame
            _prevRenderFrameSlice = SkinnerWriteDestination;
        }

        private void SetRenderFrameTextureSlices()
        {
            // Update the depth texel value to interpolate between skinning output slices
            rendererComponent.GetPropertyBlock(MatBlock);

            MatBlock.SetFloat(U_ATTRIBUTE_TEXEL_SLICE_PROP_ID, SkinnerLayoutSlice + GetSliceValue(SkinnerWriteDestination));
            MatBlock.SetFloat(U_PREV_POSITION_TEXEL_SLICE_PROP_ID, SkinnerLayoutSlice + GetSliceValue(_prevRenderFrameSlice));

            rendererComponent.SetPropertyBlock(MatBlock);
        }

        private float GetSliceValue(SkinningOutputFrame outputFrame)
        {
            switch (outputFrame)
            {
                case SkinningOutputFrame.FrameZero:
                    return 0.0f;
                case SkinningOutputFrame.FrameOne:
                    return 1.0f;
            }

            return 0.0f;
        }
    }
}
