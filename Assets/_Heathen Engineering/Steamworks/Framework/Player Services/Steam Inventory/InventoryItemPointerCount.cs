#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    [Serializable]
    public class InventoryItemPointerCount
    {
        public InventoryItemPointer Item;
        public uint Count;
        
        public override string ToString()
        {
            return Item.DefinitionID.m_SteamItemDef + "x" + Count;
        }
    }
}
#endif