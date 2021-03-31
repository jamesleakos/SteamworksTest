#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Networking;
using HeathenEngineering.SteamApi.Foundation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamApi.Networking.Demo
{
    /// <summary>
    /// Controls the state and function of the Find Match button aka the Quick Search button.
    /// </summary>
    public class FindMatchButton : MonoBehaviour
    {
        [FormerlySerializedAs("SteamSettings")]
        public SteamSettings steamSettings;
        [FormerlySerializedAs("LobbySettings")]
        public SteamworksLobbySettings lobbySettings;
        [FormerlySerializedAs("QuickMatchFilter")]
        public LobbyHunterFilter quickMatchFilter;
        public Button quickMatchButton;
        public Text quickMatchLabel;

        private void Start()
        {
            Debug.Log("FindMatchButton found " + steamSettings.client.user.DisplayName);
        }

        void Update()
        {
            quickMatchButton.interactable = !lobbySettings.Manager.IsSearching && !lobbySettings.Manager.IsQuickSearching;
            if (quickMatchButton.interactable)
            {
                if (!lobbySettings.InLobby)
                    quickMatchLabel.text = "Quick Match";
                else
                    quickMatchLabel.text = "Leave Lobby";
            }
            else
            {
                quickMatchLabel.text = "Searching";
            }
        }

        /// <summary>
        /// This is called when the user clicks the find match button
        /// </summary>
        public void SimpleFindMatch()
        {
            if (!lobbySettings.InLobby)
            {
                Debug.Log("[FindMatchButton.SimpleFindMatch] Startomg a quickmatch search for a lobby that matches the filter defined in [FindMatchButton.quickMatchFilter].");
                lobbySettings.Manager.QuickMatch(quickMatchFilter);
            }
            else
            {
                lobbySettings.LeaveAllLobbies();
            }
        }

        /// <summary>
        /// This is called by the Steamworks Lobby Manager when the Quick Match Failed event is invoked
        /// </summary>
        public void CreateMatch()
        {
            Debug.Log("[FindMatchButton.CreateMatch] Quick match found 0 matches, creating a new lobby with 4 slots.");
            lobbySettings.CreateLobby(Steamworks.ELobbyType.k_ELobbyTypePublic, 4);
        }

        public void GetHelp()
        {
            Application.OpenURL("https://partner.steamgames.com/doc/features/multiplayer/matchmaking");
        }

        public void KickMember(string id)
        {
            lobbySettings.KickMember(new Steamworks.CSteamID(ulong.Parse(id)));
        }

        public void OnEnterLobby(SteamLobby lobby)
        {
            if (lobby.IsHost)
            {
                lobby.Name = steamSettings.client.user.DisplayName + "'s Lobby";

                foreach(var stringSetting in quickMatchFilter.stringValues)
                {
                    lobby[stringSetting.key] = stringSetting.value;
                }

                foreach (var numberSetting in quickMatchFilter.numberValues)
                {
                    lobby[numberSetting.key] = numberSetting.value.ToString();
                }

                foreach (var nearSetting in quickMatchFilter.nearValues)
                {
                    lobby[nearSetting.key] = nearSetting.value.ToString();
                }
            }
            Debug.Log("Entered lobby: " + lobby.Name);
        }

        public void OnExitLobby(SteamLobby lobby)
        {
            Debug.Log("Exiting lobby: " + lobby.Name);
        }
    }
}
#endif