#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [Serializable]
    public class ValveItemDefAttribute
    {
        public ValveItemDefSchemaAttributes attribute;
        public ValveItemDefLanguages language = ValveItemDefLanguages.none;
        
        public string stringValue = "";
        public bool boolValue = false;
        public Color colorValue;
        public int intValue;
        public List<string> stringArray;
        //public List<ExchangeItemCount> exchangeArray;
        public ValveItemDefPriceData priceDataValue;
        public ValveItemDefPriceCategory priceCategoryValue;
        public List<ValveItemDefPromoRule> promoRulesValue;
        public List<ValveItemDefInventoryItemTag> inventoryTagValue;
        public List<TagGeneratorDefinition> tagGeneratorValue;

        public override string ToString()
        {
            switch (attribute)
            {
                case ValveItemDefSchemaAttributes.background_color:
                    var bgColor = colorValue;
                    if (bgColor != null)
                        return "\"background_color\": \"" + ColorUtility.ToHtmlStringRGB(bgColor) + "\"";
                    else
                        return string.Empty;
                case ValveItemDefSchemaAttributes.description:
                    return "\"description" + GetLanguageSuffix() + "\": \"" + stringValue.Replace("\n\r", "\\n").Replace("\n", "\\n").Replace("\r", "\\n") + "\"";
                case ValveItemDefSchemaAttributes.display_type:
                    return "\"display_type" + GetLanguageSuffix() + "\": \"" + stringValue.Replace("\n\r", "\\n").Replace("\n", "\\n").Replace("\r", "\\n") + "\"";
                case ValveItemDefSchemaAttributes.drop_interval:
                    return "\"drop_interval\": " + intValue.ToString();
                case ValveItemDefSchemaAttributes.drop_limit:
                    return "\"drop_limit\": " + intValue.ToString();
                case ValveItemDefSchemaAttributes.drop_max_per_winidow:
                    return "\"drop_max_per_window\": " + intValue.ToString();
                case ValveItemDefSchemaAttributes.drop_start_time:
                    return "\"drop_start_time\": \"" + stringValue + "\"";
                case ValveItemDefSchemaAttributes.drop_window:
                    return "\"drop_window\": " + intValue.ToString();
                case ValveItemDefSchemaAttributes.granted_manually:
                    return "\"granted_manually\": " + boolValue.ToString().ToLower();
                case ValveItemDefSchemaAttributes.hidden:
                    return "\"hidden\": " + boolValue.ToString().ToLower();
                case ValveItemDefSchemaAttributes.icon_url:
                    return "\"icon_url\": \"" + stringValue.Replace("\n\r", "").Replace("\n", "").Replace("\r", "") + "\"";
                case ValveItemDefSchemaAttributes.icon_url_large:
                    return "\"icon_url_large\": \"" + stringValue.Replace("\n\r", "").Replace("\n", "").Replace("\r", "") + "\"";
                case ValveItemDefSchemaAttributes.item_quality:
                    return "\"item_quality\": " + intValue;
                case ValveItemDefSchemaAttributes.item_slot:
                    return "\"item_slot\": \"" + stringValue + "\"";
                case ValveItemDefSchemaAttributes.marketable:
                    return "\"marketable\": " + boolValue.ToString().ToLower();
                case ValveItemDefSchemaAttributes.name:
                    return "\"name" + GetLanguageSuffix() + "\": \"" + stringValue.Replace("\n\r", "\\n").Replace("\n", "\\n").Replace("\r", "\\n") + "\"";
                case ValveItemDefSchemaAttributes.name_color:
                    var nColor = colorValue;
                    if (nColor != null)
                        return "\"name_color\": \"" + ColorUtility.ToHtmlStringRGB(nColor) + "\"";
                    else
                        return string.Empty;
                case ValveItemDefSchemaAttributes.price:
                    var pData = priceDataValue;
                    if (pData != null)
                        return "\"price\": \"" + pData.ToString() + "\"";
                    else
                        return string.Empty;
                case ValveItemDefSchemaAttributes.price_category:
                    var cData = priceCategoryValue;
                    if (cData != null)
                        return "\"price\": \"" + cData.ToString() + "\"";
                    else
                        return string.Empty;
                case ValveItemDefSchemaAttributes.promo:
                    var pRules = promoRulesValue;
                    if (pRules != null)
                    {
                        var pRuleSB = new StringBuilder("\"");
                        foreach (var r in pRules)
                        {
                            if (pRuleSB.Length > 1)
                                pRuleSB.Append(";");

                            pRuleSB.Append(r.ToString());
                        }
                        pRuleSB.Append("\"");

                        return "\"promo\": " + pRuleSB.ToString();
                    }
                    else
                        return string.Empty;
                case ValveItemDefSchemaAttributes.store_hidden:
                    return "\"store_hidden\": " + boolValue.ToString().ToLower();
                case ValveItemDefSchemaAttributes.store_images:
                    var sImages = stringArray;
                    if (sImages != null)
                    {
                        var imageList = new StringBuilder("\"");
                        foreach (var i in sImages)
                        {
                            if (imageList.Length > 1)
                                imageList.Append(";");

                            imageList.Append(i);
                        }
                        imageList.Append("\"");

                        return "\"store_images\": \"" + imageList.ToString() + "\"";
                    }
                    else
                        return string.Empty;
                case ValveItemDefSchemaAttributes.store_tags:
                    var sTags = stringArray;
                    if (sTags != null)
                    {
                        var tagList = new StringBuilder("\"");
                        foreach (var i in sTags)
                        {
                            if (tagList.Length > 1)
                                tagList.Append(";");

                            tagList.Append(i);
                        }
                        tagList.Append("\"");

                        return "\"store_tags\": " + tagList.ToString();
                    }
                    else
                        return string.Empty;
                case ValveItemDefSchemaAttributes.tags:
                    var rTags = inventoryTagValue;
                    if (rTags != null)
                    {
                        var tagList = new StringBuilder("\"");
                        foreach (var i in rTags)
                        {
                            if (tagList.Length > 1)
                                tagList.Append(";");

                            tagList.Append(i.ToString());
                        }
                        tagList.Append("\"");

                        return "\"tags\": " + tagList.ToString();
                    }
                    else
                        return string.Empty;
                case ValveItemDefSchemaAttributes.tag_generators:
                    var tagGenerators = tagGeneratorValue;
                    if (tagGenerators != null)
                    {
                        var sb = new StringBuilder("\"");
                        foreach (var generator in tagGenerators)
                        {
                            if (sb.Length > 1)
                                sb.Append(";");

                            sb.Append(generator.DefinitionID.m_SteamItemDef);
                        }
                        sb.Append("\"");

                        return "\"tag_generators\": " + sb.ToString();
                    }
                    else
                        return string.Empty;
                case ValveItemDefSchemaAttributes.tradable:
                    return "\"tradable\": " + boolValue.ToString().ToLower();
                case ValveItemDefSchemaAttributes.use_bundle_price:
                    return "\"use_bundle_price\": " + boolValue.ToString().ToLower();
                case ValveItemDefSchemaAttributes.use_drop_limit:
                    return "\"use_drop_limit\": " + boolValue.ToString().ToLower();
                case ValveItemDefSchemaAttributes.use_drop_window:
                    return "\"use_drop_limit\": " + boolValue.ToString().ToLower();
                case ValveItemDefSchemaAttributes.purchase_bundle_discount:
                    return "\"purchase_bundle_discount\": " + intValue.ToString();
                default:
                    return string.Empty;

            }
        }

        string GetLanguageSuffix()
        {
            switch (language)
            {
                case ValveItemDefLanguages.none:
                    return string.Empty;
                default:
                    return "_" + language.ToString();
            }
        }
    }
}
#endif