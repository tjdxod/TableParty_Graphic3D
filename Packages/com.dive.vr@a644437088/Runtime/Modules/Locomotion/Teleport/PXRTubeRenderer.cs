using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Dive.VRModule
{
    public struct TubePoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public float relativeLength;
    }

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class PXRTubeRenderer : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct VertexLayout
        {
            public Vector3 pos;
            public Color32 color;
            public Vector2 uv;
        }

        #region Private Fields

        [SerializeField]
        private int divisions = 6;

        [SerializeField]
        private int bevel = 4;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh mesh;

        private VertexAttributeDescriptor[] dataLayout;
        private NativeArray<VertexLayout> vertsData;
        private VertexLayout layout = new VertexLayout();
        private int[] tris;
        private int initializedSteps = -1;
        private int vertsCount;

        private static readonly int FadeLimitsShaderID = Shader.PropertyToID("_FadeLimit");
        private static readonly int FadeSignShaderID = Shader.PropertyToID("_FadeSign");
        private static readonly int OffsetFactorShaderPropertyID = Shader.PropertyToID("_OffsetFactor");
        private static readonly int OffsetUnitsShaderPropertyID = Shader.PropertyToID("_OffsetUnits");

        #endregion

        #region Public Properties

        [field: SerializeField]
        public int RenderQueue { get; set; } = -1;

        [field: SerializeField]
        public Vector2 RenderOffset { get; set; } = Vector2.zero;

        [field: SerializeField]
        public float Radius { get; set; } = 0.005f;

        [field: SerializeField]
        public Gradient Gradient { get; set; } = null;

        [field: SerializeField]
        public Color Tint { get; set; } = Color.white;

        [field: SerializeField]
        public float ProgressFade { get; set; } = 0.2f;

        [field: SerializeField]
        public float StartFadeThreshold { get; set; } = 0.2f;

        [field: SerializeField]
        public float EndFadeThreshold { get; set; } = 0.2f;

        [field: SerializeField]
        public bool InvertThreshold { get; set; } = false;

        [field: SerializeField]
        public float Feather { get; set; } = 0.2f;

        [field: SerializeField]
        public bool MirrorTexture { get; set; } = false;

        public float Progress { get; set; } = 0f;
        public float TotalLength { get; set; } = 0f;

        private MeshRenderer MeshRenderer
        {
            get
            {
                if (meshRenderer == null)
                    meshRenderer = this.GetComponent<MeshRenderer>();

                return meshRenderer;
            }
        }

        private MeshFilter MeshFilter
        {
            get
            {
                if (meshFilter == null)
                    meshFilter = this.GetComponent<MeshFilter>();

                return meshFilter;
            }
        }

        #endregion

        #region Public Methods

        public void RenderTube(TubePoint[] points, Space space = Space.Self)
        {
            var steps = points.Length;
            if (steps != initializedSteps)
            {
                InitializeMeshData(steps);
                initializedSteps = steps;
            }

            vertsData = new NativeArray<VertexLayout>(vertsCount, Allocator.Temp);
            UpdateMeshData(points, space);
            MeshRenderer.enabled = enabled;
        }

        public void ResetTube()
        {
            initializedSteps = -1;
            MeshRenderer.enabled = false;
        }
        
        public void Hide()
        {
            MeshRenderer.enabled = false;
        }

        public void RedrawFadeThresholds()
        {
            var originFadeIn = StartFadeThreshold / TotalLength;
            var originFadeOut = (StartFadeThreshold + Feather) / TotalLength;
            var endFadeIn = (TotalLength - EndFadeThreshold) / TotalLength;
            var endFadeOut = (TotalLength - EndFadeThreshold - Feather) / TotalLength;

            MeshRenderer.material.SetVector(FadeLimitsShaderID, new Vector4(
                InvertThreshold ? originFadeOut : originFadeIn,
                InvertThreshold ? originFadeIn : originFadeOut,
                endFadeOut,
                endFadeIn));
            MeshRenderer.material.SetFloat(FadeSignShaderID, InvertThreshold ? -1 : 1);
            // ReSharper disable once Unity.InefficientPropertyAccess
            MeshRenderer.material.renderQueue = RenderQueue;

            MeshRenderer.material.SetFloat(OffsetFactorShaderPropertyID, RenderOffset.x);
            MeshRenderer.material.SetFloat(OffsetUnitsShaderPropertyID, RenderOffset.y);
        }

        #endregion

        #region Private Methods

        protected virtual void OnEnable()
        {
            MeshRenderer.enabled = true;
        }

        protected virtual void OnDisable()
        {
            MeshRenderer.enabled = false;
        }

        private void InitializeMeshData(int steps)
        {
            dataLayout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            };

            vertsCount = SetVertexCount(steps, divisions, bevel);
            var subMeshDesc = new SubMeshDescriptor(0, tris.Length);

            mesh = new Mesh();
            mesh.SetVertexBufferParams(vertsCount, dataLayout);
            mesh.SetIndexBufferParams(tris.Length, IndexFormat.UInt32);
            mesh.SetIndexBufferData(tris, 0, 0, tris.Length);
            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, subMeshDesc);

            MeshFilter.mesh = mesh;
        }

        private void UpdateMeshData(TubePoint[] points, Space space)
        {
            var steps = points.Length;
            var totalLength = 0f;
            var prevPoint = Vector3.zero;
            var pose = Pose.identity;
            var start = Pose.identity;
            var end = Pose.identity;

            var tr = transform;
            var transformScale = tr.lossyScale;

            var rootPose = new Pose(tr.position, tr.rotation);
            var inverseRootRotation = Quaternion.Inverse(rootPose.rotation);
            var rootPositionScaled = new Vector3(
                rootPose.position.x / transformScale.x,
                rootPose.position.y / transformScale.y,
                rootPose.position.z / transformScale.z);
            var uniformScale = space == Space.World ? transformScale.x : 1f;

            TransformPose(points[0], ref start);
            TransformPose(points[^1], ref end);

            BevelCap(start, false, 0);

            for (var i = 0; i < steps; i++)
            {
                TransformPose(points[i], ref pose);
                var point = pose.position;
                var rotation = pose.rotation;

                var progress = points[i].relativeLength;
                var color = Gradient.Evaluate(progress) * Tint;

                if (i > 0)
                {
                    totalLength += Vector3.Distance(point, prevPoint);
                }

                prevPoint = point;

                if (i / (steps - 1f) < Progress)
                {
                    color.a *= ProgressFade;
                }

                layout.color = color;

                WriteCircle(point, rotation, Radius, i + bevel, progress);
            }

            BevelCap(end, true, bevel + steps);

            mesh.bounds = new Bounds(
                (start.position + end.position) * 0.5f,
                end.position - start.position);
            mesh.SetVertexBufferData(vertsData, 0, 0, vertsData.Length, 0, MeshUpdateFlags.DontRecalculateBounds);

            TotalLength = totalLength * uniformScale;

            RedrawFadeThresholds();

            void TransformPose(in TubePoint tubePoint, ref Pose p)
            {
                if (space == Space.Self)
                {
                    p.position = tubePoint.position;
                    p.rotation = tubePoint.rotation;
                    return;
                }

                p.position = inverseRootRotation * (tubePoint.position - rootPositionScaled);
                p.rotation = inverseRootRotation * tubePoint.rotation;
            }
        }


        private int SetVertexCount(int positionCount, int divs, int bevelCap)
        {
            bevelCap = bevelCap * 2;
            var vertsPerPosition = divs + 1;
            var vertCount = (positionCount + bevelCap) * vertsPerPosition;

            var tubeTriangles = (positionCount - 1 + bevelCap) * divs * 6;
            var capTriangles = (divs - 2) * 3;
            var triangleCount = tubeTriangles + capTriangles * 2;
            tris = new int[triangleCount];

            // handle triangulation
            for (var i = 0; i < positionCount - 1 + bevelCap; i++)
            {
                // add faces
                for (var j = 0; j < divs; j++)
                {
                    int vert0 = i * vertsPerPosition + j;
                    int vert1 = (i + 1) * vertsPerPosition + j;
                    int t = (i * divs + j) * 6;
                    tris[t] = vert0;
                    tris[t + 1] = tris[t + 4] = vert1;
                    tris[t + 2] = tris[t + 3] = vert0 + 1;
                    tris[t + 5] = vert1 + 1;
                }
            }

            // triangulate the ends
            Cap(tubeTriangles, 0, divs - 1, true);
            Cap(tubeTriangles + capTriangles, vertCount - divs, vertCount - 1);

            void Cap(int t, int firstVert, int lastVert, bool clockwise = false)
            {
                for (var i = firstVert + 1; i < lastVert; i++)
                {
                    tris[t++] = firstVert;
                    tris[t++] = clockwise ? i : i + 1;
                    tris[t++] = clockwise ? i + 1 : i;
                }
            }

            return vertCount;
        }

        private void BevelCap(in Pose pose, bool end, int indexOffset)
        {
            var origin = pose.position;
            var rotation = pose.rotation;
            for (var i = 0; i < bevel; i++)
            {
                var radiusFactor = Mathf.InverseLerp(-1, bevel + 1, i);
                if (end)
                {
                    radiusFactor = 1 - radiusFactor;
                }

                var positionFactor = Mathf.Sqrt(1 - radiusFactor * radiusFactor);
                var point = origin + rotation * Vector3.forward * ((end ? 1 : -1) * Radius * positionFactor);
                WriteCircle(point, rotation, Radius * radiusFactor, i + indexOffset, end ? 1 : 0);
            }
        }

        private void WriteCircle(Vector3 point, Quaternion rotation, float width, int index, float progress)
        {
            var color = Gradient.Evaluate(progress) * Tint;
            if (progress < Progress)
            {
                color.a *= ProgressFade;
            }

            layout.color = color;

            for (var j = 0; j <= divisions; j++)
            {
                var radius = 2 * Mathf.PI * j / divisions;
                var circle = new Vector3(Mathf.Sin(radius), Mathf.Cos(radius), 0);
                var normal = rotation * circle;

                layout.pos = point + normal * width;
                if (MirrorTexture)
                {
                    var x = (j / (float)divisions) * 2f;
                    if (j >= divisions * 0.5f)
                    {
                        x = 2 - x;
                    }

                    layout.uv = new Vector2(x, progress);
                }
                else
                {
                    layout.uv = new Vector2(j / (float)divisions, progress);
                }

                var vertIndex = index * (divisions + 1) + j;
                vertsData[vertIndex] = layout;
            }
        }

        #endregion
    }
}