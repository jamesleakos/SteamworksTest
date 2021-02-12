#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.PlayerServices;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices.Demo
{
    /// <summary>
    /// Demonstrates the use of the <see cref="SteamworksPlayerInventory"/> system
    /// </summary>
    public class SteamworksInventoryDemonstrations : MonoBehaviour
    {
        /// <summary>
        /// Fetch all items and print the results to the console.
        /// </summary>
        public void getAllTest()
        {
            if (SteamworksPlayerInventory.GetAllItems((status, results) =>
             {
                 if (status)
                 {
                     Debug.Log("Query returned " + results.Length + " items.");
                 }
                 else
                 {
                     Debug.Log("Query failed.");
                 }
             }))
            {
                Debug.Log("Get All Items request sent to Steam.");
            }
            else
            {
                Debug.Log("Get All Items failed to send to Steam.");
            }
        }

        /// <summary>
        /// Grant promotion items and print the results to the console.
        /// </summary>
        public void grantPromoTest()
        {
            if (SteamworksPlayerInventory.GrantPromoItems((status, results) =>
             {
                 if (status)
                 {
                     Debug.Log("Granted " + results.Length + " promo items.");
                 }
                 else
                 {
                     Debug.Log("Grant Promo Items Failed.");
                 }
             }))
            {
                Debug.Log("Grant Promo Items request sent to Steam.");
            }
            else
            {
                Debug.Log("Grant Promo Items failed to send to Steam.");
            }
        }
    }
}
#endif
