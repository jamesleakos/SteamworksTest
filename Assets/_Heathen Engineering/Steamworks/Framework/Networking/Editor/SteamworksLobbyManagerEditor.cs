#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Networking;
using Steamworks;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(SteamworksLobbyManager))]
    public class SteamworksLobbyManagerEditor : Editor
    {
        private SteamworksLobbyManager pManager;
        private SerializedProperty LobbySettings;
        private SerializedProperty OnKickedFromLobby;
        private SerializedProperty OnGameLobbyJoinRequest;
        private SerializedProperty OnLobbyMatchList;
        private SerializedProperty OnLobbyCreated;
        private SerializedProperty OnOwnershipChange;
        private SerializedProperty OnMemberJoined;
        private SerializedProperty OnMemberLeft;
        private SerializedProperty OnMemberDataChanged;
        private SerializedProperty OnLobbyEnter;
        private SerializedProperty OnLobbyExit;
        private SerializedProperty OnLobbyDataChanged;
        private SerializedProperty OnGameServerSet;
        private SerializedProperty OnLobbyChatUpdate;
        private SerializedProperty QuickMatchFailed;
        private SerializedProperty SearchStarted;
        private SerializedProperty OnChatMessageReceived;
        private SerializedProperty ChatMemberStateChangeEntered;
        private SerializedProperty ChatMemberStateChangeLeft;
        private SerializedProperty ChatMemberStateChangeDisconnected;
        private SerializedProperty ChatMemberStateChangeKicked;
        private SerializedProperty ChatMemberStateChangeBanned;

        private int tabPage = 0;

        private void OnEnable()
        {
            LobbySettings = serializedObject.FindProperty("LobbySettings");
        }

        private SerializedObject BuildReferences()
        {
            if (LobbySettings.objectReferenceValue == null)
                return null;

            var settingsObject = new SerializedObject(LobbySettings.objectReferenceValue);

            OnKickedFromLobby = serializedObject.FindProperty("OnKickedFromLobby");
            OnGameLobbyJoinRequest = serializedObject.FindProperty("OnGameLobbyJoinRequest");
            OnLobbyMatchList = serializedObject.FindProperty("OnLobbyMatchList");
            OnLobbyCreated = serializedObject.FindProperty("OnLobbyCreated");
            OnOwnershipChange = serializedObject.FindProperty("OnOwnershipChange");
            OnMemberJoined = serializedObject.FindProperty("OnMemberJoined");
            OnMemberLeft = serializedObject.FindProperty("OnMemberLeft");
            OnMemberDataChanged = serializedObject.FindProperty("OnMemberDataChanged");
            OnLobbyEnter = serializedObject.FindProperty("OnLobbyEnter");
            OnLobbyExit = serializedObject.FindProperty("OnLobbyExit");
            OnLobbyDataChanged = serializedObject.FindProperty("OnLobbyDataChanged");
            OnGameServerSet = serializedObject.FindProperty("OnGameServerSet");
            OnLobbyChatUpdate = serializedObject.FindProperty("OnLobbyChatUpdate");
            QuickMatchFailed = serializedObject.FindProperty("QuickMatchFailed");
            SearchStarted = serializedObject.FindProperty("SearchStarted");
            OnChatMessageReceived = serializedObject.FindProperty("OnChatMessageReceived");
            ChatMemberStateChangeEntered = serializedObject.FindProperty("ChatMemberStateChangeEntered");
            ChatMemberStateChangeLeft = serializedObject.FindProperty("ChatMemberStateChangeLeft");
            ChatMemberStateChangeDisconnected = serializedObject.FindProperty("ChatMemberStateChangeDisconnected");
            ChatMemberStateChangeKicked = serializedObject.FindProperty("ChatMemberStateChangeKicked");
            ChatMemberStateChangeBanned = serializedObject.FindProperty("ChatMemberStateChangeBanned");

            return settingsObject;
        }

        public override void OnInspectorGUI()
        {
            pManager = target as SteamworksLobbyManager;

            if (pManager.LobbySettings == null)
            {
                EditorGUILayout.HelpBox("You should provide a Lobby Settings object here for easier use with other components.", MessageType.Warning);
                EditorGUILayout.PropertyField(LobbySettings);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            var settingsObject = BuildReferences();

            Rect hRect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");

            
            

            Rect bRect = new Rect(hRect);
            bRect.width = hRect.width / 4f;
            tabPage = GUI.Toggle(bRect, tabPage == 0, "Settings", EditorStyles.toolbarButton) ? 0 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 1, "Common Events", EditorStyles.toolbarButton) ? 1 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 2, "Search Events", EditorStyles.toolbarButton) ? 2 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 3, "Chat Events", EditorStyles.toolbarButton) ? 3 : tabPage;
            EditorGUILayout.EndHorizontal();

            switch (tabPage)
            {
                case 0: DrawSettingsTab(); break;
                case 1: DrawCommonEventsTab(); break;
                case 2: DrawSearchEventsTab(); break;
                case 3: DrawChatEventsTab(); break;
                default: DrawSettingsTab(); break;
            }

            serializedObject.ApplyModifiedProperties();
            settingsObject.ApplyModifiedProperties();
        }

        private void DrawSettingsTab()
        {
            if (pManager.LobbySettings == null)
            {
                EditorGUILayout.HelpBox("You should provide a Lobby Settings object here for easier use with other components.", MessageType.Warning);
            }
            EditorGUILayout.PropertyField(LobbySettings);
        }

        private void DrawCommonEventsTab()
        {
            EditorGUILayout.PropertyField(OnLobbyCreated);
            EditorGUILayout.PropertyField(OnLobbyEnter);
            EditorGUILayout.PropertyField(OnLobbyExit);
            //EditorGUILayout.PropertyField(OnLobbyDataChanged);
            //EditorGUILayout.PropertyField(OnMemberJoined);
            //EditorGUILayout.PropertyField(OnMemberLeft);
            //EditorGUILayout.PropertyField(OnKickedFromLobby);
            //EditorGUILayout.PropertyField(OnMemberDataChanged);
            //EditorGUILayout.PropertyField(OnOwnershipChange);
            EditorGUILayout.PropertyField(OnGameServerSet);
        }

        private void DrawSearchEventsTab()
        {
            EditorGUILayout.PropertyField(SearchStarted);
            EditorGUILayout.PropertyField(OnLobbyMatchList);
            EditorGUILayout.PropertyField(QuickMatchFailed);
        }

        private void DrawChatEventsTab()
        {
            EditorGUILayout.PropertyField(OnLobbyChatUpdate);
            EditorGUILayout.PropertyField(ChatMemberStateChangeEntered);
            EditorGUILayout.PropertyField(ChatMemberStateChangeLeft);
            EditorGUILayout.PropertyField(ChatMemberStateChangeDisconnected);
            EditorGUILayout.PropertyField(ChatMemberStateChangeKicked);
            EditorGUILayout.PropertyField(ChatMemberStateChangeBanned);
            EditorGUILayout.PropertyField(OnChatMessageReceived);
        }
    }
}
#endif