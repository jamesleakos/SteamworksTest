#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Events;
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.Networking.UI;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Networking.Demo
{
    /// <summary>
    /// Demonstrates the concept of commands
    /// This is an example command handler meaning it defines the methods that get called when a command is detected.
    /// </summary>
    public class CommandHandler : MonoBehaviour
    {
        public SteamSettings steamSettings;
        public SteamworksLobbyChat lobbyChat;
        public GameEvent sayMyNameEvent;
        public StringGameEvent echoThisEvent;

        private void Start()
        {
            sayMyNameEvent.AddListener(SayMyName);
            echoThisEvent.AddListener(echoThisMessage);
        }

        private void echoThisMessage(EventData<string> message)
        {
            lobbyChat.SendSystemMessage("Heathen Engineer", "You want me to say \"" + message + "\"\nOkay " + message.value.ToUpper() + "!!!");
        }

        private void SayMyName(EventData data)
        {
            lobbyChat.SendSystemMessage("Heathen Engineer", "Your name is " + steamSettings.client.user.DisplayName);
        }
    }
}
#endif
