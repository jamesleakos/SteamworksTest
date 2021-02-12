#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [CreateAssetMenu(menuName = "Steamworks/Player Services/Inventory Settings")]
    public class SteamworksInventorySettings : ScriptableObject
    {
        public static SteamworksInventorySettings Current;
        public bool IsActive
        {
            get
            {
                return Current == this;
            }
            set
            {
                if (value)
                    Register();
                else if (Current == this)
                    Current = null;
            }
        }

        public bool LogDebugMessages = false;

        public List<InventoryItemDefinition> ItemDefinitions = new List<InventoryItemDefinition>();

        public List<ItemGeneratorDefinition> ItemGenerators = new List<ItemGeneratorDefinition>();

        public List<TagGeneratorDefinition> TagGenerators = new List<TagGeneratorDefinition>();

        public List<InventoryItemBundleDefinition> ItemBundles = new List<InventoryItemBundleDefinition>();

        [HideInInspector]
        public UnityEvent ItemInstancesUpdated = new UnityEvent();
        [HideInInspector]
        public UnityItemDetailEvent ItemsGranted = new UnityItemDetailEvent();
        [HideInInspector]
        public UnityItemDetailEvent ItemsConsumed = new UnityItemDetailEvent();
        [HideInInspector]
        public UnityItemDetailEvent ItemsExchanged = new UnityItemDetailEvent();
        [HideInInspector]
        public UnityItemDetailEvent ItemsDroped = new UnityItemDetailEvent();

        private Dictionary<SteamItemDef_t, InventoryItemPointer> ItemPointerIndex = new Dictionary<SteamItemDef_t, InventoryItemPointer>();
        private Dictionary<SteamItemDef_t, InventoryItemDefinition> ItemDefinitionIndex = new Dictionary<SteamItemDef_t, InventoryItemDefinition>();
        private Dictionary<SteamItemDef_t, ItemGeneratorDefinition> ItemGeneratorIndex = new Dictionary<SteamItemDef_t, ItemGeneratorDefinition>();

        #region Utilities
        /// <summary>
        /// Indexes the items and generators for fast look up by ID
        /// </summary>
        public void BuildIndex()
        {
            if (ItemPointerIndex == null)
                ItemPointerIndex = new Dictionary<SteamItemDef_t, InventoryItemPointer>();

            if (ItemDefinitionIndex == null)
                ItemDefinitionIndex = new Dictionary<SteamItemDef_t, InventoryItemDefinition>();

            if (ItemGeneratorIndex == null)
                ItemGeneratorIndex = new Dictionary<SteamItemDef_t, ItemGeneratorDefinition>();

            if (LogDebugMessages)
            {
                var items = ItemDefinitions != null ? ItemDefinitions.Count : 0;
                var generators = ItemGenerators != null ? ItemGenerators.Count : 0;
                Debug.Log("Building internal indices for " + items + " items and " + generators + " generators.");
            }

            foreach (var item in ItemDefinitions)
            {
                if (ItemDefinitionIndex.ContainsKey(item.DefinitionID))
                    ItemDefinitionIndex[item.DefinitionID] = item;
                else
                    ItemDefinitionIndex.Add(item.DefinitionID, item);

                if(ItemPointerIndex.ContainsKey(item.DefinitionID))
                    ItemPointerIndex[item.DefinitionID] = item;
                else
                    ItemPointerIndex.Add(item.DefinitionID, item);
            }

            foreach (var item in ItemGenerators)
            {
                if (ItemGeneratorIndex.ContainsKey(item.DefinitionID))
                    ItemGeneratorIndex[item.DefinitionID] = item;
                else
                    ItemGeneratorIndex.Add(item.DefinitionID, item);

                if (ItemPointerIndex.ContainsKey(item.DefinitionID))
                    ItemPointerIndex[item.DefinitionID] = item;
                else
                    ItemPointerIndex.Add(item.DefinitionID, item);
            }
        }
        /// <summary>
        /// Clears all instances for all registered items
        /// </summary>
        public void ClearItemCounts()
        {
            foreach (var item in ItemDefinitions)
            {
                if (item.Instances == null)
                    item.Instances = new List<SteamItemDetails_t>();
                else
                    item.Instances.Clear();
            }
        }
        /// <summary>
        /// Used internally to update registered items when new details are recieved from Steam
        /// </summary>
        /// <param name="details"></param>
        public static void InternalItemDetailUpdate(IEnumerable<SteamItemDetails_t> details)
        {
            if (Current != null && details != null)
                Current.HandleItemDetailUpdate(details);
        }
        /// <summary>
        /// Handles detail item updates typically from Steam
        /// </summary>
        /// <param name="details"></param>
        public void HandleItemDetailUpdate(IEnumerable<SteamItemDetails_t> details)
        {
            var hasUpdate = false;
            foreach(var detail in details)
            {
                if(ItemDefinitionIndex.ContainsKey(detail.m_iDefinition))
                {
                    hasUpdate = true;
                    var target = ItemDefinitionIndex[detail.m_iDefinition];
                    if (target.Instances == null)
                        target.Instances = new List<SteamItemDetails_t>();
                    else
                        target.Instances.RemoveAll(p => p.m_itemId == detail.m_itemId);

                    target.Instances.Add(detail);
                }
                else if (LogDebugMessages)
                {
                    Debug.LogWarning("No item definition found for item " + detail.m_iDefinition.m_SteamItemDef + " but an item instance " + detail.m_itemId.m_SteamItemInstanceID + " exists in the player's inventory with a unit count of " + detail.m_unQuantity + "\nConsider adding an item definition for this to your Steam Inventory Settings.");
                }
            }

            if (hasUpdate)
            {
                if(LogDebugMessages)
                {
                    StringBuilder sb = new StringBuilder("Inventory Item Detail Update:\n");
                    foreach(var item in ItemDefinitions)
                    {
                        sb.Append("\t[" + item.name + "] has " + item.Instances.Count + " instances for a sum of " + item.Count + " units.\n");
                    }
                    Debug.Log(sb.ToString());
                }

                ItemInstancesUpdated.Invoke();
            }
        }
        /// <summary>
        /// Registeres this instance of settings as the active Steamworks Inventory Settings
        /// </summary>
        public void Register()
        {
            if(LogDebugMessages)
            {
                if(Current == null)
                {
                    Debug.Log("Registering a new Steamworks Inventory Settings object [" + name + "]");
                }
                else if (Current != this)
                {
                    Debug.Log("Replacing Steamworks Inventory Settings object [" + Current.name + "] with [" + name + "]");
                }
                if (Current != null)
                {
                    Debug.Log("RE-Registering Steamworks Inventory Settings object [" + name + "]");
                }
            }

            BuildIndex();
            Current = this;
        }
        /// <summary>
        /// Gets the Item Definition for the specified detail object
        /// </summary>
        /// <typeparam name="T">The InventoryItemDefinition derived type that the definition should be casted to</typeparam>
        /// <param name="steamDetail"></param>
        /// <returns></returns>
        public T GetDefinition<T>(SteamItemDetails_t steamDetail) where T: InventoryItemDefinition
        {
            return this[steamDetail] as T;
        }
        /// <summary>
        /// Gets the Item Definition for the specified steam definition id
        /// </summary>
        /// <typeparam name="T">The InventoryItemDefinition derived type that the definition should be casted to</typeparam>
        /// <param name="steamDefinition"></param>
        /// <returns></returns>
        public T GetDefinition<T>(SteamItemDef_t steamDefinition) where T : InventoryItemDefinition
        {
            return this[steamDefinition] as T;
        }
        /// <summary>
        /// Gets the Item Definition for the specified detail object
        /// </summary>
        /// <param name="steamDetail"></param>
        /// <returns></returns>
        public InventoryItemDefinition GetDefinition(SteamItemDetails_t steamDetail)
        {
            return this[steamDetail] as InventoryItemDefinition;
        }
        /// <summary>
        /// Gets the Item Definition for the specified steam definition id
        /// </summary>
        /// <param name="steamDefinition"></param>
        /// <returns></returns>
        public InventoryItemDefinition GetDefinition(SteamItemDef_t steamDefinition)
        {
            return this[steamDefinition] as InventoryItemDefinition;
        }

        public InventoryItemDefinition GetDefinition(int steamDefinition)
        {
            return this[steamDefinition] as InventoryItemDefinition;
        }

        public InventoryItemPointer this[SteamItemDef_t id]
        {
            get
            {
                if (ItemPointerIndex == null)
                    BuildIndex();

                if (ItemPointerIndex.ContainsKey(id))
                    return ItemPointerIndex[id];
                else
                    return null;
            }
        }

        public InventoryItemPointer this[SteamItemDetails_t item]
        {
            get
            {
                return this[item.m_iDefinition];
            }
        }

        public InventoryItemPointer this[int itemId]
        {
            get
            {
                return this[new SteamItemDef_t(itemId)];
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
            foreach (var item in ItemDefinitions)
            {
                if (item.Instances == null)
                    item.Instances = new List<SteamItemDetails_t>();
                else
                    item.Instances.Clear();
            }

            var result = SteamworksPlayerInventory.GetAllItems(null);
            if (!result)
                Debug.LogWarning("[SteamworksInventorySettings.RefreshInventory] - Call failed");
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
            var result = SteamworksPlayerInventory.GrantPromoItems((status, results) =>
            {
                ItemsGranted.Invoke(status, results);
            });
            if (!result)
                Debug.LogWarning("[SteamworksInventorySettings.GrantAllPromotionalItems] - Call failed");
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
            var result = SteamworksPlayerInventory.AddPromoItem(itemDefinition.DefinitionID, (status, results) =>
            {
                ItemsGranted.Invoke(status, results);
            });
            if (!result)
                Debug.LogWarning("[SteamworksInventorySettings.GrantPromotionalItem] - Call failed");
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
            List<SteamItemDef_t> items = new List<SteamItemDef_t>();
            foreach(var itemDef in itemDefinitions)
            {
                items.Add(itemDef.DefinitionID);
            }

            var result = SteamworksPlayerInventory.AddPromoItems(items, (status, results) =>
            {
                ItemsGranted.Invoke(status, results);
            });
            if (!result)
                Debug.LogWarning("[SteamworksInventorySettings.GrantPromotionalItems] - Call failed");
        }

        /// <summary>
        /// <para>Determins if the result handle belongs to the user</para>
        /// </summary>
        /// <param name="resultHandle">The inventory result handle to check the user on</param>
        /// <param name="user">The user to check against</param>
        public bool CheckUserResult(SteamInventoryResult_t resultHandle, ulong user)
        {
            return SteamworksPlayerInventory.CheckResultSteamID(resultHandle, user);
        }

        /// <summary>
        /// <para>Determins if the result handle belongs to the user</para>
        /// </summary>
        /// <param name="resultHandle">The inventory result handle to check the user on</param>
        /// <param name="user">The user to check against</param>
        public bool CheckUserResult(SteamInventoryResult_t resultHandle, CSteamID user)
        {
            return SteamworksPlayerInventory.CheckResultSteamID(resultHandle, user);
        }

        /// <summary>
        /// <para>Determins if the result handle belongs to the user</para>
        /// </summary>
        /// <param name="resultHandle">The inventory result handle to check the user on</param>
        /// <param name="user">The user to check against</param>
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
            var target = itemDefinition.Instances.FirstOrDefault(p => p.m_unQuantity > 0);

            var result = SteamworksPlayerInventory.ConsumeItem(target.m_itemId, (status, results) =>
            {
                if(status)
                {
                    itemDefinition.Instances.RemoveAll(p => p.m_itemId == target.m_itemId);
                    target.m_unQuantity--;
                    itemDefinition.Instances.Add(target);
                    ItemInstancesUpdated.Invoke();
                    ItemsConsumed.Invoke(status, results);
                }
            });

            if (!result)
                Debug.LogWarning("[SteamworksInventorySettings.ConsumeItem] - Call failed");
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
            if (count < 1)
            {
                Debug.LogWarning("Attempted to consume a number of items less than 1; this is not possible and was note requested.");
                return;
            }

            List<ExchangeItemCount> counts = new List<ExchangeItemCount>();
            int currentCount = 0;
            foreach(var details in itemDefinition.Instances)
            {
                if(details.m_unQuantity >= count - currentCount)
                {
                    counts.Add(new ExchangeItemCount() { InstanceId = details.m_itemId, Quantity = System.Convert.ToUInt32(count - currentCount )});
                    currentCount = count;
                    break;
                }
                else
                {
                    counts.Add(new ExchangeItemCount() { InstanceId = details.m_itemId, Quantity = details.m_unQuantity });
                    currentCount += details.m_unQuantity;
                }
            }

            bool noErrors = true;
            foreach (var exchange in counts)
            {
                var result = SteamworksPlayerInventory.ConsumeItem(exchange.InstanceId, exchange.Quantity, (status, results) =>
                {
                    if (status)
                    {
                        var target = itemDefinition.Instances.FirstOrDefault(p => p.m_itemId == exchange.InstanceId);
                        itemDefinition.Instances.RemoveAll(p => p.m_itemId == exchange.InstanceId);
                        target.m_unQuantity -= System.Convert.ToUInt16(exchange.Quantity);
                        itemDefinition.Instances.Add(target);
                        ItemInstancesUpdated.Invoke();
                        ItemsConsumed.Invoke(status, results);
                    }
                });

                if (!result)
                {
                    noErrors = false;
                    Debug.LogWarning("Failed to consume all requested items");
                    break;
                }
            }

            if (!noErrors)
                Debug.LogWarning("[SteamworksInventorySettings.ConsumeItem] - Call failed");
        }

        /// <summary>
        /// <para>Consumes the indicated number of units of this item from this specific instance stack</para>
        /// <para>NOTE: this is the most efficent way to consume multiple units of an item at a time.</para>
        /// </summary>
        /// <param name="itemDefinition"></param>
        /// <param name="instanceId"></param>
        /// <param name="count"></param>
        public void ConsumeItem(InventoryItemDefinition itemDefinition, SteamItemInstanceID_t instanceId, int count)
        {
            var target = itemDefinition.Instances.FirstOrDefault(p => p.m_itemId == instanceId);
            var result = SteamworksPlayerInventory.ConsumeItem(instanceId, (status, results) =>
            {
                if (status)
                {
                    itemDefinition.Instances.RemoveAll(p => p.m_itemId == target.m_itemId);
                    target.m_unQuantity -= System.Convert.ToUInt16(count);
                    itemDefinition.Instances.Add(target);
                    ItemInstancesUpdated.Invoke();
                    ItemsConsumed.Invoke(status, results);
                }
            });

            if (!result)
                Debug.LogWarning("[SteamworksInventorySettings.ConsumeItem] - Call failed");
        }

        /// <summary>
        /// <para>Attampts to consume the indicated number of items from the instance provided.</para>
        /// <para>Note this method must look up the instance's related Item Definition which can take time and can be error prone. It is recomended that you provide the ItemDefinition with your call to the consume method.</para>
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="count"></param>
        public void ConsumeItem(SteamItemInstanceID_t instanceId, int count)
        {
            var itemDefinition = ItemDefinitions.FirstOrDefault(p => p.Instances.Any(i => i.m_itemId == instanceId));
            
            if(itemDefinition == null)
            {
                Debug.LogError("Unable to locate the Item Definition for Item Instance " + instanceId.m_SteamItemInstanceID.ToString());
                return;
            }

            var target = itemDefinition.Instances.FirstOrDefault(p => p.m_itemId == instanceId);
            var result = SteamworksPlayerInventory.ConsumeItem(instanceId, (status, results) =>
            {
                if (status)
                {
                    itemDefinition.Instances.RemoveAll(p => p.m_itemId == target.m_itemId);
                    target.m_unQuantity -= System.Convert.ToUInt16(count);
                    itemDefinition.Instances.Add(target);
                    ItemInstancesUpdated.Invoke();
                    ItemsConsumed.Invoke(status, results);
                }
            });

            if (!result)
                Debug.LogWarning("[SteamworksInventorySettings.ConsumeItem] - Call failed");
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
        public void TriggerItemDrop(ItemGeneratorDefinition generator, bool postDropRefresh)
        {
            generator.TriggerDrop((status, results) =>
            {
                if(status)
                {
                    if(LogDebugMessages)
                    {
                        Debug.Log("Item Drop for [" + generator.name + "] completed with status " + status + " and " + results.Count() + " instances effected.");
                    }
                    if (postDropRefresh)
                        RefreshInventory();
                }

                ItemsDroped.Invoke(status, results);
            });
        }

        public void TriggerItemDrop(ItemGeneratorDefinition generator)
        {
            TriggerItemDrop(generator, false);
        }

        public void TriggerItemDropAndRefresh(ItemGeneratorDefinition generator)
        {
            TriggerItemDrop(generator, true);
        }

        /// <summary>
        /// Splits an instance quantity, if the destination instance is -1 this will create a new stack of the defined quantity.
        /// </summary>
        /// <param name="source">The instance to split</param>
        /// <param name="quantity">The number of items to remove from the source stack</param>
        /// <param name="destination">The target to move the quanity to</param>
        /// <returns></returns>
        public bool TransferQuantity(InventoryItemDefinition item, SteamItemDetails_t source, uint quantity, SteamItemInstanceID_t destination)
        {
            return item.TransferQuantity(source, quantity, destination);
        }

        /// <summary>
        /// Moves the quantity from the source into a new stack
        /// </summary>
        /// <param name="source">Source instance to move units from</param>
        /// <param name="quantity">The number of units to move</param>
        /// <returns></returns>
        public bool SplitInstance(InventoryItemDefinition item, SteamItemDetails_t source, uint quantity)
        {
            return item.SplitInstance(source, quantity);
        }

        /// <summary>
        /// Moves the source instance in its entirety to the destination. 
        /// </summary>
        /// <param name="source">The source to move</param>
        /// <param name="destination">The target destination</param>
        /// <returns></returns>
        public bool StackInstance(InventoryItemDefinition item, SteamItemDetails_t source, SteamItemInstanceID_t destination)
        {
            return item.StackInstance(source, destination);
        }

        /// <summary>
        /// Consolodate all stacks of this into a single stack
        /// </summary>
        /// <returns></returns>
        public void Consolidate(InventoryItemDefinition item)
        {
            item.Consolidate();
        }
    }
}
#endif