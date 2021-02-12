#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// Used in <see cref="CraftingRecipe"/> to represent a specific item and quantity to be used.
    /// </summary>
    [Serializable]
    public class InventoryItemDefinitionCount
    {
        public InventoryItemDefinition Item;
        public uint Count;

        /// <summary>
        /// Gets the ExchangeItemCount record for this item optionally decreasing the detail quantities to match that which was consumed
        /// </summary>
        /// <param name="decriment">Should the detail quantities be decrimented to show the use of the items fetched</param>
        /// <returns>Null if insufficent quantity available</returns>
        public List<ExchangeItemCount> FetchFromItem(bool decriment)
        {
            return Item.FetchItemCount(Count, decriment);
        }

        public override string ToString()
        {
            return Item.DefinitionID.m_SteamItemDef + "x" + Count;
        }
    }
}
#endif