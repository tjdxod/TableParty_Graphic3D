#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using BNG;
using Dive.VRModule;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class AutoHandEditor : OdinEditorWindow
{
    private static AutoPoseData autoPoseData;

    public static void ShowWindow()
    {
        autoPoseData = FindObjectOfType<AutoPoseData>();

        if (autoPoseData == null)
        {
            EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
            return;
        }

        var tuple = autoPoseData.CheckInit();

        AutoHandEditor window = GetWindow<AutoHandEditor>();
        window.titleContent = new GUIContent("AutoPose");
        window.Show();

        if (!tuple.Item1)
            return;

        var assetObject = AssetDatabase.LoadAssetAtPath<GameObject>(tuple.Item2);
        window.model = assetObject;
    }

    [EnumToggleButtons, BoxGroup("Settings"), OnValueChanged(nameof(OnColorChange))]
    public AutoPoseData.ColorMode colorMode = AutoPoseData.ColorMode.Ghost;

    [HorizontalGroup("data", 125)]
    [PreviewField(125)]
    [AssetsOnly]
    [ShowInInspector]
    private GameObject model;

    [HorizontalGroup("data/info")]
    [VerticalGroup("data/info/left")]
    [ShowInInspector]
    [LabelText("모델 이름")]
    private string modelName => model != null ? model.name : "모델 없음";

    [HorizontalGroup("data/info")]
    [VerticalGroup("data/info/left")]
    [ShowInInspector]
    [LabelText("모델 크기")]
    private string modelSize => GetModelSize();

    [PropertySpace(20)]
    [HorizontalGroup("data/info")]
    [VerticalGroup("data/info/left")]
    [Button("모델 생성", ButtonSizes.Large)]
    [EnableIf("model")]
    public void CreateModel()
    {
        if (model == null)
        {
            EditorUtility.DisplayDialog("Warning", "모델이 없습니다.", "OK");
            return;
        }

        if (autoPoseData == null)
        {
            EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
            return;
        }

        autoPoseData.CreateModel(model);
    }

    [HorizontalGroup("data/info")]
    [VerticalGroup("data/info/left")]
    [Button("모델 삭제", ButtonSizes.Large)]
    [EnableIf("model")]
    public void DeleteModel()
    {
        if (autoPoseData == null)
        {
            EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
            return;
        }

        model = null;
        autoPoseData.DeleteModel();
    }

    [BoxGroup("Pose")]
    [Button("현재 포즈 저장", ButtonSizes.Large)]
    [EnableIf("model")]
    public void SavePose()
    {
        if (autoPoseData == null)
        {
            EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
            return;
        }

        autoPoseData.SavePose(posePath, meshPath, prefabPath);
    }

    [VerticalGroup("Pose/Pose")]
    [LabelText("오토포즈 활성화"), OnValueChanged(nameof(UseAutoHand))]
    public bool useAutoPose = true;

    [VerticalGroup("Pose/Pose")]
    [LabelText("테스트용 포즈")]
    public HandPose pose;

    [VerticalGroup("Pose/PoseButton")]
    [Button("테스트 포즈 적용", ButtonSizes.Large)]
    [EnableIf("pose")]
    public void SetPose()
    {
        if (autoPoseData == null)
        {
            EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
            return;
        }

        if (pose == null)
        {
            EditorUtility.DisplayDialog("Warning", "포즈가 없습니다.", "OK");
            return;
        }

        autoPoseData.SetPose(pose);
    }

    [VerticalGroup("Pose/PoseButton")]
    [Button("초기 포즈로", ButtonSizes.Large)]
    public void ResetPose()
    {
        if (autoPoseData == null)
        {
            EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
            return;
        }

        autoPoseData.ResetPose();
    }

    [VerticalGroup("Pose/Pose")]
    [LabelText("포즈 저장 경로")]
    public string posePath = "Assets/Utility/PoseModule/SavedData_Legacy/Poses";

    [VerticalGroup("Pose/Pose")]
    [LabelText("메쉬 저장 경로")]
    public string meshPath = "Assets/Utility/PoseModule/SavedData_Legacy/Meshes";

    [VerticalGroup("Pose/Pose")]
    [LabelText("메쉬 저장 경로")]
    public string prefabPath = "Assets/Utility/PoseModule/SavedData_Legacy/Prefabs";


    private void UseAutoHand()
    {
        if (autoPoseData == null)
        {
            EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
            return;
        }

        autoPoseData.UseAutoHand(useAutoPose);
    }

    private void OnColorChange()
    {
        autoPoseData.SetColorMaterial(colorMode);

        // SceneView 포커스
        SceneView sceneView = SceneView.lastActiveSceneView;

        if (sceneView != null)
        {
            sceneView.Focus();
        }
    }

    private string GetModelSize()
    {
        if (model == null)
        {
            return "모델 없음";
        }

        var renderers = model.GetComponentsInChildren<Renderer>();

        if (renderers == null)
        {
            return "모델 없음";
        }

        if (renderers.Length > 1)
        {
            return "여러 렌더러";
        }

        Vector3 size = renderers[0].bounds.size;
        var scale = model.transform.localScale;

        return $"X: {size.x * scale.x}, Y: {size.y * scale.y}, Z: {size.z * scale.z}";
    }

    [ButtonGroup("Pose/PoseButton/Focus")]
    [Button("포즈 폴더 포커스", ButtonSizes.Large)]
    public void FocusPoseFolder()
    {
        if (autoPoseData == null)
        {
            EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
            return;
        }

        var path = posePath;

        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Warning", "포즈 폴더 경로가 없습니다.", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            EditorUtility.DisplayDialog("Warning", "포즈 폴더가 없습니다.", "OK");
            return;
        }

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
    }

    [ButtonGroup("Pose/PoseButton/Focus")]
    [Button("메쉬 폴더 포커스", ButtonSizes.Large)]
    public void FocusMeshFolder()
    {
        if (autoPoseData == null)
        {
            EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
            return;
        }

        var path = meshPath;

        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Warning", "메쉬 폴더 경로가 없습니다.", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            EditorUtility.DisplayDialog("Warning", "메쉬 폴더가 없습니다.", "OK");
            return;
        }

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
    }

    [ButtonGroup("Pose/PoseButton/Focus")]
    [Button("프리펩 폴더 포커스", ButtonSizes.Large)]
    public void FocusPrefabFolder()
    {
        if (autoPoseData == null)
        {
            EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
            return;
        }

        var path = prefabPath;

        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Warning", "프리펩 폴더 경로가 없습니다.", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            EditorUtility.DisplayDialog("Warning", "프리펩 폴더가 없습니다.", "OK");
            return;
        }

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
    }
}

