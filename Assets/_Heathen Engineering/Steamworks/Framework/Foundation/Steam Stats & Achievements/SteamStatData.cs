#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> containing the definition of a Steam Stat.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this object simply contains the definition of a stat that has been created in the Steam API.
    /// for more information please see <a href="https://partner.steamgames.com/doc/features/achievements">https://partner.steamgames.com/doc/features/achievements</a>
    /// </para>
    /// </remarks>
    [Serializable]
    public abstract class SteamStatData : ScriptableObject
    {
        /// <summary>
        /// The name of the stat as it appears in the Steam Portal
        /// </summary>
        public string statName;
        /// <summary>
        /// Indicates the data type of this stat.
        /// This is used when working with the generic <see cref="SteamStatData"/> reference.
        /// </summary>
        public abstract StatDataType DataType { get; }
        /// <summary>
        /// This should only be called internally when the Steam client notifies the system of an updated value
        /// This does not call SetStat on the Steam backend
        /// </summary>
        /// <param name="value"></param>
        internal abstract void InternalUpdateValue(int value);
        /// <summary>
        /// This should only be called internally when the Steam client notifies the system of an updated value
        /// This does not call SetStat on the Steam backend
        /// </summary>
        /// <param name="value"></param>
        internal abstract void InternalUpdateValue(float value);
        /// <summary>
        /// Returns the value of this stat as an int.
        /// This is used when working with the generic <see cref="SteamStatData"/> reference.
        /// </summary>
        /// <returns></returns>
        public abstract int GetIntValue();
        /// <summary>
        /// Returns the value of this stat as a float.
        /// This is used when working with the generic <see cref="SteamStatData"/> reference.
        /// </summary>
        /// <returns></returns>
        public abstract float GetFloatValue();
        /// <summary>
        /// Sets the value of this stat on the Steam API.
        /// This is used when working with the generic <see cref="SteamStatData"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public abstract void SetIntStat(int value);
        /// <summary>
        /// Sets the value of this stat on the Steam API.
        /// This is used when working with the generic <see cref="SteamStatData"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public abstract void SetFloatStat(float value);
        /// <summary>
        /// This stores all stats to the Valve backend servers it is not possible to store only 1 stat at a time
        /// Note that this will cause a callback from Steam which will cause the stats to update
        /// </summary>
        public abstract void StoreStats();
        /// <summary>
        /// Occures when the Set Value methods are called.
        /// </summary>
        public UnityStatEvent ValueChanged;

        /// <summary>
        /// The availble type of stat data used in the Steam API
        /// </summary>
        public enum StatDataType
        {
            Int,
            Float
        }
    }
}
#endif
