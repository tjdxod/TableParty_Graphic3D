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

// #define SUBMESH_DEBUGGING

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.Android;
using Unity.Profiling;

using static Oculus.Avatar2.CAPI;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Application = UnityEngine.Application;

/**
 * @file OvrAvatarEntity.cs
 * This is the main file for a partial class.
 * Other functionality is split out into the files below:
 * - OvrAvatarEntity_Color.cs
 * - OvrAvatarEntity_Debug.cs
 * - OvrAvatarEntity_Loading.cs
 * - OvrAvatarEntity_LOD.cs
 * - OvrAvatarEntity_Rendering.cs
 * - OvrAvatarEntity_Streaming.cs
 * - OvrAvatarEntity_Attachables.cs
 */
namespace Oculus.Avatar2
{

    /**
     * Describes a single avatar with multiple levels of detail.
     *
     * An avatar entity encapsulates all of the assets associated with it.
     * It provides access to the avatar's skeleton, meshes and materials
     * and integrates input from tracking components to drive the
     * avatar's motion. This base class provides defaults for all of these.
     * To customize avatar behavior you must derive from OvrAvatarEntity.
     * Then you can supply your application's preferences.
     *
     * # Loading an Avatar #
     * An avatar may have many representations varying in complexity
     * and fidelity. You can select among these capabilities
     * by setting _creationInfo flags before calling [CreateEntity()](@ref OvrAvatarEntity.CreateEntity).
     *
     * You can load the avatar's assets from LoadUser to download the user's custom avatar.
     * You can also load from [zip files](@ref LoadAssetsFromZipSource),
     * [streaming assets](@ref LoadAssetsFromStreamingAssets] or
     * [directly from memory](@ref LoadAssetsFromData) from within your subclass
     * to provide application-specific avatar loading.
     *
     * The avatar's assets are loaded in the background so your application
     * will not display the avatar immediately after load. The @ref IsPendingAvatar
     * flag is set when loading starts and cleared when it has finished.
     */
    public partial class OvrAvatarEntity : MonoBehaviour
    {
        // If any variable is used across these files, it should be placed in this file

        //:: Constants
        private const string logScope = "entity";

        protected const CAPI.ovrAvatar2EntityFeatures UPDATE_POSE_FEATURES =
            CAPI.ovrAvatar2EntityFeatures.AnalyticIk |
            CAPI.ovrAvatar2EntityFeatures.Animation;
        protected const CAPI.ovrAvatar2EntityFeatures UPDATE_MOPRHS_FEATURES =
            CAPI.ovrAvatar2EntityFeatures.Animation |
            CAPI.ovrAvatar2EntityFeatures.UseDefaultFaceAnimations;

        //:: Internal Structs & Classes
        protected readonly struct SkeletonJoint : IComparable<SkeletonJoint>
        {
            // The name is only intended to be used by Editor tools, it won't be loaded out of Editor.
            public readonly string name;
            public readonly Transform transform;
            public readonly int parentIndex;
            public readonly CAPI.ovrAvatar2NodeId nodeId;

            public SkeletonJoint(string jointName, Transform tx, int parentIdx, CAPI.ovrAvatar2NodeId nodeIdentifier)
            {
                name = jointName;
                transform = tx;
                parentIndex = parentIdx;
                nodeId = nodeIdentifier;
            }

            public SkeletonJoint(in SkeletonJoint oldJoint, string newName, int parentIdx)
                : this(newName, oldJoint.transform, parentIdx, oldJoint.nodeId)
            { }

            public int CompareTo(SkeletonJoint other)
            {
                return other.parentIndex - parentIndex;
            }
        }

        protected class ProfilerMarkers
        {
            public readonly ProfilerMarker AttachablesMarker;

            public ProfilerMarkers()
            {
                var categories = OvrAvatarProfilingUtils.AvatarCategories;
                AttachablesMarker = new ProfilerMarker(OvrAvatarProfilingUtils.AvatarCategories.Animation, "OvrAvatarPrimitive::AttachablesMarker");
            }
        }
        private static ProfilerMarkers _s_markers = null;
        protected static ProfilerMarkers s_markers => _s_markers ??= new ProfilerMarkers();

        // TODO: This should just include LodCost?
        protected sealed class PrimitiveRenderData : IDisposable
        {
            public PrimitiveRenderData(CAPI.ovrAvatar2NodeId mshNodeId
                , CAPI.ovrAvatar2Id primId
                , CAPI.ovrAvatar2PrimitiveRenderInstanceID renderInstId
                , OvrAvatarRenderable ovrRenderable, OvrAvatarPrimitive ovrPrim)
            {

                meshNodeId = mshNodeId;
                primitiveId = primId;
                instanceId = renderInstId;
                renderable = ovrRenderable;
                skinnedRenderable = ovrRenderable as OvrAvatarSkinnedRenderable;
                hasSkinnedRenderable = skinnedRenderable != null;
                primitive = ovrPrim;
            }

            public readonly CAPI.ovrAvatar2NodeId meshNodeId;
            public readonly CAPI.ovrAvatar2Id primitiveId;
            public readonly CAPI.ovrAvatar2PrimitiveRenderInstanceID instanceId;
            public readonly OvrAvatarRenderable renderable;
            public readonly OvrAvatarSkinnedRenderable skinnedRenderable;
            public readonly OvrAvatarPrimitive primitive;

            public readonly bool hasSkinnedRenderable;

            public bool IsValid => instanceId != CAPI.ovrAvatar2PrimitiveRenderInstanceID.Invalid;

            public void Dispose()
            {
                if (renderable != null) { renderable.Dispose(); }
            }

            public override string ToString()
            {
                return $"nodeId:{meshNodeId},primId:{primitiveId},instId:{instanceId},renderable:{renderable.name},prim:{primitive.name}";
            }
        }

        //:: Public Properties

        public bool HasJoints => SkeletonJointCount > 0;

        // TODO: Remove and replace IsCreated and IsLoading with corresponding checks of LoadState?
        public bool IsCreated => entityId != CAPI.ovrAvatar2EntityId.Invalid;

        /// True if the model assets have been loaded, and are currently being applied to the avatar.
        public bool IsApplyingModels { get; private set; }

        /// True if the assets for an avatar are still loading.
        public bool IsPendingAvatar
        {
            get => IsPendingCdnAvatar || IsPendingZipAvatar;
        }

        /// True if the avatar is still loading the default avatar.
        public bool IsPendingDefaultModel { get; private set; }

        /// True if the avatar is still loading assets from a ZIP file.
        public bool IsPendingZipAvatar { get; private set; }

        /// True if the avatar is still loading assets from a CDN.
        public bool IsPendingCdnAvatar { get; private set; }

        /// True if the avatar has any avatar loaded other than the default avatar.
        public bool HasNonDefaultAvatar { get; private set; }

        /// Number of joints in the avatar skeleton.
        public int SkeletonJointCount => _skeleton.Length;

        // TODO: what does "active" mean?
        /// True if this entity is active.
        public bool EntityActive
        {
            get => _lastActive;
            set => _nextActive = value;
        }

        protected virtual bool CreateEntityOnAwake => true;

        // External classes or overrides can add to this Action as needed.
        [System.Obsolete("Use `OnAvatarCulled` instead", false)]
        public event Action<bool> OnCulled;

        public event Action<OvrAvatarEntity, bool> OnAvatarCulled;

        // The list of child GPU instances of the avatar
        private readonly List<GPUInstancedAvatar> GPUInstances = new List<GPUInstancedAvatar>();

        //:: Serialized Fields
        /**
         * Avatar creation configuration to use when creating this avatar.
         * Specifies overall level of detail, body parts to display,
         * rendering and animation characteristics and the avatar's point of view.
         *
         * If you are instantiating the same avatar asset more than once in the same
         * scene, you must use the same *renderFilter* for both. Using different
         * render filters on the same avatar asset is not supported.
         *
         * @see CAPI.ovrAvatar2EntityCreateInfo
         * @see CAPI.ovrAvatar2EntityLODFlags
         * @see CAPI.ovrAvatar2EntityManifestationFlags
         * @see CAPI.ovrAvatar2EntityFeatures
         * @see CAPI.ovrAvatar2EntityFilters
         */
        [Tooltip("Include the largest set of options that will be needed at runtime.")]
        [SerializeField]
        protected CAPI.ovrAvatar2EntityCreateInfo _creationInfo = new CAPI.ovrAvatar2EntityCreateInfo()
        {
            features = CAPI.ovrAvatar2EntityFeatures.Preset_Default,
            renderFilters = CAPI.OvrAvatar2_DefaultLoadFilters
        };

        [Tooltip("If set to true, on the next `LoadUser` call any cached asset will be validated with the server before loading.\n" +
            "This will increase load times, but will ensure that you will not temporarily see an older avatar.\n")]
        [SerializeField]
        protected bool _validateCache = true;

        [Tooltip("Active view should match the views available in the creation info render filters.")]
        [SerializeField, SingleSelectEnum(EnumType = typeof(CAPI.ovrAvatar2EntityViewFlags), HiddenValues = new int[] { (int)CAPI.ovrAvatar2EntityViewFlags.All })]
        private CAPI.ovrAvatar2EntityViewFlags _activeView = CAPI.ovrAvatar2EntityViewFlags.FirstPerson;
        [Tooltip("Active manifestation should match the manifestations available in the creation info render filters.")]
        [SerializeField, SingleSelectEnum(EnumType = typeof(CAPI.ovrAvatar2EntityManifestationFlags), HiddenValues = new int[] { (int)CAPI.ovrAvatar2EntityManifestationFlags.All })]
        private CAPI.ovrAvatar2EntityManifestationFlags _activeManifestation = CAPI.ovrAvatar2EntityManifestationFlags.Half;

#if UNITY_EDITOR
        // to validate of changes between _activeManifestation and _creationInfo.renderFilters.manifestationFlags
        // as well between _activeView and _creationInfo.renderFilters.viewFlags,
        // and between _activeSubMeshes and_creationInfo.renderFilters.subMeshInclusionFlags

