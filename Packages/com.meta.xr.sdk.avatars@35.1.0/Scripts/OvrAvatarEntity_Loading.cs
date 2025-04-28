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

// TODO: Delete when multi LOD primitives align correctly when rendered
#define OVR_AVATAR_HIDE_CONTROLLER_PRIMITIVES

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace Oculus.Avatar2
{
    public partial class OvrAvatarEntity : MonoBehaviour
    {
        /////////////////////////////////////////////////
        //:: Deprecated LoadingState APIs

        #region Deprecated LoadingState APIs

        public enum LoadingState
        {
            Failed = -1,
            NotCreated = 0,
            Created,
            Loading,
            Success,
        }

        public enum AssetSource
        {
            /// Load from one of the preloaded .zip files
            Zip,

            /// Load a loose glb file directly from StreamingAssets
            StreamingAssets,

        }

        [Obsolete("Deprecated, please refer to documentation")]
        public LoadingState LoadState { get; protected set; }
        private LoadingState _lastInvokedLoadState = LoadingState.NotCreated;

        [Obsolete("Deprecated, please refer to documentation")]
        [HideInInspector]
        public class LoadingStateEvent : UnityEvent<LoadingState> { }

        [Obsolete("Deprecated, please refer to documentation")]
        [HideInInspector]
        public class EntityLoadingStateEvent : UnityEvent<OvrAvatarEntity> { }

        [Header("Events (Deprecated)")]
        [Obsolete("Deprecated, please refer to documentation")]
        public LoadingStateEvent LoadingStateChanged = new LoadingStateEvent();
        [Obsolete("Deprecated, please refer to documentation")]
        public EntityLoadingStateEvent EntityLoadingStateChanged = new EntityLoadingStateEvent();

        #endregion

        /////////////////////////////////////////////////
        //:: Public API

        #region Public APIs

        ///
        /// Represents what is currently loaded & ready to be used/accessed on this Avatar Entity
        /// Order is guaranteed:
        ///   None -> Created -> Skeleton -> DefaultAvatar (if enabled) -> UserAvatar
        ///
        /// If a load step fails, the sequence will not advance
        ///
        /// If the user avatar is destroyed, it calls "PreTeardown" and before returning to state None
        ///
        public enum AvatarState
        {
            /// Initial state - Not yet created
            None = 0,

            /// Native Avatar Entity has been created
            Created,

            //------
            // Enum values after this are guaranteed to have a skeleton loaded
            // ----

            /// Skeleton has loaded, but no model. Streaming/Animation APIs available, attachments, etc.
            Skeleton,

            /// (Optional) Default Avatar is loaded and renderable
            DefaultAvatar,

            /// (Optional) Fast Load Avatar is loaded and renderable
            FastLoad,

            /// A non-default (CDN or preset) avatar is loaded and renderable
            UserAvatar
        }

        public AvatarState CurrentState { get; protected set; } = AvatarState.None;

        [Serializable]
        public class AvatarStateEvent : UnityEvent<OvrAvatarEntity> { }

        [Serializable]
        public class AvatarLoadFailedEvent : UnityEvent<OvrAvatarEntity, CAPI.ovrAvatar2LoadRequestInfo> { }

        [CollapsibleSectionStart("Events", true)]
        /// Called when native avatar is created, after entering state AvatarState.Created
        public AvatarStateEvent OnCreatedEvent = new AvatarStateEvent();

        /// Called when skeleton is loaded, after entering state AvatarState.Skeleton
        public AvatarStateEvent OnSkeletonLoadedEvent = new AvatarStateEvent();

        /// Called when Default Avatar is loaded, after entering state AvatarState.DefaultAvatar
        public AvatarStateEvent OnDefaultAvatarLoadedEvent = new AvatarStateEvent();

        /// Called when Fast Load Avatar is loaded, after entering state AvatarState.FastLoad
        public AvatarStateEvent OnFastLoadAvatarLoadedEvent = new AvatarStateEvent();

        /// Called the first time a User Avatar is loaded, after entering state AvatarState.UserAvatar
        public AvatarStateEvent OnUserAvatarLoadedEvent = new AvatarStateEvent();

        /// Called at the start of Teardown() before reverting to state AvatarState.None
        public AvatarStateEvent PreTeardownEvent = new AvatarStateEvent();

        [CollapsibleSectionEnd]
        /// Called when a load fails
        public AvatarLoadFailedEvent OnLoadFailedEvent = new AvatarLoadFailedEvent();

        public enum AvatarStyle
        {
            InvalidStyle = 0, // Indicating there's actually an error with detecting the style
            Style1 = 1,
            Style2 = 2,
        }
        #endregion

        /////////////////////////////////////////////////
        //:: Private/Protected State

        #region Private/Protected State

        // TODO: This can probably be consolidated w/ `_primitiveRenderables`
        private readonly Dictionary<CAPI.ovrAvatar2NodeId, PrimitiveRenderData[]> _meshNodes
            = new Dictionary<CAPI.ovrAvatar2NodeId, PrimitiveRenderData[]>();

        private uint primitiveRenderCount => (uint)_visiblePrimitiveRenderers.Length;
        // Used to quickly detect when render state has changed
        private CAPI.ovrAvatar2HierarchyVersion _currentHierarchyVersion
            = CAPI.ovrAvatar2HierarchyVersion.Invalid;
        private CAPI.ovrAvatar2EntityRenderStateVersion _currentAllNodesVersion
            = CAPI.ovrAvatar2EntityRenderStateVersion.Invalid;
        private CAPI.ovrAvatar2EntityRenderStateVersion _currentVisibleNodesVersion
            = CAPI.ovrAvatar2EntityRenderStateVersion.Invalid;

        private CAPI.ovrAvatar2HierarchyVersion _targetHierarchyVersion
            = CAPI.ovrAvatar2HierarchyVersion.Invalid;
        private CAPI.ovrAvatar2EntityRenderStateVersion _targetAllNodesVersion
            = CAPI.ovrAvatar2EntityRenderStateVersion.Invalid;
        private CAPI.ovrAvatar2EntityRenderStateVersion _targetVisibleNodesVersion
            = CAPI.ovrAvatar2EntityRenderStateVersion.Invalid;

        protected bool IsUnitySynced => IsUnityHierarchySynced
            && AreUnityNodesSynced
            && IsUnityVisibiltySynced;

        protected bool IsUnityHierarchySynced => _currentHierarchyVersion == _targetHierarchyVersion;
        protected bool AreUnityNodesSynced => _currentAllNodesVersion == _targetAllNodesVersion;
        protected bool IsUnityVisibiltySynced => _currentVisibleNodesVersion == _targetVisibleNodesVersion;

        protected CAPI.ovrAvatar2Result entityStatus => CAPI.ovrAvatar2Entity_GetStatus(entityId);

        // Hold original load settings, need after the fast load has finished, to issue the full load
        private CAPI.ovrAvatar2EntityFilters _loadFilters;

        private bool _isZipAvatar = false;

        // forces the avatar to stay in FastLoad state
        protected bool _forcePermanentFastLoadInternalOverride = false;

        #endregion

        /////////////////////////////////////////////////
        //:: Protected Virtual Methods

        #region Subclass extensions

        /// Called when native avatar is created, after entering state AvatarState.Created
        protected virtual void OnCreated() { }

        /// Called when skeleton is loaded, after entering state AvatarState.Skeleton
        protected virtual void OnSkeletonLoaded() { }

        /// Called when Default Avatar is loaded, after entering state AvatarState.DefaultAvatar
        protected virtual void OnDefaultAvatarLoaded() { }

        /// Called when FastLoad Avatar is loaded, after entering state AvatarState.FastLoad
        protected virtual void OnFastLoadAvatarLoaded() { }

        /// Called the first time a User Avatar is loaded, after entering state AvatarState.UserAvatar
        protected virtual void OnUserAvatarLoaded() { }

        /// Called at the start of Teardown() before reverting to state AvatarState.None
        protected virtual void PreTeardown() { }

        /// Called when a load operation fails. See LoadRequestInfo for reason
        protected virtual void OnLoadFailed(CAPI.ovrAvatar2LoadRequestInfo loadRequest) { }

        ///
        /// Called on any LoadRequest state change for this entity. Overriding this is not recommended because the
        /// behavior may not always be intuitive; For example, "Success" on a LoadRequest only means that the
        /// assets have been loaded in native code, it does not mean that any Unity game objects have been created/updated.
        /// Most users should prefer the AvatarState callbacks such as "OnUserAvatarLoaded".
        protected virtual void OnLoadRequestStateChanged(CAPI.ovrAvatar2LoadRequestInfo loadRequest)
        {
            if (loadRequest.state == CAPI.ovrAvatar2LoadRequestState.Failed)
            {
                OvrAvatarLog.LogInfo($"[{entityId}] OnLoadFailed requestId={loadRequest.id} ({loadRequest.failedReason.ToString()})", logScope, this);
                OnLoadFailed(loadRequest);
                OnLoadFailedEvent?.Invoke(this, loadRequest);
            }
        }

        /// Called when a new OvrAvatarRenderable is created during the asset loading process
        protected virtual void OnRenderableCreated(OvrAvatarRenderable newRenderable)
        {
            OvrAvatarLog.LogVerbose($"Created new renderable - {newRenderable.name}", logScope, this);

#if OVR_AVATAR_HIDE_CONTROLLER_PRIMITIVES
            // HACK: hide controllers by meshName (quest + rift)
            if (newRenderable.name.Contains("controller") || newRenderable.name.Contains("touch"))
            {
                newRenderable.IsHidden = true;
            }
#endif
        }

        protected virtual void OnRenderableDisposed(OvrAvatarRenderable disposedRenderable)
        {
            OvrAvatarLog.LogVerbose($"Disposed renderable - {disposedRenderable.name}", logScope, this);
        }

        #endregion // Subclass extensions

        /////////////////////////////////////////////////
        //:: Private Functions

        #region Asset Loading
        public Task<OvrAvatarManager.HasAvatarChangedRequestResultCode> HasAvatarChangedAsync()
        {
            return OvrAvatarManager.Instance.SendHasAvatarChangedRequestAsync(entityId);
        }

        private bool ShouldFastLoad()
        {
            if (_forcePermanentFastLoadInternalOverride)
            {
                return true;
            }

            return OvrAvatarManager.Instance.FastLoadAvatarEnabled &&
                   CurrentState < AvatarState.FastLoad &&
                   // UltraLight Avatars are visually the same as FastLoadAvatars, so it doesn't make sense to cycle
                   // states in that case.
                   GetActiveQuality() != CAPI.ovrAvatar2EntityQuality.Ultralight &&
                   // when loaded from disk, there is no point in showing the progressive fastload state
                   // since the UltraLight quality fastload state is generally used to compensate for network time.
                   !_isZipAvatar;
        }

        /* Load local user's CDN asset with filters specified in `creationInfo.renderFilters` */
        protected void LoadUser(CAPI.ovrAvatar2Graph graphType = CAPI.ovrAvatar2Graph.Oculus)
        {
            LoadUserWithFilters(in _creationInfo.renderFilters, graphType);
        }

        protected bool LoadUserWithFilters(in CAPI.ovrAvatar2EntityFilters filters, CAPI.ovrAvatar2Graph graphType = CAPI.ovrAvatar2Graph.Oculus)
        {
            if (!IsCreated)
            {
                OvrAvatarLog.LogError("Cannot load assets before entity has been created.", logScope, this);
                return false;
            }

            if (!OvrAvatarEntitlement.AccessTokenIsValid(graphType))
            {
                OvrAvatarLog.LogError($"Cannot LoadUser until a valid Access Token is set.", logScope, this);
                return false;
            }

            if (_userId == 0)
            {
                OvrAvatarLog.LogError("Cannot LoadUser until User Id is set.", logScope, this);
                return false;
            }

            if (ShouldFastLoad())
            {
                _loadFilters = filters;
            }

            CAPI.ovrAvatar2Result result;
            CAPI.ovrAvatar2LoadRequestId loadRequestId;
            if (ShouldFastLoad())
            {
                result = CAPI.OvrAvatar2Entity_LoadUserFromGraphWithFiltersFast(entityId, _userId, graphType,
                    in filters, false, _validateCache, out loadRequestId);
            }
            else
            {
                result = CAPI.OvrAvatar2Entity_LoadUserFromGraphWithFilters(entityId, _userId, graphType,
                    in filters, false, _validateCache, out loadRequestId);
            }

            if (result == CAPI.ovrAvatar2Result.Pending)
            {
                OvrAvatarLog.LogVerbose($"Loaded user ID {_userId} onto entity {entityId}", logScope, this);
                ClearFailedLoadState();
                IsPendingCdnAvatar = true;
                OvrAvatarManager.Instance.RegisterLoadRequest(this, loadRequestId);
            }
            else
            {
                OvrAvatarLog.LogError($"LoadUser Failed: {result}", logScope, this);
                IsPendingCdnAvatar = false;
                // TODO: This is good in theory, but it conflicts w/ rendering logic
#pragma warning disable 618
                LoadState = LoadingState.Failed;
#pragma warning restore 618
                return false;
            }

            return true;
        }

        /**
         * Load avatar assets from a Zip file or StreamingAssets
         * This function loads all levels of detail.
         * @param string[]   array of strings containing asset directories to search.
         * @param bool
         * @see LoadAssetsFromData
         * @see LoadAssetsFromStreamingAssets
         */
        protected bool LoadAssets(string[] assetPaths, AssetSource assetSource)
        {
            switch (assetSource)
            {
                case AssetSource.Zip:
                    return LoadAssetsFromZipSource(assetPaths);
                case AssetSource.StreamingAssets:
                    return LoadAssetsFromStreamingAssets(assetPaths);
            }

            return false;
        }

        // TODO: Rename? This is also how you load assets from cdn if it can't find it from a zip source
        /**
         * Load avatar assets from a Zip file from Unity streaming assets.
         * This function loads all levels of detail.
         * @param string[]   array of strings containing asset directories to search.
         * @see LoadAssets
         * @see LoadAssetsFromData
         * @see LoadAssetsFromStreamingAssets
         */
        protected bool LoadAssetsFromZipSource(string[] assetPaths)
        {
            return LoadAssetsFromZipSource(assetPaths, _creationInfo.renderFilters.lodFlags);
        }

        /**
         * Load avatar assets from a Zip file from Unity streaming assets.
         * @param string[]   array of strings containing asset directories to search.
         * @param lodFilter  level of detail(s) to load.
         * @see LoadAssets
         * @see LoadAssetsFromData
         * @see LoadAssetsFromStreamingAssets
         * @see CAPI.ovrAvatar2EntityLODFlags
         */
        protected bool LoadAssetsFromZipSource(string[] assetPaths, CAPI.ovrAvatar2EntityLODFlags lodFilter)
        {
            if (!IsCreated)
            {
                OvrAvatarLog.LogError("Cannot load assets before entity has been created.", logScope, this);
                return false;
            }

            _isZipAvatar = true;

            var loadFilters = _creationInfo.renderFilters;
            loadFilters.lodFlags = lodFilter;

            bool didLoadZipAsset = false;

            if (assetPaths != null)
            {
                foreach (var path in assetPaths)
                {
                    CAPI.ovrAvatar2Result result;
                    CAPI.ovrAvatar2LoadRequestId loadRequestId;

                    if (_forcePermanentFastLoadInternalOverride)
                    {
                        var stdPostfix = OvrAvatarManager.Instance.GetPlatformGLBPostfix(CAPI.ovrAvatar2EntityQuality.Standard, true).ToLower();
                        var lightPostfix = OvrAvatarManager.Instance.GetPlatformGLBPostfix(CAPI.ovrAvatar2EntityQuality.Light, true).ToLower();
                        var ultralightPostfix = OvrAvatarManager.Instance.GetPlatformGLBPostfix(CAPI.ovrAvatar2EntityQuality.Ultralight, true).ToLower();
                        var fastpath = path.Replace(stdPostfix, ultralightPostfix)
                            .Replace(lightPostfix, ultralightPostfix);
                        fastpath = $"zip://{fastpath}";
                        result = CAPI.OvrAvatar2Entity_LoadUriWithFiltersFast(entityId, fastpath, loadFilters, out loadRequestId);
                    }
                    else
                    {
                        result = CAPI.OvrAvatar2Entity_LoadUriWithFilters(entityId, $"zip://{path}", loadFilters, out loadRequestId);
                    }


                    if (result.IsSuccess())
                    {
                        didLoadZipAsset = true;
                        OvrAvatarManager.Instance.RegisterLoadRequest(this, loadRequestId);
                    }
                    else
                    {
                        OvrAvatarLog.LogError($"Failed to load asset. {result} at path: {path}", logScope, this);
                    }
                }
                OvrAvatarLog.Assert(didLoadZipAsset);
            }

            IsPendingZipAvatar = didLoadZipAsset;
            if (didLoadZipAsset)
            {
                ClearFailedLoadState();
            }

            return didLoadZipAsset;
        }

        /**
        * Load avatar assets from Unity streaming assets.
        * @param string[]   array of strings containing asset directories
        *                   to search relative to *Application.streamingAssetsPath*.
        * @see LoadAssets
        * @see LoadAssetsFromData
        * @see LoadAssetsFromZipSource
        */
        protected bool LoadAssetsFromStreamingAssets(string[] assetPaths)
        {
            if (!IsCreated)
            {
                OvrAvatarLog.LogError("Cannot load assets before entity has been created.", logScope, this);
                return false;
            }

            string prefix = OvrAvatarManager.IsAndroidStandalone ? $"apk://" : $"file://{Application.streamingAssetsPath}/";
            var success = true;
            foreach (var path in assetPaths)
            {
                CAPI.ovrAvatar2Result result = CAPI.OvrAvatar2Entity_LoadUriWithFilters(entityId, prefix + path, _creationInfo.renderFilters, out var loadRequestId);
                if (result.IsSuccess())
                {
                    ClearFailedLoadState();
                    OvrAvatarManager.Instance.RegisterLoadRequest(this, loadRequestId);
                }
                else
                {
                    OvrAvatarLog.LogError(
                        $"Failed to load asset from streaming assets. {result} at path: {prefix + path}"
                        , logScope, this);
                    success = false;
                }
            }

            return success;
        }

        /// Load assets described in an in-memory GLB file into the entity.
        /// @param ovrAvatar2Entity to load into
        /// @param pointer to the beginning of a memory buffer
        /// @param the length of the memory buffer
        /// @param name what should be used to reference this glb
        /// @param (out) loadRequestID to retrieve status from ovrAvatar2Asset_GetLoadRequestInfo
        public CAPI.ovrAvatar2Result LoadMemory(
            IntPtr data,
            UInt32 dataSize,
            string nameId,
            out CAPI.ovrAvatar2LoadRequestId requestId
        )
        {
            return CAPI.OvrAvatar2Entity_LoadMemory(entityId, data, dataSize, nameId, out requestId);
        }


        private IEnumerator<OvrTime.SliceStep> LoadAsync_BuildSkeletonAndPrimitives()
        {
            Debug.Assert(IsApplyingModels);
            OvrAvatarLog.LogVerbose("Beginning LoadAsync_BuildSkeletonAndPrimitives", logScope, this);

            bool builtSkeleton = false;
            do
            {
                // Make sure native SDK code indicates all loading has finished.
                // This is due to the fact that some assets (such as those needed for RtRig)
                // are added solely on native code side. For the moment, checking the entity's
                // "pending count" is the only means that the C# layer has any knowledge of
                // certain assets loading correctly/incorrectly
                var pendingCount = CAPI.ovrAvatar2Asset_GetEntityPendingCount(entityId);
                while (pendingCount > 0)
                {
                    // Wait for all loads to finish
                    yield return OvrTime.SliceStep.Wait;
                    pendingCount = CAPI.ovrAvatar2Asset_GetEntityPendingCount(entityId);
                }

                if (QueryEntityPose(out var entityPose, out var hierarchyVersion))
                {
                    var task = LoadASync_BuildSkeleton(entityPose, hierarchyVersion);
                    while (task.MoveNext())
                    {
                        yield return task.Current;
                    }

                    builtSkeleton = true;
                }
                else
                {
                    OvrAvatarLog.LogError("Failed to query entity pose for building skeleton!", logScope, this);
                }
            } while (!builtSkeleton);

            OvrAvatarLog.Assert(CurrentState != AvatarState.None);
            if (CurrentState == AvatarState.Created && SkeletonJointCount > 0)
            {
                OvrAvatarLog.LogVerbose($"Skeleton loaded with {SkeletonJointCount} joints", logScope, this);
                CurrentState = AvatarState.Skeleton;
                InvokeOnSkeletonLoaded();
            }
            else
            {
                OvrAvatarLog.LogInfo("No skeleton loaded", logScope, this);
            }

            if (QueryEntityRenderState(out var renderState))
            {
                _targetVisibleNodesVersion = renderState.visibleNodesVersion;
            }

            var primitives = LoadAsync_BuildPrimitives();
            while (primitives.MoveNext())
            {
                yield return primitives.Current;
            }

            OvrAvatarLog.LogVerbose("Finished LoadAsync_BuildSkeletonAndPrimitives", logScope, this);

            _loadingRoutineBacking.Clear();
        }

        private IEnumerator<OvrTime.SliceStep> LoadAsync_BuildPrimitives()
        {
            var task = LoadAsyncCoroutine_BuildPrimitives_Internal();
            while (task.MoveNext())
            {
                yield return task.Current;
            }

            if (OvrTime.ShouldHold)
            {
                yield return OvrTime.SliceStep.Hold;
            }

            LoadAsync_Finalize_Internal();
            _loadingRoutineBacking.Clear();
        }

        private void LoadASync_BuildSkeleton_MapJoints()
        {
            const int allJointCount = (int)CAPI.ovrAvatar2JointType.Count;
            using var allJointTypes
                = new NativeArray<CAPI.ovrAvatar2JointType>(allJointCount
                    , Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            unsafe
            {
                CAPI.ovrAvatar2JointType* typesData = allJointTypes.GetPtr();
                for (int i = 0; i < allJointCount; ++i)
                {
                    typesData[i] = (CAPI.ovrAvatar2JointType)i;
                }

                using var allJointNodes
                    = CAPI.OvrAvatar2Entity_QueryJointTypeNodes_NativeArray(entityId, in allJointTypes, this);
                if (allJointTypes.Length != allJointNodes.Length)
                {
                    OvrAvatarLog.LogError(
                        $"allJointTypes.Length ({allJointTypes.Length}) != allJointNodes.Length ({allJointTypes.Length})"
                        , logScope, this);
                }
                else
                {
                    CAPI.ovrAvatar2NodeId* nodeData = allJointNodes.array.GetPtr();
                    for (int i = 0; i < allJointNodes.Length; ++i)
                    {
                        _jointTypeToNodeId.Add(typesData[i], nodeData[i]);
                    }
                }
            }
        }

        private IEnumerator<OvrTime.SliceStep> LoadASync_BuildSkeleton(CAPI.ovrAvatar2Pose entityPose, CAPI.ovrAvatar2HierarchyVersion hierVersion)
        {
            _targetHierarchyVersion = hierVersion;

            OvrAvatarLog.LogVerbose($"Starting BuildSkeleton", logScope, this);

            var previousSkeleton = _skeleton;
            var previousNodeToIndex = _nodeToIndex;

            var jointCount = entityPose.jointCount;
            var newSkeleton = new SkeletonJoint[jointCount];

            var buildNewNodeToIndex = new Dictionary<CAPI.ovrAvatar2NodeId, uint>((int)jointCount);

            // Only create transforms if we aren't using Joint Monitoring
            bool createTransforms = _jointMonitor == null;

            // Build hierarchy array
            OvrAvatarLog.LogVerbose($"Building skeleton with {jointCount} joints", logScope, this);
            Profiler.BeginSample("LoadASync_BuildSkeleton::BuildHierarchy");
            for (uint i = 0; i < jointCount; ++i)
            {
                var nodeId = entityPose.GetNodeIdAtIndex(i);

                // Joint name is currently only used in an Editor tool that is used to view the skeleton.
                // Its is fairly expensive to create, some string allocs, and a CAPI call, per joint.
                // So only load the real name in the Editor. Loading the name for 160+ Joints takes 1ms
#if UNITY_EDITOR
                var jointName = GetNameForNode(nodeId, in entityPose);
                if (string.IsNullOrWhiteSpace(jointName)) { jointName = $"joint{i}"; }
#else
                var jointName = "Joint name is only populated in the Editor";
#endif
                if (nodeId == CAPI.ovrAvatar2NodeId.Invalid)
                {
                    // Skip this index
                    OvrAvatarLog.LogError($"Invalid nodeId for {jointName}", logScope, this);
                    continue;
                }

                buildNewNodeToIndex.Add(nodeId, i);

                var jointParentIndex = entityPose.GetParentIndex(i);

                if (previousNodeToIndex.TryGetValue(nodeId, out var previousIndex))
                {
                    // Existing SkeletonJoint
                    var previousJoint = previousSkeleton[previousIndex];
                    OvrAvatarLog.Assert(previousJoint.nodeId == nodeId);

                    // TODO: Does jointParentIndex change?
                    newSkeleton[i] = new SkeletonJoint(in previousJoint, jointName, jointParentIndex);
                }
                else
                {
                    // New SkeletonJoint
                    var jointObject = createTransforms ? (new GameObject(jointName)).transform : null;

                    newSkeleton[i] = new SkeletonJoint(jointName
                            , jointObject
                            , jointParentIndex
                            , nodeId
                        );
                }
            }
            Profiler.EndSample(); // BuildHierarchy

            if (OvrTime.ShouldHold)
            {
                yield return OvrTime.SliceStep.Hold;
            }

            if (createTransforms)
            {
                Profiler.BeginSample("LoadASync_BuildSkeleton::ReparentSkeletonJoints");
                ReparentSkeletonJoints(newSkeleton, _baseTransform, in entityPose);
                Profiler.EndSample();
            }

            if (OvrTime.ShouldHold)
            {
                yield return OvrTime.SliceStep.Hold;
            }

            Profiler.BeginSample("LoadASync_BuildSkeleton::DestroyJoints");

            // Destroy unused SkeletonJoints
            List<SkeletonJoint> jointsToDestroy = null;
            foreach (var nodeToIndex in previousNodeToIndex)
            {
                if (buildNewNodeToIndex.ContainsKey(nodeToIndex.Key)) { continue; }

                var oldJoint = previousSkeleton[nodeToIndex.Value];
                if (jointsToDestroy == null)
                {
                    // Don't prealloc capacity below 4 (C# default) - in addition, 0 or negative values will throw an exception
                    int capacity = Mathf.Max(4, previousNodeToIndex.Count - buildNewNodeToIndex.Count);
                    jointsToDestroy = new List<SkeletonJoint>(capacity);
                }
                jointsToDestroy.Add(oldJoint);
            }

            if (jointsToDestroy != null)
            {
                jointsToDestroy.Sort();
                foreach (var joint in jointsToDestroy)
                {
                    DestroyJoint(in joint);
                }
                jointsToDestroy.Clear();
            }

            _skeleton = newSkeleton;

            // Clear previous state
            _nodeToIndex.CopyFrom(buildNewNodeToIndex);
            buildNewNodeToIndex.Clear();
            _jointTypeToNodeId.Clear();

            Profiler.EndSample(); // DestroyJoints

            if (OvrTime.ShouldHold)
            {
                yield return OvrTime.SliceStep.Hold;
            }

            if (jointCount > 0)
            {
                Profiler.BeginSample("LoadASync_BuildSkeleton::MapJoints");
                // Map each Joint Type to a Node ID. If there is no corresponding node, the dictionary holds a value of Invalid
                LoadASync_BuildSkeleton_MapJoints();
                Profiler.EndSample(); // MapJoints
            }

            if (OvrTime.ShouldHold)
            {
                yield return OvrTime.SliceStep.Hold;
            }

            // Don't build critical joints if there are no joints or if using Joint Monitor
            if (_jointMonitor != null)
            {
                Profiler.BeginSample("LoadASync_BuildSkeleton::UpdateMonitoredJoints");
                UpdateMonitoredJoints();
                MonitorJoints(in entityPose); // Update monitored joints to their correct positions
                _unityUpdateJointIndices = Array.Empty<uint>();
                Profiler.EndSample(); // UpdateMonitoredJoints
            }
            else if (jointCount > 0)
            {
                Profiler.BeginSample("LoadASync_BuildSkeleton::UpdateCriticalJoints");
                var updateJointArray = UpdateCriticalJoints();
                // finally sort and store all unity update joints into our member array of indices
                Array.Sort(updateJointArray);
                _unityUpdateJointIndices = updateJointArray;
                Profiler.EndSample(); // UpdateCriticalJoints
            }
            else
            {
                _unityUpdateJointIndices = Array.Empty<uint>();
            }
#if !UNITY_WEBGL
            SetupLodCullingPoints();
#endif

            _currentHierarchyVersion = hierVersion;
        }

        protected void UpdateMonitoredJoints()
        {
            _monitoredJointTypes.Clear();
            _monitoredJointPoses.Clear();

            foreach (var jointType in _criticalJointTypes)
            {
                AddMonitoredJoint(jointType);
            }

            var centerJoint = AvatarLODManager.Instance.JointTypeToCenterOn;
            if (centerJoint != CAPI.ovrAvatar2JointType.Invalid)
            {
                AddMonitoredJoint(centerJoint);
            }

            var jointTypes = AvatarLODManager.Instance.JointTypesToCullOn;
            var jointTypesCount = jointTypes.Count;
            for (int jointIdx = 0; jointIdx < jointTypesCount; ++jointIdx)
            {
                AddMonitoredJoint(jointTypes[jointIdx]);
            }
        }

        private uint[] UpdateCriticalJoints()
        {
            // In the following loops, fill the array `criticalJointIndices` wih any joint indices for joints
            // that needed to be updated on the Unity object hierarchy for important world space functions or
            // app logic reference. These will then be copied into `_unityUpdateJointIndices`
            HashSet<uint> criticalJointIndices = new HashSet<uint>();

            var critJointTypeSet = new HashSet<CAPI.ovrAvatar2JointType>(_criticalJointTypes);

            if (AvatarLODManager.hasInstance)
            {
                var centerJoint = AvatarLODManager.Instance.JointTypeToCenterOn;
                if (centerJoint != CAPI.ovrAvatar2JointType.Invalid)
                {
                    critJointTypeSet.Add(centerJoint);
                }

                var jointTypes = AvatarLODManager.Instance.JointTypesToCullOn;
                var jointTypesCount = jointTypes.Count;
                for (int jointIdx = 0; jointIdx < jointTypesCount; ++jointIdx)
                {
                    critJointTypeSet.Add(jointTypes[jointIdx]);
                }
            }

            if (critJointTypeSet.Count > 0)
            {
                foreach (var jointType in critJointTypeSet)
                {
                    var nodeId = GetNodeForType(jointType);
                    if (nodeId == CAPI.ovrAvatar2NodeId.Invalid) continue;

                    var critIdx = GetIndexForNode(nodeId);
                    criticalJointIndices.Add(critIdx);
                }
            }

            if (criticalJointIndices.Count > 0)
            {
                // Ensure all parent joint transforms are updated as well
                // TODO: Proxy criticalJoints directly w/out intermediate hierarchy
                foreach (var skelIdx in criticalJointIndices.ToArray())
                {
                    int parentIndex = _skeleton[skelIdx].parentIndex;
                    while (parentIndex >= 0)
                    {
                        criticalJointIndices.Add((uint)parentIndex);
                        parentIndex = _skeleton[parentIndex].parentIndex;
                    }
                }
            }

            return criticalJointIndices.ToArray();
        }

        // TODO: Could save a lot of struct copying by just passing the Transform directly
        private void DestroyJoint(in SkeletonJoint joint)
        {
            var jointTx = joint.transform;
            if (jointTx != null)
            {
                var jointTxChildCount = jointTx.childCount;
                if (jointTxChildCount > 0)
                {
                    OvrAvatarLog.LogWarning(
                        $"Skeleton joint {joint.name}, aka: {jointTx} (`parentIndex=={joint.parentIndex}`, `nodeId=={joint.nodeId}`) being destroyed during hierarchy update with {jointTxChildCount} attached children!"
                        , logScope, this);
                    for (int idx = 0; idx < jointTxChildCount; ++idx)
                    {
                        var child = jointTx.GetChild(idx);
                        OvrAvatarLog.LogWarning(
                            $"Destroying skeleton joint child {idx + 1}/{jointTxChildCount} {child}", logScope, child);
                    }
                }

                GameObject.Destroy(jointTx.gameObject);
            }
            else
            {
                OvrAvatarLog.LogInfo(
                    $"Skeleton joint {joint.name} with parent index (`parentIndex=={joint.parentIndex}`, `nodeId=={joint.nodeId}`) missing!"
                    , logScope, this);
            }
        }

        private static void _SetupInitialJointTransform(in CAPI.ovrAvatar2Pose entityPose, in SkeletonJoint joint, uint txIdx)
        {
            unsafe
            {
                CAPI.ovrAvatar2Transform* jointTransform = entityPose.localTransforms + txIdx;

#if OVR_AVATAR_DISABLE_CLIENT_XFORM
                // HACK: Mirror rendering transforms across X to fixup coordinate system errors
                if (joint.parentIndex == -1)
                {
                    var flipCopy = *jointTransform;
                    flipCopy.scale.z = -flipCopy.scale.z;
                    joint.transform.ApplyOvrTransform(in flipCopy);
                }
                else
                {
                    joint.transform.ApplyOvrTransform(jointTransform);
                }
#else
                joint.transform.ApplyOvrTransform(jointTransform);
#endif
            }
        }

        private OvrTime.SliceStep WaitForLoad(ref CheckPrimitivesResult result, int iPrimitive)
        {
            if (!QueryPrimitiveRenderState_Direct(iPrimitive, out var primState))
            {
                OvrAvatarLog.LogError($"Failed to query primitiveIndex {iPrimitive}", logScope, this);

#pragma warning disable 618
                LoadState = LoadingState.Failed;
#pragma warning restore 618
                return OvrTime.SliceStep.Cancel;
            }

            // Check if we already have a renderable for this instance - thus primitive must be loaded
            if (_meshNodes.TryGetValue(primState.meshNodeId, out var primRenderDatas))
            {
                // TODO: Would be nice to setup our maps to make this check very concise
                // |-> loop is unnecessary if all primitives for a given node and always rendered together? That seems possible?
                foreach (var primRenderData in primRenderDatas)
                {
                    if (primRenderData.primitiveId == primState.primitiveId)
                    {
                        // This primitive is already loaded onto this node
                        // Return `Continue` to signal to the caller they can move on to the next primitive
                        return OvrTime.SliceStep.Continue;
                    }
                }
            }

            if (!OvrAvatarManager.GetOvrAvatarAsset(primState.primitiveId, out OvrAvatarPrimitive primitive) || primitive == null)
            {
                // TODO: This case should be completely impossible now
                // - primitives should be stood be stubbed out before any building begins
                OvrAvatarLog.LogError($"Failed to find asset primitiveId {primState.primitiveId}", logScope, this);

#pragma warning disable 618
                LoadState = LoadingState.Failed;
#pragma warning restore 618
                return OvrTime.SliceStep.Cancel;
            }
            if (primitive.isCancelled)
            {
                OvrAvatarLog.LogError($"primitiveId {primState.primitiveId} was cancelled", logScope, this);

#pragma warning disable 618
                LoadState = LoadingState.Failed;
#pragma warning restore 618
                return OvrTime.SliceStep.Cancel;
            }

            // Wait for primitive to load
            if (!primitive.isLoaded)
            {
                // This primitiveID must load before proceeding
                // ReSharper disable once HeapView.ObjectAllocation.Evident
                result.newPrimitiveIds ??= new HashSet<CAPI.ovrAvatar2Id>();
                result.newPrimitiveIds.Add(primState.primitiveId);
                return OvrTime.SliceStep.Delay;
            }

            if (!_hasSetCreationContext && !SetCreationContext(primitive.primitiveStyle))
            {
                OvrAvatarLog.LogWarning("Failed to set creation context.");
                return OvrTime.SliceStep.Cancel;
            }

            // Mark new instance for instantiation
            result.newRenderIndices ??= new HashSet<UInt32>();
            result.newRenderIndices.Add((uint)iPrimitive);

            // Load complete, continue to next index
            return OvrTime.SliceStep.Continue;
        }

        // flag to track primitive renderables loading success
        private bool BuildNewPrimitiveRenderablesAsyncBuildAll = false;
        private IEnumerator<OvrTime.SliceStep> BuildNewPrimitiveRenderablesASync(NativeArray<UInt32> newRenderIndices)
        {
            foreach (var newRenderIndex in newRenderIndices)
            {
                Profiler.BeginSample("BuildNewPrimitiveRenderablesAsync::BuildPrimitiveRenderable");
                var newRenderable = BuildPrimitiveRenderable(newRenderIndex);
                Profiler.EndSample(); // BuildPrimitiveRenderable

                if (newRenderable != null)
                {
                    // TODO: This could use a better data structure, remove `_primitiveRenderables` first
                    if (!_meshNodes.TryGetValue(newRenderable.meshNodeId, out var existingRenderData))
                    {
                        var newRenderDatas = new PrimitiveRenderData[] { newRenderable };

                        _meshNodes.Add(newRenderable.meshNodeId, newRenderDatas);
                        _primitiveRenderables.Add(newRenderable.instanceId, newRenderDatas);
                    }
                    else
                    {
                        var lastElement = existingRenderData.Length;
                        Array.Resize(ref existingRenderData, lastElement + 1);
                        existingRenderData[lastElement] = newRenderable;

                        _meshNodes[newRenderable.meshNodeId] = existingRenderData;
                        _primitiveRenderables[newRenderable.instanceId] = existingRenderData;
                    }

                    if (OvrTime.ShouldHold) { yield return OvrTime.SliceStep.Hold; }

                    Profiler.BeginSample("BuildNewPrimitiveRenderablesAsync::InitializeRenderable");
                    InitializeRenderable(newRenderable.renderable);
                    Profiler.EndSample(); // InitializeRenderable
                }
                else
                {
                    BuildNewPrimitiveRenderablesAsyncBuildAll = false;
                    OvrAvatarLog.LogError("Failed to build primitive renderable", logScope, this);
                }

                if (OvrTime.ShouldHold) { yield return OvrTime.SliceStep.Hold; }
            }
        }

        private PrimitiveRenderData BuildPrimitiveRenderable(uint iPrimitive)
        {
            if (!QueryPrimitiveRenderState_Direct(iPrimitive, out var primState))
            {
                OvrAvatarLog.LogError($"Failed to query primitiveIndex {iPrimitive}", logScope, this);
#pragma warning disable 618
                LoadState = LoadingState.Failed;
#pragma warning restore 618
                return null;
            }

            // nodeId for this renderer, used by visibility control
            var meshNodeId = primState.meshNodeId;
            if (meshNodeId == CAPI.ovrAvatar2NodeId.Invalid)
            {
                OvrAvatarLog.LogError($"Invalid meshNodeId {iPrimitive}", logScope, this);
#pragma warning disable 618
                LoadState = LoadingState.Failed;
#pragma warning restore 618
                return null;
            }

            if (!OvrAvatarManager.GetOvrAvatarAsset(primState.primitiveId, out OvrAvatarPrimitive primitive) || primitive == null)
            {
                // TODO: This case should be completely impossible now
                // - primitives should be stood be stubbed out before any building begins
                OvrAvatarLog.LogError($"Failed to find asset primitiveId {primState.primitiveId}", logScope, this);
#pragma warning disable 618
                LoadState = LoadingState.Failed;
#pragma warning restore 618
                return null;
            }

            System.Diagnostics.Debug.Assert(primitive.isLoaded);
            System.Diagnostics.Debug.Assert(!primitive.isCancelled);

            OvrAvatarRenderable renderable = null;
            if (primitive.joints.Length == 0)
            {
                // TODO: T76240496 - Verify unskinned meshes still behave correctly
                renderable = CreateRenderable(primitive);
            }
            else
            {
                // Build mapping
                Transform[] joints = new Transform[primState.pose.jointCount];

                bool validJoints = true;
                for (int iJoint = 0; iJoint < primitive.joints.Length; ++iJoint)
                {
                    // Get the primitive's joint index and name
                    int poseIndex = primitive.joints[iJoint];
                    var jointNodeId = primState.pose.GetNodeIdAtIndex(poseIndex);

                    // Find the corresponding joint in the entity pose
                    // TODO: This should no longer be necessary
                    if (!_nodeToIndex.TryGetValue(jointNodeId, out uint entityIndex))
                    {
                        string jointName = GetNameForNode(jointNodeId, in primState.pose);
                        OvrAvatarLog.LogError(
                            $"Could not map primitive {iPrimitive} to the entity pose. " +
                            $"Joint {iJoint} {jointName} not found", logScope, this);

                        validJoints = false;
                        break;
                        // TODO(T80085915) Bring this back once controllers can be successfully rendered in Unity.
                        //LoadState = LoadingState.Failed;
                        //_loadingRoutine = null;
                        //yield break;
                    }

                    joints[iJoint] = _skeleton[entityIndex].transform;
                }

                if (validJoints)
                {
                    renderable = CreateRenderable(primitive);
                    var skinnedRenderable = renderable as OvrAvatarSkinnedRenderable;

                    if (skinnedRenderable != null)
                    {
                        skinnedRenderable.ApplySkeleton(joints);
                        if (_skinnedRenderables.ContainsKey(primitive))
                        {
                            OvrAvatarLog.LogError("Trying to insert already existing primitive key to skinned renderables");
                        }
                        _skinnedRenderables[primitive] = skinnedRenderable;
                    }
                }
            }

            if (!(renderable is null))
            {
                SampleSkinningOrigin(in primState, out var skinningOrigin);
                renderable.transform.ApplyOvrTransform(in skinningOrigin);
            }

            return new PrimitiveRenderData
            (
                meshNodeId, primitive.assetId, primState.id, renderable, primitive
            );
        }

        private struct CheckPrimitivesResult
        {
            public HashSet<CAPI.ovrAvatar2Id> newPrimitiveIds;
            public HashSet<uint> newRenderIndices;

            public CAPI.ovrAvatar2EntityRenderStateVersion targetAllNodesVersion;
            public CAPI.ovrAvatar2EntityRenderStateVersion targetVisVersion;
            public CAPI.ovrAvatar2HierarchyVersion targetHierarchyVersion;

            public CheckPrimitivesResult(CAPI.ovrAvatar2EntityRenderStateVersion allNodesVersion,
                CAPI.ovrAvatar2EntityRenderStateVersion visVersion, CAPI.ovrAvatar2HierarchyVersion requiredHierarchyVersion)
            {
                newPrimitiveIds = null;
                newRenderIndices = null;

                targetAllNodesVersion = allNodesVersion;
                targetVisVersion = visVersion;
                targetHierarchyVersion = requiredHierarchyVersion;
            }
        }

        private IEnumerator<OvrTime.SliceStep> LoadAsyncCoroutine_BuildPrimitives_Internal()
        {
            // TODO: A timeout seems logical, but we've never had one so far :X
            // |-> In theory, once we finish waiting for loading building should never fail
            do
            {
                if (OvrTime.ShouldHold) { yield return OvrTime.SliceStep.Hold; }
                OvrTime.SliceStep step = LoadSync_CheckPrimitivesLoaded_Internal(out var result);
                if (step != OvrTime.SliceStep.Continue)
                {
                    // Unrecoverable loading error - abort
                    if (step == OvrTime.SliceStep.Cancel) { yield break; }
                    // Try again next frame
                    yield return step;
                    // - from the top!
                    continue;
                }

                System.Diagnostics.Debug.Assert(result.targetAllNodesVersion != CAPI.ovrAvatar2EntityRenderStateVersion.Invalid);
                System.Diagnostics.Debug.Assert(result.targetVisVersion != CAPI.ovrAvatar2EntityRenderStateVersion.Invalid);

                // To create the primitives we need to make sure we have required hierarchy loaded and read
                // When we use async it's possible the render state to be ahead of what we have loaded
                if (result.targetHierarchyVersion != _currentHierarchyVersion) { yield break; }

                if (result.newRenderIndices != null)
                {
                    BuildNewPrimitiveRenderablesAsyncBuildAll = true;

                    var task = LoadAsync_BuildPrimitives_Internal(result);
                    while (task.MoveNext())
                    {
                        yield return task.Current;
                    }

                    if (!BuildNewPrimitiveRenderablesAsyncBuildAll)
                    {
                        // Retry next frame
                        yield return OvrTime.SliceStep.Wait;
                        // - from the top!
                        continue;
                    }
                }

                // Update visibility flags
                Profiler.BeginSample("LoadAsync_BuildPrimitives_Internal::UpdateVisibility");
                UpdateVisibility();
                Profiler.EndSample();

                // Finish
                break;
            }
            while (true);
        }

        private OvrTime.SliceStep LoadSync_CheckPrimitivesLoaded_Internal(out CheckPrimitivesResult result)
        {
            if (!QueryEntityRenderState(out var entityRenderState))
            {
                result = default;

                OvrAvatarLog.LogError("Unable to query entity render state", logScope, this);
                return OvrTime.SliceStep.Cancel;
            }

            result = new CheckPrimitivesResult(entityRenderState.allNodesVersion, entityRenderState.visibleNodesVersion
                , entityRenderState.hierarchyVersion);

            // TODO: Should use instanceId
            for (int iPrimitive = 0; iPrimitive < entityRenderState.primitiveCount; ++iPrimitive)
            {
                var step = WaitForLoad(ref result, iPrimitive);
                if (step != OvrTime.SliceStep.Continue) { return step; }
            }
            return OvrTime.SliceStep.Continue;
        }

        private IEnumerator<OvrTime.SliceStep> LoadAsync_BuildPrimitives_Internal(CheckPrimitivesResult result)
        {
            using (var newRenderArray = new NativeArray<uint>(result.newRenderIndices.Count
                , Allocator.Persistent, NativeArrayOptions.UninitializedMemory))
            {
                newRenderArray.CopyFrom(result.newRenderIndices);

                OvrAvatarLog.LogVerbose($"Starting BuildPrimitives", logScope, this);

                var task = BuildNewPrimitiveRenderablesASync(newRenderArray);
                while (task.MoveNext())
                {
                    yield return task.Current;
                }
                OvrAvatarLog.AssertConstMessage(BuildNewPrimitiveRenderablesAsyncBuildAll
                    , "Unable to update allNodesVersion", logScope, this);
            }
        }

        private void CheckLoadedAssets()
        {
            bool hasDefaultModel = false;
            bool hasUserModel = false;
            var wasPendingZipAvatar = IsPendingZipAvatar;
            var wasPendingCdnAvatar = IsPendingCdnAvatar;

            using (var loadedAssetTypes = CAPI.OvrAvatar2Entity_GetLoadedAssetTypes_NativeArray(entityId))
            {
                foreach (var assetType in loadedAssetTypes)
                {
                    switch (assetType)
                    {
                        case CAPI.ovrAvatar2EntityAssetType.SystemDefaultModel:
                            hasDefaultModel = true;

                            IsPendingDefaultModel = false;
                            break;

                        case CAPI.ovrAvatar2EntityAssetType.SystemOther:
                            // Controllers and other misc, not currently tracked from Unity
                            break;

                        case CAPI.ovrAvatar2EntityAssetType.Other:
                            hasUserModel = true;

                            HasNonDefaultAvatar = true;
                            IsPendingCdnAvatar = false;
                            IsPendingZipAvatar = false;
                            break;
                    }
                }
            }

            if (hasDefaultModel || hasUserModel)
            {
                // If Skeleton is loaded, the avatar should be in at least the "Skeleton" state
                if (!_forcePermanentFastLoadInternalOverride)
                {
                    OvrAvatarLog.AssertConstMessage(CurrentState >= AvatarState.Skeleton,
                        "Entity is unexpected state after loading model", logScope, this);
                }
            }

            // After the transition to AvatarState.Skeleton it takes one or two more frames for all the joints to load
            // The next conditional should activate on the following call to CheckLoadedAssets()

            if (SkeletonJointCount > 0 && CurrentState == AvatarState.Skeleton && hasDefaultModel)
            {
                CurrentState = AvatarState.DefaultAvatar;
                InvokeOnDefaultAvatarLoaded();
            }

            if (hasDefaultModel && hasUserModel)
            {
                CAPI.OvrAvatar2Entity_UnloadDefaultModel(entityId);
            }

            if (ShouldFastLoad() && CurrentState < AvatarState.FastLoad && hasUserModel)
            {
                CurrentState = AvatarState.FastLoad;

                if (_forcePermanentFastLoadInternalOverride)
                {
                    InvokeOnFastLoadAvatarLoaded();
                    return;
                }

                if (wasPendingZipAvatar)
                {
                    //LoadAssetsFromZipSource(_assetPaths, _lodFilters);
                }
                else if (wasPendingCdnAvatar)
                {
                    CAPI.OvrAvatar2Entity_LoadUserFromGraphWithFilters(entityId, _userId,
                        CAPI.ovrAvatar2Graph.Oculus, _loadFilters, false, _validateCache, out var loadRequestId);
                }
                InvokeOnFastLoadAvatarLoaded();
            }
            else if (CurrentState < AvatarState.UserAvatar && hasUserModel)
            {
                CurrentState = AvatarState.UserAvatar;
                InvokeOnUserAvatarLoaded();
            }
        }

        private void LoadAsync_Finalize_Internal()
        {
            CheckLoadedAssets();

            LoadAsync_Finalize();

#pragma warning disable 618
            if (LoadState != LoadingState.Failed && CurrentState == AvatarState.UserAvatar)
            {
                LoadState = LoadingState.Success;
            }
#pragma warning restore 618

            IsApplyingModels = false;

            // Null out the backing explicitly to avoid cancelling this coroutine
            _loadingRoutineBacking.Clear();

            // Allow OvrAvatarManager to start the next queued load
            OvrAvatarManager.Instance.FinishedAvatarLoad();
        }

        // Override hook for additional work once primitives are loaded, but before loading is marked complete
        protected virtual void LoadAsync_Finalize() { }

        private static void ReparentSkeletonJoints(SkeletonJoint[] newSkel
            , Transform baseTransform, in CAPI.ovrAvatar2Pose entityPose)
        {
            // Reparent transforms into hierarchy
            uint jointCount = entityPose.jointCount;
            for (uint i = 0; i < jointCount; ++i)
            {
                ref readonly SkeletonJoint joint = ref newSkel[i];

                Transform parentTransform =
                    joint.parentIndex < 0 ? baseTransform : newSkel[joint.parentIndex].transform;

                joint.transform.SetParent(parentTransform, false);

                _SetupInitialJointTransform(in entityPose, in joint, i);
            }
        }

        private void UpdateVisibility()
        {
            if (QueryEntityRenderState(out var renderState))
            {
                UpdateVisibility(in renderState);
            }
        }

        private void UpdateVisibility(in CAPI.ovrAvatar2EntityRenderState renderState)
        {
            unsafe
            {
                if (renderState.visibleNodesVersion != _currentVisibleNodesVersion)
                {
                    _targetVisibleNodesVersion = renderState.visibleNodesVersion;

                    Array.Resize(ref _visiblePrimitiveRenderers, (int)renderState.visibleMeshNodesCount);

                    // TODO: These aren't *both* necessary but there are two pieces of logic twisted together here currently
                    uint visibleIndex = 0;
                    uint subsequentVisibleIdx = 0;
                    var nextVisibleNodeId = _CheckNextVisibleNodeId(in renderState, ref subsequentVisibleIdx);

                    for (int allNodeIdx = 0; allNodeIdx < renderState.allMeshNodesCount; allNodeIdx++)
                    {
                        var node = renderState.allMeshNodes[allNodeIdx];

                        bool isVisible = node == nextVisibleNodeId;
                        if (_meshNodes.TryGetValue(node, out var nodeRenderData))
                        {
                            // TODO: Setup a callback? This would be useful state change info for apps
                            // TODO: Sum up the cost for the node?
                            foreach (var primRenderable in nodeRenderData)
                            {
                                var renderable = primRenderable.renderable;
                                if (renderable.Visible != isVisible)
                                {
                                    if (renderable.Visible)
                                    {
                                        RemoveVisibleLodCost(in primRenderable);
                                    }
                                    else
                                    {
                                        AddVisibleLodCost(in primRenderable);
                                    }

                                    renderable.Visible = isVisible;
                                }
                            }
                        }

                        if (isVisible)
                        {
                            nextVisibleNodeId = _CheckNextVisibleNodeId(in renderState, ref subsequentVisibleIdx);

                            // TODO: We could gather this list more efficiently while we're checking renderState
                            // This location covers our weird untrimmed edge cases though
                            _visiblePrimitiveRenderers[visibleIndex++] = nodeRenderData;
                            if (nodeRenderData == null)
                            {
                                OvrAvatarLog.LogError($"Missing visible meshNode with id {node}", logScope, this);
                            }
                        }
                    }

                    RefreshLODRange();

                    SetupLodGroups();

                    RefreshAllActives();

                    _currentVisibleNodesVersion = renderState.visibleNodesVersion;
                }
            }
        }

        private static CAPI.ovrAvatar2NodeId _CheckNextVisibleNodeId(in CAPI.ovrAvatar2EntityRenderState renderState, ref uint nextIndex)
        {
            if (nextIndex < renderState.visibleMeshNodesCount)
            {
                return renderState.GetVisibleMeshNodeAtIdx(nextIndex++);
            }
            return CAPI.ovrAvatar2NodeId.Invalid;
        }

        private void UpdateAllNodes(in CAPI.ovrAvatar2EntityRenderState renderState)
        {
            if (_currentAllNodesVersion != renderState.allNodesVersion)
            {
                _targetAllNodesVersion = renderState.allNodesVersion;

                // If `allNodes` has changed, some may have been removed
                // New nodes will have been built during the visibility update

                // Avoid allocs if there's nothing to clean up
                if (_meshNodes.Count > 0)
                {
                    // TODO: Not ideal to be checking the nodes we just added for removal :/

                    // TODO: Avoid allocs, this is a fairly rare operation though
                    var allNodeHash = new HashSet<CAPI.ovrAvatar2NodeId>();
                    var disposeList = new List<CAPI.ovrAvatar2NodeId>();

                    for (uint allIdx = 0; allIdx < renderState.allMeshNodesCount; ++allIdx)
                    {
                        allNodeHash.Add(renderState.GetAllMeshNodeAtIdx(allIdx));
                    }

                    foreach (var meshNodeKVP in _meshNodes)
                    {
                        var nodeId = meshNodeKVP.Key;
                        if (!allNodeHash.Contains(nodeId))
                        {
                            disposeList.Add(nodeId);
                        }
                    }
                    foreach (var disposePrimData in disposeList)
                    {
                        RemoveNodeRenderables(disposePrimData);
                    }
                }

                _currentAllNodesVersion = renderState.allNodesVersion;
            }
        }

        private void DestroySkeleton()
        {
            ResetLODRange();
            ResetLodCullingPoints();

            for (int i = 0; i < _skeleton.Length; ++i)
            {
                var skelTx = _skeleton[i].transform;
                if (skelTx != null)
                {
                    var skelGob = skelTx.gameObject;
                    skelGob.SetActive(false);
                    GameObject.Destroy(skelGob);
                }
            }

            for (int idx = 0; idx < _visibleLodData.Length; ++idx)
            {
                ref var lodObject = ref _visibleLodData[idx];
                if (lodObject.IsValid)
                {
                    DestroyLODObject(ref lodObject);
                }
            }
            OvrAvatarLog.Assert(lodObjectCount == 0);
            lodObjectCount = 0;

            _skeleton = Array.Empty<SkeletonJoint>();

            foreach (var renderables in _primitiveRenderables)
            {
                foreach (var renderableData in renderables.Value)
                {
                    renderableData?.Dispose();
                }
            }
            _primitiveRenderables.Clear();

            _jointTypeToNodeId.Clear();
            _nodeToIndex.Clear();

            _unityUpdateJointIndices = Array.Empty<uint>();
        }

        // TODO: This whole method should move into _Rendering
        private OvrAvatarRenderable CreateRenderable(OvrAvatarPrimitive primitive)
        {
            GameObject primitiveObject = new GameObject(primitive.name, typeof(MeshFilter));

            var renderable = AddRenderableComponent(primitiveObject, primitive);

            // LODs
            var parent = _baseTransform;
            if (primitive.lodFlags != 0)
            {
                // TODO: Remove this special case for `CAPI.ovrAvatar2EntityLODFlags.All`
                if (primitive.lodFlags == CAPI.ovrAvatar2EntityLODFlags.All)
                {
                    // Currently this is covering controller meshes
                    if (!_visibleAllLodData.IsValid)
                    {
                        // TODO: Coverage value is unused... probably the wrong structure?
                        var allLodGo = new GameObject($"AllLOD");
                        var allLodTx = allLodGo.transform;

                        allLodTx.SetParent(_baseTransform, false);

                        _visibleAllLodData = new LodData(allLodGo);
                    }

                    // TODO: Update the lod range... but only if there are no other primitives :/
                    parent = _visibleAllLodData.transform;
                }
                else
                {
                    var withoutLeastFlag = primitive.lodFlags & (primitive.lodFlags - 1);

                    if (withoutLeastFlag == 0)
                    {
                        // Primitive is for only one LOD

                        uint onlyBit = (uint)(withoutLeastFlag ^ primitive.lodFlags);
                        uint lodIndex = (uint)Mathf.Log(onlyBit, 2);

                        // Primitive represents a specific LOD
                        Debug.Assert((1 << (int)lodIndex) == onlyBit);

                        ref var lodData = ref _visibleLodData[lodIndex];

                        // First renderable at this LOD level
                        if (!lodData.IsValid)
                        {
                            GameObject go = new GameObject($"LOD{lodIndex}");
                            var goTX = go.transform;

                            goTX.SetParent(_baseTransform, false);

                            lodData = new LodData(go);

                            lodObjectCount++;
                        }

                        parent = lodData.transform;
                    }
                    else
                    {
                        OvrAvatarLog.LogError("Multi LOD Primitives are not currently supported", logScope, this);
                        // Primitive is used for multiple LODs

                        // TODO: This needs to handle any partial set of flags (not just All)
                        /* Primitive covers multiple LODs
                        for (uint idx = CAPI.ovrAvatar2EntityLODFlagsCount - 1; idx >= 0; --idx)
                        {
                            ref var lodData = ref _visibleLodData[idx];

                            var lodFlag = (CAPI.ovrAvatar2EntityLODFlags)(1 << (int)idx);
                            if ((lodFlag & primitive.lodFlags) != 0)
                            {
                                // TODO: Should expand LOD range - but this doesn't *quite* work correctly w/ the current setup
                                // ExpandLODRange(idx);
                                // TODO: Add to cost of each covered LOD to ensure accurate scheduling
                                // - we need to potentially create the LOD entry from here? Refactor?
                                // - We wouldn't have the right coverage value to initialize with :/
                            }
                        } */
                    }
                }
            }

            primitiveObject.transform.SetParent(parent, false);

            return renderable;
        }

        private void AddVisibleLodCost(in PrimitiveRenderData renderData)
        {
            OvrAvatarLog.Assert(renderData.renderable);

            // Increase the LodCost for affected lod levels.
            {
                if (renderData.primitive.lodFlags == CAPI.ovrAvatar2EntityLODFlags.All)
                {
                    _visibleAllLodData.AddInstance(renderData.renderable);
                }
                else
                {
                    // TODO: Update all LODs for renderData
                    /*
                    for (uint idx = 0; idx < CAPI.ovrAvatar2EntityLODFlagsCount; ++idx)
                    {
                        if ((lodFlag & primitive.lodFlags) != 0)
                    /*/
                    var primHighestQualityLODIndex = renderData.primitive.HighestQualityLODIndex;
                    OvrAvatarLog.Assert(primHighestQualityLODIndex >= 0);
                    if (primHighestQualityLODIndex >= 0)
                    {
                        var idx = (uint)primHighestQualityLODIndex;
                        //*/
                        {
                            ref var lodObject = ref _visibleLodData[idx];

                            lodObject.AddInstance(renderData.renderable);
                        }
                    }
                }
            }
        }

        private void RemoveVisibleLodCost(in PrimitiveRenderData renderData)
        {
            OvrAvatarLog.Assert(renderData.renderable);
            // Reduce the LodCost for affected lod levels.
            {
                if (renderData.primitive.lodFlags == CAPI.ovrAvatar2EntityLODFlags.All)
                {
                    var didRemove = _visibleAllLodData.RemoveInstance(renderData.renderable);
                    OvrAvatarLog.Assert(didRemove, logScope, this);
                }
                else
                {
                    // TODO: Update all LODs for renderData
                    /*
                    for (uint idx = 0; idx < CAPI.ovrAvatar2EntityLODFlagsCount; ++idx)
                    {
                        if ((lodFlag & primitive.lodFlags) != 0)
                    /*/
                    var primHighestQualityLODIndex = renderData.primitive.HighestQualityLODIndex;
                    OvrAvatarLog.Assert(primHighestQualityLODIndex >= 0);
                    if (primHighestQualityLODIndex >= 0)
                    {
                        var idx = (uint)primHighestQualityLODIndex;
                        //*/
                        {
                            ref var lodObject = ref _visibleLodData[idx];

                            bool didRemove = lodObject.RemoveInstance(renderData.renderable);
                            OvrAvatarLog.Assert(didRemove);
                            AvatarLOD.updateLODCost((int)idx, lodObject);
                        }
                    }
                }
            }
        }

        private void RemoveNodeRenderables(CAPI.ovrAvatar2NodeId meshNodeId)
        {
            OvrAvatarLog.Assert(_meshNodes.ContainsKey(meshNodeId), logScope, this);
            if (_meshNodes.TryGetValue(meshNodeId, out var primRenderDatas))
            {
                bool didRemove = _meshNodes.Remove(meshNodeId);
                OvrAvatarLog.Assert(didRemove, logScope, this);

                foreach (var renderData in primRenderDatas)
                {
                    var primitive = renderData.primitive;
                    _skinnedRenderables.Remove(primitive);
                    _primitiveRenderables.Remove(renderData.instanceId);

                    // If visible reduce the vertex count for the lod level.
                    if (renderData.renderable.Visible)
                    {
                        RemoveVisibleLodCost(in renderData);
                    }

                    DestroyRenderable(in renderData);
                }
            }
        }

        private void DestroyRenderable(in PrimitiveRenderData renderData)
        {
            // Detach the game object from the hierarchy so its components will not be found.
            var renderable = renderData.renderable;
            if (renderable != null)
            {
                renderable.enabled = false;

                var gob = renderable.gameObject;
                gob.SetActive(false);
                gob.transform.SetParent(null, false);
                GameObject.Destroy(gob);
            }
        }

        private void DestroyLODObject(ref LodData lodObject)
        {
            OvrAvatarLog.Assert(lodObject.IsValid);

            lodObjectCount--;
            Destroy(lodObject.gameObject);
            lodObject = default;
        }

        private void SetRequiredFeatures()
        {
            if (UseGpuSkinning)
            {
                if (ForceEnableFeatures(CAPI.ovrAvatar2EntityFeatures.Rendering_SkinningMatrices))
                {
                    OvrAvatarLog.LogWarning(
                        "Rendering_SkinningMatrices force enabled due to GPU Skinning - consider enabling in prefab `_creationInfo.features`",
                        logScope, this);
                }
            }

            if (HasAllFeatures(CAPI.ovrAvatar2EntityFeatures.Rendering_Prims))
            {
                if (ForceEnableFeatures(CAPI.ovrAvatar2EntityFeatures.Rendering_SkinningMatrices))
                {
                    OvrAvatarLog.LogWarning(
                        @"Rendering_SkinningMatrices force enabled due to Rendering_Prims being enabled.
Needed for bounding box calculation. Consider enabling in the prefab `_creationInfo.features`", logScope, this);
                }
            }
        }

        private void ClearFailedLoadState()
        {
#pragma warning disable 618
            if (LoadState == LoadingState.Failed)
            {
                LoadState = LoadingState.Created;
            }
#pragma warning restore 618
        }

        private static StringBuilder creationContextBuilder = null;
        private bool SetCreationContext(AvatarStyle avatarStyle)
        {
            if (avatarStyle == AvatarStyle.InvalidStyle)
            {
                return true; // Not logging if the avatar is invalid
            }
            var shaderManager = OvrAvatarManager.Instance.ShaderManager;
            if (shaderManager == null || !shaderManager.Initialized)
            {
                OvrAvatarLog.LogError("Attempting to load avatar before AvatarSDK has finished initializing!", logScope, this);
                return false;
            }

            var shaderIdentifier = shaderManager.GetDefaultShaderIdentifier();


            creationContextBuilder ??= new StringBuilder(64);
            creationContextBuilder.Append("{\"skinning_type\": ");
            creationContextBuilder.Append((int)SkinningType);
            creationContextBuilder.Append(", \"shader_file_type\": ");
            creationContextBuilder.Append(shaderIdentifier);
            creationContextBuilder.Append(", \"avatar_style\": ");
            creationContextBuilder.Append((int)avatarStyle);
            creationContextBuilder.Append('}');
            string jsonString = creationContextBuilder.ToString();
            creationContextBuilder.Clear();

            _hasSetCreationContext = true;
            return CAPI.OvrAvatar2Entity_SetCreationContext(entityId, jsonString);
        }

        #endregion

        #region User Callback Invocation

        private void InvokeOnCreated()
        {
            OvrAvatarLog.LogInfo($"[{entityId}] OnCreated", logScope, this);

            Profiler.BeginSample("OvrAvatarEntity::OnCreated Callbacks");
            try
            {
                OnCreated();
                OnCreatedEvent?.Invoke(this);
            }
            catch (Exception e)
            {
                OvrAvatarLog.LogException("OnCreated user callback", e, logScope, this);
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        private void InvokeOnSkeletonLoaded()
        {
            OvrAvatarLog.LogInfo($"[{entityId}] OnSkeletonLoaded", logScope, this);

            Profiler.BeginSample("OvrAvatarEntity::OnSkeletonLoaded Callbacks");
            try
            {
                OnSkeletonLoaded();
                OnSkeletonLoadedEvent?.Invoke(this);
            }
            catch (Exception e)
            {
                OvrAvatarLog.LogException("OnSkeletonLoaded user callback", e, logScope, this);
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        private void InvokeOnDefaultAvatarLoaded()
        {
            OvrAvatarLog.LogInfo($"[{entityId}] OnDefaultAvatarLoaded", logScope, this);

            Profiler.BeginSample("OvrAvatarEntity::OnDefaultAvatarLoaded Callbacks");
            try
            {
                OnDefaultAvatarLoaded();
                OnDefaultAvatarLoadedEvent?.Invoke(this);
            }
            catch (Exception e)
            {
                OvrAvatarLog.LogException("OnDefaultAvatarLoaded user callback", e, logScope, this);
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        private void InvokeOnFastLoadAvatarLoaded()
        {
            OvrAvatarLog.LogInfo($"[{entityId}] OnFastLoadAvatarLoaded", logScope, this);

            Profiler.BeginSample("OvrAvatarEntity::OnFastLoadAvatarLoaded Callbacks");
            try
            {
                OnFastLoadAvatarLoaded();
                OnFastLoadAvatarLoadedEvent?.Invoke(this);
            }
            catch (Exception e)
            {
                OvrAvatarLog.LogException("OnFastLoadAvatarLoaded user callback", e, logScope, this);
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        private void InvokeOnUserAvatarLoaded()
        {
            OvrAvatarLog.LogInfo($"[{entityId}] OnUserAvatarLoaded", logScope, this);

            Profiler.BeginSample("OvrAvatarEntity::OnUserAvatarLoaded Callbacks");
            try
            {
                OnUserAvatarLoaded();
                OnUserAvatarLoadedEvent?.Invoke(this);
            }
            catch (Exception e)
            {
                OvrAvatarLog.LogException("OnUserAvatarLoaded user callback", e, logScope, this);
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        private void InvokePreTeardown()
        {
            OvrAvatarLog.LogInfo($"[{entityId}] PreTeardown");

            Profiler.BeginSample("OvrAvatarEntity::PreTeardown Callbacks");
            try
            {
                // Order is intentionally reversed from the others - For teardown typically you'd want external systems to
                // clean up before OvrAvatarEntity starts tearing down.
                PreTeardownEvent?.Invoke(this);
                PreTeardown();
            }
            catch (Exception e)
            {
                OvrAvatarLog.LogException("PreTeardown user callback", e, logScope, this);
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        internal void InvokeOnLoadRequestStateChanged(CAPI.ovrAvatar2LoadRequestInfo loadRequestInfo)
        {
            Profiler.BeginSample("OvrAvatarEntity::OnLoadRequestStateChanged Callbacks");
            try
            {
                OnLoadRequestStateChanged(loadRequestInfo);
            }
            catch (Exception e)
            {
                OvrAvatarLog.LogException("OnLoadRequestStateChanged user callback", e, logScope, this);
            }
            finally
            {
                Profiler.EndSample();
            }
        }

        #endregion
    }
}
