#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
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
    [CreateAssetMenu(menuName = "Steamworks/Foundation/Int Stat Data")]
    public class SteamIntStatData : SteamStatData
    {
        [SerializeField]
        private int value;
        /// <summary>
        /// On get this returns the current stored value of the stat.
        /// On set this sets the value on the Steam API
        /// </summary>
        public int Value
        {
            get { return value; }
            set
            {
                SetIntStat(value);
            }
        }

        /// <summary>
        /// Indicates the data type of this stat.
        /// This is used when working with the generic <see cref="SteamStatData"/> reference.
        /// </summary>
        public override StatDataType DataType { get { return StatDataType.Int; } }

        /// <summary>
        /// Returns the value of this stat as a float.
        /// This is used when working with the generic <see cref="SteamStatData"/> reference.
        /// </summary>
        /// <returns></returns>
        public override float GetFloatValue()
        {
            return Value;
        }

        /// <summary>
        /// Returns the value of this stat as an int.
        /// This is used when working with the generic <see cref="SteamStatData"/> reference.
        /// </summary>
        /// <returns></returns>
        public override int GetIntValue()
        {
            return Value;
        }

        /// <summary>
        /// Sets the value of this stat on the Steam API.
        /// This is used when working with the generic <see cref="SteamStatData"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public override void SetFloatStat(float value)
        {
            if (this.value != (int)value)
            {
                this.value = (int)value;
                SteamUserStats.SetStat(statName, value);
                ValueChanged.Invoke(this);
            }
        }

        /// <summary>
        /// Sets the value of this stat on the Steam API.
        /// This is used when working with the generic <see cref="SteamStatData"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public override void SetIntStat(int value)
        {
            if (this.value != value)
            {
                this.value = value;
                SteamUserStats.SetStat(statName, value);
                ValueChanged.Invoke(this);
            }
        }

        /// <summary>
        /// This stores all stats to the Valve backend servers it is not possible to store only 1 stat at a time
        /// Note that this will cause a callback from Steam which will cause the stats to update
        /// </summary>
        public override void StoreStats()
        {
            SteamUserStats.StoreStats();
        }

        /// <summary>
        /// This should only be called internally when the Steam client notifies the system of an updated value
        /// This does not call SetStat on the Steam backend
        /// </summary>
        /// <param name="value"></param>
        internal override void InternalUpdateValue(int value)
        {
            if (value != Value)
            {
                Value = value;
                ValueChanged.Invoke(this);
            }
        }

        /// <summary>
        /// This should only be called internally when the Steam client notifies the system of an updated value
        /// This does not call SetStat on the Steam backend
        /// </summary>
        /// <param name="value"></param>
        internal override void InternalUpdateValue(float value)
        {
            var v = (int)value;
            if (v != Value)
            {
                Value = v;
                ValueChanged.Invoke(this);
            }
        }
    }
}
#endif