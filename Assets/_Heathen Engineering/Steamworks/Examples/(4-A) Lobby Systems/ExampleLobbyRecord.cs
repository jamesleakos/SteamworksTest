#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Networking;
using Steamworks;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Networking.Demo
{
    /// <summary>
    /// An example implamentation of <see cref="LobbyRecordBehvaiour"/>.
    /// This is a UI behvaiour used to display <see cref="LobbyHunterLobbyRecord"/> objects to the user.
    /// </summary>
    public class ExampleLobbyRecord : LobbyRecordBehvaiour
    {
        public SteamworksLobbySettings LobbySettings;
        public UnityEngine.UI.Text lobbyId;
        public UnityEngine.UI.Text lobbySize;
        public UnityEngine.UI.Button connectButton;
        public UnityEngine.UI.Text buttonLabel;

        [Header("List Record")]
        public LobbyHunterLobbyRecord record;

        /// <summary>
        /// Registers the <see cref="LobbyHunterLobbyRecord"/> to the UI behaviour.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="lobbySettings"></param>
        public override void SetLobby(LobbyHunterLobbyRecord record, SteamworksLobbySettings lobbySettings)
        {
            Debug.Log("Setting lobby data for " + record.lobbyId);

            LobbySettings = lobbySettings;
            this.record = record;
            lobbyId.text = string.IsNullOrEmpty(record.name) ? "<unknown>" : record.name;

            if(record.metadata.ContainsKey("gamemode"))

            lobbySize.text = record.maxSlots.ToString();
        }

        /// <summary>
        /// Called when the UI object is selected in the by the user
        /// </summary>
        public void Selected()
        {
            OnSelected.Invoke(record.lobbyId);
        }

        private void Update()
        {
            if (record.lobbyId != CSteamID.Nil
                && LobbySettings.lobbies[0].id.m_SteamID == record.lobbyId.m_SteamID)
            {
                connectButton.interactable = false;
                buttonLabel.text = "You are here!";
            }
            else
            {
                connectButton.interactable = true;
                buttonLabel.text = "Join lobby!";
            }
        }
    }
}
#endif