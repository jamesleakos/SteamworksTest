#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// Handles Friend chat message events
    /// See <a href="https://partner.steamgames.com/doc/api/steam_api#EChatEntryType">https://partner.steamgames.com/doc/api/steam_api#EChatEntryType</a> for more details
    /// </summary>
    /// <remarks>
    /// will invoke a method that takes a <see cref="SteamUserData"/>, string and <see cref="EChatEntryType"/> as a param e.g.
    /// <code>
    /// private void HandleFriendChatMessageEvent(SteamUserData user, string message, EChatEntryType entryType)
    /// {
    /// }
    /// </code>
    /// </remarks>
    [Serializable]
    public class FriendChatMessageEvent : UnityEvent<SteamUserData, string, EChatEntryType> { }
}
#endif