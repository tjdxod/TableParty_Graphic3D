#ifndef OVR_AVATAR_MOTION_VECTORS_POSITION_INCLUDED
#define OVR_AVATAR_MOTION_VECTORS_POSITION_INCLUDED

#include "OvrAvatarVertexFetch.hlsl"
#include "OvrAvatarCommonVertexParams.hlsl"

float _OvrPrevRenderFrameInterpolationValue;
int _OvrAttributeOutputPrevRenderFrameLatestAnimFrameOffset;
int _OvrAttributeOutputPrevRenderFramePrevAnimFrameOffset;


float3 OvrMotionVectorsGetObjectSpacePositionFromTexture(uint vertexId, int numAttributes, bool applyOffsetAndBias) {
  return OvrGetVertexPositionFromTexture(vertexId, numAttributes, applyOffsetAndBias, _OvrAttributeInterpolationValue).xyz;
}

float3 OvrMotionVectorsGetPrevObjectSpacePositionFromTexture(uint vertexId, int numAttributes, bool applyOffsetAndBias) {
  return OvrGetVertexPositionFromTexture(vertexId, numAttributes, applyOffsetAndBias, _OvrPrevRenderFrameInterpolationValue).xyz;
}

#if defined(OVR_SUPPORT_EXTERNAL_BUFFERS)
  float3 OvrMotionVectorsGetObjectSpacePositionFromBuffer(uint vertexId) {
    const uint numPosEntriesPerVert = _OvrNumOutputEntriesPerAttribute;
    const uint startIndexOfPositionEntries = vertexId * numPosEntriesPerVert;
    const uint posEntryIndex = startIndexOfPositionEntries + _OvrAttributeOutputLatestAnimFrameEntryOffset;

    return OvrGetPositionEntryFromExternalBuffer(posEntryIndex);
  }

  float3 OvrMotionVectorsGetInterpolatedObjectSpacePositionFromBuffer(uint vertexId) {
    const uint numPosEntriesPerVert = _OvrNumOutputEntriesPerAttribute;
    const uint startIndexOfPositionEntries = vertexId * numPosEntriesPerVert;

    const uint latestPosEntryIndex = startIndexOfPositionEntries + _OvrAttributeOutputLatestAnimFrameEntryOffset;
    const uint prevPosEntryIndex = startIndexOfPositionEntries + _OvrAttributeOutputPrevAnimFrameEntryOffset;

    const float3 p0 = OvrGetPositionEntryFromExternalBuffer(prevPosEntryIndex);
    const float3 p1 = OvrGetPositionEntryFromExternalBuffer(latestPosEntryIndex);

    return lerp(p0, p1, _OvrAttributeInterpolationValue);
  }

  float3 OvrMotionVectorsGetPrevObjectSpacePositionFromBuffer(uint vertexId) {
    const uint numPosEntriesPerVert = _OvrNumOutputEntriesPerAttribute;
    const uint startIndexOfPositionEntries = vertexId * numPosEntriesPerVert;
    const uint posEntryIndex = startIndexOfPositionEntries + _OvrAttributeOutputPrevRenderFrameLatestAnimFrameOffset;

    return OvrGetPositionEntryFromExternalBuffer(posEntryIndex);
  }

  float3 OvrMotionVectorsGetPrevInterpolatedObjectSpacePositionFromBuffer(uint vertexId) {
    const uint numPosEntriesPerVert = _OvrNumOutputEntriesPerAttribute;
    const uint startIndexOfPositionEntries = vertexId * numPosEntriesPerVert;

    const uint pos1EntryIndex = startIndexOfPositionEntries + _OvrAttributeOutputPrevRenderFrameLatestAnimFrameOffset;
    const uint pos0EntryIndex = startIndexOfPositionEntries + _OvrAttributeOutputPrevRenderFramePrevAnimFrameOffset;

    const float3 p0 = OvrGetPositionEntryFromExternalBuffer(pos0EntryIndex);
    const float3 p1 = OvrGetPositionEntryFromExternalBuffer(pos1EntryIndex);

    return lerp(p0, p1, _OvrPrevRenderFrameInterpolationValue);
  }
