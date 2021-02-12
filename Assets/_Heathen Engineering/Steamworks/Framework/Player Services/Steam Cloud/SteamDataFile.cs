#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// <para>Defines file data relative to Steam Remote Storage</para>
    /// <para>This is the raw structure of data as seen on the Steam Remote Storage system and includes its address, metadata about the file and the raw data of the file.</para>
    /// </summary>
    [Serializable]
    public class SteamDataFile
    {
        /// <summary>
        /// Metadata regarding the Steam Data File including its location, size, name and time stamps.
        /// </summary>
        public SteamworksRemoteStorageManager.FileAddress address;
        /// <summary>
        /// The binary data of the file in question.
        /// </summary>
        [HideInInspector]
        public byte[] binaryData;
        /// <summary>
        /// apiCall handle ... this is used internally to direct callbacks from asynchronious operations.
        /// </summary>
        [HideInInspector]
        public SteamAPICall_t? apiCall;
        /// <summary>
        /// The status of of the most resent operation ran against this process.
        /// </summary>
        [HideInInspector]
        public EResult result = EResult.k_EResultPending;
        /// <summary>
        /// The Steam Data Library this file is assoceated with. This is used to determ how to deserialize the byte data returned into meaningful fields for use in Unity.
        /// </summary>
        [HideInInspector]
        public SteamDataLibrary linkedLibrary;

        public Action<SteamDataFile> Complete;

        /// <summary>
        /// Reads the data from a SteamDataLibrary into the byte[] in preperation for submiting the data to the Steam Remote Storage system.
        /// </summary>
        /// <param name="dataLibrary"></param>
        public void ReadFromLibrary(SteamDataLibrary dataLibrary)
        {
            linkedLibrary = dataLibrary;
            dataLibrary.SyncToBuffer(out binaryData);
        }

        /// <summary>
        /// Writes the data stored in the <see cref="binaryData"/> field into the target SteamDataLibrary e.g. loads the data into meaningful Unity memory.
        /// </summary>
        /// <param name="dataLibrary"></param>
        public void WriteToLibrary(SteamDataLibrary dataLibrary)
        {
            linkedLibrary = dataLibrary;
            dataLibrary.SyncFromBuffer(binaryData);
        }

        /// <summary>
        /// Sets the binary data of the file equal to the encoded form of the <paramref name="jsonObject"/>
        /// </summary>
        /// <param name="jsonObject">The object to be encoded, this can be any type that is supported by UnityEngine.JsonUtility.</param>
        /// <param name="encoding">The encoding format to use on the resulting JSON string when converting it to a byte[] ... this would typically be System.Text.Encoding.UTF8</param>
        public void SetDataFromObject(object jsonObject, System.Text.Encoding encoding)
        {
            binaryData = encoding.GetBytes(JsonUtility.ToJson(jsonObject));
        }

        #region Encoding
        /// <summary>
        /// Encodes the binary data into UTF8
        /// </summary>
        /// <returns></returns>
        public string FromUTF8()
        {
            if (binaryData.Length > 0)
                return System.Text.Encoding.UTF8.GetString(binaryData);
            else
                return string.Empty;
        }
        /// <summary>
        /// Encodes the binary data into UTF32
        /// </summary>
        /// <returns></returns>
        public string FromUTF32()
        {
            if (binaryData.Length > 0)
                return System.Text.Encoding.UTF32.GetString(binaryData);
            else
                return string.Empty;
        }
        /// <summary>
        /// Encodes the binary data into Unicode
        /// </summary>
        /// <returns></returns>
        public string FromUnicode()
        {
            if (binaryData.Length > 0)
                return System.Text.Encoding.Unicode.GetString(binaryData);
            else
                return string.Empty;
        }
        /// <summary>
        /// Econdes the binary data into the system default encoding this will be platform dependent
        /// </summary>
        /// <returns></returns>
        public string FromDefaultEncoding()
        {
            if (binaryData.Length > 0)
                return System.Text.Encoding.Default.GetString(binaryData);
            else
                return string.Empty;
        }
        /// <summary>
        /// Encodes the binary data into ASCII
        /// </summary>
        /// <returns></returns>
        public string FromASCII()
        {
            if (binaryData.Length > 0)
                return System.Text.Encoding.ASCII.GetString(binaryData);
            else
                return string.Empty;
        }

        public string FromEncoding(System.Text.Encoding encoding)
        {
            return encoding.GetString(binaryData);
        }

        /// <summary>
        /// Treats the binary data as a JSON object stored with the indicated encoding and uses the <see cref="JsonUtility"/> to deserialize it
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="encoding">The encoding to encode the byte array with ... this is typically Encoding.UTF8</param>
        /// <returns></returns>
        public T FromJson<T>(System.Text.Encoding encoding)
        {
            return JsonUtility.FromJson<T>(encoding.GetString(binaryData));
        }
        #endregion

        public void HandleFileReadAsyncComplete(RemoteStorageFileReadAsyncComplete_t param, bool bIOFailure)
        {
            result = param.m_eResult;
            //If the request result was okay fetch the binary data
            if (result == EResult.k_EResultOK)
            {
                binaryData = new byte[address.fileSize];
                if (!SteamRemoteStorage.FileReadAsyncComplete(param.m_hFileReadAsync, binaryData, (uint)binaryData.Length))
                {
                    //If we failed to read the binary data update the result to fail
                    result = EResult.k_EResultFail;
                }
                else if (linkedLibrary != null)
                {
                    linkedLibrary.activeFile = this;
                    WriteToLibrary(linkedLibrary);
                }
            }

            if (Complete != null)
                Complete.Invoke(this);
        }

        public void HandleFileWriteAsyncComplete(RemoteStorageFileWriteAsyncComplete_t param, bool bIOFailure)
        {
            result = param.m_eResult;
            //If the request result was okay fetch the binary data
            if (result == EResult.k_EResultOK && linkedLibrary != null)
            {
                linkedLibrary.activeFile = this;
                WriteToLibrary(linkedLibrary);
            }

            if (Complete != null)
                Complete.Invoke(this);
        }
    }
}
#endif