        EnumFlagsValidator<CAPI.ovrAvatar2EntityManifestationFlags> ManifestationEnumFlagsValidator = new EnumFlagsValidator<CAPI.ovrAvatar2EntityManifestationFlags>();
        EnumFlagsValidator<CAPI.ovrAvatar2EntityViewFlags> ViewEnumFlagsValidator = new EnumFlagsValidator<CAPI.ovrAvatar2EntityViewFlags>();
        EnumFlagsValidator<CAPI.ovrAvatar2EntitySubMeshInclusionFlags> SubMeshInclusionEnumFlagsValidator = new EnumFlagsValidator<CAPI.ovrAvatar2EntitySubMeshInclusionFlags>();
#endif

#if UNITY_EDITOR
        [Tooltip("These flags allow the original sub-meshes used to create the avatar to be enbled in the index buffer. This only works in Unity Editor. To use this for production, modify the CreationInfo.RenderFilters.SubMeshInclusionFlags above.")]
        [SerializeField]
        private CAPI.ovrAvatar2EntitySubMeshInclusionFlags _activeSubMeshes = CAPI.ovrAvatar2EntitySubMeshInclusionFlags.All;
        private bool _enableActiveSubMeshDebugging = false;

        [Tooltip("If new sub-mesh types are introduced after this version of the SDK, this flag determines if they are visible by default. If excluding only one mesh it's best to heep this on. If including ony one mesh it's best to keep this off.")]
        [SerializeField]
        private bool _activeSubMeshesIncludeUntyped = true;
#endif

        [Header("Tracking Input")]
        [SerializeField]
        [FormerlySerializedAs("_bodyTracking")]
        private OvrAvatarInputManagerBehavior _inputManager;

        [FormerlySerializedAs("_faceTrackingBehavior")]
        [SerializeField]
        private OvrAvatarFacePoseBehavior _facePoseBehavior;

        [FormerlySerializedAs("_eyeTrackingBehavior")]
        [SerializeField]
        private OvrAvatarEyePoseBehavior _eyePoseBehavior;

        [SerializeField]
        private OvrAvatarLipSyncBehavior _lipSync;

        [Header("Skeleton Output")]

        [Tooltip("Joints which will always have their Unity.Transform updated")]
        [SerializeField]
        protected CAPI.ovrAvatar2JointType[] _criticalJointTypes = Array.Empty<CAPI.ovrAvatar2JointType>();

        [Header("Experimental Features", order = 30)]
        [Tooltip("Use Unity Transform Jobs to update critical joint transforms for this entity")]
        [SerializeField] private bool _useCriticalJointJobs = false;


        private uint[] _unityUpdateJointIndices = Array.Empty<uint>(); // names with missing/inactive joints removed

        //:: Protected Variables
        protected internal virtual Transform _baseTransform => transform;

        protected CAPI.ovrAvatar2EntityId entityId { get; private set; } = CAPI.ovrAvatar2EntityId.Invalid;

        internal CAPI.ovrAvatar2EntityId internalEntityId => entityId;

        protected UInt64 _userId = 0;

        protected CAPI.ovrAvatar2EntityViewFlags ActiveView { get; set; }

        //:: Private Variables

        // Coroutine is started on OvrAvatarManager to prevent early stops from disabling the MonoBehaviour
        private OvrTime.SliceHandle _loadingRoutineBacking;
        private OvrTime.SliceHandle _loadingRoutine
        {
            get { return _loadingRoutineBacking; }
            set
            {
                if (_loadingRoutineBacking.IsValid)
                {
                    if (value.IsValid)
                    {
                        OvrAvatarLog.LogError("More than one loading function running simultaneously!", logScope, this);
                    }
                    _loadingRoutineBacking.Cancel();
                }

                _loadingRoutineBacking = value;
            }
        }

        // Mappings
        protected PrimitiveRenderData[][] _visiblePrimitiveRenderers = Array.Empty<PrimitiveRenderData[]>();

        // TODO: This likely has no utility anymore? It's a mirror of _meshNodes w/ a different key
        protected readonly Dictionary<CAPI.ovrAvatar2PrimitiveRenderInstanceID, PrimitiveRenderData[]> _primitiveRenderables
            = new Dictionary<CAPI.ovrAvatar2PrimitiveRenderInstanceID, PrimitiveRenderData[]>();

        // Render Data
        private SkeletonJoint[] _skeleton = Array.Empty<SkeletonJoint>();

        private readonly Dictionary<CAPI.ovrAvatar2NodeId, uint> _nodeToIndex = new Dictionary<CAPI.ovrAvatar2NodeId, uint>();
        private readonly Dictionary<CAPI.ovrAvatar2JointType, CAPI.ovrAvatar2NodeId> _jointTypeToNodeId = new Dictionary<CAPI.ovrAvatar2JointType, CAPI.ovrAvatar2NodeId>();

        private bool _lastActive = true;
        private bool _nextActive = true;

        // Suppress "never used" warning in non-editor builds
#pragma warning disable 0169
        // Suppress "never assigned to" warning
#pragma warning disable 0649
        [System.Serializable]
        protected struct DebugDrawing
        {
            [Header("Debug drawing is visible in Scene view only, not Game view.")]
            public bool drawTrackingPose;
            public bool drawBoneNames;
            public bool drawSkelHierarchy;
            public bool drawSkelHierarchyInGame;
            public bool drawSkinTransformsInGame;
            public bool drawCriticalJoints;
            public Color skeletonColor;
            public bool drawGazePos;
            public Color gazePosColor;
        }

        [Header("Debug")]

        [SerializeField]
        protected DebugDrawing _debugDrawing = new DebugDrawing
        {
            skeletonColor = Color.red,
            gazePosColor = Color.magenta
        };
#pragma warning restore 0649
#pragma warning restore 0169

        public bool TrackingPoseValid
        {
            get
            {
                if (IsCreated)
                {
                    var result = CAPI.ovrAvatar2Tracking_GetPoseValid(entityId, out var isValid);
                    if (result == CAPI.ovrAvatar2Result.Success)
                    {
                        return isValid;
                    }
                    OvrAvatarLog.LogError($"ovrAvatar2Tracking_GetPoseValid failed with {result}");
                    return false;
                }
                return false;
            }
        }

        internal bool UseCriticalJointJobs => _useCriticalJointJobs;

        private MaterialPropertyBlock _gpuInstanceMaterialProperties = null;

        private bool _hasSetCreationContext = false;

        /////////////////////////////////////////////////
        //:: Core Unity Functions

        #region Core Unity Functions

        protected virtual void Awake()
        {

            if (_creationInfo.renderFilters.quality == CAPI.ovrAvatar2EntityQuality.Ultralight)
            {
                OvrAvatarLog.LogWarning("Ultralight quality not supported for current avatar version. Overriding to Light quality");
                _creationInfo.renderFilters.quality = CAPI.ovrAvatar2EntityQuality.Light;
            }

            if (CreateEntityOnAwake)
            {
                CreateEntity();
            }

#if UNITY_EDITOR
            SceneView.duringSceneGui += OnSceneGUI;
#endif
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Camera.onPostRender += OnCameraPostRender;
#endif
            InitAvatarLOD();
            OnAwakeAttachables();
        }


        public void UpdateLODInternal()
        {
            if (!isActiveAndEnabled || !IsCreated)
            {
                return;
            }

            Profiler.BeginSample("OvrAvatarEntity::UpdateLODInternal");

            UpdateAvatarLODOverride();

            SendImportanceAndCost();

            Profiler.EndSample();
        }

        // Non-virtual update run before virtual method
        internal unsafe bool PreSDKUpdateInternal(CAPI.ovrAvatar2Transform* transforms, uint index, bool active)
        {
            using var livePerfMarker = OvrAvatarXLivePerf_Marker(ovrAvatarXLivePerf_UnitySectionID.Entity_PreUpdate);
            // State validation
            // Validate sync of tracked state
            OvrAvatarLog.AssertConstMessage(active == _lastActive
                , "Inconsistent activity state", logScope, this);

            // Apply C# active/inactive state to nativeSDK
            if (_lastActive != _nextActive)
            {
                if (CAPI.ovrAvatar2Entity_SetActive(entityId, _nextActive)
                    .EnsureSuccess("ovrAvatar2Entity_SetActive", logScope, this))
                {
                    _lastActive = _nextActive;
                }
            }

            // If active, apply C# state changes to nativeSDK
            transforms[index] = _baseTransform.ToWorldOvrTransform().ConvertSpace();

            return _nextActive;
        }

        internal void PostSDKUpdateInternal(bool active)
        {
            // TODO: Notify other systems of state change?
            _lastActive = _nextActive = active;
        }

        // This behaviour is manually updated at a specific time during OvrAvatarManager::Update()
        // to prevent issues with Unity script update ordering
        internal void UpdateInternal(float dT)
        {
            if (!isActiveAndEnabled || !IsCreated) { return; }

            Profiler.BeginSample("OvrAvatarEntity::UpdateInternal");
            using var livePerfMarker = OvrAvatarXLivePerf_Marker(ovrAvatarXLivePerf_UnitySectionID.Entity_Update);

            if (IsApplyingModels || IsPendingAvatar)
            {
                Profiler.BeginSample("OvrAvatarEntity::UpdateLoadingStatus");
                // TODO: What other errors should we be checking for?
                var result = entityStatus;

                // Occurs when spec download fails
                if (result == CAPI.ovrAvatar2Result.DataNotAvailable
                // Occurs when failing to parse glb asset
                    || result == CAPI.ovrAvatar2Result.InvalidData
                // Occurs when CDN download fails
                    || result == CAPI.ovrAvatar2Result.Unknown)
                {
#pragma warning disable 618
                    LoadState = LoadingState.Failed;
#pragma warning restore 618
                    IsApplyingModels = false;
                }
                Profiler.EndSample();
            }

            // Skip active render state updates if the entity is not active
            if (EntityActive)
            {
                Profiler.BeginSample("OvrAvatarEntity::EntityActiveRenderUpdate");
                EntityActiveRenderUpdate(dT);
                Profiler.EndSample();
            }

            // Only apply render updates if created and not loading
            if (IsCreated)
            {
                Profiler.BeginSample("OvrAvatarEntity::PerFrameRenderUpdate");
                PerFrameRenderUpdate(dT);
                Profiler.EndSample();
            }

            if (!IsLocal && useRenderLods && !IsApplyingModels)
            {
                Profiler.BeginSample("OvrAvatarEntity::ComputeNetworkLod");
                ComputeNetworkLod();
                Profiler.EndSample();
            }
            {
                using var attachablesScope = s_markers.AttachablesMarker.Auto();
                OnUpdateAttachables();
            }
            // accessing obsolete members
#pragma warning disable 618
            if (_lastInvokedLoadState != LoadState)
            {
                _lastInvokedLoadState = LoadState;

                LoadingStateChanged.Invoke(LoadState);
                EntityLoadingStateChanged.Invoke(this);
            }
#pragma warning restore 618

            Profiler.EndSample(); // "OvrAvatarEntity::UpdateInternal"
        }

