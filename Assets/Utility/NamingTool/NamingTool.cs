#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class NamingTool : EditorWindow
{
    private bool _bEditOn = true;

    [SerializeField]
    public List<GameObject> ins_GameObjects = new List<GameObject>();

    [MenuItem("Tools/������Ʈ �̸��ٲٱ�")]
    private static void Open()
    {
        NamingTool win = GetWindow<NamingTool>();
        win.titleContent = new GUIContent("Naming Tool");
        win.Show();
    }
    private void OnEnable()
    {
        _bEditOn = true;
    }
    private void OnDisable()
    {
        _bEditOn = false;
        _nCount = 0;
    }

    private Editor _editor;
    private string _strName = null;
    private int _nCount = 0;

    private void OnGUI()
    {
        if (!_editor) { _editor = Editor.CreateEditor(this); }
        if (_editor) { _editor.OnInspectorGUI(); }

        GUILayout.BeginVertical("BOX");

        GUILayout.Space(10);
        GUILayout.Label("���� �� �̸�", GUILayout.Width(75));
        _strName = EditorGUILayout.TextField(_strName, GUILayout.ExpandWidth(true));
        GUILayout.Label("���� �ε���(-1�̸� �ε����� �Ⱥ���)", GUILayout.ExpandWidth(true));
        _nCount = EditorGUILayout.IntField(_nCount, GUILayout.ExpandWidth(true));
        GUILayout.Space(10);

        GUILayout.EndHorizontal();

        if (GUILayout.Button("����"))
        {
            int nIdx = _nCount;
            string strName = _strName;
            for (int i = 0; i < ins_GameObjects.Count; i++)
            {
                strName = _strName;
                if (_nCount != -1)
                {
                    strName += nIdx;
                    nIdx++;
                }
                ins_GameObjects[i].name = strName;
            }

            if (ins_GameObjects.Count == 0)
            {
                Debug.LogError("����Ʈ�� �������");
            }
            else
            {
                Debug.LogError("���� �Ϸ�");
            }
        }
    }
}

[CustomEditor(typeof(NamingTool), true)]
public class NamingToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var list = serializedObject.FindProperty("ins_GameObjects");
        EditorGUILayout.PropertyField(list, new GUIContent("������Ʈ ����Ʈ"), true);

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("�ʱ�ȭ"))
        {
            list.arraySize = 0;
        }
    }
}

#endif