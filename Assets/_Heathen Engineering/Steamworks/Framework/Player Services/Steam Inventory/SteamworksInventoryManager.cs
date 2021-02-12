#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    public class SteamworksInventoryManager : MonoBehaviour
    {
        public SteamworksInventorySettings Settings;
        public bool RefreshOnStart = true;

        public UnityEvent ItemInstancesUpdated;
        public UnityItemDetailEvent ItemsGranted;
        public UnityItemDetailEvent ItemsConsumed;
        public UnityItemDetailEvent ItemsExchanged;
        public UnityItemDetailEvent ItemsDroped;

        private void OnEnable()
        {
            if(Settings == null)
            {
                Debug.LogWarning("Steamworks Inventory Manager requires a Steamworks Inventory Settings object to funciton!\nThis componenet will be disabled.");
                this.enabled = false;
                return;
            }

            Settings.Register();

            if (Settings.ItemInstancesUpdated == null)
                Settings.ItemInstancesUpdated = new UnityEvent();
            Settings.ItemInstancesUpdated.AddListener(ItemInstancesUpdated.Invoke);

            if (Settings.ItemsGranted == null)
                Settings.ItemsGranted = new UnityItemDetailEvent();
            Settings.ItemsGranted.AddListener(ItemsGranted.Invoke);

            if (Settings.ItemsConsumed == null)
                Settings.ItemsConsumed = new UnityItemDetailEvent();
            Settings.ItemsConsumed.AddListener(ItemsConsumed.Invoke);

            if (Settings.ItemsExchanged == null)
                Settings.ItemsExchanged = new UnityItemDetailEvent();
            Settings.ItemsExchanged.AddListener(ItemsExchanged.Invoke);

            if (Settings.ItemsDroped == null)
                Settings.ItemsDroped = new UnityItemDetailEvent();
            Settings.ItemsDroped.AddListener(ItemsDroped.Invoke);
        }

        private void OnDisable()
        {
            if (Settings == null)
                return;

            if (Settings.ItemInstancesUpdated != null)
                Settings.ItemInstancesUpdated.RemoveListener(ItemInstancesUpdated.Invoke);

            if (Settings.ItemsGranted != null)
                Settings.ItemsGranted.RemoveListener(ItemsGranted.Invoke);

            if (Settings.ItemsConsumed != null)
                Settings.ItemsConsumed.RemoveListener(ItemsConsumed.Invoke);

            if (Settings.ItemsExchanged != null)
                Settings.ItemsExchanged.RemoveListener(ItemsExchanged.Invoke);

            if (Settings.ItemsDroped != null)
                Settings.ItemsDroped.RemoveListener(ItemsDroped.Invoke);
        }

        private void Start()
        {
            if (Settings != null && RefreshOnStart)
            {
                Settings.ClearItemCounts();
                Settings.RefreshInventory();
            }
        }

        #region Wrapper
        #region Utilities
        public T GetDefinition<T>(SteamItemDetails_t steamDetail) where T : InventoryItemDefinition
        {
            return Settings.GetDefinition<T>(steamDetail);
        }

        public T GetDefinition<T>(SteamItemDef_t steamDefinition) where T : InventoryItemDefinition
        {
            return Settings.GetDefinition<T>(steamDefinition);
        }

        public InventoryItemDefinition GetDefinition(SteamItemDetails_t steamDetail)
        {
            return Settings.GetDefinition(steamDetail);
        }

        public InventoryItemDefinition GetDefinition(SteamItemDef_t steamDefinition)
        {
            return Settings.GetDefinition(steamDefinition);
        }

        public InventoryItemDefinition GetDefinition(int steamDefinition)
        {
            return Settings.GetDefinition(steamDefinition);
        }

        public InventoryItemDefinition this[SteamItemDetails_t item]
        {
            get
            {
                return GetDefinition(item);
            }
        }

        public InventoryItemDefinition this[SteamItemDef_t item]
        {
            get
            {
                return GetDefinition(item);
            }
        }

        public InventoryItemDefinition this[int itemId]
        {
            get
            {
                return GetDefinition(itemId);
            }
        }
        #endregion

        /// <summary>
        /// <para>Updates the InventoryItemDefinition.Instances list of each of the referenced Item Definitions with the results of a 'Get All Items' query against the current user's Steam Inventory.</para>
        /// <para>This will cause the Instances member of each item to reflect the current state of the users inventory.</para>
        /// <para> <para>
        /// <para>This will trigger the Item Instances Updated event after steam responds with the users inventory items and the items have been updated to reflect the correct instances.</para>
        /// </summary>
        public void RefreshInventory()
        {
            Settings.RefreshInventory();
        }

        /// <summary>
        /// <para>Grants the user all available promotional items</para>
        /// <para>This will trigger the Item Instances Updated event after steam responds with the users inventory items and the items have been updated to reflect the correct instances.</para>
        /// <para> <para>
        /// <para>NOTE: this additivly updates the Instance list for effected items and is not a clean refresh!
        /// Consider a call to Refresh Inventory to resolve a complete and accurate refresh of all player items.</para>
        /// </summary>
        public void GrantAllPromotionalItems()
        {
            Settings.GrantAllPromotionalItems();
        }

        /// <summary>
        /// <para>Grants the user a promotional item</para>
        /// <para>This will trigger the Item Instances Updated event after steam responds with the users inventory items and the items have been updated to reflect the correct instances.</para>
        /// <para> <para>
        /// <para>NOTE: this additivly updates the Instance list for effected items and is not a clean refresh!
        /// Consider a call to Refresh Inventory to resolve a complete and accurate refresh of all player items.</para>
        /// </summary>
        /// <paramref name="itemDefinition">The item type to grant to the user.</paramref>
        public void GrantPromotionalItem(InventoryItemDefinition itemDefinition)
        {
            Settings.GrantPromotionalItem(itemDefinition);
        }

        /// <summary>
        /// <para>Grants the user the promotional items indicated</para>
        /// <para>This will trigger the Item Instances Updated event after steam responds with the users inventory items and the items have been updated to reflect the correct instances.</para>
        /// <para>NOTE: this additivly updates the Instance list for effected items and is not a clean refresh!
        /// Consider a call to Refresh Inventory to resolve a complete and accurate refresh of all player items.</para>
        /// </summary>
        /// <param name="itemDefinitions">The list of items to be granted if available</param>
        public void GrantPromotionalItems(IEnumerable<InventoryItemDefinition> itemDefinitions)
        {
            Settings.GrantPromotionalItems(itemDefinitions);
        }

        /// <summary>
        /// <para>Determins if the result handle belongs to the user</para>
        /// </summary>
        /// <param name="resultHandle">The inventory result handle to check the user on</param>
        /// <param name="user">The user to check against</param>
        /// <returns>True if the result belongs to the target user; otherwise false</returns>
        public bool CheckUserResult(SteamInventoryResult_t resultHandle, ulong user)
        {
            return SteamworksPlayerInventory.CheckResultSteamID(resultHandle, user);
        }

        /// <summary>
        /// <para>Determins if the result handle belongs to the user</para>
        /// </summary>
        /// <param name="resultHandle">The inventory result handle to check the user on</param>
        /// <param name="user">The user to check against</param>
        /// <returns>True if the result belongs to the target user; otherwise false</returns>
        public bool CheckUserResult(SteamInventoryResult_t resultHandle, CSteamID user)
        {
            return SteamworksPlayerInventory.CheckResultSteamID(resultHandle, user);
        }

        /// <summary>
        /// <para>Determins if the result handle belongs to the user</para>
        /// </summary>
        /// <param name="resultHandle">The inventory result handle to check the user on</param>
        /// <param name="user">The user to check against</param>
        /// <returns>True if the result belongs to the target user; otherwise false</returns>
        public bool CheckUserResult(SteamInventoryResult_t resultHandle, SteamUserData user)
        {
            return SteamworksPlayerInventory.CheckResultSteamID(resultHandle, user);
        }

        /// <summary>
        /// Consumes a single unit of a single instnace of this item if available.
        /// </summary>
        /// <param name="itemDefinition"></param>
        public void ConsumeItem(InventoryItemDefinition itemDefinition)
        {
            Settings.ConsumeItem(itemDefinition);
        }

        /// <summary>
        /// <para>Attempts to consume the requested units of the indicated item</para>
        /// <para>NOTE: this may need to iterate over multiple instances and may need to send multiple consume requests any of which may fail and each of which will trigger an Item Instance Update event call.</para>
        /// <para>You are recomended to use the the SteamItemInstance_t overload of this method when consuming more than 1 unit of an item.</para>
        /// </summary>
        /// <param name="itemDefinition">The item to consume for</param>
        /// <param name="count">The number of item units to try and consume</param>
        public void ConsumeItem(InventoryItemDefinition itemDefinition, int count)
        {
            Settings.ConsumeItem(itemDefinition, count);
        }

        /// <summary>
        /// <para>Consumes the indicate number of units of this item from this specific instance stack</para>
        /// <para>NOTE: this is the most efficent way to consume multiple units of an item at a time.</para>
        /// </summary>
        /// <param name="itemDefinition"></param>
        /// <param name="instanceId"></param>
        /// <param name="count"></param>
        public void ConsumeItem(InventoryItemDefinition itemDefinition, SteamItemInstanceID_t instanceId, int count)
        {
            Settings.ConsumeItem(itemDefinition, instanceId, count);
        }

        /// <summary>
        /// <para>Attampts to consume the indicated number of items from the instance provided.</para>
        /// <para>Note this method must look up the instance's related Item Definition which can take time and can be error prone. It is recomended that you provide the ItemDefinition with your call to the consume method.</para>
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="count"></param>
        public void ConsumeItem(SteamItemInstanceID_t instanceId, int count)
        {
            Settings.ConsumeItem(instanceId, count);
        }

        /// <summary>
        /// <para>Exchange items for the indicated recipe</para>
        /// <para>NOTE: this method will trigger the Items Exchanged event and can optionally trigger a full inventory refresh on completion of the exchange.</para>
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="postExchangeRefresh"></param>
        public void ExchangeItems(InventoryItemDefinition itemToCraft, CraftingRecipe recipe)
        {
            itemToCraft.Craft(recipe);
        }

        public void ExchangeItems(InventoryItemDefinition itemToCraft, int recipeIndex)
        {
            itemToCraft.Craft(recipeIndex);
        }

        /// <summary>
        /// <para>Triggers the indicated generator to drop and item if available.</para>
        /// <para>NOTE: This will trigger an Items Droped event and optionally a Refresh of the player's inventory</para>
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="postDropRefresh"></param>
        public void TriggerItemDrop(ItemGeneratorDefinition generator, bool postDropRefresh = false)
        {
            Settings.TriggerItemDrop(generator, postDropRefresh);
        }

        /// <summary>
        /// Consolodate all stacks of this into a single stack
        /// </summary>
        /// <returns></returns>
        public void Consolidate(InventoryItemDefinition item)
        {
            item.Consolidate();
        }

        public void TriggerItemDrop(ItemGeneratorDefinition generator)
        {
            TriggerItemDrop(generator, false);
        }

        public void TriggerItemDropAndRefresh(ItemGeneratorDefinition generator)
        {
            TriggerItemDrop(generator, true);
        }
        #endregion
    }
}
#endif