        protected void OnDestroy()
        {
            if (Point2PointCorrespondenceManager.HasInstance)
            {
                Point2PointCorrespondenceManager.Instance.ClearP2PCorrespondences(entityId);
            }

            bool shouldTearDown = IsCreated || CurrentState != AvatarState.None || _skeleton.Length > 0 ||
                                  IsApplyingModels || _loadingRoutine.IsValid || _jointMonitor != null ||
                                  !(_avatarLOD is null);
            if (shouldTearDown)
            {
                Teardown();
            }
            OnDestroyAttachables();
            OnDestroyCalled();

#if UNITY_EDITOR
            SceneView.duringSceneGui -= OnSceneGUI;
#endif
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Camera.onPostRender -= OnCameraPostRender;
#endif
        }

        protected virtual void OnDestroyCalled() { }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            // validate/warn of changes between the flags and the bit fields where they are inconsistent and allow for quick corrections
            ManifestationEnumFlagsValidator.DoValidation(ref _activeManifestation,
                                                         ref _creationInfo.renderFilters.manifestationFlags,
                                                         "Active Manifestation",
                                                         "Creation Info Manifestation");
            ViewEnumFlagsValidator.DoValidation(ref _activeView,
                                                ref _creationInfo.renderFilters.viewFlags,
                                                "Active View",
                                                "Creation Info View");
            SubMeshInclusionEnumFlagsValidator.DoValidation(ref _activeSubMeshes,
                                                            ref _creationInfo.renderFilters.subMeshInclusionFlags,
                                                            "Active Sub Meshes",
                                                            "Creation Info Sub Mesh");

            // This flag has been hiden away in inspector, this logic disables the flag if it was previously enabled.
            _creationInfo.features &= ~CAPI.ovrAvatar2EntityFeatures.AnalyticIk;

            // Allows the tracking to up updated in the editor.
            SetInputManager(_inputManager);

            SetFacePoseProvider(_facePoseBehavior);
            SetEyePoseProvider(_eyePoseBehavior);

            SetLipSync(_lipSync);
            if (IsCreated)
            {
                RefreshAllActives();
                Hidden = Hidden;
            }
            OnValidateAttachables();
        }
#endif // UNITY_EDITOR

        #endregion

        /////////////////////////////////////////////////
        //:: Protected Functions

        #region Life Cycle

        /**
         * Initializes this OvrAvatarEntity instance.
         * An avatar may have many representations varying in complexity
         * and fidelity. These are described by @ref CAPI.ovrAvatar2EntityCreateInfo.
         * The value of @ref OvrAvatarEntity._creationInfo specifies the
         * avatar configuration to create this entity.
         *
         * You can customize entity creation and settings by extending this class and overriding
         * [ConfigureCreationInfo](@ref OvrAvatarEntity.ConfigureCreationInfo) or
         * [ConfigureEntity](@ref OvrAvatarEntity.ConfigureEntity)
         * @see CAPI.ovrAvatar2EntityCreateInfo
         * @see CAPI.ovrAvatar2EntityFeatures
         * @see CAPI.ovrAvatar2EntityManifestationFlags
         * @see CAPI.ovrAvatar2EntityViewFlags
         */
        protected void CreateEntity()
        {
            if (IsCreated)
            {
                OvrAvatarLog.LogWarning("Setup called on an entity that has already been set up.", logScope, this);
                return;
            }
            if (_material == null)
            {
                _material = new OvrAvatarMaterial();
            }

            var createInfoOverride = ConfigureCreationInfo();
            if (createInfoOverride.HasValue)
            {
                _creationInfo = createInfoOverride.Value;
            }

            AddRequiredExperimentalFeatureFlags(ref _creationInfo.features);

            IsPendingDefaultModel = HasAllFeatures(CAPI.ovrAvatar2EntityFeatures.UseDefaultModel);
            ValidateSkinningType();
            SetRequiredFeatures();

            // This flag has been hiden away in inspector, this logic makes it so it's always disabled.
            _creationInfo.features &= ~CAPI.ovrAvatar2EntityFeatures.AnalyticIk;

            entityId = CreateNativeEntity(in _creationInfo);
            if (entityId == CAPI.ovrAvatar2EntityId.Invalid)
            {
                OvrAvatarLog.LogError("Failed to create entity pointer.", logScope, this);
                IsPendingDefaultModel = false;
                return;
            }

            OvrAvatarManager.Instance.AddTrackedEntity(this);

#pragma warning disable 618
            LoadState = LoadingState.Created;
#pragma warning restore 618

            IsApplyingModels = false;
            CurrentState = AvatarState.Created;
            InvokeOnCreated();

            RefreshAllActives();

            if (IsLocal)
            {
                ConfigureLocalAvatarSettings();
            }
            else
            {
                SetStreamingPlayback(true);
            }

            OvrAvatarLog.LogDebug($"Entity {entityId} created as a {(IsLocal ? "local" : "remote")} avatar", logScope, this);

            InitializeTrackingProviders();

            ConfigureEntity();

            // For right now, only GPU/compute skinning supports motion smoothing
            if (((UseGpuSkinning || UseComputeSkinning) && UseMotionSmoothingRenderer))
            {
                var entityAnimator = new EntityAnimatorMotionSmoothing(this);

                _entityAnimator = entityAnimator;
                _interpolationValueProvider = entityAnimator;
            }
            else
            {
                _entityAnimator = new EntityAnimatorDefault(this);
            }

            if (UseGpuSkinning || UseComputeSkinning)
            {
                if (UseMotionSmoothingRenderer)
                {
                    _jointMonitor = _useCriticalJointJobs || OvrAvatarManager.Instance.UseCriticalJointJobs
                        ? new OvrAvatarEntitySmoothingJointJobMonitor(this, _interpolationValueProvider)
                        : new OvrAvatarEntitySmoothingJointMonitor(this, _interpolationValueProvider);
                }
                else
                {
                    _jointMonitor = _useCriticalJointJobs
                        ? new OvrAvatarEntityJointJobMonitor(this)
                        : new OvrAvatarEntityJointMonitor(this);
                }
            }
            else
            {
                // Only GpuSkinning supports MotionSmoothing, if we didn't/can't use that - indicate that it is off
                MotionSmoothingSettings = MotionSmoothingOptions.FORCE_OFF;
            }
        }

        private void InitializeTrackingProviders()
        {
            SetInputManager(_inputManager, true);
            SetFacePoseProvider(_facePoseBehavior);
            SetEyePoseProvider(_eyePoseBehavior);
            SetLipSync(_lipSync);
        }

        /**
         * To entity settings, extend this class and override ConfigureEntity
         *
         * @code
         * class MyAvatar : public OvrAvatarEntity
         * {
         *     protected override void ConfigureEntity()
         *     {
         *
         *         // ex: set to third person
         *         SetActiveView(...);
         *         SetActiveManifestation(...);
         *         SetActiveSubMeshInclusion(...);
         *         SetInputManager(...);
         *         SetLipSync(...);
         *     }
         * @endcode
         *
        **/
        protected virtual void ConfigureEntity()
        {
            OvrAvatarLog.LogVerbose("Base ConfigureEntity Invoked", logScope, this);
        }

        /**
         * To configure creation info on an entity, extend this class and override ConfigureCreationInfo
         *
         * @code
         * class MyAvatar : public OvrAvatarEntity
         * {
         *     protected override void ConfigureCreationInfo()
         *     {
         *        return new CAPI.ovrAvatar2EntityCreateInfo
         *           {
         *               features = CAPI.ovrAvatar2EntityFeatures.Preset_Default,
         *               renderFilters = CAPI.OvrAvatar2_DefaultLoadFilters
         *           };
         *      }
         * };
         *
         *  It is necessary to set all of the members of
         *  @ref ovrAvatar2EntityCreateInfo. The default values are unspecified.
         * @endcode
         *
        **/
        protected virtual CAPI.ovrAvatar2EntityCreateInfo? ConfigureCreationInfo()
        {
            OvrAvatarLog.LogVerbose("Base ConfigureCreationInfo Invoked", logScope, this);
            return null;
        }

        protected static void AddRequiredExperimentalFeatureFlags(ref CAPI.ovrAvatar2EntityFeatures featureFlags)
        {
            featureFlags.AddRequiredExperimentalFeatures();
        }

        protected static bool HasExperimentalFeatureFlags(CAPI.ovrAvatar2EntityFeatures featureFlags)
        {
            return featureFlags.HasAnyExperimentalFeatures();
        }
        protected static bool HasAllExperimentalFeatureFlags(CAPI.ovrAvatar2EntityFeatures featureFlags)
        {
            return featureFlags.HasAllExperimentalFeatures();
        }

        //adds the ability to override the initial manifestation
        protected void SetInitialManifestatation(CAPI.ovrAvatar2EntityManifestationFlags manifestation)
        {
            OvrAvatarLog.AssertConstMessage(!IsCreated, "SetInitialManifestation should only be called before the entity IsCreated", logScope, this);
            _activeManifestation = manifestation;
        }

