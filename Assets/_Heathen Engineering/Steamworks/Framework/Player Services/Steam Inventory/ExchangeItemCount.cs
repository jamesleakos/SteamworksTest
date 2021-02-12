#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [Serializable]
    /// <summary>
    /// Used for internal processes. This represents the item and quantity to be used in various operations such as item exchanges.
    /// </summary>
    public struct ExchangeItemCount
    {
        public SteamItemInstanceID_t InstanceId;
        public uint Quantity;
    }
}
#endif