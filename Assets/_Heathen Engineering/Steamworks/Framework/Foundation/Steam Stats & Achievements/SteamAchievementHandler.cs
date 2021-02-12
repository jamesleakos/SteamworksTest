#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// Exposes the 'On Unlock' Unity Event of an achievement to the Unity inspector. 
    /// </summary>
    /// <remarks>
    /// Use this componenet to connect methods within other mono behaviours to the On Unlock event of a specific <see cref="SteamAchievementData"/> object.
    /// </remarks>
    public class SteamAchievementHandler : MonoBehaviour
    {
        /// <summary>
        /// A reference to the achievement this component should listen for
        /// </summary>
        public SteamAchievementData achievement;
        public UnityEvent onUnlock;

        private void OnEnable()
        {
            achievement.OnUnlock.AddListener(handleUnlock);
        }

        private void OnDisable()
        {
            achievement.OnUnlock.RemoveListener(handleUnlock);
        }

        private void handleUnlock()
        {
            onUnlock.Invoke();
        }
    }
}
#endif