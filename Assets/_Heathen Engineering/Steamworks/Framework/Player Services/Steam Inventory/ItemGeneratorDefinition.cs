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
    [CreateAssetMenu(menuName = "Steamworks/Player Services/Inventory Item Generator")]
    public class ItemGeneratorDefinition : InventoryItemPointer
    {
        public override InventoryItemType ItemType { get { return InventoryItemType.ItemGenerator; } }

        public List<InventoryItemPointerCount> Content;

        public void TriggerDrop(Action<bool, SteamItemDetails_t[]> callback)
        {
            if(!SteamworksPlayerInventory.TriggerItemDrop(DefinitionID, callback))
            {
                Debug.LogWarning("[ItemGeneratorDefinition.TriggerDrop] - Call failed.");
            }
        }

        public void TriggerDrop()
        {
            var result = SteamworksPlayerInventory.TriggerItemDrop(DefinitionID, (status, results) =>
            {
                if (!status)
                {
                    Debug.LogWarning("[ItemGeneratorDefinition.TriggerDrop] - Call returned an error status.");
                }
            });

            if(!result)
            {
                Debug.LogWarning("[ItemGeneratorDefinition.TriggerDrop] - Call failed.");
            }
        }
    }
}
#endif