#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> containing the definition of a Steam Achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this object simply contains the definition of an achievement that has been created in the Steam API.
    /// for more information please see <a href="https://partner.steamgames.com/doc/features/achievements">https://partner.steamgames.com/doc/features/achievements</a>
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = "Steamworks/Foundation/Achievement Data")]
    public class SteamAchievementData : ScriptableObject
    {
        /// <summary>
        /// The API Name as it appears in the Steam portal.
        /// </summary>
        public string achievementId;
        /// <summary>
        /// Indicates that this achievment has been unlocked by this user.
        /// </summary>
        [NonSerialized]
        public bool isAchieved;
        /// <summary>
        /// The display name for this achievement.
        /// </summary>
        [NonSerialized]
        public string displayName;
        /// <summary>
        /// The display description for this achievement.
        /// </summary>
        [NonSerialized]
        public string displayDescription;
        /// <summary>
        /// Is this achievement a hidden achievement.
        /// </summary>
        [NonSerialized]
        public bool hidden;
        /// <summary>
        /// Occures when this achivement has been unlocked.
        /// </summary>
        public UnityEvent OnUnlock;

        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
        /// </summary>
        public void Unlock()
        { 
            if (!isAchieved)
            {
                isAchieved = true;
                SteamUserStats.SetAchievement(achievementId);
                OnUnlock.Invoke();
            }
        }

        /// <summary>
        /// <para>Resets the unlock status of an achievmeent.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
        /// </summary>
        public void ClearAchievement()
        {
            isAchieved = false;
            SteamUserStats.ClearAchievement(achievementId);
        }
    }
}
#endif