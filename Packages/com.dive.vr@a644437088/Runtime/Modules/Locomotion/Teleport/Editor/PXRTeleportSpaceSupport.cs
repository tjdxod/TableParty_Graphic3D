#if UNITY_EDITOR

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dive.VRModule
{
    [Serializable]
    public struct PointCoordinate
    {
        public float x;
        public float z;

        public PointCoordinate(float x, float z)
        {
            this.x = x;
            this.z = z;
        }
        
        public static PointCoordinate operator +(PointCoordinate a, PointCoordinate b)
        {
            return new PointCoordinate(a.x + b.x, a.z + b.z);
        }
        
        public static PointCoordinate operator -(PointCoordinate a, PointCoordinate b)
        {
            return new PointCoordinate(a.x - b.x, a.z - b.z);
        }
        
        public static PointCoordinate operator *(PointCoordinate a, PointCoordinate b)
        {
            return new PointCoordinate(a.x * b.x, a.z * b.z);
        }
        
        public static PointCoordinate operator /(PointCoordinate a, PointCoordinate b)
        {
            return new PointCoordinate(a.x / b.x, a.z / b.z);
        }
        
        public static PointCoordinate operator +(PointCoordinate a, float b)
        {
            return new PointCoordinate(a.x + b, a.z + b);
        }
        
        public static PointCoordinate operator -(PointCoordinate a, float b)
        {
            return new PointCoordinate(a.x - b, a.z - b);
        }
        
        public static PointCoordinate operator *(PointCoordinate a, float b)
        {
            return new PointCoordinate(a.x * b, a.z * b);
        }
        
        public static PointCoordinate operator /(PointCoordinate a, float b)
        {
            return new PointCoordinate(a.x / b, a.z / b);
        }
        
        public static implicit operator PointCoordinate(Vector3 a)
        {
            return new PointCoordinate(a.x, a.z);
        }
        
        public Vector3 ToPosition()
        {
            return new Vector3(x, 0.01f, z);
        }

        public Vector3 ToDrawPosition()
        {
            return new Vector3(x, 0f, z);
        }
        
        public Vector3 ToLowerPosition()
        {
            return new Vector3(x, 0f, z);
        }
    }

    public class PXRTeleportSpaceSupport : MonoBehaviour
    {
        #region Private Fields

        [SerializeField]
        private List<PointCoordinate> outerPoints = null;
        
        [Space, SerializeField]
        private bool isInner = false;
        
        [SerializeField]
        private List<PointCoordinate> innerPoints = null;
        
        [Space, SerializeField, LabelText("Base Index"), Tooltip("Use for Create Mesh Base Index")]
        private int baseIndex;
        
        [Space, ShowIf(nameof(IsInner), true), SerializeField, LabelText("Inner Index"), Tooltip("Use for Create Mesh Inner Index")]
        private int innerIndex;
        
        private MeshRenderer meshRenderer;

        private MeshFilter meshFilter;
        
        private MeshCollider meshCollider;        
        
        #endregion

        #region Public Properties

        public List<PointCoordinate> OuterPoints
        {
            get
            {
                if (outerPoints != null)
                    return outerPoints;
                
                outerPoints = new List<PointCoordinate>()
                {
                    new(-Scale.x, -Scale.z),
                    new(Scale.x, -Scale.z),
                    new(Scale.x, Scale.z),
                    new(-Scale.x, Scale.z),
                };

                return outerPoints;
            }
        }
        
        public List<PointCoordinate> InnerPoints
        {
            get
            {
                if (innerPoints != null)
                    return innerPoints;
                
                innerPoints = new List<PointCoordinate>()
                {
                    new(-Scale.x / 2, -Scale.z / 2),
                    new(Scale.x / 2, -Scale.z / 2),
                    new(Scale.x / 2, Scale.z / 2),
                    new(-Scale.x / 2, Scale.z / 2),
                };
                
                return innerPoints;
            }
        }
        
        public bool IsInner => isInner;
        
        public MeshRenderer MeshRenderer
        {
            get
            {
                if (meshRenderer != null) 
                    return meshRenderer;
                
                var mr = GetComponent<MeshRenderer>();

                if (mr == null)
                {
                    mr = gameObject.AddComponent<MeshRenderer>();
                }
                    
                meshRenderer = mr;

                return meshRenderer;
            }
        }

        public MeshFilter MeshFilter
        {
            get
            {
                if (meshFilter != null)
                    return meshFilter;
                
                var mf = GetComponent<MeshFilter>();
                
                if (mf == null)
                {
                    mf = gameObject.AddComponent<MeshFilter>();
                }
                
                meshFilter = mf;
                
                return meshFilter;
            }
        }        
        
        public MeshCollider MeshCollider
        {
            get
            {
                if (meshCollider != null)
                    return meshCollider;
                
                var mc = GetComponent<MeshCollider>();
                
                if (mc == null)
                {
                    mc = gameObject.AddComponent<MeshCollider>();
                }
                
                meshCollider = mc;
                
                return meshCollider;
            }
        }        
        
        public int BaseIndex
        {
            get
            {
                if (baseIndex < 0)
                    baseIndex = 0;
                
                if (baseIndex >= OuterPoints.Count)
                    baseIndex = OuterPoints.Count - 1;

                return baseIndex;
            }
        }        
        
        public int InnerIndex
        {
            get
            {
                if (innerIndex < 0)
                    innerIndex = 0;
                
                if (innerIndex >= InnerPoints.Count)
                    innerIndex = InnerPoints.Count - 1;

                return innerIndex;
            }
        }
        
        #endregion

        #region Private Properties

        private Vector3 Position => transform.position;
        public Vector3 Scale => transform.localScale;        
        
        #endregion
    }
}

#endif
