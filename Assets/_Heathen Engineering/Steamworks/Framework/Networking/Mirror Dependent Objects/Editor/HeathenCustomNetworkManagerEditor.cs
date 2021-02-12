#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using HeathenEngineering.SteamApi.Networking;
using Mirror;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(HeathenCustomNetworkManager), true)]
    [CanEditMultipleObjects]
    public class HeathenCustomNetworkManagerEditor : Editor
    {
        private SerializedProperty dontDestroyOnLoad;
        private SerializedProperty runInBackground;
        private SerializedProperty startOnHeadless;
        private SerializedProperty showDebugMessages;
        private SerializedProperty offlineScene;
        private SerializedProperty onlineScene;
        private SerializedProperty transport;
        private SerializedProperty playerPrefab;
        private SerializedProperty networkAddress;
        private SerializedProperty maxConnections;
        private SerializedProperty autoCreatePlayer;
        private SerializedProperty spawnListProperty;
        private SerializedProperty playerSpawnMethod;
        private SerializedProperty OnHostStarted;
        private SerializedProperty OnServerStarted;
        private SerializedProperty OnClientStarted;
        private SerializedProperty OnServerStoped;
        private SerializedProperty OnClientStoped;
        private SerializedProperty OnHostStoped;
        private SerializedProperty OnRegisterServerMessages;
        private SerializedProperty OnRegisterClientMessages;

        private ReorderableList spawnList;

        private int tabPage = 0;

        private HeathenCustomNetworkManager networkManager;

        protected void Init()
        {
            if (spawnList == null)
            {
                spawnListProperty = serializedObject.FindProperty("spawnPrefabs");

                spawnList = new ReorderableList(serializedObject, spawnListProperty);
                spawnList.drawHeaderCallback = DrawHeader;
                spawnList.drawElementCallback = DrawChild;
                spawnList.onReorderCallback = Changed;
                spawnList.onRemoveCallback = RemoveButton;
                spawnList.onChangedCallback = Changed;
                spawnList.onReorderCallback = Changed;
                spawnList.onAddCallback = AddButton;
                spawnList.elementHeight = 16; // this uses a 16x16 icon. other sizes make it stretch.
            }
        }

        private void OnEnable()
        {
            dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");
            runInBackground = serializedObject.FindProperty("runInBackground");
            showDebugMessages = serializedObject.FindProperty("showDebugMessages");
            startOnHeadless = serializedObject.FindProperty("startOnHeadless");
            if (startOnHeadless == null)
                startOnHeadless = serializedObject.FindProperty("autoStartServerBuild");
            offlineScene = serializedObject.FindProperty("offlineScene");
            onlineScene = serializedObject.FindProperty("onlineScene");
            transport = serializedObject.FindProperty("transport");
            networkAddress = serializedObject.FindProperty("networkAddress");
            maxConnections = serializedObject.FindProperty("maxConnections");
            playerPrefab = serializedObject.FindProperty("playerPrefab");
            autoCreatePlayer = serializedObject.FindProperty("autoCreatePlayer");
            spawnListProperty = serializedObject.FindProperty("spawnListProperty");
            playerSpawnMethod = serializedObject.FindProperty("playerSpawnMethod");
            OnHostStarted = serializedObject.FindProperty("OnHostStarted");
            OnServerStarted = serializedObject.FindProperty("OnServerStarted");
            OnClientStarted = serializedObject.FindProperty("OnClientStarted");
            OnServerStoped = serializedObject.FindProperty("OnServerStopped");
            OnClientStoped = serializedObject.FindProperty("OnClientStopped");
            OnHostStoped = serializedObject.FindProperty("OnHostStopped");
            OnRegisterServerMessages = serializedObject.FindProperty("OnRegisterServerMessages");
            OnRegisterClientMessages = serializedObject.FindProperty("OnRegisterClientMessages");
        }

        public override void OnInspectorGUI()
        {
            networkManager = target as HeathenCustomNetworkManager;

            Init();

            Rect hRect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");

            Rect bRect = new Rect(hRect);
            bRect.width = hRect.width / 3f;
            tabPage = GUI.Toggle(bRect, tabPage == 0, "Settings", EditorStyles.toolbarButton) ? 0 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 1, "Spawnables", EditorStyles.toolbarButton) ? 1 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 2, "Events", EditorStyles.toolbarButton) ? 2 : tabPage;
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            if (tabPage == 0)
            {
                NetworkSettings();
            }
            else if (tabPage == 1)
            {
                SpawnSettings();
            }
            else
            {
                Events();
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void NetworkSettings()
        {
            EditorGUILayout.PropertyField(dontDestroyOnLoad);
            EditorGUILayout.PropertyField(runInBackground);
            EditorGUILayout.PropertyField(startOnHeadless);
            EditorGUILayout.PropertyField(showDebugMessages);
            EditorGUILayout.PropertyField(offlineScene);
            EditorGUILayout.PropertyField(onlineScene);
            EditorGUILayout.PropertyField(transport);
            EditorGUILayout.PropertyField(networkAddress);
            EditorGUILayout.PropertyField(maxConnections);
            EditorGUILayout.PropertyField(playerPrefab);
            EditorGUILayout.PropertyField(autoCreatePlayer);
            EditorGUILayout.PropertyField(playerSpawnMethod);
        }

        private void SpawnSettings()
        {
            spawnList.DoLayoutList();
        }

        private void Events()
        {
            EditorGUILayout.PropertyField(OnHostStarted);
            EditorGUILayout.PropertyField(OnServerStarted);
            EditorGUILayout.PropertyField(OnClientStarted);
            EditorGUILayout.PropertyField(OnServerStoped);
            EditorGUILayout.PropertyField(OnClientStoped);
            EditorGUILayout.PropertyField(OnHostStoped);
            EditorGUILayout.PropertyField(OnRegisterServerMessages);
            EditorGUILayout.PropertyField(OnRegisterClientMessages);
        }

        static void DrawHeader(Rect headerRect)
        {
            GUI.Label(headerRect, "Registered Spawnable Prefabs:");
        }

        internal void DrawChild(Rect r, int index, bool isActive, bool isFocused)
        {
            SerializedProperty prefab = spawnListProperty.GetArrayElementAtIndex(index);
            GameObject go = (GameObject)prefab.objectReferenceValue;

            GUIContent label;
            if (go == null)
            {
                label = new GUIContent("Empty", "Drag a prefab with a NetworkIdentity here");
            }
            else
            {
                var identity = go.GetComponent<NetworkIdentity>();
                label = new GUIContent(go.name, identity != null ? "AssetId: [" + identity.assetId + "]" : "No Network Identity");
            }

            var newGameObject = (GameObject)EditorGUI.ObjectField(r, label, go, typeof(GameObject), false);

            if (newGameObject != go)
            {
                if (newGameObject != null && !newGameObject.GetComponent<NetworkIdentity>())
                {
                    Debug.LogError("Prefab " + newGameObject + " cannot be added as spawnable as it doesn't have a NetworkIdentity.");
                    return;
                }
                prefab.objectReferenceValue = newGameObject;
            }
        }

        internal void Changed(ReorderableList list)
        {
            EditorUtility.SetDirty(target);
        }

        internal void AddButton(ReorderableList list)
        {
            spawnListProperty.arraySize += 1;
            list.index = spawnListProperty.arraySize - 1;

            var obj = spawnListProperty.GetArrayElementAtIndex(spawnListProperty.arraySize - 1);
            obj.objectReferenceValue = null;

            spawnList.index = spawnList.count - 1;

            Changed(list);
        }

        internal void RemoveButton(ReorderableList list)
        {
            spawnListProperty.DeleteArrayElementAtIndex(spawnList.index);
            if (list.index >= spawnListProperty.arraySize)
            {
                list.index = spawnListProperty.arraySize - 1;
            }
        }
    }
}
#endif