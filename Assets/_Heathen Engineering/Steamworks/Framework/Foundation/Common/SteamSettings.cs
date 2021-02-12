#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Events;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// <para>The root of Heathen Engieering's Steamworks system. <see cref="SteamSettings"/> provides access to all core funcitonality including stats, achievements, the friend system and the overlay system.</para>
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="SteamSettings"/> object is the root of Heathen Engineering's Steamworks kit.
    /// <see cref="SteamSettings"/> contains the configuration for the fundamental systems of the Steam API and provides access to all core funcitonality.
    /// You can easily access the active <see cref="SteamSettings"/> object any time via <see cref="current"/> a static member that is populated on initalization of the Steam API with the settings that are being used to configure it.</para>
    /// <para><see cref="SteamSettings"/> is divided into 2 major areas being <see cref="client"/> and <see cref="server"/>.
    /// The <see cref="client"/> member provides easy access to features and systems relivent for your "client" that is the applicaiton the end user is actually playing e.g. your game.
    /// This would include features such as overlay, friends, clans, stats, achievements, etc.
    /// <see cref="server"/> in contrast deals with tthe configuraiton of Steam Game Server features and only comes into play for server builds.
    /// Note that the <see cref="server"/> member and its funcitonality are stripped out of client builds, that is it is only accessable in a server build and in the Unity Editor</para>
    /// </remarks>
    [HelpURL("https://heathen-engineering.github.io/steamworks-documentation/class_heathen_engineering_1_1_steam_api_1_1_foundation_1_1_steam_settings.html")]
    [CreateAssetMenu(menuName = "Steamworks/Foundation/Steam Settings")]
    public class SteamSettings : ScriptableObject
    {
        public static SteamSettings current;

        /// <summary>
        /// The current applicaiton ID
        /// </summary>
        /// <remarks>
        /// <para>It is importnat that this is set to your game's AppId.
        /// Note that when working in Unity Editor you need to change this value in the <see cref="SteamSettings"/> object yoru using but also in the steam_appid.txt file located in the root of your project.
        /// You can read more about the steam_appid.txt file here <a href="https://heathen-engineering.mn.co/posts/steam_appidtxt"/></para>
        /// </remarks>
        [FormerlySerializedAs("ApplicationId")]
        public AppId_t applicationId = new AppId_t(0x0);
        /// <summary>
        /// Indicates rather or not the Steamworks API is initalized
        /// </summary>
        /// <remarks>
        /// <para>This value gets set to true when <see cref="Init"/> is called by the <see cref="SteamworksFoundationManager"/>.
        /// Note that if Steam API fails to initalize such as if the Steam client is not installed, running and logged in with a valid Steam user then the call to Init will fail and the <see cref="Initialized"/> value will remain false.</para>
        /// </remarks>
        public bool Initialized { get; private set; }
        /// <summary>
        /// Used in various processes to determin the level of detail to log
        /// </summary>
        public bool isDebugging = false;

        /// <summary>
        /// Gathers and stores information about a Steam Clan aka Steam Group
        /// </summary>
        /// <remarks>
        /// <para>Steam Clans also know as Steam Groups are a social feature of the Steam API.
        /// You can fetch a list <see cref="SteamClan"/> objects for all the clans/groups a user belongs to via the <see cref="GameClient.ListClans"/> method accessable from the <see cref="client"/> member.</para>
        /// </remarks>
        [Serializable]
        public class SteamClan
        {
            public CSteamID id;
            public string displayName;
            public string tagString;
            public SteamUserData Owner;
            public List<SteamUserData> Officers;
            
            public SteamClan(CSteamID clanId)
            {
                id = clanId;
                displayName = SteamFriends.GetClanName(id);
                tagString = SteamFriends.GetClanTag(id);
                Owner = SteamSettings.current.client.GetUserData(SteamFriends.GetClanOwner(id));

                var officerCount = SteamFriends.GetClanOfficerCount(id);
                Officers = new List<SteamUserData>();
                for (int i = 0; i < officerCount; i++)
                {
                    Officers.Add(SteamSettings.current.client.GetUserData(SteamFriends.GetClanOfficerByIndex(id, i)));
                }
            }

            /// <summary>
            /// Opens the Steam Overlay to the clan chat
            /// </summary>
            /// <remarks>
            /// Note that it is possible to handle chat messages in game however Valve doesn't offier sufficent events to track membership and messages well.
            /// As a result we strongly recomend you use Clan chat through the Steam Overlay ... if you are interested in learning more check Valve's documentation at
            /// <see href="https://partner.steamgames.com/doc/api/ISteamFriends#JoinClanChatRoom"/>
            /// </remarks>
            public void OpenChat()
            {
                SteamFriends.ActivateGameOverlayToUser("chat", id);
            }

            /// <summary>
            /// Returns a list of chat members in the clan chat.
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// This has some limitaitons as outlined in the Valve documetnation <see href="https://partner.steamgames.com/doc/api/ISteamFriends#GetClanChatMemberCount"/>.
            /// In particular large groups will not be able to be iterated over so will return 0 or incomplete lists of users. It is strongly recomended that you use the <see cref="OpenChat"/> feature to open the Steam Overlay to the chat as opposed to trying to manage it in game.
            /// </remarks>
            public List<SteamUserData> GetChatMembers()
            {
                var results = new List<SteamUserData>();
                var chatCount = SteamFriends.GetClanChatMemberCount(id);

                for (int i = 0; i < chatCount; i++)
                {
                    results.Add(SteamSettings.current.client.GetUserData(SteamFriends.GetChatMemberByIndex(id, i)));
                }

                return results;
            }
        }

#if !CONDITIONAL_COMPILE || (UNITY_SERVER || UNITY_EDITOR)
        /// <summary>
        /// configuration settings and features unique to the Server API
        /// </summary>
        /// <remarks>
        /// Note that this is not available in client builds and can only be accessed in server and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        [Serializable]
        public class GameServer
        {
            [Serializable]
            public class DisconnectedEvent : UnityEvent<SteamServersDisconnected_t> { }

            [Serializable]
            public class ConnectedEvent : UnityEvent<SteamServersConnected_t> { }

            [Serializable]
            public class FailureEvent : UnityEvent<SteamServerConnectFailure_t> { }

            [Header("System Configuraiton")]
            public bool autoInitalize = false;
            public bool enableMirror = true;

            [Header("Initalization Settings")]
            public uint ip = 0;
            [Tooltip("Port for the master server updater to listen on")]
            public ushort masterServerUpdaterPort = 27016;
            [Tooltip("Port for the server to do authentication on")]
            public ushort authenticationPort = 8766;
            [Tooltip("Port for the server to listen on")]
            public ushort serverPort = 27015;
            public string serverVersion = "1.0.0.0";
            [Tooltip("Only used if supporting spectators.")]
            public ushort spectatorPort = 27017;


            [Header("Server Settings")]
            [Tooltip("This will get set on logon and is how users will connect.")]
            public CSteamID serverId;
            [Tooltip("Should the system use the Game Server Authentication API.")]
            public bool usingGameServerAuthApi = false;
            [Tooltip("Heartbeats notify the master server of this servers details, if disabled your server will not list\nIf usingGameServerAuthApi is enabled heartbeats are always enabled..")]
            public bool enableHeartbeats = true;
            [Tooltip("If true the spectator port and server name will be used and configured on the server.")]
            public bool supportSpectators = false;
            [Tooltip("Only used if supporting spectators.")]
            public string spectatorServerName = "Usually GameDescription + Spectator";
            public bool anonymousServerLogin = false;
            [Tooltip("See https://steamcommunity.com/dev/managegameservers \nOr\nUse Anonymous Server Login")]
            public string gameServerToken = "See https://steamcommunity.com/dev/managegameservers";
            public bool isPasswordProtected = false;
            public string serverName = "My Server Name";
            [Tooltip("It is recomended to set this to the full name of your game.")]
            public string gameDescription = "Usually the name of your game";
            [Tooltip("Typically the same as the game's name e.g. its folder name.")]
            public string gameDirectory = "e.g. its folder name";
            public bool isDedicated = false;
            public int maxPlayerCount = 4;
            public int botPlayerCount = 0;
            public string mapName = "";
            [Tooltip("A delimited string used for Matchmaking Filtering e.g. CoolPeopleOnly,NoWagonsAllowed.\nThe above represents 2 data points matchmaking will then filter accordingly\n... see Heathen Game Server Browser for more informaiton.")]
            public string gameData;
            public List<StringKeyValuePair> rulePairs = new List<StringKeyValuePair>();

            [Header("Events")]
            public UnityEvent gameServerShuttingDown;
            public DisconnectedEvent disconnected;
            public ConnectedEvent connected;
            public FailureEvent failure;

            public Callback<SteamServerConnectFailure_t> steamServerConnectFailure;
            public Callback<SteamServersConnected_t> steamServersConnected;
            public Callback<SteamServersDisconnected_t> steamServersDisconnected;

            public void OnSteamServersDisconnected(SteamServersDisconnected_t param)
            {
                if (current.isDebugging)
                    Debug.Log("Server disconnected!");

                disconnected.Invoke(param);
            }

            public void OnSteamServersConnected(SteamServersConnected_t param)
            {
                if (current.isDebugging)
                    Debug.Log("Server connected!");

                connected.Invoke(param);
            }

            public void OnSteamServerConnectFailure(SteamServerConnectFailure_t param)
            {
                if (current.isDebugging)
                    Debug.Log("Server failure!" + param.m_eResult);

                failure.Invoke(param);
            }
        }

        /// <summary>
        /// Contains server side funcitonality and is not available in client builds
        /// </summary>
        /// <remarks>
        /// Note that this is not available in client builds and can only be accessed in server and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public GameServer server = new GameServer();
#endif

#if !CONDITIONAL_COMPILE || (!UNITY_SERVER || UNITY_EDITOR)
        /// <summary>
        /// configuration settings and features unique to the Client API
        /// </summary>
        /// <remarks>
        /// Note that this is not available in server builds and can only be accessed in client and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        [Serializable]
        public class GameClient
        {
            /// <summary>
            /// <para>A wrapper around common SteamAPI Overlay funcitonlity. This class is used to provide access to Overlay funcitons and features.</para>
            /// </summary>
            [Serializable]
            public class Overlay
            {
                /// <summary>
                /// The offset of the Steam notification panel relative to its <see cref="notificationPosition"/>
                /// </summary>
                public Vector2Int notificationInset;
                /// <summary>
                /// The position the notification pannel of the Steam overlay system will anchor
                /// </summary>
                public ENotificationPosition notificationPosition = ENotificationPosition.k_EPositionBottomRight;

                /// <summary>
                /// <para>A wrap around <see cref="Steamworks.SteamUtils.IsOverlayEnabled()"/></para>   
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamUtils#IsOverlayEnabled">https://partner.steamgames.com/doc/api/ISteamUtils#IsOverlayEnabled</a> for more information.
                /// </summary>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Checks if the Steam Overlay is running & the user can access it. The overlay process could take a few seconds to start & hook the game process, so this function will initially return false while the overlay is loading.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings.</para></description>
                /// <code>
                /// if(settings.Overlay.IsEnable)
                ///      Debug.Log("The overlay is enabled and ready for use!");
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public bool IsEnabled
                {
                    get
                    {
                        return Steamworks.SteamUtils.IsOverlayEnabled();
                    }
                }
                private static bool _OverlayOpen = false;

                /// <summary>
                /// <para>Indicates that the Steam Overlay is currently open.</para>   
                /// See <a href="https://partner.steamgames.com/doc/features/overlay">https://partner.steamgames.com/doc/features/overlay</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Indicates that the overlay is currently open.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings.</para></description>
                /// <code>
                /// if(settings.Overlay.IsOpen)
                ///      Debug.Log("The overlay is currently open.");
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public bool IsOpen
                {
                    get
                    {
                        return _OverlayOpen;
                    }
                }

                /// <summary>
                /// For internal use only
                /// </summary>
                /// <param name="data"></param>
                public void HandleOnOverlayOpen(GameOverlayActivated_t data)
                {
                    _OverlayOpen = data.m_bActive == 1;
                }

                /// <summary>
                /// <para>A wrap around <see cref="Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(CSteamId lobbyId)"/>.</para>   
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayInviteDialog">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayInviteDialog</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the overlay with the invite dialog populated for the indicated lobby.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.Invite(myLobby);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void Invite(CSteamID lobbyId)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);
                }

                /// <summary>
                /// <para>Opens the overlay to the current games store page.</para>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to the current apps store page.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStore();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenStore()
                {
                    OpenStore(SteamUtils.GetAppID(), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
                }

                /// <summary>
                /// <para>Opens the overlay to the store page of the provide app Id.</para>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to the current apps store page.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStore(settings.ApplicationId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="appId">The application id of the game you wish to open the store to</param>
                public void OpenStore(uint appId)
                {
                    OpenStore(new AppId_t(appId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
                }

                /// <summary>
                /// <para>Opens the overlay to the store page of the provide app Id with the provided overlay store flag.</para>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to the current apps store page.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStore(settings.ApplicationId.m_AppId, EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCartAndShow);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="appId">The application id of the game you wish to open the store to</param>
                /// <param name="flag">Modifies the behaviour of the store page when opened.</param>
                public void OpenStore(uint appId, EOverlayToStoreFlag flag)
                {
                    OpenStore(new AppId_t(appId), flag);
                }

                /// <summary>
                /// <para>Opens the overlay to the store page of the provide app Id with the provided overlay store flag.</para>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to the current apps store page.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStore(settings.ApplicationId.m_AppId, EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCartAndShow);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="appId">The application id of the game you wish to open the store to, See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#EOverlayToStoreFlag">https://partner.steamgames.com/doc/api/ISteamFriends#EOverlayToStoreFlag</a> for more details</param>
                public void OpenStore(AppId_t appId, EOverlayToStoreFlag flag)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToStore(appId, flag);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to the indicated dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStore("friends");
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
                public void Open(string dialog)
                {
                    Steamworks.SteamFriends.ActivateGameOverlay(dialog);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToWebPage">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToWebPage</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to the indicated web page.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenWebPage("http://www.google.com");
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
                public void OpenWebPage(string URL)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToWebPage(URL);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to friends dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenFriends();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenFriends()
                {
                    Steamworks.SteamFriends.ActivateGameOverlay("friends");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to community dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenCommunity();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenCommunity()
                {
                    Steamworks.SteamFriends.ActivateGameOverlay("community");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to players dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenPlayers();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenPlayers()
                {
                    Steamworks.SteamFriends.ActivateGameOverlay("players");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to settings dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenSettings();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenSettings()
                {
                    Steamworks.SteamFriends.ActivateGameOverlay("settings");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to offical game group dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenOfficialGameGroup();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenOfficialGameGroup()
                {
                    Steamworks.SteamFriends.ActivateGameOverlay("officialgamegroup");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to stats dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStats();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenStats()
                {
                    Steamworks.SteamFriends.ActivateGameOverlay("stats");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to achievements dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenArchievements();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenAchievements()
                {
                    Steamworks.SteamFriends.ActivateGameOverlay("achievements");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to chat dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenChat(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user to open the chat dialog with</param>
                public void OpenChat(CSteamID user)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToUser("Chat", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to profile dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenProfile(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user whoes profile you want to open</param>
                public void OpenProfile(CSteamID user)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToUser("steamid", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to a trade dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenTrade(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user who you want to trade with</param>
                public void OpenTrade(CSteamID user)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToUser("jointrade", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to stats dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStats(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user whoes stats you want to display</param>
                public void OpenStats(CSteamID user)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToUser("stats", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to achievements dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenAchievements(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The id of the user whoes achievements you want to display</param>
                public void OpenAchievements(CSteamID user)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToUser("achievements", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to friends add dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenFriendAdd(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The Id of the user you want to add as a friend</param>
                public void OpenFriendAdd(CSteamID user)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToUser("friendadd", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to friend remove dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenFriendRemove(userId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user you want to remove from friends</param>
                public void OpenFriendRemove(CSteamID user)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToUser("friendremove", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to request accept dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenRequestAccept(userId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user whoes request you want to accept</param>
                public void OpenRequestAccept(CSteamID user)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToUser("friendrequestaccept", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steam Overlay to request ignore dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenFriends();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user whoes request you want to ignore</param>
                public void OpenRequestIgnore(CSteamID user)
                {
                    Steamworks.SteamFriends.ActivateGameOverlayToUser("friendrequestignore", user);
                }
            }

            [Obsolete("Please use user as opposed to userData. This field will be removed in a future update.")]
#pragma warning disable IDE1006 // Naming Styles
            public SteamUserData userData { get => user; set => user = value; }
#pragma warning restore IDE1006 // Naming Styles
            /// <summary>
            /// cashe of the local users data
            /// </summary>
            /// <remarks>
            /// <para>
            /// This can be used to fetch the local users display name, persona state information, rich precense information, icon/avatar and to operate against the overlay for dialogs specific to this user such as the Invite dialog or Profile dialog.
            /// </para>
            /// </remarks>
            public SteamUserData user;
            
            public Overlay overlay = new Overlay();
            
            /// <summary>
            /// Count of players currently playing this game. This can be refreshed on demand by calling 
            /// </summary>
            public int lastKnownPlayerCount;
            /// <summary>
            /// A disctionary of <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> keyed on the <see cref="ulong"/> id of the user.
            /// </summary>
            public Dictionary<ulong, SteamUserData> knownUsers = new Dictionary<ulong, SteamUserData>();

            /// <summary>
            /// The list of <see cref="HeathenEngineering.SteamApi.Foundation.SteamStatData"/> tracked by the system
            /// </summary>
            public List<SteamStatData> stats = new List<SteamStatData>();
            /// <summary>
            /// The list of <see cref="HeathenEngineering.SteamApi.Foundation.SteamAchievementData"/> tracked by the system
            /// </summary>
            public List<SteamAchievementData> achievements = new List<SteamAchievementData>();

#pragma warning disable IDE0052 // Remove unread private members
            private CGameID m_GameID;
            private Callback<AvatarImageLoaded_t> avatarLoadedCallback;
            private Callback<PersonaStateChange_t> personaStateChange;
            private Callback<UserStatsReceived_t> m_UserStatsReceived;
            private Callback<UserStatsStored_t> m_UserStatsStored;
            /// <summary>
            /// For internal user
            /// </summary>
            public Callback<GameOverlayActivated_t> m_GameOverlayActivated;
            private Callback<UserAchievementStored_t> m_UserAchievementStored;
            private Callback<GameConnectedFriendChatMsg_t> m_GameConnectedFrinedChatMsg;
            private CallResult<NumberOfCurrentPlayers_t> m_OnNumberOfCurrentPlayersCallResult;
            private CallResult<FriendsGetFollowerCount_t> m_FriendsGetFollowerCount;
            private Dictionary<CSteamID, Action<SteamUserData, int>> FollowCallbacks = new Dictionary<CSteamID, Action<SteamUserData, int>>();
#pragma warning restore IDE0052 // Remove unread private members

            #region Events
            /// <summary>
            /// Occures on load of a Steam avatar
            /// </summary>
            [HideInInspector]
            public UnityAvatarImageLoadedEvent onAvatarLoaded;
            /// <summary>
            /// Ocucres on change of Steam User Data persona information
            /// </summary>
            [HideInInspector]
            public UnityPersonaStateChangeEvent onPersonaStateChanged;
            /// <summary>
            /// Occures when user stats and achievements are recieved from Valve
            /// </summary>
            [HideInInspector]
            public UnityUserStatsReceivedEvent onUserStatsReceived;
            /// <summary>
            /// Occures when user stats are stored to Valve
            /// </summary>
            [HideInInspector]
            public UnityUserStatsStoredEvent onUserStatsStored;
            /// <summary>
            /// Occures when the Steam overlay is activated / shown
            /// </summary>
            [HideInInspector]
            public UnityBoolEvent onOverlayActivated;
            /// <summary>
            /// Occures when Achivements are stored to Valve
            /// </summary>
            [HideInInspector]
            public UnityUserAchievementStoredEvent onAchievementStored;

            /// <summary>
            /// Occures when a chat message from a friend is recieved.
            /// </summary>
            [HideInInspector]
            public FriendChatMessageEvent onRecievedFriendChatMessage;

            /// <summary>
            /// Occures as the result of a RefreshPlayerCount call
            /// </summary>
            [HideInInspector]
            public UnityNumberOfCurrentPlayersResultEvent onNumberOfCurrentPlayersResult;
            #endregion

            #region Achievement System
            /// <summary>
            /// <para>Stores the stats and achievements to Valve</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#StoreStats">https://partner.steamgames.com/doc/api/ISteamUserStats#StoreStats</a>
            /// </summary>
            /// <remarks>
            /// This must be called in order to store updated stats to the backend. Note that this will get called when the game closes.
            /// </remarks>
            public void StoreStatsAndAchievements()
            {
                SteamUserStats.StoreStats();
            }

            /// <summary>
            /// Registeres the achievement callbacks
            /// </summary>
            public void RegisterAchievementsSystem()
            {
                // Cache the GameID for use in the Callbacks
                m_GameID = new CGameID(SteamUtils.GetAppID());

                m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(HandleUserStatsReceived);
                m_UserStatsStored = Callback<UserStatsStored_t>.Create(HandleUserStatsStored);
                m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(HandleAchievementStored);
                m_FriendsGetFollowerCount = CallResult<FriendsGetFollowerCount_t>.Create(HandleGetFollowerCount);
                m_OnNumberOfCurrentPlayersCallResult = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
            }

            /// <summary>
            /// <para>Requests the current users stats from Valve servers</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#RequestCurrentStats">https://partner.steamgames.com/doc/api/ISteamUserStats#RequestCurrentStats</a>
            /// </summary>
            /// <returns>Returns true if the server accepted the request.</returns>
            public bool RequestCurrentStats()
            {
                var handle = SteamUserStats.GetNumberOfCurrentPlayers();
                m_OnNumberOfCurrentPlayersCallResult.Set(handle);
                return SteamUserStats.RequestCurrentStats();
            }

            /// <summary>
            /// <para>
            /// Requests the count of current players from Steam for this application
            /// On return this will update the SteamSettings.LastKnownPlayerCount value
            /// and trigger the OnNumberOfCurrentPlayersResult event for the SteamSettings 
            /// object and the connected Foundation Manager
            /// </para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#GetNumberOfCurrentPlayers">https://partner.steamgames.com/doc/api/ISteamUserStats#GetNumberOfCurrentPlayers</a>
            /// </summary>
            public void RefreshPlayerCount()
            {
                var handle = SteamUserStats.GetNumberOfCurrentPlayers();
                m_OnNumberOfCurrentPlayersCallResult.Set(handle);
            }

            private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
            {
                if (!bIOFailure)
                {
                    if (pCallback.m_bSuccess == 1)
                        lastKnownPlayerCount = pCallback.m_cPlayers;

                    if (onNumberOfCurrentPlayersResult != null)
                        onNumberOfCurrentPlayersResult.Invoke(pCallback);
                }
            }

            private void HandleUserStatsReceived(UserStatsReceived_t pCallback)
            {
                // we may get callbacks for other games' stats arriving, ignore them
                if ((ulong)m_GameID == pCallback.m_nGameID)
                {
                    if (EResult.k_EResultOK == pCallback.m_eResult)
                    {
                        // load achievements
                        foreach (SteamAchievementData ach in achievements)
                        {
                            bool ret = SteamUserStats.GetAchievement(ach.achievementId.ToString(), out ach.isAchieved);
                            if (ret)
                            {
                                ach.displayName = SteamUserStats.GetAchievementDisplayAttribute(ach.achievementId, "name");
                                ach.displayDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.achievementId, "desc");
                                ach.hidden = SteamUserStats.GetAchievementDisplayAttribute(ach.achievementId, "hidden") == "1";
                            }
                            else
                            {
                                Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.achievementId + "\nIs it registered in the Steam Partner site?");
                            }
                        }

                        foreach (var stat in stats)
                        {
                            if (stat.DataType == SteamStatData.StatDataType.Float)
                            {
                                float rValue;
                                if (SteamUserStats.GetStat(stat.statName, out rValue))
                                    stat.InternalUpdateValue(rValue);
                                else
                                    Debug.LogWarning("SteamUserStats.GetAchievement failed for Stat " + stat.statName + "\nIs it registered in the Steam Partner site and the correct data type?");
                            }
                            else
                            {
                                int rValue;
                                if (SteamUserStats.GetStat(stat.statName, out rValue))
                                    stat.InternalUpdateValue(rValue);
                                else
                                    Debug.LogWarning("SteamUserStats.GetAchievement failed for Stat " + stat.statName + "\nIs it registered in the Steam Partner site and the correct data type?");
                            }
                        }

                        onUserStatsReceived.Invoke(pCallback);
                    }
                    else
                    {
                        Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
                    }
                }
            }

            private void HandleUserStatsStored(UserStatsStored_t pCallback)
            {
                // we may get callbacks for other games' stats arriving, ignore them
                if ((ulong)m_GameID == pCallback.m_nGameID)
                {
                    if (EResult.k_EResultOK == pCallback.m_eResult)
                    {
                        onUserStatsStored.Invoke(pCallback);
                    }
                    else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
                    {
                        // One or more stats we set broke a constraint. They've been reverted,
                        // and we should re-iterate the values now to keep in sync.
                        Debug.Log("StoreStats - some failed to validate, re-syncing data now in an attempt to correct.");
                        // Fake up a callback here so that we re-load the values.
                        UserStatsReceived_t callback = new UserStatsReceived_t();
                        callback.m_eResult = EResult.k_EResultOK;
                        callback.m_nGameID = (ulong)m_GameID;
                        HandleUserStatsReceived(callback);
                    }
                    else
                    {
                        Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
                    }
                }
            }

            private void HandleAchievementStored(UserAchievementStored_t pCallback)
            {
                // We may get callbacks for other games' stats arriving, ignore them
                if ((ulong)m_GameID == pCallback.m_nGameID)
                {
                    if (0 == pCallback.m_nMaxProgress)
                    {
                        Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
                    }
                    else
                    {
                        Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
                    }

                    onAchievementStored.Invoke(pCallback);
                }
            }

            private void HandleGetFollowerCount(FriendsGetFollowerCount_t param, bool bIOFailure)
            {
                if (FollowCallbacks.ContainsKey(param.m_steamID))
                {
                    var callback = FollowCallbacks[param.m_steamID];
                    if (param.m_eResult != EResult.k_EResultOK || bIOFailure)
                    {
                        if (callback != null)
                            callback.Invoke(GetUserData(param.m_steamID), -1);
                        FollowCallbacks.Remove(param.m_steamID);
                    }
                    else
                    {
                        if (callback != null)
                            callback.Invoke(GetUserData(param.m_steamID), param.m_nCount);
                        FollowCallbacks.Remove(param.m_steamID);
                    }
                }
            }

            /// <summary>
            /// <para>Unlocks the achievement.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
            /// </summary>
            public void UnlockAchievement(uint achievementIndex)
            {
                SteamAchievementData target = achievements[System.Convert.ToInt32(achievementIndex)];
                if (target != default(SteamAchievementData) && !target.isAchieved)
                    UnlockAchievementData(target);
            }

            /// <summary>
            /// <para>Unlocks the achievement.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
            /// </summary>
            public void UnlockAchievementData(SteamAchievementData data)
            {
                data.Unlock();
            }

            /// <summary>
            /// <para>Resets the unlock status of an achievmeent.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
            /// </summary>
            /// <param name="achievementIndex">The index of the registered achievment you wish to reset.</param>
            public void ClearAchievement(uint achievementIndex)
            {
                SteamAchievementData target = achievements[System.Convert.ToInt32(achievementIndex)];
                if (target != default(SteamAchievementData) && !target.isAchieved)
                    ClearAchievement(target);
            }

            /// <summary>
            /// <para>Resets the unlock status of an achievmeent.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
            /// </summary>
            /// <param name="data">The achievement you wish to reset.</param>
            public void ClearAchievement(SteamAchievementData data)
            {
                data.ClearAchievement();
            }
            #endregion

            #region Overlay System
            /// <summary>
            /// Called by the Heathen Steam Manager when the GameOverlayActivated callback is triggered
            /// </summary>
            /// <param name="data"></param>
            public void HandleOnOverlayOpen(GameOverlayActivated_t data)
            {
                overlay.HandleOnOverlayOpen(data);
                onOverlayActivated.Invoke(overlay.IsOpen);
            }

            /// <summary>
            /// <para>Sets the overlay notification positon.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#SetOverlayNotificationPosition">https://partner.steamgames.com/doc/api/ISteamUtils#SetOverlayNotificationPosition</a>
            /// </summary>
            /// <param name="position">The ENotificationPosition to set, see <a href="https://partner.steamgames.com/doc/api/steam_api#ENotificationPosition">https://partner.steamgames.com/doc/api/steam_api#ENotificationPosition</a> for details</param>
            public void SetNotificationPosition(ENotificationPosition position)
            {
                Steamworks.SteamUtils.SetOverlayNotificationPosition(overlay.notificationPosition);
                overlay.notificationPosition = position;
            }

            /// <summary>
            /// Updates the notification inset
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Y"></param>
            public void SetNotificationInset(int X, int Y)
            {
                Steamworks.SteamUtils.SetOverlayNotificationInset(X, Y);
                overlay.notificationInset = new Vector2Int(X, Y);
            }

            /// <summary>
            /// Updates the notification inset
            /// </summary>
            /// <param name="inset"></param>
            public void SetNotificationInset(Vector2Int inset)
            {
                Steamworks.SteamUtils.SetOverlayNotificationInset(inset.x, inset.y);
                overlay.notificationInset = inset;
            }
            #endregion

            #region Friend System
            /// <summary>
            /// Gets a list of <see cref="SteamUserData"/> representing the 'friends' that match the indicated friend flag.
            /// </summary>
            /// <remarks>
            /// For more details on what each flag option means please read <see href="https://partner.steamgames.com/doc/api/ISteamFriends#EFriendFlags"/> in Valve's documentation.
            /// </remarks>
            /// <param name="friendFlags">The category of friend list to return ... defaults to <see cref="EFriendFlags.k_EFriendFlagImmediate"/> aka the current user's "Regular" friends.</param>
            /// <returns>A list of the <see cref="SteamUserData"/> objects for each friend in this category.</returns>
            public List<SteamUserData> ListFriends(EFriendFlags friendFlags = EFriendFlags.k_EFriendFlagImmediate)
            {
                List<SteamUserData> friendList = new List<SteamUserData>();

                var friendCount = SteamFriends.GetFriendCount(friendFlags);
                for (int i = 0; i < friendCount; i++)
                {
                    friendList.Add(GetUserData(SteamFriends.GetFriendByIndex(i, friendFlags)));
                }

                return friendList;
            }

            /// <summary>
            /// Gets a list of <see cref="SteamClan"/> representing the 'clans' or 'groups' the local user is a member of
            /// </summary>
            /// <remarks>
            /// For more details on what a Steam Clan or Group is and how this method works please read <see href="https://partner.steamgames.com/doc/api/ISteamFriends#GetClanCount"/> in Valve's documentation.
            /// </remarks>
            /// <returns>A list of <see cref="SteamClan"/> objects for each clan/group the user is a member of</returns>
            public List<SteamClan> ListClans()
            {
                List<SteamClan> clanList = new List<SteamClan>();

                var clanCount = SteamFriends.GetClanCount();
                for (int i = 0; i < clanCount; i++)
                {
                    clanList.Add(new SteamClan(SteamFriends.GetClanByIndex(i)));
                }

                return clanList;
            }

            /// <summary>
            /// Gets the number of users following the specified user.
            /// </summary>
            /// <remarks>
            /// For more details please read <see href="https://partner.steamgames.com/doc/api/ISteamFriends#GetFollowerCount"/> in Valve's documentation.
            /// </remarks>
            /// <param name="followingThisUser">The user ID of the Steam user you want to count the followers of</param>
            /// <param name="callback">The call back to invoke when the count completes. This will take the form of HandleCallback(SteamUserData user, int followers)</param>
            public void GetFollowerCount(CSteamID followingThisUser, Action<SteamUserData, int> callback)
            {
                FollowCallbacks.Add(followingThisUser, callback);
                SteamFriends.GetFollowerCount(followingThisUser);
            }

            /// <summary>
            /// Sets a Rich Presence key/value for the current user that is automatically shared to all friends playing the same game. Each user can have up to 20 keys.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            /// <remarks>
            /// For details on Rich Presence or the usage of this method please read <see href="https://partner.steamgames.com/doc/api/ISteamFriends#SetRichPresence"/>
            /// </remarks>
            public bool SetRichPresence(string key, string value)
            {
                return SteamFriends.SetRichPresence(key, value);
            }

            /// <summary>
            /// Clears all of the current user's Rich Presence key/values.
            /// </summary>
            public void ClearRichPresence()
            {
                SteamFriends.ClearRichPresence();
            }

            /// <summary>
            /// For internal use, this regisers the Friend system and is called by the <see cref="HeathenEngineering.SteamApi.Foundation.SteamworksFoundationManager"/> as required.
            /// </summary>
            /// <param name="data"></param>
            public void RegisterFriendsSystem(SteamUserData data = null)
            {
                avatarLoadedCallback = Callback<AvatarImageLoaded_t>.Create(HandleAvatarLoaded);
                personaStateChange = Callback<PersonaStateChange_t>.Create(HandlePersonaStatReceived);
                m_GameConnectedFrinedChatMsg = Callback<GameConnectedFriendChatMsg_t>.Create(HandleGameConnectedFriendMsg);

                if (onRecievedFriendChatMessage == null)
                    onRecievedFriendChatMessage = new FriendChatMessageEvent();

                if (onAvatarLoaded == null)
                    onAvatarLoaded = new UnityAvatarImageLoadedEvent();

                if (onPersonaStateChanged == null)
                    onPersonaStateChanged = new UnityPersonaStateChangeEvent();

                if (data != null)
                    user = data;

                if (user == null)
                    user = ScriptableObject.CreateInstance<SteamUserData>();

                user.id = SteamUser.GetSteamID();

                knownUsers.Clear();
                knownUsers.Add(user.id.m_SteamID, user);

                int imageId = SteamFriends.GetLargeFriendAvatar(user.id);
                //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
                if (imageId > 0)
                {
                    ApplyAvatarImage(user, imageId);
                }
            }

            private void HandleAvatarLoaded(AvatarImageLoaded_t data)
            {
                if (knownUsers.ContainsKey(data.m_steamID.m_SteamID))
                {
                    SteamUserData u = knownUsers[data.m_steamID.m_SteamID];
                    ApplyAvatarImage(u, data.m_iImage);
                    if (u.OnAvatarLoaded == null)
                        u.OnAvatarLoaded = new UnityEngine.Events.UnityEvent();
                    u.OnAvatarLoaded.Invoke();
                }
                else
                {
                    var n = ScriptableObject.CreateInstance<SteamUserData>();
                    n.id = data.m_steamID;
                    knownUsers.Add(n.id.m_SteamID, n);
                    ApplyAvatarImage(n, data.m_iImage);
                    n.OnAvatarLoaded.Invoke();
                }

                onAvatarLoaded.Invoke(data);
            }

            private void HandleGameConnectedFriendMsg(GameConnectedFriendChatMsg_t callback)
            {
                string message;
                EChatEntryType chatType;
                SteamFriends.GetFriendMessage(callback.m_steamIDUser, callback.m_iMessageID, out message, 2048, out chatType);
                onRecievedFriendChatMessage.Invoke(GetUserData(callback.m_steamIDUser), message, chatType);
            }

            private void HandlePersonaStatReceived(PersonaStateChange_t pCallback)
            {
                SteamUserData target = null;
                if (knownUsers.ContainsKey(pCallback.m_ulSteamID))
                {
                    target = knownUsers[pCallback.m_ulSteamID];
                }
                else
                {
                    target = ScriptableObject.CreateInstance<SteamUserData>();
                    target.id = new CSteamID(pCallback.m_ulSteamID);
                    knownUsers.Add(target.id.m_SteamID, target);
                }

                switch (pCallback.m_nChangeFlags)
                {
                    case EPersonaChange.k_EPersonaChangeAvatar:
                        try
                        {
                            int imageId = SteamFriends.GetLargeFriendAvatar(target.id);
                            if (imageId > 0)
                            {
                                target.iconLoaded = true;
                                uint imageWidth, imageHeight;
                                SteamUtils.GetImageSize(imageId, out imageWidth, out imageHeight);
                                byte[] imageBuffer = new byte[4 * imageWidth * imageHeight];
                                if (SteamUtils.GetImageRGBA(imageId, imageBuffer, imageBuffer.Length))
                                {
                                    target.avatar.LoadRawTextureData(SteamUtilities.FlipImageBufferVertical((int)imageWidth, (int)imageHeight, imageBuffer));
                                    target.avatar.Apply();
                                    target.OnAvatarChanged.Invoke();
                                }
                            }
                        }
                        catch { }
                        break;
                    case EPersonaChange.k_EPersonaChangeComeOnline:
                        if (target.OnComeOnline != null)
                            target.OnComeOnline.Invoke();
                        if (target.OnStateChange != null)
                            target.OnStateChange.Invoke();
                        break;
                    case EPersonaChange.k_EPersonaChangeGamePlayed:
                        if (target.OnGameChanged != null)
                            target.OnGameChanged.Invoke();
                        if (target.OnStateChange != null)
                            target.OnStateChange.Invoke();
                        break;
                    case EPersonaChange.k_EPersonaChangeGoneOffline:
                        if (target.OnGoneOffline != null)
                            target.OnGoneOffline.Invoke();
                        if (target.OnStateChange != null)
                            target.OnStateChange.Invoke();
                        break;
                    case EPersonaChange.k_EPersonaChangeName:
                        if (target.OnNameChanged != null)
                            target.OnNameChanged.Invoke();
                        break;
                }

                onPersonaStateChanged.Invoke(pCallback);
            }

            private void ApplyAvatarImage(SteamUserData user, int imageId)
            {
                uint width, height;
                SteamUtils.GetImageSize(imageId, out width, out height);
                user.avatar = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                int bufferSize = (int)(width * height * 4);
                byte[] imageBuffer = new byte[bufferSize];
                SteamUtils.GetImageRGBA(imageId, imageBuffer, bufferSize);
                user.avatar.LoadRawTextureData(SteamUtilities.FlipImageBufferVertical((int)width, (int)height, imageBuffer));
                user.avatar.Apply();
                user.iconLoaded = true;
            }

            /// <summary>
            /// <para>Set rather or not the system should listen for Steam Friend chat messages</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#SetListenForFriendsMessages">https://partner.steamgames.com/doc/api/ISteamFriends#SetListenForFriendsMessages</a>
            /// </summary>
            /// <param name="isOn">True if you want to turn this feature on, otherwise false</param>
            /// <returns>True if successfully enabled, otherwise false</returns>
            public bool ListenForFriendMessages(bool isOn)
            {
                return SteamFriends.SetListenForFriendsMessages(isOn);
            }

            /// <summary>
            /// Send a Steam Friend Chat message to the indicated user
            /// </summary>
            /// <param name="friend"></param>
            /// <param name="message"></param>
            /// <returns></returns>
            public bool SendFriendChatMessage(SteamUserData friend, string message)
            {
                return friend.SendMessage(message);
            }

            /// <summary>
            /// <para>Send a Steam Friend Chat message to the indicated user</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage">https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage</a>
            /// </summary>
            /// <param name="friend">The friend you wish to send the message to</param>
            /// <param name="message">The message to be sent</param>
            /// <returns></returns>
            public bool SendFriendChatMessage(ulong friendId, string message)
            {
                return SendFriendChatMessage(new CSteamID(friendId), message);
            }

            /// <summary>
            /// <para>Send a Steam Friend Chat message to the indicated user</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage">https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage</a>
            /// </summary>
            /// <param name="friend">The friend you wish to send the message to</param>
            /// <param name="message">The message to be sent</param>
            /// <returns></returns>
            public bool SendFriendChatMessage(CSteamID friend, string message)
            {
                return SteamFriends.ReplyToFriendMessage(friend, message);
            }

            /// <summary>
            /// <para>Requests the users avatar from Valve
            /// This is handled by the Friends subsystem but can be called manually to force a refresh</para>  
            /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#GetLargeFriendAvatar">https://partner.steamgames.com/doc/api/ISteamFriends#GetLargeFriendAvatar</a>
            /// </summary>
            /// <param name="userData">The user whoes avatar should be updated</param>
            public void RefreshAvatar(SteamUserData userData)
            {
                int imageId = SteamFriends.GetLargeFriendAvatar(userData.id);
                //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
                if (imageId > 0)
                {
                    ApplyAvatarImage(userData, imageId);
                }
            }

            /// <summary>
            /// <para>Locates the Steam User Data for the user provided 
            /// This will read from the friends subsystem if availabel or will create a new entery if none is found</para>
            /// </summary>
            /// <param name="steamID">THe user to find or load as required.</param>
            /// <returns>The <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> for the indicated user.</returns>
            /// <example>
            /// <list type="bullet">
            /// <item>
            /// <description>Get the <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> of a Steam user whose ID is stored in myFriendId</description>
            /// <code>
            /// var userData = settings.GetUserData(myFriendId);
            /// Debug.Log("Located the user data for " + userData.DisplayName);
            /// </code>
            /// </item>
            /// </list>
            /// </example>
            public SteamUserData GetUserData(CSteamID steamID)
            {
                if (knownUsers.ContainsKey(steamID.m_SteamID))
                {
                    var n = knownUsers[steamID.m_SteamID];

                    int imageId = SteamFriends.GetLargeFriendAvatar(steamID);
                    //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
                    if (imageId > 0)
                    {
                        ApplyAvatarImage(n, imageId);
                    }

                    return n;
                }
                else
                {
                    SteamUserData n = CreateInstance<SteamUserData>();
                    n.id = steamID;

                    knownUsers.Add(steamID.m_SteamID, n);

                    int imageId = SteamFriends.GetLargeFriendAvatar(steamID);
                    //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
                    if (imageId > 0)
                    {
                        ApplyAvatarImage(n, imageId);
                    }

                    return n;
                }
            }

            public SteamUserData GetUserData(ulong steamID)
            {
                return GetUserData(new CSteamID(steamID));
            }
            #endregion
        }

        /// <summary>
        /// Contains client side funcitonality
        /// </summary>
        /// <remarks>
        /// Note that this is not available in server builds and can only be accessed in client and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public GameClient client = new GameClient();

        #region Depracated Members
        /// <summary>
        /// Depracated Use SteamSettings.client.overlay.notificationPosition
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.notificationPosition", false)]
        public ENotificationPosition NotificationPosition => client.overlay.notificationPosition;

        [Obsolete("UserData is deprecated and will be removed in a later update, use SteamSettings.client.userData in its place", false)]
        public SteamUserData UserData => client.userData;

        [Obsolete("LastKnownPlayerCount is deprecated and will be removed in a later update, use SteamSettings.client.lastKnownPlayerCount in its place", false)]
        public int LastKnownPlayerCount => client.lastKnownPlayerCount;

        [Obsolete("Overlay is deprecated and will be removed in a later update, use SteamSettings.client.overlay in its place", false)]
        public GameClient.Overlay Overlay => client.overlay;

        /// <summary>
        /// Depracated Use SteamSettings.client.knownUsers
        /// </summary>
        [Obsolete("Use SteamSettings.client.knownUsers", false)]
        public Dictionary<ulong, SteamUserData> KnownUsers => client.knownUsers;

        /// <summary>
        /// Depracated Use SteamSettings.client.onAvatarLoaded
        /// </summary>
        [Obsolete("Use SteamSettings.client.onAvatarLoaded", false)]
        public UnityAvatarImageLoadedEvent OnAvatarLoaded => client.onAvatarLoaded;
        /// <summary>
        /// Depracated Use SteamSettings.client.onPersonaStateChanged
        /// </summary>
        [Obsolete("Use SteamSettings.client.onPersonaStateChanged", false)]
        public UnityPersonaStateChangeEvent OnPersonaStateChanged => client.onPersonaStateChanged;
        /// <summary>
        /// Depracated Use SteamSettings.client.onUserStatsReceived
        /// </summary>
        [Obsolete("Use SteamSettings.client.onUserStatsReceived", false)]
        public UnityUserStatsReceivedEvent OnUserStatsReceived => client.onUserStatsReceived;
        /// <summary>
        /// Depracated Use SteamSettings.client.onUserStatsStored
        /// </summary>
        [Obsolete("Use SteamSettings.client.onUserStatsStored", false)]
        public UnityUserStatsStoredEvent OnUserStatsStored => client.onUserStatsStored;
        /// <summary>
        /// Depracated Use SteamSettings.client.onOverlayActivated
        /// </summary>
        [Obsolete("Use SteamSettings.client.onOverlayActivated", false)]
        public UnityBoolEvent OnOverlayActivated => client.onOverlayActivated;
        /// <summary>
        /// Depracated Use SteamSettings.client.onAchievementStored
        /// </summary>
        [Obsolete("Use SteamSettings.client.onAchievementStored", false)]
        public UnityUserAchievementStoredEvent OnAchievementStored => client.onAchievementStored;

        /// <summary>
        /// Depracated Use SteamSettings.client.onRecievedFriendChatMessage
        /// </summary>
        [Obsolete("Use SteamSettings.client.onRecievedFriendChatMessage", false)]
        public FriendChatMessageEvent OnRecievedFriendChatMessage => client.onRecievedFriendChatMessage;

        /// <summary>
        /// Depracated Use SteamSettings.client.onNumberOfCurrentPlayersResult
        /// </summary>
        [Obsolete("Use SteamSettings.client.onNumberOfCurrentPlayersResult", false)]
        public UnityNumberOfCurrentPlayersResultEvent OnNumberOfCurrentPlayersResult => client.onNumberOfCurrentPlayersResult;
        #endregion
#endif

        public void Init()
        {
            current = this;
#if !UNITY_SERVER
            Initialized = SteamAPI.Init();
#endif
#if UNITY_SERVER //|| UNITY_EDITOR
            EServerMode eMode = EServerMode.eServerModeNoAuthentication;
            if (server.usingGameServerAuthApi)
                eMode = EServerMode.eServerModeAuthenticationAndSecure;

            Initialized = Steamworks.GameServer.Init(server.ip, server.authenticationPort, server.serverPort, server.masterServerUpdaterPort, eMode, server.serverVersion);
#endif
        }
    }

    [Serializable]
    public struct StringKeyValuePair
    {
        public string key;
        public string value;
    }
}
#endif