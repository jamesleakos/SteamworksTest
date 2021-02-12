#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using HeathenEngineering.SteamApi.Networking;
using Mirror;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Networking.Demo
{
    /// <summary>
    /// Provides functionality to the P2P demo scene's buttons
    /// </summary>
    public class NetworkingTestScript : MonoBehaviour
    {
        public SteamworksLobbySettings LobbySettings;
        public UnityEngine.UI.Button ServerButton;
        public UnityEngine.UI.Text ButtonText;

        private void Update()
        {
            if(LobbySettings.lobbies.Count > 0 && LobbySettings.lobbies[0].IsHost)
            {
                ServerButton.interactable = true;
                if(NetworkManager.singleton.isNetworkActive)
                {
                    ButtonText.text = "Stop Hosting";
                }
                else
                {
                    ButtonText.text = "Start Hosting";
                }
            }
            else
            {
                if (NetworkManager.singleton.isNetworkActive)
                {
                    ServerButton.interactable = true;
                    ButtonText.text = "Stop Client";
                }
                else
                {
                    ServerButton.interactable = false;
                    ButtonText.text = "Waiting for host";
                }
            }
        }

        public void OnButtonClick()
        {
            if (LobbySettings.lobbies.Count > 0 && LobbySettings.lobbies[0].IsHost)
            {
                if (NetworkManager.singleton.isNetworkActive)
                {
                    Debug.Log("Stoping the host!");
                    NetworkManager.singleton.StopHost();

                    /***********************************************
                     * When we disconnect from a server we also 
                     * disconnect from the lobby if we have not 
                     * already done so
                     * 
                     * A Steam lobby cannot start a server twice so
                     * if we remain in this lobby we will never get
                     * another call to join a server
                     ***********************************************/

                    Debug.Log("Leaving the lobby!");
                    LobbySettings.lobbies[0].Leave();
                }
                else
                {
                    NetworkManager.singleton.StartHost();
                }
            }
            else
            {
                if (NetworkManager.singleton.isNetworkActive)
                {
                    Debug.Log("Stoping the client!");
                    NetworkManager.singleton.StopClient();

                    /***********************************************
                     * When we disconnect from a server we also 
                     * disconnect from the lobby if we have not 
                     * already done so
                     * 
                     * A Steam lobby cannot start a server twice so
                     * if we remain in this lobby we will never get
                     * another call to join a server
                     ***********************************************/

                    Debug.Log("Leaving the lobby!");
                    LobbySettings.lobbies[0].Leave();
                }
            }
        }

        public void OnGameReady()
        {
            Debug.Log("Recieved the On Game Ready notification from the Steam Lobby Manager!");

            if (LobbySettings.lobbies.Count > 0 && !LobbySettings.lobbies[0].IsHost)
            {
                Debug.Log("Starting up the Network Client in responce to Steam Lobby Manager's On Game Ready message!");
                //The Heathen Steam Transport uses CSteamIDs as addresses e.g. we are connecting to Steam Users not IP addresses
                NetworkManager.singleton.networkAddress = LobbySettings.lobbies[0].Owner.userData.id.m_SteamID.ToString();
                NetworkManager.singleton.StartClient();
            }
        }

        public void GetHelp()
        {
            Application.OpenURL("https://github.com/vis2k/Mirror");
        }
    }
}
#endif