#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.CommandSystem;
using HeathenEngineering.Tools;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HeathenEngineering.SteamApi.Networking.UI
{
    public class SteamworksLobbyChat : HeathenUIBehaviour
    {
        [Header("Settings")]
        public SteamworksLobbySettings LobbySettings;
        [Tooltip("Optional, if provided all messages will be tested for a command before they sent to Steam\nAll recieved messages will be tested for commands before they displayed")]
        public CommandParser CommandParser;
        public int maxMessages;
        public bool sendOnKeyCode = false;
        public KeyCode SendCode = KeyCode.Return;
        [Header("UI Elements")]
        public UnityEngine.UI.ScrollRect scrollRect;
        public RectTransform collection;
        public UnityEngine.UI.InputField input;        
        [Header("Templates")]
        public GameObject selfMessagePrototype;
        public GameObject othersMessagePrototype;
        public GameObject sysMessagePrototype;

        [Header("Events")]
        public UnityEvent NewMessageRecieved;

        [HideInInspector]
        public List<GameObject> messages;

        private void OnEnable()
        {
            if (LobbySettings != null && LobbySettings.Manager != null)
            {
                LobbySettings.OnChatMessageReceived.AddListener(HandleLobbyChatMessage);
            }
            else
            {
                Debug.LogWarning("Lobby Chat was unable to locate the Lobby Manager, A Heathen Steam Lobby Manager must register the Lobby Settings before this control can initalize.\nIf you have referenced a Lobby Settings object that is registered on a Heathen Lobby Manager then make sure the Heathen Lobby Manager is configured to execute before Lobby Chat.");
                enabled = false;
            }                
        }

        private void OnDisable()
        {
            if (LobbySettings != null)
            {
                LobbySettings.OnChatMessageReceived.RemoveListener(HandleLobbyChatMessage);
            }
        }

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == input.gameObject && Input.GetKeyDown(SendCode))
            {
                SendChatMessage();
            }
        }
        
        private void HandleLobbyChatMessage(LobbyChatMessageData data)
        {
            string errorMessage;
            if (CommandParser == null || !CommandParser.TryCallCommand(data.message, false, out errorMessage))
            {
                var isNewMessage = data.sender.userData.id.m_SteamID != SteamUser.GetSteamID().m_SteamID;
                var prototype = isNewMessage ? othersMessagePrototype : selfMessagePrototype;
                var go = Instantiate(prototype, collection);
                var msg = go.GetComponent<ILobbyChatMessage>();
                msg.RegisterChatMessage(data);

                messages.Add(go);

                Canvas.ForceUpdateCanvases();
                if (messages.Count > maxMessages)
                {
                    var firstLine = messages[0];
                    messages.Remove(firstLine);
                    Destroy(firstLine.gameObject);
                }
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;

                if (isNewMessage)
                    NewMessageRecieved.Invoke();
            }
        }

        /// <summary>
        /// Iterates over the messages list and destroys all messages
        /// </summary>
        public void ClearMessages()
        {
            while(messages.Count > 0)
            {
                var target = messages[0];
                messages.RemoveAt(0);
                Destroy(target);
            }
        }

        /// <summary>
        /// Send a chat message over the Steam Lobby Chat system
        /// </summary>
        /// <param name="message"></param>
        public void SendChatMessage(string message)
        {
            if (LobbySettings.InLobby)
            {
                var errorMessage = string.Empty;
                if (CommandParser == null || !CommandParser.TryCallCommand(message, true, out errorMessage))
                {
                    //If we are trying to parse a bad command let the player know
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        SendSystemMessage("", errorMessage);
                    }
                    else
                    {
                        LobbySettings.SendChatMessage(message);
                        input.ActivateInputField();
                    }
                }
            }
        }

        public void SendChatMessage()
        {
            if (!string.IsNullOrEmpty(input.text) && LobbySettings.InLobby)
            {
                SendChatMessage(input.text);
                input.text = string.Empty;
            }
            else
            {
                if (!LobbySettings.InLobby)
                    Debug.LogWarning("Attempted to send a lobby chat message without an established connection");
            }
        }

        /// <summary>
        /// This message is not sent over the network and only appears to this user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public void SendSystemMessage(string sender, string message)
        {
            var go = Instantiate(sysMessagePrototype, collection);
            var msg = go.GetComponent<ILobbyChatMessage>();
            msg.SetMessageText(sender, message);

            messages.Add(go);

            Canvas.ForceUpdateCanvases();
            if (messages.Count > maxMessages)
            {
                var firstLine = messages[0];
                messages.Remove(firstLine);
                Destroy(firstLine.gameObject);
            }
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
#endif