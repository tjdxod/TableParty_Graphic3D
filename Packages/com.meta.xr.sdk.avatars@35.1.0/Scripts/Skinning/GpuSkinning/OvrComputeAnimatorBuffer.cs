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
using System.Runtime.InteropServices;

using Oculus.Avatar2;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using UnityEngine;

namespace Oculus.Skinning.GpuSkinning
{
    internal class OvrComputeAnimatorBuffer : IDisposable
    {
        public ComputeBuffer UnsynchronizedBuffer { get; private set; }

        public int VertexInfosOffset { get; }

        public int DynamicFrameIndex => _currentFrame;

        public OvrComputeAnimatorBuffer(
            in NativeArray<OvrAvatarComputeSkinnedPrimitive.VertexIndices> vertIndices,
            int numJointMatrices,
            int numMorphTargetWeights,
            int vertexBufferMetaDataOffsetBytes,
            int positionOutputOffsetBytes,
            Vector3 positionOutputBias,
            Vector3 positionOutputScale,
            int frenetOutputOffsetBytes)
        {
            // Data layout for "per instance" buffer is
            // [An array of "VertexInfo"] -> one for each vertex. Serves as header
            // [a single mesh_instance_meta_data] -> Since there is only 1 mesh instance (currently)
            // [output_slice as uint] -> output slice
            // [numJointMatrices float4x4] -> An array joint matrices
            // [numMorphTargetWeights floats] -> An array of morph target weights
            // The "header" is the only thing required at a specific location (the begging)
            // everything else is referenced via offsets in the header.

            // Calculate the necessary offset/sizes
            var meshInstanceMetaDataSize = UnsafeUtility.SizeOf<MeshInstanceMetaData>();
            var vertexInfoDataSize = UnsafeUtility.SizeOf<VertexInfo>();

            var sizeOfVertexInfos = vertIndices.Length * vertexInfoDataSize;
            var sizeOfMeshInstanceMetaDatas = meshInstanceMetaDataSize;
            var sizeOfJointMatrices = numJointMatrices * JOINT_MATRIX_STRIDE_BYTES;
            var sizeOfMorphTargetWeights = numMorphTargetWeights * WEIGHTS_STRIDE_BYTES;
            var sizeOfOutputSlice = OUTPUT_SLICE_STRIDE_BYTES;

            VertexInfosOffset = VERTEX_INFO_DATA_OFFSET;
            var meshInstanceMetaDataOffset = VertexInfosOffset + sizeOfVertexInfos;
            var outputSliceOffset = meshInstanceMetaDataOffset + sizeOfMeshInstanceMetaDatas;
            var jointMatricesOffset = outputSliceOffset + sizeOfOutputSlice;
            var morphTargetWeightsOffset = jointMatricesOffset + sizeOfJointMatrices;

            // Create a compute buffer that is "unsynchronized" with direct memory access (if available).
            // Will need to create N number of "mutable sections", one for each frame. The mutable sections
            // will be the parts of the buffer that can potentially change (joint matrices, morph weights, write destination)
            var staticSectionSize = sizeOfVertexInfos + sizeOfMeshInstanceMetaDatas;
            var mutableSize = sizeOfJointMatrices + sizeOfMorphTargetWeights + sizeOfOutputSlice;

            // Update total size to incorporate multiple frames' worth of "mutable size"
            var totalBufferSize = staticSectionSize + mutableSize * GetNumDynamicFrames();
            var computeBuffer = OvrComputeUtils.CreateUnsynchronizedRawComputeBuffer((uint)totalBufferSize);

            // Now write static data and initial data to the unsynchronized buffer
            var wholeBufferArray = computeBuffer.BeginWrite<byte>(0, totalBufferSize);

            /////////////////////////////////////////////////////////////////////////////////////////////////
            // First, create an array of "VertexInfos" which will be at the beginning of the per instance
            // data buffer.
            // Then create single MeshInstanceMetaData
            var vertexInfos = wholeBufferArray.GetSubArray(VertexInfosOffset, sizeOfVertexInfos).Reinterpret<VertexInfo>(sizeof(byte));

            // Write out the "vertex infos"
            InitializeVertexInfos(vertIndices, (uint)meshInstanceMetaDataOffset, ref vertexInfos);

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // Now write out the single MeshInstanceMetaData
            var singleMeshInstanceMetaData = wholeBufferArray
                .GetSubArray(meshInstanceMetaDataOffset, sizeOfMeshInstanceMetaDatas).Reinterpret<MeshInstanceMetaData>(sizeof(byte));

            unsafe
            {
                var metaDataPtr = singleMeshInstanceMetaData.GetPtr();
                metaDataPtr->vertexBufferMetaDataOffsetBytes = (uint)vertexBufferMetaDataOffsetBytes;
                metaDataPtr->morphTargetWeightsOffsetBytes = (uint)morphTargetWeightsOffset;
                metaDataPtr->jointMatricesOffsetBytes = (uint)jointMatricesOffset;
                metaDataPtr->outputPositionsOffsetBytes = (uint)positionOutputOffsetBytes;
                metaDataPtr->outputFrenetOffsetBytes = (uint)frenetOutputOffsetBytes;
                metaDataPtr->outputSliceOffsetBytes = (uint)outputSliceOffset;
                metaDataPtr->mutableFrameStrideBytes = (uint)mutableSize;
                metaDataPtr->vertexOutputPositionBias = positionOutputBias;
                metaDataPtr->vertexOutputPositionScale = positionOutputScale;
            }

            _mutableOffset = staticSectionSize;
            _mutableSize = mutableSize;

            // The "mutators" below have their offsets relative to the start of the "mutable offset"
            outputSliceOffset -= _mutableOffset;
            jointMatricesOffset -= _mutableOffset;
            morphTargetWeightsOffset -= _mutableOffset;

            computeBuffer.EndWrite<byte>(totalBufferSize);

            // Now create the "mutator" for the mutable part of the per instance buffer.
            if (numJointMatrices > 0)
            {
                if (numMorphTargetWeights > 0)
                {
                    // Joints and morphs
                    _bufferMutator = new MorphAndJointsMutator(
                        outputSliceOffset,
                        sizeOfOutputSlice,
                        jointMatricesOffset,
                        sizeOfJointMatrices,
                        morphTargetWeightsOffset,
                        sizeOfMorphTargetWeights);
                }
                else
                {
                    // joints only
                    _bufferMutator = new JointsOnlyMutator(
                        outputSliceOffset,
                        sizeOfOutputSlice,
                        jointMatricesOffset,
                        sizeOfJointMatrices);
                }
            }
            else
            {
                // Morphs only
                _bufferMutator = new MorphsOnlyMutator(
                    outputSliceOffset,
                    sizeOfOutputSlice,
                    morphTargetWeightsOffset,
                    sizeOfMorphTargetWeights);
            }

            _currentFrame = 0;
            UnsynchronizedBuffer = computeBuffer;
        }

