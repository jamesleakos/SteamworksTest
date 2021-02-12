#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// <para>This replaces the SteamManager concept from classic Steamworks.NET</para>
    /// <para>The <see cref="SteamworksFoundationManager"/> initalizes the client SteamAPI and handles callbacks for the system. For the convenance of users using a singleton model this class also provides a <see cref="Instance"/> static member and wraps all major funcitons and event of the <see cref="SteamSettings"/> object.</para>
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="SteamworksFoundationManager"/> is the core compoenent to the Heathen Steamworks kit and replaces the funcitonality present in Steamworks.NET's SteamManager.
    /// The primary funciton of the manager is to operate the update loop required by the Steam API and to handle and direct callbacks from the Steam API.</para>
    /// <para>It is strongly advised that you never unload or reload the <see cref="SteamworksFoundationManager"/> for example you should not place the <see cref="SteamworksFoundationManager"/> in your title scene because that scene will be unloaded and reloaded multiple times.
    /// Even if you mark the object as Do Not Destroy on Load, on reload of the title or similar scene Unity will create a second <see cref="SteamworksFoundationManager"/> creating issues with the memory it manages.</para>
    /// <para>The recomended approch is place your <see cref="SteamworksFoundationManager"/> and any other "manager" in a bootstrap scene that loads first and is never reloaded through the life of the game.
    /// This will help insure that through the life of your users play session 1 and exsactly 1 <see cref="SteamworksFoundationManager"/> is created and never destroyed.
    /// While there are other approches you could take to insure this using a simple bootstrap scene is typically the simplest. For more information on this subject see <a href="https://heathen-engineering.mn.co/posts/scenes-management-quick-start"/>.
    /// This article referes to another tool available from Heathen Engineering however the concepts within apply rather or not your using that tool.</para>
    /// </remarks>
    [HelpURL("https://heathen-engineering.github.io/steamworks-documentation/class_heathen_engineering_1_1_steam_api_1_1_foundation_1_1_steamworks_foundation_manager.html")]
    [DisallowMultipleComponent]
    public class SteamworksFoundationManager : MonoBehaviour
    {
#region Editor Exposed Values
        /// <summary>
        /// Reference to the <see cref="SteamSettings"/> object containing the configuration to be used for intialization of the Steam API
        /// </summary>
        [FormerlySerializedAs("Settings")]
        public SteamSettings settings;
        /// <summary>
        /// Should this <see cref="GameObject"/> be marked as Do Not Distroy On Load ... this is class Unity feature used with single scene game architectures ... that is games taht only have 1 scene active at a time.
        /// </summary>
        public BoolReference _doNotDistroyOnLoad = new BoolReference(false);
        /// <summary>
        /// An event raised when the Steam API has been intialzied
        /// </summary>
        [FormerlySerializedAs("OnSteamInitalized")]
        public UnityEvent onSteamInitalized;
        /// <summary>
        /// An event raised when an error has occred while intializing the Steam API
        /// </summary>
        [FormerlySerializedAs("OnSteamInitalizationError")]
        public UnityStringEvent onSteamInitalizationError;

#if !CONDITIONAL_COMPILE || !UNITY_SERVER
        /// <summary>
        /// An event raised when the overlay is opened by the user.
        /// This is only avilable in client builds.
        /// </summary>
        [FormerlySerializedAs("OnOverlayActivated")]
        public UnityBoolEvent onOverlayActivated;
        /// <summary>
        /// An event raised when user stats are updated.
        /// This is only avilable in client builds.
        /// </summary>
        [FormerlySerializedAs("OnUserStatsRecieved")]
        public UnityUserStatsReceivedEvent onUserStatsRecieved;
        /// <summary>
        /// An event raised when user stats are stored to the server.
        /// This is only avilable in client builds.
        /// </summary>
        [FormerlySerializedAs("OnUserStatsStored")]
        public UnityUserStatsStoredEvent onUserStatsStored;
        /// <summary>
        /// An event raised when number of current players of this game has been updated in the local cashe.
        /// This is only avilable in client builds.
        /// </summary>
        [FormerlySerializedAs("OnNumberOfCurrentPlayersResult")]
        public UnityNumberOfCurrentPlayersResultEvent onNumberOfCurrentPlayersResult;
        /// <summary>
        /// An event raised when achievements have been stored to the server.
        /// This is only avilable in client builds.
        /// </summary>
        [FormerlySerializedAs("OnAchievementStored")]
        public UnityUserAchievementStoredEvent onAchievementStored;
        /// <summary>
        /// An event raised when a user avatar has been loaded e.g. the image represening a user.
        /// This is only avilable in client builds.
        /// </summary>
        [FormerlySerializedAs("OnSteamIOnAvatarLoadednitalized")]
        public UnityAvatarImageLoadedEvent onAvatarLoaded;
        /// <summary>
        /// An event raised when information about a Steam User's persona state has been updated.
        /// This is only avilable in client builds.
        /// </summary>
        [FormerlySerializedAs("OnPersonaStateChanged")]
        public UnityPersonaStateChangeEvent onPersonaStateChanged;
        /// <summary>
        /// An event raised when when a chat message from a friend has been recieved.
        /// This is only avilable in client builds.
        /// </summary>
        [FormerlySerializedAs("OnRecievedFriendChatMessage")]
        public FriendChatMessageEvent onRecievedFriendChatMessage;
#endif
#if !CONDITIONAL_COMPILE || UNITY_SERVER || UNITY_EDITOR
        /// <summary>
        /// An event raised when the Steam Game Server shut down has been called.
        /// This is only avilable in server builds.
        /// </summary>
        [FormerlySerializedAs("GameServerShuttingDown")]
        public UnityEvent gameServerShuttingDown;
        /// <summary>
        /// An event raised when by Steam debugging on disconnected.
        /// This is only avilable in server builds.
        /// </summary>
        [FormerlySerializedAs("Disconnected")]
        public SteamSettings.GameServer.DisconnectedEvent disconnected;
        /// <summary>
        /// An event raised by Steam debugging on connected.
        /// This is only avilable in server builds.
        /// </summary>
        [FormerlySerializedAs("Connected")]
        public SteamSettings.GameServer.ConnectedEvent connected;
        /// <summary>
        /// An event raised by Steam debugging on failure.
        /// This is only avilable in server builds.
        /// </summary>
        [FormerlySerializedAs("Failure")]
        public SteamSettings.GameServer.FailureEvent failure;
#if MIRROR
        /// <summary>
        /// A reference to the active networkManager if Mirror is installed
        /// </summary>
        public Mirror.NetworkManager networkManager;
#endif
#endif
        #endregion

        private static SteamworksFoundationManager s_instance;
        
        /// <summary>
        /// For internal use
        /// </summary>
        public static bool s_EverInialized;
        private ENotificationPosition currentNotificationPosition = ENotificationPosition.k_EPositionBottomRight;
        private Vector2Int currentNotificationIndent = Vector2Int.zero;

#region Depracated Members
        /// <summary>
        /// <para>For use with the singleton approch</para>
        /// <para>Heathen Engineering recomends the use of direct references. Note that all required funcitonality of the <see cref="SteamworksFoundationManager"/> is available in the <see cref="SteamSettings"/> scriptable object which can be referenced on any game object directly as it is not a scene object it is not limited to references within the current scene.</para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// The singleton approch is viable however not recomended as this creates a dependency between GameObejcts and consaquently execution timing that is unnessiary.
        /// The recomended solution is to use a reference to the <see cref="SteamSettings"/> object directly ... e.g. for any script that needs to access Steam features simply give it a reference of
        /// </para>
        /// <code>
        /// public SteamSettings settings;
        /// </code>
        /// <para>
        /// You can now reference the settings scriptable object and use it directly in that script. As the Scriptable Object is defined in the asset database it is always available regardless of execution order and does not requrie GameObject scene references.
        /// </para>
        /// </remarks>
        [Obsolete("This field exists to support a singleton based design model however singleton as a model created unnessisary dependencies between GameObjects. It is recomended that you use SteamSettings objects directly.", false)]
        public static SteamworksFoundationManager Instance
        {
            get
            {
                return s_instance;
            }
        }

        [Obsolete("use SteamSettings.initialized", false)]
        public bool _initialized => settings.Initialized;

        /// <summary>
        /// Is the foundaiton manager initalized and ready to use
        /// </summary>
        [Obsolete("Reference the BoolVariable assigned to the SteamSettings.initialized member directly as opposed to using static members", false)]
        public static bool Initialized
        {
            get
            {
                return Instance._initialized;
            }
        }
#endregion

        private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
        private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }

        private void Awake()
        {
            // Only one instance of SteamManager at a time!
            if (s_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            s_instance = this;

            if (s_EverInialized)
            {
                // This is almost always an error.
                // The most common case where this happens is when SteamManager gets destroyed because of Application.Quit(),
                // and then some Steamworks code in some other OnDestroy gets called afterwards, creating a new SteamManager.
                // You should never call Steamworks functions in OnDestroy, always prefer OnDisable if possible.
                onSteamInitalizationError.Invoke("Tried to Initialize the SteamAPI twice in one session!");
                throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
            }

            if (_doNotDistroyOnLoad.Value)
                DontDestroyOnLoad(gameObject);

            if (!Packsize.Test())
            {
                Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
                onSteamInitalizationError.Invoke("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
                onSteamInitalizationError.Invoke("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            }

            try
            {
                // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
                // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

                // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
                // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
                // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
                //AppId = SteamAppId != null ? new AppId_t(SteamAppId.Value) : AppId_t.Invalid;
                if (SteamAPI.RestartAppIfNecessary(settings.applicationId))
                {
                    Application.Quit();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            { // We catch this exception here, as it will be the first occurence of it.
                Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);
                onSteamInitalizationError.Invoke("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);
                Application.Quit();
                return;
            }
        }

        // This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
        private void OnEnable()
        {
            if (s_instance == null)
            {
                s_instance = this;
            }

#if !UNITY_SERVER
            Debug.Log("Client Startup Detected!");
            settings.Init();

            if (!settings.Initialized)
            {
                Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
                onSteamInitalizationError.Invoke("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
                return;
            }

            s_EverInialized = true;
            onSteamInitalized.Invoke();
            Debug.Log("Steam client Initalized!");


            if (!settings.Initialized)
            {
                return;
            }

            if (m_SteamAPIWarningMessageHook == null)
            {
                // Set up our callback to recieve warning messages from Steam.
                // You must launch with "-debug_steamapi" in the launch args to recieve warnings.
                m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
            }

            //Register the overlay callbacks
            settings.client.m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(settings.client.HandleOnOverlayOpen);
            settings.client.onOverlayActivated.AddListener(onOverlayActivated.Invoke);

            //Register the achievements system
            settings.client.RegisterAchievementsSystem();
            settings.client.onAchievementStored.AddListener(onAchievementStored.Invoke);
            settings.client.onUserStatsReceived.AddListener(onUserStatsRecieved.Invoke);
            settings.client.onUserStatsStored.AddListener(onUserStatsStored.Invoke);
            settings.client.onNumberOfCurrentPlayersResult.AddListener(onNumberOfCurrentPlayersResult.Invoke);
            settings.client.RequestCurrentStats();

            //Register the friends system
            settings.client.RegisterFriendsSystem(settings.client.user);
            settings.client.onAvatarLoaded.AddListener(onAvatarLoaded.Invoke);
            settings.client.onPersonaStateChanged.AddListener(onPersonaStateChanged.Invoke);
            settings.client.onRecievedFriendChatMessage.AddListener(onRecievedFriendChatMessage.Invoke);
#endif
#if UNITY_SERVER //|| UNITY_EDITOR
            Debug.Log("Server Startup Detected!");
            if (settings.server.autoInitalize)
                InitializeGameServer();
#endif
        }

        private void OnDisable()
        {
#if !UNITY_SERVER
            settings.client.onOverlayActivated.RemoveListener(onOverlayActivated.Invoke);
            settings.client.onAchievementStored.RemoveListener(onAchievementStored.Invoke);
            settings.client.onUserStatsReceived.RemoveListener(onUserStatsRecieved.Invoke);
            settings.client.onUserStatsStored.RemoveListener(onUserStatsStored.Invoke);
            settings.client.onNumberOfCurrentPlayersResult.RemoveListener(onNumberOfCurrentPlayersResult.Invoke);
            settings.client.onAvatarLoaded.RemoveListener(onAvatarLoaded.Invoke);
            settings.client.onPersonaStateChanged.RemoveListener(onPersonaStateChanged.Invoke);
            settings.client.onRecievedFriendChatMessage.RemoveListener(onRecievedFriendChatMessage.Invoke);
#endif

#if UNITY_SERVER //|| UNITY_EDITOR
            if (settings.Initialized)
            {
                Debug.Log("Logging off the Steam Game Server");

                if (settings.server.usingGameServerAuthApi)
                    SteamGameServer.EnableHeartbeats(false);

                //Notify listeners of the shutdown
                settings.server.gameServerShuttingDown.Invoke();

#if MIRROR
                if (settings.server.enableMirror)
                    networkManager.StopServer();
#endif
            }

            //Remove the settings event listeners
            settings.server.gameServerShuttingDown.RemoveListener(gameServerShuttingDown.Invoke);
            settings.server.disconnected.RemoveListener(disconnected.Invoke);
            settings.server.connected.RemoveListener(connected.Invoke);
            settings.server.failure.RemoveListener(failure.Invoke);
            settings.server.gameServerShuttingDown.RemoveListener(LogShutDown);
            settings.server.disconnected.RemoveListener(LogDisconnect);
            settings.server.connected.RemoveListener(LogConnect);
            settings.server.failure.RemoveListener(LogFailure);

            if (settings.Initialized)
            {
                //Log the server off of Steam
                SteamGameServer.LogOff();
                Debug.Log("Steam Game Server has been logged off");
            }
#endif
        }

        // OnApplicationQuit gets called too early to shutdown the SteamAPI.
        // Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
        // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
        private void OnDestroy()
        {
#if !UNITY_SERVER
            if (settings != null && settings.client.user != null)
                settings.client.user.ClearData();

            if (s_instance != this)
            {
                return;
            }
            s_instance = null;

            if (!settings.Initialized)
            {
                return;
            }

            SteamAPI.Shutdown();
#endif
#if UNITY_SERVER //|| UNITY_EDITOR
            GameServer.Shutdown();
#endif
        }

        private void Update()
        {
            if (!settings.Initialized)
            {
                return;
            }

#if !UNITY_SERVER
            SteamAPI.RunCallbacks();            

            if (settings != null)
            {
                //Refresh the notification position
                if (currentNotificationPosition != settings.client.overlay.notificationPosition)
                {
                    currentNotificationPosition = settings.client.overlay.notificationPosition;
                    settings.client.SetNotificationPosition(settings.client.overlay.notificationPosition);
                }

                if (currentNotificationIndent != settings.client.overlay.notificationInset)
                {
                    currentNotificationIndent = settings.client.overlay.notificationInset;
                    settings.client.SetNotificationInset(settings.client.overlay.notificationInset);
                }
            }
#endif
#if UNITY_SERVER //|| UNITY_EDITOR
            GameServer.RunCallbacks();
#endif
        }

        #region Server Only Logic
#if !CONDITIONAL_COMPILE || UNITY_SERVER// || UNITY_EDITOR
        private void InitializeGameServer()
        {
            //Insure the setting events are initalized ... Unity doesn't do this for you as it does with behaviours
            if (settings.server.gameServerShuttingDown == null)
                settings.server.gameServerShuttingDown = new UnityEvent();
            if (settings.server.disconnected == null)
                settings.server.disconnected = new SteamSettings.GameServer.DisconnectedEvent();
            if (settings.server.connected == null)
                settings.server.connected = new SteamSettings.GameServer.ConnectedEvent();
            if (settings.server.failure == null)
                settings.server.failure = new SteamSettings.GameServer.FailureEvent();

            //Register on the Steam callback for the related events
            settings.server.steamServerConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(settings.server.OnSteamServerConnectFailure);
            settings.server.steamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(settings.server.OnSteamServersConnected);
            settings.server.steamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(settings.server.OnSteamServersDisconnected);

            //Pass through the invoke to the settings events to the behaviour events
            settings.server.gameServerShuttingDown.AddListener(gameServerShuttingDown.Invoke);
            settings.server.disconnected.AddListener(disconnected.Invoke);
            settings.server.connected.AddListener(connected.Invoke);
            settings.server.connected.AddListener(OnSteamServersConnected);
            settings.server.failure.AddListener(failure.Invoke);

            //If debugging
            if (settings.isDebugging)
            {
                Debug.Log("Establishing debug hooks");
                settings.server.gameServerShuttingDown.AddListener(LogShutDown);
                settings.server.disconnected.AddListener(LogDisconnect);
                settings.server.connected.AddListener(LogConnect);
                settings.server.failure.AddListener(LogFailure);
            }

            settings.Init();

            if (!settings.Initialized)
            {
                Debug.Log("SteamGameServer_Init call failed!");
                onSteamInitalizationError.Invoke("SteamGameServer_Init call failed!");
                return;
            }
            else
            {
                Debug.Log("SteamGameServer_Init call succeded\n\tPublic IP = " + SteamGameServer.GetPublicIP().ToString() + "\n\tIP = " + settings.server.ip.ToString() + "\n\tAuthentication Port = " + settings.server.authenticationPort.ToString() + "\n\tServer Port = " + settings.server.serverPort.ToString() + "\n\tMaster Server Updater Port = " + settings.server.masterServerUpdaterPort.ToString() + "\n\tVersion = " + settings.server.serverVersion);
                onSteamInitalized.Invoke();
            }

            // Set the "game dir".
            // This is currently required for all games.  However, soon we will be
            // using the AppID for most purposes, and this string will only be needed
            // for mods.  it may not be changed after the server has logged on
            SteamGameServer.SetModDir(settings.server.gameDirectory);

            // These fields are currently required, but will go away soon.
            // See their documentation for more info
            SteamGameServer.SetProduct(settings.applicationId.m_AppId.ToString());
            SteamGameServer.SetGameDescription(settings.server.gameDescription);

            if (settings.server.supportSpectators)
            {
                if (settings.isDebugging)
                    Debug.Log("Spectator enabled:\n\tName = " + settings.server.spectatorServerName + "\n\tSpectator Port = " + settings.server.spectatorPort.ToString());

                SteamGameServer.SetSpectatorPort(settings.server.spectatorPort);
                SteamGameServer.SetSpectatorServerName(settings.server.spectatorServerName);
            }
            else if (settings.isDebugging)
                Debug.Log("Spectator Set Up Skipped");

            if (settings.server.anonymousServerLogin)
            {
                if (settings.isDebugging)
                    Debug.Log("Logging on with Anonymous");

                SteamGameServer.LogOnAnonymous();
            }
            else
            {
                if (settings.isDebugging)
                    Debug.Log("Logging on with token");

                SteamGameServer.LogOn(settings.server.gameServerToken);
            }

            // We want to actively update the master server with our presence so players can
            // find us via the steam matchmaking/server browser interfaces
            if (settings.server.usingGameServerAuthApi || settings.server.enableHeartbeats)
                SteamGameServer.EnableHeartbeats(true);

            Debug.Log("Steam Game Server Started.\nWaiting for connection result from Steam");
        }

        private void OnSteamServersConnected(SteamServersConnected_t pLogonSuccess)
        {
            settings.server.serverId = SteamGameServer.GetSteamID();
            Debug.Log("Game Server connected to Steam successfully!\n\tMod Directory = " + settings.server.gameDirectory + "\n\tApplicaiton ID = " + settings.applicationId.m_AppId.ToString() + "\n\tServer ID = " + settings.server.serverId.m_SteamID.ToString() + "\n\tServer Name = " + settings.server.serverName + "\n\tGame Description = " + settings.server.gameDescription + "\n\tMax Player Count = " + settings.server.maxPlayerCount.ToString());

#if MIRROR
            if (settings.server.enableMirror)
            {
                networkManager.maxConnections = settings.server.maxPlayerCount;
                networkManager.networkAddress = "localhost";
                networkManager.StartServer();
            }
#endif

            // Tell Steam about our server details
            SendUpdatedServerDetailsToSteam();
        }

        private void SendUpdatedServerDetailsToSteam()
        {
            SteamGameServer.SetMaxPlayerCount(settings.server.maxPlayerCount);
            SteamGameServer.SetPasswordProtected(settings.server.isPasswordProtected);
            SteamGameServer.SetServerName(settings.server.serverName);
            SteamGameServer.SetBotPlayerCount(settings.server.botPlayerCount);
            SteamGameServer.SetMapName(settings.server.mapName);
            SteamGameServer.SetDedicatedServer(settings.server.isDedicated);

            if (settings.server.rulePairs != null && settings.server.rulePairs.Count > 0)
            {
                foreach (var pair in settings.server.rulePairs)
                {
                    SteamGameServer.SetKeyValue(pair.key, pair.value);
                }
            }
        }

        private void LogFailure(SteamServerConnectFailure_t arg0)
        {
            Debug.LogError("Connection Failure: " + arg0.m_eResult.ToString());
        }

        private void LogConnect(SteamServersConnected_t arg0)
        {
            Debug.LogError("Connection Ready");
        }

        private void LogDisconnect(SteamServersDisconnected_t arg0)
        {
            Debug.LogError("Connection Closed: " + arg0.m_eResult.ToString());
        }

        private void LogShutDown()
        {
            Debug.LogError("Game Server Logging Off");
        }
#endif
        #endregion

        #region Client Only Logic
#if !CONDITIONAL_COMPILE || !UNITY_SERVER
        /// <summary>
        /// Set rather or not the system should listen for Steam Friend chat messages
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
        /// Send a Steam Friend Chat message to the indicated user
        /// </summary>
        /// <param name="friendId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendFriendChatMessage(ulong friendId, string message)
        {
            return SendFriendChatMessage(new CSteamID(friendId), message);
        }

        /// <summary>
        /// Send a Steam Friend Chat message to the indicated user
        /// </summary>
        /// <param name="friend"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendFriendChatMessage(CSteamID friend, string message)
        {
            return SteamFriends.ReplyToFriendMessage(friend, message);
        }

#region Depracated Static Accessors
        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.SetNotificationPosition"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.SetNotificationPosition", false)]
        public static void _SetNotificationPosition(ENotificationPosition position)
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.SetNotificationPosition(position);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.SetNotificationInset"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.SetNotificationInset", false)]
        public static void _SetNotificationInset(Vector2Int inset)
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.SetNotificationInset(inset);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStore"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStore", false)]
        public static void _OpenStore()
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenStore();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStore"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStore", false)]
        public static void _OpenStore(uint appId)
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenStore(appId);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStore"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStore", false)]
        public static void _OpenStore(uint appId, EOverlayToStoreFlag flag)
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenStore(appId, flag);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStore"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStore", false)]
        public static void _OpenStore(AppId_t appId, EOverlayToStoreFlag flag)
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenStore(appId, flag);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.Open"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.Open", false)]
        public static void _Open(string dialog)
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.Open(dialog);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenWebPage"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenWebPage", false)]
        public static void _OpenWebPage(string URL)
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenWebPage(URL);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenFriends"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenFriends", false)]
        public static void _OpenFriends()
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenFriends();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenCommunity"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenCommunity", false)]
        public static void _OpenCommunity()
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenCommunity();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenPlayers"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenPlayers", false)]
        public static void _OpenPlayers()
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenPlayers();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenSettings"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenSettings", false)]
        public static void _OpenSettings()
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenSettings();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenOfficialGameGroup"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenOfficialGameGroup", false)]
        public static void _OpenOfficialGameGroup()
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenOfficialGameGroup();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStats"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStats", false)]
        public static void _OpenStats()
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenStats();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenAchievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenAchievements", false)]
        public static void _OpenAchievements()
        {
            if (Instance != null && Instance.settings != null)
                Instance.settings.client.overlay.OpenAchievements();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenChat"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenChat", false)]
        public static void _OpenChat(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.settings.client.overlay.OpenChat(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenProfile"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenProfile", false)]
        public static void _OpenProfile(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.settings.client.overlay.OpenProfile(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenTrade"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenTrade", false)]
        public static void _OpenTrade(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.settings.client.overlay.OpenTrade(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStats"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStats", false)]
        public static void _OpenStats(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.settings.client.overlay.OpenStats(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenAchievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenAchievements", false)]
        public static void _OpenAchievements(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.settings.client.overlay.OpenAchievements(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenFriendAdd"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenFriendAdd", false)]
        public static void _OpenFriendAdd(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.settings.client.overlay.OpenFriendAdd(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenFriendRemove"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenFriendRemove", false)]
        public static void _OpenFriendRemove(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.settings.client.overlay.OpenFriendRemove(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenRequestAccept"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenRequestAccept", false)]
        public static void _OpenRequestAccept(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.settings.client.overlay.OpenRequestAccept(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenRequestIgnore"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenRequestIgnore", false)]
        public static void _OpenRequestIgnore(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.settings.client.overlay.OpenRequestIgnore(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.SetNotificationPosition"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.SetNotificationPosition", false)]
        public void SetNotificationPosition(ENotificationPosition position)
        {
            if (settings != null)
                settings.client.SetNotificationPosition(position);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.SetNotificationInset"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.SetNotificationInset", false)]
        public void SetNotificationInset(Vector2Int inset)
        {
            if (settings != null)
                settings.client.SetNotificationInset(inset);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStore"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStore", false)]
        public void OpenStore()
        {
            if (settings != null)
                settings.client.overlay.OpenStore();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStore"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStore", false)]
        public void OpenStore(uint appId)
        {
            if (settings != null)
                settings.client.overlay.OpenStore(appId);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStore"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStore", false)]
        public void OpenStore(uint appId, EOverlayToStoreFlag flag)
        {
            if (settings != null)
                settings.client.overlay.OpenStore(appId, flag);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStore"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStore", false)]
        public void OpenStore(AppId_t appId, EOverlayToStoreFlag flag)
        {
            if (settings != null)
                settings.client.overlay.OpenStore(appId, flag);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.Open"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.Open", false)]
        public void Open(string dialog)
        {
            if (settings != null)
                settings.client.overlay.Open(dialog);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenWebPage"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenWebPage", false)]
        public void OpenWebPage(string URL)
        {
            if (settings != null)
                settings.client.overlay.OpenWebPage(URL);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenFriends"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenFriends", false)]
        public void OpenFriends()
        {
            if (settings != null)
                settings.client.overlay.OpenFriends();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenCommunity"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenCommunity", false)]
        public void OpenCommunity()
        {
            if (settings != null)
                settings.client.overlay.OpenCommunity();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenPlayers"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenPlayers", false)]
        public void OpenPlayers()
        {
            if (settings != null)
                settings.client.overlay.OpenPlayers();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenSettings"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenSettings", false)]
        public void OpenSettings()
        {
            if (settings != null)
                settings.client.overlay.OpenSettings();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenOfficialGameGroup"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenOfficialGameGroup", false)]
        public void OpenOfficialGameGroup()
        {
            if (settings != null)
                settings.client.overlay.OpenOfficialGameGroup();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStats"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStats", false)]
        public void OpenStats()
        {
            if (settings != null)
                settings.client.overlay.OpenStats();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenAchievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenAchievements", false)]
        public void OpenAchievements()
        {
            if (settings != null)
                settings.client.overlay.OpenAchievements();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenChat"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenChat", false)]
        public void OpenChat(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                settings.client.overlay.OpenChat(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenProfile"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenProfile", false)]
        public void OpenProfile(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                settings.client.overlay.OpenProfile(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenTrade"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenTrade", false)]
        public void OpenTrade(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                settings.client.overlay.OpenTrade(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenStats"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenStats", false)]
        public void OpenStats(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                settings.client.overlay.OpenStats(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenAchievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenAchievements", false)]
        public void OpenAchievements(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                settings.client.overlay.OpenAchievements(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenFriendAdd"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenFriendAdd", false)]
        public void OpenFriendAdd(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                settings.client.overlay.OpenFriendAdd(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenFriendRemove"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenFriendRemove", false)]
        public void OpenFriendRemove(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                settings.client.overlay.OpenFriendRemove(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenRequestAccept"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenRequestAccept", false)]
        public void OpenRequestAccept(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                settings.client.overlay.OpenRequestAccept(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.Overlay.OpenRequestIgnore"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.overlay.OpenRequestIgnore", false)]
        public void OpenRequestIgnore(SteamUserData user)
        {
            if (user.id.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                settings.client.overlay.OpenRequestIgnore(user.id);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.userData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.userData", false)]
        public static SteamUserData _UserData
        {
            get
            {
                if (Instance != null && Instance.settings != null)
                    return Instance.settings.client.userData;
                else
                    return null;
            }
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.DisplayName"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public static string _GetUserName(ulong steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
                return u.DisplayName;
            else
                return string.Empty;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.GameInfo"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public static FriendGameInfo_t _GetUserGameInfo(ulong steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
                return u.GameInfo;
            else
                return default(FriendGameInfo_t);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.avatar"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public static Texture2D _GetUserAvatar(ulong steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
            {
                if (!u.iconLoaded)
                {
                    _RefreshAvatar(u);
                }
                return u.avatar;
            }
            else
                return null;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public static SteamUserData _GetUserData(ulong steamId)
        {
            return Instance.settings.client.GetUserData(new CSteamID(steamId));
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.DisplayName"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public static string _GetUserName(CSteamID steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
                return u.DisplayName;
            else
                return string.Empty;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.GameInfo"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public static FriendGameInfo_t _GetUserGameInfo(CSteamID steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
                return u.GameInfo;
            else
                return default(FriendGameInfo_t);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.avatar"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public static Texture2D _GetUserAvatar(CSteamID steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
            {
                if (!u.iconLoaded)
                {
                    _RefreshAvatar(u);
                }
                return u.avatar;
            }
            else
                return null;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public static SteamUserData _GetUserData(CSteamID steamId)
        {
            return Instance.settings.client.GetUserData(steamId);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.RefreshAvatar"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.RefreshAvatar", false)]
        public static void _RefreshAvatar(SteamUserData userData)
        {
            Instance.settings.client.RefreshAvatar(userData);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.userData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.userData", false)]
        public SteamUserData UserData
        {
            get
            {
                if (settings != null)
                    return settings.client.userData;
                else
                    return null;
            }
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.DisplayName"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public string GetUserName(ulong steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
                return u.DisplayName;
            else
                return string.Empty;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.GameInfo"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public FriendGameInfo_t GetUserGameInfo(ulong steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
                return u.GameInfo;
            else
                return default(FriendGameInfo_t);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.avatar"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public Texture2D GetUserAvatar(ulong steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
            {
                if (!u.iconLoaded)
                {
                    RefreshAvatar(u);
                }
                return u.avatar;
            }
            else
                return null;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.GameInfo"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public SteamUserData GetUserData(ulong steamId)
        {
            return settings.client.GetUserData(new CSteamID(steamId));
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.DisplayName"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public string GetUserName(CSteamID steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
                return u.DisplayName;
            else
                return string.Empty;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.GameInfo"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public FriendGameInfo_t GetUserGameInfo(CSteamID steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
                return u.GameInfo;
            else
                return default(FriendGameInfo_t);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/> and use the <see cref="SteamUserData.avatar"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public Texture2D GetUserAvatar(CSteamID steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
            {
                if (!u.iconLoaded)
                {
                    RefreshAvatar(u);
                }
                return u.avatar;
            }
            else
                return null;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.GetUserData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.GetUserData", false)]
        public SteamUserData GetUserData(CSteamID steamId)
        {
            return settings.client.GetUserData(steamId);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.RefreshAvatar"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.RefreshAvatar", false)]
        public void RefreshAvatar(SteamUserData userData)
        {
            settings.client.RefreshAvatar(userData);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.StoreStatsAndAchievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.StoreStatsAndAchievements", false)]
        public void StoreStatsAndAchievements()
        {
            settings.client.StoreStatsAndAchievements();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public SteamAchievementData GetAchievement(string achievementId)
        {
            if (settings != null)
            {
                if (settings.client.achievements.Exists(a => a.achievementId == achievementId))
                {
                    var ach = settings.client.achievements.FirstOrDefault(a => a.achievementId == achievementId);
                    return ach;
                }
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public SteamAchievementData GetAchievement(int achievementIndex)
        {
            if (settings != null)
            {
                if (settings.client.achievements.Count > achievementIndex && achievementIndex > -1)
                {
                    var ach = settings.client.achievements[achievementIndex];
                    return ach;
                }
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.UnlockAchievementData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.UnlockAchievementData", false)]
        public void UnlockAchievement(SteamAchievementData achievementData)
        {
            achievementData.Unlock();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.UnlockAchievementData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.UnlockAchievementData", false)]
        public void UnlockAchievement(string achievementId)
        {
            if (settings != null)
            {
                if (settings.client.achievements.Exists(a => a.achievementId == achievementId))
                {
                    var ach = settings.client.achievements.FirstOrDefault(a => a.achievementId == achievementId);
                    ach.Unlock();
                }
            }
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.UnlockAchievementData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.UnlockAchievementData", false)]
        public void UnlockAchievement(int achievementIndex)
        {
            if (settings != null)
            {
                if (settings.client.achievements.Count > achievementIndex && achievementIndex > -1)
                {
                    var ach = settings.client.achievements[achievementIndex];
                    ach.Unlock();
                }
            }
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public bool IsAchievementAchieved(string achievementId)
        {
            var ach = GetAchievement(achievementId);
            if (ach != null)
                return ach.isAchieved;
            else
                return false;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public bool IsAchievementAchieved(int achievementIndex)
        {
            var ach = GetAchievement(achievementIndex);
            if (ach != null)
                return ach.isAchieved;
            else
                return false;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public bool AchievementExists(string achievementId)
        {
            var ach = GetAchievement(achievementId);
            return ach != null;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public bool AchievementExists(int achievementIndex)
        {
            var ach = GetAchievement(achievementIndex);
            return ach != null;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.StoreStatsAndAchievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.StoreStatsAndAchievements", false)]
        public static void _StoreStatsAndAchievements()
        {
            Instance.settings.client.StoreStatsAndAchievements();
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public static SteamAchievementData _GetAchievement(string achievementId)
        {
            if (Instance != null)
                return Instance.GetAchievement(achievementId);
            else
                return null;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public static SteamAchievementData _GetAchievement(int achievementIndex)
        {
            if (Instance != null)
                return Instance.GetAchievement(achievementIndex);
            else
                return null;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.UnlockAchievementData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.UnlockAchievementData", false)]
        public static void _UnlockAchievement(SteamAchievementData achievementData)
        {
            if (Instance != null)
                Instance.UnlockAchievement(achievementData);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.UnlockAchievementData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.UnlockAchievementData", false)]
        public static void _UnlockAchievement(string achievementId)
        {
            if (Instance != null)
                Instance.UnlockAchievement(achievementId);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.UnlockAchievementData"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.UnlockAchievementData", false)]
        public static void _UnlockAchievement(int achievementIndex)
        {
            if (Instance != null)
                Instance.UnlockAchievement(achievementIndex);
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public static bool _IsAchievementAchieved(string achievementId)
        {
            if (Instance != null)
                return Instance.AchievementExists(achievementId);
            return false;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public static bool _IsAchievementAchieved(int achievementIndex)
        {
            if (Instance != null)
                return Instance.IsAchievementAchieved(achievementIndex);
            return false;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public static bool _AchievementExists(string achievementId)
        {
            if (Instance != null)
                return Instance.AchievementExists(achievementId);
            return false;
        }

        /// <summary>
        /// Depracated please use the <see cref="SteamSettings.GameClient.achievements"/> member on <see cref="SteamSettings.client"/>
        /// </summary>
        [Obsolete("Use SteamSettings.client.achievements", false)]
        public static bool _AchievementExists(int achievementIndex)
        {
            if (Instance != null)
                return Instance.AchievementExists(achievementIndex);
            return false;
        }
#endregion

#endif
#endregion

    }
}
#endif