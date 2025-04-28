using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace Dive.VRModule
{
    [DisallowMultipleComponent]
    public class LuxOutline : MonoBehaviour
    {
        public enum Mode
        {
            OutlineAll,
            OutlineVisible,
            OutlineHidden,
            SilhouetteOnly
        }

        [Serializable]
        private class ListVector3
        {
            #region Public Fields

            public List<Vector3> data;

            #endregion
        }

        #region Public Fields

        [Header("Optional")]
        [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
                                 + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
        public bool precomputeOutline;

        #endregion

        #region Private Fields

        private static readonly HashSet<Mesh> RegisteredMeshes = new HashSet<Mesh>();

        [SerializeField]
        private Mode outlineMode = Mode.OutlineVisible;

        [SerializeField]
        private Color outlineColor = UnityEngine.Color.white;

        [SerializeField]
        private bool useScale = false;

#if ODIN_INSPECTOR
        [HideIf(nameof(useScale)), SerializeField, Range(0.01f, 50f)]
#else
        [ShowIf(nameof(useScale), false), SerializeField, Range(0.01f, 50f)]
#endif
        private float outlineWidth = 2f;

#if ODIN_INSPECTOR
        [ShowIf(nameof(useScale)), SerializeField, Range(1f, 2f)]
#else
        [ShowIf(nameof(useScale), true), SerializeField, Range(1f, 2f)]
#endif
        private float outlineScale = 1.01f;

        [Tooltip("런타임에 MeshRenderer의 Mesh가 변경되는경우")]
        [SerializeField]
        private bool enableRefreshRenderers = false;

        [SerializeField, HideInInspector]
        private List<Mesh> bakeKeys = new List<Mesh>();

        [SerializeField, HideInInspector]
        private List<ListVector3> bakeValues = new List<ListVector3>();

        [HideInInspector]
        public List<Renderer> renderers;

        private MaterialPropertyBlock materialPropertyBlock;
        private Material outlineMaterial;
        private bool needsUpdate;

        private const string ShaderName = "FastOutlineDoublePass";
        private const string ColorPropertyName = "_OutlineColor";
        private const string SpZTestPropertyName = "_SPZTest";
        private const string SpCullPropertyName = "_SPCull";
        private const string ZTestPropertyName = "_ZTest";
        private const string CullingPropertyName = "_Cull";
        private const string WidthPropertyName = "_Border";
        private const string StencilPropertyName = "_StencilCompare";
        private const string StencilRefPropertyName = "_StencilRef";
        private const string StencilReadMaskPropertyName = "_ReadMask";
        private const string UseScalePropertyName = "_UseScale";


        private static readonly int SpzTest = Shader.PropertyToID(SpZTestPropertyName);
        private static readonly int SpCull = Shader.PropertyToID(SpCullPropertyName);
        private static readonly int ZTest = Shader.PropertyToID(ZTestPropertyName);
        private static readonly int Cull = Shader.PropertyToID(CullingPropertyName);
        private static readonly int Border = Shader.PropertyToID(WidthPropertyName);
        private static readonly int StencilCompare = Shader.PropertyToID(StencilPropertyName);
        private static readonly int StencilRef = Shader.PropertyToID(StencilRefPropertyName);
        private static readonly int ReadMask = Shader.PropertyToID(StencilReadMaskPropertyName);
        private static readonly int Color = Shader.PropertyToID(ColorPropertyName);
        private static readonly int UseScale = Shader.PropertyToID(UseScalePropertyName);

        #endregion

        #region Public Properties

        public Mode OutlineMode
        {
            get => outlineMode;
            set
            {
                outlineMode = value;
                needsUpdate = true;
            }
        }

        public Color OutlineColor
        {
            get => outlineColor;
            set
            {
                outlineColor = value;
                needsUpdate = true;
            }
        }

        public float OutlineWidth
        {
            get => outlineWidth;
            set
            {
                outlineWidth = value;
                needsUpdate = true;
            }
        }

        #endregion

        #region Private Methods

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true).ToList();

            outlineMaterial = Instantiate(Resources.Load<Material>($@"Materials/{ShaderName}"));
            materialPropertyBlock = new MaterialPropertyBlock();

            LoadSmoothNormals();

            needsUpdate = true;
        }

        private void OnEnable()
        {
            if (enableRefreshRenderers)
            {
                renderers = GetComponentsInChildren<Renderer>(true).ToList();
                LoadSmoothNormals();
                needsUpdate = true;

                if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
                {
                    bakeKeys.Clear();
                    bakeValues.Clear();
                }

                if (precomputeOutline && bakeKeys.Count == 0)
                {
                    Bake();
                }
            }

            materialPropertyBlock = new MaterialPropertyBlock();

            foreach (var rend in renderers)
            {
                var materials = rend.sharedMaterials.ToList();
                materials.Add(outlineMaterial);

                rend.materials = materials.ToArray();
            }
        }

        private void OnValidate()
        {
            needsUpdate = true;

            if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
            {
                bakeKeys.Clear();
                bakeValues.Clear();
            }

            if (precomputeOutline && bakeKeys.Count == 0)
            {
                Bake();
            }
        }

        private void Update()
        {
            if (!needsUpdate)
                return;

            needsUpdate = false;

            UpdateMaterialProperties();
        }

        private void OnDisable()
        {
            foreach (var rend in renderers)
            {
                var materials = rend.sharedMaterials.ToList();
                materials.Remove(outlineMaterial);

                rend.materials = materials.ToArray();
            }
        }

        private void Bake()
        {
            var bakedMeshes = new HashSet<Mesh>();

            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                if (!bakedMeshes.Add(meshFilter.sharedMesh))
                    continue;

                var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

                bakeKeys.Add(meshFilter.sharedMesh);
                bakeValues.Add(new ListVector3() {data = smoothNormals});
            }
        }

        private void LoadSmoothNormals()
        {
            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                if (!RegisteredMeshes.Add(meshFilter.sharedMesh))
                    continue;

                var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
                var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

                meshFilter.sharedMesh.SetUVs(3, smoothNormals);
            }

            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (!RegisteredMeshes.Add(skinnedMeshRenderer.sharedMesh))
                    continue;

                var sharedMesh = skinnedMeshRenderer.sharedMesh;
                sharedMesh.uv4 = new Vector2[sharedMesh.vertexCount];
                CombineSubMeshes(sharedMesh, skinnedMeshRenderer.sharedMaterials);
            }
        }

        private List<Vector3> SmoothNormals(Mesh mesh)
        {
            var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);
            var smoothNormals = new List<Vector3>(mesh.normals);

            foreach (var group in groups)
            {
                if (group.Count() == 1)
                    continue;

                var smoothNormal = Vector3.zero;

                foreach (var pair in group)
                {
                    smoothNormal += mesh.normals[pair.Value];
                }

                smoothNormal.Normalize();

                foreach (var pair in group)
                {
                    smoothNormals[pair.Value] = smoothNormal;
                }
            }

            return smoothNormals;
        }

        private void CombineSubMeshes(Mesh mesh, Material[] materials)
        {
            if (mesh.subMeshCount == 1)
                return;

            if (mesh.subMeshCount > materials.Length)
                return;

            mesh.subMeshCount++;
            mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
        }

        private void UpdateMaterialProperties()
        {
            var spzTest = outlineMode == Mode.SilhouetteOnly
                ? (float)UnityEngine.Rendering.CompareFunction.LessEqual
                : (float)UnityEngine.Rendering.CompareFunction.Always;

            var zTest = outlineMode switch
            {
                Mode.OutlineAll => (float)UnityEngine.Rendering.CompareFunction.Always,
                Mode.OutlineVisible => (float)UnityEngine.Rendering.CompareFunction.LessEqual,
                Mode.OutlineHidden => (float)UnityEngine.Rendering.CompareFunction.GreaterEqual,
                Mode.SilhouetteOnly => (float)UnityEngine.Rendering.CompareFunction.Greater,
                _ => (float)UnityEngine.Rendering.CompareFunction.Always
            };

            var width = outlineMode == Mode.SilhouetteOnly ? 0 : outlineWidth;
            var compare = outlineMode == Mode.SilhouetteOnly
                ? (float)UnityEngine.Rendering.CompareFunction.Always
                : (float)UnityEngine.Rendering.CompareFunction.NotEqual;

            outlineMaterial.SetColor(Color, outlineColor);
            outlineMaterial.SetFloat(StencilRef, 2);
            outlineMaterial.SetFloat(ReadMask, 2);

            outlineMaterial.SetFloat(SpzTest, spzTest);
            outlineMaterial.SetFloat(SpCull, (float)UnityEngine.Rendering.CullMode.Back);
            outlineMaterial.SetFloat(ZTest, zTest);
            outlineMaterial.SetFloat(Cull, (float)UnityEngine.Rendering.CullMode.Back);
            outlineMaterial.SetFloat(Border, useScale ? outlineScale : width);
            outlineMaterial.SetFloat(StencilCompare, compare);

            outlineMaterial.SetInteger(UseScale, useScale ? 1 : 0);
        }

        #endregion
    }
}