#endif

void OvrMotionVectorsGetPositions(uint vertexId, inout float3 currentPos, inout float3 prevPos)
{
  // Backward compatibility/optimization support if application is ok with additional variants
  // The shader compiler should optimize out branches that are based on static const values
  #if defined(OVR_VERTEX_FETCH_VERT_BUFFER)
    static const int fetchMode = OVR_VERTEX_FETCH_MODE_STRUCT;
  #elif defined(OVR_VERTEX_FETCH_EXTERNAL_BUFFER) && defined(OVR_SUPPORT_EXTERNAL_BUFFERS)
    static const int fetchMode = OVR_VERTEX_FETCH_MODE_EXTERNAL_BUFFERS;
  #elif defined(OVR_VERTEX_FETCH_TEXTURE) || defined(OVR_VERTEX_FETCH_TEXTURE_UNORM)
    static const int fetchMode = OVR_VERTEX_FETCH_MODE_EXTERNAL_TEXTURES;
  #else
    const int fetchMode = _OvrVertexFetchMode;
  #endif

  #if defined(OVR_VERTEX_HAS_TANGENTS)
    static const bool hasTangents = true;
  #elif defined(OVR_VERTEX_NO_TANGENTS)
    static const bool hasTangents = false;
  #else
    const bool hasTangents = _OvrHasTangents;
  #endif

  #if defined(OVR_VERTEX_INTERPOLATE_ATTRIBUTES)
    static const bool interpolateAttributes = true;
  #elif defined(OVR_VERTEX_DO_NOT_INTERPOLATE_ATTRIBUTES)
    static const bool interpolateAttributes = false;
  #else
    const bool interpolateAttributes = _OvrInterpolateAttributes;
  #endif

  // Hope that the compiler branches here. The [branch] attribute here seems to lead to compile
  // probably due to "use of gradient function, such as tex3d"
  if (fetchMode == OVR_VERTEX_FETCH_MODE_EXTERNAL_TEXTURES) {
    const int numAttributes =  hasTangents ? 3 : 2;

    #if defined(OVR_VERTEX_FETCH_TEXTURE)
        static const bool applyOffsetAndBias = false;
    #else
        static const bool applyOffsetAndBias = true;
    #endif

    if (u_IsExternalAttributeSourceValid) {
      currentPos = OvrMotionVectorsGetObjectSpacePositionFromTexture(vertexId, numAttributes, applyOffsetAndBias);
      prevPos = OvrMotionVectorsGetPrevObjectSpacePositionFromTexture(vertexId, numAttributes, applyOffsetAndBias);
    } else {
      currentPos = 0.0;
      prevPos = 0.0;
    }
#ifdef OVR_SUPPORT_EXTERNAL_BUFFERS
  } else if (fetchMode == OVR_VERTEX_FETCH_MODE_EXTERNAL_BUFFERS) {

    // Make degenerate triangle of source isn't valid
    if (u_IsExternalAttributeSourceValid) {
      if (interpolateAttributes) {
        currentPos = OvrMotionVectorsGetInterpolatedObjectSpacePositionFromBuffer(vertexId);
        prevPos = OvrMotionVectorsGetPrevInterpolatedObjectSpacePositionFromBuffer(vertexId);
      } else {
        currentPos = OvrMotionVectorsGetObjectSpacePositionFromBuffer(vertexId);
        prevPos = OvrMotionVectorsGetPrevObjectSpacePositionFromBuffer(vertexId);
      }
    } else {
      currentPos = 0.0;
      prevPos = 0.0;
    }
#endif
  }
}


#endif // OVR_AVATAR_MOTION_VECTORS_POSITION_INCLUDED
