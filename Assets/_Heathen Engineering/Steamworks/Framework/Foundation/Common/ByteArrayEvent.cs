#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// Used in the <see cref="HeathenEngineering.SteamApi.GameServices.SteamworksVoiceManager"/> class to return VoiceStream data
    /// </summary>
    /// <remarks>
    /// This is a simple wrap around UnityEvent&lt;byte[]&gt; to make it visible in Unity Editor windows.
    /// The event will invoke a method that takes a byte[] as a param such as
    /// <code>
    /// private void HandleByteArrayEvent(byte[] param) 
    /// {
    /// }
    /// </code>
    /// </remarks>
    [Serializable]
    public class ByteArrayEvent : UnityEvent<byte[]>
    { }
}
#endif