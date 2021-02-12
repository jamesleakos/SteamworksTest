#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Foundation
{

    /// <summary>
    /// <para>This class wraps and extends SteamUtils</para>
    /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils">https://partner.steamgames.com/doc/api</a>
    /// </summary>
    public static class SteamUtilities
    {
        #region Colors
        public static class Colors
        {
            public static Color SteamBlue = new Color(0.2f, 0.60f, 0.93f, 1f);
            public static Color SteamGreen = new Color(0.2f, 0.42f, 0.2f, 1f);
            public static Color BrightGreen = new Color(0.4f, 0.84f, 0.4f, 1f);
            public static Color HalfAlpha = new Color(1f, 1f, 1f, 0.5f);
            public static Color ErrorRed = new Color(1, 0.5f, 0.5f, 1);
        }
        #endregion
        
        /// <summary>
        /// <para>Checks if the Overlay needs a present. Only required if using event driven render updates.</para>
        /// <para>Typically this call is unneeded if your game has a constantly running frame loop that calls the D3D Present API, or OGL SwapBuffers API every frame as is the case in most games. However, if you have a game that only refreshes the screen on an event driven basis then that can break the overlay, as it uses your Present/SwapBuffers calls to drive it's internal frame loop and it may also need to Present() to the screen any time a notification happens or when the overlay is brought up over the game by a user. You can use this API to ask the overlay if it currently need a present in that case, and then you can check for this periodically (roughly 33hz is desirable) and make sure you refresh the screen with Present or SwapBuffers to allow the overlay to do it's work.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#BOverlayNeedsPresent">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static bool OverlayNeedsPresent()
        {
            return SteamUtils.BOverlayNeedsPresent();
        }

        /// <summary>
        /// <para>Gets the App ID of the current process.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#GetAppID">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static AppId_t GetAppId()
        {
            return SteamUtils.GetAppID();
        }

        /// <summary>
        /// <para>Gets the current amount of battery power on the computer.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#GetCurrentBatteryPower">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static float GetCurrentBatteryPower()
        {
            var steamPower = SteamUtils.GetCurrentBatteryPower();
            return Mathf.Clamp01(steamPower / 100f);
        }

        /// <summary>
        /// <para>Returns the Steam server time in Unix epoch format. (Number of seconds since Jan 1, 1970 UTC)</para>  
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#GetServerRealTime">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static uint GetServerRealUnixTime()
        {
            return SteamUtils.GetServerRealTime();
        }

        /// <summary>
        /// <para>Returns the Steam server time in Unix epoch format. (Number of seconds since Jan 1, 1970 UTC)</para>  
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#GetServerRealTime">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static DateTime GetServerRealDateTime()
        {
            return ConvertUnixDate(GetServerRealUnixTime());
        }

        /// <summary>
        /// <para>Returns the language the steam client is running in.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#GetSteamUILanguage">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static string GetSteamClientLanguage()
        {
            return SteamUtils.GetSteamUILanguage();
        }

        /// <summary>
        /// <para>Gets the current language that the user has set.
        /// This falls back to the Steam UI language if the user hasn't explicitly picked a language for the title.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetCurrentGameLanguage">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentGameLanguage()
        {
            return SteamApps.GetCurrentGameLanguage();
        }

        /// <summary>
        /// <para>Gets the buildid of this app, may change at any time based on backend updates to the game.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetAppBuildId">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static int GetAppBuildId()
        {
            return SteamApps.GetAppBuildId();
        }

        /// <summary>
        /// <para>Gets the install folder for a specific AppID.
        /// This works even if the application is not installed, based on where the game would be installed with the default Steam library location.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetAppInstallDir">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static string GetAppInstallDir(AppId_t appId)
        {
            string results;
            SteamApps.GetAppInstallDir(appId, out results, 1024);
            return results;
        }

        /// <summary>
        /// <para>Gets the Steam ID of the original owner of the current app. If it's different from the current user then it is borrowed.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetAppOwner">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static CSteamID GetAppOwner()
        {
            return SteamApps.GetAppOwner();
        }

        /// <summary>
        /// <para>Gets the number of DLC pieces for the current app.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetDLCCount">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static int GetDLCCount()
        {
            return SteamApps.GetDLCCount();
        }

        /// <summary>
        /// <para>Returns a app DLC metadata object for the specified index.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#BGetDLCDataByIndex">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static AppDlcData GetDLCDataByIndex(int index)
        {
            var nData = new AppDlcData();
            if (SteamApps.BGetDLCDataByIndex(index, out nData.appId, out nData.available, out nData.name, 2048))
                return nData;
            else
            {
                nData.appId = AppId_t.Invalid;
                return nData;
            }
        }

        /// <summary>
        /// <para>Returns a collection of app DLC metadata for all available DLC.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#BGetDLCDataByIndex">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static List<AppDlcData> GetDLCData()
        {
            var results = new List<AppDlcData>();
            var count = GetDLCCount();
            for (int i = 0; i < count; i++)
            {
                var nData = new AppDlcData();
                if (SteamApps.BGetDLCDataByIndex(i, out nData.appId, out nData.available, out nData.name, 2048))
                    results.Add(nData);
            }
            return results;
        }

        /// <summary>
        /// <para>Gets the time of purchase of the specified app in Unix epoch format (time since Jan 1st, 1970).</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetEarliestPurchaseUnixTime">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static uint GetEarliestPurchaseUnixTime(AppId_t appId)
        {
            return SteamApps.GetEarliestPurchaseUnixTime(appId);
        }

        /// <summary>
        /// <para>Gets the time of purchase of the specified app.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetEarliestPurchaseUnixTime">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static DateTime GetEarliestPurchaseDateTime(AppId_t appId)
        {
            return ConvertUnixDate(GetEarliestPurchaseUnixTime(appId));
        }

        /// <summary>
        /// <para>Gets the command line if the game was launched via Steam URL, e.g. steam://run/<appid>//<command line>/. This method is preferable to launching with a command line via the operating system, which can be a security risk. In order for rich presence joins to go through this and not be placed on the OS command line, you must enable "Use launch command line" from the Installation > General page on your app.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetLaunchCommandLine">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static string GetLaunchCommandLine()
        {
            string buffer;
            SteamApps.GetLaunchCommandLine(out buffer, 1024);
            return buffer;
        }

        /// <summary>
        /// <para>Checks if Steam & the Steam Overlay are running in Big Picture mode.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#IsSteamInBigPictureMode">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        public static bool IsSteamInBigPictureMode
        {
            get { return SteamUtils.IsSteamInBigPictureMode(); }
        }

        /// <summary>
        /// <para>Checks if Steam is running in VR mode.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#IsSteamRunningInVR">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        public static bool IsSteamRunningInVR
        {
            get { return SteamUtils.IsSteamRunningInVR(); }
        }

        /// <summary>
        /// Flips an image buffer
        /// This is used when loading images from Steam as they tend to be inverted for what Unity wants
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] FlipImageBufferVertical(int width, int height, byte[] buffer)
        {
            byte[] result = new byte[buffer.Length];

            int xWidth = width * 4;
            int yHeight = height;

            for (int y = 0; y < yHeight; y++)
            {
                for (int x = 0; x < xWidth; x++)
                {
                    result[x + ((yHeight - 1 - y) * xWidth)] = buffer[x + (xWidth * y)];
                }
            }

            return result;
        }

        /// <summary>
        /// Read an image file from disk
        /// </summary>
        /// <param name="path"></param>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static bool LoadImageFromDisk(string path, out Texture2D texture)
        {
            if (File.Exists(path))
            {
                byte[] byteArray = File.ReadAllBytes(path);
                texture = new Texture2D(2, 2);
                return texture.LoadImage(byteArray);
            }
            else
            {
                Debug.LogError("Load Image From Disk called on file [" + path + "] but no such file was found.");
                texture = new Texture2D(2, 2);
                return false;
            }
        }

        /// <summary>
        /// Converts a Unix epoc style time stamp to a DateTime object
        /// </summary>
        /// <param name="nixTime"></param>
        /// <returns></returns>
        public static DateTime ConvertUnixDate(uint nixTime)
        {
            var timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return timeStamp.AddSeconds(nixTime);
        }

        /// <summary>
        /// Checks if the 'checkFlag' value is in the 'value'
        /// </summary>
        /// <param name="value"></param>
        /// <param name="checkflag"></param>
        /// <returns></returns>
        public static bool WorkshopItemStateHasFlag(EItemState value, EItemState checkflag)
        {
            return (value & checkflag) == checkflag;
        }

        /// <summary>
        /// Cheks if any of the 'checkflags' values are in the 'value'
        /// </summary>
        /// <param name="value"></param>
        /// <param name="checkflags"></param>
        /// <returns></returns>
        public static bool WorkshopItemStateHasAllFlags(EItemState value, params EItemState[] checkflags)
        {
            foreach (var checkflag in checkflags)
            {
                if ((value & checkflag) != checkflag)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Octet order from left to right as seen in string is index 0, 1, 2, 3
        /// </summary>
        /// <param name="address">And string which can be parsed by System.Net.IPAddress.Parse</param>
        /// <returns>Octet order from left to right as seen in string is index 0, 1, 2, 3</returns>
        public static byte[] IPStringToBytes(string address)
        {
            var ipAddress = IPAddress.Parse(address);
            return ipAddress.GetAddressBytes();
        }

        /// <summary>
        /// Expects octet order from index 0 to 3 for example string octet 1 as in the left most should be stored in index 0
        /// </summary>
        /// <param name="address">Expects octet order from index 0 to 3 for example string octet 1 as in the left most should be stored in index 0</param>
        /// <returns></returns>
        public static string IPBytesToString(byte[] address)
        {
            var ipAddress = new IPAddress(address);
            return ipAddress.ToString();
        }

        /// <summary>
        /// Octet order from left to right as seen in string e.g. byte 24, byte 16, byte 8, byte 0
        /// </summary>
        /// <param name="address">And string which can be parsed by System.Net.IPAddress.Parse</param>
        /// <returns>Octet order from left to right as seen in string e.g. byte 24, byte 16, byte 8, byte 0</returns>
        public static uint IPStringToUint(string address)
        {
            var ipBytes = IPStringToBytes(address);
            var ip = (uint)ipBytes[0] << 24;
            ip += (uint)ipBytes[1] << 16;
            ip += (uint)ipBytes[2] << 8;
            ip += (uint)ipBytes[3];
            return ip;
        }

        /// <summary>
        /// Returns a human friendly string version of the uint address
        /// </summary>
        /// <param name="address">Octet order from left to right as seen in string e.g. byte 24, byte 16, byte 8, byte 0</param>
        /// <returns></returns>
        public static string IPUintToString(uint address)
        {
            var ipBytes = BitConverter.GetBytes(address);
            var ipBytesRevert = new byte[4];
            ipBytesRevert[0] = ipBytes[3];
            ipBytesRevert[1] = ipBytes[2];
            ipBytesRevert[2] = ipBytes[1];
            ipBytesRevert[3] = ipBytes[0];
            return new IPAddress(ipBytesRevert).ToString();
        }
    }
}
#endif