#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.PlayerServices.UI;
using Steamworks;
using UnityEditor;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(SteamworksLeaderboardList))]
    public class HeathenSteamLeaderboardEditor : Editor
    {
        private SteamworksLeaderboardList board;

        public SerializedProperty Settings;
        public SerializedProperty entryPrototype;
        public SerializedProperty collection;
        public SerializedProperty focusPlayer;
        public SerializedProperty ignorePlayerRefresh;

        private void OnEnable()
        {
            Settings = serializedObject.FindProperty("Settings");
            entryPrototype = serializedObject.FindProperty("entryPrototype");
            collection = serializedObject.FindProperty("collection");
            focusPlayer = serializedObject.FindProperty("focusPlayer");
            ignorePlayerRefresh = serializedObject.FindProperty("ignorePlayerRefresh");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(Settings);
            serializedObject.ApplyModifiedProperties();

            board = target as SteamworksLeaderboardList;

            if (board.Settings == null)
                return;

            var b = EditorGUILayout.Toggle("Create Leaderboard if missing?", board.Settings.createIfMissing);
            if (b != board.Settings.createIfMissing)
            {
                board.Settings.createIfMissing = b;
                EditorUtility.SetDirty(board.Settings);
            }

            if (board.Settings.createIfMissing)
            {
                var v1 = (ELeaderboardSortMethod)EditorGUILayout.EnumPopup("Sort Method", board.Settings.sortMethod);
                if (v1 != board.Settings.sortMethod)
                {
                    board.Settings.sortMethod = v1;
                    EditorUtility.SetDirty(board.Settings);
                }

                var v2 = (ELeaderboardDisplayType)EditorGUILayout.EnumPopup("Display Type", board.Settings.displayType);
                if (v2 != board.Settings.displayType)
                {
                    board.Settings.displayType = v2;
                    EditorUtility.SetDirty(board.Settings);
                }
            }

            EditorGUILayout.PropertyField(focusPlayer);
            var n = EditorGUILayout.TextField("Name", board.Settings.leaderboardName);
            if (n != board.Settings.leaderboardName)
            {
                board.Settings.leaderboardName = n;
                EditorUtility.SetDirty(board.Settings);
            }
            
            EditorGUILayout.PropertyField(entryPrototype);
            EditorGUILayout.PropertyField(collection);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif