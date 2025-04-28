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

// NOTE: In a shipping apllication it is inefficient to do this since the
// Update() dispatch is an unnecessary expense. It will often be the case
// that a scene will be loaded with an IBL setup, and this will not change
// until the next scene is loaded.
#if UNITY_EDITOR
#define ALLOW_DYNAMIC_IBL_SWTICH
#endif


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Avatar2
{

    public class IblSetupForEnvironment : MonoBehaviour
    {
        [Tooltip("This value should affect all tone mapped and non tone mapped lighting exposures.")]
        public float CurrentExposure = 1.0f;
        private float _previousExposure;
        [Tooltip("Exposure shader parameter name.")]
        public string ExposureShaderParameterName = "u_Exposure";

        [Tooltip("A prefiltered single mip texture for diffuse influences.")]
        public Texture DiffuseEnvironmentCubeMap;
        private Texture _previousDiffuseEnvironmentCubeMap;
        [Tooltip("Diffuse environment sampler shader parameter name.")]
        public string[] DiffuseEnvironmentShaderParameterNames = new string[] { "u_DiffuseEnvSampler", "u_LambertianEnvSampler" };

        [Tooltip("Ambient specular reflections on a multiple mip texture, chosen via the roughness of materials.")]
        public Texture SpecularEnvironmentCubeMap;
        private Texture _previousSpecularEnvironmentCubeMap;
        [Tooltip("Specular environment sampler shader parameter name.")]
        [SerializeField]
        private string[] SpecularEnvironmentShaderParameterNames = new string[] { "u_SpecularEnvSampler", "u_GGXEnvSampler" };
        [Tooltip("Represents the number of mips in the specular texture map. This is automatically set when setting the texture.")]
        public string SpecularMapMipCountShaderPameterName = "u_MipCount";

        [Tooltip("A 2 channel look up textrure to represent the BRDF function.")]
        public Texture BrdfLutMap;
        private Texture _previousBrdfLutMap;
        [Tooltip("BRDF Lut sammpler shader parameter name.")]
        public string[] BrdfLutShaderParameterNames = new string[] { "u_brdfLUT", "u_GGXLUT" };

        [Tooltip("Optional world object cubemap material, the specular environment cube above will be set into it.")]
        public Material CubeMapMaterial;
        [Tooltip("Name of the texture parameter in the world object cubemap shader.")]
        public string CubeMapShaderParameterName = "_Tex";


        private void SetExposureScopeParm()
        {
            Shader.SetGlobalFloat(ExposureShaderParameterName, CurrentExposure);
            _previousExposure = CurrentExposure;
        }

        private void SetAllIblGlobalScopeParams()
        {
            foreach (var parameterName in DiffuseEnvironmentShaderParameterNames)
            {
                Shader.SetGlobalTexture(parameterName, DiffuseEnvironmentCubeMap);
            }
            foreach (var parameterName in SpecularEnvironmentShaderParameterNames)
            {
                Shader.SetGlobalTexture(parameterName, SpecularEnvironmentCubeMap);
            }
            foreach (var parameterName in BrdfLutShaderParameterNames)
            {
                Shader.SetGlobalTexture(parameterName, BrdfLutMap);
            }
            Shader.SetGlobalInt(SpecularMapMipCountShaderPameterName, SpecularEnvironmentCubeMap.mipmapCount);
            if (CubeMapMaterial != null)
            {
                CubeMapMaterial.SetTexture(CubeMapShaderParameterName, SpecularEnvironmentCubeMap);
            }


            _previousDiffuseEnvironmentCubeMap = DiffuseEnvironmentCubeMap;
            _previousSpecularEnvironmentCubeMap = SpecularEnvironmentCubeMap;
            _previousBrdfLutMap = BrdfLutMap;
        }

        protected virtual void OnEnable()
        {
            SetExposureScopeParm();
            SetAllIblGlobalScopeParams();
        }

        protected virtual void OnDisable()
        {

        }

#if ALLOW_DYNAMIC_IBL_SWTICH
        protected virtual void Update()
        {
            if(CurrentExposure != _previousExposure) {
                SetExposureScopeParm();
            }
            if ( _previousDiffuseEnvironmentCubeMap != DiffuseEnvironmentCubeMap ||
                _previousSpecularEnvironmentCubeMap != SpecularEnvironmentCubeMap ||
                _previousBrdfLutMap != BrdfLutMap )
            {
                SetAllIblGlobalScopeParams();
            }
        }
#endif
    }
}
