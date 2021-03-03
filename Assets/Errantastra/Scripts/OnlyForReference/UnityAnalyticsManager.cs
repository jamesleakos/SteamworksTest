
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Errantastra
{ 
    /// <summary>
    /// Manager fo tracking user actions in-game and sending them to further processing
    /// or visualization of events triggered by players, using Unity Analytics.
    /// </summary>
    public class UnityAnalyticsManager : MonoBehaviour
    {
        /*
        ---not implemented yet--
        
        
        /// <summary>
        /// 1+2+2+3=8 Analysis Points
        /// </summary>
        public static void MainSceneClosed(int shopOpened, int settingsOpened, bool hasMusic, string activeTank)
        {		
            Analytics.CustomEvent("MainSceneClosed", new Dictionary<string, object>
            {
                { "shopOpened", shopOpened },
                { "settOpened", settingsOpened },
                { "hasMusic", hasMusic },
                { "activeTank", activeTank }
            });
        }


        /// <summary>
        /// 1=1 Analysis Points
        /// </summary>
        public static void RateStart()
        {
            Analytics.CustomEvent("RateStart", null);
        }


        /// <summary>
        /// 1=1 Analysis Points
        /// </summary>
        public static void ShareStart()
        {
            Analytics.CustomEvent("ShareStart", null);
        }


        /// <summary>
        /// 1=1 Analysis Points
        /// </summary>
        public static void ShareComplete()
        {
            Analytics.CustomEvent("ShareComplete", null);
        }


        /// <summary>
        /// 1=1 Analysis Points
        /// </summary>
        public static void RoundStart(int players)
        {		
            Analytics.CustomEvent("RoundStart", null);
        }


        /// <summary>
        /// 1+4+6+4=15 Analysis Points
        /// </summary>
        public static void RoundComplete(string team, bool isOver, bool hasWon,
                      int players, int timePlayed, int kills, int deaths,
                      int shotsFired, int powerUsed) 
        {
            Analytics.CustomEvent("RoundComplete", new Dictionary<string, object>
            {
                { "team", team },
                { "isOver", isOver },
                { "players", players },
                { "timePlayed", timePlayed },
                { "kills", kills },
                { "deaths", deaths },
                { "shotsFired", shotsFired },
                { "powerUsed", powerUsed }
            });
        }
        
        */
    }
}
