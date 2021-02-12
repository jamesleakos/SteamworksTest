#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Scriptable;
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// Wraps Valve's SteamInventory with a simplified solution that does not depend on callbacks or callresults.
    /// </summary>
    public static class SteamworksPlayerInventory
    {
        private static bool callbacksRegistered = false;
        private static Dictionary<SteamInventoryResult_t, CallRequest> pendingCalls;
        private static Callback<SteamInventoryResultReady_t> m_SteamInventoryResultReady;
        private static CallResult<SteamInventoryEligiblePromoItemDefIDs_t> m_SteamInventoryEligiblePromoItemDefIDs;

        public static bool RegisterCallbacks()
        {
            if (SteamSettings.current.Initialized)
            {
                if (!callbacksRegistered)
                {
                    callbacksRegistered = true;
                    m_SteamInventoryResultReady = Callback<SteamInventoryResultReady_t>.Create(HandleSteamInventoryResult);
                    m_SteamInventoryEligiblePromoItemDefIDs = CallResult<SteamInventoryEligiblePromoItemDefIDs_t>.Create(HandleEligiblePromoItemDefIDs);
                    return true;
                }
                else
                    return true;
            }
            else
                return false;
        }

        #region Processing
        private static void ProcessDetailQuery(SteamInventoryResultReady_t param, CallRequest callRequest, string callerName)
        {
            try
            {
                if (param.m_result != EResult.k_EResultOK)
                {
                    Debug.LogError("The call from " + callerName + " failed to process on steam as expected, EResult = " + param.m_result + ".\nThis will report as a failed call to the provided callback.");

                    if (callRequest.DetailCallback != null)
                        callRequest.DetailCallback.Invoke(false, null);
                }
                else
                {
                    //This is to work around a bug with Steamworks.NET where in it must have an initalized array to fetch the item count
                    uint count = 10000;
                    SteamItemDetails_t[] temp = new SteamItemDetails_t[10000];

                    if (SteamInventory.GetResultItems(param.m_handle, temp, ref count))
                    {
                        SteamItemDetails_t[] results = new SteamItemDetails_t[count];
                        var responce = SteamInventory.GetResultItems(param.m_handle, results, ref count);
                        if (responce)
                        {
                            try
                            {
                                SteamworksInventorySettings.InternalItemDetailUpdate(results);
                                if (callRequest.DetailCallback != null)
                                    callRequest.DetailCallback.Invoke(true, results);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("The callback provided to the " + callerName + " request threw an exception when invoked!");
                                Debug.LogException(ex);
                            }
                        }
                        else
                        {
                            Debug.LogError("Steam Inventory " + callerName + " failed to retrive the resulting Inventory Item details.");

                            try
                            {
                                if (callRequest.DetailCallback != null)
                                    callRequest.DetailCallback.Invoke(false, null);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("The callback provided to the " + callerName + " request threw an exception when invoked!");
                                Debug.LogException(ex);
                            }
                        }
                    }
                    else
                    {
                        if (callRequest.DetailCallback != null)
                            callRequest.DetailCallback.Invoke(true, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                try
                {
                    if (callRequest.DetailCallback != null)
                        callRequest.DetailCallback.Invoke(false, null);
                }
                catch (Exception ex2)
                {
                    Debug.LogError("The callback provided to the " + callerName + " request threw an exception when invoked!");
                    Debug.LogException(ex2);
                }
            }
            Debug.Log("Destroying handle: " + param.m_handle.ToString());
            SteamInventory.DestroyResult(param.m_handle);
        }
        
        private static void ProcessSerializationRequest(SteamInventoryResultReady_t param, CallRequest callRequest)
        {
            if (callRequest.SerializationCallback != null)
            {
                try
                {
                    if (param.m_result == EResult.k_EResultOK)
                    {
                        uint count;
                        if (SteamInventory.SerializeResult(param.m_handle, null, out count))
                        {
                            byte[] buffer = new byte[count];
                            if (SteamInventory.SerializeResult(param.m_handle, buffer, out count))
                            {
                                callRequest.SerializationCallback(true, buffer);
                            }
                            else
                            {
                                Debug.LogError("Steam Inventory - Serialize Result: Failed to load the serialized results to memory.");
                                callRequest.SerializationCallback(false, null);
                            }
                        }
                        else
                        {
                            Debug.LogError("Steam Inventory - Serialize Result: Failed to calculate the size requirement for the serialized result.");
                            callRequest.SerializationCallback(false, null);
                        }
                    }
                    else
                    {
                        Debug.LogError("Steam Inventory - Serialize Result: Steamworks Result state: " + param.m_result.ToString() + ".");
                        callRequest.SerializationCallback(false, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("The callback provided to the Consumption Request threw an exception when invoked!");
                    Debug.LogException(ex);
                }
            }

            SteamInventory.DestroyResult(param.m_handle);
        }

        private static void ProcessDeserializeRequest(SteamInventoryResultReady_t param, CallRequest callRequest)
        {
            try
            {
                //This is to work around a bug with Steamworks.NET where in it must have an initalized array to fetch the item count
                uint count = 10000;
                SteamItemDetails_t[] temp = new SteamItemDetails_t[10000];

                if (SteamInventory.GetResultItems(param.m_handle, temp, ref count))
                {
                    SteamItemDetails_t[] results = new SteamItemDetails_t[count];
                    var responce = SteamInventory.GetResultItems(param.m_handle, results, ref count);
                    if (responce)
                    {
                        try
                        {
                            if (SteamInventory.CheckResultSteamID(param.m_handle, callRequest.SteamUserId))
                            {
                                if (callRequest.DetailCallback != null)
                                    callRequest.DetailCallback.Invoke(true, results);
                            }
                            else
                            {
                                Debug.LogWarning("Deserialize results returned successfuly however found that the results did not match the Steam User ID and thus are invalid.\nThis is a security measure to insure users cannot spoof inventory results.");

                                if (callRequest.DetailCallback != null)
                                    callRequest.DetailCallback.Invoke(false, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("The callback provided to the Deserialize Result request threw an exception when invoked!");
                            Debug.LogException(ex);
                        }
                    }
                    else
                    {
                        Debug.LogError("Steam Inventory Deserialize Result failed to retrive the resulting Inventory Item details.");

                        try
                        {
                            if (callRequest.DetailCallback != null)
                                callRequest.DetailCallback.Invoke(false, null);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("The callback provided to the Deserialize Result request threw an exception when invoked!");
                            Debug.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                try
                {
                    if (callRequest.DetailCallback != null)
                        callRequest.DetailCallback.Invoke(false, null);
                }
                catch (Exception ex2)
                {
                    Debug.LogError("The callback provided to the Deserialize Result request threw an exception when invoked!");
                    Debug.LogException(ex2);
                }
            }

            SteamInventory.DestroyResult(param.m_handle);
        }

        private static void ProcessTransferRequest(SteamInventoryResultReady_t param, CallRequest callRequest)
        {
            try
            {
                if (param.m_result == EResult.k_EResultOK)
                {
                    try
                    {
                        if (callRequest.BoolCallback != null)
                            callRequest.BoolCallback.Invoke(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("The callback provided to the Transfer Item Quantity request threw an exception when invoked!");
                        Debug.LogException(ex);
                    }
                }
                else
                {
                    try
                    {
                        Debug.LogError("Steam Inventory - Transfer Item Quantity: Steamworks Result state: " + param.m_result.ToString() + ".");
                        if (callRequest.BoolCallback != null)
                            callRequest.BoolCallback.Invoke(false);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("The callback provided to the Transfer Item Quantity request threw an exception when invoked!");
                        Debug.LogException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                try
                {
                    if (callRequest.BoolCallback != null)
                        callRequest.BoolCallback.Invoke(false);
                }
                catch (Exception ex2)
                {
                    Debug.LogError("The callback provided to the Transfer Item Quantity request threw an exception when invoked!");
                    Debug.LogException(ex2);
                }
            }

            SteamInventory.DestroyResult(param.m_handle);
        }

        private static void HandleSteamInventoryResult(SteamInventoryResultReady_t param)
        {
            if(pendingCalls.ContainsKey(param.m_handle))
            {
                var request = pendingCalls[param.m_handle];
                pendingCalls.Remove(param.m_handle);
                switch(request.Type)
                {
                    case CallRequestType.AddPromoItem:
                        ProcessDetailQuery(param, request, "Add Promo Item");
                        break;
                    case CallRequestType.AddPromoItems:
                        ProcessDetailQuery(param, request, "Add Promo Items");
                        break;
                    case CallRequestType.ConsumeItem:
                        ProcessDetailQuery(param, request, "Consume Items");
                        break;
                    case CallRequestType.ExchangeItems:
                        ProcessDetailQuery(param, request, "Exchange Items");
                        break;
                    case CallRequestType.GenerateItems:
                        ProcessDetailQuery(param, request, "Generate Items");
                        break;
                    case CallRequestType.GetAllItems:
                        ProcessDetailQuery(param, request, "Get All Items");
                        break;
                    case CallRequestType.GetItemsByID:
                        ProcessDetailQuery(param, request, "Get Items By ID");
                        break;
                    case CallRequestType.GetItemIDsToSerialize:
                        ProcessSerializationRequest(param, request);
                        break;
                    case CallRequestType.DeserializeResult:
                        ProcessDeserializeRequest(param, request);
                        break;
                    case CallRequestType.TransferItemQuantity:
                        ProcessDetailQuery(param, request, "Transfer Item Quantity");
                        break;
                    case CallRequestType.TriggerItemDrop:
                        ProcessDetailQuery(param, request, "Trigger Item Drop");
                        break;
                    case CallRequestType.GrantPromoItems:
                        ProcessDetailQuery(param, request, "Grant Promo Items");
                        break;
                }
            }
            else
            {
                Debug.LogWarning("Handling an unidentified Steam Inventory Result request. This may lead to a leak in that the handle may not be disposed correctly.");
            }
        }
        #endregion
        
        #region Steam API Wrap
        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#GetAllItems">https://partner.steamgames.com/doc/api/ISteamInventory#GetAllItems</see>
        /// <para>Start retrieving all items in the current users inventory.</para>
        /// <para>    </para>
        /// <para>NOTE: Calls to this function are subject to rate limits and may return cached results if called too frequently. It is suggested that you call this function only when you are about to display the user's full inventory, or if you expect that the inventory may have changed.</para>
        /// </summary>
        /// <param name="callback">The method to call when compelted</param>
        /// <returns></returns>
        public static bool GetAllItems(Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Get All Items before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.GetAllItems,
                DetailCallback = callback
            };


            if (SteamInventory.GetAllItems(out SteamInventoryResult_t handle))
            {
                if(pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Get All Items from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#AddPromoItem">https://partner.steamgames.com/doc/api/ISteamInventory#AddPromoItem</see>
        /// <para>Grant a specific one-time promotional item to the current user.</para>
        /// </summary>
        /// <param name="itemDefinition">The ItemDef to grant the player.</param>
        /// <param name="callback">Method to be called on completion returning a bool indicating success or failure.</param>
        /// <returns></returns>
        public static bool AddPromoItem(SteamItemDef_t itemDefinition, Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Add Promo Item before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.AddPromoItem,
                DetailCallback = callback
            };

            if (SteamInventory.AddPromoItem(out SteamInventoryResult_t handle, itemDefinition))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Add Promo Item from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#AddPromoItems">https://partner.steamgames.com/doc/api/ISteamInventory#AddPromoItems</see>
        /// <para>Grant a specific one-time promotional item to the current user.</para>
        /// </summary>
        /// <param name="itemDefinitions">The ItemDefs to grant the player.</param>
        /// <param name="callback">Method to be called on completion returning a bool indicating success or failure.</param>
        /// <returns></returns>
        public static bool AddPromoItems(IEnumerable<SteamItemDef_t> itemDefinitions, Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Add Promo Item before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.AddPromoItem,
                DetailCallback = callback
            };

            if (SteamInventory.AddPromoItems(out SteamInventoryResult_t handle, itemDefinitions.ToArray(), System.Convert.ToUInt32(itemDefinitions.Count())))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Add Promo Item from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#CheckResultSteamID">https://partner.steamgames.com/doc/api/ISteamInventory#CheckResultSteamID</see>
        /// <para>Checks whether an inventory result handle belongs to the specified Steam ID.</para>
        /// </summary>
        /// <param name="resultHandle">The inventory result handle to check the Steam ID on.</param>
        /// <param name="steamIDExpected">The Steam ID to verify.</param>
        /// <returns></returns>
        public static bool CheckResultSteamID(SteamInventoryResult_t resultHandle, CSteamID steamIDExpected)
        {
            return SteamInventory.CheckResultSteamID(resultHandle, steamIDExpected);
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#CheckResultSteamID">https://partner.steamgames.com/doc/api/ISteamInventory#CheckResultSteamID</see>
        /// <para>Checks whether an inventory result handle belongs to the specified Steam ID.</para>
        /// </summary>
        /// <param name="resultHandle">The inventory result handle to check the Steam ID on.</param>
        /// <param name="steamUserExpected">The Steam User Data to verify.</param>
        /// <returns></returns>
        public static bool CheckResultSteamID(SteamInventoryResult_t resultHandle, SteamUserData steamUserExpected)
        {
            return SteamInventory.CheckResultSteamID(resultHandle, steamUserExpected.id);
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#CheckResultSteamID">https://partner.steamgames.com/doc/api/ISteamInventory#CheckResultSteamID</see>
        /// <para>Checks whether an inventory result handle belongs to the specified Steam ID.</para>
        /// </summary>
        /// <param name="resultHandle">The inventory result handle to check the Steam ID on.</param>
        /// <param name="steamIDExpected">The Steam ID to verify.</param>
        /// <returns></returns>
        public static bool CheckResultSteamID(SteamInventoryResult_t resultHandle, ulong steamIDExpected)
        {
            return SteamInventory.CheckResultSteamID(resultHandle, new CSteamID(steamIDExpected));
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#ConsumeItem">https://partner.steamgames.com/doc/api/ISteamInventory#ConsumeItem</see>
        /// <para>Consumes items from a user's inventory. If the quantity of the given item goes to zero, it is permanently removed.</para>
        /// 
        /// <para>Once an item is removed it cannot be recovered. This is not for the faint of heart - if your game implements item removal at all, a high-friction UI confirmation process is highly recommended. 
        /// ConsumeItem can be restricted to certain item definitions or fully blocked via the Steamworks website to minimize support/abuse issues such as the classic "my brother borrowed my laptop and deleted all of my rare items".</para>
        /// </summary>
        /// <param name="instanceId">The instance id of the item to consume</param>
        /// <param name="callback">The callback to invoke when the request has been completed by Steam Inventory Services</param>
        /// <returns>True if the request was sent, false otherwise.</returns>
        public static bool ConsumeItem(SteamItemInstanceID_t instanceId, Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Consume Item before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            var callRequest = new CallRequest()
            {
                Type = CallRequestType.ConsumeItem,
                DetailCallback = callback
            };

            var result = SteamInventory.ConsumeItem(out SteamInventoryResult_t handle, instanceId, 1);
            if (!result)
            {
                Debug.LogWarning("Failed to request Consume Item from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
            else
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#ConsumeItem">https://partner.steamgames.com/doc/api/ISteamInventory#ConsumeItem</see>
        /// <para>Consumes items from a user's inventory. If the quantity of the given item goes to zero, it is permanently removed.</para>
        /// 
        /// <para>Once an item is removed it cannot be recovered. This is not for the faint of heart - if your game implements item removal at all, a high-friction UI confirmation process is highly recommended. 
        /// ConsumeItem can be restricted to certain item definitions or fully blocked via the Steamworks website to minimize support/abuse issues such as the classic "my brother borrowed my laptop and deleted all of my rare items".</para>
        /// </summary>
        /// <param name="instanceId">The instance id of the item to consume</param>
        /// <param name="quantity">The number to consume</param>
        /// <param name="callback">The callback to invoke when the request has been completed by Steam Inventory Services</param>
        /// <returns>True if the request was sent, false otherwise.</returns>
        public static bool ConsumeItem(SteamItemInstanceID_t instanceId, uint quantity, Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Consume Item before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            var callRequest = new CallRequest()
            {
                Type = CallRequestType.ConsumeItem,
                DetailCallback = callback
            };

            var result = SteamInventory.ConsumeItem(out SteamInventoryResult_t handle, instanceId, quantity);
            if (!result)
            {
                Debug.LogWarning("Failed to request Consume Item from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
            else
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#DeserializeResult">https://partner.steamgames.com/doc/api/ISteamInventory#DeserializeResult</see>
        /// <para>Deserializes a result set and verifies the signature bytes.</para>
        /// <para>With Heathen code this will automatically test ownership</para>
        /// </summary>
        /// <param name="buffer">The buffer to deserialize.</param>
        /// <param name="fromUser">The user the buffered data supposedly belongs to</param>
        /// <param name="callback">The callback to call when completed</param>
        /// <returns>Always returns true</returns>
        public static bool DeserializeResult(byte[] buffer, CSteamID fromUser, Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Deserialize Result before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.DeserializeResult,
                SteamUserId = fromUser,
                DetailCallback = callback
            };

            if (SteamInventory.DeserializeResult(out SteamInventoryResult_t handle, buffer, System.Convert.ToUInt32(buffer.Length)))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Deserialize Result from the Steamworks Steam Inventory service. This should never happen according to Steamworks documentaiton ... please contact Valve's partner support for more information.");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#DeserializeResult">https://partner.steamgames.com/doc/api/ISteamInventory#DeserializeResult</see>
        /// <para>Deserializes a result set and verifies the signature bytes.</para>
        /// <para>With Heathen code this will automatically test ownership</para>
        /// </summary>
        /// <param name="buffer">The buffer to deserialize.</param>
        /// <param name="fromUser">The user the buffered data supposedly belongs to</param>
        /// <param name="callback">The callback to call when completed</param>
        /// <returns>Always returns true</returns>
        public static bool DeserializeResult(byte[] buffer, SteamUserData fromUser, Action<bool, SteamItemDetails_t[]> callback)
        {
            return DeserializeResult(buffer, fromUser.id, callback);
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#DeserializeResult">https://partner.steamgames.com/doc/api/ISteamInventory#DeserializeResult</see>
        /// <para>Deserializes a result set and verifies the signature bytes.</para>
        /// <para>With Heathen code this will automatically test ownership</para>
        /// </summary>
        /// <param name="buffer">The buffer to deserialize.</param>
        /// <param name="fromUser">The user the buffered data supposedly belongs to</param>
        /// <param name="callback">The callback to call when completed</param>
        /// <returns>Always returns true</returns>
        public static bool DeserializeResult(byte[] buffer, ulong fromUser, Action<bool, SteamItemDetails_t[]> callback)
        {
            return DeserializeResult(buffer, new CSteamID(fromUser), callback);
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#ExchangeItems">https://partner.steamgames.com/doc/api/ISteamInventory#ExchangeItems</see>
        /// <para>Grant one item in exchange for a set of other items. It is recomended to refresh your item cashe after performing this operation</para>
        /// <para>This can be used to implement crafting recipes or transmutations, or items which unpack themselves into other items (e.g., a chest).</para>
        /// </summary>
        /// <param name="recipe">The recipe to use for the exchange</param>
        /// <param name="callback">A method to be called when the process compeltes, the provided bool represents success or failure while the array of detials indicates the resulting item created</param>
        /// <returns></returns>
        public static bool ExchangeItems(ItemExchangeRecipe recipe, Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Exchange Items before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.ExchangeItems,
                DetailCallback = callback
            };

            if (SteamInventory.ExchangeItems(out SteamInventoryResult_t handle, new SteamItemDef_t[] { recipe.ItemToGenerate }, new uint[] { 1 }, 1, recipe.GetInstanceArray(), recipe.GetQuantityArray(), System.Convert.ToUInt32(recipe.ItemsToConsume.Count)))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Exchange Items from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#ExchangeItems">https://partner.steamgames.com/doc/api/ISteamInventory#ExchangeItems</see>
        /// <para>Grant one item in exchange for a set of other items.</para>
        /// <para>This can be used to implement crafting recipes or transmutations, or items which unpack themselves into other items (e.g., a chest).</para>
        /// </summary>
        /// <param name="toGenerate">The item type to be produced</param>
        /// <param name="toBeConsumed">The items to be consumed in the process</param>
        /// <param name="callback">A method to be called when the process compeltes, the provided bool represents success or failure while the array of detials indicates the resulting item created</param>
        /// <returns></returns>
        public static bool ExchangeItems(SteamItemDef_t toGenerate, IEnumerable<ExchangeItemCount> toBeConsumed, Action<bool, SteamItemDetails_t[]> callback)
        {
            return ExchangeItems(new ItemExchangeRecipe(toGenerate, toBeConsumed), callback);
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#GenerateItems">https://partner.steamgames.com/doc/api/ISteamInventory#GenerateItems</see>
        /// <para>Grants specific items to the current user, for developers only.</para>
        /// <para>This API is only intended for prototyping - it is only usable by Steam accounts that belong to the publisher group for your game.</para>
        /// </summary>
        /// <param name="ItemDefinitions"></param>
        /// <param name="callback">A method to be called when the process compeltes, the provided bool represents success or failure while the array of detials indicates the resulting items created</param>
        /// <returns></returns>
        public static bool DeveloperOnlyGenerateItems(List<GenerateItemCount> ItemDefinitions, Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Generate Items before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.GenerateItems,
                DetailCallback = callback
            };

            var itemList = ItemDefinitions.ConvertAll((p) => { return p.ItemId; });
            var quaantityList = ItemDefinitions.ConvertAll((p) => { return p.Quantity; });

            if (SteamInventory.GenerateItems(out SteamInventoryResult_t handle, itemList.ToArray(), quaantityList.ToArray(), System.Convert.ToUInt32(ItemDefinitions.Count)))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Generate Items from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#GetItemsByID">https://partner.steamgames.com/doc/api/ISteamInventory#GetItemsByID</see>
        /// <para>Gets the state of a subset of the current user's inventory.</para>
        /// <para>The subset is specified by an array of item instance IDs.</para>
        /// </summary>
        /// <param name="InstanceIDs"></param>
        /// <param name="callback">A method to be called when the process compeltes, the provided bool represents success or failure while the array of detials indicates the resulting items updated</param>
        /// <returns></returns>
        public static bool GetItemsByID(IEnumerable<SteamItemInstanceID_t> InstanceIDs, Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Get Items By ID before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.GetItemsByID,
                DetailCallback = callback
            };

            if (SteamInventory.GetItemsByID(out SteamInventoryResult_t handle, InstanceIDs.ToArray(), System.Convert.ToUInt32(InstanceIDs.Count())))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Get Items By ID from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#GrantPromoItems">https://partner.steamgames.com/doc/api/ISteamInventory#GrantPromoItems</see>
        /// <para>Grant all potential one-time promotional items to the current user.</para>
        /// </summary>
        /// <param name="callback">A method to be called when the process compeltes, the provided bool represents success or failure while the array of detials indicates the resulting items created</param>
        /// <returns></returns>
        public static bool GrantPromoItems(Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Grant Promo Items before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.GrantPromoItems,
                DetailCallback = callback
            };

            if (SteamInventory.GrantPromoItems(out SteamInventoryResult_t handle))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Grant Promo Items from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#RequestEligiblePromoItemDefinitionsIDs">https://partner.steamgames.com/doc/api/ISteamInventory#RequestEligiblePromoItemDefinitionsIDs</see>
        /// <para>Request the list of "eligible" promo items that can be manually granted to the given user.</para>
        /// </summary>
        /// <param name="steamID">The Steam ID of the user to request the eligible promo items for.</param>
        /// <param name="callback">A method to be called when the process compeltes, the provided bool represents success or failure while the array of definitions indicates the resulting items</param>
        /// <returns></returns>
        public static bool RequestEligiblePromoItemDefinitionsIDs(CSteamID steamID, Action<bool, SteamItemDef_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Grant Promo Items before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            try
            {
                var call = SteamInventory.RequestEligiblePromoItemDefinitionsIDs(steamID);
                m_SteamInventoryEligiblePromoItemDefIDs.Set(call, (SteamInventoryEligiblePromoItemDefIDs_t param, bool bIOFailure) =>
                {
                    SteamItemDef_t[] results = new SteamItemDef_t[param.m_numEligiblePromoItemDefs];
                    uint count = System.Convert.ToUInt32(param.m_numEligiblePromoItemDefs);
                    if (SteamInventory.GetEligiblePromoItemDefinitionIDs(steamID, results, ref count))
                    {
                        callback.Invoke(true, results);
                    }
                    else
                    {
                        callback.Invoke(false, new SteamItemDef_t[] { });
                    }
                });
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#SerializeResult">https://partner.steamgames.com/doc/api/ISteamInventory#SerializeResult</see>
        /// <para>Serialized result sets contain a short signature which can't be forged or replayed across different game sessions.</para>
        /// </summary>
        /// <param name="InstanceIDs">The instance IDs of the current player's inventory to be serialized</param>
        /// <param name="callback">A method to be called when the process compeltes, the provided bool represents success or failure while the array is the resulting serialized data.</param>
        /// <returns></returns>
        public static bool SerializeResults(IEnumerable<SteamItemInstanceID_t> InstanceIDs, Action<bool, byte[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Get Items By ID before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.GetItemIDsToSerialize,
                SerializationCallback = callback
            };

            if (SteamInventory.GetItemsByID(out SteamInventoryResult_t handle, InstanceIDs.ToArray(), System.Convert.ToUInt32(InstanceIDs.Count())))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Get Items By ID from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#TransferItemQuantity">https://partner.steamgames.com/doc/api/ISteamInventory#TransferItemQuantity</see>
        /// <para>Transfer items between stacks within a user's inventory.</para>
        /// </summary>
        /// <param name="sourceItem">The source item to transfer.</param>
        /// <param name="quantityToMove">The quantity of the item that will be transfered from source to destination</param>
        /// <param name="destinationItem">The destination item. You can pass SteamItemInstanceID_t.Invalid to split the source stack into a new item stack with the requested quantity.</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static bool TransferQuantity(SteamItemInstanceID_t sourceItem, uint quantityToMove, SteamItemInstanceID_t destinationItem, Action<bool> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Stack Items before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.TransferItemQuantity,
                BoolCallback = callback
            };

            if (SteamInventory.TransferItemQuantity(out SteamInventoryResult_t handle, sourceItem, quantityToMove, destinationItem))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Stack Item (Transfer Item Quantity) from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#TransferItemQuantity">https://partner.steamgames.com/doc/api/ISteamInventory#TransferItemQuantity</see>
        /// <para>Transfer items between stacks within a user's inventory. This transfers to a null item e.g. creates a new item stack.</para>
        /// </summary>
        /// <param name="sourceItem">The source item to transfer.</param>
        /// <param name="quantityToMove">The quantity of the item that will be transfered from source to destination</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static bool SplitItems(SteamItemInstanceID_t sourceItem, uint quantityToMove, Action<bool> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Stack Items before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.TransferItemQuantity,
                BoolCallback = callback
            };

            if (SteamInventory.TransferItemQuantity(out SteamInventoryResult_t handle, sourceItem, quantityToMove, SteamItemInstanceID_t.Invalid))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Stack Item (Transfer Item Quantity) from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        /// <summary>
        /// <see cref="!:https://partner.steamgames.com/doc/api/ISteamInventory#TriggerItemDrop">https://partner.steamgames.com/doc/api/ISteamInventory#TriggerItemDrop</see>
        /// <para>Trigger an item drop if the user has played a long enough period of time.</para>
        /// </summary>
        /// <param name="dropListDefinition">This must refer to an itemdefid of the type "playtimegenerator". See the inventory schema for more details.</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static bool TriggerItemDrop(SteamItemDef_t dropListDefinition, Action<bool, SteamItemDetails_t[]> callback)
        {
            if (!RegisterCallbacks())
            {
                Debug.LogError("Attempted call to Trigger Item Drop before the Steam Foundation Manager Initialized");
                return false;
            }

            if (pendingCalls == null)
                pendingCalls = new Dictionary<SteamInventoryResult_t, CallRequest>();

            CallRequest callRequest = new CallRequest()
            {
                Type = CallRequestType.TriggerItemDrop,
                DetailCallback = callback
            };

            if (SteamInventory.TriggerItemDrop(out SteamInventoryResult_t handle, dropListDefinition))
            {
                if (pendingCalls.ContainsKey(handle))
                {
                    Debug.LogError("Attempting to add a callback listener on an existing handle. This sugests a handle leak.");
                    pendingCalls.Remove(handle);
                }

                Debug.Log("Generated new handle: " + handle.ToString());

                pendingCalls.Add(handle, callRequest);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to request Trigger Item Drop from the Steamworks Steam Inventory service. This call is not valid from a Steam Game Server");
                return false;
            }
        }

        private static void HandleEligiblePromoItemDefIDs(SteamInventoryEligiblePromoItemDefIDs_t param, bool bIOFailure)
        {
            Debug.Log("Handle Eligible Promo Item Def IDs default deligate called!");
        }
        #endregion

        private class CallRequest
        {
            public CallRequestType Type;
            public CSteamID SteamUserId;
            public Action<bool> BoolCallback;
            public Action<bool, SteamItemDetails_t[]> DetailCallback;
            public Action<bool, byte[]> SerializationCallback;
        }

        private enum CallRequestType
        {
            AddPromoItem,
            AddPromoItems,
            ConsumeItem,
            ExchangeItems,
            GenerateItems,
            GetAllItems,
            DeserializeResult,
            GetItemsByID,
            GrantPromoItems,
            GetItemIDsToSerialize,
            TransferItemQuantity,
            TriggerItemDrop
        }
    }
}
#endif