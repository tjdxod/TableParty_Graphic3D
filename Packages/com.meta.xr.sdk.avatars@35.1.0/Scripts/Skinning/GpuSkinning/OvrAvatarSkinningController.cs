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


#nullable disable

using System;
using System.Collections.Generic;

using Oculus.Skinning.GpuSkinning;

using UnityEngine;
using UnityEngine.Profiling;

using static Oculus.Avatar2.CAPI;

namespace Oculus.Avatar2
{
    public sealed class OvrAvatarSkinningController : System.IDisposable
    {
        // Avoid skinning more avatars than technically feasible
        public const uint MaxGpuSkinnedAvatars = MaxSkinnedAvatarsPerFrame * 8;

        // Avoid skinning more avatars than GPU resources are preallocated for
        public const uint MaxSkinnedAvatarsPerFrame = 32;

        private const int NumExpectedAvatars = 16;

        private readonly List<OvrGpuMorphTargetsCombiner> _activeCombinerList = new List<OvrGpuMorphTargetsCombiner>(NumExpectedAvatars);
        private readonly List<IOvrGpuSkinner> _activeSkinnerList = new List<IOvrGpuSkinner>(NumExpectedAvatars);
        private readonly List<OvrComputeMeshAnimator> _activeAnimators = new List<OvrComputeMeshAnimator>(NumExpectedAvatars);

        private OvrComputeBufferPool bufferPool = null;

        public OvrAvatarSkinningController(bool usingGPUskinner)
        {
            if (usingGPUskinner)
            {
                bufferPool = new OvrComputeBufferPool();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isMainThread) { bufferPool?.Dispose(); }

        ~OvrAvatarSkinningController()
        {
            Dispose(false);
        }

        internal void AddActiveCombiner(OvrGpuMorphTargetsCombiner combiner)
        {
            AddGpuSkinningElement(_activeCombinerList, combiner);
        }

        internal void AddActiveSkinner(IOvrGpuSkinner skinner)
        {
            AddGpuSkinningElement(_activeSkinnerList, skinner);
        }

        internal void AddActivateComputeAnimator(OvrComputeMeshAnimator meshAnimator)
        {
            AddGpuSkinningElement(_activeAnimators, meshAnimator);
        }

        // This behaviour is manually updated at a specific time during OvrAvatarManager::Update()
        // to prevent issues with Unity script update ordering
        internal void UpdateInternal()
        {
            Profiler.BeginSample("OvrAvatarSkinningController::UpdateInternal");
            using var livePerfMarker = OvrAvatarXLivePerf_Marker(ovrAvatarXLivePerf_UnitySectionID.SkinningController);

            if (_activeCombinerList.Count > 0)
            {
                Profiler.BeginSample("OvrAvatarSkinningController.CombinerCalls");
                using var livePerfMarker_Combine = OvrAvatarXLivePerf_Marker(ovrAvatarXLivePerf_UnitySectionID.SkinningController_Combine);
                foreach (var combiner in _activeCombinerList)
                {
                    combiner.CombineMorphTargetWithCurrentWeights();
                }
                _activeCombinerList.Clear();
                Profiler.EndSample(); // "OvrAvatarSkinningController.CombinerCalls"
            }

            if (_activeSkinnerList.Count > 0)
            {
                Profiler.BeginSample("OvrAvatarSkinningController.SkinnerCalls");
                using var livePerfMarker_Skinner = OvrAvatarXLivePerf_Marker(ovrAvatarXLivePerf_UnitySectionID.SkinningController_Skinner);
                foreach (var skinner in _activeSkinnerList)
                {
                    skinner.UpdateOutputTexture();
                }
                _activeSkinnerList.Clear();
                Profiler.EndSample(); // "OvrAvatarSkinningController.SkinnerCalls"
            }

            if (_activeAnimators.Count > 0)
            {
                Profiler.BeginSample("OvrAvatarSkinningController.AnimatorDispatches");
                using var livePerfMarker_Animator = OvrAvatarXLivePerf_Marker(ovrAvatarXLivePerf_UnitySectionID.SkinningController_Animator);
                foreach (var animator in _activeAnimators)
                {
                    animator.UpdateOutputs();
                }
                _activeAnimators.Clear();
                Profiler.EndSample(); // "OvrAvatarSkinningController.AnimatorDispatches"
            }

            Profiler.EndSample();
        }

        private void AddGpuSkinningElement<T>(List<T> list, T element) where T : class
        {
            Debug.Assert(element != null);
            Debug.Assert(!list.Contains(element));
            list.Add(element);
        }

        internal void StartFrame()
        {
            bufferPool?.StartFrame();
        }

        internal void EndFrame()
        {
            bufferPool?.EndFrame();
        }

        internal OvrComputeBufferPool.EntryJoints GetNextEntryJoints()
        {
            return bufferPool.GetNextEntryJoints();
        }

        internal ComputeBuffer GetJointBuffer()
        {
            return bufferPool.GetJointBuffer();
        }

        internal ComputeBuffer GetWeightsBuffer()
        {
            return bufferPool.GetWeightsBuffer();
        }

        internal OvrComputeBufferPool.EntryWeights GetNextEntryWeights(int numMorphTargets)
        {
            return bufferPool.GetNextEntryWeights(numMorphTargets);
        }
    }
}
