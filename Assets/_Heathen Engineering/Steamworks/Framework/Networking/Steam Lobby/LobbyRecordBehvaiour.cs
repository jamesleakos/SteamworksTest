#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Networking
{
    public class LobbyRecordBehvaiour : MonoBehaviour
    {
        public UnitySteamIdEvent OnSelected;

        public virtual void SetLobby(LobbyHunterLobbyRecord record, SteamworksLobbySettings lobbySettings)
        {

        }
    }
}
#endif
