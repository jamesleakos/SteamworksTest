#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [CreateAssetMenu(menuName = "Steamworks/Player Services/Inventory Tag Generator")]
    public class TagGeneratorDefinition : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
        /// <summary>
        /// This must match the definition ID set in Steam for this item
        /// </summary>
        public SteamItemDef_t DefinitionID;

        public string TagName;
        public List<TagGeneratorValue> TagValues;
    }
}
#endif