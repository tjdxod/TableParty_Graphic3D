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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using UnityEngine;
// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InvalidXmlDocComment

/**
 * @file OvrAvatarAPI_Types.cs
 */
namespace Oculus.Avatar2
{
    /**
     * @class CAPI
     * Encapsulates C# entry points for Avatar SDK native implementation.
     */
    public static partial class CAPI
    {
        //-----------------------------------------------------------------
        //
        // Opaque ID types
        //
        //

        public enum ovrAvatar2EntityId : Int32
        {
            Invalid = 0
        }

        public enum ovrAvatar2RequestId : Int32
        {
            Invalid = 0
        }

        // TODO: This is Int32 in native
        public enum ovrAvatar2Id : Int32
        {
            Invalid = 0
        }

        public enum ovrAvatar2VertexBufferId : Int32
        {
            Invalid = 0,
        }

        public enum ovrAvatar2MorphTargetBufferId : Int32
        {
            Invalid = 0,
        }

        public enum ovrAvatar2CompactSkinningDataId : Int32
        {
            Invalid = 0,
        }

        public enum ovrAvatar2NodeId : Int32
        {
            Invalid = 0,
        }

        public enum ovrAvatar2LoadRequestId : Int32
        {
            Invalid = 0,
        }

        //-----------------------------------------------------------------
        //
        // Opaque version types
        //
        //

        public enum ovrAvatar2HierarchyVersion : Int32
        {
            Invalid = 0,
        }

        public enum ovrAvatar2EntityRenderStateVersion : Int32
        {
            Invalid = 0,
        }


        //-----------------------------------------------------------------
        //
        // Flags
        //
        //

        /**
         * Configures avatar level of detail.
         * One or more flags may be set.
         *
         * @see ovrAvatar2EntityCreateInfo
         */
        [Flags]
        [System.Serializable]
        public enum ovrAvatar2EntityLODFlags : Int32
        {
            /// level of detail 0 (highest fidelity)
            LOD_0 = 1 << 0,

            /// level of detail 1
            LOD_1 = 1 << 1,

            /// level of detail 2
            LOD_2 = 1 << 2,

            /// level of detail 3
            LOD_3 = 1 << 3,

            /// level of detail 4 (lowest level)
            LOD_4 = 1 << 4,

            /// All levels of detail
            All = LOD_0 | LOD_1 | LOD_2 | LOD_3 | LOD_4,
        }

        public const uint ovrAvatar2EntityLODFlagsCount = 5;

        /**
         * Configures how the avatar is manifested
         * (full body, head and hands only).
         * NOTE: Only Half is currently available
         *
         * @see ovrAvatar2EntityCreateInfo
         */
        [Flags]
        [System.Serializable]
        public enum ovrAvatar2EntityManifestationFlags : Int32
        {
            /// No avatar parts manifested
            None = 0,

            /// All body parts
            Full = 1 << 0,

            /// Upper body only
            Half = 1 << 1,

            /// Head and hands only
            HeadHands = 1 << 2,

            /// Head only
            Head = 1 << 3,

            /// Hands only
            Hands = 1 << 4,

            ///  All manifestations requested.
            All = Full | Half | HeadHands | Head | Hands,
        }

        /**
         * Configures how the avatar is viewed
         * (first person, third person).
         *
         * @see ovrAvatar2EntityCreateInfo
         */
        [Flags]
        [System.Serializable]
        public enum ovrAvatar2EntityViewFlags : Int32
        {
            None = 0,

            /// First person view
            FirstPerson = 1 << 0,

            /// Third person view
            ThirdPerson = 1 << 1,

            /// All views
            All = FirstPerson | ThirdPerson
        }

        /**
         * Configures what sub-meshes of the avatar
         * will show.
         *
         * @see ovrAvatar2EntityMaterialTypes_
         */
        [Flags]
        [System.Serializable]
        public enum ovrAvatar2EntitySubMeshInclusionFlags : Int32
        {
            None = 0,

            /// Outfit only
            Outfit = 1 << 0,

            /// Body only
            Body = 1 << 1,

            /// Head only
            Head = 1 << 2,

            /// Hair only
            Hair = 1 << 3,

            /// Eyebrow only
            Eyebrow = 1 << 4,

            /// L Eye only
            L_Eye = 1 << 5,

            /// R Eye only
            R_Eye = 1 << 6,

            /// Lashes only
            Lashes = 1 << 7,

