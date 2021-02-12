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

    [CustomEditor(typeof(SteamworksRemoteStorageManager))]
    public class SteamworksRemoteStorageEditor : Editor
    {
        private SteamworksRemoteStorageManager cloud;
        public SerializedProperty FileReadAsyncComplete;
        public SerializedProperty FileWriteAsyncComplete;

        public Texture2D dataIcon;
        public Texture2D dropBoxTexture;

        private int tabPage = 0;

        private void OnEnable()
        {
            FileReadAsyncComplete = serializedObject.FindProperty("FileReadAsyncComplete");
            FileWriteAsyncComplete = serializedObject.FindProperty("FileWriteAsyncComplete");
        }

        public override void OnInspectorGUI()
        {
            cloud = target as SteamworksRemoteStorageManager;

            Rect hRect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");

            Rect bRect = new Rect(hRect);
            bRect.width = hRect.width / 2f;
            tabPage = GUI.Toggle(bRect, tabPage == 0, "Settings", EditorStyles.toolbarButton) ? 0 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 1, "Events", EditorStyles.toolbarButton) ? 1 : tabPage;
            EditorGUILayout.EndHorizontal();

            if (tabPage == 0)
            {
                if (!DropAreaGUI("... Drop Data Libraries Here ..."))
                    DrawDataLibList();
            }
            else
            {
                EditorGUILayout.PropertyField(FileReadAsyncComplete);
                EditorGUILayout.PropertyField(FileWriteAsyncComplete);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDataLibList()
        {
            var bgColor = GUI.backgroundColor;
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < cloud.GameDataModel.Count; i++)
            {
                var item = cloud.GameDataModel[i];
                
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(dataIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                    Selection.activeObject = item;
                }
                if (GUILayout.Button(item.name, EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                }
                item.filePrefix = EditorGUILayout.TextField(item.filePrefix);
                var color = GUI.contentColor;
                GUI.contentColor = SteamUtilities.Colors.ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    cloud.GameDataModel.RemoveAt(i);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;
            GUI.backgroundColor = bgColor;
        }

        private bool DropAreaGUI(string message)
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 70.0f, GUILayout.ExpandWidth(true));

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
                            if (dragged_object.GetType() == typeof(SteamDataLibrary))
                            {
                                SteamDataLibrary go = dragged_object as SteamDataLibrary;
                                if (!cloud.GameDataModel.Contains(go))
                                {
                                    cloud.GameDataModel.Add(go);
                                    return true;
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