public class HandTrackingEditor : OdinMenuEditorWindow
{
    public static string DefaultPosePath = @"Assets/Utility/PoseModule/SavedData/Poses";
    public static string DefaultMeshPath = @"Assets/Utility/PoseModule/SavedData/Meshes";
    public static string DefaultPrefabPath = @"Assets/Utility/PoseModule/SavedData/Prefabs";

    public static string LastPosePath = DefaultPosePath;
    public static string LastMeshPath = DefaultMeshPath;
    public static string LastPrefabPath = DefaultPrefabPath;

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();

        var createPose = new CreatePose(this);

        tree.Add("Create Pose", createPose);
        tree.AddAllAssetsAtPath("Saved Pose", LastPosePath, typeof(PXRPose), true);

        return tree;
    }

    public static void ShowWindow()
    {
        HandTrackingEditor window = GetWindow<HandTrackingEditor>();
        window.titleContent = new GUIContent("HandPose");
        window.position = new Rect(300, 300, 800, 600);
        window.Show();
    }

    public static void CloseWindow()
    {
        var window = GetWindow<HandTrackingEditor>();
        window.Close();
    }

    public class CreatePose
    {
        private HandTrackingEditor editor;
        private static HandPoseData handPoseData;

        public CreatePose(HandTrackingEditor editor)
        {
            this.editor = editor;
            handPoseData = FindObjectOfType<HandPoseData>();

            if (handPoseData == null)
            {
                EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
                return;
            }

            var tuple = handPoseData.CheckInit();

            if (!tuple.Item1)
                return;

            var assetObject = AssetDatabase.LoadAssetAtPath<GameObject>(tuple.Item2);
            model = assetObject;
        }


        [EnumToggleButtons, BoxGroup("Settings"), OnValueChanged(nameof(OnColorChange))]
        public HandPoseData.ColorMode colorMode = HandPoseData.ColorMode.Ghost;

        [EnumToggleButtons, BoxGroup("Settings")]
        public HandPoseData.HandMode handMode = HandPoseData.HandMode.Right;

        [HorizontalGroup("data", 125)]
        [PreviewField(125)]
        [AssetsOnly]
        [ShowInInspector]
        private GameObject model;

        [HorizontalGroup("data/info")]
        [VerticalGroup("data/info/left")]
        [ShowInInspector]
        [LabelText("모델 이름")]
        private string modelName => model != null ? model.name : "모델 없음";

        [PropertySpace(20)]
        [HorizontalGroup("data/info")]
        [VerticalGroup("data/info/left")]
        [Button("모델 생성", ButtonSizes.Large)]
        [EnableIf("model")]
        public void CreateModel()
        {
            if (model == null)
            {
                EditorUtility.DisplayDialog("Warning", "모델이 없습니다.", "OK");
                return;
            }

            if (handPoseData == null)
            {
                EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
                return;
            }

            handPoseData.CreateModel(model);
        }

        [HorizontalGroup("data/info")]
        [VerticalGroup("data/info/left")]
        [Button("모델 삭제", ButtonSizes.Large)]
        [EnableIf("model")]
        public void DeleteModel()
        {
            if (handPoseData == null)
            {
                EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
                return;
            }

            model = null;
            handPoseData.DeleteModel();
        }

        [VerticalGroup("Pose/Pose")]
        [LabelText("저장 지연 시간")]
        [HideIf("useMultiplePhoto")]
        [PropertyRange(0, 15)]
        public float delay = 5;

        [HorizontalGroup("Pose/Pose/MultiplePhoto")]
        [ShowIf("useMultiplePhoto")]
        [LabelText("촬영 간격"), PropertyRange(2, 5)]
        public float interval = 3;

        [VerticalGroup("Pose/Pose")]
        [ShowIf("useMultiplePhoto")]
        [LabelText("촬영 횟수")]
        public int photoCount = 5;

        [VerticalGroup("Pose/Pose")]
        [LabelText("연속 촬영 모드")]
        public bool useMultiplePhoto = false;

        [VerticalGroup("Pose/Pose")]
        [LabelText("테스트용 포즈")]
        public PXRPose pose;

        [VerticalGroup("Pose/Pose/PoseSave")]
        [ShowInInspector]
        [LabelText("포즈 저장 경로"), ReadOnly]
        public string posePath => HandTrackingEditor.LastPosePath;

        [VerticalGroup("Pose/Pose/PoseSave")]
        [Button("경로 변경", Stretch = false, ButtonAlignment = 1)]
        public void ChangePosePath()
        {
            var path = EditorUtility.OpenFolderPanel("포즈 저장 경로", posePath, "");

            // Assets부터 시작하도록 자르기
            HandTrackingEditor.LastPosePath = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal));
            editor.ForceMenuTreeRebuild();
        }

        [VerticalGroup("Pose/Pose/MeshSave")]
        [ShowInInspector]
        [LabelText("메쉬 저장 경로"), ReadOnly]
        public string meshPath => HandTrackingEditor.LastMeshPath;

        [VerticalGroup("Pose/Pose/MeshSave")]
        [Button("경로 변경", Stretch = false, ButtonAlignment = 1)]
        public void ChangeMeshPath()
        {
            var path = EditorUtility.OpenFolderPanel("메쉬 저장 경로", meshPath, "");

            HandTrackingEditor.LastMeshPath = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal));
            editor.ForceMenuTreeRebuild();
        }

        [VerticalGroup("Pose/Pose/PrefabSave")]
        [ShowInInspector]
        [LabelText("프리펩 저장 경로"), ReadOnly]
        public string prefabPath => HandTrackingEditor.LastPrefabPath;

        [VerticalGroup("Pose/Pose/PrefabSave")]
        [Button("경로 변경", Stretch = false, ButtonAlignment = 1)]
        public void ChangePrefabPath()
        {
            var path = EditorUtility.OpenFolderPanel("프리펩 저장 경로", prefabPath, "");

            HandTrackingEditor.LastPrefabPath = path.Substring(path.IndexOf("Assets", StringComparison.Ordinal));
            editor.ForceMenuTreeRebuild();
        }

        [VerticalGroup("Pose/Pose")]
        [SerializeField]
        [LabelText("생성 후 컴바인 메쉬 보여주기")]
        public bool afterCreated = true;

        [VerticalGroup("Pose/Pose")]
        [SerializeField]
        [LabelText("물건없이 포즈 잡기 허용")]
        public bool noObjectPose = false;

        [VerticalGroup("Pose/Pose")]
        [SerializeField]
        [LabelText("포즈 이름 설정")]
        [ShowIf("noObjectPose")]
        public string poseName;

        private bool enableSave => model != null || noObjectPose;

        [BoxGroup("Pose")]
        [Button("현재 포즈 저장", ButtonSizes.Large)]
        [EnableIf("enableSave")]
        public void SavePose()
        {
            if (handPoseData == null)
            {
                EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
                return;
            }

            var variable = new PoseVariable();
            variable.posePath = posePath;
            variable.meshPath = meshPath;
            variable.prefabPath = prefabPath;
            variable.afterCreated = afterCreated;
            variable.noObjectPose = noObjectPose;
            variable.poseName = poseName;
            variable.useMultiplePhoto = useMultiplePhoto;
            variable.interval = interval;
            variable.photoCount = photoCount;
            variable.duration = delay;
            variable.HandMode = handMode;
            
            handPoseData.SavePose(variable);
        }

        [VerticalGroup("Pose/PoseButton")]
        [Button("테스트 포즈 적용", ButtonSizes.Large)]
        [EnableIf("pose")]
        public void SetPose()
        {
            if (handPoseData == null)
            {
                EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
                return;
            }

            if (pose == null)
            {
                EditorUtility.DisplayDialog("Warning", "포즈가 없습니다.", "OK");
                return;
            }

            handPoseData.SetTestPose(pose, handMode);
        }

        [VerticalGroup("Pose/PoseButton")]
        [Button("초기 포즈로", ButtonSizes.Large)]
        public void ResetPose()
        {
            if (handPoseData == null)
            {
                EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
                return;
            }

            handPoseData.ResetPose();
        }

        [ButtonGroup("Pose/PoseButton/Focus")]
        [Button("포즈 폴더 포커스", ButtonSizes.Large)]
        public void FocusPoseFolder()
        {
            if (handPoseData == null)
            {
                EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
                return;
            }

            var path = posePath;

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Warning", "포즈 폴더 경로가 없습니다.", "OK");
                return;
            }

            if (!AssetDatabase.IsValidFolder(path))
            {
                EditorUtility.DisplayDialog("Warning", "포즈 폴더가 없습니다.", "OK");
                return;
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        [ButtonGroup("Pose/PoseButton/Focus")]
        [Button("메쉬 폴더 포커스", ButtonSizes.Large)]
        public void FocusMeshFolder()
        {
            if (handPoseData == null)
            {
                EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
                return;
            }

            var path = meshPath;

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Warning", "메쉬 폴더 경로가 없습니다.", "OK");
                return;
            }

            if (!AssetDatabase.IsValidFolder(path))
            {
                EditorUtility.DisplayDialog("Warning", "메쉬 폴더가 없습니다.", "OK");
                return;
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        [ButtonGroup("Pose/PoseButton/Focus")]
        [Button("프리펩 폴더 포커스", ButtonSizes.Large)]
        public void FocusPrefabFolder()
        {
            if (handPoseData == null)
            {
                EditorUtility.DisplayDialog("Warning", "PoseData를 찾을 수 없습니다.", "OK");
                return;
            }

            var path = prefabPath;

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Warning", "프리펩 폴더 경로가 없습니다.", "OK");
                return;
            }

            if (!AssetDatabase.IsValidFolder(path))
            {
                EditorUtility.DisplayDialog("Warning", "프리펩 폴더가 없습니다.", "OK");
                return;
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        private void OnColorChange()
        {
            handPoseData.SetColorMaterial(colorMode);

            // SceneView 포커스
            SceneView sceneView = SceneView.lastActiveSceneView;

            if (sceneView != null)
            {
                sceneView.Focus();
            }
        }
    }
}

public class PoseInit
{
    [MenuItem("Tools/손 포즈 잡기 (AutoPose)")]
    public static void MoveAutoPoseScene()
    {
        // 현재씬이 poseScene인 경우 에디터만 열기
        if (SceneManager.GetActiveScene().name == "PoseScene_AutoPose")
        {
            AutoHandEditor.ShowWindow();
            return;
        }

        // 현재 씬에 변경사항이 있는지 확인
        if (SceneManager.GetActiveScene().isDirty)
        {
            // 씬을 저장하지 않고 종료할 수 없도록 경고 메시지 표시
            EditorUtility.DisplayDialog("Warning", "씬을 저장하지 않고 종료할 수 없습니다.", "OK");
            return;
        }

        // PoseScene 씬을 로드
        Scene poseScene = EditorSceneManager.OpenScene($"Assets/Utility/PoseModule/PoseScene_AutoPose.unity", OpenSceneMode.Single);

        // 씬을 로드한 후, PoseSettings 창을 열기
        AutoHandEditor.ShowWindow();
    }

    [MenuItem("Tools/손 포즈 잡기 (HandTracking)")]
    public static void MoveHandTrackingPoseScene()
    {
        // 현재씬이 poseScene인 경우 에디터만 열기
        if (SceneManager.GetActiveScene().name == "PoseScene_HandTracking")
        {
            HandTrackingEditor.ShowWindow();
            return;
        }

        // 현재 씬에 변경사항이 있는지 확인
        if (SceneManager.GetActiveScene().isDirty)
        {
            // 씬을 저장하지 않고 종료할 수 없도록 경고 메시지 표시
            EditorUtility.DisplayDialog("Warning", "씬을 저장하지 않고 종료할 수 없습니다.", "OK");
            return;
        }

        // PoseScene 씬을 로드
        Scene poseScene = EditorSceneManager.OpenScene($"Assets/Utility/PoseModule/PoseScene_HandTracking.unity", OpenSceneMode.Single);

        // 씬을 로드한 후, PoseSettings 창을 열기
        // HandTrackingSettings.ShowWindow();
    }
}


#endif