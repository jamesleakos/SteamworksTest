using UnityEngine;
using UnityEditor;
using HeathenEngineering.UIX;
using UnityEditor.UI;

[CanEditMultipleObjects, CustomEditor(typeof(RayCatcher), false)]
public class RayCatcherEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Script, new GUILayoutOption[0]);
        RaycastControlsGUI();
        serializedObject.ApplyModifiedProperties();
    }
}

