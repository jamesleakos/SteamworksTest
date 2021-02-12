#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// Defines the list of items and quantities required to create some item from some other group of items.
    /// </summary>
    [CreateAssetMenu(menuName = "Steamworks/Player Services/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
        /// <summary>
        /// The list of items and quantities required to create the item this recipie is related to.
        /// </summary>
        public List<InventoryItemDefinitionCount> Items;

        public override string ToString()
        {
            string value = "";
            foreach(var item in Items)
            {
                value += item.Item.DefinitionID.m_SteamItemDef.ToString() + "x" + item.Count.ToString() + ",";
            }
            return value.Remove(value.Length - 1, 1);
        }
    }
}
#endif