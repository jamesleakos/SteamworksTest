#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS && MIRROR
using Mirror;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{
    public class HeathenCustomNetworkManager : Mirror.NetworkManager
    {
        public UnityEvent OnHostStarted;
        public UnityEvent OnServerStarted;
        public UnityEvent OnClientStarted;
        public UnityEvent OnServerStopped;
        public UnityEvent OnClientStopped;
        public UnityEvent OnHostStopped;
        [Obsolete("No longer used.")]
        public UnityEvent OnRegisterServerMessages;
        [Obsolete("No longer used.")]
        public UnityEvent OnRegisterClientMessages;

        public override void OnStartHost()
        { OnHostStarted.Invoke(); }
        public override void OnStartServer()
        { OnServerStarted.Invoke(); }
        
        public override void OnStartClient()
        {
            OnClientStarted.Invoke();
        }
        public override void OnStopServer()
        { OnServerStopped.Invoke(); }
        public override void OnStopClient()
        { OnClientStopped.Invoke(); }
        public override void OnStopHost()
        { OnHostStopped.Invoke(); }
    }
}
#endif