using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BNG {

    [ExecuteInEditMode]
    public class HandPoser : MonoBehaviour {
        public bool ShowGizmos = true;

        [Tooltip("Path of the directory where handposes should be stored. Tip : Keep these in a 'Resources' directory so you can use Resources.Load().")]
        public string ResourcePath = "Assets/BNG Framework/HandPoser/Poses/Resources/";

        public string PoseName = "Default";

        public HandPose CurrentPose;

        public float AnimationSpeed = 15f;

        // Hand Pose Transform Definitions
        public HandPoseDefinition HandPoseJoints {
            get {
                return GetHandPoseDefinition();
            }
        }

        public Transform WristJoint;
        public Transform[] ThumbJoints;
        public Transform[] IndexJoints;
        public Transform[] MiddleJoints;
        public Transform[] RingJoints;
        public Transform[] PinkyJoints;
        public Transform[] OtherJoints;

        public string WristJointName;
        public string[] ThumbJointNames;
        public string[] IndexJointNames;
        public string[] MiddleJointNames;
        public string[] RingJointNames;
        public string[] PinkyJointNames;
        public string[] OtherJointNames;
        
        [HideInInspector]
        public HandPose PreviousPose;
        bool doSingleAnimation;

        // Continuously update pose state
        public bool ContinuousUpdate = false;

        void Start() {
            // Trigger a pose change to start the animation
            OnPoseChanged();
        }

        // This is also run in the editor
        void Update() {

            // Check for pose change event
            CheckForPoseChange();

            // Lerp to Hand Pose
            if (ContinuousUpdate || doSingleAnimation) {
                DoPoseUpdate();
            }
        }

        public void CheckForPoseChange() {
            if (PreviousPose == null || (CurrentPose != null && PreviousPose != null && PreviousPose.name != CurrentPose.name && CurrentPose != null)) {
                OnPoseChanged();
                PreviousPose = CurrentPose;
            }
        }

        public void OnPoseChanged() {

            // Allow pose to change animation
            editorAnimationTime = 0;
            doSingleAnimation = true;
        }

        public FingerJoint GetWristJoint() {
            return GetJointFromTransform(0, 0, WristJoint);
        }

        public FingerJoint[] GetThumbJoints() {
            return GetJointsFromTransforms(1, ThumbJoints);
        }

        public FingerJoint[] GetIndexJoints() {
            return GetJointsFromTransforms(2, IndexJoints);
        }

        public FingerJoint[] GetMiddleJoints() {
            return GetJointsFromTransforms(3, MiddleJoints);
        }

        public FingerJoint[] GetRingJoints() {
            return GetJointsFromTransforms(4, RingJoints);
        }

        public FingerJoint[] GetPinkyJoints() {
            return GetJointsFromTransforms(5, PinkyJoints);
        }

        public FingerJoint[] GetOtherJoints() {
            return GetJointsFromTransforms(6, OtherJoints);
        }

        public Transform GetTip(Transform[] transforms) {
            if(transforms != null) {
                return transforms[^1];
            }

            return null;
        }

        public Transform GetThumbTip() { return GetTip(ThumbJoints); }
        public Transform GetIndexTip() { return GetTip(IndexJoints); }
        public Transform GetMiddleTip() { return GetTip(MiddleJoints); }
        public Transform GetRingTip() { return GetTip(RingJoints); }
        public Transform GetPinkyTip() { return GetTip(PinkyJoints); }

        public static Vector3 GetFingerTipPositionWithOffset(Transform[] jointTransforms, float tipRadius) {

            if(jointTransforms == null || jointTransforms.Length == 0) {
                return Vector3.zero;
            }

            // Not available
            if(jointTransforms[^1] == null) {
                return Vector3.zero;
            }

            Vector3 tipPosition = jointTransforms[^1].position;

            if(jointTransforms.Length == 1) {
                return tipPosition;
            }

            return tipPosition + (jointTransforms[^2].position - tipPosition).normalized * tipRadius;
        }

        public virtual FingerJoint[] GetJointsFromTransforms(int finger, Transform[] jointTransforms) {
            var joints = new FingerJoint[jointTransforms.Length];
            
            // Add any joint information to the list
            for (int x = 0; x < joints.Length; x++) {
                joints[x] = GetJointFromTransform(finger, x, jointTransforms[x]);
            }

            return joints;
        }

        public virtual FingerJoint GetJointFromTransform(int finger, int idx, Transform jointTransform) {
            
            return new FingerJoint() {
                TransformName = GetName(finger, idx),
                LocalPosition = jointTransform.localPosition,
                LocalRotation = jointTransform.localRotation
            };
        }

        private string GetName(int finger, int idx)
        {
            switch (finger)
            {
                case 0:
                    return WristJointName;
                case 1:
                    return ThumbJointNames[idx];
                case 2:
                    return IndexJointNames[idx];
                case 3:
                    return MiddleJointNames[idx];
                case 4:
                    return RingJointNames[idx];
                case 5:
                    return PinkyJointNames[idx];
                case 6:
                    return OtherJointNames[idx];
                default:
                    return string.Empty;
            }
        }

        public virtual void UpdateHandPose(HandPoseDefinition pose, bool lerp) {
            UpdateJoint(pose.WristJoint, WristJoint, lerp);
            UpdateJoints(pose.ThumbJoints, ThumbJoints, lerp);
            UpdateJoints(pose.IndexJoints, IndexJoints, lerp);
            UpdateJoints(pose.MiddleJoints, MiddleJoints, lerp);
            UpdateJoints(pose.RingJoints, RingJoints, lerp);
            UpdateJoints(pose.PinkyJoints, PinkyJoints, lerp);
            UpdateJoints(pose.OtherJoints, OtherJoints, lerp);
        }

        public virtual void UpdateJoint(FingerJoint fromJoint, Transform toTransform, bool doLerp) {

            // Invalid joint
            if(toTransform == null) {
                return;
            }

            if (doLerp) {
                var clampLerp = Mathf.Clamp(Time.deltaTime * AnimationSpeed, 0, 1);
                toTransform.localPosition = Vector3.Lerp(toTransform.localPosition, fromJoint.LocalPosition, clampLerp);
                toTransform.localRotation = Quaternion.Lerp(toTransform.localRotation, fromJoint.LocalRotation, clampLerp);
            }
            else {
                toTransform.localPosition = fromJoint.LocalPosition;
                toTransform.localRotation = fromJoint.LocalRotation;
            }
        }

        public virtual void UpdateJoints(FingerJoint[] joints, Transform[] toTransforms, bool doLerp) {

            // Sanity check
            if(joints == null || toTransforms == null) {
                return;
            }

            // Cache the count of our lists
            int jointCount = joints.Length;
            int transformsCount = toTransforms.Length;

            // If our joint counts don't add up, then make sure the names match before applying any changes
            // Otherwise there is a good chance the wrong transforms are being updated
            // bool verifyTransformName = jointCount != transformsCount;
            for (int x = 0; x < jointCount; x++) {
                // Make sure the indexes match
                if(x < toTransforms.Length) {

                    // Joint may have not been assigned or destroyed
                    if(toTransforms[x] == null) {
                        continue;
                    }

                    UpdateJoint(joints[x], toTransforms[x], doLerp);
                    
                    // if(verifyTransformName && joints[x].TransformName == toTransforms[x].name) {
                    //     UpdateJoint(joints[x], toTransforms[x], doLerp);
                    // }
                    // else if(verifyTransformName == false) {
                    //     UpdateJoint(joints[x], toTransforms[x], doLerp);
                    // }
                }
            }
        }

        public virtual HandPoseDefinition GetHandPoseDefinition() {
            return new HandPoseDefinition() {
                WristJoint = GetWristJoint(),
                ThumbJoints = GetThumbJoints(),
                IndexJoints = GetIndexJoints(),
                MiddleJoints = GetMiddleJoints(),
                RingJoints = GetRingJoints(),
                PinkyJoints = GetPinkyJoints(),
                OtherJoints = GetOtherJoints()
            };
        }

        /// <summary>
        /// Saves the current hand configuration as a Unity Scriptable object 
        /// Pose will be saved to the provided 'HandPosePath' (Typically within in a Resources folder)
        /// *Scriptable Objects can only be saved from within the UnityEditor.
        /// </summary>
        /// <param name="poseName"></param>
        /// <returns></returns>
        public virtual void SavePoseAsScriptablObject(string poseName) {
#if UNITY_EDITOR
            
            string fileName = poseName + ".asset";

            var poseObject = GetHandPoseScriptableObject();

            // Creates the file in the folder path
            //string fullPath = Application.dataPath + directory + fileName;
            string fullPath = ResourcePath + fileName;            
            bool exists = System.IO.File.Exists(fullPath);

            // Delete if asset already exists
            if (exists) {
                UnityEditor.AssetDatabase.DeleteAsset(fullPath);
            }

            UnityEditor.AssetDatabase.CreateAsset(poseObject, fullPath);

            UnityEditor.AssetDatabase.SaveAssets();

            UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceUpdate);

            UnityEditor.EditorUtility.SetDirty(poseObject);

            if (exists) {
                Debug.Log("Updated Hand Pose : " + poseName);
            }
            else {
                Debug.Log("Created new Hand Pose : " + poseName);
            }
#else
    Debug.Log("Scriptable Objects can only be saved from within the Unity Editor. Consider storing in another format like JSON instead.");
#endif
        }

        /// <summary>
        /// Will create an original handpose with the specified PoseName. If the provided poseName is already taken, a number will be added to the name until a unique name is found.
        /// For example, if you provide the PoseName of "Default" and there are already two poses named "Default" and "Default 1", then a new pose named "Default 2" will be created.
        /// </summary>
        /// <param name="poseName"></param>
        public virtual void CreateUniquePose(string poseName) {

            // Don't allow empty pose names
            if(string.IsNullOrEmpty(poseName)) {
                poseName = "Pose";
            }

            string formattedPoseName = poseName;
            string fullPath = ResourcePath + formattedPoseName + ".asset";
            bool exists = System.IO.File.Exists(fullPath);
            int checkCount = 0;
            // Find a path that doesn't exist
            while(exists) {
                // Ex : "path/Pose 5.asset"
                formattedPoseName = poseName + " " + checkCount;
                exists = System.IO.File.Exists(ResourcePath + formattedPoseName + ".asset");
                
                checkCount++;
            }

            // Save the new pose
            SavePoseAsScriptablObject(formattedPoseName);
        }

        public virtual HandPose GetHandPoseScriptableObject() {
#if UNITY_EDITOR
            var poseObject = UnityEditor.Editor.CreateInstance<HandPose>();
            poseObject.PoseName = PoseName;

            poseObject.Joints = new HandPoseDefinition();
            poseObject.Joints.WristJoint = GetWristJoint();
            poseObject.Joints.ThumbJoints = GetThumbJoints();
            poseObject.Joints.IndexJoints = GetIndexJoints();
            poseObject.Joints.MiddleJoints = GetMiddleJoints();
            poseObject.Joints.RingJoints = GetRingJoints();
            poseObject.Joints.PinkyJoints = GetPinkyJoints();
            poseObject.Joints.OtherJoints = GetOtherJoints();

            return poseObject;
#else
    return null;
#endif
        }


        // How long to check for animations while in the editor mode
        float editorAnimationTime = 0f;
        float maxEditorAnimationTime = 2f;

        public virtual void DoPoseUpdate() {

            if (CurrentPose != null) {
                UpdateHandPose(CurrentPose.Joints, true);
            }

            // Are we done requesting a single animation?
            if (doSingleAnimation) {
                editorAnimationTime += Time.deltaTime;

                // Reset
                if (editorAnimationTime >= maxEditorAnimationTime) {
                    editorAnimationTime = 0;
                    doSingleAnimation = false;
                }
            }
        }

        public virtual void ResetEditorHandles() {
            EditorHandle[] handles = GetComponentsInChildren<EditorHandle>();
            for (int x = 0; x < handles.Length; x++) {
                if(handles[x] != null && handles[x].gameObject != null) {
                    GameObject.DestroyImmediate((handles[x]));
                }
            }
        }

        void OnDrawGizmos() {
#if UNITY_EDITOR
            // Update every frame even while in editor
            if (!Application.isPlaying) {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
#endif
        }
    }
}

