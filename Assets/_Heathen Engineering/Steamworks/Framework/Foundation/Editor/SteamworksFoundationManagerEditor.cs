#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(SteamworksFoundationManager))]
    public class SteamworksFoundationManagerEditor : Editor
    {
        private SteamworksFoundationManager pManager;
        private SerializedProperty Settings;
        
        private SerializedProperty DoNotDestroyOnLoad;
        private SerializedProperty OnSteamInitalized;
        private SerializedProperty OnSteamInitalizationError;
        private SerializedProperty OnOverlayActivated;
        private SerializedProperty OnUserStatsRecieved;
        private SerializedProperty OnUserStatsStored;
        private SerializedProperty OnAchievementStored;
        private SerializedProperty OnAvatarLoaded;
        private SerializedProperty OnPersonaStateChanged;
        private SerializedProperty OnNumberOfCurrentPlayersResult;
        private SerializedProperty OnRecievedFriendChatMessage;
        private SerializedProperty disconnected;
        private SerializedProperty connected;
        private SerializedProperty failure;
        private SerializedProperty networkManager;
        public Texture2D achievementIcon;
        public Texture2D statIcon;
        public Texture2D dropBoxTexture;

        private int appTabPage = 0;
        private int seTab = 0;

        private void OnEnable()
        {
            Settings = serializedObject.FindProperty(nameof(SteamworksFoundationManager.settings));

#if MIRROR
            networkManager = serializedObject.FindProperty(nameof(SteamworksFoundationManager.networkManager));
#endif

            DoNotDestroyOnLoad = serializedObject.FindProperty(nameof(SteamworksFoundationManager._doNotDistroyOnLoad));
            OnSteamInitalized = serializedObject.FindProperty(nameof(SteamworksFoundationManager.onSteamInitalized));
            OnSteamInitalizationError = serializedObject.FindProperty(nameof(SteamworksFoundationManager.onSteamInitalizationError));
            OnOverlayActivated = serializedObject.FindProperty(nameof(SteamworksFoundationManager.onOverlayActivated));

            OnUserStatsRecieved = serializedObject.FindProperty(nameof(SteamworksFoundationManager.onUserStatsRecieved));
            OnUserStatsStored = serializedObject.FindProperty(nameof(SteamworksFoundationManager.onUserStatsStored));
            OnAchievementStored = serializedObject.FindProperty(nameof(SteamworksFoundationManager.onAchievementStored));
            OnAvatarLoaded = serializedObject.FindProperty(nameof(SteamworksFoundationManager.onAvatarLoaded));
            OnPersonaStateChanged = serializedObject.FindProperty(nameof(SteamworksFoundationManager.onPersonaStateChanged));
            OnRecievedFriendChatMessage = serializedObject.FindProperty(nameof(SteamworksFoundationManager.onRecievedFriendChatMessage));

            disconnected = serializedObject.FindProperty(nameof(SteamworksFoundationManager.disconnected));
            connected = serializedObject.FindProperty(nameof(SteamworksFoundationManager.connected));
            failure = serializedObject.FindProperty(nameof(SteamworksFoundationManager.failure));

            OnNumberOfCurrentPlayersResult = serializedObject.FindProperty(nameof(SteamworksFoundationManager.onNumberOfCurrentPlayersResult));
        }

        public override void OnInspectorGUI()
        { 
            pManager = target as SteamworksFoundationManager;

            if (pManager != null)
            {
                if(pManager.settings != null)
                {
                    if (pManager.settings.client == null)
                        pManager.settings.client = new SteamSettings.GameClient();

                    if (pManager.settings.server == null)
                        pManager.settings.server = new SteamSettings.GameServer();

                    if (pManager.settings.client.achievements == null)
                        pManager.settings.client.achievements = new System.Collections.Generic.List<SteamAchievementData>();

                    if (pManager.settings.client.stats == null)
                        pManager.settings.client.stats = new System.Collections.Generic.List<SteamStatData>();

                    pManager.settings.client.stats.RemoveAll(p => p == null);
                    pManager.settings.client.achievements.RemoveAll(p => p == null);
                }
            }

            EditorGUILayout.PropertyField(Settings);

            if (pManager.settings == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Assign a Steam Settings object to get started!");
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Create a new Steam Settings object by right clicking in your Project panel and selecting [Create] > [Steamworks] > [Steam Settings]", MessageType.Info);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                seTab = GUILayout.Toggle(seTab == 0, "Settings", EditorStyles.toolbarButton) ? 0 : seTab;
                seTab = GUILayout.Toggle(seTab == 1, "Events", EditorStyles.toolbarButton) ? 1 : seTab;
                EditorGUILayout.EndHorizontal();

                if (seTab == 0)
                {
                    if (pManager.settings != null)
                    {
                        GeneralDropAreaGUI("... Drop Stats & Achievments Here ...", pManager);

                        DrawAppOverlayData(pManager);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        DrawStatsList(pManager);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        DrawAchievementList(pManager);
                    }
                }
                else
                {
                    if (pManager.settings != null)
                    {

                        EditorGUILayout.BeginHorizontal();

                        appTabPage = GUILayout.Toggle(appTabPage == 0, "Application", EditorStyles.toolbarButton) ? 0 : appTabPage;
                        appTabPage = GUILayout.Toggle(appTabPage == 1, "Overlay", EditorStyles.toolbarButton) ? 1 : appTabPage;
                        appTabPage = GUILayout.Toggle(appTabPage == 2, "Friends", EditorStyles.toolbarButton) ? 2 : appTabPage;
                        EditorGUILayout.EndHorizontal();

                        if (appTabPage == 0)
                        {
                            EditorGUILayout.PropertyField(OnNumberOfCurrentPlayersResult);
                            EditorGUILayout.PropertyField(OnSteamInitalized);
                            EditorGUILayout.PropertyField(OnSteamInitalizationError);
                            EditorGUILayout.PropertyField(connected, new GUIContent("Game Server Connected to Steam", "Occures when the log on attempt of the Steam Server Initialization method returns as connected to the Steam backend service."));
                            EditorGUILayout.PropertyField(disconnected, new GUIContent("Game Server Disconnected from Steam", "Occures when the API disconnects from the Steam backend service."));
                            EditorGUILayout.PropertyField(failure, new GUIContent("Game Server Connection to Steam Failed", "Occures when the log on attempt of the Steam Server Initalization method fails to connect to the Steam backend service."));
                        }
                        else if (appTabPage == 1)
                        {
                            EditorGUILayout.PropertyField(OnOverlayActivated);
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(OnAvatarLoaded);
                            EditorGUILayout.PropertyField(OnPersonaStateChanged);
                            EditorGUILayout.PropertyField(OnRecievedFriendChatMessage);
                            EditorGUILayout.PropertyField(OnUserStatsRecieved);
                            EditorGUILayout.PropertyField(OnUserStatsStored);
                            EditorGUILayout.PropertyField(OnAchievementStored);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Requires Steam Settings");
                    }
                }
                //}
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSteamUserData(SteamworksFoundationManager pManager)
        {
            if(pManager.settings == null)
            {
                EditorGUILayout.HelpBox("Requires Steam Settings", MessageType.Info);
                return;
            }

            if(pManager.settings.client.user == null)
            {
                EditorGUILayout.HelpBox("Requires you reference a Steam User Data object in your Steam Settings", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Steam Id");
            EditorGUILayout.LabelField(pManager != null ? pManager.settings.client.user.id.m_SteamID.ToString() : "unknown");
            EditorGUILayout.EndHorizontal();

            if (pManager.settings.client.user.State == Steamworks.EPersonaState.k_EPersonaStateAway)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Away");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.settings.client.user.State == Steamworks.EPersonaState.k_EPersonaStateBusy)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Busy");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.settings.client.user.State == Steamworks.EPersonaState.k_EPersonaStateLookingToPlay)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Looking to Play");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.settings.client.user.State == Steamworks.EPersonaState.k_EPersonaStateLookingToTrade)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Looking to Trade");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.settings.client.user.State == Steamworks.EPersonaState.k_EPersonaStateMax)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Max");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.settings.client.user.State == Steamworks.EPersonaState.k_EPersonaStateOffline)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Offline");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.settings.client.user.State == Steamworks.EPersonaState.k_EPersonaStateOnline)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Online");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.settings.client.user.State == Steamworks.EPersonaState.k_EPersonaStateSnooze)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Snooze");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("unknown");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Steam Name");
            EditorGUILayout.LabelField(pManager != null && !string.IsNullOrEmpty(pManager.settings.client.user.DisplayName) ? pManager.settings.client.user.DisplayName : "unknown");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Avatar");
            Rect iRect = EditorGUILayout.GetControlRect(true, 150);
            EditorGUILayout.EndHorizontal();

            //iRect.y += iRect.height;
            iRect.width = 150;
            //iRect.height = 150;

            EditorGUILayout.Space();

            if (pManager.settings.client.user.avatar != null)
            {
                EditorGUI.DrawPreviewTexture(iRect, pManager.settings.client.user.avatar);
            }
            else
            {
                EditorGUI.DrawRect(iRect, Color.black);
            }
        }

        private void DrawAppOverlayData(SteamworksFoundationManager pManager)
        {
            EditorGUILayout.PropertyField(DoNotDestroyOnLoad);
            EditorGUILayout.BeginHorizontal();
            if (pManager.settings != null)
            {
                var v = System.Convert.ToUInt32(EditorGUILayout.IntField("Steam App Id", System.Convert.ToInt32(pManager.settings.applicationId.m_AppId)));
                if (v != pManager.settings.applicationId.m_AppId)
                {
                    pManager.settings.applicationId.m_AppId = v;
                    EditorUtility.SetDirty(pManager.settings);
                }
            }
            EditorGUILayout.EndHorizontal();
#if MIRROR
            EditorGUILayout.PropertyField(networkManager);
#endif
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Notification Settings");
            if (pManager.settings == null)
            {
                EditorGUILayout.LabelField("Requires Steam Settings");
            }
            else
            {
                int cSelected = (int)pManager.settings.client.overlay.notificationPosition;

                EditorGUILayout.BeginVertical();
                cSelected = EditorGUILayout.Popup(cSelected, new string[] { "Top Left", "Top Right", "Bottom Left", "Bottom Right" });

                var v = EditorGUILayout.Vector2IntField(GUIContent.none, pManager.settings.client.overlay.notificationInset);
                if (pManager.settings.client.overlay.notificationInset != v)
                {
                    pManager.settings.client.overlay.notificationInset = v;
                    EditorUtility.SetDirty(pManager.settings);
                }
                EditorGUILayout.EndVertical();

                if (pManager.settings.client.overlay.notificationPosition != (ENotificationPosition)cSelected)
                {
                    pManager.settings.client.overlay.notificationPosition = (ENotificationPosition)cSelected;
                    EditorUtility.SetDirty(pManager.settings);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatsList(SteamworksFoundationManager pManager)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Stats", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            for (int i = 0; i < pManager.settings.client.stats.Count; i++)
            {
                var target = pManager.settings.client.stats[i];
                if (target == null)
                    continue;

                Color sC = GUI.backgroundColor;

                GUI.backgroundColor = sC;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(statIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(target);
                    Selection.activeObject = target;
                }
                if (GUILayout.Button(target.name.Replace(" Float Stat Data", "").Replace(" Int Stat Data", "").Replace("Float Stat Data ", "").Replace("Int Stat Data ", "") + " ID", EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(target);
                }

                target.statName = EditorGUILayout.TextField(target.statName);

                var color = GUI.contentColor;
                var terminate = false;
                GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    pManager.settings.client.stats.RemoveAt(i);
                    terminate = true;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                if (terminate)
                    break;
            }
            EditorGUI.indentLevel = il;
        }

        private void DrawAchievementList(SteamworksFoundationManager pManager)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Achievements", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            for (int i = 0; i < pManager.settings.client.achievements.Count; i++)
            {
                var target = pManager.settings.client.achievements[i];

                if (target == null)
                    continue;

                Color sC = GUI.backgroundColor;

                GUI.backgroundColor = sC;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(achievementIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(target);
                    Selection.activeObject = target;
                }
                if (GUILayout.Button(target.name.Replace("Steam Achievement Data ", "") + " ID", EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(target);
                }

                target.achievementId = EditorGUILayout.TextField(target.achievementId);

                var terminate = false;
                var color = GUI.contentColor;
                GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    pManager.settings.client.achievements.RemoveAt(i);
                    terminate = true;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                if (terminate)
                    break;
            }
            EditorGUI.indentLevel = il;
        }
        
        private bool GeneralDropAreaGUI(string message, SteamworksFoundationManager pManager)
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 70.0f, GUILayout.ExpandWidth(true));

            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = dropBoxTexture;
            style.normal.textColor = Color.white;
            style.border = new RectOffset(20, 20, 20, 20);
            var color = GUI.backgroundColor;
            var fontColor = GUI.contentColor;
            GUI.backgroundColor = SteamUtilities.Colors.SteamGreen * SteamUtilities.Colors.HalfAlpha;
            GUI.contentColor = SteamUtilities.Colors.BrightGreen;
            GUI.Box(drop_area, "\n\n" + message, style);
            GUI.backgroundColor = color;
            GUI.contentColor = fontColor;

            bool result = false;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return false;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            // Do On Drag Stuff here
                            if (dragged_object.GetType() == typeof(SteamFloatStatData) || dragged_object.GetType() == typeof(SteamIntStatData))
                            {
                                SteamStatData go = dragged_object as SteamStatData;
                                if (!pManager.settings.client.stats.Exists(p => p == go))
                                {
                                    pManager.settings.client.stats.Add(go);
                                    EditorUtility.SetDirty(pManager.settings);
                                    result = true;
                                }
                            }
                            else if (dragged_object.GetType() == typeof(SteamAchievementData))
                            {
                                SteamAchievementData go = dragged_object as SteamAchievementData;
                                if (!pManager.settings.client.achievements.Exists(p => p == go))
                                {
                                    pManager.settings.client.achievements.Add(go);
                                    EditorUtility.SetDirty(pManager.settings);
                                    result = true;
                                }
                            }
                        }
                    }
                    break;
            }

            return result;
        }

    }
}
#endif