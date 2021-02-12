#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using System.Linq;
using HeathenEngineering.Scriptable;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// <para>A collection of fields that can be serialized and saved to the Steam Remote Storage system. This can be most simply thought of as the structure of a save file.</para>
    /// <para>To create a new <see cref="SteamDataLibrary"/> in your project locate the folder where you want it to live and right click selecting the following path</para>
    /// <para>Create >> Library >> Steam Game Data Library</para>
    /// <para>This will create a new <see cref="SteamDataLibrary"/> in your project where you add fields</para>
    /// <para>To add a field locate a folder where you want the field to reside, right click and select</para>
    /// <para>Create >> Variables</para>
    /// <para>And then select the type of field you would like to create. These <see cref="HeathenEngineering.Scriptable.DataVariable"/></para>
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "Library Variables/Steam Game Data Library")]
    public class SteamDataLibrary : DataLibraryVariable
    {
        /// <summary>
        /// <para>The prefix of the file.</para>
        /// <para>This serves a funciton similar to a file extension in Windows in that the prefix of a file is how the system determins which <see cref="SteamDataLibrary"/> a file belongs to  ... e.g. if the prefix of a library is 'sys_' then all files whoes name start with sys_ will be assumed to belong to that library.</para>
        /// </summary>
        public string filePrefix;
        /// <summary>
        /// If data has been loaded this will point to the SteamDataFile object that was loaded.
        /// This is useful for saving updates back to that same file.
        /// </summary>
        [HideInInspector]
        public SteamDataFile activeFile;
        /// <summary>
        /// <para>A list of files matched to this library that are available to be loaded.</para>
        /// <para>This gets updated for all <see cref="SteamDataLibrary"/> objects registered to <see cref="SteamworksRemoteStorageManager"/> when the <see cref="SteamworksRemoteStorageManager.RefreshFileList"/> method is called.</para>
        /// </summary>
        [HideInInspector]
        public List<SteamworksRemoteStorageManager.FileAddress> availableFiles = new List<SteamworksRemoteStorageManager.FileAddress>();

        /// <summary>
        /// Saves the current library data to the current active file
        /// </summary>
        /// <returns></returns>
        public void Save()
        {
            if(activeFile == null)
            {
                Debug.Log("");
            }

            activeFile.linkedLibrary = this;
            SteamworksRemoteStorageManager.FileWrite(activeFile);
        }
        
        /// <summary>
        /// Saves the file with a new name.
        /// Note that if the provided file name does not start with the filePrefix defined then it will be added
        /// </summary>
        /// <param name="fileName">The name to save as
        /// Note that if the provided file name does not start with the filePrefix defined then it will be added</param>
        /// <returns></returns>
        public void SaveAs(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            if (!fileName.StartsWith(filePrefix))
                fileName = filePrefix + fileName;

            var result = SteamworksRemoteStorageManager.FileWrite(fileName, this);

            if(result)
            {
                Debug.Log("[SteamDataLibrary.SaveAs] Saved '" + fileName + "' successfully.");
            }
            else
            {
                Debug.LogWarning("[SteamDataLibrary.SaveAs] Failed to save '" + fileName + "' to Steam Remote Storage.\nPlease consult https://partner.steamgames.com/doc/api/ISteamRemoteStorage#FileWrite for more information.");
            }
        }

        /// <summary>
        /// Saves the current library data to the current active file
        /// </summary>
        /// <returns></returns>
        public void SaveAsync()
        {
            activeFile.linkedLibrary = this;
            var file = SteamworksRemoteStorageManager.FileWriteAsync(activeFile);
            if(file.result != Steamworks.EResult.k_EResultFail)
            {
                file.Complete = results =>
                {
                    activeFile = results;
                };
            }
        }

        /// <summary>
        /// Saves the file with a new name.
        /// Note that if the provided file name does not start with the filePrefix defined then it will be added
        /// </summary>
        /// <param name="fileName">The name to save as
        /// Note that if the provided file name does not start with the filePrefix defined then it will be added</param>
        /// <returns></returns>
        public void SaveAsAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            if (!fileName.StartsWith(filePrefix))
                fileName = filePrefix + fileName;

            var file = SteamworksRemoteStorageManager.FileWriteAsync(fileName, this);
            if (file.result != Steamworks.EResult.k_EResultFail)
            {
                file.Complete = results =>
                {
                    activeFile = results;
                };
            }
        }

        public void Load(string fileName)
        {
            if (fileName.StartsWith(filePrefix))
            {
                var result = SteamworksRemoteStorageManager.FileReadSteamDataFile(fileName);
                activeFile = result;
                result.WriteToLibrary(this);
            }
            else
            {
                var result = SteamworksRemoteStorageManager.FileReadSteamDataFile(filePrefix + fileName);
                activeFile = result;
                result.WriteToLibrary(this);
            }
        }

        public void LoadAsync(string fileName)
        {
            if (fileName.StartsWith(filePrefix))
            {
                SteamworksRemoteStorageManager.FileReadAsync(fileName).Complete = fileResult =>
                {
                    activeFile = fileResult;
                    fileResult.WriteToLibrary(this);
                };
            }
            else
            {
                SteamworksRemoteStorageManager.FileReadAsync(filePrefix + fileName).Complete = fileResult =>
                {
                    activeFile = fileResult;
                    fileResult.WriteToLibrary(this);
                };
            }
        }

        /// <summary>
        /// Loads the data for the current active file if any
        /// Note that this will overwrite the data current stored in the library
        /// </summary>
        /// <returns>True if the operation completed, false if skiped such as for a blank active file</returns>
        public void Load()
        {
            if (activeFile != null)
            {
                activeFile = SteamworksRemoteStorageManager.FileReadSteamDataFile(activeFile.address);
                activeFile.WriteToLibrary(this);
            }
        }

        /// <summary>
        /// Loads the data for the current active file if any
        /// Note that this will overwrite the data current stored in the library
        /// </summary>
        /// <returns>True if the operation completed, false if skiped such as for a blank active file</returns>
        public void LoadAsync()
        {
            if (activeFile != null)
            {
                SteamworksRemoteStorageManager.FileReadAsync(activeFile.address).Complete = results =>
                {
                    activeFile = results;
                    activeFile.WriteToLibrary(this);
                };
            }
        }

        /// <summary>
        /// Loads the data from a given address
        /// Note that the load operation will only establish the result as the active data if its prefix matches
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public void Load(SteamworksRemoteStorageManager.FileAddress address)
        {
            if (!string.IsNullOrEmpty(address.fileName) && address.fileName.StartsWith(filePrefix))
            {
                activeFile = SteamworksRemoteStorageManager.FileReadSteamDataFile(address);
                activeFile.WriteToLibrary(this);
            }
        }

        /// <summary>
        /// Loads the data from a given address
        /// Note that the load operation will only establish the result as the active data if its prefix matches
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public void LoadAsync(SteamworksRemoteStorageManager.FileAddress address)
        {
            if (!string.IsNullOrEmpty(address.fileName) && address.fileName.StartsWith(filePrefix))
            {
                var nDataFile = SteamworksRemoteStorageManager.FileReadAsync(address);
                if(nDataFile.result != Steamworks.EResult.k_EResultFail)
                {
                    nDataFile.Complete = results =>
                    {
                        activeFile = results;
                        activeFile.WriteToLibrary(this);
                    };
                }
            }
        }

        /// <summary>
        /// Loads the data from a given address
        /// Note that the load operation will only establish the result as the active data if its prefix matches
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public void Load(int availableFileIndex)
        {
            if (availableFileIndex >= 0 && availableFileIndex < availableFiles.Count)
            {
                Load(availableFiles[availableFileIndex]);
            }
        }

        /// <summary>
        /// Loads the data from a given address
        /// Note that the load operation will only establish the result as the active data if its prefix matches
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public void LoadAsync(int availableFileIndex)
        {
            if (availableFileIndex >= 0 && availableFileIndex < availableFiles.Count)
            {
                LoadAsync(availableFiles[availableFileIndex]);
            }
        }
    }
}
#endif