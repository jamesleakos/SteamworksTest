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
using System.IO;
using System.Threading;

namespace HeathenEngineering.SteamApi.Networking
{
    /// <summary>
    /// A Mirror compatable transport which leverages Steam's SteamP2PNetworking interface for peer to peer networking situation.
    /// </summary>
    /// <remarks>
    /// Derived from FizzCube's FizzySteamyMirror available under the MIT license on GetHub as an extension of Mirror. 
    /// For the latest version check his GitHub at https://github.com/Raystorms/FizzySteamyMirror
    /// Note that this version has been modified to suit Heathen Steamworks.
    /// </remarks>
    public class HeathenSteamP2PTransport : Transport
    {
        private const string STEAM_SCHEME = "steam";

        private Client client;
        private Server server;

        private Common activeNode;

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
                activeNode?.ReceiveData();
            }
        }

        public override bool ClientConnected() => ClientActive() && client.Connected;
        public override void ClientConnect(string address)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("SteamWorks not initialized. Client could not be started.");
                OnClientDisconnected.Invoke();
                return;
            }

            FetchSteamID();

            if (ServerActive())
            {
                Debug.LogError("Transport already running as server!");
                return;
            }

            if (!ClientActive() || client.Error)
            {
                Debug.Log($"Starting client, target address {address}.");

                SteamNetworking.AllowP2PPacketRelay(AllowSteamRelay);
                client = Client.CreateClient(this, address);
                activeNode = client;
            }
            else
            {
                Debug.LogError("Client already running!");
            }
        }

        public override void ClientConnect(Uri uri)
        {
            if (uri.Scheme != STEAM_SCHEME)
                throw new ArgumentException($"Invalid url {uri}, use {STEAM_SCHEME}://SteamID instead", nameof(uri));

            ClientConnect(uri.Host);
        }

        public override void ClientSend(int channelId, ArraySegment<byte> segment)
        {
            byte[] data = new byte[segment.Count];
            Array.Copy(segment.Array, segment.Offset, data, 0, segment.Count);
            client.Send(data, channelId);
        }

        public override void ClientDisconnect()
        {
            if (ClientActive())
            {
                Shutdown();
            }
        }
        public bool ClientActive() => client != null;


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
                SteamNetworking.AllowP2PPacketRelay(AllowSteamRelay);
                server = Server.CreateServer(this, NetworkManager.singleton.maxConnections);
                activeNode = server;
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
            client?.Disconnect();

            server = null;
            client = null;
            activeNode = null;
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
            if (activeNode != null)
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

        public abstract class Common
        {
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

            protected readonly HeathenSteamP2PTransport transport;

            protected Common(HeathenSteamP2PTransport transport)
            {
                channels = transport.Channels;

                callback_OnNewConnection = Callback<P2PSessionRequest_t>.Create(OnNewConnection);
                callback_OnConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnConnectFail);

                this.transport = transport;
            }

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

            protected abstract void OnNewConnection(P2PSessionRequest_t result);

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

            protected void SendInternal(CSteamID target, InternalMessages type) => SteamNetworking.SendP2PPacket(target, new byte[] { (byte)type }, 1, EP2PSend.k_EP2PSendReliable, internal_ch);
            protected void Send(CSteamID host, byte[] msgBuffer, int channel)
            {
                SteamNetworking.SendP2PPacket(host, msgBuffer, (uint)msgBuffer.Length, channels[channel], channel);
            }
            private bool Receive(out CSteamID clientSteamID, out byte[] receiveBuffer, int channel)
            {
                if (SteamNetworking.IsP2PPacketAvailable(out uint packetSize, channel))
                {
                    receiveBuffer = new byte[packetSize];
                    return SteamNetworking.ReadP2PPacket(receiveBuffer, packetSize, out _, out clientSteamID, channel);
                }

                receiveBuffer = null;
                clientSteamID = CSteamID.Nil;
                return false;
            }

            protected void CloseP2PSessionWithUser(CSteamID clientSteamID) => SteamNetworking.CloseP2PSessionWithUser(clientSteamID);

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

            protected abstract void OnReceiveInternalData(InternalMessages type, CSteamID clientSteamID);
            protected abstract void OnReceiveData(byte[] data, CSteamID clientSteamID, int channel);
            protected abstract void OnConnectionFailed(CSteamID remoteId);
        }

        /// <summary>
        /// Internal class defining the client structure for P2P connections
        /// </summary>
        public class Client : Common
        {
            public bool Connected { get; private set; }
            public bool Error { get; private set; }

            private event Action<byte[], int> OnReceivedData;
            private event Action OnConnected;
            private event Action OnDisconnected;

            private TimeSpan ConnectionTimeout;

            private CSteamID hostSteamID = CSteamID.Nil;
            private TaskCompletionSource<Task> connectedComplete;
            private CancellationTokenSource cancelToken;

            private Client(HeathenSteamP2PTransport transport) : base(transport)
            {
                ConnectionTimeout = TimeSpan.FromSeconds(Math.Max(1, transport.Timeout));
            }

            public static Client CreateClient(HeathenSteamP2PTransport transport, string host)
            {
                Client c = new Client(transport);

                c.OnConnected += () => transport.OnClientConnected.Invoke();
                c.OnDisconnected += () => transport.OnClientDisconnected.Invoke();
                c.OnReceivedData += (data, channel) => transport.OnClientDataReceived.Invoke(new ArraySegment<byte>(data), channel);

                if (SteamManager.Initialized)
                {
                    c.Connect(host);
                }
                else
                {
                    Debug.LogError("SteamWorks not initialized");
                    c.OnConnectionFailed(CSteamID.Nil);
                }

                return c;
            }

            private async void Connect(string host)
            {
                cancelToken = new CancellationTokenSource();

                try
                {
                    hostSteamID = new CSteamID(UInt64.Parse(host));
                    connectedComplete = new TaskCompletionSource<Task>();

                    OnConnected += SetConnectedComplete;

                    SendInternal(hostSteamID, InternalMessages.CONNECT);

                    Task connectedCompleteTask = connectedComplete.Task;

                    if (await Task.WhenAny(connectedCompleteTask, Task.Delay(ConnectionTimeout, cancelToken.Token)) != connectedCompleteTask)
                    {
                        Debug.LogError($"Connection to {host} timed out.");
                        OnConnected -= SetConnectedComplete;
                        OnConnectionFailed(hostSteamID);
                    }

                    OnConnected -= SetConnectedComplete;
                }
                catch (FormatException)
                {
                    Debug.LogError($"Connection string was not in the right format. Did you enter a SteamId?");
                    Error = true;
                    OnConnectionFailed(hostSteamID);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                    Error = true;
                    OnConnectionFailed(hostSteamID);
                }
                finally
                {
                    if (Error)
                    {
                        OnConnectionFailed(CSteamID.Nil);
                    }
                }

            }

            public void Disconnect()
            {
                Debug.Log("Sending Disconnect message");
                SendInternal(hostSteamID, InternalMessages.DISCONNECT);
                Dispose();
                cancelToken?.Cancel();

                WaitForClose(hostSteamID);
            }

            private void SetConnectedComplete() => connectedComplete.SetResult(connectedComplete.Task);

            protected override void OnReceiveData(byte[] data, CSteamID clientSteamID, int channel)
            {
                if (clientSteamID != hostSteamID)
                {
                    Debug.LogError("Received a message from an unknown");
                    return;
                }

                OnReceivedData.Invoke(data, channel);
            }

            protected override void OnNewConnection(P2PSessionRequest_t result)
            {
                if (hostSteamID == result.m_steamIDRemote)
                {
                    SteamNetworking.AcceptP2PSessionWithUser(result.m_steamIDRemote);
                }
                else
                {
                    Debug.LogError("P2P Acceptance Request from unknown host ID.");
                }
            }

            protected override void OnReceiveInternalData(InternalMessages type, CSteamID clientSteamID)
            {
                switch (type)
                {
                    case InternalMessages.ACCEPT_CONNECT:
                        Connected = true;
                        OnConnected.Invoke();
                        Debug.Log("Connection established.");
                        break;
                    case InternalMessages.DISCONNECT:
                        Connected = false;
                        Debug.Log("Disconnected.");
                        OnDisconnected.Invoke();
                        break;
                    default:
                        Debug.Log("Received unknown message type");
                        break;
                }
            }

            public void Send(byte[] data, int channelId) => Send(hostSteamID, data, channelId);

            protected override void OnConnectionFailed(CSteamID remoteId) => OnDisconnected.Invoke();
        }

        /// <summary>
        /// Internal class defining the server structure for P2P connections
        /// </summary>
        public class Server : Common
        {
            private event Action<int> OnConnected;
            private event Action<int, byte[], int> OnReceivedData;
            private event Action<int> OnDisconnected;
            private event Action<int, Exception> OnReceivedError;

            private BidirectionalDictionary<CSteamID, int> steamToMirrorIds;
            private int maxConnections;
            private int nextConnectionID;

            public static Server CreateServer(HeathenSteamP2PTransport transport, int maxConnections)
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

            private Server(HeathenSteamP2PTransport transport, int maxConnections) : base(transport)
            {
                this.maxConnections = maxConnections;
                steamToMirrorIds = new BidirectionalDictionary<CSteamID, int>();
                nextConnectionID = 1;
            }

            protected override void OnNewConnection(P2PSessionRequest_t result) => SteamNetworking.AcceptP2PSessionWithUser(result.m_steamIDRemote);

            protected override void OnReceiveInternalData(InternalMessages type, CSteamID clientSteamID)
            {
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

            protected override void OnReceiveData(byte[] data, CSteamID clientSteamID, int channel)
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

            protected override void OnConnectionFailed(CSteamID remoteId)
            {
                int connectionId = steamToMirrorIds.TryGetValue(remoteId, out int connId) ? connId : nextConnectionID++;
                OnDisconnected.Invoke(connectionId);

                steamToMirrorIds.Remove(remoteId);
            }
        }
    }
}
#endif