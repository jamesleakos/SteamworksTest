#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [CreateAssetMenu(menuName = "Steamworks/Player Services/Item Bundle")]
    public class InventoryItemBundleDefinition : InventoryItemPointer
    {
        public override InventoryItemType ItemType { get { return InventoryItemType.ItemBundle; } }

        public List<InventoryItemPointerCount> Content;

        /// <summary>
        /// <para>Instructs the Steam backend to start a purchase for this item and the quantity indicated</para>
        /// <para>Note that this process is tightly integrated with the item definition as configured on your Steam partner backend. It is keenly important that you have set up proper priceses for your times before this method will work correctly.</para>
        /// <para>If the purchase is successful e.g. if the user competes the purchase then a results ready message will be processed and handled by the Heathen Steam Inventory system updating the item instances and quantities available of the items purchased.</para>
        /// </summary>
        /// <param name="quantity"></param>
        public void StartPurchase(uint quantity)
        {
            SteamItemDef_t[] items = { DefinitionID };
            uint[] itemQuantity = { quantity };

            SteamInventory.StartPurchase(items, itemQuantity, 1);
        }
    }
}
#endif