        public class GPUInstancedAvatar : MonoBehaviour
        {
            public Transform parentTransform = null;
            // TODO: this can be passed to the class as a configuration parameter
            private readonly Matrix4x4 _reflectionMatrix = new Matrix4x4(
                new Vector4(-1.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

            public void SetTransform(Vector3 position, Quaternion rotation)
            {
                var cachedTransform = transform;
                cachedTransform.localPosition = position;

                // copy scale and rotation from the parent
                cachedTransform.localScale = parentTransform.localScale;
                cachedTransform.rotation = parentTransform.rotation * rotation;
            }

            public Matrix4x4 GetTransform()
            {
                return _reflectionMatrix * transform.localToWorldMatrix;
            }
        }

        public GameObject CreateGPUInstance()
        {
            // Will be used later during rendering
            if (_gpuInstanceMaterialProperties == null)
            {
                _gpuInstanceMaterialProperties = new MaterialPropertyBlock();
            }

            SetActiveView(CAPI.ovrAvatar2EntityViewFlags.ThirdPerson);
            var instance = new GameObject($"{name}_gpu_instance_{GPUInstances.Count}");
            var gpuInstancedAvatar = instance.AddComponent<GPUInstancedAvatar>();
            GPUInstances.Add(gpuInstancedAvatar);
            return instance;
        }

        public bool DestroyGPUInstance(GameObject instance)
        {
            OvrAvatarLog.Assert(instance != null, logScope, this);
            var gpuInstancedAvatar = instance.GetComponent<GPUInstancedAvatar>();
            if (gpuInstancedAvatar == null)
            {
                OvrAvatarLog.LogError("Attempted to destroy GPUInstance which does not have a GPUInstancedAvatar component", logScope, instance);
                return false;
            }
            return DestroyGPUInstance(gpuInstancedAvatar);
        }

        public bool DestroyGPUInstance(GPUInstancedAvatar instance)
        {
            OvrAvatarLog.Assert(instance != null, logScope, this);
            if (!GPUInstances.Remove(instance))
            {
                OvrAvatarLog.LogError($"Passed instance {instance} doesn't belong to current OvrAvatarEntity", logScope, instance);
                return false;
            }

            GPUInstancedAvatar.Destroy(instance);
            return true;
        }

        private OvrAvatarSkinnedRenderable findActiveRenderable()
        {
            OvrAvatarSkinnedRenderable activeRenderable = null;
            foreach (var keyval in _skinnedRenderables)
            {
                OvrAvatarSkinnedRenderable renderable = keyval.Value;
                if (renderable.isActiveAndEnabled)
                {
                    activeRenderable = renderable;
                    break;
                }
            }

            return activeRenderable;
        }

        private void UpdateGPUInstances()
        {
            if (GPUInstances.Count == 0)
            {
                return;
            }

            Profiler.BeginSample("OvrAvatarEntity::UpdateGPUInstances");

            var renderable = findActiveRenderable();
            if (renderable == null) { return; }

            renderable.GetRenderParameters(out var mesh, out var material, out var transform, _gpuInstanceMaterialProperties);
            if (mesh == null || material == null || transform == null)
            {
                return;
            }

            foreach (var instance in GPUInstances)
            {
                // TODO: this loop can be replaced with calling DrawMeshInstanced for batched GPU instancing
                // but as of 2/16/2022 this doesn't seem to work with the material we're using, perhaps
                // something is making it incompatible with batched instancing
                // Update parent transform so rotation and scale can be copied
                instance.parentTransform = transform;
                Graphics.DrawMesh(mesh, instance.GetTransform(), material, 0, null, 0, _gpuInstanceMaterialProperties);
            }

            Profiler.EndSample();
        }

        private readonly List<OvrAvatarSkinnedRenderable> _perFrameRenderableCache = new List<OvrAvatarSkinnedRenderable>();
        private void PerFrameRenderUpdate(float dT)
        {
            using var livePerfMarker = OvrAvatarXLivePerf_Marker(ovrAvatarXLivePerf_UnitySectionID.Entity_PerFrameRenderUpdate);
            Debug.Assert(IsCreated);

            // Find the visible skinned renderables to update. Note if the renderers all have valid animation data
            _perFrameRenderableCache.Clear();
            var activeAndEnabledRenderables = _perFrameRenderableCache;
            bool doAllRenderersHaveValidAnimationData = true;
            foreach (var primRenderables in _visiblePrimitiveRenderers)
            {
                if (primRenderables == null)
                {
                    continue;
                }
                foreach (var primRenderable in primRenderables)
                {
                    var skinnedRenderable = primRenderable.skinnedRenderable;
                    if (skinnedRenderable is null || !skinnedRenderable.isActiveAndEnabled) { continue; }

                    activeAndEnabledRenderables.Add(skinnedRenderable);

                    if (doAllRenderersHaveValidAnimationData && !skinnedRenderable.IsAnimationDataCompletelyValid)
                    {
                        // Not valid animation data, thus all renderers' data is not valid
                        doAllRenderersHaveValidAnimationData = false;
                    }
                }
            }

            Profiler.BeginSample("OvrAvatarEntity::UpdateAnimationTime");
            _entityAnimator.UpdateAnimationTime(dT, doAllRenderersHaveValidAnimationData);
            Profiler.EndSample();

            if (_jointMonitor != null)
            {
                Profiler.BeginSample("OvrAvatarEntity::JointMonitor::UpdateJoints");
                _jointMonitor.UpdateJoints(dT);
                Profiler.EndSample();
            }

            BroadcastPerFrameRenderUpdateToVisibleRenderables(activeAndEnabledRenderables);

            UpdateGPUInstances();
        }

        private void BroadcastPerFrameRenderUpdateToVisibleRenderables(List<OvrAvatarSkinnedRenderable> visibleRenderables)
        {
            foreach (var skinnedRenderable in visibleRenderables)
            {
                skinnedRenderable.RenderFrameUpdate();
            }
        }

        private void EntityActiveRenderUpdate(float dT)
        {
            using var livePerfMarker = OvrAvatarXLivePerf_Marker(ovrAvatarXLivePerf_UnitySectionID.Entity_ActiveRenderUpdate);
            Debug.Assert(EntityActive && IsCreated);

            // Check that our pose is valid
            if (!QueryEntityPose(out var entityPose, out var hierarchyVersion)) { return; }
            OvrAvatarLog.Assert(hierarchyVersion != CAPI.ovrAvatar2HierarchyVersion.Invalid, logScope, this);

            if (!QueryEntityRenderState(out var renderState)) { return; }
            OvrAvatarLog.Assert(renderState.allNodesVersion != CAPI.ovrAvatar2EntityRenderStateVersion.Invalid, logScope, this);
            OvrAvatarLog.Assert(renderState.visibleNodesVersion != CAPI.ovrAvatar2EntityRenderStateVersion.Invalid, logScope, this);
            OvrAvatarLog.AssertConstMessage(hierarchyVersion == renderState.hierarchyVersion
                , "hierarchyVersion mismatch", logScope, this);

            // TODO: Checking this isn't really part of "RenderUpdate", move outside this method
            if (_currentHierarchyVersion != hierarchyVersion)
            {
                // Verify an update to this version isn't already in progress
                if (_targetHierarchyVersion != hierarchyVersion)
                {
                    // HACK: If a model is currently being applied, wait for it to finish to avoid race conditions
                    if (IsApplyingModels)
                    {
                        // TODO: Cancel current skeleton update if one is in progress
                        return;
                    }
                    // END-HACK: If a model is currently being applied, wait for it to finish to avoid race conditions
                    // Refresh current Unity version to match the current runtime version
                    QueueUpdateSkeleton(hierarchyVersion);
                }

                // We can not update our current render objects w/ the native runtime state (out of sync)
                // Leave Unity in previous state (freeze render updates)
                return;
            }

            // TODO: We shouldn't need to stall here, this matches "legacy" behavior more accurately though
            if (_currentVisibleNodesVersion != renderState.visibleNodesVersion)
            {
                // Return if we are already processing the target version or we are currently applying models
                if (_targetVisibleNodesVersion == renderState.visibleNodesVersion || IsApplyingModels) { return; }

                QueueBuildPrimitives(renderState.visibleNodesVersion);

                // Leave previous state (freeze render updates)
                return;
            }

            // Clean up nodes which are no longer in use
            // TODO: This likely isn't the best timing for this?
            UpdateAllNodes(in renderState);

            Profiler.BeginSample("OvrAvatarEntity::OnActiveRender");
            OnActiveRender(in renderState, in entityPose, hierarchyVersion, dT);
            Profiler.EndSample();
        }

        private void QueueUpdateSkeleton(CAPI.ovrAvatar2HierarchyVersion hierarchyVersion)
        {
            _targetHierarchyVersion = hierarchyVersion;

            OvrAvatarLog.Assert(!IsApplyingModels);
            // Reset load state
#pragma warning disable 618
            LoadState = LoadingState.Loading;
#pragma warning restore 618
            IsApplyingModels = true;

            OvrAvatarManager.Instance.QueueLoadAvatar(this, () =>
            {
                _loadingRoutine = OvrTime.Slice(LoadAsync_BuildSkeletonAndPrimitives());
            });
        }
        private void QueueBuildPrimitives(CAPI.ovrAvatar2EntityRenderStateVersion visibleNodesVersion)
        {
            _targetVisibleNodesVersion = visibleNodesVersion;

            OvrAvatarLog.Assert(!IsApplyingModels);
            // Reset load state
#pragma warning disable 618
            LoadState = LoadingState.Loading;
#pragma warning restore 618
            IsApplyingModels = true;

            OvrAvatarManager.Instance.QueueLoadAvatar(this, () =>
            {
                _loadingRoutine = OvrTime.Slice(LoadAsync_BuildPrimitives());
            });
        }

        private bool ShouldRender()
        {
            // If the avatar is remote, and it hasn't received any stream data yet, delay rendering until it does.
            // This prevents the avatar from momentarily t-posing after initialization.
            if (!_isLocal && !_initialStreamDataApplied)
            {
                return false;
            }

            return true;
        }

        // TODO*: Better naming when the lifecycle of the entity is fixed properly
        private void OnActiveRender(in CAPI.ovrAvatar2EntityRenderState renderState
            , in CAPI.ovrAvatar2Pose entityPose
            , CAPI.ovrAvatar2HierarchyVersion hierVersion
            , float dT)
        {
            using var livePerfMarker = OvrAvatarXLivePerf_Marker(ovrAvatarXLivePerf_UnitySectionID.Entity_OnActiveRender);
            OvrAvatarLog.AssertConstMessage(IsCreated
                , "OnActiveRender while not created", logScope, this);

            OvrAvatarLog.Assert(_currentHierarchyVersion == hierVersion, logScope, this);

            _entityAnimator.AddNewAnimationFrame(
                Time.time,
                dT,
                entityPose,
                renderState);
        }

        [Obsolete("DidRender is obsolete and will never be invoked.", true)]
        protected virtual void DidRender() { }

        /**
         * Tear down this entity, stop all tracking and rendering
         * and free associated memory.
         * @see CreateEntity
         */
        public void Teardown()
        {
            System.Diagnostics.Debug.Assert(entityId != CAPI.ovrAvatar2EntityId.Invalid);

            if (CurrentState != AvatarState.None)
            {
                InvokePreTeardown();
            }

            // If not, we are shutting down and will skip some steps
            bool hasManagerInstance = OvrAvatarManager.hasInstance;

            OvrAvatarLog.LogDebug($"Tearing down entity {entityId}", logScope, this);
            if (_loadingRoutineBacking.IsValid)
            {
                _loadingRoutineBacking.Cancel();

                if (hasManagerInstance)
                {
                    OvrAvatarManager.Instance.FinishedAvatarLoad();
                }
            }

            if (hasManagerInstance)
            {
                OvrAvatarManager.Instance.RemoveLoadRequests(this);
                if (IsApplyingModels)
                {
                    OvrAvatarManager.Instance.RemoveQueuedLoad(this);
                }

                if (IsCreated)
                {
                    if (!IsLocal)
                    {
                        SetStreamingPlayback(false);
                    }

                    OvrAvatarManager.Instance.RemoveTrackedEntity(this);
                }
            }

            if (_avatarLOD is not null)
            {
                _avatarLOD.Dispose();
                TeardownLodCullingPoints();
                ShutdownAvatarLOD();
            }
            ResetLODRange();

            if (IsCreated)
            {
                var didDestroy = DestroyNativeEntity();
                if (!didDestroy)
                {
                    OvrAvatarLog.LogWarning("Failed to destroy native entity", logScope, this);
                }
            }

#pragma warning disable 618
            LoadState = LoadingState.NotCreated;
#pragma warning restore 618
            IsApplyingModels = false;
            CurrentState = AvatarState.None;

            // TODO: These will get destroyed w/ LODObjects - though being explicit would be nice
            // This trips an error in Unity currently,
            /* "Can't destroy Transform component of 'S0_L2_M1_V1_optimized_geom,0'.
             * If you want to destroy the game object, please call 'Destroy' on the game object instead.
             * Destroying the transform component is not allowed." */
            //for (int i = 0; i < _primitiveRenderables.Length; ++i)
            //{
            //    ref readonly var primRend = ref _primitiveRenderables[i];
            //    DestroyRenderable(in primRend);
            //}
            _visiblePrimitiveRenderers = Array.Empty<PrimitiveRenderData[]>();
            _skinnedRenderables.Clear();
            _meshNodes.Clear();

            _jointMonitor?.Dispose();
            _jointMonitor = null;

            DestroySkeleton();
            TeardownEntityBehavior();

            _currentVisibleNodesVersion = _targetVisibleNodesVersion = CAPI.ovrAvatar2EntityRenderStateVersion.Invalid;
            _currentAllNodesVersion = _targetAllNodesVersion = CAPI.ovrAvatar2EntityRenderStateVersion.Invalid;
            _currentHierarchyVersion = _targetHierarchyVersion = CAPI.ovrAvatar2HierarchyVersion.Invalid;
        }

        #endregion

        #region Asset Loading Requests

        /**
         * Load avatar assets from a block of data.
         * @param data         C++ pointer to asset data.
         * @param size         byte size of asset data block.
         * @param callbackName name of function to call after data has loaded.
         * @see LoadAssetsFromZipSource
         */
        protected void LoadAssetsFromData(IntPtr data, UInt32 size, string callbackName)
        {
            if (!IsCreated)
            {
                OvrAvatarLog.LogError("Cannot load assets before entity has been created.", logScope, this);
                return;
            }

            CAPI.ovrAvatar2Result result = CAPI.OvrAvatar2Entity_LoadMemoryWithFilters(entityId, data, size, callbackName, _creationInfo.renderFilters, out var loadRequestId);
            if (result.IsSuccess())
            {
                ClearFailedLoadState();
                OvrAvatarManager.Instance.RegisterLoadRequest(this, loadRequestId);
            }
            else
            {
                OvrAvatarLog.LogError($"Failed to load asset from data of size {size}. {result}", logScope, this);
            }
        }

        /**
         * Load avatar assets from a block of data.
         * @param data         byte array containing asset data.
         * @param callbackName name of function to call after data has loaded
         * @see LoadAssetsFromZipSource
         */
        protected void LoadAssetsFromData(byte[] data, string callbackName)
        {
            if (!IsCreated)
            {
                OvrAvatarLog.LogError("Cannot load assets before entity has been created.", logScope, this);
                return;
            }

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            LoadAssetsFromData(handle.AddrOfPinnedObject(), (UInt32)data.Length, callbackName);
            handle.Free();
        }

        #endregion

        #region Getters/Setters

        /**
         * Gets the current avatar view flags (first person, third person).
         * It is possible for multiple flags to be true at once.
         * @returns CAPI.ovrAvatar2EntityViewFlags designating avatar viewpoint.
         * @see CAPI.ovrAvatar2EntityViewFlags
         * @see SetActiveView
         */
        protected CAPI.ovrAvatar2EntityViewFlags GetActiveView() => _activeView;

        /**
         * Selects the avatar viewpoint (first person, third person).
         * @param CAPI.ovrAvatar2EntityViewFlags designating avatar viewpoint.
         * @see CAPI.ovrAvatar2EntityViewFlags
         * @see GetActiveView
         */
        protected void SetActiveView(CAPI.ovrAvatar2EntityViewFlags view)
        {
            var result = CAPI.ovrAvatar2Entity_SetViewFlags(entityId, _hidden ? CAPI.ovrAvatar2EntityViewFlags.None : view);
            if (result.EnsureSuccess("ovrAvatar2Entity_SetViewFlags", logScope, this))
            {
                _activeView = view;
            }
        }

        /**
         * Gets the body parts included for this avatar.
         * It is possible for multiple manifestations to be active at once
         * @returns CAPI.ovrAvatar2EntityManifestationFlags designating body parts.
         * @see CAPI.ovrAvatar2EntityManifestationFlags
         * @see SetActiveManifestation
         */
        protected CAPI.ovrAvatar2EntityManifestationFlags GetActiveManifestation() => _activeManifestation;

        /**
         * Selects the body parts to include for this avatar.
         * Loading the changes will be triggered on the next Update
         * @param CAPI.ovrAvatar2EntityManifestationFlags designating avatar body parts to include
         * @see CAPI.ovrAvatar2EntityManifestationFlags
         * @see GetActiveManifestation
         */
        protected void SetActiveManifestation(CAPI.ovrAvatar2EntityManifestationFlags manifestation)
        {
            if (IsCreated)
            {
                var result = CAPI.ovrAvatar2Entity_SetManifestationFlags(entityId, manifestation);
                if (!result.EnsureSuccess("ovrAvatar2Entity_SetManifestationFlags", logScope, this))
                {
                    return;
                }
            }
            _activeManifestation = manifestation;
        }

#if UNITY_EDITOR
        // Although not initially intended, SetActiveSubMeshInclusion() can only be used within the Unity Editor.
        // In order to make sub-meshes appear or disappear, the function currently gets the index buffer of
        // the meshes and creates empty sections to blank these sub-meshes out. Such an operation is still
        // incredibly valuable to a programmer when debugging the Avatar rendering. The Unity API ONLY allows
        // mesh manipulation while debugging to protect the sanctity of these buffers and avoid security threats.
        // Currently, to use sub-mesh inclusion in production, it has to be set into the creation info of the
        // Avatar Entity, rather than the Active flags. If switching meshes on and off are required, the best way
        // is to create multiple Entities with different creation flags and swap inbetween them using GameObject.SetActive().
        protected void SetActiveSubMeshInclusion(CAPI.ovrAvatar2EntitySubMeshInclusionFlags subMeshInclusion)
        {
            var result = CAPI.ovrAvatar2Entity_SetSubMeshInclusionFlags(entityId, subMeshInclusion);

            // By default we do not want to start manipulating a sub-mesh until it is necessary and
            // the sub-mesh inclusion is incomplete. Dynamically loaded Meshes are not writable
            // in Unity, so using this debugger will incur warnings in the debug console.
            if (subMeshInclusion != CAPI.ovrAvatar2EntitySubMeshInclusionFlags.All &&
               (Int32)subMeshInclusion != CAPI.SubMesh_All_Exclusive)
            {
                _enableActiveSubMeshDebugging = true;
            }

            if (_enableActiveSubMeshDebugging && result.IsSuccess())
            {
                _activeSubMeshes = subMeshInclusion;

                // TODO: change this to a class member with a mx number of meshes
                List<UnityEngine.Rendering.SubMeshDescriptor> subMeshDescriptors = new List<UnityEngine.Rendering.SubMeshDescriptor>(64);
                foreach (PrimitiveRenderData[] renderDatas in _visiblePrimitiveRenderers)
                {
                    if (renderDatas == null)
                    {
                        continue;
                    }
                    foreach (PrimitiveRenderData renderData in renderDatas)
                    {
                        OvrAvatarRenderable renderable = renderData.renderable;
                        MeshRenderer renderer = renderable.rendererComponent as MeshRenderer;
                        if (renderer != null)
                        {
                            MeshFilter filter = renderer.GetComponent<MeshFilter>();
                            Mesh mesh = filter.sharedMesh;
#if SUBMESH_DEBUGGING
                            OvrAvatarLog.LogInfo("BEFORE MeshInfo: " + mesh.triangles.Length + " triangles, " + mesh.subMeshCount + " submesh count", logScope);
                            for (int i = 0; i < mesh.subMeshCount; i++) {
                                UnityEngine.Rendering.SubMeshDescriptor desc = mesh.GetSubMesh(i);
                                OvrAvatarLog.LogInfo("BEFORE SubMeshInfo[" + i + "]: " + desc.indexStart + ", " + desc.indexCount, logScope);
                            }
#endif
                            int originalNumberIndices = renderable.originalNumberIndices;
                            UInt16[] originalIndexBuffer = renderable.originalIndexBuffer;
                            if (originalNumberIndices <= 0 && mesh.triangles.Length > 0)
                            {
                                renderable.originalNumberIndices = mesh.triangles.Length;
                                renderable.originalIndexBuffer = Array.ConvertAll(mesh.triangles, item => (UInt16)item);

                                originalNumberIndices = renderable.originalNumberIndices;
                                originalIndexBuffer = renderable.originalIndexBuffer;
                            }

                            if (originalNumberIndices <= 0)
                            {
                                // we can't continue at this point, it may be indicative that the model is still loading/unloaded
                            }
                            else if ((subMeshInclusion & CAPI.ovrAvatar2EntitySubMeshInclusionFlags.All) == CAPI.ovrAvatar2EntitySubMeshInclusionFlags.All)
                            {
                                int unitySubMeshIndex = 0;
                                UnityEngine.Rendering.SubMeshDescriptor desc = new UnityEngine.Rendering.SubMeshDescriptor(0, originalNumberIndices);
                                mesh.SetIndexBufferParams(originalNumberIndices, UnityEngine.Rendering.IndexFormat.UInt16);
                                mesh.SetIndexBufferData<UInt16>(originalIndexBuffer, 0, 0, originalNumberIndices);
                                mesh.subMeshCount = 1;
                                mesh.SetTriangles(originalIndexBuffer, unitySubMeshIndex);
                                mesh.SetSubMesh(unitySubMeshIndex, desc);
                            }
                            else
                            {
                                // each primitive has it's own submeshes so do a for loop on the subMeshes
                                uint avatarSdkSubMeshCount = 0;
                                var countResult = CAPI.ovrAvatar2Primitive_GetSubMeshCount(renderData.primitiveId, out avatarSdkSubMeshCount);
                                if (countResult.IsSuccess())
                                {
                                    unsafe
                                    {
                                        int totalSubMeshBufferSize = 0;

                                        for (uint subMeshIndex = 0; subMeshIndex < avatarSdkSubMeshCount; subMeshIndex++)
                                        {

                                            CAPI.ovrAvatar2PrimitiveSubmesh subMesh;
                                            var subMeshResult = CAPI.ovrAvatar2Primitive_GetSubMeshByIndex(renderData.primitiveId, subMeshIndex, out subMesh);
                                            if (subMeshResult.IsSuccess())
                                            {
                                                CAPI.ovrAvatar2EntitySubMeshInclusionFlags localInclusionFlags = subMesh.inclusionFlags;
                                                if (!(localInclusionFlags == CAPI.ovrAvatar2EntitySubMeshInclusionFlags.All && !_activeSubMeshesIncludeUntyped) && (localInclusionFlags & subMeshInclusion) != 0)
                                                {
                                                    UnityEngine.Rendering.SubMeshDescriptor desc = new UnityEngine.Rendering.SubMeshDescriptor((int)subMesh.indexStart, (int)subMesh.indexCount);
                                                    subMeshDescriptors.Add(desc);
                                                    totalSubMeshBufferSize += desc.indexCount;
                                                }
                                            }
                                        }

                                        mesh.SetIndexBufferParams(originalNumberIndices, UnityEngine.Rendering.IndexFormat.UInt16);
                                        UInt16[] indexBufferCopy;
                                        {
                                            indexBufferCopy = new UInt16[totalSubMeshBufferSize];
                                            // Workaround because Sub Mesh API is not working as planned...
                                            int subMeshDestination = 0;
                                            for (int unitySubMeshIndex = 0; unitySubMeshIndex < subMeshDescriptors.Count; unitySubMeshIndex++)
                                            {
                                                var desc = subMeshDescriptors[unitySubMeshIndex];
                                                for (int indexNumber = desc.indexStart; indexNumber < desc.indexCount + desc.indexStart; indexNumber++)
                                                {
                                                    indexBufferCopy[subMeshDestination] = renderable.originalIndexBuffer[indexNumber];
                                                    subMeshDestination++;
                                                }
                                            }
                                        }
                                        subMeshDescriptors.Clear();

                                        mesh.subMeshCount = 1;
                                        mesh.SetIndexBufferData<UInt16>(indexBufferCopy, 0, 0, totalSubMeshBufferSize);
                                        mesh.SetTriangles(indexBufferCopy, 0);
                                    }
                                }
                            }
#if SUBMESH_DEBUGGING
                            OvrAvatarLog.LogInfo("AFTER MeshInfo: " + mesh.triangles.Length + " triangles, " + mesh.subMeshCount + " submesh count", logScope);
                            for (int i = 0; i < mesh.subMeshCount; i++)
                            {
                                UnityEngine.Rendering.SubMeshDescriptor desc = mesh.GetSubMesh(i);
                                OvrAvatarLog.LogInfo("AFTER SubMeshInfo[" + i + "]: " + desc.indexStart + ", " + desc.indexCount, logScope);
                            }
#endif
                        }
                    }
                }
            }
        }
#endif // UNITY_EDITOR

        /**
         * Gets the current avatar high quality flags (normal map, hair map).
         * It is possible for multiple flags to be true at once.
         * @returns CAPI.ovrAvatar2EntityQuality designating avatar quality vs perf level
         * @see CAPI.ovrAvatar2EntityQuality
         * @see SetActiveQuality
         */
        protected CAPI.ovrAvatar2EntityQuality GetActiveQuality()
        {
            return _creationInfo.renderFilters.quality;
        }

        /**
         * Selects the high quality flags (normal map, hair map).
         * @param CAPI.ovrAvatar2EntityQuality designating avatar quality vs perf level
         * @see CAPI.ovrAvatar2EntityViewFlags
         * @see GetActiveQuality
         */
        private void SetActiveQuality(CAPI.ovrAvatar2EntityQuality quality)
        {
            var result = CAPI.ovrAvatar2Entity_SetQuality(entityId, quality);
            if (result.IsSuccess())
            {
                // TODO: Should we iterate over all the renderers here the way the submeshes do?
                // TODO: change this to a class member with a mx number of meshes
                List<UnityEngine.Rendering.SubMeshDescriptor> subMeshDescriptors = new List<UnityEngine.Rendering.SubMeshDescriptor>(64);
                foreach (PrimitiveRenderData[] renderDatas in _visiblePrimitiveRenderers)
                {
                    if (renderDatas == null)
                    {
                        continue;
                    }
                    foreach (PrimitiveRenderData renderData in renderDatas)
                    {
                        OvrAvatarRenderable renderable = renderData.renderable;
                        MeshRenderer renderer = renderable.rendererComponent as MeshRenderer;
                        if (renderer != null)
                        {
                            bool enableNormalMap = (quality <= CAPI.ovrAvatar2EntityQuality.Standard);
                            bool enablePropertyHairMap = (quality <= CAPI.ovrAvatar2EntityQuality.Standard);
                            bool enableRimLighting = (quality <= CAPI.ovrAvatar2EntityQuality.Standard);

                            // The rim lighting in most avatars in most shaders is casting a highly ghostish glow.
                            // We must disable this until it can be repaired
                            enableRimLighting = false;

                            // <= V24, These keywords are set for backwards compatibility
                            if (enableNormalMap)
                            {
                                renderer.sharedMaterial.EnableKeyword("HAS_NORMAL_MAP_ON");
                                renderer.sharedMaterial.SetFloat("HAS_NORMAL_MAP", 1.0f);
                            }
                            else
                            {
                                renderer.sharedMaterial.DisableKeyword("HAS_NORMAL_MAP_ON");
                                renderer.sharedMaterial.SetFloat("HAS_NORMAL_MAP", 0.0f);
                            }

                            if (enablePropertyHairMap)
                            {
                                renderer.sharedMaterial.EnableKeyword("ENABLE_HAIR_ON");
                                renderer.sharedMaterial.SetFloat("ENABLE_HAIR", 1.0f);
                            }
                            else
                            {
                                renderer.sharedMaterial.DisableKeyword("ENABLE_HAIR_ON");
                                renderer.sharedMaterial.SetFloat("ENABLE_HAIR", 0.0f);
                            }

                            if (enableRimLighting)
                            {
                                renderer.sharedMaterial.EnableKeyword("ENABLE_RIM_LIGHT_ON");
                                renderer.sharedMaterial.SetFloat("ENABLE_RIM_LIGHT_ON", 1.0f);
                            }
                            else
                            {
                                renderer.sharedMaterial.DisableKeyword("ENABLE_RIM_LIGHT_ON");
                                renderer.sharedMaterial.SetFloat("ENABLE_RIM_LIGHT_ON", 0.0f);
                            }

                            // > V24, These keywords are set to control Style 2 Avatars
                            if (!renderData.primitive.hasCurvature && !renderData.primitive.hasNormalMap)
                            {   // Choose STYLE_1_LIGHT
                                renderer.sharedMaterial.DisableKeyword("STYLE_2_LIGHT");        // 0.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_2_STANDARD");      // 1.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_2_EXPERIMENTAL"); // 2.0
                                renderer.sharedMaterial.EnableKeyword("STYLE_1_LIGHT");        // 3.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_1_STANDARD");     // 4.0
                                if (renderer.sharedMaterial.HasFloat("Style"))
                                {
                                    renderer.sharedMaterial.SetFloat("Style", 3.0f);
                                }
                            }
                            else if (!renderData.primitive.hasCurvature && renderData.primitive.hasNormalMap)
                            {   // Choose STYLE_1_STANDARD
                                renderer.sharedMaterial.DisableKeyword("STYLE_2_LIGHT");        // 0.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_2_STANDARD");      // 1.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_2_EXPERIMENTAL"); // 2.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_1_LIGHT");        // 3.0
                                renderer.sharedMaterial.EnableKeyword("STYLE_1_STANDARD");     // 4.0
                                if (renderer.sharedMaterial.HasFloat("Style"))
                                {
                                    renderer.sharedMaterial.SetFloat("Style", 4.0f);
                                }
                            }
                            else if (enablePropertyHairMap)
                            {   // Choose STYLE_2_STANDARD
                                renderer.sharedMaterial.DisableKeyword("STYLE_2_LIGHT");        // 0.0
                                renderer.sharedMaterial.EnableKeyword("STYLE_2_STANDARD");      // 1.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_2_EXPERIMENTAL"); // 2.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_1_LIGHT");        // 3.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_1_STANDARD");     // 4.0
                                if (renderer.sharedMaterial.HasFloat("Style"))
                                {
                                    renderer.sharedMaterial.SetFloat("Style", 1.0f);
                                }
                            }
                            else
                            {   // Choose STYLE_2_LIGHT
                                renderer.sharedMaterial.EnableKeyword("STYLE_2_LIGHT");         // 0.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_2_STANDARD");     // 1.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_2_EXPERIMENTAL"); // 2.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_1_LIGHT");        // 3.0
                                renderer.sharedMaterial.DisableKeyword("STYLE_1_STANDARD");     // 4.0
                                if (renderer.sharedMaterial.HasFloat("Style"))
                                {
                                    renderer.sharedMaterial.SetFloat("Style", 0.0f);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                OvrAvatarLog.LogError($"SetQuality Failed: {result}");
            }
        }

        #endregion

        #region Misc
        // TODO: Remove pose requirement
        protected string GetNameForNode(CAPI.ovrAvatar2NodeId nodeId, in CAPI.ovrAvatar2Pose tempPose)
        {
            return CAPI.OvrAvatar2Entity_GetNodeName(entityId, nodeId);
        }

        protected CAPI.ovrAvatar2NodeId GetNodeForType(CAPI.ovrAvatar2JointType type)
        {
            if (!_jointTypeToNodeId.TryGetValue(type, out var nodeId))
            {
                return CAPI.ovrAvatar2NodeId.Invalid;
            }
            return nodeId;
        }

        protected uint GetIndexForNode(CAPI.ovrAvatar2NodeId nodeId)
        {
            if (!_nodeToIndex.TryGetValue(nodeId, out var idx))
            {
                OvrAvatarLog.LogError($"Unable to find index for nodeId '{nodeId}'", logScope, this);
                idx = uint.MaxValue;
            }
            return idx;
        }

        protected SkeletonJoint GetSkeletonJoint(int index)
        {
            Debug.Assert(index >= 0,
                $"Index {index} out of range for Skeleton joints. Joint count is {SkeletonJointCount}");

            return GetSkeletonJoint((uint)index);
        }

        protected SkeletonJoint GetSkeletonJoint(uint index)
        {
            Debug.Assert(index < SkeletonJointCount,
                $"Index {index} out of range for Skeleton joints. Joint count is {SkeletonJointCount}");

            return _skeleton[index];
        }

        protected SkeletonJoint? GetSkeletonJoint(CAPI.ovrAvatar2JointType jointType)
        {
            var nodeId = GetNodeForType(jointType);
            if (nodeId == CAPI.ovrAvatar2NodeId.Invalid) { return null; }

            var idx = GetIndexForNode(nodeId);
            return GetSkeletonJoint(idx);
        }

        protected Transform GetSkeletonTransformByType(CAPI.ovrAvatar2JointType jointType)
        {
            // If joint monitor is disabled due to using Unity Skinning,
            // we can still provide the transform from the entity skeleton hierarchy
            if (_jointMonitor != null && _jointMonitor.TryGetTransform(jointType, out var tx))
            {
                return tx;
            }
            else
            {
                return GetSkeletonJoint(jointType)?.transform;
            }
        }

        public Transform GetSkeletonTransform(CAPI.ovrAvatar2JointType jointType)
        {
            if (!_criticalJointTypes.Contains(jointType))
            {
                OvrAvatarLog.LogError($"Can't access joint {jointType} unless it is in critical joint set");
                return null;
            }

            return GetSkeletonTransformByType(jointType);
        }

        public IReadOnlyList<CAPI.ovrAvatar2JointType> GetCriticalJoints() => _criticalJointTypes;

        public bool HasCriticalJoint(CAPI.ovrAvatar2JointType jointType) => _criticalJointTypes.Contains(in jointType);

        // NOTE: This does NOT retrieve transforms for Critical Joints when Optimize Critical Joints is enabled
        protected Transform GetSkeletonTxByIndex(int index)
        {
            OvrAvatarLog.Assert(index >= 0);
            return GetSkeletonTxByIndex((uint)index);
        }

        // NOTE: This does NOT retrieve transforms for Critical Joints when Optimize Critical Joints is enabled
        protected Transform GetSkeletonTxByIndex(uint index)
        {
            if (_jointMonitor != null)
            {
                OvrAvatarLog.LogWarning(
                    "Optimize Critical Joints is enabled. GetSkeletonTxByIndex will always return null. Use GetSkeletonTransformByType instead",
                    logScope, this);
                return null;
            }

            OvrAvatarLog.Assert(index < _skeleton.Length);
            var tx = _skeleton[index].transform;
            OvrAvatarLog.Assert(tx != null, logScope, this);
            return tx;
        }

        public bool GetMonitoredPositionAndOrientation(CAPI.ovrAvatar2JointType jointType, out Vector3 position
            , out Quaternion orientation)
        {
            Debug.Assert(_jointMonitor != null);
            if (_jointMonitor != null &&
                _jointMonitor.TryGetPositionAndOrientation(jointType, out position, out orientation))
            {
                return true;
            }
            position = Vector3.zero;
            orientation = Quaternion.identity;
            return false;
        }

        /**
         * Get the current focal point that the avatar is looking at.
         * @returns 3D vector with gaze position.
         */
        public Vector3? GetGazePosition()
        {
            var gazePos = new CAPI.ovrAvatar2Vector3f();
            var result = CAPI.ovrAvatar2Behavior_GetGazePos(entityId, ref gazePos);
            if (!result.EnsureSuccess("ovrAvatar2Behavior_GetGazePos", logScope))
            {
                return null;
            }

#if OVR_AVATAR_DISABLE_CLIENT_XFORM
            return gazePos.ConvertSpace();
#else
            return gazePos;
#endif
        }

        /**
         * Determines if this avatar has one or more of the given features.
         *
         * The avatar features are specified when the avatar is created.
         * @param features set of features to check for.
         * @returns true if one of the specified features is set for this avatar.
         * @see CAPI.ovrAvatar2EntityFeatures
         * @see CreateEntity
         * @see CAPI.ovrAvatar2EntityCreateInfo
         */
        public bool HasAnyFeatures(CAPI.ovrAvatar2EntityFeatures features)
        {
            return (_creationInfo.features & features) != 0;
        }


        /**
         * Determines if this avatar has all of the given features.
         *
         * The avatar features are specified when the avatar is created.
         * @param features set of features to check for.
         * @returns true if all of the specified features are set for this avatar.
         * @see CAPI.ovrAvatar2EntityFeatures
         * @see CreateEntity
         * @see CAPI.ovrAvatar2EntityCreateInfo
         */
        public bool HasAllFeatures(CAPI.ovrAvatar2EntityFeatures features)
        {
            return (_creationInfo.features & features) == features;
        }

        private bool ForceEnableFeatures(CAPI.ovrAvatar2EntityFeatures features)
        {
            var newFeatures = _creationInfo.features | features;
            bool didAdd = newFeatures != _creationInfo.features;
            if (didAdd)
            {
                OvrAvatarLog.LogVerbose($"Force enabling features {features & ~_creationInfo.features}", logScope, this);

                _creationInfo.features = newFeatures;
            }
            return didAdd;
        }

        private bool ForceDisableFeatures(CAPI.ovrAvatar2EntityFeatures features)
        {
            var newFeatures = _creationInfo.features & ~features;
            bool didRemove = newFeatures != _creationInfo.features;
            if (didRemove)
            {
                OvrAvatarLog.LogVerbose($"Force disabling features {features & _creationInfo.features}", logScope, this);

                _creationInfo.features = newFeatures;
            }
            return didRemove;
        }

        #endregion

        /////////////////////////////////////////////////
        //:: Private Functions

        #region Private Helpers

        private bool QueryEntityPose(out CAPI.ovrAvatar2Pose entityPose, out CAPI.ovrAvatar2HierarchyVersion poseVersion)
        {
            var result = CAPI.ovrAvatar2Entity_GetPose(entityId, out entityPose, out poseVersion);
            if (!result.IsSuccess())
            {
                OvrAvatarLog.LogError($"Entity_GetPose {result}", logScope, this);
                return false;
            }

            return true;
        }

        private bool QueryEntityRenderState(out CAPI.ovrAvatar2EntityRenderState renderState)
        {
            if (!HasAnyFeatures(CAPI.ovrAvatar2EntityFeatures.Rendering_Prims))
            {
                renderState = default;
                return false;
            }

            var result = CAPI.ovrAvatar2Render_QueryRenderState(entityId, out renderState);
            if (!result.IsSuccess())
            {
                OvrAvatarLog.LogError($"QueryRenderState Error: {result}", logScope, this);
                return false;
            }

            return true;
        }

        private bool QueryPrimitiveRenderState(int index, out CAPI.ovrAvatar2PrimitiveRenderState renderState)
        {
            if (index < 0 || index >= primitiveRenderCount)
            {
                renderState = default;
                OvrAvatarLog.LogError(
                    $"IndexOutOfRange. Tried {index} when _primitiveRenderCount is {primitiveRenderCount}");
                return false;
            }

            return QueryPrimitiveRenderState_Direct(index, out renderState);
        }

        // No range checking, used while building primitives
        private bool QueryPrimitiveRenderState_Direct(int index, out CAPI.ovrAvatar2PrimitiveRenderState renderState)
        {
            if (index < 0)
            {
                OvrAvatarLog.LogError($"GetPrimitiveRenderStateByIndex Invalid Index: {index}", logScope, this);
                renderState = default;
                return false;
            }
            return QueryPrimitiveRenderState_Direct((uint)index, out renderState);
        }

        private bool QueryPrimitiveRenderState_Direct(uint index, out CAPI.ovrAvatar2PrimitiveRenderState renderState)
        {
            if (!HasAnyFeatures(CAPI.ovrAvatar2EntityFeatures.Rendering_Prims))
            {
                renderState = default;
                return false;
            }

            var result = CAPI.ovrAvatar2Render_GetPrimitiveRenderStateByIndex(entityId, (UInt32)index, out renderState);
            if (!result.EnsureSuccess("ovrAvatar2Render_GetPrimitiveRenderStateByIndex with index: " + index, logScope))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Tracking

        // The current body tracking behavior used by this avatar.
        public OvrAvatarInputManagerBehavior InputManager => _inputManager;

        private void UpdateInputProviders(OvrAvatarInputManagerBehavior inputManagerBehavior)
        {

            if (IsCreated && inputManagerBehavior != null)
            {
                SetInputControlProvider(inputManagerBehavior.InputControlProvider);
                SetInputTrackingProvider(inputManagerBehavior.InputTrackingProvider);
                SetHandTrackingProvider(inputManagerBehavior.HandTrackingProvider);
                SetBodyTrackingContext(inputManagerBehavior.BodyTrackingContext);
            }
        }

        /**
         * Select the input manager to use for this avatar.
         *
         * The input manager owns input providers
         * which implement the interface between the Avatar SDK
         * and the input provider implementation.
         *
         * @param inputManagerBehavior input manager implementation.
         * @see OvrAvatarInputManagerBehavior
         */
        public void SetInputManager(OvrAvatarInputManagerBehavior inputManagerBehavior, bool force = false)
        {
            if ((inputManagerBehavior == _inputManager) && !force)
            {
                return;
            }

            if (_inputManager != null)
            {
                _inputManager.OnBodyTrackingContextContextChanged.RemoveListener(UpdateInputProviders);
            }
            _inputManager = inputManagerBehavior;
            if (_inputManager != null)
            {
                _inputManager.OnBodyTrackingContextContextChanged.AddListener(UpdateInputProviders);
            }
            UpdateInputProviders(_inputManager);
        }

        [Obsolete("SetBodyTracking has been renamed to SetInputManager", false)]
        public void SetBodyTracking(OvrAvatarInputManagerBehavior inputManagerBehavior)
        {
            SetInputManager(inputManagerBehavior);
        }

        /**
         * Select the face pose to use for this avatar.
         */
        public void SetFacePoseProvider(OvrAvatarFacePoseBehavior facePoseBehavior)
        {
            _facePoseBehavior = facePoseBehavior;
            if (IsCreated)
            {
                SetFacePoseProvider(_facePoseBehavior?.FacePoseProvider);
            }
        }

        /**
         * Select the eye pose to use for this avatar.
         */
        public void SetEyePoseProvider(OvrAvatarEyePoseBehavior eyePoseBehavior)
        {
            _eyePoseBehavior = eyePoseBehavior;
            if (IsCreated)
            {
                SetEyePoseProvider(_eyePoseBehavior?.EyePoseProvider);
            }
        }

        /**
         * Select the lip sync behavior to use for this avatar.
         *
         * The input body tracker has a *LipSyncContext* member
         * which implements the interface between the Avatar SDK
         * and the lip tracking implementation.
         *
         * @param lipSyncBehavior lip sync implementation.
         * @see OvrAvatarLipSyncBehavior
         * @see OvrAvatarLipSyncContextBase
         */
        public void SetLipSync(OvrAvatarLipSyncBehavior lipSyncBehavior)
        {
            _lipSync = lipSyncBehavior;
            if (IsCreated)
            {
                SetLipSyncContext(_lipSync != null ? _lipSync.LipSyncContext : null);
            }
        }

        private void SetInputControlProvider(IOvrAvatarInputControlDelegate newDelegate)
        {
            // Special case to reduce overhead
            if (newDelegate is IOvrAvatarNativeInputControlDelegate nativeInputDelegate)
            {
                var nativeContext = nativeInputDelegate.NativeContext;
                CAPI.ovrAvatar2Input_SetInputControlContextNative(entityId, in nativeContext)
                    .EnsureSuccess("ovrAvatar2Input_SetInputControlContextNative", logScope, this);
            }
            else
            {
                var dataContext = newDelegate is OvrAvatarInputControlProviderBase provider
                    ? provider.Context
                    : new CAPI.ovrAvatar2InputControlContext();
                CAPI.ovrAvatar2Input_SetInputControlContext(entityId, in dataContext)
                    .EnsureSuccess("ovrAvatar2Input_SetInputControlContext", logScope);
            }
        }

        private void SetInputTrackingProvider(IOvrAvatarInputTrackingDelegate newDelegate)
        {
            // Special case to reduce overhead
            if (newDelegate is IOvrAvatarNativeInputDelegate nativeInputDelegate)
            {
                var nativeContext = nativeInputDelegate.NativeContext;
                CAPI.ovrAvatar2Input_SetInputTrackingContextNative(entityId, in nativeContext)
                    .EnsureSuccess("ovrAvatar2Input_SetInputTrackingContextNative", logScope, this);
            }
            else
            {
                var dataContext = newDelegate is OvrAvatarInputTrackingProviderBase provider
                    ? provider.Context
                    : new CAPI.ovrAvatar2InputTrackingContext();
                CAPI.ovrAvatar2Input_SetInputTrackingContext(entityId, in dataContext)
                    .EnsureSuccess("ovrAvatar2Input_SetInputTrackingContext", logScope);
            }
        }

        private void SetBodyTrackingContext(OvrAvatarBodyTrackingContextBase newContext)
        {
            // Special case to reduce overhead
            if (newContext is IOvrAvatarNativeBodyTracking bodyTracking)
            {
                var nativeContext = bodyTracking.NativeDataContext;
                CAPI.ovrAvatar2Tracking_SetBodyTrackingContextNative(entityId, in nativeContext)
                    .EnsureSuccess("ovrAvatar2Tracking_SetBodyTrackingContextNative", logScope, this);
            }
            else
            {
                var dataContext = newContext?.DataContext ?? new CAPI.ovrAvatar2TrackingDataContext();
                CAPI.ovrAvatar2Tracking_SetBodyTrackingContext(entityId, in dataContext)
                    .EnsureSuccess("ovrAvatar2Tracking_SetBodyTrackingContext", logScope, this);
            }
        }

        private void SetHandTrackingProvider(OvrAvatarHandTrackingPoseProviderBase newProvider)
        {
            // Special case to reduce overhead
            if (newProvider is IOvrAvatarNativeHandDelegate nativeHandDelegate)
            {
                var nativeContext = nativeHandDelegate.NativeContext;
                CAPI.ovrAvatar2Input_SetHandTrackingContextNative(entityId, in nativeContext)
                    .EnsureSuccess("ovrAvatar2Input_SetHandTrackingContextNative", logScope, this);
            }
            else
            {
                var dataContext = newProvider?.Context ?? new CAPI.ovrAvatar2HandTrackingDataContext();
                CAPI.ovrAvatar2Input_SetHandTrackingContext(entityId, in dataContext)
                    .EnsureSuccess("ovrAvatar2Input_SetHandTrackingContext", logScope);
            }
        }

        private void SetFacePoseProvider(OvrAvatarFacePoseProviderBase newProvider)
        {
            // Special case to reduce overhead
            if (newProvider is IOvrAvatarNativeFacePose facePose)
            {
                var nativeProvider = facePose.NativeProvider;
                CAPI.ovrAvatar2Input_SetFacePoseProviderNative(entityId, in nativeProvider)
                    .EnsureSuccess("ovrAvatar2Input_SetFacePoseProviderNative", logScope, this);
            }
            else
            {
                var dataContext = newProvider?.Provider ?? new CAPI.ovrAvatar2FacePoseProvider();
                CAPI.ovrAvatar2Input_SetFacePoseProvider(entityId, in dataContext)
                    .EnsureSuccess("ovrAvatar2Input_SetFacePoseProvider", logScope, this);
            }
        }

        private void SetEyePoseProvider(OvrAvatarEyePoseProviderBase newProvider)
        {
            // Special case to reduce overhead
            if (newProvider is IOvrAvatarNativeEyePose eyePose)
            {
                var nativeProvider = eyePose.NativeProvider;
                CAPI.ovrAvatar2Input_SetEyePoseProviderNative(entityId, in nativeProvider)
                    .EnsureSuccess("ovrAvatar2Input_SetEyePoseProviderNative", logScope, this);
            }
            else
            {
                var dataContext = newProvider?.Context ?? new CAPI.ovrAvatar2EyePoseProvider();
                CAPI.ovrAvatar2Input_SetEyePoseProvider(entityId, in dataContext)
                    .EnsureSuccess("ovrAvatar2Input_SetEyePoseProvider", logScope, this);
            }
        }

        private void SetLipSyncContext(OvrAvatarLipSyncContextBase newContext)
        {
            // Special case to reduce overhead
            if (newContext is OvrAvatarVisemeContext visemeContext)
            {
                var ncb = visemeContext.NativeCallbacks;
                CAPI.ovrAvatar2Tracking_SetLipSyncContextNative(entityId, in ncb)
                    .EnsureSuccess("ovrAvatar2Tracking_SetLipSyncContextNative", logScope, this);
            }
            else
            {
                var dataContext = newContext?.DataContext ?? new CAPI.ovrAvatar2LipSyncContext();
                CAPI.ovrAvatar2Tracking_SetLipSyncContext(entityId, in dataContext)
                    .EnsureSuccess("ovrAvatar2Tracking_SetLipSyncContext", logScope, this);
            }
        }

        #endregion
    }
}
