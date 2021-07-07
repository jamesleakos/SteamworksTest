#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using System.Collections.Generic;
using UnityEngine;
using System;
using Steamworks;
using Mirror;
using System.Threading.Tasks;
using System.Collections;
using HeathenEngineering.SteamApi.Foundation;
using System.Threading;

namespace HeathenEngineering.SteamApi.Networking
{
    /// <summary>
    /// A Mirror compatable transport which leverages Steam's SteamGameServerNetworking interface for client/server networking situation.
    /// </summary>
    /// <remarks>
    /// Derived from FizzCube's FizzySteamyMirror available under the MIT license on GetHub as an extension of Mirror. 
    /// For the latest version check his GitHub at https://github.com/Raystorms/FizzySteamyMirror
    /// Note that this version has been modified to suit Heathen Steamworks.
    /// </remarks>
    public class HeathenSteamGameServerTransport : Transport
    {
        private const string STEAM_SCHEME = "steam";

        private Server server;

        [SerializeField]
        public EP2PSend[] Channels = new EP2PSend[1] { EP2PSend.k_EP2PSendReliable };

        [Tooltip("Timeout for connecting in seconds.")]
        public int Timeout = 25;
        [Tooltip("Allow or disallow P2P connections to fall back to being relayed through the Steam servers if a direct connection or NAT-traversal cannot be established.")]
        public bool AllowSteamRelay = true;

        [Header("Info")]
        [Tooltip("This will display your Steam User ID when you start or connect to a server.")]
        public ulong SteamUserID;

        private void Awake()
        {
            Debug.Assert(Channels != null && Channels.Length > 0, "No channel configured for Heathen Steam P2P Transport.");

            Invoke(nameof(FetchSteamID), 1f);
        }

        private void LateUpdate()
        {
            if (enabled)
            {
                server?.ReceiveData();
            }
        }

        public override bool ClientConnected() => false;
        public override void ClientConnect(string address)
        {
            throw new InvalidOperationException("[1] SteamGameServerTransport is only inteded for use on Game Servers and cannot create a Client object");
        }

        public override void ClientConnect(Uri uri)
        {
            throw new InvalidOperationException("[2] SteamGameServerTransport is only inteded for use on Game Servers and cannot create a Client object");
        }

        public override void ClientSend(int channelId, ArraySegment<byte> segment)
        {
            throw new InvalidOperationException("[3] SteamGameServerTransport is only inteded for use on Game Servers and cannot create a Client object");
        }

        public override void ClientDisconnect()
        {
        }
        public bool ClientActive() => false;


        public override bool ServerActive() => server != null;
        public override void ServerStart()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("SteamWorks not initialized. Server could not be started.");
                return;
            }

            FetchSteamID();

            if (ClientActive())
            {
                Debug.LogError("Transport already running as client!");
                return;
            }

