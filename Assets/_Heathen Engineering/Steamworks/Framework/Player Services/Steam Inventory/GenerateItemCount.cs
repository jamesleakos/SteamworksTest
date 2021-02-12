#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// Used for internal purpses.
    /// </summary>
    public struct GenerateItemCount
    {
        public SteamItemDef_t ItemId;
        public uint Quantity;
    }
}
#endif