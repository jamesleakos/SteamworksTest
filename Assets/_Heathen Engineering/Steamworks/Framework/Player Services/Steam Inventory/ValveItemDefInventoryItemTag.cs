#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

using System;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [Serializable]
    public class ValveItemDefInventoryItemTag
    {
        public string category;
        public string tag;

        public override string ToString()
        {
            return category + ":" + tag;
        }
    }
}
#endif