        public bool FrameUpdate(
            bool updateJoints,
            bool updateMorphs,
            SkinningOutputFrame outputSlice,
            CAPI.ovrAvatar2EntityId entityId,
            CAPI.ovrAvatar2PrimitiveRenderInstanceID primitiveInstanceId)
        {
            // Advance frame counter
            _currentFrame = (_currentFrame + 1) % GetNumDynamicFrames();

            var thisFrameOffset = _mutableOffset + (_currentFrame * _mutableSize);
            var mutableSection = UnsynchronizedBuffer.BeginWrite<byte>(thisFrameOffset, _mutableSize);

            bool jointsUpdated = _bufferMutator.PopulateMutableSection(
                ref mutableSection,
                updateJoints,
                updateMorphs,
                outputSlice,
                entityId,
                primitiveInstanceId);

            UnsynchronizedBuffer.EndWrite<byte>(_mutableSize);

            return jointsUpdated;
        }

        public virtual void Dispose()
        {
            UnsynchronizedBuffer.Dispose();
            UnsynchronizedBuffer = null;

            _bufferMutator = null;
        }

        private void InitializeVertexInfos(
            in NativeArray<OvrAvatarComputeSkinnedPrimitive.VertexIndices> vertIndices,
            UInt32 meshInstanceMetaDataOffset,
            ref NativeArray<VertexInfo> vertexInfos)
        {
            unsafe
            {
                var vertexInfoPtr = vertexInfos.GetPtr();
                var vertIndexPtr = vertIndices.GetPtr();

                for (int index = 0; index < vertIndices.Length; ++index)
                {
                    vertexInfoPtr->meshInstanceDataOffsetBytes = meshInstanceMetaDataOffset;
                    vertexInfoPtr->vertexBufferIndex = vertIndexPtr->compactSkinningIndex;
                    vertexInfoPtr->outputBufferIndex = vertIndexPtr->outputBufferIndex;

                    vertexInfoPtr++;
                    vertIndexPtr++;
                }
            }
        }

