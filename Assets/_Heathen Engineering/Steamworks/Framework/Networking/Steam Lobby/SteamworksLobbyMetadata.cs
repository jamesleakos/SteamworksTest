#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeathenEngineering.SteamApi.Networking
{
    [Serializable]
    public struct SteamworksLobbyMetadata
    {
        public List<MetadataRecord> Records;

        public string this[string dataKey]
        {
            get
            {
                if (Records == null || Records.Count < 1)
                    return string.Empty;
                else
                    return Records.FirstOrDefault(p => p.key == dataKey).value;
            }
            set
            {
                if(string.IsNullOrEmpty(dataKey))
                {
                    throw new IndexOutOfRangeException("Attempted to store Value = '" + value + "' in an empty key. The key must be a non-empty string.");
                }

                if (Records == null)
                    Records = new List<MetadataRecord>();

                if (Records.Count < 1 || !Records.Exists(p => p.key == dataKey))
                {
                    Records.Add(new MetadataRecord() { key = dataKey, value = value });
                }
                else
                {
                    Records.RemoveAll(p => p.key == dataKey);
                    Records.Add(new MetadataRecord() { key = dataKey, value = value });
                }
            }
        }
    }
}
#endif