            /// Facial hair only
            FacialHair = 1 << 8,

            /// Headwear only
            Headwear = 1 << 9,

            /// Earrings only
            Earrings = 1 << 10,

            /// Mouth only
            Mouth = 1 << 11,

            /// IMPORTANT: SubMesh_All_Exclusive (below) needs to be updated as new enumerations are added

            ///  Works both as a test and also might be useful in some real applications.
            BothEyes = L_Eye | R_Eye,

            ///  All manifestations requested. To accomodate the Unity IDE and
            ///  prefabs, his -1 value should very intentionally keep all bits filled.
            ///  As flags are added, they'll be part of this value without entity updates.
            All = -1,
        }

        // "All exclusive" is useful for conditional statements as it is returned from the SDK,
        // where the boolean flags are analytically calculated based on what is found in the
        // asset. If the primitive has all flags enabled, higher order bits will still be zero.
        public const Int32 SubMesh_All_Exclusive = -(1 - ((Int32)ovrAvatar2EntitySubMeshInclusionFlags.Mouth));

        /**
         * Configures how the avatar is loaded and displayed
         *
         * @see ovrAvatar2EntityCreateInfo
         */
        [System.Serializable]
        public enum ovrAvatar2EntityQuality : Int32
        {
            // Default quality with normal maps and hair maps.
            Standard = 0,

            // Lower quality but lighter-weight. No normal maps. Metallic-roughness and skin shading still on.
            Light = 1,

            // Extremely light. No textures, only vertex colors. LODs 2 and 4 only.
            Ultralight = 2
        }

        [Flags]
        public enum ovrAvatar2EntityQualityFlags : Int32
        {
            None = 0,
            Standard = 1 << 0,
            Light = 1 << 1,
            Ultralight = 1 << 2,
            All = Standard | Light | Ultralight
        }

        public enum ovrAvatar2DataFormat : Int32
        {
            Invalid = 0,
            U8 = 1,

            ///< Unsigned 8 bit integer
            U16 = 2,

            ///< Unsigned 16 bit integer
            U32 = 3,

            ///< Unsigned 32 bit integer
            S8 = 4,

            ///< Signed 8 bit integer
            S16 = 5,

            ///< Signed 16 bit integer
            S32 = 6,

            ///< Signed 32 bit integer
            F16 = 7,

            ///< 16 bit floating point number
            F32 = 8,

            ///< 32 bit floating point number
            Unorm8 = 9,

            ///< 8 bit unsigned normalized number
            Unorm10_10_10_2 = 10,

            /// < 4 unsigned normalized components (10/10/10/2 bits
            /// )
            Unorm16 = 11,

            ///< 16 bit unsigned normalized number
            Snorm8 = 12,

            ///< 8 bit signed normalized number
            Snorm10_10_10_2 = 13,

            /// < 4 signed normalized components (10/10/10/2 bits
            /// )
            Snorm16 = 14, ///< 16 bit signed normalized number
        }

        public enum ovrAvatar2CompactMeshAttributes : Int32
        {
            ///< Usually an error, for completeness.
            None = 0,
            ///< Vertex Colors and material type
            Colors = 1 << 0,
            ///< Will be present if the primitive has textures.
            TexCoord0 = 1 << 1,
            ///< Vertex Colors ORMT/OFSB
            Properties = 1 << 2,
            ///< Curvature information for skin
            Curvature = 1 << 3,

            All = TexCoord0 | Colors | Properties | Curvature,
        }

        public const int ovrAvatar2CompactMeshAttributesCount = 4;

        //-----------------------------------------------------------------
        //
        // Math
        //
        //

        /// 2D Vector Type
        [StructLayout(LayoutKind.Sequential)]
        public struct ovrAvatar2Vector2f
        {
            public float x;
            public float y;

            public static implicit operator ovrAvatar2Vector2f(Vector2 v)
            {
                ovrAvatar2Vector2f result = new ovrAvatar2Vector2f();
                result.x = v.x;
                result.y = v.y;
                return result;
            }

            public static implicit operator Vector2(ovrAvatar2Vector2f v)
            {
                return new Vector3(v.x, v.y);
            }
        };

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ovrAvatar2Vector2u
        {
            public readonly UInt32 x;
            public readonly UInt32 y;

            public ovrAvatar2Vector2u(UInt32 X, UInt32 Y)
            {
                x = X;
                y = Y;
            }
        };

