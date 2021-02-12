#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.PlayerServices;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(CraftingRecipe))]
    public class CraftingRecipeEditor : Editor
    {
        private SerializedProperty DeveloperDescription;

        private void OnEnable()
        {
            DeveloperDescription = serializedObject.FindProperty("DeveloperDescription");
        }

        public override void OnInspectorGUI()
        {
            var pManager = target as CraftingRecipe;

            EditorGUILayout.PropertyField(DeveloperDescription);
            EditorGUILayout.Space();

            if (DefinitionDropAreaGUI("Drop Item Definitions here to add them", pManager)
                || DrawDefinitionList(pManager))
                EditorUtility.SetDirty(pManager);

            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawDefinitionList(CraftingRecipe pManager)
        {
            var isDirty = false;
            if (pManager.Items == null)
                pManager.Items = new System.Collections.Generic.List<InventoryItemDefinitionCount>();

            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < pManager.Items.Count; i++)
            {
                Rect r = EditorGUILayout.GetControlRect();
                var style = EditorStyles.miniButtonLeft;
                Color sC = GUI.backgroundColor;
                GUI.backgroundColor = new Color(sC.r * 1.25f, sC.g * 0.5f, sC.b * 0.5f, sC.a);
                if (GUI.Button(new Rect(r) { x = r.width, width = 20, height = 15 }, "X", EditorStyles.miniButtonLeft))
                {
                    GUI.backgroundColor = sC;
                    pManager.Items.RemoveAt(i);
                    return true;
                }
                else
                {
                    GUI.backgroundColor = sC;
                    if (GUI.Button(new Rect(r) { x = 0, width = 20, height = 15 }, "P", EditorStyles.miniButtonRight))
                    {
                        EditorGUIUtility.PingObject(pManager.Items[i].Item);
                    }
                    var w = r.width;
                    r.width = 50;
                    var c = System.Convert.ToUInt32(EditorGUI.IntField(r, System.Convert.ToInt32(pManager.Items[i].Count)));
                    if (c != pManager.Items[i].Count)
                    {
                        isDirty = true;
                        pManager.Items[i].Count = c;
                    }
                    r.x += 45;
                    r.width = w - 70;
                    var p = EditorGUI.ObjectField(r, pManager.Items[i].Item, typeof(InventoryItemDefinition), false) as InventoryItemDefinition;
                    if(p != pManager.Items[i].Item)
                    {
                        pManager.Items[i].Item = p;
                        isDirty = true;
                        if (p == null)
                        {
                            pManager.Items.RemoveAt(i);
                            return true;
                        }
                    }
                }
            }
            EditorGUI.indentLevel = il;
            return isDirty;
        }

        private bool DefinitionDropAreaGUI(string message, CraftingRecipe pManager)
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, "\n\n" + message, EditorStyles.helpBox);

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
                            if (dragged_object.GetType().IsSubclassOf(typeof(InventoryItemDefinition))
                                || dragged_object.GetType() == typeof(InventoryItemDefinition))
                            {
                                InventoryItemDefinition go = dragged_object as InventoryItemDefinition;
                                if (go != null)
                                {
                                    if (!pManager.Items.Exists(p => p.Item == go))
                                    {
                                        pManager.Items.Add(new InventoryItemDefinitionCount() { Count = 1, Item = go });
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