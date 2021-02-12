#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEngine;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamApi.Foundation.Demo
{
    /// <summary>
    /// Demonstrates Achievement and Status update activity
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a demo script and is not inteded for prodcution use.
    /// It does not adhere to best practices for production grade code instead it focuses on verbose demonstration of easily understood features and fucntons.
    /// This code should be used to understand how one can use the members of the Steam Achievement and Steam Stat related objects.
    /// </para>
    /// </remarks>
    public class ExampleStatsUpdate : MonoBehaviour
    {
        /// <summary>
        /// Reference to the <see cref="steamSettings"/> object
        /// </summary>
        [FormerlySerializedAs("SteamSettings")]
        public SteamSettings steamSettings;
        /// <summary>
        /// Reference to the <see cref="SteamFloatStatData"/> object
        /// </summary>
        [FormerlySerializedAs("StatDataObject")]
        public SteamFloatStatData statDataObject;
        /// <summary>
        /// Reference to the <see cref="SteamAchievementData"/> object
        /// </summary>
        [FormerlySerializedAs("WinnerAchievement")]
        public SteamAchievementData winnerAchievement;
        /// <summary>
        /// Used to display the current stat value
        /// </summary>
        [FormerlySerializedAs("StatValue")]
        public UnityEngine.UI.Text statValue;
        /// <summary>
        /// Used to display the current achievement unlock status
        /// </summary>
        [FormerlySerializedAs("WinnerAchievmentStatus")]
        public UnityEngine.UI.Text winnerAchievmentStatus;

        private void Update()
        {
            statValue.text = "Feet Traveled = " + statDataObject.Value.ToString();
            winnerAchievmentStatus.text = winnerAchievement.displayName + "\n" + (winnerAchievement.isAchieved ? "(Unlocked)" : "(Locked)");
        }

        /// <summary>
        /// Sets and stores the value of the Steam stat
        /// </summary>
        /// <param name="amount"></param>
        public void UpdateStatValue(float amount)
        {
            statDataObject.SetFloatStat(statDataObject.Value + amount);
            steamSettings.client.StoreStatsAndAchievements();
        }

        /// <summary>
        /// Open the Valve documentation to the Achievements page
        /// </summary>
        public void GetHelp()
        {
            Application.OpenURL("https://partner.steamgames.com/doc/features/achievements");
        }

        /// <summary>
        /// Open the valve documentation to the Overlay page
        /// </summary>
        public void GetOverlayHelp()
        {
            Application.OpenURL("https://partner.steamgames.com/doc/features/overlay");
        }

        /// <summary>
        /// Notify when the stats are recieved
        /// This is meant to be connected to the Unity Events on the Steamworks Foundation Manager
        /// </summary>
        public void OnRetrieveStatsAndAchievements()
        {
            Debug.Log("[ExampleStatsUpdate.OnRetrieveStatsAndAchievement]\nStats loaded!");
        }

        /// <summary>
        /// Notify when the stats are stored
        /// This is meant to be connected to the Unity Events on the Steamworks Foundation Manager
        /// </summary>
        public void OnStoredStatsAndAchievements()
        {
            Debug.Log("[ExampleStatsUpdate.OnStoredStatsAndAchievements]\nStats stored!");
        }
    }
}
#endif