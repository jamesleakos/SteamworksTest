#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.PlayerServices;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomPropertyDrawer(typeof(InventoryItemDefinitionCount))]
    public class ExchangeItemCountDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            //var indent = EditorGUI.indentLevel;
            //EditorGUI.indentLevel = 0;



            // Calculate rects
            Rect unitRect = new Rect(position.x, position.y, 90, position.height);
            Rect amountRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("Item"), GUIContent.none);
            EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("Count"), GUIContent.none);

            // Set indent back to what it was
            //EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
#endif