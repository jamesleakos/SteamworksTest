#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.PlayerServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    public class ItemDefJsonGenerator : EditorWindow
    {
        public Texture icon;
        public Texture itemIcon;
        public Texture generatorIcon;
        public Texture tagIcon;
        public Texture bundleIcon;
        public Texture recipeIcon;
        public Texture pointerIcon;
        public Texture2D dropBoxTexture;

        public static ItemDefJsonGenerator instance;
        const string settingsPath = "Heathen.Steamworks.Inventory.Settings";
        const string steamSettingsPath = "Heathen.Steamworks.Settings";

        private GUIStyle popupStyle;
        private bool showTips = true;
        private string[] AttributeRuleTypes =
        {
            "Owns",
            "Achievement",
            "Played",
            "Manual"
        };
        private string[] AttributeTypes =
        {
            "Name",
            "Description",
            "Display Type",
            "Promo",
            "Drop Start Time",
            "Price",
            "Price Category",
            "Background Color",
            "Name Color",
            "Icon Url",
            "Icon Url Large",
            "Marketable",
            "Tradable",
            "Tags",
            "Tag Generators",
            "Store Tags",
            "Store Images",
            "Hidden",
            "Store Hidden",
            "Use Drop Limit",
            "Drop Limit",
            "Drop Interval",
            "Use Drop Window",
            "Drop Window",
            "Drop Max Per Winidow",
            "Granted Manually",
            "Use Bundle Price",
            "Item Slot",
            "Item Quality",
            "Purchase Bundle Discount"
        };

        private string[] AttributeLanguage =
        {
            "None",
            "العربية (Arabic)",
            "български език (Bulgarian)",
            "简体中文 (Simplified Chinese)",
            "繁體中文 (Traditional Chinese)",
            "čeština (Czech)",
            "Dansk (Danish)",
            "Nederlands (Dutch)",
            "English",
            "Suomi (Finnish)",
            "Français (French)",
            "Deutsch (German)",
            "Ελληνικά (Greek)",
            "Magyar (Hungarian)",
            "Italiano (Italian)",
            "日本語 (Japanese)",
            "한국어 (Korean)",
            "Norsk (Norwegian)",
            "Polski (Polish)",
            "Português (Portuguese)",
            "Português-Brasil (Brazilian)",
            "Română (Romanian)",
            "Русский (Russian)",
            "Español-España (Spanish-Spain)",
            "Español-Latinoamérica (Spanish-Latin America)",
            "Svenska (Swedish)",
            "ไทย (Thai)",
            "Türkçe (turkish)",
            "Українська (Ukrainian)",
            "Tiếng Việt (Vietnamese)"
        };

        private string[] AttributeCurrencyName =
        {
            "Euro",
            "British Pound",
            "Austrailian Dollar",
            "United States Dollar",
            "Poland Zlotych",
            "Russia Rubles"
        };

        private string[] AttributeCurrencyCode =
        {
            "EUR",
            "GBP",
            "AUD",
            "USD",
            "PLN",
            "RUB"
        };

        private string[] AttributeVLV =
        {
            "0.25",
            "0.50",
            "0.75",
            "1.00",
            "1.50",
            "2.00",
            "2.50",
            "3.00",
            "3.50",
            "4.00",
            "4.50",
            "5.00",
            "5.50",
            "6.00",
            "6.50",
            "7.00",
            "7.50",
            "8.00",
            "8.50",
            "9.00",
            "9.50",
            "10.00",
            "11.00",
            "12.00",
            "13.00",
            "14.00",
            "15.00",
            "16.00",
            "17.00",
            "18.00",
            "19.00",
            "20.00",
            "25.00",
            "30.00",
            "35.00",
            "40.00",
            "45.00",
            "50.00",
            "60.00",
            "70.00",
            "80.00",
            "90.00",
            "100.00"
        };

        private SteamworksInventorySettings settings;
        private SteamSettings steamSettings;
        private bool isValidModel = false;
        private Vector2 listScrollPos;
        private Vector2 editorScrollPos;
        private InventoryItemDefinition activeItem;
        private ItemGeneratorDefinition activeGenerator;
        private TagGeneratorDefinition activeTag;
        private InventoryItemBundleDefinition activeBundle;
        private CraftingRecipe activeRecipie;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Steamworks/Inventory Editor")]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            ItemDefJsonGenerator window = EditorWindow.GetWindow<ItemDefJsonGenerator>(" Inventory Editor", new Type[] { typeof(UnityEditor.SceneView) });
            window.titleContent = new GUIContent(" Inventory Editor", window.icon);
            window.Show();
        }

        public ItemDefJsonGenerator()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        void OnGUI()
        {
            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            DrawSettingsArea();

            if (settings != null && steamSettings != null)
            {
                settings.ItemDefinitions.RemoveAll(p => p == null);
                settings.ItemBundles.RemoveAll(p => p == null);
                settings.ItemGenerators.RemoveAll(p => p == null);
                settings.TagGenerators.RemoveAll(p => p == null);

                if (settings != null && activeItem == null && activeGenerator == null && activeBundle == null && activeTag == null && activeRecipie == null)
                {
                    //We should select something if we can
                    if (settings.ItemDefinitions != null && settings.ItemDefinitions.Count > 0)
                        activeItem = settings.ItemDefinitions[0];
                    else if (settings.ItemBundles != null && settings.ItemBundles.Count > 0)
                        activeBundle = settings.ItemBundles[0];
                    else if (settings.ItemGenerators != null && settings.ItemGenerators.Count > 0)
                        activeGenerator = settings.ItemGenerators[0];
                    else if (settings.TagGenerators != null && settings.TagGenerators.Count > 0)
                        activeTag = settings.TagGenerators[0];
                }

                EditorGUILayout.BeginHorizontal();
                DrawItemList();
                DrawItemEditor();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("You must provide a Steam Settings and Steam Inventory Settings object to use this tool!", MessageType.Info);
            }
        }

        private void DrawItemEditor()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            Color erredColor = new Color(1, 0.50f, 0.50f, 1);
            Color normalColor = GUI.contentColor;

            if (activeItem != null)
            {
                if (settings.ItemDefinitions.Contains(activeItem))
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField(activeItem.name, EditorStyles.whiteLabel);

                    showTips = EditorGUILayout.Toggle("Show Tips", showTips);

                    if (GUILayout.Button("Add Empty", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);
                        activeItem.ValveItemDefAttributes.Add(new ValveItemDefAttribute());
                    }
                    if (GUILayout.Button("Add All", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);

                        if (activeItem.ValveItemDefAttributes == null)
                            activeItem.ValveItemDefAttributes = new List<ValveItemDefAttribute>();

                        for (int i = 0; i < 30; i++)
                        {
                            if (!activeItem.ValveItemDefAttributes.Any(p => (int)p.attribute == i))
                            {
                                activeItem.ValveItemDefAttributes.Add(new ValveItemDefAttribute()
                                {
                                    attribute = (ValveItemDefSchemaAttributes)i
                                });
                            }
                        }
                    }
                    if (GUILayout.Button("Add Common", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);

                        for (int i = 0; i < 30; i++)
                        {
                            if (i == 2 || i == 3 || i == 4 || i == 5 || i == 6 || i == 7 || i == 8 || i == 14 || i == 16 || i == 17 || i == 18
                                || i == 19 || i == 20 || i == 21 || i == 22 || i == 23 || i == 24 || i == 25 || i == 26 || i == 27 || i == 28 || i == 29)
                            {
                                continue;
                            }

                            if (!activeItem.ValveItemDefAttributes.Any(p => (int)p.attribute == i))
                            {
                                activeItem.ValveItemDefAttributes.Add(new ValveItemDefAttribute()
                                {
                                    attribute = (ValveItemDefSchemaAttributes)i
                                });
                            }
                        }
                    }
                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);

                        activeItem.ValveItemDefAttributes.Clear();
                    }
                    EditorGUILayout.EndHorizontal();

                    editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos);

                    var color = GUI.contentColor;

                    #region Item ID
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    if (!IsItemIdUnique(activeItem.DefinitionID.m_SteamItemDef))
                    {
                        GUI.contentColor = erredColor;
                        EditorGUILayout.PrefixLabel(new GUIContent("ID", "This ID is not unqiue"));
                    }
                    else
                    {
                        GUI.contentColor = normalColor;
                        EditorGUILayout.PrefixLabel(new GUIContent("ID"));
                    }

                    activeItem.DefinitionID.m_SteamItemDef = EditorGUILayout.IntField(activeItem.DefinitionID.m_SteamItemDef);

                    GUI.contentColor = normalColor;

                    EditorGUILayout.EndHorizontal();
                    #endregion

                    #region Exchange
                    if (showTips)
                    {
                        EditorGUILayout.HelpBox("Each recipie defines a collection and quantity of items that can be exchanged for 1 of this item.\nRecipies are Scriptable Objects, right click and select Create > Steamworks > Player Services > Crafting Recipie to create a new one.", MessageType.Info);
                    }
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.PrefixLabel("Exchange");

                    if (activeItem.Recipes == null)
                        activeItem.Recipes = new List<CraftingRecipe>();

                    EditorGUILayout.BeginVertical();
                    RecipieDropAreaGUI("... Drop Recipies Here ...", activeItem);

                    for (int i = 0; i < activeItem.Recipes.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var data = activeItem.Recipes[i];

                        if (!ValidateRecipie(data))
                            GUI.contentColor = erredColor;
                        else
                            GUI.contentColor = normalColor;

                        activeItem.Recipes[i] = EditorGUILayout.ObjectField(data, typeof(CraftingRecipe), false) as CraftingRecipe;

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            activeItem.Recipes.RemoveAt(i);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    DrawItemPointerAttributes(activeItem);

                    EditorGUILayout.EndScrollView();
                }
                else
                    activeItem = null;
            }
            else if (activeGenerator != null)
            {
                if (settings.ItemGenerators.Contains(activeGenerator))
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField(activeGenerator.name, EditorStyles.whiteLabel);

                    showTips = EditorGUILayout.Toggle("Show Tips", showTips);

                    if (GUILayout.Button("Add Empty", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);
                        activeGenerator.ValveItemDefAttributes.Add(new ValveItemDefAttribute());
                    }
                    if (GUILayout.Button("Add All", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);

                        if (activeGenerator.ValveItemDefAttributes == null)
                            activeGenerator.ValveItemDefAttributes = new List<ValveItemDefAttribute>();

                        for (int i = 0; i < 30; i++)
                        {
                            if (!activeGenerator.ValveItemDefAttributes.Any(p => (int)p.attribute == i))
                            {
                                activeGenerator.ValveItemDefAttributes.Add(new ValveItemDefAttribute()
                                {
                                    attribute = (ValveItemDefSchemaAttributes)i
                                });
                            }
                        }
                    }
                    if (GUILayout.Button("Add Common", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);

                        for (int i = 0; i < 30; i++)
                        {
                            if (i == 2 || i == 3 || i == 4 || i == 5 || i == 6 || i == 7 || i == 8 || i == 9 || i == 10 || i == 13 || i == 14 || i == 15 || i == 16 || i == 17 || i == 18
                                || i == 19 || i == 20 || i == 21 || i == 22 || i == 23 || i == 24 || i == 25 || i == 26 || i == 27 || i == 28 || i == 29)
                            {
                                continue;
                            }

                            if (!activeGenerator.ValveItemDefAttributes.Any(p => (int)p.attribute == i))
                            {
                                activeGenerator.ValveItemDefAttributes.Add(new ValveItemDefAttribute()
                                {
                                    attribute = (ValveItemDefSchemaAttributes)i
                                });
                            }
                        }
                    }
                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);

                        activeGenerator.ValveItemDefAttributes.Clear();
                    }
                    EditorGUILayout.EndHorizontal();

                    var color = GUI.contentColor;

                    editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos);

                    #region Item ID
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.PrefixLabel("ID");

                    if (!IsItemIdUnique(activeGenerator.DefinitionID.m_SteamItemDef))
                        GUI.contentColor = erredColor;
                    else
                        GUI.contentColor = normalColor;

                    activeGenerator.DefinitionID.m_SteamItemDef = EditorGUILayout.IntField(activeGenerator.DefinitionID.m_SteamItemDef);

                    GUI.contentColor = normalColor;

                    EditorGUILayout.EndHorizontal();
                    #endregion

                    #region Exchange
                    if (showTips)
                    {
                        EditorGUILayout.HelpBox("Each recipie defines a collection and quantity of items that can be exchanged for 1 of this item.\nRecipies are Scriptable Objects, right click and select Create > Steamworks > Player Services > Crafting Recipie to create a new one.", MessageType.Info);
                    }
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.PrefixLabel("Exchange");

                    if (activeGenerator.Recipes == null)
                        activeGenerator.Recipes = new List<CraftingRecipe>();

                    EditorGUILayout.BeginVertical();
                    RecipieDropAreaGUI("... Drop Recipies Here ...", activeGenerator);

                    for (int i = 0; i < activeGenerator.Recipes.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var data = activeGenerator.Recipes[i];

                        if (!ValidateRecipie(data))
                            GUI.contentColor = erredColor;
                        else
                            GUI.contentColor = normalColor;

                        activeGenerator.Recipes[i] = EditorGUILayout.ObjectField(data, typeof(CraftingRecipe), false) as CraftingRecipe;

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            activeGenerator.Recipes.RemoveAt(i);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    #region Content
                    if (showTips)
                    {
                        EditorGUILayout.HelpBox("Lists the items that this generator can produce, the weights are used to deterim which item will be generated, this is done by teh Valve backend and will favor options with a higher weight.", MessageType.Info);
                    }
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    if (activeGenerator.Content == null)
                        activeGenerator.Content = new List<InventoryItemPointerCount>();

                    if (activeGenerator.Content == null || activeGenerator.Content.Count < 1)
                        GUI.contentColor = erredColor;
                    else
                        GUI.contentColor = normalColor;

                    EditorGUILayout.PrefixLabel("Content");

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        GUI.FocusControl(null);
                        activeGenerator.Content.Add(new InventoryItemPointerCount() { Count = 1 });
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < activeGenerator.Content.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var value = activeGenerator.Content[i];

                        if (value.Count < 1)
                            GUI.contentColor = erredColor;
                        else
                            GUI.contentColor = normalColor;

                        value.Count = (uint)EditorGUILayout.IntField((int)value.Count, GUILayout.Width(50));

                        string toolTipText = "A reference to an Item, Bundle or Generator to be included in this Generator";

                        if (value.Item == null)
                        {
                            toolTipText = "You must provide a valid referece; you can use an Item, Bundle or Generator";
                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        }
                        else if (value.Item == activeGenerator as InventoryItemPointer)
                        {
                            toolTipText = "Self reference ... you must not reference the parent item in the content of a generator";
                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        }
                        else if (ItemIdCount(value.Item.DefinitionID.m_SteamItemDef) != 1)
                        {
                            toolTipText = "Insure this object is referenced (appears in the Item, Generator or Bundle list) and that it's ID is unique.";
                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        }
                        else
                            GUI.contentColor = color;

                        value.Item = (InventoryItemPointer)EditorGUILayout.ObjectField(new GUIContent(pointerIcon, toolTipText), value.Item, typeof(InventoryItemPointer), false);

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            activeGenerator.Content.RemoveAt(i);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    DrawItemPointerAttributes(activeGenerator);

                    EditorGUILayout.EndScrollView();
                }
                else
                    activeGenerator = null;
            }
            else if (activeTag != null)
            {
                if (settings.TagGenerators.Contains(activeTag))
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField(activeTag.name, EditorStyles.whiteLabel);

                    showTips = EditorGUILayout.Toggle("Show Tips", showTips);

                    EditorGUILayout.EndHorizontal();

                    var color = GUI.contentColor;

                    editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos);

                    #region Item ID
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.PrefixLabel("ID");

                    if (!IsItemIdUnique(activeTag.DefinitionID.m_SteamItemDef))
                        GUI.contentColor = erredColor;
                    else
                        GUI.contentColor = normalColor;

                    activeTag.DefinitionID.m_SteamItemDef = EditorGUILayout.IntField(activeTag.DefinitionID.m_SteamItemDef);

                    GUI.contentColor = normalColor;
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    #region Tags
                    if (showTips)
                    {
                        EditorGUILayout.HelpBox("Establish the possible tag values and value weights assoceated with a particular tag value. This is used to procedurally establish tags according to a perscribed probability.", MessageType.Info);
                    }
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.PrefixLabel("Tag Values");

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        GUI.FocusControl(null);
                        activeTag.TagValues.Add(new TagGeneratorValue { name = "New Tag Value", weight = 1 });
                        return;
                    }

                    activeTag.TagName = EditorGUILayout.TextField("Tag Name", activeTag.TagName);

                    EditorGUILayout.EndHorizontal();
                    for (int i = 0; i < activeTag.TagValues.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var value = activeTag.TagValues[i];
                        value.weight = EditorGUILayout.IntField("Weight", (int)value.weight);
                        value.name = EditorGUILayout.TextField("Value Name", value.name);

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            activeTag.TagValues.RemoveAt(i);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    EditorGUILayout.EndScrollView();
                }
                else
                    activeTag = null;
            }
            else if (activeBundle != null)
            {
                if (settings.ItemBundles.Contains(activeBundle))
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField(activeBundle.name, EditorStyles.whiteLabel);

                    showTips = EditorGUILayout.Toggle("Show Tips", showTips);

                    if (GUILayout.Button("Add Empty", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);
                        activeBundle.ValveItemDefAttributes.Add(new ValveItemDefAttribute());
                    }
                    if (GUILayout.Button("Add All", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);

                        if (activeBundle.ValveItemDefAttributes == null)
                            activeBundle.ValveItemDefAttributes = new List<ValveItemDefAttribute>();

                        for (int i = 0; i < 30; i++)
                        {
                            if (!activeBundle.ValveItemDefAttributes.Any(p => (int)p.attribute == i))
                            {
                                activeBundle.ValveItemDefAttributes.Add(new ValveItemDefAttribute()
                                {
                                    attribute = (ValveItemDefSchemaAttributes)i
                                });
                            }
                        }
                    }
                    if (GUILayout.Button("Add Common", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);

                        for (int i = 0; i < 30; i++)
                        {
                            if (i == 2 || i == 3 || i == 4 || i == 5 || i == 6 || i == 7 || i == 8 || i == 14 || i == 16 || i == 17 || i == 18
                                || i == 19 || i == 20 || i == 21 || i == 22 || i == 23 || i == 24 || i == 25 || i == 26 || i == 27 || i == 28)
                            {
                                continue;
                            }

                            if (!activeBundle.ValveItemDefAttributes.Any(p => (int)p.attribute == i))
                            {
                                activeBundle.ValveItemDefAttributes.Add(new ValveItemDefAttribute()
                                {
                                    attribute = (ValveItemDefSchemaAttributes)i
                                });
                            }
                        }
                    }
                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    {
                        GUI.FocusControl(null);

                        activeBundle.ValveItemDefAttributes.Clear();
                    }
                    EditorGUILayout.EndHorizontal();

                    editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos);

                    var color = GUI.contentColor;

                    #region Item ID
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.PrefixLabel("ID");

                    if (!IsItemIdUnique(activeBundle.DefinitionID.m_SteamItemDef))
                        GUI.contentColor = erredColor;
                    else
                        GUI.contentColor = normalColor;

                    activeBundle.DefinitionID.m_SteamItemDef = EditorGUILayout.IntField(activeBundle.DefinitionID.m_SteamItemDef);

                    GUI.contentColor = normalColor;

                    EditorGUILayout.EndHorizontal();
                    #endregion

                    #region Exchange
                    if (showTips)
                    {
                        EditorGUILayout.HelpBox("Each recipie defines a collection and quantity of items that can be exchanged for 1 of this item.\nRecipies are Scriptable Objects, right click and select Create > Steamworks > Player Services > Crafting Recipie to create a new one.", MessageType.Info);
                    }
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.PrefixLabel("Exchange");

                    if (activeBundle.Recipes == null)
                        activeBundle.Recipes = new List<CraftingRecipe>();

                    EditorGUILayout.BeginVertical();
                    RecipieDropAreaGUI("... Drop Recipies Here ...", activeBundle);

                    for (int i = 0; i < activeBundle.Recipes.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var data = activeBundle.Recipes[i];

                        if (!ValidateRecipie(data))
                            GUI.contentColor = erredColor;
                        else
                            GUI.contentColor = normalColor;

                        activeBundle.Recipes[i] = EditorGUILayout.ObjectField(data, typeof(CraftingRecipe), false) as CraftingRecipe;

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            activeBundle.Recipes.RemoveAt(i);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    #region Content
                    if (showTips)
                    {
                        EditorGUILayout.HelpBox("Lists the items this bundle contains.", MessageType.Info);
                    }
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    if (activeBundle.Content == null)
                        activeBundle.Content = new List<InventoryItemPointerCount>();

                    if (activeBundle.Content == null || activeBundle.Content.Count < 1)
                        GUI.contentColor = erredColor;
                    else
                        GUI.contentColor = normalColor;

                    EditorGUILayout.PrefixLabel("Content");

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        GUI.FocusControl(null);
                        activeBundle.Content.Add(new InventoryItemPointerCount() { Count = 1 });
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < activeBundle.Content.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var value = activeBundle.Content[i];

                        if (value.Count < 1)
                            GUI.contentColor = erredColor;
                        else
                            GUI.contentColor = normalColor;

                        value.Count = (uint)EditorGUILayout.IntField((int)value.Count, GUILayout.Width(50));


                        string toolTipText = "A reference to an Item, Bundle or Generator to be included in this Bundle";

                        if (value.Item == null)
                        {
                            toolTipText = "You must provide a valid referece; you can use an Item, Bundle or Generator";
                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        }
                        else if (value.Item == activeBundle as InventoryItemPointer)
                        {
                            toolTipText = "Self reference ... you must not reference the parent item in the content of a bundle";
                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        }
                        else if (ItemIdCount(value.Item.DefinitionID.m_SteamItemDef) != 1)
                        {
                            toolTipText = "Insure this object is referenced (appears in the Item, Generator or Bundle list) and that it's ID is unique.";
                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        }
                        else
                            GUI.contentColor = color;

                        value.Item = (InventoryItemPointer)EditorGUILayout.ObjectField(new GUIContent(pointerIcon, toolTipText), value.Item, typeof(InventoryItemPointer), false);

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            activeBundle.Content.RemoveAt(i);
                            EditorUtility.SetDirty(activeBundle);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    #endregion

                    DrawItemPointerAttributes(activeBundle);

                    EditorGUILayout.EndScrollView();
                }
                else
                    activeBundle = null;
            }
            else if (activeRecipie != null)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(activeRecipie.name, EditorStyles.whiteLabel);

                showTips = EditorGUILayout.Toggle("Show Tips", showTips);

                EditorGUILayout.EndHorizontal();

                editorScrollPos = EditorGUILayout.BeginScrollView(editorScrollPos);

                var color = GUI.contentColor;

                #region Content
                if (showTips)
                {
                    EditorGUILayout.HelpBox("Lists the items and quantities required for this recipie.\nYou will then attach the recipie to one or more items.", MessageType.Info);
                }
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.PrefixLabel("Ingredients");

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    activeRecipie.Items.Add(new InventoryItemDefinitionCount { Count = 1 });
                    EditorUtility.SetDirty(activeRecipie);
                    return;
                }
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < activeRecipie.Items.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    var value = activeRecipie.Items[i];

                    if (value.Count < 1)
                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                    else
                        GUI.contentColor = color;

                    var Count = (uint)EditorGUILayout.IntField((int)value.Count, GUILayout.Width(50));

                    if(value.Count != Count)
                    {
                        value.Count = Count;
                        EditorUtility.SetDirty(activeRecipie);
                    }

                    if (value.Item == null
                        || !settings.ItemDefinitions.Contains(value.Item))
                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                    else
                        GUI.contentColor = color;

                    var Item = (InventoryItemDefinition)EditorGUILayout.ObjectField(value.Item, typeof(InventoryItemDefinition), false);

                    if(Item != value.Item)
                    {
                        value.Item = Item;
                        EditorUtility.SetDirty(activeRecipie);
                    }

                    GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                    if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        GUI.FocusControl(null);
                        activeRecipie.Items.RemoveAt(i);
                        EditorUtility.SetDirty(activeRecipie);
                        return;
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                #endregion

                EditorGUILayout.EndScrollView();
            }


            EditorGUILayout.EndVertical();
        }

        private void DrawSettingsArea()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Settings:", GUILayout.Width(100));

            if (steamSettings == null)
            {
                var steamPath = PlayerPrefs.GetString(steamSettingsPath);
                if (!string.IsNullOrEmpty(steamPath))
                {
                    try
                    {
                        steamSettings = (SteamSettings)AssetDatabase.LoadAssetAtPath(steamPath, typeof(SteamSettings));
                    }
                    catch { }
                }
            }

            if (settings == null)
            {
                var currentPath = PlayerPrefs.GetString(settingsPath);
                if (!string.IsNullOrEmpty(currentPath))
                {
                    try
                    {
                        settings = (SteamworksInventorySettings)AssetDatabase.LoadAssetAtPath(currentPath, typeof(SteamworksInventorySettings));
                    }
                    catch { }
                }
            }

            EditorGUILayout.BeginHorizontal();

            steamSettings = EditorGUILayout.ObjectField(GUIContent.none, steamSettings, typeof(SteamSettings), false, GUILayout.Width(250)) as SteamSettings;

            var path = AssetDatabase.GetAssetPath(steamSettings);
            PlayerPrefs.SetString(steamSettingsPath, path);

            settings = EditorGUILayout.ObjectField(GUIContent.none, settings, typeof(SteamworksInventorySettings), false, GUILayout.Width(250)) as SteamworksInventorySettings;

            path = AssetDatabase.GetAssetPath(settings);
            PlayerPrefs.SetString(settingsPath, path);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            var bgColor = GUI.backgroundColor;
            GUI.backgroundColor = isValidModel ? Color.green : Color.red;
            if (GUILayout.Button("Generate JSON", EditorStyles.toolbarButton, GUILayout.Width(100)) && isValidModel)
            {
                GUI.FocusControl(null);

                StringBuilder sb = new StringBuilder();
                sb.Append("{\n\t\"appid\": ");
                sb.Append(steamSettings.applicationId.m_AppId);
                sb.Append(",\n\t\"items\": [");

                foreach (var item in settings.ItemDefinitions)
                {
                    sb.Append("\n\t{\n\t\t\"itemdefid\": ");
                    sb.Append(item.DefinitionID.m_SteamItemDef);
                    sb.Append(",");
                    sb.Append("\n\t\t\"type\": \"item\"");
                    foreach (var attribute in item.ValveItemDefAttributes)
                    {
                        sb.Append(",");
                        sb.Append("\n\t\t");
                        sb.Append(attribute.ToString());
                    }

                    if (item.Recipes != null && item.Recipes.Count > 0)
                    {
                        sb.Append(",\n\t\t\"exchange\": ");
                        var vals = new StringBuilder("\"");
                        foreach (var rec in item.Recipes)
                        {
                            vals.Append(rec.ToString());
                            vals.Append(";");
                        }
                        vals.Remove(vals.Length - 1, 1);
                        vals.Append("\"");
                        sb.Append(vals);
                    }

                    sb.Append("\n\t},");
                }

                foreach (var item in settings.ItemGenerators)
                {
                    sb.Append("\n\t{\n\t\t\"itemdefid\": ");
                    sb.Append(item.DefinitionID.m_SteamItemDef);
                    sb.Append(",");
                    sb.Append("\n\t\t\"type\": \"generator\"");
                    foreach (var attribute in item.ValveItemDefAttributes)
                    {
                        sb.Append(",");
                        sb.Append("\n\t\t");
                        sb.Append(attribute.ToString());
                    }

                    if (item.Recipes != null && item.Recipes.Count > 0)
                    {
                        sb.Append(",\n\t\t\"exchange\": ");
                        var vals = new StringBuilder("\"");
                        foreach (var rec in item.Recipes)
                        {
                            vals.Append(rec.ToString());
                            vals.Append(";");
                        }
                        vals.Remove(vals.Length - 1, 1);
                        vals.Append("\"");
                        sb.Append(vals);
                    }

                    if (item.Content != null && item.Content.Count > 0)
                    {
                        sb.Append(",\n\t\t\"bundle\": ");
                        var vals = new StringBuilder("\"");
                        foreach (var rec in item.Content)
                        {
                            vals.Append(rec.ToString());
                            vals.Append(";");
                        }
                        vals.Remove(vals.Length - 1, 1);
                        vals.Append("\"");
                        sb.Append(vals);
                    }

                    sb.Append("\n\t},");
                }

                foreach (var item in settings.ItemBundles)
                {
                    sb.Append("\n\t{\n\t\t\"itemdefid\": ");
                    sb.Append(item.DefinitionID.m_SteamItemDef);
                    sb.Append(",");
                    sb.Append("\n\t\t\"type\": \"bundle\"");
                    foreach (var attribute in item.ValveItemDefAttributes)
                    {
                        sb.Append(",");
                        sb.Append("\n\t\t");
                        sb.Append(attribute.ToString());
                    }

                    if (item.Recipes != null && item.Recipes.Count > 0)
                    {
                        sb.Append(",\n\t\t\"exchange\": ");
                        var vals = new StringBuilder("\"");
                        foreach (var rec in item.Recipes)
                        {
                            vals.Append(rec.ToString());
                            vals.Append(";");
                        }
                        vals.Remove(vals.Length - 1, 1);
                        vals.Append("\"");
                        sb.Append(vals);
                    }

                    if (item.Content != null && item.Content.Count > 0)
                    {
                        sb.Append(",\n\t\t\"bundle\": ");
                        var vals = new StringBuilder("\"");
                        foreach (var rec in item.Content)
                        {
                            vals.Append(rec.ToString());
                            vals.Append(";");
                        }
                        vals.Remove(vals.Length - 1, 1);
                        vals.Append("\"");
                        sb.Append(vals);
                    }

                    sb.Append("\n\t},");
                }

                foreach (var item in settings.TagGenerators)
                {
                    sb.Append("\n\t{\n\t\t\"itemdefid\": ");
                    sb.Append(item.DefinitionID.m_SteamItemDef);
                    sb.Append(",");
                    sb.Append("\n\t\t\"type\": \"tag_generator\",");
                    sb.Append("\n\t\t\"name\": \"" + item.name + "\",");
                    sb.Append("\n\t\t\"tag_generator_name\": \"" + item.TagName + "\"");
                    if (item.TagValues != null && item.TagValues.Count > 0)
                    {
                        sb.Append(",\n\t\t\"tag_generator_values\": \"");
                        var vals = new StringBuilder();
                        foreach (var rec in item.TagValues)
                        {
                            vals.Append(rec.ToString());
                            vals.Append(";");
                        }
                        vals.Remove(vals.Length - 1, 1);
                        sb.Append(vals);
                    }

                    sb.Append("\"\n\t},");
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append("\n\t]\n}");

                var targetPath = PlayerPrefs.GetString(settingsPath);
                targetPath = targetPath.Replace(settings.name + ".asset", "InventoryItemDefinition.JSON");
                File.WriteAllText("./" + targetPath, sb.ToString());

                var target = (TextAsset)AssetDatabase.LoadAssetAtPath(targetPath, typeof(TextAsset));
                AssetDatabase.Refresh();
                target = (TextAsset)AssetDatabase.LoadAssetAtPath(targetPath, typeof(TextAsset));
                EditorGUIUtility.PingObject(target);
                Selection.activeObject = target;
                Debug.Log("Wrote new JSON export to:\n./" + targetPath);
            }
            GUI.backgroundColor = bgColor;
            EditorGUILayout.EndHorizontal();
            PlayerPrefs.Save();
        }

        private void DrawItemList()
        {
            isValidModel = settings.ItemDefinitions != null && settings.ItemDefinitions.Count > 0;
            var bgColor = GUI.backgroundColor;
            var cColor = GUI.contentColor;

            var rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300), GUILayout.ExpandHeight(true));
            listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);
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


                DrawDefinitionList();

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

                DrawBundleList();

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

                DrawGeneratorList();

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

                DrawTagList();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("Recipes", EditorStyles.whiteLabel, GUILayout.Width(50));
                EditorGUILayout.LabelField("(used in an Item, Bundle or Generator)", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                DrawRecipeList();
            }
            EditorGUILayout.EndScrollView();
            GeneralDropAreaGUI("... Drop Items of any type here to add them ...");
            EditorGUILayout.EndVertical();
        }

        private bool RecipieDropAreaGUI(string message, InventoryItemPointer target)
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, "", EditorStyles.helpBox);

            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = dropBoxTexture;
            style.normal.textColor = Color.white;
            style.border = new RectOffset(5, 5, 5, 5);
            var color = GUI.backgroundColor;
            var fontColor = GUI.contentColor;
            GUI.backgroundColor = SteamUtilities.Colors.SteamGreen * SteamUtilities.Colors.HalfAlpha;
            GUI.contentColor = SteamUtilities.Colors.BrightGreen;
            GUI.Box(drop_area, "\n" + message, style);
            GUI.backgroundColor = color;
            GUI.contentColor = fontColor;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return false;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            // Do On Drag Stuff here
                            if (dragged_object.GetType().IsAssignableFrom(typeof(CraftingRecipe)))
                            {
                                CraftingRecipe go = dragged_object as CraftingRecipe;
                                if (go != null)
                                {
                                    if (!target.Recipes.Exists(p => p == go))
                                    {
                                        target.Recipes.Add(go);
                                        EditorUtility.SetDirty(settings);
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

        private void DrawDefinitionList()
        {
            var bgColor = GUI.backgroundColor;
            var contentColor = GUI.contentColor;
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < settings.ItemDefinitions.Count; i++)
            {
                var item = settings.ItemDefinitions[i];

                if (item.Recipes == null)
                    item.Recipes = new List<CraftingRecipe>();

                item.Recipes.RemoveAll(p => p == null);

                if (!ValidateItemPointer(item))
                {
                    isValidModel = false;
                    GUI.backgroundColor = SteamUtilities.Colors.ErrorRed;
                }
                else
                    GUI.backgroundColor = bgColor;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

                if (GUILayout.Button(itemIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                    Selection.activeObject = item;
                    activeItem = item;
                    activeGenerator = null;
                    activeTag = null;
                    activeBundle = null;
                    activeRecipie = null;
                }


                if (GUILayout.Toggle(activeItem == item, item.name, EditorStyles.toolbarButton))
                {
                    activeItem = item;
                    activeGenerator = null;
                    activeTag = null;
                    activeBundle = null;
                    activeRecipie = null;
                }

                var color = GUI.contentColor;
                GUI.contentColor = SteamUtilities.Colors.ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    settings.ItemDefinitions.RemoveAt(i);
                    EditorUtility.SetDirty(settings);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;
            GUI.backgroundColor = bgColor;
        }

        private void DrawBundleList()
        {
            var bgColor = GUI.backgroundColor;
            var contentColor = GUI.contentColor;
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < settings.ItemBundles.Count; i++)
            {
                var item = settings.ItemBundles[i];

                if (item.Recipes == null)
                    item.Recipes = new List<CraftingRecipe>();

                item.Recipes.RemoveAll(p => p == null);

                if (!ValidateItemPointer(item))
                {
                    isValidModel = false;
                    GUI.backgroundColor = SteamUtilities.Colors.ErrorRed;
                }
                else
                    GUI.backgroundColor = bgColor;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

                if (GUILayout.Button(bundleIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                    Selection.activeObject = item;
                    activeItem = null;
                    activeGenerator = null;
                    activeTag = null;
                    activeBundle = item;
                    activeRecipie = null;
                }

                if (GUILayout.Toggle(activeBundle == item, item.name, EditorStyles.toolbarButton))
                {
                    activeItem = null;
                    activeGenerator = null;
                    activeTag = null;
                    activeBundle = item;
                    activeRecipie = null;
                }

                var color = GUI.contentColor;
                GUI.contentColor = SteamUtilities.Colors.ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    settings.ItemBundles.RemoveAt(i);
                    EditorUtility.SetDirty(settings);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;

            GUI.backgroundColor = bgColor;
        }

        private void DrawGeneratorList()
        {
            var bgColor = GUI.backgroundColor;
            var erredColor = new Color(1, 0.5f, 0.5f, 1);
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < settings.ItemGenerators.Count; i++)
            {
                var item = settings.ItemGenerators[i];

                if (item.Recipes == null)
                    item.Recipes = new List<CraftingRecipe>();

                item.Recipes.RemoveAll(p => p == null);

                if (!ValidateItemPointer(item))
                {
                    isValidModel = false;
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
                    activeItem = null;
                    activeGenerator = item;
                    activeTag = null;
                    activeBundle = null;
                    activeRecipie = null;
                }
                if (GUILayout.Toggle(activeGenerator == item, item.name, EditorStyles.toolbarButton))
                {
                    activeItem = null;
                    activeGenerator = item;
                    activeTag = null;
                    activeBundle = null;
                    activeRecipie = null;
                }

                var color = GUI.contentColor;
                GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    settings.ItemGenerators.RemoveAt(i);
                    EditorUtility.SetDirty(settings);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;

            GUI.backgroundColor = bgColor;
        }

        private void DrawTagList()
        {
            var bgColor = GUI.backgroundColor;
            var erredColor = new Color(1, 0.5f, 0.5f, 1);
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < settings.TagGenerators.Count; i++)
            {
                var item = settings.TagGenerators[i];

                if (!ValidateTagGenerator(item))
                {
                    isValidModel = false;
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
                    activeItem = null;
                    activeGenerator = null;
                    activeTag = item;
                    activeBundle = null;
                    activeRecipie = null;
                }
                if (GUILayout.Toggle(activeTag == item, item.name, EditorStyles.toolbarButton))
                {
                    activeItem = null;
                    activeGenerator = null;
                    activeTag = item;
                    activeBundle = null;
                    activeRecipie = null;
                }

                var color = GUI.contentColor;
                GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    settings.TagGenerators.RemoveAt(i);
                    EditorUtility.SetDirty(settings);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = bgColor;
            }
            EditorGUI.indentLevel = il;

        }

        private void DrawRecipeList()
        {
            var bgColor = GUI.backgroundColor;
            var erredColor = new Color(1, 0.5f, 0.5f, 1);
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            List<CraftingRecipe> usedRecipies = new List<CraftingRecipe>();
            foreach (var item in settings.ItemDefinitions)
            {
                if (item.Recipes == null)
                    item.Recipes = new List<CraftingRecipe>();

                foreach (var recipie in item.Recipes)
                {
                    if (!usedRecipies.Contains(recipie))
                        usedRecipies.Add(recipie);
                }
            }

            foreach (var item in settings.ItemGenerators)
            {
                if (item.Recipes == null)
                    item.Recipes = new List<CraftingRecipe>();

                foreach (var recipie in item.Recipes)
                {
                    if (!usedRecipies.Contains(recipie))
                        usedRecipies.Add(recipie);
                }
            }

            foreach (var item in settings.ItemBundles)
            {
                if (item.Recipes == null)
                    item.Recipes = new List<CraftingRecipe>();

                foreach (var recipie in item.Recipes)
                {
                    if (!usedRecipies.Contains(recipie))
                        usedRecipies.Add(recipie);
                }
            }

            for (int i = 0; i < usedRecipies.Count; i++)
            {
                var recipe = usedRecipies[i];

                if (!ValidateRecipie(recipe))
                    GUI.backgroundColor = erredColor;
                else
                    GUI.backgroundColor = bgColor;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(recipeIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(recipe);
                    Selection.activeObject = recipe;
                    activeItem = null;
                    activeGenerator = null;
                    activeTag = null;
                    activeBundle = null;
                    activeRecipie = recipe;
                }
                if (GUILayout.Toggle(activeGenerator == recipe, recipe.name, EditorStyles.toolbarButton))
                {
                    activeItem = null;
                    activeGenerator = null;
                    activeTag = null;
                    activeBundle = null;
                    activeRecipie = recipe;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;

            GUI.backgroundColor = bgColor;
        }

        private bool ValidateRecipie(CraftingRecipe recipe)
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

        private void DrawItemPointerAttributes(InventoryItemPointer pointer)
        {
            var color = GUI.contentColor;

            if (pointer.ValveItemDefAttributes == null)
            {
                pointer.ValveItemDefAttributes = new List<ValveItemDefAttribute>();
                EditorUtility.SetDirty(pointer);
            }

            foreach (var at in pointer.ValveItemDefAttributes)
            {
                int result = (int)at.attribute;

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                result = EditorGUILayout.Popup(result, AttributeTypes, popupStyle, GUILayout.Width(25));
                if (result != (int)at.attribute)
                {
                    at.attribute = (ValveItemDefSchemaAttributes)result;
                    EditorUtility.SetDirty(pointer);
                }
                EditorGUILayout.LabelField(AttributeTypes[result], GUILayout.Width(200));

                switch (at.attribute)
                {
                    case ValveItemDefSchemaAttributes.background_color:
                    case ValveItemDefSchemaAttributes.name_color:

                        var colorValue = EditorGUILayout.ColorField(GUIContent.none, at.colorValue, true, false, false);
                        if (colorValue != at.colorValue)
                        {
                            at.colorValue = colorValue;
                            EditorUtility.SetDirty(pointer);
                        }

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;

                        break;
                    case ValveItemDefSchemaAttributes.description:
                    case ValveItemDefSchemaAttributes.name:
                    case ValveItemDefSchemaAttributes.display_type:
                        EditorGUILayout.BeginVertical();
                        if (showTips)
                        {
                            EditorGUILayout.HelpBox("This attribute can be listed multiple times for each language.\nAlternativly you can select None as the langauge in which case it will be assumed to be the applications defualt langauge.", MessageType.Info);
                        }
                        EditorGUILayout.BeginHorizontal();
                        result = (int)at.language;
                        result = EditorGUILayout.Popup(result, AttributeLanguage, GUILayout.Width(200));
                        if (result != (int)at.language)
                        {
                            at.language = (ValveItemDefLanguages)result;
                            EditorUtility.SetDirty(pointer);
                        }
                        var style = new GUIStyle(GUI.skin.textArea);
                        style.wordWrap = true;
                        var nStringValue = EditorGUILayout.TextArea(at.stringValue, style);
                        if (nStringValue != at.stringValue)
                        {
                            at.stringValue = nStringValue;
                            EditorUtility.SetDirty(pointer);
                        }

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    case ValveItemDefSchemaAttributes.icon_url:
                    case ValveItemDefSchemaAttributes.icon_url_large:
                    case ValveItemDefSchemaAttributes.item_slot:
                    case ValveItemDefSchemaAttributes.drop_start_time:
                        EditorGUILayout.BeginVertical();
                        if (showTips)
                        {
                            if (at.attribute == ValveItemDefSchemaAttributes.icon_url)
                            {
                                EditorGUILayout.HelpBox("This attribute should be a public facing URL. Valve recomends a resolution of 200x200", MessageType.Info);
                            }
                            else if (at.attribute == ValveItemDefSchemaAttributes.icon_url_large)
                            {
                                EditorGUILayout.HelpBox("This attribute should be a public facing URL. Valve recomends a resolution of 2048x2048", MessageType.Info);
                            }
                            else if (at.attribute == ValveItemDefSchemaAttributes.item_slot)
                            {
                                EditorGUILayout.HelpBox("Simple text expressing the use of the item e.g. Weapon, Hat, Powerup, etc.", MessageType.Info);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("UTC timestamp - prevent promo grants before this time, only applicable when promo = manual.\nExample: 20170801T120000Z\nExample represents 2017 08 01 12:00:00 UTC", MessageType.Info);
                            }
                        }
                        EditorGUILayout.BeginHorizontal();
                        var stringValue = EditorGUILayout.TextArea(at.stringValue);
                        if (stringValue != at.stringValue)
                        {
                            at.stringValue = stringValue;
                            EditorUtility.SetDirty(pointer);
                        }

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    case ValveItemDefSchemaAttributes.tradable:
                    case ValveItemDefSchemaAttributes.use_bundle_price:
                    case ValveItemDefSchemaAttributes.use_drop_limit:
                    case ValveItemDefSchemaAttributes.use_drop_window:
                    case ValveItemDefSchemaAttributes.hidden:
                    case ValveItemDefSchemaAttributes.store_hidden:
                    case ValveItemDefSchemaAttributes.marketable:
                    case ValveItemDefSchemaAttributes.granted_manually:
                        EditorGUILayout.BeginVertical();
                        if (showTips)
                        {
                            if (at.attribute == ValveItemDefSchemaAttributes.granted_manually)
                            {
                                EditorGUILayout.HelpBox("If set to true the item can only be granted manually and will not drop by time.", MessageType.Info);
                            }
                            else if (at.attribute == ValveItemDefSchemaAttributes.store_hidden)
                            {
                                EditorGUILayout.HelpBox("If true, this item will be hidden in the Steam Item Store for your app. By default, any items with a price will be shown in the store.", MessageType.Info);
                            }
                            else if (at.attribute == ValveItemDefSchemaAttributes.hidden)
                            {
                                EditorGUILayout.HelpBox("If true, the item definition will not be shown to clients. Use this to hide unused, or under-development, itemdefs.", MessageType.Info);
                            }
                        }
                        EditorGUILayout.BeginHorizontal();
                        var boolValue = EditorGUILayout.Toggle(at.boolValue);
                        if (boolValue != at.boolValue)
                        {
                            at.boolValue = boolValue;
                            EditorUtility.SetDirty(pointer);
                        }

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    case ValveItemDefSchemaAttributes.drop_interval:
                    case ValveItemDefSchemaAttributes.drop_limit:
                    case ValveItemDefSchemaAttributes.drop_max_per_winidow:
                    case ValveItemDefSchemaAttributes.drop_window:
                    case ValveItemDefSchemaAttributes.item_quality:
                    case ValveItemDefSchemaAttributes.purchase_bundle_discount:
                        EditorGUILayout.BeginVertical();
                        if (showTips)
                        {
                            if (at.attribute == ValveItemDefSchemaAttributes.drop_interval)
                            {
                                EditorGUILayout.HelpBox("Playtime in minutes before the item can be granted to the user.", MessageType.Info);
                            }
                            else if (at.attribute == ValveItemDefSchemaAttributes.drop_limit)
                            {
                                EditorGUILayout.HelpBox("Limits for a specific user the number of times this item will be dropped via ISteamInventory::TriggerItemDrop. Settings to zero will prevent any future drops of this item. ", MessageType.Info);
                            }
                            else if (at.attribute == ValveItemDefSchemaAttributes.drop_max_per_winidow)
                            {
                                EditorGUILayout.HelpBox("Numbers of grants within the window permitted before Cool-down applies. Default value is 1.", MessageType.Info);
                            }
                            else if (at.attribute == ValveItemDefSchemaAttributes.drop_window)
                            {
                                EditorGUILayout.HelpBox("Elapsed time in minutes of a cool-down window before we will grant an item.", MessageType.Info);
                            }
                            else if (at.attribute == ValveItemDefSchemaAttributes.item_quality)
                            {
                                EditorGUILayout.HelpBox("0- junk, 1- common, 2- uncommon, 3-rare, 4-[...]", MessageType.Info);
                            }
                            else if (at.attribute == ValveItemDefSchemaAttributes.purchase_bundle_discount)
                            {
                                EditorGUILayout.HelpBox("Not relivent for this item type.", MessageType.Info);
                            }
                        }
                        EditorGUILayout.BeginHorizontal();
                        var intValue = EditorGUILayout.IntField(at.intValue);
                        if (intValue != at.intValue)
                        {
                            at.intValue = intValue;
                            EditorUtility.SetDirty(pointer);
                        }

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    case ValveItemDefSchemaAttributes.store_images:
                    case ValveItemDefSchemaAttributes.store_tags:
                        EditorGUILayout.BeginVertical();
                        if (showTips)
                        {
                            if (at.attribute == ValveItemDefSchemaAttributes.store_images)
                            {
                                EditorGUILayout.HelpBox("Image URLs delimited by ';' character. These images will be proxied and used on the detail page of the Steam item store for your app.", MessageType.Info);
                            }
                            else if (at.attribute == ValveItemDefSchemaAttributes.store_tags)
                            {
                                EditorGUILayout.HelpBox("String with 'tags' delimited by ';' character. These tags will be used to categorize/filter items in the Steam item store for your app.", MessageType.Info);
                            }
                        }

                        if (at.stringArray == null)
                            at.stringArray = new List<string>();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            at.stringArray.Add(string.Empty);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        EditorGUILayout.EndHorizontal();
                        for (int i = 0; i < at.stringArray.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            string value = at.stringArray[i];

                            value = EditorGUILayout.TextField(value);
                            if (value != at.stringArray[i])
                            {
                                at.stringArray[i] = value;
                                EditorUtility.SetDirty(pointer);
                            }

                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                            {
                                GUI.FocusControl(null);
                                at.stringArray.RemoveAt(i);
                                EditorUtility.SetDirty(pointer);
                                return;
                            }
                            GUI.contentColor = color;
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    case ValveItemDefSchemaAttributes.price:
                        EditorGUILayout.BeginVertical();
                        if (showTips)
                        {
                            EditorGUILayout.HelpBox("Expresses the price of the item. This must be set up correctly for Start Purchase calls to work.\n\nNote that currency values are in the smallest unit applicable to that currency ... e.g. USD150 represents $1.50.\nAny currency not listed will be calcualted at checkout based on available currency listings ... e.g. currency conversion at the till.\n\nDo not use both Price and Price Category, pick one or the other.", MessageType.Info);
                        }

                        if (at.priceDataValue == null)
                        {
                            at.priceDataValue = new ValveItemDefPriceData();
                            EditorUtility.SetDirty(pointer);
                        }

                        if (at.priceDataValue.values == null)
                        {
                            at.priceDataValue.values = new List<ValveItemDefPriceDataEntry>();
                            EditorUtility.SetDirty(pointer);
                        }
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            at.priceDataValue.values.Add(new ValveItemDefPriceDataEntry() { currencyCode = "EUR", value = 100 });
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        at.priceDataValue.version = (uint)EditorGUILayout.IntField("Version: ", (int)at.priceDataValue.version);
                        EditorGUILayout.EndHorizontal();

                        for (int i = 0; i < at.priceDataValue.values.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            var data = at.priceDataValue.values[i];
                            if (string.IsNullOrEmpty(data.currencyCode) || !AttributeCurrencyCode.Contains(data.currencyCode))
                            {
                                data.currencyCode = "EUR";
                                EditorUtility.SetDirty(pointer);
                            }

                            var currencyIndex = Array.FindIndex(AttributeCurrencyCode, p => p == data.currencyCode);
                            currencyIndex = EditorGUILayout.Popup(currencyIndex, AttributeCurrencyName, GUILayout.Width(150));

                            if (data.currencyCode != AttributeCurrencyCode[currencyIndex])
                            {
                                data.currencyCode = AttributeCurrencyCode[currencyIndex];
                                EditorUtility.SetDirty(pointer);
                            }

                            data.value = (uint)EditorGUILayout.IntField((int)data.value);
                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                            {
                                GUI.FocusControl(null);
                                at.priceDataValue.values.RemoveAt(i);
                                EditorUtility.SetDirty(pointer);
                                return;
                            }
                            GUI.contentColor = color;
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    case ValveItemDefSchemaAttributes.price_category:
                        EditorGUILayout.BeginVertical();
                        if (showTips)
                        {
                            EditorGUILayout.HelpBox("Sets the price according to the Valve Look Up Value ... these are based on USD values and translate other currencies based on Valve exchange.\n\nDo not use both Price and Price Category, pick one or the other.", MessageType.Info);
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (at.priceCategoryValue == null)
                        {
                            at.priceCategoryValue = new ValveItemDefPriceCategory() { price = ValvePriceCategories.VLV100, version = 1 };
                            EditorUtility.SetDirty(pointer);
                        }

                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                        var version = (uint)EditorGUILayout.IntField("Version: ", (int)at.priceCategoryValue.version);
                        if (version != at.priceCategoryValue.version)
                        {
                            at.priceCategoryValue.version = version;
                            EditorUtility.SetDirty(pointer);
                        }

                        var vlv = (int)at.priceCategoryValue.price;
                        vlv = EditorGUILayout.Popup(vlv, AttributeVLV, GUILayout.Width(150));
                        if (vlv != (int)at.priceCategoryValue.price)
                        {
                            at.priceCategoryValue.price = (ValvePriceCategories)vlv;
                            EditorUtility.SetDirty(pointer);
                        }
                        EditorGUILayout.EndHorizontal();

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    case ValveItemDefSchemaAttributes.promo:
                        EditorGUILayout.BeginVertical();
                        if (showTips)
                        {
                            EditorGUILayout.HelpBox("Define the rules to be verified before this item can be granted as a promot item.\nNote that these rules when true will grant this item when GrantPromoItems is called. However if you set a rule of manual then the item must be granted explicitly e.g. GrantPromoItem(X) as opposed to the generic GrantPromoitems", MessageType.Info);
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (at.promoRulesValue == null)
                            at.promoRulesValue = new List<ValveItemDefPromoRule>();

                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            at.promoRulesValue.Add(new ValveItemDefPromoRule() { type = ValveItemDefPromoRuleType.played, app = new Steamworks.AppId_t(480), minutes = 15 });
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        EditorGUILayout.EndHorizontal();

                        for (int i = 0; i < at.promoRulesValue.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            var rule = at.promoRulesValue[i];
                            var ruleType = (int)rule.type;
                            ruleType = EditorGUILayout.Popup(ruleType, AttributeRuleTypes, GUILayout.Width(150));

                            if (ruleType != (int)rule.type)
                            {
                                rule.type = (ValveItemDefPromoRuleType)ruleType;
                                EditorUtility.SetDirty(pointer);
                            }

                            uint m_AppId;
                            uint minutes;
                            SteamAchievementData achievment;

                            switch (rule.type)
                            {
                                case ValveItemDefPromoRuleType.owns:
                                    m_AppId = (uint)EditorGUILayout.IntField("App ID", (int)rule.app.m_AppId);

                                    if (rule.app.m_AppId == m_AppId)
                                    {
                                        EditorUtility.SetDirty(pointer);
                                    }
                                    break;
                                case ValveItemDefPromoRuleType.achievement:
                                    achievment = (SteamAchievementData)EditorGUILayout.ObjectField(rule.achievment, typeof(SteamAchievementData), false);

                                    if (rule.achievment == achievment)
                                    {
                                        EditorUtility.SetDirty(pointer);
                                    }
                                    break;
                                case ValveItemDefPromoRuleType.played:
                                    m_AppId = (uint)EditorGUILayout.IntField("App ID", (int)rule.app.m_AppId);
                                    minutes = (uint)EditorGUILayout.IntField("Minutes Played", (int)rule.minutes);

                                    if (rule.app.m_AppId == m_AppId || minutes != rule.minutes)
                                    {
                                        EditorUtility.SetDirty(pointer);
                                    }
                                    break;
                                case ValveItemDefPromoRuleType.manual:
                                    break;
                            }

                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                            {
                                GUI.FocusControl(null);
                                at.promoRulesValue.RemoveAt(i);
                                EditorUtility.SetDirty(pointer);
                                return;
                            }
                            GUI.contentColor = color;
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    case ValveItemDefSchemaAttributes.tags:
                        EditorGUILayout.BeginVertical();
                        if (showTips)
                        {
                            EditorGUILayout.HelpBox("Defines the tags applied to this item.", MessageType.Info);
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (at.inventoryTagValue == null)
                        {
                            at.inventoryTagValue = new List<ValveItemDefInventoryItemTag>();
                            EditorUtility.SetDirty(pointer);
                        }

                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            at.inventoryTagValue.Add(new ValveItemDefInventoryItemTag() { category = "Category", tag = "Tag" });
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        EditorGUILayout.EndHorizontal();

                        for (int i = 0; i < at.inventoryTagValue.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            var tag = at.inventoryTagValue[i];

                            var category = EditorGUILayout.TextField("Category:", tag.category);
                            var tagName = EditorGUILayout.TextField("Tag:", tag.tag);

                            if (tag.category != category || tag.tag != tagName)
                            {
                                EditorUtility.SetDirty(pointer);
                            }

                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                            {
                                GUI.FocusControl(null);
                                at.inventoryTagValue.RemoveAt(i);
                                EditorUtility.SetDirty(pointer);
                                return;
                            }
                            GUI.contentColor = color;
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                    case ValveItemDefSchemaAttributes.tag_generators:
                        EditorGUILayout.BeginVertical();
                        if (showTips)
                        {
                            EditorGUILayout.HelpBox("Lists the Tag Generators that should be resolved and applied to this item on instantiation.", MessageType.Info);
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (at.tagGeneratorValue == null)
                        {
                            at.tagGeneratorValue = new List<TagGeneratorDefinition>();
                            EditorUtility.SetDirty(pointer);
                        }

                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            at.tagGeneratorValue.Add(null);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        EditorGUILayout.EndHorizontal();

                        for (int i = 0; i < at.tagGeneratorValue.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            var tag = at.tagGeneratorValue[i];

                            tag = (TagGeneratorDefinition)EditorGUILayout.ObjectField(tag, typeof(TagGeneratorDefinition), false);
                            if (tag != at.tagGeneratorValue[i])
                            {
                                at.tagGeneratorValue[i] = tag;
                                EditorUtility.SetDirty(pointer);
                            }

                            GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                            {
                                GUI.FocusControl(null);
                                at.tagGeneratorValue.RemoveAt(i);
                                EditorUtility.SetDirty(pointer);
                                return;
                            }
                            GUI.contentColor = color;
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();

                        GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                        {
                            GUI.FocusControl(null);
                            pointer.ValveItemDefAttributes.Remove(at);
                            EditorUtility.SetDirty(pointer);
                            return;
                        }
                        GUI.contentColor = color;

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private bool GeneralDropAreaGUI(string message)
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

        T CreateAsset<T>(string name) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }

        bool ValidateTagGenerator(TagGeneratorDefinition pointer)
        {
            if (!IsItemIdUnique(pointer.DefinitionID.m_SteamItemDef))
            {
                return false;
            }
            else
                return true;
        }

        bool IsItemIdUnique(int Id)
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

        int ItemIdCount(int Id)
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

            return countedIds.Count;
        }

        bool ValidateItemPointer(InventoryItemPointer pointer)
        {
            var result = true;

            if (!IsItemIdUnique(pointer.DefinitionID.m_SteamItemDef))
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