#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

using System;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [Serializable]
    public class ValveItemDefPriceDataEntry
    {
        public string currencyCode = "EUR";
        public uint value = 100;

        public override string ToString()
        {
            return currencyCode + value.ToString("000");
        }
    }
}
#endif