        /// 3D Vector Type
        [StructLayout(LayoutKind.Sequential)]
        public struct ovrAvatar2Vector3f
        {
            public float x;
            public float y;
            public float z;

            public float Length => Mathf.Sqrt(LengthSquared);
            public float LengthSquared => (x * x + y * y + z * z);

            public ovrAvatar2Vector3f(float x_, float y_, float z_)
            {
                x = x_;
                y = y_;
                z = z_;
            }

            public static implicit operator ovrAvatar2Vector3f(in Vector3 v)
            {
                return new ovrAvatar2Vector3f(v.x, v.y, v.z);
            }

            public static implicit operator Vector3(in ovrAvatar2Vector3f v)
            {
                return new Vector3(v.x, v.y, v.z);
            }

            public static ovrAvatar2Vector3f operator +(in ovrAvatar2Vector3f lhs, in ovrAvatar2Vector3f rhs)
            {
                return new ovrAvatar2Vector3f
                (
                    lhs.x + rhs.x,
                    lhs.y + rhs.y,
                    lhs.z + rhs.z
                );
            }

            public static ovrAvatar2Vector3f operator -(in ovrAvatar2Vector3f lhs, in ovrAvatar2Vector3f rhs)
            {
                return new ovrAvatar2Vector3f
                (
                    lhs.x - rhs.x,
                    lhs.y - rhs.y,
                    lhs.z - rhs.z
                );
            }

            public static ovrAvatar2Vector3f operator *(in ovrAvatar2Vector3f vec, float scale)
            {
                return new ovrAvatar2Vector3f
                (
                    vec.x * scale,
                    vec.y * scale,
                    vec.z * scale
                );
            }

            public static ovrAvatar2Vector3f operator /(in ovrAvatar2Vector3f numer, float denom)
            {
                return numer * (1.0f / denom);
            }

            [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
            public static bool operator ==(in ovrAvatar2Vector3f lhs, in ovrAvatar2Vector3f rhs)
            {
                return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
            }
            public static bool operator !=(in ovrAvatar2Vector3f lhs, in ovrAvatar2Vector3f rhs) => !(lhs == rhs);
            public bool Equals(ovrAvatar2Vector3f other) => this == other;
            public override bool Equals(object? obj) => obj is ovrAvatar2Vector3f other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(x, y, z);
        }

        /// 4D Vector Type
        [StructLayout(LayoutKind.Sequential)]
        public struct ovrAvatar2Vector4f : IEquatable<ovrAvatar2Vector4f>
        {
            public float x;
            public float y;
            public float z;
            public float w;

            public ovrAvatar2Vector4f(float x_, float y_, float z_, float w_)
            {
                x = x_;
                y = y_;
                z = z_;
                w = w_;
            }

            public static implicit operator ovrAvatar2Vector4f(in Vector4 v)
            {
                return new ovrAvatar2Vector4f(v.x, v.y, v.z, v.w);
            }

            public static implicit operator Vector4(in ovrAvatar2Vector4f v)
            {
                return new Vector4(v.x, v.y, v.z, v.w);
            }

            public static implicit operator Color(in ovrAvatar2Vector4f v)
            {
                return new Color(v.x, v.y, v.z, v.w);
            }

            [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
            public static bool operator ==(in ovrAvatar2Vector4f lhs, in ovrAvatar2Vector4f rhs)
            {
                return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
            }
            public static bool operator !=(in ovrAvatar2Vector4f lhs, in ovrAvatar2Vector4f rhs) => !(lhs == rhs);
            public bool Equals(ovrAvatar2Vector4f other) => this == other;
            public override bool Equals(object? obj) => obj is ovrAvatar2Vector4f other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(x, y, z, w);
        }

        /// 4D Vector Type
        [StructLayout(LayoutKind.Sequential)]
        public struct ovrAvatar2Vector4ub
        {
            public Byte x;
            public Byte y;
            public Byte z;
            public Byte w;
        }

        /// 4D Vector Type
        [StructLayout(LayoutKind.Sequential)]
        public struct ovrAvatar2Vector4us
        {
            public UInt16 x;
            public UInt16 y;
            public UInt16 z;
            public UInt16 w;
        };

        /// Quaternion Type
        [StructLayout(LayoutKind.Sequential)]
        public struct ovrAvatar2Quatf : IEquatable<ovrAvatar2Quatf>
        {
            public float x;
            public float y;
            public float z;
            public float w;

