#if UNITY_EDITOR

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

using System.Collections.Generic;
using System.Linq;
using Dive.Utility.UnityExtensions;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Csg;
using EditorUtility = UnityEditor.EditorUtility;

namespace Dive.VRModule
{
    [CustomEditor(typeof(PXRTeleportSpaceSupport))]
    public class PXRTeleportSpaceEditor : Editor
    {
        #region Private Fields

        private List<PointCoordinate> outerPoints;

        private List<PointCoordinate> innerPoints;

        #endregion

        #region Private Properties

        private List<PointCoordinate> OuterPoints
        {
            get
            {
                if (outerPoints != null)
                    return outerPoints;

                var support = target as PXRTeleportSpaceSupport;

                if (support == null)
                    return null;

                if (support.OuterPoints == null)
                    return null;

                outerPoints = support.OuterPoints;
                return outerPoints;
            }
        }

        private List<PointCoordinate> InnerPoints
        {
            get
            {
                if (innerPoints != null)
                    return innerPoints;

                var support = target as PXRTeleportSpaceSupport;

                if (support == null)
                    return null;

                if (support.InnerPoints == null)
                    return null;

                innerPoints = support.InnerPoints;
                return innerPoints;
            }
        }

        #endregion

        #region Public Methods

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20);
            GUILayout.Label("Outer Point Editor");

            var support = target as PXRTeleportSpaceSupport;

            if (support == null)
                return;

            OuterPointEditor(support);
            InnerPointEditor(support);

            GUILayout.Space(10);
            GUILayout.Label("Area Editor");
            
            if (!support.IsInner)
            {
                if (GUILayout.Button("Create Mesh Base Center (None Inner Mode)"))
                {
                    CreateMesh();
                }

                if (GUILayout.Button("Create Mesh Base Index (None Inner Mode)"))
                {
                    CreateMesh(support.BaseIndex);
                }
            }
            else
            {
                if (GUILayout.Button("Create Mesh Base Center With Inner (Inner Mode)"))
                {
                    CreateMeshWithInner();
                }

                if (GUILayout.Button("Create Mesh Base Index With Inner (Inner Mode)"))
                {
                    CreateMeshWithInner(support.BaseIndex, support.InnerIndex);
                }
            }

