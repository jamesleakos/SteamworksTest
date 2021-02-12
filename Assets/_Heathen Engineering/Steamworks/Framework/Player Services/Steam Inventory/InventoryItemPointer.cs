#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// The base class of both <see cref="InventoryItemDefinition"/> and <see cref="ItemGeneratorDefinition"/>. 
    /// This object defines the most basic funcitonality of any Steam Inventory item regardless of type.
    /// </summary>
    public abstract class InventoryItemPointer : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
        /// <summary>
        /// This must match the definition ID set in Steam for this item
        /// </summary>
        public SteamItemDef_t DefinitionID;

        public abstract InventoryItemType ItemType { get; }

        /// <summary>
        /// This is simply an in game representation of the confugraiton you should have created in your Steamworks Partner site for this item.
        /// </summary>
        public List<CraftingRecipe> Recipes;
        [HideInInspector]
        public List<ValveItemDefAttribute> ValveItemDefAttributes;

        public CraftingRecipe this[string name]
        {
            get { return Recipes?.FirstOrDefault(p => p.name == name); }
        }

        /// <summary>
        /// Converts a Crafting Recipe into a ItemExchangeRecipe suitable for use in the Steam API
        /// </summary>
        /// <param name="recipe">The recipe to create</param>
        /// <param name="Edits">This will contain the resulting edits to in memory item instances assuming the exchange is accepted by Steam.</param>
        /// <returns></returns>
        public ItemExchangeRecipe PrepareItemExchange(CraftingRecipe recipe, out Dictionary<InventoryItemDefinition, List<SteamItemDetails_t>> Edits)
        {
            //Build ItemExchangeRecipe from available instances to match this recipe
            ItemExchangeRecipe itemRecipe = new ItemExchangeRecipe();
            itemRecipe.ItemToGenerate = DefinitionID;
            itemRecipe.ItemsToConsume = new List<ExchangeItemCount>();

            //Verify quantity
            foreach (var reagent in recipe.Items)
            {
                if (reagent.Item.Count < reagent.Count)
                {
                    Debug.LogError("InventoryItemPointer.Craft - Failed to fetch the required items for the recipe, insufficent supply of '" + reagent.Item.name + "'.");
                    Edits = null;
                    return null;
                }
            }

            Edits = new Dictionary<InventoryItemDefinition, List<SteamItemDetails_t>>();

            //Extract required amounts
            foreach (var reagent in recipe.Items)
            {
                if (reagent.Item.Count >= reagent.Count)
                {
                    var ConsumedSoFar = 0;
                    List<ExchangeItemCount> resultCounts = new List<ExchangeItemCount>();

                    List<SteamItemDetails_t> ItemEdits = new List<SteamItemDetails_t>();

                    foreach (var instance in reagent.Item.Instances)
                    {
                        if (reagent.Count - ConsumedSoFar >= instance.m_unQuantity)
                        {
                            //We need to consume all of these
                            ConsumedSoFar += instance.m_unQuantity;

                            resultCounts.Add(new ExchangeItemCount() { InstanceId = instance.m_itemId, Quantity = instance.m_unQuantity });

                            var edit = instance;
                            edit.m_unQuantity = 0;
                            ItemEdits.Add(edit);
                        }
                        else
                        {
                            //We only need some of these
                            int need = Convert.ToInt32(reagent.Count - ConsumedSoFar);
                            ConsumedSoFar += need;

                            resultCounts.Add(new ExchangeItemCount() { InstanceId = instance.m_itemId, Quantity = Convert.ToUInt32(need) });

                            var edit = instance;
                            edit.m_unQuantity -= Convert.ToUInt16(need);
                            ItemEdits.Add(edit);

                            break;
                        }
                    }

                    Edits.Add(reagent.Item, ItemEdits);

                    itemRecipe.ItemsToConsume.AddRange(resultCounts);
                }
                else
                {
                    Debug.LogWarning("Crafting request was unable to complete due to insuffient resources.");
                    return null;
                }
            }

            return itemRecipe;
        }

        /// <summary>
        /// Attempts to exchange the required items for a new copy of this item
        /// This is subject to checks by valve as to rather or not this is a legitimate recipie and that the use has sufficent items available for exchange
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns>True if the request is successfuly sent to Steam for processing</returns>
        public void Craft(CraftingRecipe recipe)
        {
            var itemRecipe = PrepareItemExchange(recipe, out Dictionary<InventoryItemDefinition, List<SteamItemDetails_t>> edits);

            if(itemRecipe.ItemsToConsume == null || itemRecipe.ItemsToConsume.Count < 1)
            {
                Debug.LogWarning("Attempted to craft item [" + name + "] with no items to consume selected!\nThis will be refused by Steam so will not be sent!");
                return;
            }

            if (itemRecipe != null)
            {
                var result = SteamworksPlayerInventory.ExchangeItems(itemRecipe, (status, results) =>
                {
                    if (status)
                    {
                        //Remove the counts for the consumed items
                        foreach (var kvp in edits)
                        {
                            foreach (var item in kvp.Value)
                            {
                                kvp.Key.Instances.RemoveAll(p => p.m_itemId == item.m_itemId);
                                kvp.Key.Instances.Add(item);
                            }
                        }

                        if(SteamworksInventorySettings.Current != null && SteamworksInventorySettings.Current.LogDebugMessages)
                        {
                            StringBuilder sb = new StringBuilder("Inventory Item [" + name + "] Crafted,\nItems Consumed:\n");
                            foreach(var item in recipe.Items)
                            {
                                sb.Append("\t" + item.Count + " [" + item.Item.name + "]");
                            }
                        }
                    }
                    else
                    {
                        if (SteamworksInventorySettings.Current != null && SteamworksInventorySettings.Current.LogDebugMessages)
                        {
                            Debug.LogWarning("Request to craft item [" + name + "] failed, confirm the item and recipie configurations are correct in the app settings.");
                        }
                    }

                    if(SteamworksInventorySettings.Current != null)
                    {
                        SteamworksInventorySettings.Current.ItemsExchanged.Invoke(status, results);
                    }
                });

                if(!result)
                {
                    if (SteamworksInventorySettings.Current != null)
                    {
                        SteamworksInventorySettings.Current.ItemsExchanged.Invoke(false, new SteamItemDetails_t[] { });
                        if (SteamworksInventorySettings.Current.LogDebugMessages)
                            Debug.LogWarning("Request to craft item [" + name + "] was refused by Steam.");
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to exchange the required items for a new copy of this item
        /// This is subject to checks by valve as to rather or not this is a legitimate recipie and that the use has sufficent items available for exchange
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns>True if the request is successfuly sent to Steam for processing</returns>
        public void Craft(int recipeIndex)
        {
            var recipe = Recipes[recipeIndex];
            Craft(recipe);
        }

        /// <summary>
        /// Grants a copy of this item if available in the items definition on the Steam backend.
        /// </summary>
        public void GrantPromoItem()
        {
            SteamworksPlayerInventory.AddPromoItem(DefinitionID, (status, results) =>
            {
                if (SteamworksInventorySettings.Current != null)
                {
                    SteamworksInventorySettings.Current.ItemsGranted.Invoke(status, results);
                }
            });
        }
    }
}
#endif