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

using UnityEngine;
using UnityEngine.Rendering;

namespace Oculus.Skinning.GpuSkinning
{
    internal class OvrMorphTargetsData
    {
        public static string[] ShaderKeywordsForMorphTargets(bool useIndirectionTexture = true)
        {
            return useIndirectionTexture ? _shaderIndirectionKeywords : _shaderKeywords;
        }

        public int IndirectionTexWidth => _indirectionTex.Width;
        public int IndirectionTexHeight => _indirectionTex.Height;

        public OvrMorphTargetsData(
            OvrGpuMorphTargetsCombiner morphTargetsCombiner,
            OvrExpandableTextureArray indirectionTexture,
            Material skinningMaterial)
        {
            _combiner = morphTargetsCombiner;
            _indirectionTex = indirectionTexture;
            _skinningMaterial = skinningMaterial;

            _combiner.ArrayResized += CombinerArrayResized;
            _indirectionTex.ArrayResized += IndirectionTexArrayResized;

            SetIndirectionTextureInMaterial(_indirectionTex.GetTexArray());
            SetCombinedMorphTargetsTextureInMaterial(morphTargetsCombiner.GetCombinedShapesTexArray());
        }

        public void Destroy()
        {
            _combiner.ArrayResized -= CombinerArrayResized;
            _indirectionTex.ArrayResized -= IndirectionTexArrayResized;
        }

        private void CombinerArrayResized(OvrGpuMorphTargetsCombiner sender, RenderTexture newArray)
        {
            SetCombinedMorphTargetsTextureInMaterial(newArray);
        }

        private void IndirectionTexArrayResized(OvrExpandableTextureArray sender, Texture2DArray newArray)
        {
            SetIndirectionTextureInMaterial(newArray);
        }

        private void SetIndirectionTextureInMaterial(Texture2DArray indirectionTex)
        {
            _skinningMaterial.SetTexture(INDIRECTION_TEX_PROP, indirectionTex);
        }

        private void SetCombinedMorphTargetsTextureInMaterial(RenderTexture combinedMorphTargetsTex)
        {
            _skinningMaterial.SetTexture(COMBINED_MORPH_TARGETS_TEX_PROP, combinedMorphTargetsTex, RenderTextureSubElement.Color);
        }

        private const string OVR_MORPH_TARGET_KEYWORD = "OVR_HAS_MORPH_TARGETS";
        private const string OVR_MORPH_TARGET_INDIRECTION_KEYWORD = "OVR_HAS_MORPH_TARGETS_INDIRECTION_TEXTURE";

        private static readonly string[] _shaderKeywords = { OVR_MORPH_TARGET_KEYWORD };
        private static readonly string[] _shaderIndirectionKeywords = { OVR_MORPH_TARGET_INDIRECTION_KEYWORD };

        private static readonly int COMBINED_MORPH_TARGETS_TEX_PROP = Shader.PropertyToID("u_CombinedMorphTargetsTex");
        private static readonly int INDIRECTION_TEX_PROP = Shader.PropertyToID("u_IndirectionTex");

        private Material _skinningMaterial;
        private OvrGpuMorphTargetsCombiner _combiner;
        private OvrExpandableTextureArray _indirectionTex;
    }
}
