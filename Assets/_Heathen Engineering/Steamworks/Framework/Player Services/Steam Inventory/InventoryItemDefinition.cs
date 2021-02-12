#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{

    /// <summary>
    /// <para>The base of your in game inventory item definitions</para>
    /// <para>This can be derived from to create custom Item Definitions. It defines all the required fields for the wider system to funciton correctly but can be expanded with images, models and other data that might be useful for your game.</para>
    /// </summary>
    /// /// <example>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <para>An example of expanding the InventoryItemDefinition structure to add a Avatar image for use in UI rendering.</para>
    /// <para>Note that in this case we have added a asset menu field such that right clicking in your project folder and selecting
    /// Create > Steamworks > My Game > Item Definition with Avatar
    /// will give you a new scriptable avatar with all the required fields and an extra Texture field to relate a given image to as your avatar.</para>
    /// </description>
    /// <code>
    /// [CreateAssetMenu(menuName = "Steamworks/My Game/Item Definition with Avatar")]
    /// public class ItemDefintionWithAvatar : InventoryItemDefinition
    /// {
    ///         public Texture avatarImage;
    /// }
    /// </code>
    /// </item>
    /// </list>
    /// </example>
    public abstract class InventoryItemDefinition : InventoryItemPointer
    {
        public override InventoryItemType ItemType { get { return InventoryItemType.ItemDefinition; } }

        [SerializeField]
        /// <summary>
        /// A list of Steam Item Details related to this item definition
        /// This is refreshed from Steam by calling SteamworksInventoryManager.RefreshInventory
        /// </summary>
        public List<SteamItemDetails_t> Instances;

        /// <summary>
        /// The total quantity of all instances
        /// </summary>
        public int Count
        {
            get
            {
                if (Instances != null)
                    return Instances.Sum(p => p.m_unQuantity);
                else
                    return 0;
            }
        }

        /// <summary>
        /// This cannot be undone and will remove items from the palyer's inventory
        /// Be very sure the player wants to do this!
        /// </summary>
        /// <param name="count"></param>
        public void Consume(int count)
        {
            if (Count > count)
            {
                var ConsumedSoFar = 0;

                List<SteamItemDetails_t> Edits = new List<SteamItemDetails_t>();

                foreach (var instance in Instances)
                {
                    if (count - ConsumedSoFar >= instance.m_unQuantity)
                    {
                        //We need to consume all of these
                        ConsumedSoFar += instance.m_unQuantity;
                        SteamworksPlayerInventory.ConsumeItem(instance.m_itemId, instance.m_unQuantity,
                            (status, results) =>
                            {
                                if (!status)
                                {
                                    Debug.LogWarning("Failed to consume (" + instance.m_unQuantity.ToString() + ") units of item [" + instance.m_iDefinition.m_SteamItemDef.ToString() + "]");
                                    SteamworksInventorySettings.Current.ItemsConsumed.Invoke(status, results);
                                }
                            });

                        var edit = instance;
                        edit.m_unQuantity = 0;
                        Edits.Add(edit);
                    }
                    else
                    {
                        //We only need some of these
                        var need = count - ConsumedSoFar;
                        ConsumedSoFar += need;
                        SteamworksPlayerInventory.ConsumeItem(instance.m_itemId, Convert.ToUInt32(need),
                            (status, results) =>
                            {
                                if (!status)
                                    Debug.LogWarning("Failed to consume (" + need.ToString() + ") units of item [" + instance.m_iDefinition.m_SteamItemDef.ToString() + "]");

                                if (SteamworksInventorySettings.Current != null)
                                {
                                    SteamworksInventorySettings.Current.ItemsConsumed.Invoke(status, results);
                                }
                            });

                        var edit = instance;
                        edit.m_unQuantity -= Convert.ToUInt16(need);
                        Edits.Add(edit);

                        break;
                    }
                }

                //Manually update our instances to account for the quantity changes we expect to see
                foreach (var edit in Edits)
                {
                    Instances.RemoveAll(p => p.m_itemId == edit.m_itemId);
                    Instances.Add(edit);
                }
            }
        }

        /// <summary>
        /// Get a list of items to fill the desired count
        /// </summary>
        /// <param name="count">The count to fetch</param>
        /// <param name="decriment">Should the cashed values be decrimented to match that which was used</param>
        /// <returns></returns>
        public List<ExchangeItemCount> FetchItemCount(uint count, bool decriment)
        {
            if (Count >= count)
            {
                var ConsumedSoFar = 0;
                List<ExchangeItemCount> resultCounts = new List<ExchangeItemCount>();

                List<SteamItemDetails_t> Edits = new List<SteamItemDetails_t>();

                foreach (var instance in Instances)
                {
                    if (count - ConsumedSoFar >= instance.m_unQuantity)
                    {
                        //We need to consume all of these
                        ConsumedSoFar += instance.m_unQuantity;

                        resultCounts.Add(new ExchangeItemCount() { InstanceId = instance.m_itemId, Quantity = instance.m_unQuantity });

                        var edit = instance;
                        edit.m_unQuantity = 0;
                        Edits.Add(edit);
                    }
                    else
                    {
                        //We only need some of these
                        int need = Convert.ToInt32(count - ConsumedSoFar);
                        ConsumedSoFar += need;

                        resultCounts.Add(new ExchangeItemCount() { InstanceId = instance.m_itemId, Quantity = Convert.ToUInt32(need) });

                        var edit = instance;
                        edit.m_unQuantity -= Convert.ToUInt16(need);
                        Edits.Add(edit);

                        break;
                    }
                }

                if (decriment)
                {
                    //Manually update our instances to account for the quantity changes we expect to see
                    foreach (var edit in Edits)
                    {
                        Instances.RemoveAll(p => p.m_itemId == edit.m_itemId);
                        Instances.Add(edit);
                    }
                }

                return resultCounts;
            }
            else
                return null;
        }

        /// <summary>
        /// Splits an instance quantity, if the destination instance is -1 this will create a new stack of the defined quantity.
        /// </summary>
        /// <param name="source">The stack by index to split</param>
        /// <param name="quantity">The number of items to remove from the source stack</param>
        /// <param name="destination">The stack to move the quantity to, if this is -1 it will create a new stack.</param>
        /// <returns></returns>
        public bool TransferQuantity(int source, uint quantity, int destination)
        {
            var instance = Instances[source];
            var dest = SteamItemInstanceID_t.Invalid;
            if (destination > -1)
                dest = Instances[destination].m_itemId;

            return TransferQuantity(instance, quantity, dest);
        }

        /// <summary>
        /// Splits an instance quantity, if the destination instance is -1 this will create a new stack of the defined quantity.
        /// </summary>
        /// <param name="source">The instance to split</param>
        /// <param name="quantity">The number of items to remove from the source stack</param>
        /// <param name="destination">The target to move the quanity to</param>
        /// <returns></returns>
        public bool TransferQuantity(SteamItemDetails_t source, uint quantity, SteamItemInstanceID_t destination)
        {
            if (source.m_unQuantity >= quantity)
            {
                
                var ret = SteamworksPlayerInventory.TransferQuantity(source.m_itemId, quantity, destination, (result) =>
                {
                    Instances.RemoveAll(p => p.m_itemId == source.m_itemId);
                    source.m_unQuantity -= Convert.ToUInt16(quantity);
                    Instances.Add(source);
                });

                return ret;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Moves the quantity from the source into a new stack
        /// </summary>
        /// <param name="source">Source instance to move units from</param>
        /// <param name="quantity">The number of units to move</param>
        /// <returns></returns>
        public bool SplitInstance(SteamItemDetails_t source, uint quantity)
        {
            if (source.m_unQuantity >= quantity)
            {

                var ret = SteamworksPlayerInventory.TransferQuantity(source.m_itemId, quantity, SteamItemInstanceID_t.Invalid, (result) =>
                {
                    Instances.RemoveAll(p => p.m_itemId == source.m_itemId);
                    source.m_unQuantity -= Convert.ToUInt16(quantity);
                    Instances.Add(source);
                });

                return ret;
            }
            else
            {
                Debug.LogWarning("Unable to split instance, insufficent units available to move.");
                return false;
            }
        }

        /// <summary>
        /// Moves the source instance in its entirety to the destination. 
        /// </summary>
        /// <param name="source">The source to move</param>
        /// <param name="destination">The target destination</param>
        /// <returns></returns>
        public bool StackInstance(SteamItemDetails_t source, SteamItemInstanceID_t destination)
        {
            return TransferQuantity(source, source.m_unQuantity, destination);
        }

        /// <summary>
        /// Consolodate all stacks of this into a single stack
        /// </summary>
        /// <returns></returns>
        public void Consolidate()
        {
            if (Instances != null)
            {
                if (Instances.Count > 1)
                {
                    List<SteamItemInstanceID_t> removedInstances = new List<SteamItemInstanceID_t>();
                    var primary = Instances[0];
                    for (int i = 1; i < Instances.Count; i++)
                    {
                        var toMove = Instances[i];
                        var ret = SteamworksPlayerInventory.TransferQuantity(toMove.m_itemId, toMove.m_unQuantity, primary.m_itemId, (result) =>
                        {
                            if (!result)
                            {
                                Debug.LogError("Failed to stack an instance, please refresh the item instances for item definition [" + name + "].");
                            }
                            else
                            {
                                removedInstances.Add(toMove.m_itemId);
                            }
                        });

                        if (!ret)
                        {
                            Debug.LogError("Steam activly refused a TransferItemQuantity request during the Consolodate operation. No further requests will be sent.");
                        }
                    }

                    foreach(var instance in removedInstances)
                        Instances.RemoveAll(p => p.m_itemId == instance);
                }
                else
                {
                    Debug.LogWarning("Unable to consolodate items, this item only has 1 instance. No action will be taken.");
                }
            }
            else
            {
                Debug.LogWarning("Unable to consolodate items, this item only has no instances. No action will be taken.");
            }
        }

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