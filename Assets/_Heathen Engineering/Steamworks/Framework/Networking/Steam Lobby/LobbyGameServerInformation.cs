#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;

namespace HeathenEngineering.SteamApi.Networking
{
    /// <summary>
    /// Data stored for a lobby's Game Server
    /// </summary>
    /// <remarks>
    /// <para>
    /// In the case of a P2P situation only the serverId will be set and it will be the ID of the lobby host.
    /// In the case of a Steam Game Server both the IP/Port and serverId may be set but the serverId should always be favored and will be the ID of the server suitable for use with Heahten's Steam Game Server Transport.
    /// In the case only the IP and Port information is set this indicates that the server is a traditional TCP/IP based non Steam Game Server and must be connected to using either a UDP or TCP transport.
    /// </para>
    /// </remarks>
    public struct LobbyGameServerInformation
    {
        public uint ipAddress;
        public ushort port;
        public CSteamID serverId;

        public string StringAddress
        {
            get { return SteamUtilities.IPUintToString(ipAddress); }
            set { ipAddress = SteamUtilities.IPStringToUint(value); }
        }

        public string StringPort
        {
            get { return port.ToString(); }
            set
            {
                ushort tPort = 0;
                if (ushort.TryParse(value, out tPort))
                    port = tPort;
            }
        }
    }
}
#endif