#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [Serializable]
    public class ValveItemDefPromoRule
    {
        public ValveItemDefPromoRuleType type = ValveItemDefPromoRuleType.played;
        public AppId_t app;
        public uint minutes;
        public SteamAchievementData achievment;

        public override string ToString()
        {
            switch(type)
            {
                case ValveItemDefPromoRuleType.manual:
                    return "manual";
                case ValveItemDefPromoRuleType.owns:
                    return "owns:" + app.ToString();
                case ValveItemDefPromoRuleType.played:
                    return "played:" + app.ToString() + "/" + minutes.ToString();
                case ValveItemDefPromoRuleType.achievement:
                    return "ach:" + achievment.achievementId;
                default:
                    return string.Empty;
            }
        }
    }
}
#endif