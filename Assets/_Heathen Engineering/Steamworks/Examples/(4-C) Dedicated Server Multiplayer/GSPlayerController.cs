#if !DISABLESTEAMWORKS && MIRROR
using HeathenEngineering.SteamApi.Foundation;
using Mirror;
using Steamworks;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Networking.Demo
{
    /// <summary>
    /// A basic demonstration of having the owning client report informaiton to the server which will decimnate that information to all clients
    /// </summary>
    public class GSPlayerController : NetworkBehaviour
    {
        public SteamSettings steamSettings;
        /// <summary>
        /// This will get set at runtime
        /// </summary>
        [Tooltip("This will get set at runtime")]
        public SteamUserData SteamUser;
        /// <summary>
        /// This will get set at runtime
        /// This is only put here so you can see it in the inspector
        /// </summary>
        [Tooltip("This will get set at runtime")]
        public string UserName;
        /// <summary>
        /// This will get set at runtime
        /// This is only put here so you can see it in the inspector
        /// </summary>
        [Tooltip("This will get set at runtime")]
        public Texture2D Avatar;
        /// <summary>
        /// This will get set at runtime
        /// This is only put here so you can see it in the inspector
        /// </summary>
        [Tooltip("This will get set at runtime")]
        public ulong SteamId;

        [Tooltip("This is simply to demonstrate the use of the avatar texture")]
        public UnityEngine.UI.RawImage rawImage;

        void Start()
        {
            if(hasAuthority)
            {
                //As we own this object we know its CSteamID so tell the server so it can inform each client including this client
                CmdRegisterSteamUser(Steamworks.SteamUser.GetSteamID().m_SteamID);
            }
        }
        
        /// <summary>
        /// This executes on the server being called from the client that has authority over this object
        /// </summary>
        /// <param name="SteamId"></param>
        [Command]
        private void CmdRegisterSteamUser(ulong SteamId)
        {
            //Have the server execute the UpdateSteamUserData method on each client for this object
            RpcUpdateSteamUserData(SteamId);
        }

        /// <summary>
        /// This executes on each client having been called from the server
        /// </summary>
        /// <param name="SteamId"></param>
        [ClientRpc]
        private void RpcUpdateSteamUserData(ulong SteamId)
        {
            //The server recieved the Steam ID
            SteamUser = steamSettings.client.GetUserData(new CSteamID(SteamId));
            //This is the Steam User's name as it appears in Steam social contexts
            UserName = SteamUser.DisplayName;
            //This is the Steam User's avatar e.g. the profile image
            Avatar = SteamUser.avatar;
            rawImage.texture = SteamUser.avatar;
            //And this is the Steam User's ID e.g the number which represents the profile
            this.SteamId = SteamUser.id.m_SteamID;
        }
    }
}
#endif
