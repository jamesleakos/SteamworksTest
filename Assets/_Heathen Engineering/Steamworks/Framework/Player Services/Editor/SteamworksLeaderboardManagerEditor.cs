#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.PlayerServices;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(SteamworksLeaderboardManager))]
    public class SteamworksLeaderboardManagerEditor : Editor
    {
        private SerializedProperty LeaderboardRankChanged;
        private SerializedProperty LeaderboardRankLoaded;
        private SerializedProperty LeaderboardNewHighRank;

        private int seTab = 0;
        public Texture2D leaderboardIcon;
        public Texture2D dropBoxTexture;

        private void OnEnable()
        {
            LeaderboardRankChanged = serializedObject.FindProperty("LeaderboardRankChanged");
            LeaderboardRankLoaded = serializedObject.FindProperty("LeaderboardRankLoaded");
            LeaderboardNewHighRank = serializedObject.FindProperty("LeaderboardNewHighRank");
        }

        public override void OnInspectorGUI()
        {
            var pManager = target as SteamworksLeaderboardManager;

            var hRect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            var bRect = new Rect(hRect);
            bRect.width = hRect.width / 2f;
            seTab = GUI.Toggle(bRect, seTab == 0, "Settings", EditorStyles.toolbarButton) ? 0 : seTab;
            bRect.x += bRect.width;
            seTab = GUI.Toggle(bRect, seTab == 1, "Events", EditorStyles.toolbarButton) ? 1 : seTab;
            EditorGUILayout.EndHorizontal();

            if (seTab == 0)
            {
                if (!LeaderboardDropAreaGUI("Drop Leaderboards here to add them", pManager))
                    DrawLeaderboardList(pManager);
            }
            else
            {
                EditorGUILayout.PropertyField(LeaderboardRankChanged);
                EditorGUILayout.PropertyField(LeaderboardRankLoaded);
                EditorGUILayout.PropertyField(LeaderboardNewHighRank);
            }

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawLeaderboardList(SteamworksLeaderboardManager pManager)
        {
            if (pManager.Leaderboards == null)
                pManager.Leaderboards = new System.Collections.Generic.List<SteamworksLeaderboardData>();

            pManager.Leaderboards.RemoveAll(p => p == null);
            if (pManager.Leaderboards == null)
                pManager.Leaderboards = new System.Collections.Generic.List<SteamworksLeaderboardData>();

            var bgColor = GUI.backgroundColor;
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < pManager.Leaderboards.Count; i++)
            {
                var item = pManager.Leaderboards[i];

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(leaderboardIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                    Selection.activeObject = item;
                }
                if (GUILayout.Button(item.name.Replace("Leaderboard", "") + " ID", EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                }
                item.leaderboardName = EditorGUILayout.TextField(item.leaderboardName);
                var color = GUI.contentColor;
                GUI.contentColor = SteamUtilities.Colors.ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    pManager.Leaderboards.RemoveAt(i);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;
            GUI.backgroundColor = bgColor;


        }

        private bool LeaderboardDropAreaGUI(string message, SteamworksLeaderboardManager pManager)
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));

            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = dropBoxTexture;
            style.normal.textColor = Color.white;
            style.border = new RectOffset(5, 5, 5, 5);
            var color = GUI.backgroundColor;
            var fontColor = GUI.contentColor;
            GUI.backgroundColor = SteamUtilities.Colors.SteamGreen * SteamUtilities.Colors.HalfAlpha;
            GUI.contentColor = SteamUtilities.Colors.BrightGreen;
            GUI.Box(drop_area, "\n\n" + message, style);
            GUI.backgroundColor = color;
            GUI.contentColor = fontColor;

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
                            if (dragged_object.GetType() == typeof(SteamworksLeaderboardData))
                            {
                                SteamworksLeaderboardData go = dragged_object as SteamworksLeaderboardData;
                                if (go != null)
                                {
                                    if (!pManager.Leaderboards.Exists(p => p == go))
                                    {
                                        pManager.Leaderboards.Add(go);
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            return false;
        }
    }
}
#endif