            public float Length => Mathf.Sqrt(LengthSquared);
            public float LengthSquared => ((x * x) + (y * y) + (z * z) + (w * w));

            public ovrAvatar2Quatf(float x_, float y_, float z_, float w_)
            {
                x = x_;
                y = y_;
                z = z_;
                w = w_;
            }

            public static implicit operator ovrAvatar2Quatf(in Quaternion q)
            {
                return new ovrAvatar2Quatf(q.x, q.y, q.z, q.w);
            }

            public static implicit operator Quaternion(in ovrAvatar2Quatf q)
            {
                return new Quaternion(q.x, q.y, q.z, q.w);
            }

            [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
            public static bool operator ==(in ovrAvatar2Quatf lhs, in ovrAvatar2Quatf rhs)
            {
                return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
            }
            public static bool operator !=(in ovrAvatar2Quatf lhs, in ovrAvatar2Quatf rhs) => !(lhs == rhs);
            public bool Equals(ovrAvatar2Quatf other) => this == other;
            public override bool Equals(object? obj) => obj is ovrAvatar2Quatf other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(x, y, z, w);

            public static ovrAvatar2Vector3f operator *(in ovrAvatar2Quatf quat, in ovrAvatar2Vector3f vec)
            {
                return (ovrAvatar2Vector3f)((Quaternion)quat * (Vector3)vec);
            }
        };


        /// Transform Type
        [StructLayout(LayoutKind.Sequential)]
        public struct ovrAvatar2Transform : IEquatable<ovrAvatar2Transform>
        {
            public ovrAvatar2Vector3f position;
            public ovrAvatar2Quatf orientation;
            public ovrAvatar2Vector3f scale;

            public ovrAvatar2Transform(in ovrAvatar2Vector3f position_, in ovrAvatar2Quatf orientation_)
            {
                position = position_;
                orientation = orientation_;
                scale.x = scale.y = scale.z = 1.0f;
            }

            public ovrAvatar2Transform(in ovrAvatar2Vector3f position_
                , in ovrAvatar2Quatf orientation_, in ovrAvatar2Vector3f scale_)
            {
                position = position_;
                orientation = orientation_;
                scale = scale_;
            }

            public ovrAvatar2Transform(in Vector3 position_
                , in Quaternion orientation_, in Vector3 scale_)
            {
                position = position_;
                orientation = orientation_;
                scale = scale_;
            }

            public static bool operator ==(in ovrAvatar2Transform lhs, in ovrAvatar2Transform rhs)
            {
                return lhs.position == rhs.position && lhs.orientation == rhs.orientation && lhs.scale == rhs.scale;
            }
            public static bool operator !=(in ovrAvatar2Transform lhs, in ovrAvatar2Transform rhs) => !(lhs == rhs);
            public bool Equals(ovrAvatar2Transform other) => this == other;
            public override bool Equals(object? obj) => obj is ovrAvatar2Transform other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(position, orientation, scale);

            public static explicit operator ovrAvatar2Transform(Transform t)
            {
                return new ovrAvatar2Transform(t.localPosition, t.localRotation, t.localScale);
            }
        };

        // Matrix Type
        [StructLayout(LayoutKind.Sequential)]
        public struct ovrAvatar2Matrix4f
        {
            internal float m00, m10, m20, m30;
            internal float m01, m11, m21, m31;
            internal float m02, m12, m22, m32;
            internal float m03, m13, m23, m33;

            public float this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return this.m00;
                        case 1:
                            return this.m10;
                        case 2:
                            return this.m20;
                        case 3:
                            return this.m30;
                        case 4:
                            return this.m01;
                        case 5:
                            return this.m11;
                        case 6:
                            return this.m21;
                        case 7:
                            return this.m31;
                        case 8:
                            return this.m02;
                        case 9:
                            return this.m12;
                        case 10:
                            return this.m22;
                        case 11:
                            return this.m32;
                        case 12:
                            return this.m03;
                        case 13:
                            return this.m13;
                        case 14:
                            return this.m23;
                        case 15:
                            return this.m33;
                        default:
                            throw new IndexOutOfRangeException("Invalid matrix index!");
                    }
                }
                set
                {
                    switch (index)
                    {
                        case 0:
                            this.m00 = value;
                            break;
                        case 1:
                            this.m10 = value;
                            break;
                        case 2:
                            this.m20 = value;
                            break;
                        case 3:
                            this.m30 = value;
                            break;
                        case 4:
                            this.m01 = value;
                            break;
                        case 5:
                            this.m11 = value;
                            break;
                        case 6:
                            this.m21 = value;
                            break;
                        case 7:
                            this.m31 = value;
                            break;
                        case 8:
                            this.m02 = value;
                            break;
                        case 9:
                            this.m12 = value;
                            break;
                        case 10:
                            this.m22 = value;
                            break;
                        case 11:
                            this.m32 = value;
                            break;
                        case 12:
                            this.m03 = value;
                            break;
                        case 13:
                            this.m13 = value;
                            break;
                        case 14:
                            this.m23 = value;
                            break;
                        case 15:
                            this.m33 = value;
                            break;
                        default:
                            throw new IndexOutOfRangeException("Invalid matrix index!");
                    }
                }
            }

