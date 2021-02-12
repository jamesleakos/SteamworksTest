#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;

namespace HeathenEngineering.SteamApi.Networking
{
    public class LobbyChatMessageData
    {
        public EChatEntryType chatEntryType;
        public SteamLobby lobby;
        public SteamworksLobbyMember sender;
        public DateTime recievedTime;
        public string message;
    }
}
#endif
