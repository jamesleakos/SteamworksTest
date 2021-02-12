#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using System;

namespace HeathenEngineering.SteamApi.Networking
{
    [Serializable]
    public class ServerPlayerEntry
    {
        public string name;
        public int score;
        public TimeSpan timePlayed;
    }
}
#endif