            public static explicit operator ovrAvatar2Matrix4f(in Matrix4x4 m) => m.ToAvatarMatrix();
            public static explicit operator Matrix4x4(in ovrAvatar2Matrix4f m) => m.ToUnityMatrix();
        }

        // TODO: T177790998 - it is not safe for `ovrAvatar2Pose` to be copied off the stack
        [StructLayout(LayoutKind.Sequential)]
        public readonly unsafe /*ref*/ struct ovrAvatar2Pose
        {
            public readonly UInt32 jointCount;
            public readonly ovrAvatar2Transform* localTransforms; // Array of ovrAvatar2Transforms
            public readonly ovrAvatar2Transform* objectTransforms; // Array of ovrAvatar2Transforms relative to root
            internal readonly Int32* parents; // Array of Int32
            internal readonly ovrAvatar2NodeId* nodeIds; ///< Array of node ids

            public bool HasNodeIds => nodeIds != null;

            public Int32 GetParentIndex(Int32 childIndex)
            {
                if (childIndex < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        $"Index {childIndex} is out of range of pose parent array of size {jointCount}");
                }
                return GetParentIndex((UInt32)childIndex);
            }

            public Int32 GetParentIndex(UInt32 childIndex)
            {
                if (childIndex >= jointCount)
                {
                    throw new ArgumentOutOfRangeException(
                        $"Index {childIndex} is over range of pose parent array of size {jointCount}");
                }
                if (parents == null)
                {
                    throw new NullReferenceException("parents array is null");
                }
                return parents[childIndex];
            }

