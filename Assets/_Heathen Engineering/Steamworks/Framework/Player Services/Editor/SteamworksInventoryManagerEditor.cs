#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.PlayerServices;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(SteamworksInventoryManager))]
    public class SteamworksInventoryManagerEditor : Editor
    {
        private SerializedProperty Settings;
        private SerializedProperty RefreshOnStart;
        private SerializedProperty ItemInstancesUpdated;
        private SerializedProperty ItemsGranted;
        private SerializedProperty ItemsConsumed;
        private SerializedProperty ItemsExchanged;
        private SerializedProperty ItemsDroped;
        public Texture itemIcon;
        public Texture generatorIcon;
        public Texture tagIcon;
        public Texture bundleIcon;
        public Texture recipeIcon;
        public Texture pointerIcon;
        public Texture2D dropBoxTexture;

        private int seTab = 0;

        private void OnEnable()
        {
            Settings = serializedObject.FindProperty("Settings");
            RefreshOnStart = serializedObject.FindProperty("RefreshOnStart");
            ItemInstancesUpdated = serializedObject.FindProperty("ItemInstancesUpdated");
            ItemsGranted = serializedObject.FindProperty("ItemsGranted");
            ItemsConsumed = serializedObject.FindProperty("ItemsConsumed");
            ItemsExchanged = serializedObject.FindProperty("ItemsExchanged");
            ItemsDroped = serializedObject.FindProperty("ItemsDroped");
        }

        public override void OnInspectorGUI()
        {
            var pManager = target as SteamworksInventoryManager;

            EditorGUILayout.PropertyField(Settings);

            if (pManager.Settings != null)
            {
                EditorGUILayout.PropertyField(RefreshOnStart);

                if (GUILayout.Button("Open Editor", EditorStyles.toolbarButton))
                {
                    ItemDefJsonGenerator.Init();
                }
                var hRect = EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("");
                var bRect = new Rect(hRect);
                bRect.width = hRect.width / 2f;
                seTab = GUI.Toggle(bRect, seTab == 0, "References", EditorStyles.toolbarButton) ? 0 : seTab;
                bRect.x += bRect.width;
                seTab = GUI.Toggle(bRect, seTab == 1, "Events", EditorStyles.toolbarButton) ? 1 : seTab;
                EditorGUILayout.EndHorizontal();

                if (seTab == 0)
                {
                    if (!GeneralDropAreaGUI("... Drop Inventory Objects Here ...", pManager.Settings))
                        DrawItemList(pManager.Settings);
                    else
                        EditorUtility.SetDirty(pManager.Settings);
                }
                else
                {
                    EditorGUILayout.PropertyField(ItemInstancesUpdated);
                    EditorGUILayout.PropertyField(ItemsGranted);
                    EditorGUILayout.PropertyField(ItemsConsumed);
                    EditorGUILayout.PropertyField(ItemsExchanged);
                    EditorGUILayout.PropertyField(ItemsDroped);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("You must apply a Steamworks Inventory Settings object to use the Inventory Manager.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        private bool GeneralDropAreaGUI(string message, SteamworksInventorySettings settings)
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

                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    bool retVal = false;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            // Do On Drag Stuff here
                            if (dragged_object.GetType().IsAssignableFrom(typeof(InventoryItemBundleDefinition)))
                            {
                                InventoryItemBundleDefinition go = dragged_object as InventoryItemBundleDefinition;
                                if (go != null)
                                {
                                    if (!settings.ItemBundles.Exists(p => p == go))
                                    {
                                        settings.ItemBundles.Add(go);
                                        EditorUtility.SetDirty(settings);
                                        retVal = true;
                                    }
                                }
                            }
                            else if (dragged_object.GetType().IsAssignableFrom(typeof(TagGeneratorDefinition)))
                            {
                                TagGeneratorDefinition go = dragged_object as TagGeneratorDefinition;
                                if (go != null)
                                {
                                    if (!settings.TagGenerators.Exists(p => p == go))
                                    {
                                        settings.TagGenerators.Add(go);
                                        EditorUtility.SetDirty(settings);
                                        retVal = true;
                                    }
                                }
                            }
                            else if (dragged_object.GetType().IsAssignableFrom(typeof(ItemGeneratorDefinition)))
                            {
                                ItemGeneratorDefinition go = dragged_object as ItemGeneratorDefinition;
                                if (go != null)
                                {
                                    if (!settings.ItemGenerators.Exists(p => p == go))
                                    {
                                        settings.ItemGenerators.Add(go);
                                        EditorUtility.SetDirty(settings);
                                        retVal = true;
                                    }
                                }
                            }
                            else if (dragged_object.GetType().IsSubclassOf(typeof(InventoryItemDefinition)))
                            {
                                InventoryItemDefinition go = dragged_object as InventoryItemDefinition;
                                if (go != null)
                                {
                                    if (!settings.ItemDefinitions.Exists(p => p == go))
                                    {
                                        settings.ItemDefinitions.Add(go);
                                        EditorUtility.SetDirty(settings);
                                        retVal = true;
                                    }
                                }
                            }
                        }
                    }

                    return retVal;
            }

            return false;
        }

        private void DrawItemList(SteamworksInventorySettings settings)
        {
            if (settings != null)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("Items", EditorStyles.whiteLabel, GUILayout.Width(250));
                EditorGUILayout.EndHorizontal();

                if (settings.ItemDefinitions == null)
                {
                    settings.ItemDefinitions = new List<InventoryItemDefinition>();
                    EditorUtility.SetDirty(settings);
                }


                DrawDefinitionList(settings);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("Bundles", EditorStyles.whiteLabel, GUILayout.Width(250));
                EditorGUILayout.EndHorizontal();

                if (settings.ItemDefinitions == null)
                {
                    settings.ItemDefinitions = new List<InventoryItemDefinition>();
                    EditorUtility.SetDirty(settings);
                }

                DrawBundleList(settings);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("Generators", EditorStyles.whiteLabel, GUILayout.Width(250));
                EditorGUILayout.EndHorizontal();

                if (settings.ItemGenerators == null)
                {
                    settings.ItemGenerators = new List<ItemGeneratorDefinition>();
                    EditorUtility.SetDirty(settings);
                }

                DrawGeneratorList(settings);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("Tags", EditorStyles.whiteLabel, GUILayout.Width(250));
                EditorGUILayout.EndHorizontal();

                if (settings.TagGenerators == null)
                {
                    settings.TagGenerators = new List<TagGeneratorDefinition>();
                    EditorUtility.SetDirty(settings);
                }

                DrawTagList(settings);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("Recipes", EditorStyles.whiteLabel, GUILayout.Width(50));
                EditorGUILayout.LabelField("(used in an Item, Bundle or Generator)", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                DrawRecipeList(settings);
            }
        }
        
        private void DrawDefinitionList(SteamworksInventorySettings settings)
        {
            var bgColor = GUI.backgroundColor;
            var erredColor = new Color(1, 0.5f, 0.5f, 1);
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < settings.ItemDefinitions.Count; i++)
            {
                var item = settings.ItemDefinitions[i];

                if (!ValidateItemPointer(settings, item))
                {
                    GUI.backgroundColor = erredColor;
                }
                else
                    GUI.backgroundColor = bgColor;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(itemIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
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

                var color = GUI.contentColor;
                GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    settings.ItemDefinitions.RemoveAt(i);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;
            GUI.backgroundColor = bgColor;
        }

        private void DrawBundleList(SteamworksInventorySettings settings)
        {
            var bgColor = GUI.backgroundColor;
            var erredColor = new Color(1, 0.5f, 0.5f, 1);
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < settings.ItemBundles.Count; i++)
            {
                var item = settings.ItemBundles[i];

                if (!ValidateItemPointer(settings, item))
                {
                    GUI.backgroundColor = erredColor;
                }
                else
                    GUI.backgroundColor = bgColor;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(bundleIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
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

                var color = GUI.contentColor;
                GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    settings.ItemBundles.RemoveAt(i);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;

            GUI.backgroundColor = bgColor;
        }

        private void DrawGeneratorList(SteamworksInventorySettings settings)
        {
            var bgColor = GUI.backgroundColor;
            var erredColor = new Color(1, 0.5f, 0.5f, 1);
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < settings.ItemGenerators.Count; i++)
            {
                var item = settings.ItemGenerators[i];

                if (!ValidateItemPointer(settings, item))
                {
                    GUI.backgroundColor = erredColor;
                }
                else
                    GUI.backgroundColor = bgColor;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(generatorIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
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

                var color = GUI.contentColor;
                GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    settings.ItemGenerators.RemoveAt(i);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;

            GUI.backgroundColor = bgColor;
        }

        private void DrawTagList(SteamworksInventorySettings settings)
        {
            var bgColor = GUI.backgroundColor;
            var erredColor = new Color(1, 0.5f, 0.5f, 1);
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < settings.TagGenerators.Count; i++)
            {
                var item = settings.TagGenerators[i];

                if (!ValidateTagGenerator(settings, item))
                {
                    GUI.backgroundColor = erredColor;
                }
                else
                    GUI.backgroundColor = bgColor;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(tagIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
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

                var color = GUI.contentColor;
                GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    settings.TagGenerators.RemoveAt(i);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = bgColor;
            }
            EditorGUI.indentLevel = il;

        }

        private void DrawRecipeList(SteamworksInventorySettings settings)
        {
            var bgColor = GUI.backgroundColor;
            var erredColor = new Color(1, 0.5f, 0.5f, 1);
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            List<CraftingRecipe> usedRecipies = new List<CraftingRecipe>();
            foreach (var item in settings.ItemDefinitions)
            {
                foreach (var recipie in item.Recipes)
                {
                    if (!usedRecipies.Contains(recipie))
                        usedRecipies.Add(recipie);
                }
            }

            foreach (var item in settings.ItemGenerators)
            {
                foreach (var recipie in item.Recipes)
                {
                    if (!usedRecipies.Contains(recipie))
                        usedRecipies.Add(recipie);
                }
            }

            foreach (var item in settings.ItemBundles)
            {
                foreach (var recipie in item.Recipes)
                {
                    if (!usedRecipies.Contains(recipie))
                        usedRecipies.Add(recipie);
                }
            }

            for (int i = 0; i < usedRecipies.Count; i++)
            {
                var recipe = usedRecipies[i];

                if (!ValidateRecipie(settings, recipe))
                    GUI.backgroundColor = erredColor;
                else
                    GUI.backgroundColor = bgColor;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(recipeIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(recipe);
                    Selection.activeObject = recipe;
                }
                if (GUILayout.Button(recipe.name, EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(recipe);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;

            GUI.backgroundColor = bgColor;
        }

        private bool ValidateRecipie(SteamworksInventorySettings settings, CraftingRecipe recipe)
        {
            if (recipe.Items == null || recipe.Items.Count < 1)
                return false;

            foreach (var entry in recipe.Items)
            {
                if (entry.Count < 1)
                    return false;
                else if (!settings.ItemDefinitions.Contains(entry.Item))
                    return false;
                else if (recipe.Items.Where(p => p.Item == entry.Item).Count() > 1)
                    return false;
            }

            return true;
        }
                
        bool ValidateTagGenerator(SteamworksInventorySettings settings, TagGeneratorDefinition pointer)
        {
            if (!IsItemIdUnique(settings, pointer.DefinitionID.m_SteamItemDef))
            {
                return false;
            }
            else
                return true;
        }

        bool IsItemIdUnique(SteamworksInventorySettings settings, int Id)
        {
            var countedIds = new List<int>();
            foreach (var p in settings.ItemDefinitions)
            {
                if (p.DefinitionID.m_SteamItemDef == Id)
                    countedIds.Add(Id);
            }
            foreach (var p in settings.ItemBundles)
            {
                if (p.DefinitionID.m_SteamItemDef == Id)
                    countedIds.Add(Id);
            }
            foreach (var p in settings.ItemGenerators)
            {
                if (p.DefinitionID.m_SteamItemDef == Id)
                    countedIds.Add(Id);
            }
            foreach (var p in settings.TagGenerators)
            {
                if (p.DefinitionID.m_SteamItemDef == Id)
                    countedIds.Add(Id);
            }

            if (countedIds.Count > 1)
            {
                return false;
            }
            else
                return true;
        }
        
        bool ValidateItemPointer(SteamworksInventorySettings settings, InventoryItemPointer pointer)
        {
            var result = true;

            if (!IsItemIdUnique(settings, pointer.DefinitionID.m_SteamItemDef))
            {
                return false;
            }
            if (pointer.ItemType != InventoryItemType.ItemBundle && pointer.ValveItemDefAttributes != null && pointer.ValveItemDefAttributes.Any(p => p.attribute == ValveItemDefSchemaAttributes.purchase_bundle_discount))
            {
                return false;
            }
            if (pointer.Recipes != null)
            {
                foreach (var recipie in pointer.Recipes)
                {
                    if (recipie == null)
                    {
                        return false;
                    }
                    else
                    {
                        foreach (var entry in recipie.Items)
                        {
                            if (entry.Count < 1)
                                return false;
                            else if (!settings.ItemDefinitions.Contains(entry.Item))
                                return false;
                            else if (pointer.DefinitionID.m_SteamItemDef == entry.Item.DefinitionID.m_SteamItemDef)
                                return false;
                            else if (recipie.Items.Where(p => p.Item == entry.Item).Count() > 1)
                                return false;
                        }
                    }
                }
            }

            if (pointer.ItemType == InventoryItemType.ItemBundle)
            {
                var bundle = pointer as InventoryItemBundleDefinition;

                if (bundle.Content == null || bundle.Content.Count < 1)
                    return false;

                foreach (var item in bundle.Content)
                {
                    if (item.Item == pointer)
                        return false;

                    if (item.Count < 1)
                        return false;

                    if (item.Item == null)
                        return false;

                    if (item.Item.ItemType == InventoryItemType.ItemDefinition
                        && !settings.ItemDefinitions.Contains((InventoryItemDefinition)item.Item))
                    {
                        return false;
                    }

                    if (item.Item.ItemType == InventoryItemType.ItemBundle
                        && !settings.ItemBundles.Contains((InventoryItemBundleDefinition)item.Item))
                    {
                        return false;
                    }

                    if (item.Item.ItemType == InventoryItemType.ItemGenerator
                        && !settings.ItemGenerators.Contains((ItemGeneratorDefinition)item.Item))
                    {
                        return false;
                    }
                }
            }

            if (pointer.ItemType == InventoryItemType.ItemGenerator)
            {
                var generator = pointer as ItemGeneratorDefinition;

                if (generator.Content == null || generator.Content.Count < 1)
                    return false;

                foreach (var item in generator.Content)
                {
                    if (item.Item == pointer)
                        return false;

                    if (item.Count < 1)
                        return false;

                    if (item.Item == null)
                        return false;

                    if (item.Item.ItemType == InventoryItemType.ItemDefinition
                        && !settings.ItemDefinitions.Contains((InventoryItemDefinition)item.Item))
                    {
                        return false;
                    }

                    if (item.Item.ItemType == InventoryItemType.ItemBundle
                        && !settings.ItemBundles.Contains((InventoryItemBundleDefinition)item.Item))
                    {
                        return false;
                    }

                    if (item.Item.ItemType == InventoryItemType.ItemGenerator
                        && !settings.ItemGenerators.Contains((ItemGeneratorDefinition)item.Item))
                    {
                        return false;
                    }
                }
            }

            return result;
        }
    }
}
#endif