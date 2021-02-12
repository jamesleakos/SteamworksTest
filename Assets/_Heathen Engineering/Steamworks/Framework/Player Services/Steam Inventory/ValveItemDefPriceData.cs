#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [Serializable]
    public class ValveItemDefPriceData
    {
        public uint version = 1;
        public List<ValveItemDefPriceDataEntry> values;

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach(var v in values)
            {
                if (sb.Length > 0)
                    sb.Append(",");

                sb.Append(v.ToString());
            }

            return version.ToString() + ";" + sb.ToString();
        }
    }
}
#endif