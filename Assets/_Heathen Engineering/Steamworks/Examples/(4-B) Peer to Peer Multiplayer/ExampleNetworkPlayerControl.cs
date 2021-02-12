#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using Steamworks;
using UnityEngine;
using Mirror;
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.Foundation.UI;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamApi.Networking.Demo
{
    /// <summary>
    /// <para>Demonstrates the use of the common Network Behaviour and Steam Network Behaviour features</para>
    /// <para>Note that this is for demonstration purposes in a developer focused pack. It is not an example of production quality best practice. For information on production quality best practice please refer to Mirror's community guidance and available documentation.</para>
    /// </summary>
    public class ExampleNetworkPlayerControl : NetworkBehaviour
    {
        public SteamSettings steamSettings;
        [FormerlySerializedAs("LobbySettings")]
        public SteamworksLobbySettings lobbySettings;
        public Transform selfTransform;
        public SteamUserData authorityUser;
        public float speed = 0.25f;
        public SteamUserFullIcon SteamIcon;
        /// <summary>
        /// Simply for demonstration purpses only
        /// </summary>
        [SyncVar(hook = nameof(HandleSteamIdUpdate))]
        public ulong steamId = CSteamID.Nil.m_SteamID;

        /// <summary>
        /// Note that this is only called on a client when the server has updated the SyncValue
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        private void HandleSteamIdUpdate(ulong oldId, ulong newId)
        {
            //Gets called when the steamId is being synced by the server
            Debug.Log("New steam ID recieved from the server: previous value = " + steamId.ToString() + " new value = " + newId.ToString());
            steamId = newId;
            SetSteamIconData();
        }

        /// <summary>
        /// This is called when the SteamId updates for this user
        /// The HandleSteamIdUpdate method calls it and that method is called by the network system when the steamId field is updated.
        /// </summary>
        private void SetSteamIconData()
        {
            //If its an invalid ID dont bother doign anything
            if (steamId == CSteamID.Nil.m_SteamID)
                return;

            authorityUser = steamSettings.client.GetUserData(new CSteamID(steamId));
            SteamIcon.LinkSteamUser(authorityUser);

            Debug.Log("Linking persona data for: [" + steamId.ToString() + "] " + (string.IsNullOrEmpty(authorityUser.DisplayName) ? "Unknown User" : authorityUser.DisplayName));
        }

        /// <summary>
        /// Only called on the client that has authority over this behaviour
        /// </summary>
        public override void OnStartAuthority()
        {
            Debug.Log("Player Controller On Start Authority has been called!");
            
            steamId = SteamUser.GetSteamID().m_SteamID;
            SetSteamIconData();

            //Have the authority instance of this object call the server and notify it of the local users CSteamID
            CmdSetSteamId(SteamUser.GetSteamID().m_SteamID);
        }
        
        private void Update()
        {
            //Only do this if we have authority
            if (hasAuthority)
            {
                //Simple input controls for moving left and right, this is simply to help prove that updates are being sent between clients
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                {
                    selfTransform.position += Vector3.left * speed * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                {
                    selfTransform.position += Vector3.right * speed * Time.deltaTime;
                }
            }
        }

        [Command(channel = 0)]
        void CmdSetSteamId(ulong steamId)
        {
            Debug.Log("The server received a request from connection " + connectionToClient.connectionId + " to set the SteamId of this object to " + steamId.ToString());

            this.steamId = steamId;
        }
    }
}
#endif