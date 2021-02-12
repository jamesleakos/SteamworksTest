#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.PlayerServices.UI;
using UnityEditor;
using UnityEditor.UI;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(SteamDataFileRecord))]
    public class SteamDataFileRecordEditor : ButtonEditor
    {
        //private SteamDataFileRecord record;
        private SerializedProperty FileName;
        private SerializedProperty Timestamp;
        private SerializedProperty SelectedIndicator;

        protected override void OnEnable()
        {
            base.OnEnable();

            FileName = serializedObject.FindProperty("FileName");
            Timestamp = serializedObject.FindProperty("Timestamp");
            SelectedIndicator = serializedObject.FindProperty("SelectedIndicator");
        }

        public override void OnInspectorGUI()
        {
            //record = target as SteamDataFileRecord;

            EditorGUILayout.PropertyField(FileName);
            EditorGUILayout.PropertyField(Timestamp);
            EditorGUILayout.PropertyField(SelectedIndicator);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Button Settings", EditorStyles.boldLabel);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
#endif