        private const int WEIGHTS_STRIDE_BYTES = sizeof(float); // 32-bit float per morph target
        private const int JOINT_MATRIX_STRIDE_BYTES = 16 * sizeof(float); // 4x4 32-bit float matrices per joint matrix
        private const int OUTPUT_SLICE_STRIDE_BYTES = sizeof(UInt32); // a single 32-bit int

        private const int VERTEX_INFO_DATA_OFFSET = 0; // always 0 (no batching)

        [StructLayout(LayoutKind.Sequential)]
        private struct MeshInstanceMetaData
        {
            // Make sure this matches the shader
            public uint vertexBufferMetaDataOffsetBytes;
            public uint morphTargetWeightsOffsetBytes;
            public uint jointMatricesOffsetBytes;
            public uint outputPositionsOffsetBytes;
            public uint outputFrenetOffsetBytes;
            public uint outputSliceOffsetBytes;

            public uint mutableFrameStrideBytes;

            public Vector3 vertexOutputPositionBias;
            public Vector3 vertexOutputPositionScale;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VertexInfo
        {
            // Make sure this matches the shader
            public uint meshInstanceDataOffsetBytes;
            public uint vertexBufferIndex; // Index in the vertex buffer
            public uint outputBufferIndex; // Index into the output buffer
        }

        private struct JointsMutator
        {
            private readonly int _offset;
            private readonly int _size;

            public JointsMutator(int offset, int size)
            {
                _offset = offset;
                _size = size;
            }

            public bool PopulateJointMatricesBuffer(
                ref NativeArray<byte> mutableSection,
                CAPI.ovrAvatar2EntityId entityId,
                CAPI.ovrAvatar2PrimitiveRenderInstanceID primitiveInstanceId)
            {
                const bool INTERLEAVE_NORMALS = false;

                var jointsSection = mutableSection.GetSubArray(_offset, _size);
                return OvrAvatarSkinnedRenderable.FetchJointMatrices(
                    entityId,
                    primitiveInstanceId,
                    jointsSection.GetIntPtr(),
                    jointsSection.GetBufferSize(),
                    INTERLEAVE_NORMALS,
                    LOG_SCOPE);
            }
        }

        private struct MorphsMutator
        {
            private readonly int _offset;
            private readonly int _size;

            public MorphsMutator(int offset, int size)
            {
                _offset = offset;
                _size = size;
            }

            public void PopulateMorphWeightsBuffer(
                ref NativeArray<byte> mutableSection,
                CAPI.ovrAvatar2EntityId entityId,
                CAPI.ovrAvatar2PrimitiveRenderInstanceID primitiveInstanceId)
            {
                var weights = mutableSection.GetSubArray(_offset, _size);
                OvrAvatarSkinnedRenderable.FetchMorphTargetWeights(
                    entityId,
                    primitiveInstanceId,
                    weights.GetIntPtr(),
                    weights.GetBufferSize(),
                    LOG_SCOPE);
            }
        }

        private struct OutputSliceMutator
        {
            private readonly int _offset;
            private readonly int _size;

            public OutputSliceMutator(int offset, int size)
            {
                _offset = offset;
                _size = size;
            }

            public void PopulateOutputSliceBuffer(ref NativeArray<byte> mutableSection, SkinningOutputFrame outputSlice)
            {
                var sliceInBuffer = mutableSection.GetSubArray(_offset, _size).Reinterpret<UInt32>(sizeof(byte));

                unsafe
                {
                    *(sliceInBuffer.GetPtr()) = (UInt32)outputSlice;
                }
            }
        }

        private abstract class BufferMutator
        {
            protected OutputSliceMutator SliceMutator { get; private set; }

            protected BufferMutator(int sliceOffset, int sliceSize)
            {
                SliceMutator = new OutputSliceMutator(sliceOffset, sliceSize);
            }


            // Returns true if joints are updated.
            public abstract bool PopulateMutableSection(
                ref NativeArray<byte> mutableSection,
                bool updateJoints,
                bool updateMorphs,
                SkinningOutputFrame outputSlice,
                CAPI.ovrAvatar2EntityId entityId,
                CAPI.ovrAvatar2PrimitiveRenderInstanceID primitiveInstanceId);
        }

