using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ElementCreate), true)]
[CanEditMultipleObjects]
public class ElementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        serializedObject.Update();
        if (GUILayout.Button("Clear All AssetBundle"))
        {
            (target as ElementCreate).ClearAllAssetBundle();
        }
        if (GUILayout.Button("Create"))
        {
            (target as ElementCreate).Create();
        }
        if (GUILayout.Button("Clone"))
        {
            (target as ElementCreate).Clone();
        }
        if (GUILayout.Button("CloneAll"))
        {
            (target as ElementCreate).CloneAll();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