            public ovrAvatar2NodeId GetNodeIdAtIndex(Int32 index)
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        $"Index {index} is out of range of pose nodeIds array of size {jointCount}");
                }
                return GetNodeIdAtIndex((UInt32)index);
            }
            public ovrAvatar2NodeId GetNodeIdAtIndex(UInt32 index)
            {
                if (index >= jointCount)
                {
                    throw new ArgumentOutOfRangeException(
                        $"Index {index} is over range of pose nodeIds array of size {jointCount}");
                }
                if (nodeIds == null)
                {
                    throw new NullReferenceException("nodeIds array is null");
                }
                Debug.Assert(index < jointCount);
                return nodeIds[index];
            }

            public bool IsValid(float tolerance = 0.0f)
            {
                if (jointCount == 0 || localTransforms == null || objectTransforms == null || parents == null ||
                    nodeIds == null)
                {
                    return false;
                }

                for (int idx = 0; idx < jointCount; ++idx)
                {
                    if (!localTransforms[idx].IsValid(tolerance) || !objectTransforms[idx].IsValid(tolerance))
                    {
                        return false;
                    }
                }

                return true;
            }

            // Mocks for Unit Tests
            internal ovrAvatar2Pose(
                UInt32 jointCount_,
                ovrAvatar2Transform* localTransforms_,
                ovrAvatar2Transform* objectTransforms_,
                Int32* parents_,
                ovrAvatar2NodeId* nodeIds_)
            {
                jointCount = jointCount_;
                localTransforms = localTransforms_;
                objectTransforms = objectTransforms_;
                parents = parents_;
                nodeIds = nodeIds_;
            }
        }

        public enum ovrAvatar2Side : Int32
        {
            Left = 0,
            Right = 1,
            Count = 2
        }

        //-----------------------------------------------------------------
        //
        // Results
        //
        //

        public enum ovrAvatar2Result : Int32
        {
            Success = 0,
            Unknown = 1,
            OutOfMemory = 2,
            NotInitialized = 3,
            AlreadyInitialized = 4,
            BadParameter = 5,
            Unsupported = 6,
            NotFound = 7,
            AlreadyExists = 8,
            IndexOutOfRange = 9,
            InvalidEntity = 10,
            Reserved_11 = 11, // Formerly InvalidThread, but the native side is neutral about this now
            BufferTooSmall = 12,
            DataNotAvailable = 13,
            InvalidData = 14,
            SkeletonMismatch = 15,
            LibraryLoadFailed = 16,
            Pending = 17,
            MissingAccessToken = 18,
            MemoryLeak = 19,
            RequestCallbackNotSet = 20,
            UnmatchedLoadFilters = 21,
            DeserializationPending = 22,
            StaticJointTypeFallback = 23,
            UnableToConnectToDevTools = 24,
            RequestCancelled = 25,
            BufferLargerThanExpected = 26,
            BufferMisaligned = 27,
            TypeMismatch = 28,

            Count,
        }

        //-----------------------------------------------------------------
        //
        // Joints
        //

        public enum ovrAvatar2JointType : Int32
        {
            Invalid = -1,

            Root = 0,
            Hips = 1,
            LeftLegUpper = 2,
            LeftLegLower = 3,
            LeftFootAnkle = 4,
            LeftFootBall = 5,
            RightLegUpper = 6,
            RightLegLower = 7,
            RightFootAnkle = 8,
            RightFootBall = 9,
            SpineLower = 10,
            SpineMiddle = 11,
            SpineUpper = 12,
            Chest = 13,
            Neck = 14,
            Head = 15,
            LeftShoulder = 16,
            LeftArmUpper = 17,
            LeftArmLower = 18,
            LeftHandWrist = 19,
            RightShoulder = 20,
            RightArmUpper = 21,
            RightArmLower = 22,
            RightHandWrist = 23,
            LeftHandThumbTrapezium = 24,
            LeftHandThumbMeta = 25,
            LeftHandThumbProximal = 26,
            LeftHandThumbDistal = 27,
            LeftHandIndexMeta = 28,
            LeftHandIndexProximal = 29,
            LeftHandIndexIntermediate = 30,
            LeftHandIndexDistal = 31,
            LeftHandMiddleMeta = 32,
            LeftHandMiddleProximal = 33,
            LeftHandMiddleIntermediate = 34,
            LeftHandMiddleDistal = 35,
            LeftHandRingMeta = 36,
            LeftHandRingProximal = 37,
            LeftHandRingIntermediate = 38,
            LeftHandRingDistal = 39,
            LeftHandPinkyMeta = 40,
            LeftHandPinkyProximal = 41,
            LeftHandPinkyIntermediate = 42,
            LeftHandPinkyDistal = 43,
            RightHandThumbTrapezium = 44,
            RightHandThumbMeta = 45,
            RightHandThumbProximal = 46,
            RightHandThumbDistal = 47,
            RightHandIndexMeta = 48,
            RightHandIndexProximal = 49,
            RightHandIndexIntermediate = 50,
            RightHandIndexDistal = 51,
            RightHandMiddleMeta = 52,
            RightHandMiddleProximal = 53,
            RightHandMiddleIntermediate = 54,
            RightHandMiddleDistal = 55,
            RightHandRingMeta = 56,
            RightHandRingProximal = 57,
            RightHandRingIntermediate = 58,
            RightHandRingDistal = 59,
            RightHandPinkyMeta = 60,
            RightHandPinkyProximal = 61,
            RightHandPinkyIntermediate = 62,
            RightHandPinkyDistal = 63,

            Count
        }

        //-----------------------------------------------------------------
        //
        // Data types
        //
        //

        // A mutable span (block/bag) of mutable bytes
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct ovrAvatar2DataBlock
        {
            public byte* data;
            public UInt64 size;
        }

        // A fixed (at runtime) span (block/bag) of mutable bytes
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct ovrAvatar2DataSpan
        {
            public byte* data;
            public UInt64 size;
        }

        // Mutable access into fixed size span
        [StructLayout(LayoutKind.Sequential)]
        public struct ovrAvatar2DataBuffer
        {
            // Fixed size memory block into which data will be written
            ovrAvatar2DataSpan data;
            // Valid number of bytes in `data`
            public UInt64 bytesWritten;
        }


        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ovrAvatar2BufferMetaData
        {
            public readonly ovrAvatar2DataFormat dataFormat;
            public readonly UInt32 dataSizeBytes;
            public readonly UInt32 strideBytes;
            public readonly UInt32 count;
        }
    }
}
