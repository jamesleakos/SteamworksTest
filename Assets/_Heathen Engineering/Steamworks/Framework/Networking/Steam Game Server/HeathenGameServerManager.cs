#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using HeathenEngineering.SteamApi.Foundation;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Networking
{
    /// <summary>
    /// Depracated system, funcitonality contained here has been moved to <see cref="SteamSettings"/> and <see cref="SteamworksFoundationManager"/>
    /// </summary>
    [Obsolete("Heathen Game Server Manager is depricated its funcitonality has been moved to SteamSettings and SteamworksFoundationManager", true)]
    public class HeathenGameServerManager : MonoBehaviour
    { }
}
#endif