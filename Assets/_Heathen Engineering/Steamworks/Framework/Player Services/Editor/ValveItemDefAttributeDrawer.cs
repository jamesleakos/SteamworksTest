#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.PlayerServices;
using System;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    // IngredientDrawerUIE
    [CustomPropertyDrawer(typeof(ValveItemDefAttribute))]
    public class ValveItemDefAttributeDrawer : PropertyDrawer
    {
        private GUIStyle popupStyle;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty attribute = property.FindPropertyRelative("attribute");
            SerializedProperty language = property.FindPropertyRelative("language");

            ValveItemDefSchemaAttributes attributeValue = (ValveItemDefSchemaAttributes)Enum.Parse(typeof(ValveItemDefSchemaAttributes), attribute.enumNames[attribute.enumValueIndex]);

            switch (attributeValue)
            {
                case ValveItemDefSchemaAttributes.store_tags:
                    return EditorGUI.GetPropertyHeight(language) +
                    EditorGUI.GetPropertyHeight(property.FindPropertyRelative("stringArray"));
                //case ValveItemDefSchemaAttributes.bundle:
                //    return EditorGUI.GetPropertyHeight(language) +
                //    EditorGUI.GetPropertyHeight(property.FindPropertyRelative("exchangeArray"), GUIContent.none, true);
                case ValveItemDefSchemaAttributes.price:
                    return EditorGUI.GetPropertyHeight(language) +
                    EditorGUI.GetPropertyHeight(property.FindPropertyRelative("priceDataValue"), GUIContent.none, true);
                case ValveItemDefSchemaAttributes.price_category:
                    return EditorGUI.GetPropertyHeight(language) +
                    EditorGUI.GetPropertyHeight(property.FindPropertyRelative("priceCategoryValue"), GUIContent.none, true);
                case ValveItemDefSchemaAttributes.promo:
                    return EditorGUI.GetPropertyHeight(language) +
                    EditorGUI.GetPropertyHeight(property.FindPropertyRelative("promoRulesValue"), GUIContent.none, true);
                case ValveItemDefSchemaAttributes.tags:
                    return EditorGUI.GetPropertyHeight(language) +
                    EditorGUI.GetPropertyHeight(property.FindPropertyRelative("inventoryTagValue"), GUIContent.none, true);
                case ValveItemDefSchemaAttributes.tag_generators:
                    return EditorGUI.GetPropertyHeight(language) +
                    EditorGUI.GetPropertyHeight(property.FindPropertyRelative("tagGeneratorValue"), GUIContent.none, true);
                default:
                    return EditorGUI.GetPropertyHeight(language);
            }
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attributeType = property.FindPropertyRelative("attribute");
            var typeIndex = attributeType.enumValueIndex;
            var typeNames = attributeType.enumNames;
            var frinedNames = attributeType.enumDisplayNames;
            var language = property.FindPropertyRelative("language");

            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            ValveItemDefSchemaAttributes attribute = (ValveItemDefSchemaAttributes)Enum.Parse(typeof(ValveItemDefSchemaAttributes), typeNames[typeIndex]);

            EditorGUI.BeginProperty(position, GUIContent.none, property);
            
            var typeRect = new Rect(position.x + 15, position.y, popupStyle.fixedWidth + popupStyle.margin.right + 15, EditorGUI.GetPropertyHeight(language));

            int result = EditorGUI.Popup(typeRect, attributeType.enumValueIndex, attributeType.enumDisplayNames, popupStyle);
            attributeType.enumValueIndex = result;

            var labelRect = new Rect(typeRect.x + typeRect.width, position.y, Mathf.Max((frinedNames[typeIndex].Length * 8), 75), EditorGUI.GetPropertyHeight(language));
            // Draw label
            EditorGUI.LabelField(labelRect, new GUIContent(frinedNames[typeIndex]), GUIContent.none);

            // Don't make child fields be indented 
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            
            var languageRect = new Rect(labelRect.x + labelRect.width, position.y, 85, EditorGUI.GetPropertyHeight(language));
            var valueRect = new Rect(languageRect.x + languageRect.width + 10, position.y, position.width - (languageRect.x + languageRect.width), EditorGUI.GetPropertyHeight(language));
            var valueRectFull = new Rect(labelRect.x + labelRect.width + 10, position.y, position.width - (labelRect.x + labelRect.width), EditorGUI.GetPropertyHeight(language));
            var valueRectFullChild = new Rect(labelRect.x, position.y + languageRect.height, position.width - (labelRect.x), position.height - EditorGUI.GetPropertyHeight(language));

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            
            //EditorGUI.PropertyField(typeRect, property.FindPropertyRelative("attribute"), GUIContent.none);

            switch (attribute)
            {
                case ValveItemDefSchemaAttributes.background_color:
                case ValveItemDefSchemaAttributes.name_color:
                    EditorGUI.PropertyField(valueRectFull, property.FindPropertyRelative("colorValue"), GUIContent.none);
                    break;
                case ValveItemDefSchemaAttributes.description:
                case ValveItemDefSchemaAttributes.name:
                case ValveItemDefSchemaAttributes.display_type:
                    EditorGUI.PropertyField(languageRect, language, GUIContent.none);
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("stringValue"), GUIContent.none);
                    break;
                case ValveItemDefSchemaAttributes.icon_url:
                case ValveItemDefSchemaAttributes.icon_url_large:
                case ValveItemDefSchemaAttributes.item_slot:
                case ValveItemDefSchemaAttributes.drop_start_time:
                    EditorGUI.PropertyField(valueRectFull, property.FindPropertyRelative("stringValue"), GUIContent.none);
                    break;
                case ValveItemDefSchemaAttributes.tradable:
                case ValveItemDefSchemaAttributes.use_bundle_price:
                case ValveItemDefSchemaAttributes.use_drop_limit:
                case ValveItemDefSchemaAttributes.use_drop_window:
                case ValveItemDefSchemaAttributes.hidden:
                case ValveItemDefSchemaAttributes.store_hidden:
                case ValveItemDefSchemaAttributes.marketable:
                case ValveItemDefSchemaAttributes.granted_manually:
                    EditorGUI.PropertyField(valueRectFull, property.FindPropertyRelative("boolValue"), GUIContent.none);
                    break;
                case ValveItemDefSchemaAttributes.drop_interval:
                case ValveItemDefSchemaAttributes.drop_limit:
                case ValveItemDefSchemaAttributes.drop_max_per_winidow:
                case ValveItemDefSchemaAttributes.drop_window:
                case ValveItemDefSchemaAttributes.item_quality:
                    EditorGUI.PropertyField(valueRectFull, property.FindPropertyRelative("intValue"), GUIContent.none);
                    break;
                case ValveItemDefSchemaAttributes.store_images:
                case ValveItemDefSchemaAttributes.store_tags:
                    EditorGUI.PropertyField(valueRectFullChild, property.FindPropertyRelative("stringArray"), new GUIContent("Value"), true);
                    break;
                //case ValveItemDefSchemaAttributes.bundle:
                //    EditorGUI.PropertyField(valueRectFullChild, property.FindPropertyRelative("exchangeArray"), new GUIContent("Value"), true);
                //    break;
                case ValveItemDefSchemaAttributes.price:
                    EditorGUI.PropertyField(valueRectFullChild, property.FindPropertyRelative("priceDataValue"), new GUIContent("Value"), true);
                    break;
                case ValveItemDefSchemaAttributes.price_category:
                    EditorGUI.PropertyField(valueRectFullChild, property.FindPropertyRelative("priceCategoryValue"), new GUIContent("Value"), true);
                    break;
                case ValveItemDefSchemaAttributes.promo:
                    EditorGUI.PropertyField(valueRectFullChild, property.FindPropertyRelative("promoRulesValue"), new GUIContent("Value"), true);
                    break;
                case ValveItemDefSchemaAttributes.tags:
                    EditorGUI.PropertyField(valueRectFullChild, property.FindPropertyRelative("inventoryTagValue"), new GUIContent("Value"), true);
                    break;
                case ValveItemDefSchemaAttributes.tag_generators:
                    EditorGUI.PropertyField(valueRectFullChild, property.FindPropertyRelative("tagGeneratorValue"), new GUIContent("Value"), true);
                    break;
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
#endif