            if (GUILayout.Button("Create and Save Mesh"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Save Mesh", "TeleportSpace", "asset", "Save Mesh");
                if (string.IsNullOrEmpty(path))
                    return;

                CreateMesh();
                AssetDatabase.CreateAsset(support.MeshFilter.sharedMesh, path);
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Save Mesh"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Save Mesh", "TeleportSpace", "asset", "Save Mesh");
                if (string.IsNullOrEmpty(path))
                    return;

                AssetDatabase.CreateAsset(support.MeshFilter.sharedMesh, path);
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Reset Mesh"))
            {
                support.MeshFilter.sharedMesh = null;
                support.MeshCollider.sharedMesh = null;
                support.MeshRenderer.sharedMaterial = null;
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Finish");

            if (GUILayout.Button("Destroy Support"))
            {
                DestroyImmediate(support);
            }
        }

        #endregion

        #region Private Methods

        private void OnEnable()
        {
            SceneView.duringSceneGui += this.OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            var support = target as PXRTeleportSpaceSupport;
            if (support == null)
                return;

            DrawOuter(support);
            DrawInner(support);
        }

        private void DrawOuter(PXRTeleportSpaceSupport support)
        {
            var outerColor = PXRTeleportSpaceResource.BaseColor;
            Handles.color = outerColor;

            var count = OuterPoints.Count;

            var position = support.transform.position;
            var coord = new PointCoordinate(position.x, position.z);

            for (var i = 0; i < count; i++)
            {
                var point = coord + OuterPoints[i];
                var nextPoint = coord + OuterPoints[(i + 1) % count];

                Handles.DrawLine(point.ToDrawPosition(), nextPoint.ToDrawPosition(), PXRTeleportSpaceResource.BaseThickness);
            }

            outerColor = PXRTeleportSpaceResource.BaseSphereColor;
            Handles.color = outerColor;

            var coords = new PointCoordinate[count];

            for (var i = 0; i < count; i++)
            {
                coords[i] = Handles.FreeMoveHandle(i, (coord + OuterPoints[i]).ToDrawPosition(), PXRTeleportSpaceResource.BasePointSize, Vector3.zero, Handles.SphereHandleCap);
                Handles.Label((coord + OuterPoints[i]).ToDrawPosition(), i.ToString(), i == support.BaseIndex
                    ? new GUIStyle()
                    {
                        normal = new GUIStyleState()
                        {
                            textColor = PXRTeleportSpaceResource.BasePointIndexFontColor
                        },
                        fontSize = PXRTeleportSpaceResource.BasePointIndexFontSize
                    }
                    : new GUIStyle()
                    {
                        normal = new GUIStyleState()
                        {
                            textColor = PXRTeleportSpaceResource.BasePointFontColor
                        },
                        fontSize = PXRTeleportSpaceResource.BasePointFontSize
                    });
            }

            for (var i = 0; i < count; i++)
            {
                OuterPoints[i] = coords[i] - coord;
            }
        }

        private void DrawInner(PXRTeleportSpaceSupport support)
        {
            var color = PXRTeleportSpaceResource.InnerColor;
            Handles.color = color;

            var count = InnerPoints.Count;
            var outerCount = OuterPoints.Count;

            if (!support.IsInner)
                return;

            var position = support.transform.position;
            var coord = new PointCoordinate(position.x, position.z);
            
            for (var i = 0; i < count; i++)
            {
                var point = coord + InnerPoints[i];
                var nextPoint = coord + InnerPoints[(i + 1) % count];

                Handles.DrawLine(point.ToDrawPosition(), nextPoint.ToDrawPosition(), PXRTeleportSpaceResource.InnerThickness);
            }

            color = PXRTeleportSpaceResource.InnerSphereColor;
            Handles.color = color;

            var coords = new PointCoordinate[count];

            for (var i = 0; i < count; i++)
            {
                coords[i] = Handles.FreeMoveHandle(outerCount + i, (coord + InnerPoints[i]).ToDrawPosition(), PXRTeleportSpaceResource.InnerPointSize, Vector3.zero, Handles.SphereHandleCap);
                Handles.Label((coord + InnerPoints[i]).ToDrawPosition(), i.ToString(), i == support.InnerIndex
                    ? new GUIStyle()
                    {
                        normal = new GUIStyleState()
                        {
                            textColor = PXRTeleportSpaceResource.InnerPointIndexFontColor
                        },
                        fontSize = PXRTeleportSpaceResource.InnerPointIndexFontSize
                    }
                    : new GUIStyle()
                    {
                        normal = new GUIStyleState()
                        {
                            textColor = PXRTeleportSpaceResource.InnerPointFontColor
                        },
                        fontSize = PXRTeleportSpaceResource.InnerPointFontSize
                    });
            }

            for (var i = 0; i < count; i++)
            {
                InnerPoints[i] = coords[i] - coord;
            }
        }

        private void CreateMesh(int index = -1)
        {
            var support = target as PXRTeleportSpaceSupport;
            if (support == null)
                return;

            if (support.MeshCollider.sharedMesh != null)
                support.MeshCollider.sharedMesh.Clear();

            if (support.MeshFilter.sharedMesh != null)
                support.MeshFilter.sharedMesh.Clear();

            support.MeshRenderer.sharedMaterial = null;

            Vector3[] allVertices = new Vector3[OuterPoints.Count * 2 + 2];
            int[] allTriangles = new int[(OuterPoints.Count) * 12];

            var upperCenter = new PointCoordinate(0, 0);
            
            if (index == -1)
            {
                upperCenter = OuterPoints.Aggregate(upperCenter, (current, point) => current + point);
                upperCenter /= OuterPoints.Count;

                allVertices[0] = upperCenter.ToPosition();
                allVertices[OuterPoints.Count + 1] = upperCenter.ToLowerPosition();
            }
            else
            {
                allVertices[0] = OuterPoints[index].ToPosition();
                allVertices[OuterPoints.Count + 1] = OuterPoints[index].ToLowerPosition();
            }
            
            if (index == -1)
            {
                #region Upper
                
                for (var i = 1; i < OuterPoints.Count + 1; i++)
                {
                    allVertices[i] = OuterPoints[i - 1].ToPosition();
                }

                for (var i = 0; i < OuterPoints.Count - 1; i++)
                {
                    var idx = i * 3;
                    allTriangles[idx] = i + 2;
                    allTriangles[idx + 1] = i + 1;
                    allTriangles[idx + 2] = 0;
                }

                var upperLast = (OuterPoints.Count - 1) * 3;

                allTriangles[upperLast] = 1;
                allTriangles[upperLast + 1] = OuterPoints.Count;
                allTriangles[upperLast + 2] = 0;

                #endregion

                #region Lower

                allVertices[OuterPoints.Count + 1] = upperCenter.ToLowerPosition();

                for (var i = OuterPoints.Count + 2; i < allVertices.Length; i++)
                {
                    allVertices[i] = OuterPoints[i - OuterPoints.Count - 2].ToLowerPosition();
                }

                for (var i = OuterPoints.Count; i < OuterPoints.Count * 2 - 1; i++)
                {
                    var idx = i * 3;

                    allTriangles[idx] = OuterPoints.Count + 1;
                    allTriangles[idx + 1] = i + 2;
                    allTriangles[idx + 2] = i + 3;
                }

                var lowerLast = (OuterPoints.Count * 2 - 1) * 3;

                allTriangles[lowerLast] = OuterPoints.Count + 1;
                allTriangles[lowerLast + 1] = allVertices.Length - 1;
                allTriangles[lowerLast + 2] = OuterPoints.Count + 2;

                #endregion

                #region Side

                var compare = OuterPoints.Count + 1;
                var sideStart = OuterPoints.Count * 2 * 3;

                for (var i = 1; i < OuterPoints.Count; i++)
                {
                    var idx = (i - 1) * 3;

                    allTriangles[sideStart + idx] = i;
                    allTriangles[sideStart + idx + 1] = i + 1;
                    allTriangles[sideStart + idx + 2] = i + 1 + compare;
                }

                sideStart += (OuterPoints.Count - 1) * 3;

                allTriangles[sideStart] = OuterPoints.Count;
                allTriangles[sideStart + 1] = 1;
                allTriangles[sideStart + 2] = 1 + compare;

                sideStart += 3;

                for (var i = 1; i < OuterPoints.Count; i++)
                {
                    var idx = (i - 1) * 3;

                    allTriangles[sideStart + idx] = i;
                    allTriangles[sideStart + idx + 1] = i + 1 + compare;
                    allTriangles[sideStart + idx + 2] = i + compare;
                }

                sideStart += (OuterPoints.Count - 1) * 3;

                allTriangles[sideStart] = OuterPoints.Count;
                allTriangles[sideStart + 1] = OuterPoints.Count + 2;
                allTriangles[sideStart + 2] = OuterPoints.Count + compare;

                #endregion
            }
            else
            {
                #region Upper

                allVertices[0] = OuterPoints[support.BaseIndex].ToPosition();

                for (var i = 1; i < OuterPoints.Count + 1; i++)
                {
                    allVertices[i] = OuterPoints[i - 1].ToPosition();
                }

                for (var i = 0; i < OuterPoints.Count - 1; i++)
                {
                    var idx = i * 3;
                    allTriangles[idx] = i + 2;
                    allTriangles[idx + 1] = i + 1;
                    allTriangles[idx + 2] = 0;
                }

                var upperLast = (OuterPoints.Count - 1) * 3;

                allTriangles[upperLast] = 1;
                allTriangles[upperLast + 1] = OuterPoints.Count;
                allTriangles[upperLast + 2] = 0;

                #endregion

                #region Lower

                allVertices[OuterPoints.Count + 1] = OuterPoints[support.BaseIndex].ToLowerPosition();

                for (var i = OuterPoints.Count + 2; i < allVertices.Length; i++)
                {
                    allVertices[i] = OuterPoints[i - OuterPoints.Count - 2].ToLowerPosition();
                }

                for (var i = OuterPoints.Count; i < OuterPoints.Count * 2 - 1; i++)
                {
                    var idx = i * 3;

                    allTriangles[idx] = OuterPoints.Count + 1;
                    allTriangles[idx + 1] = i + 2;
                    allTriangles[idx + 2] = i + 3;
                }

                var lowerLast = (OuterPoints.Count * 2 - 1) * 3;

                allTriangles[lowerLast] = OuterPoints.Count + 1;
                allTriangles[lowerLast + 1] = allVertices.Length - 1;
                allTriangles[lowerLast + 2] = OuterPoints.Count + 2;

                #endregion

                #region Side

                var compare = OuterPoints.Count + 1;
                var sideStart = OuterPoints.Count * 2 * 3;

                for (var i = 1; i < OuterPoints.Count; i++)
                {
                    var idx = (i - 1) * 3;

                    allTriangles[sideStart + idx] = i;
                    allTriangles[sideStart + idx + 1] = i + 1;
                    allTriangles[sideStart + idx + 2] = i + 1 + compare;
                }

                sideStart += (OuterPoints.Count - 1) * 3;

                allTriangles[sideStart] = OuterPoints.Count;
                allTriangles[sideStart + 1] = 1;
                allTriangles[sideStart + 2] = 1 + compare;

                sideStart += 3;

                for (var i = 1; i < OuterPoints.Count; i++)
                {
                    var idx = (i - 1) * 3;

                    allTriangles[sideStart + idx] = i;
                    allTriangles[sideStart + idx + 1] = i + 1 + compare;
                    allTriangles[sideStart + idx + 2] = i + compare;
                }

                sideStart += (OuterPoints.Count - 1) * 3;

                allTriangles[sideStart] = OuterPoints.Count;
                allTriangles[sideStart + 1] = OuterPoints.Count + 2;
                allTriangles[sideStart + 2] = OuterPoints.Count + compare;

                #endregion
            }

            var mesh = new Mesh();
            mesh.SetVertices(allVertices);
            mesh.SetTriangles(allTriangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            support.MeshFilter.sharedMesh = mesh;
            support.MeshCollider.sharedMesh = mesh;
            support.MeshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            support.gameObject.layer = PXRNameToLayer.TeleportSpace;

            var space = support.GetComponent<PXRTeleportArea>();

            if (space == null)
                space = support.AddComponent<PXRTeleportArea>();

            EditorUtility.SetDirty(this);
        }
        
        private void CreateMeshWithInner(int outerIndex = -1, int innerIndex = -1)
        {
            var support = target as PXRTeleportSpaceSupport;
            if (support == null)
                return;

            if (support.MeshCollider.sharedMesh != null)
                support.MeshCollider.sharedMesh.Clear();

            if (support.MeshFilter.sharedMesh != null)
                support.MeshFilter.sharedMesh.Clear();

            support.MeshRenderer.sharedMaterial = null;

            var outerMesh = GetMesh(OuterPoints, outerIndex, out var outerFaces);
            var innerMesh = GetMesh(InnerPoints, innerIndex, out var innerFaces);

            var lhs = ProBuilderMesh.Create(outerMesh.vertices, outerFaces);
            var rhs = ProBuilderMesh.Create(innerMesh.vertices, innerFaces);

            var lhsRenderer = lhs.GetComponent<MeshRenderer>();
            var rhsRenderer = rhs.GetComponent<MeshRenderer>();

            lhsRenderer.sharedMaterials = new Material[] {new Material(Shader.Find("Universal Render Pipeline/Lit"))};
            rhsRenderer.sharedMaterials = new Material[] {new Material(Shader.Find("Universal Render Pipeline/Lit"))};

            var sel = new[] {lhs, rhs};
            UndoUtility.RecordSelection(sel, "Subtract");

            var result = CSG.Subtract(lhs.gameObject, rhs.gameObject);
            var mesh = (Mesh)result;

            DestroyImmediate(lhs.gameObject);
            DestroyImmediate(rhs.gameObject);

            support.MeshFilter.sharedMesh = mesh;
            support.MeshCollider.sharedMesh = mesh;
            support.MeshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        }

        private static Mesh GetMesh(IReadOnlyList<PointCoordinate> points, int index, out List<Face> faces)
        {
            faces = new List<Face>();

            Vector3[] allVertices = new Vector3[points.Count * 2 + 2];
            int[] allTriangles = new int[(points.Count) * 12];

            var upperCenter = new PointCoordinate(0, 0);

            if (index == -1)
            {
                upperCenter = points.Aggregate(upperCenter, (current, point) => current + point);
                upperCenter /= points.Count;

                allVertices[0] = upperCenter.ToPosition();
                allVertices[points.Count + 1] = upperCenter.ToLowerPosition();
            }
            else
            {
                allVertices[0] = points[index].ToPosition();
                allVertices[points.Count + 1] = points[index].ToLowerPosition();
            }

            #region Upper

            for (var i = 1; i < points.Count + 1; i++)
            {
                allVertices[i] = points[i - 1].ToPosition();
            }

            for (var i = 0; i < points.Count - 1; i++)
            {
                var idx = i * 3;
                allTriangles[idx] = i + 2;
                allTriangles[idx + 1] = i + 1;
                allTriangles[idx + 2] = 0;
            }

            var upperLast = (points.Count - 1) * 3;

            allTriangles[upperLast] = 1;
            allTriangles[upperLast + 1] = points.Count;
            allTriangles[upperLast + 2] = 0;

            #endregion

            #region Lower
            
            for (var i = points.Count + 2; i < allVertices.Length; i++)
            {
                allVertices[i] = points[i - points.Count - 2].ToLowerPosition();
            }

            for (var i = points.Count; i < points.Count * 2 - 1; i++)
            {
                var idx = i * 3;

                allTriangles[idx] = points.Count + 1;
                allTriangles[idx + 1] = i + 2;
                allTriangles[idx + 2] = i + 3;
            }

            var lowerLast = (points.Count * 2 - 1) * 3;

            allTriangles[lowerLast] = points.Count + 1;
            allTriangles[lowerLast + 1] = allVertices.Length - 1;
            allTriangles[lowerLast + 2] = points.Count + 2;

            #endregion

            #region Side

            var compare = points.Count + 1;
            var sideStart = points.Count * 2 * 3;

            for (var i = 1; i < points.Count; i++)
            {
                var idx = (i - 1) * 3;

                allTriangles[sideStart + idx] = i;
                allTriangles[sideStart + idx + 1] = i + 1;
                allTriangles[sideStart + idx + 2] = i + 1 + compare;
            }

            sideStart += (points.Count - 1) * 3;

            allTriangles[sideStart] = points.Count;
            allTriangles[sideStart + 1] = 1;
            allTriangles[sideStart + 2] = 1 + compare;

            sideStart += 3;

            for (var i = 1; i < points.Count; i++)
            {
                var idx = (i - 1) * 3;

                allTriangles[sideStart + idx] = i;
                allTriangles[sideStart + idx + 1] = i + 1 + compare;
                allTriangles[sideStart + idx + 2] = i + compare;
            }

            sideStart += (points.Count - 1) * 3;

            allTriangles[sideStart] = points.Count;
            allTriangles[sideStart + 1] = points.Count + 2;
            allTriangles[sideStart + 2] = points.Count + compare;

            #endregion

            faces.Add(new Face(allTriangles)
            {
                submeshIndex = 0
            });

            var mesh = new Mesh();
            mesh.SetVertices(allVertices);
            mesh.SetTriangles(allTriangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        private void OuterPointEditor(PXRTeleportSpaceSupport support)
        {
            if (GUILayout.Button("Add Outer Point"))
            {
                var position = (OuterPoints[0] + OuterPoints[^1]) / 2;
                support.OuterPoints.Add(position);
                EditorUtility.SetDirty(this);
            }

            if (GUILayout.Button("Remove Last Added Outer Point"))
            {
                if (support.OuterPoints.Count <= 4)
                {
                    Debug.LogWarning("포인트의 최소 갯수보다 적어 삭제할 수 없습니다. 최소 4개 이상이여야 합니다.");
                    return;
                }

                support.OuterPoints.RemoveAt(support.OuterPoints.Count - 1);
                EditorUtility.SetDirty(this);
            }

            if (GUILayout.Button("Reset Outer Point"))
            {
                support.OuterPoints.Clear();
                support.OuterPoints.Add(new PointCoordinate(support.Scale.x, support.Scale.z));
                support.OuterPoints.Add(new PointCoordinate(-support.Scale.x, support.Scale.z));
                support.OuterPoints.Add(new PointCoordinate(-support.Scale.x, -support.Scale.z));
                support.OuterPoints.Add(new PointCoordinate(support.Scale.x, -support.Scale.z));

                EditorUtility.SetDirty(this);
            }
        }

        private void InnerPointEditor(PXRTeleportSpaceSupport support)
        {
            var isInner = support.IsInner;

            if (!isInner)
                return;

            GUILayout.Space(10);
            GUILayout.Label("Inner Point Editor");

            if (GUILayout.Button("Add Inner Point (Inner Mode)"))
            {
                var position = (InnerPoints[0] + InnerPoints[^1]) / 2;
                support.InnerPoints.Add(position);
                EditorUtility.SetDirty(this);
            }

            if (GUILayout.Button("Remove Last Added Inner Point (Inner Mode)"))
            {
                if (support.InnerPoints.Count <= 4)
                {
                    Debug.LogWarning("포인트의 최소 갯수보다 적어 삭제할 수 없습니다. 최소 4개 이상이여야 합니다.");
                    return;
                }

                support.InnerPoints.RemoveAt(support.InnerPoints.Count - 1);
                EditorUtility.SetDirty(this);
            }

            if (GUILayout.Button("Reset Inner Point (Inner Mode)"))
            {
                support.InnerPoints.Clear();
                support.InnerPoints.Add(new PointCoordinate(support.Scale.x / 2, support.Scale.z / 2));
                support.InnerPoints.Add(new PointCoordinate(-support.Scale.x / 2, support.Scale.z / 2));
                support.InnerPoints.Add(new PointCoordinate(-support.Scale.x / 2, -support.Scale.z / 2));
                support.InnerPoints.Add(new PointCoordinate(support.Scale.x / 2, -support.Scale.z / 2));

                EditorUtility.SetDirty(this);
            }
        }

        #endregion
    }
}

#endif