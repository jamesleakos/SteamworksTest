using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.UIX
{
    [CustomPropertyDrawer(typeof(SortingLayerValue))]
    public class SortingLayerValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIContent[] layerNames = new GUIContent[SortingLayer.layers.Length];
            int currentIndex = -1;
            string currentName = property.FindPropertyRelative("name").stringValue;
            for (int i = 0; i < SortingLayer.layers.Length; i++)
            {
                layerNames[i] = new GUIContent(SortingLayer.layers[i].name);
                if (layerNames[i].text == currentName)
                    currentIndex = i;
            }

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();
            if (currentIndex < 0)
            {
                currentIndex = 0;
                property.FindPropertyRelative("id").intValue = SortingLayer.layers[0].id;
                property.FindPropertyRelative("name").stringValue = SortingLayer.layers[0].name;
                property.FindPropertyRelative("value").intValue = SortingLayer.layers[0].value;
            }

            currentIndex = EditorGUI.Popup(position, label, currentIndex, layerNames);
            if (currentIndex >= 0)
            {
                property.FindPropertyRelative("id").intValue = SortingLayer.layers[currentIndex].id;
                property.FindPropertyRelative("name").stringValue = SortingLayer.layers[currentIndex].name;
                property.FindPropertyRelative("value").intValue = SortingLayer.layers[currentIndex].value;
            }

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }
    }
}