            if (!ServerActive())
            {
                Debug.Log("Starting server.");
                SteamGameServerNetworking.AllowP2PPacketRelay(AllowSteamRelay);
                server = Server.CreateServer(this, NetworkManager.singleton.maxConnections);
            }
            else
            {
                Debug.LogError("Server already started!");
            }
        }

        public override Uri ServerUri()
        {
            var steamBuilder = new UriBuilder
            {
                Scheme = STEAM_SCHEME,
                Host = SteamUser.GetSteamID().m_SteamID.ToString()
            };

            return steamBuilder.Uri;
        }

        public override void ServerSend(int connectionId, int channelId, ArraySegment<byte> segment)
        {
            if (ServerActive())
            {
                byte[] data = new byte[segment.Count];
                Array.Copy(segment.Array, segment.Offset, data, 0, segment.Count);
                server.SendAll(connectionId, data, channelId);
            }
        }
        public override bool ServerDisconnect(int connectionId) => ServerActive() && server.Disconnect(connectionId);
        public override string ServerGetClientAddress(int connectionId) => ServerActive() ? server.ServerGetClientAddress(connectionId) : string.Empty;
        public override void ServerStop()
        {
            if (ServerActive())
            {
                Shutdown();
            }
        }

        public override void Shutdown()
        {
            server?.Shutdown();

            server = null;
            Debug.Log("Transport shut down.");
        }

        public override int GetMaxPacketSize(int channelId)
        {
            switch (Channels[channelId])
            {
                case EP2PSend.k_EP2PSendUnreliable:
                case EP2PSend.k_EP2PSendUnreliableNoDelay:
                    return 1200;
                case EP2PSend.k_EP2PSendReliable:
                case EP2PSend.k_EP2PSendReliableWithBuffering:
                    return 1048576;
                default:
                    throw new NotSupportedException();
            }
        }

        public override bool Available()
        {
            try
            {
                return SteamManager.Initialized;
            }
            catch
            {
                return false;
            }
        }

        private void FetchSteamID()
        {
            if (SteamManager.Initialized)
            {
                SteamUserID = SteamUser.GetSteamID().m_SteamID;
            }
        }

        private void OnDestroy()
        {
            if (server != null)
            {
                Shutdown();
            }
        }

        public class BidirectionalDictionary<T1, T2> : IEnumerable
        {
            private Dictionary<T1, T2> t1ToT2Dict = new Dictionary<T1, T2>();
            private Dictionary<T2, T1> t2ToT1Dict = new Dictionary<T2, T1>();

            public IEnumerable<T1> FirstTypes => t1ToT2Dict.Keys;
            public IEnumerable<T2> SecondTypes => t2ToT1Dict.Keys;

            public IEnumerator GetEnumerator() => t1ToT2Dict.GetEnumerator();

            public int Count => t1ToT2Dict.Count;

            public void Add(T1 key, T2 value)
            {
                t1ToT2Dict[key] = value;
                t2ToT1Dict[value] = key;
            }

            public void Add(T2 key, T1 value)
            {
                t2ToT1Dict[key] = value;
                t1ToT2Dict[value] = key;
            }

            public T2 Get(T1 key) => t1ToT2Dict[key];

            public T1 Get(T2 key) => t2ToT1Dict[key];

            public bool TryGetValue(T1 key, out T2 value) => t1ToT2Dict.TryGetValue(key, out value);

            public bool TryGetValue(T2 key, out T1 value) => t2ToT1Dict.TryGetValue(key, out value);

            public bool Contains(T1 key) => t1ToT2Dict.ContainsKey(key);

            public bool Contains(T2 key) => t2ToT1Dict.ContainsKey(key);

            public void Remove(T1 key)
            {
                if (Contains(key))
                {
                    T2 val = t1ToT2Dict[key];
                    t1ToT2Dict.Remove(key);
                    t2ToT1Dict.Remove(val);
                }
            }
            public void Remove(T2 key)
            {
                if (Contains(key))
                {
                    T1 val = t2ToT1Dict[key];
                    t1ToT2Dict.Remove(val);
                    t2ToT1Dict.Remove(key);
                }
            }

            public T1 this[T2 key]
            {
                get => t2ToT1Dict[key];
                set
                {
                    t2ToT1Dict[key] = value;
                    t1ToT2Dict[value] = key;
                }
            }

            public T2 this[T1 key]
            {
                get => t1ToT2Dict[key];
                set
                {
                    t1ToT2Dict[key] = value;
                    t2ToT1Dict[value] = key;
                }
            }
        }

        /// <summary>
        /// Internal class defining the server structure for P2P connections
        /// </summary>
        public class Server
        {
            private event Action<int> OnConnected;
            private event Action<int, byte[], int> OnReceivedData;
            private event Action<int> OnDisconnected;
            private event Action<int, Exception> OnReceivedError;

            private BidirectionalDictionary<CSteamID, int> steamToMirrorIds;
            private int maxConnections;
            private int nextConnectionID;


            private EP2PSend[] channels;
            private int internal_ch => channels.Length;

            protected enum InternalMessages : byte
            {
                CONNECT,
                ACCEPT_CONNECT,
                DISCONNECT
            }

            private Callback<P2PSessionRequest_t> callback_OnNewConnection = null;
            private Callback<P2PSessionConnectFail_t> callback_OnConnectFail = null;

            protected readonly HeathenSteamGameServerTransport transport;

            protected void Dispose()
            {
                if (callback_OnNewConnection != null)
                {
                    callback_OnNewConnection.Dispose();
                    callback_OnNewConnection = null;
                }

                if (callback_OnConnectFail != null)
                {
                    callback_OnConnectFail.Dispose();
                    callback_OnConnectFail = null;
                }
            }

            public static Server CreateServer(HeathenSteamGameServerTransport transport, int maxConnections)
            {
                Server s = new Server(transport, maxConnections);

                s.OnConnected += (id) => transport.OnServerConnected.Invoke(id);
                s.OnDisconnected += (id) => transport.OnServerDisconnected.Invoke(id);
                s.OnReceivedData += (id, data, channel) => transport.OnServerDataReceived.Invoke(id, new ArraySegment<byte>(data), channel);
                s.OnReceivedError += (id, exception) => transport.OnServerError.Invoke(id, exception);

                if (!SteamManager.Initialized)
                {
                    Debug.LogError("SteamWorks not initialized.");
                }

                return s;
            }

            private Server(HeathenSteamGameServerTransport transport, int maxConnections)
            {
                channels = transport.Channels;

                callback_OnNewConnection = Callback<P2PSessionRequest_t>.Create(OnNewConnection);
                callback_OnConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnConnectFail);

                this.transport = transport;

                this.maxConnections = maxConnections;
                steamToMirrorIds = new BidirectionalDictionary<CSteamID, int>();
                nextConnectionID = 1;
            }

            private void OnConnectFail(P2PSessionConnectFail_t result)
            {
                OnConnectionFailed(result.m_steamIDRemote);
                CloseP2PSessionWithUser(result.m_steamIDRemote);

                switch (result.m_eP2PSessionError)
                {
                    case 1:
                        throw new Exception("Connection failed: The target user is not running the same game.");
                    case 2:
                        throw new Exception("Connection failed: The local user doesn't own the app that is running.");
                    case 3:
                        throw new Exception("Connection failed: Target user isn't connected to Steam.");
                    case 4:
                        throw new Exception("Connection failed: The connection timed out because the target user didn't respond.");
                    default:
                        throw new Exception("Connection failed: Unknown error.");
                }
            }

            protected void SendInternal(CSteamID target, InternalMessages type) => SteamGameServerNetworking.SendP2PPacket(target, new byte[] { (byte)type }, 1, EP2PSend.k_EP2PSendReliable, internal_ch);
            protected bool Send(CSteamID host, byte[] msgBuffer, int channel)
            {
                return SteamGameServerNetworking.SendP2PPacket(host, msgBuffer, (uint)msgBuffer.Length, channels[channel], channel);
            }
            private bool Receive(out CSteamID clientSteamID, out byte[] receiveBuffer, int channel)
            {
                if (SteamGameServerNetworking.IsP2PPacketAvailable(out uint packetSize, channel))
                {
                    receiveBuffer = new byte[packetSize];
                    return SteamGameServerNetworking.ReadP2PPacket(receiveBuffer, packetSize, out _, out clientSteamID, channel);
                }

                receiveBuffer = null;
                clientSteamID = CSteamID.Nil;
                return false;
            }

            protected void CloseP2PSessionWithUser(CSteamID clientSteamID) => SteamGameServerNetworking.CloseP2PSessionWithUser(clientSteamID);

            protected void WaitForClose(CSteamID cSteamID) => transport.StartCoroutine(DelayedClose(cSteamID));
            private IEnumerator DelayedClose(CSteamID cSteamID)
            {
                yield return null;
                CloseP2PSessionWithUser(cSteamID);
            }

            public void ReceiveData()
            {
                try
                {
                    while (transport.enabled && Receive(out CSteamID clientSteamID, out byte[] internalMessage, internal_ch))
                    {
                        if (internalMessage.Length == 1)
                        {
                            OnReceiveInternalData((InternalMessages)internalMessage[0], clientSteamID);
                            return; // Wait one frame
                        }
                        else
                        {
                            Debug.Log("Incorrect package length on internal channel.");
                        }
                    }

                    for (int chNum = 0; chNum < channels.Length; chNum++)
                    {
                        while (transport.enabled && Receive(out CSteamID clientSteamID, out byte[] receiveBuffer, chNum))
                        {
                            OnReceiveData(receiveBuffer, clientSteamID, chNum);
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            protected void OnNewConnection(P2PSessionRequest_t result)
            {
                if (SteamSettings.current.isDebugging)
                    Debug.Log("New connection request from: " + result.m_steamIDRemote);

                SteamGameServerNetworking.AcceptP2PSessionWithUser(result.m_steamIDRemote);
            }

            protected void OnReceiveInternalData(InternalMessages type, CSteamID clientSteamID)
            {
                if(SteamSettings.current.isDebugging)
                    Debug.Log("Processing internal message of type:" + type.ToString() + " for SteamId: " + clientSteamID);

                switch (type)
                {
                    case InternalMessages.CONNECT:
                        if (steamToMirrorIds.Count >= maxConnections)
                        {
                            SendInternal(clientSteamID, InternalMessages.DISCONNECT);
                            return;
                        }

                        SendInternal(clientSteamID, InternalMessages.ACCEPT_CONNECT);

                        int connectionId = nextConnectionID++;
                        steamToMirrorIds.Add(clientSteamID, connectionId);
                        OnConnected.Invoke(connectionId);
                        Debug.Log($"Client with SteamID {clientSteamID} connected. Assigning connection id {connectionId}");
                        break;
                    case InternalMessages.DISCONNECT:
                        if (steamToMirrorIds.TryGetValue(clientSteamID, out int connId))
                        {
                            OnDisconnected.Invoke(connId);
                            CloseP2PSessionWithUser(clientSteamID);
                            steamToMirrorIds.Remove(clientSteamID);
                            Debug.Log($"Client with SteamID {clientSteamID} disconnected.");
                        }
                        else
                        {
                            OnReceivedError.Invoke(-1, new Exception("ERROR Unknown SteamID"));
                        }

                        break;
                    default:
                        Debug.Log("Received unknown message type");
                        break;
                }
            }

            protected void OnReceiveData(byte[] data, CSteamID clientSteamID, int channel)
            {
                if (steamToMirrorIds.TryGetValue(clientSteamID, out int connectionId))
                {
                    OnReceivedData.Invoke(connectionId, data, channel);
                }
                else
                {
                    CloseP2PSessionWithUser(clientSteamID);
                    Debug.LogError("Data received from steam client thats not known " + clientSteamID);
                    OnReceivedError.Invoke(-1, new Exception("ERROR Unknown SteamID"));
                }
            }

            public bool Disconnect(int connectionId)
            {
                if (steamToMirrorIds.TryGetValue(connectionId, out CSteamID steamID))
                {
                    SendInternal(steamID, InternalMessages.DISCONNECT);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Trying to disconnect unknown connection id: " + connectionId);
                    return false;
                }
            }

            public void Shutdown()
            {
                foreach (KeyValuePair<CSteamID, int> client in steamToMirrorIds)
                {
                    Disconnect(client.Value);
                    WaitForClose(client.Key);
                }

                Dispose();
            }

            public void SendAll(int connId, byte[] data, int channelId)
            {
                if (steamToMirrorIds.TryGetValue(connId, out CSteamID steamId))
                {
                    Send(steamId, data, channelId);
                }
                else
                {
                    Debug.LogError("Trying to send on unknown connection: " + connId);
                    OnReceivedError.Invoke(connId, new Exception("ERROR Unknown Connection"));
                }
            }

            public string ServerGetClientAddress(int connectionId)
            {
                if (steamToMirrorIds.TryGetValue(connectionId, out CSteamID steamId))
                {
                    return steamId.ToString();
                }
                else
                {
                    Debug.LogError("Trying to get info on unknown connection: " + connectionId);
                    OnReceivedError.Invoke(connectionId, new Exception("ERROR Unknown Connection"));
                    return string.Empty;
                }
            }

            protected void OnConnectionFailed(CSteamID remoteId)
            {
                if (SteamSettings.current.isDebugging)
                    Debug.Log("Handling server side connection failure with CSteamID: " + remoteId.ToString());

                int connectionId = steamToMirrorIds.TryGetValue(remoteId, out int connId) ? connId : nextConnectionID++;
                OnDisconnected.Invoke(connectionId);

                steamToMirrorIds.Remove(remoteId);
            }
        }
    }
}
#endif