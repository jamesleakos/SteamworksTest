#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.GameServices;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(SteamworksDLCManager))]
    public class SteamworksDLCEditor : Editor
    {
        private SteamworksDLCManager manager;

        public Texture2D dataIcon;
        public Texture2D dropBoxTexture;
        
        public override void OnInspectorGUI()
        {
            manager = target as SteamworksDLCManager;

            if (!DropAreaGUI("... Drop DLC Here ..."))
                DrawDataLibList();
        }

        private void DrawDataLibList()
        {
            var bgColor = GUI.backgroundColor;
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < manager.DLC.Count; i++)
            {
                var item = manager.DLC[i];

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

                item.AppId.m_AppId = (uint)EditorGUILayout.IntField((int)item.AppId.m_AppId);

                var color = GUI.contentColor;
                GUI.contentColor = SteamUtilities.Colors.ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    manager.DLC.RemoveAt(i);
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
                            if (dragged_object.GetType() == typeof(SteamDLCData))
                            {
                                SteamDLCData go = dragged_object as SteamDLCData;
                                if (!manager.DLC.Contains(go))
                                {
                                    manager.DLC.Add(go);
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