        private class MorphAndJointsMutator : BufferMutator
        {
            public MorphAndJointsMutator(
                int sliceOffset,
                int sliceSize,
                int matsOffset,
                int matsSize,
                int weightsOffset,
                int weightsSize) : base(sliceOffset, sliceSize)
            {
                _jointsMutator = new JointsMutator(matsOffset, matsSize);
                _morphsMutator = new MorphsMutator(weightsOffset, weightsSize);
            }


            public override bool PopulateMutableSection(
                ref NativeArray<byte> mutableSection,
                bool updateJoints,
                bool updateMorphs,
                SkinningOutputFrame outputSlice,
                CAPI.ovrAvatar2EntityId entityId,
                CAPI.ovrAvatar2PrimitiveRenderInstanceID primitiveInstanceId)
            {
                SliceMutator.PopulateOutputSliceBuffer(ref mutableSection, outputSlice);

                bool jointsUpdated = false;
                if (updateJoints)
                {
                    jointsUpdated = _jointsMutator.PopulateJointMatricesBuffer(ref mutableSection, entityId, primitiveInstanceId);
                }

                if (updateMorphs)
                {
                    _morphsMutator.PopulateMorphWeightsBuffer(ref mutableSection, entityId, primitiveInstanceId);
                }

                return jointsUpdated;
            }

            private readonly JointsMutator _jointsMutator;
            private readonly MorphsMutator _morphsMutator;
        }

        private class MorphsOnlyMutator : BufferMutator
        {
            public MorphsOnlyMutator(
                int sliceOffset,
                int sliceSize,
                int weightsOffset,
                int weightsSize) : base(sliceOffset, sliceSize)
            {
                _morphsMutator = new MorphsMutator(weightsOffset, weightsSize);
            }


            public override bool PopulateMutableSection(
                ref NativeArray<byte> mutableSection,
                bool updateJoints,
                bool updateMorphs,
                SkinningOutputFrame outputSlice,
                CAPI.ovrAvatar2EntityId entityId,
                CAPI.ovrAvatar2PrimitiveRenderInstanceID primitiveInstanceId)
            {
                SliceMutator.PopulateOutputSliceBuffer(ref mutableSection, outputSlice);

                if (updateMorphs)
                {
                    _morphsMutator.PopulateMorphWeightsBuffer(ref mutableSection, entityId, primitiveInstanceId);
                }

                return false;
            }

            private readonly MorphsMutator _morphsMutator;
        }

        private class JointsOnlyMutator : BufferMutator
        {
            public JointsOnlyMutator(
                int sliceOffset,
                int sliceSize,
                int matsOffset,
                int matsSize) : base(sliceOffset, sliceSize)
            {
                _jointsMutator = new JointsMutator(matsOffset, matsSize);
            }


            public override bool PopulateMutableSection(
                ref NativeArray<byte> mutableSection,
                bool updateJoints,
                bool updateMorphs,
                SkinningOutputFrame outputSlice,
                CAPI.ovrAvatar2EntityId entityId,
                CAPI.ovrAvatar2PrimitiveRenderInstanceID primitiveInstanceId)
            {
                SliceMutator.PopulateOutputSliceBuffer(ref mutableSection, outputSlice);

                bool jointsUpdated = false;
                if (updateJoints)
                {
                    jointsUpdated = _jointsMutator.PopulateJointMatricesBuffer(ref mutableSection, entityId, primitiveInstanceId);
                }

                return jointsUpdated;
            }

            private readonly JointsMutator _jointsMutator;
            private readonly MorphsMutator _morphsMutator;
        }

        private const string LOG_SCOPE = "OvrComputeAnimatorBuffer";

        private BufferMutator _bufferMutator;

        // 3 "should" be enough for VR(2 if using low latency mode).
        // use 4 just in case, might be needed in Editor. Increase if seeing really strange behavior
        // outside of VR. If not enough, you could be writing to memory that the GPU is actively using to render.
        // Could change to use a fence/something like that if a set number of "double/triple/quad buffering"
        // is insufficient or wasteful
        private const int NUM_DYNAMIC_FRAMES = 4;
        private const int NUM_DYNAMIC_FRAMES_FOR_BATCHMODE = 256;

        private int GetNumDynamicFrames()
        {
            return Application.isBatchMode ? NUM_DYNAMIC_FRAMES_FOR_BATCHMODE : NUM_DYNAMIC_FRAMES;
        }

        private int _mutableOffset;
        private int _mutableSize;

        private int _currentFrame;
    } // end class OvrComputeAnimatorBuffer
}
