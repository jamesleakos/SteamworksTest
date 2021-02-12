#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{
    public class LobbyToMirrorConnectionCounter : MonoBehaviour
    {
        public SteamworksLobbySettings LobbySettings;
        [Header("Events")]
        [Tooltip("Occures when the number of players reported by mirror is greater than or equal\nto the number of members in the lobby.")]
        public UnityEvent LobbyCountMatched = new UnityEvent();
        [Tooltip("Occures after a previously reported Lobby Count Matched\nwhen the number of player reported by Mirror drops below the number of members in the lobby.")]
        public UnityEvent NetworkCountPending = new UnityEvent();

        public bool IsMemberCountMatched
        {
            get
            {
                return NetworkManager.singleton.numPlayers >= LobbySettings.lobbies[0].members.Count;
            }
        }

        public int PendingMembers
        {
            get
            {
                if (IsMemberCountMatched)
                    return 0;
                else
                    return LobbySettings.lobbies[0].members.Count - NetworkManager.singleton.numPlayers;
            }
        }

        private bool whereAllConnected = false;

        public void LateUpdate()
        {
            if (LobbySettings != null && LobbySettings.HasLobby && NetworkManager.singleton.isNetworkActive)
            {
                if(IsMemberCountMatched && !whereAllConnected)
                {
                    whereAllConnected = true;
                    LobbyCountMatched.Invoke();
                }
                else if (!IsMemberCountMatched && whereAllConnected)
                {
                    whereAllConnected = false;
                    NetworkCountPending.Invoke();
                }
            }
        }
    }
}
#endif