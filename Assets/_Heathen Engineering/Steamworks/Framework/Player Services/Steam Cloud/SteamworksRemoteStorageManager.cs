#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// Manages the Steam Remote Storage system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Simplifies working with Steam's remote storage system.
    /// Enables the developer to list all files stored for the current user and integrates with the <see cref="SteamDataLibrary"/> system.
    /// </para>
    /// <para>
    /// The prefered method for saving and reading data is via JSON object. 
    /// This is a change from previous versions where the prefered method was via SteamDataLibrary.
    /// <see cref="SteamDataLibrary"/> is still a viable option and suitable for code free style implamentaitons however JSON serailization of custom serializable objects proves to be simpler, faster and more flexable assuming you are comfortable creating a serializable object to serve as your file structure.
    /// </para>
    /// </remarks>
    /// <example>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Get a list of all saved files.
    /// </description>
    /// <code>
    /// SteamworksRemoteStorageManager.RefreshFileList();
    /// Debug.Log("Found: " + SteamworksRemoateStorageManager.files.Count + " files");
    /// </code>
    /// </item>
    /// <item>
    /// <description>
    /// Save data to the server as a JSON file.
    /// </description>
    /// <code>
    /// //Any object that can be serialized by JsonUtility can be saved
    /// string[] exampleData = new string[] { "Hello", "World" };
    /// SteamworksRemoteStorageManager.FileWrite("FileName.json", exampleData, System.Text.Encoding.UTF8);
    /// </code>
    /// </item>
    /// <item>
    /// <description>
    /// Read data from a JSON object saved on the server
    /// </description>
    /// <code>
    /// string[] data = SteamworksRemoteStorageManager.FileReadJson&lt;string[]&gt;("FileName.json", System.Text.Encoding.UTF8);
    /// </code>
    /// </item>
    /// <item>
    /// <description>
    /// Save data to the server asynchroniously
    /// </description>
    /// <code>
    /// var data = new string[2] { "Hello", "World" };
    /// 
    /// var dataFile = new SteamDataFile
    /// {
    ///     address = new FileAddress { fileName = "FileName.json" },
    ///     binaryData = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)),
    /// };
    /// 
    /// SteamworksRemoteStorageManager.FileWriteAsync(dataFile).Complete = dataFileState =>
    ///         {
    ///             Debug.Log("Result " + dataFileState.result);
    ///         };
    /// </code>
    /// </item>
    /// <item>
    /// <description>
    /// Read data from the server asynchroniously
    /// </description>
    /// <code>
    /// string[] data;
    /// SteamworksRemoteStorageManager.FileReadAsync("FileName.json").Complete = dataFileState =>
    /// {
    ///     if (dataFileState.result == EResult.k_EResultOK)
    ///     {
    ///         data = dataFileState.FromJson&lt;string[]&gt;(System.Text.Encoding.UTF8);
    ///         Debug.Log("Loaded " + data.Length + " strings from a file named " + dataFileState.address.fileName);
    ///     }
    ///     else
    ///         Debug.LogError("Failed to load with responce code: " + dataFileState.result);
    /// };
    /// </code>
    /// </item>
    /// </list>
    /// </example>
    public class SteamworksRemoteStorageManager : MonoBehaviour
    {
        /// <summary>
        /// Defines the address of a data file as stored in Steam Remote Storage system.
        /// </summary>
        [Serializable]
        public struct FileAddress : IEquatable<FileAddress>
        {
            /// <summary>
            /// The index of the file in the current users Steam Remote Storage system.
            /// </summary>
            public int fileIndex;
            /// <summary>
            /// The size of the file in bytes
            /// </summary>
            public int fileSize;
            /// <summary>
            /// The name of the fille as it appears on the Steam Remote Storage system.
            /// </summary>
            public string fileName;
            /// <summary>
            /// The UTC time stamp of the file as read from Steam Remote Storage.
            /// </summary>
            public DateTime UtcTimestamp;
            /// <summary>
            /// The local time translation of the UTC time stamp of the file.
            /// </summary>
            public DateTime LocalTimestamp
            {
                get
                {
                    return UtcTimestamp.ToLocalTime();
                }
                set
                {
                    UtcTimestamp = value.ToUniversalTime();
                }
            }

            /// <summary>
            /// Compares the equivlancy of a SteamDataFileAddress to another SteamDataFileAddress
            /// </summary>
            /// <param name="obj1"></param>
            /// <param name="obj2"></param>
            /// <returns></returns>
            public static bool operator ==(FileAddress obj1, FileAddress obj2)
            {
                return obj1.Equals(obj2);
            }

            /// <summary>
            /// Compares the equivlancy of a SteamDataFileAddress to another SteamDataFileAddress
            /// </summary>
            /// <param name="obj1"></param>
            /// <param name="obj2"></param>
            /// <returns></returns>
            public static bool operator !=(FileAddress obj1, FileAddress obj2)
            {
                return !obj1.Equals(obj2);
            }

            /// <summary>
            /// Compares the equivlancy of a SteamDataFileAddress to another SteamDataFileAddress
            /// </summary>
            /// <returns></returns>
            public bool Equals(FileAddress other)
            {
                return fileIndex == other.fileIndex && fileName == other.fileName && fileSize == other.fileSize;
            }

            /// <summary>
            /// Compares the equivlancy of a SteamDataFileAddress to an object
            /// </summary>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                return obj.GetType() == GetType() && Equals((FileAddress)obj);
            }

            /// <summary>
            /// Returns a hash code for this instance of an address.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = fileIndex.GetHashCode();
                    hashCode = (hashCode * 397) ^ fileSize.GetHashCode();
                    hashCode = (hashCode * 397) ^ fileName.GetHashCode();
                    return hashCode;
                }
            }
        }

        #region Depracated Members
        /// <summary>
        /// Depracataed member
        /// All singleton related features are being marked as depracated and will be removed in future updates.
        /// The recomendation is to use Scriptable Objects, or true static members.
        /// All Heathen systems will be updated such that non-static features (those dependent on instantated data) will be housed in a Scriptable Object any faeture that does not require instantiated data will be staticly accessable.
        /// </summary>
        [Obsolete("Avoid using singleton models, most funcitonality has been moved to be a static funciton where appropreate, remaining funcitonality is availabel via direct API call such as SteamRemoteStorage interface.", false)]
        public static SteamworksRemoteStorageManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    return new GameObject("HeathenSteamCloud").AddComponent<SteamworksRemoteStorageManager>();
                }
                else
                {
                    return s_instance;
                }
            }
        }

        /// <summary>
        /// Depracated member, use SteamRemoteStorage.IsCloudEnabledForAccount
        /// </summary>
        [Obsolete("Use SteamRemoteStorage.IsCloudEnabledForAccount", false)]
        public bool IsCloudEnabledForAccount
        {
            get
            {
                return SteamRemoteStorage.IsCloudEnabledForAccount();
            }
        }

        /// <summary>
        /// Depracated member, use SteamRemoteStorage.IsCloudEnabledForApp 
        /// </summary>
        [Obsolete("Use SteamRemoteStorage.IsCloudEnabledForApp", false)]
        public bool IsCloudEnabledForApp
        {
            get
            {
                return SteamRemoteStorage.IsCloudEnabledForApp();
            }
        }
        #endregion

        public static bool Initalized { get; private set; }
        private static SteamworksRemoteStorageManager s_instance;
        public static List<FileAddress> files = new List<FileAddress>();

        private static CallResult<RemoteStorageFileReadAsyncComplete_t> fileReadAsyncComplete;
#pragma warning disable IDE0052 // Remove unread private members
        private static Callback<RemoteStorageFileShareResult_t> fileShareResult;
#pragma warning restore IDE0052 // Remove unread private members
        private static CallResult<RemoteStorageFileWriteAsyncComplete_t> fileWriteAsyncComplete;

        /// <summary>
        /// Pointers to all available <see cref="SteamDataLibrary"/> objects that have been registered to the system. These represent the structure of unique types of save files.
        /// </summary>
        [Header("Remote Storage")]
        public List<SteamDataLibrary> GameDataModel = new List<SteamDataLibrary>();
        /// <summary>
        /// Occures when a file read operation is compelted.
        /// </summary>
        [Header("Events")]
        public UnityEvent FileReadAsyncComplete;
        /// <summary>
        /// Occures when a file write operation is completed.
        /// </summary>
        public UnityEvent FileWriteAsyncComplete;

        void Start()
        {
            s_instance = this;

            if (!Initalized)
            {
                Initalized = true;
                fileReadAsyncComplete = CallResult<RemoteStorageFileReadAsyncComplete_t>.Create();
                fileShareResult = Callback<RemoteStorageFileShareResult_t>.Create(HandleFileShareResult);
                fileWriteAsyncComplete = CallResult<RemoteStorageFileWriteAsyncComplete_t>.Create();
            }
        }

        #region Event Handlers
        private static void HandleFileWriteAsyncComplete(RemoteStorageFileWriteAsyncComplete_t param, bool bIOFailure)
        {
            if (s_instance != null)
                s_instance.FileWriteAsyncComplete.Invoke();
        }

        private static void HandleFileShareResult(RemoteStorageFileShareResult_t param)
        {
            //TODO: Coming Soon!
        }

        private static void HandleFileReadAsyncComplete(RemoteStorageFileReadAsyncComplete_t param, bool bIOFailure)
        {
            if (s_instance != null)
                s_instance.FileReadAsyncComplete.Invoke();
        }
        #endregion

        /// <summary>
        /// Populates the SteamDataFilesIndex with all files available to this Steam User
        /// </summary>
        public static void RefreshFileList()
        {
            files.Clear();

            if (s_instance != null)
                s_instance.ClearAvailableLibraries();

            var count = SteamRemoteStorage.GetFileCount();
            for (int i = 0; i < count; i++)
            {
                int size;
                var name = SteamRemoteStorage.GetFileNameAndSize(i, out size);
                var timeStamp = SteamRemoteStorage.GetFileTimestamp(name);
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(timeStamp);
                var data = new FileAddress()
                {
                    fileIndex = i,
                    fileName = name,
                    fileSize = size,
                    UtcTimestamp = dateTime
                };
                files.Add(data);

                if (s_instance != null)
                {
                    var lib = s_instance.GetDataModelLibrary(name);
                    if (lib != null)
                    {
                        lib.availableFiles.Add(data);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the data model type library that this file should belong to
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public SteamDataLibrary GetDataModelLibrary(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (GameDataModel.Exists(p => fileName.StartsWith(p.filePrefix)))
                {
                    return GameDataModel.First(p => fileName.StartsWith(p.filePrefix));
                }
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        /// Gets the data model type library that this file should belong to
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public SteamDataLibrary GetDataModelLibrary(FileAddress address)
        {
            return GetDataModelLibrary(address.fileName);
        }

        /// <summary>
        /// Gets the data model type library that this file should belong to
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public SteamDataLibrary GetDataModelLibrary(SteamDataFile file)
        {
            if (file != null)
            {
                return GetDataModelLibrary(file.address.fileName);
            }
            else
                return null;
        }

        private void ClearAvailableLibraries()
        {
            foreach (var lib in GameDataModel)
            {
                lib.availableFiles.Clear();
            }
        }

        // Wrappers around standard Steamworks funcitonality for ease of access and integraiton with Heathen Systems
        #region Steamworks Native
        /// <summary>
        /// Toggles whether the Steam Cloud is enabled for your application.
        /// This setting can be queried with IsCloudEnabledForApp.
        /// </summary>
        /// <remarks>
        /// This must only ever be called as the direct result of the user explicitly requesting that it's enabled or not. This is typically accomplished with a checkbox within your in-game options.
        /// </remarks>
        /// <param name="enable"></param>
        public void SetCloudEnabledForApp(bool enable)
        {
            SteamRemoteStorage.SetCloudEnabledForApp(enable);
        }

        /// <summary>
        /// Allows you to specify which operating systems a file will be synchronized to.
        /// Use this if you have a multiplatform game but have data which is incompatible between platforms.
        /// Files default to k_ERemoteStoragePlatformAll when they are first created.You can use the bitwise OR operator, "|" to specify multiple platforms.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public bool SetSyncPlatforms(SteamDataFile file, ERemoteStoragePlatform platform)
        {
            return SteamRemoteStorage.SetSyncPlatforms(file.address.fileName, platform);
        }

        /// <summary>
        /// Allows you to specify which operating systems a file will be synchronized to.
        /// Use this if you have a multiplatform game but have data which is incompatible between platforms.
        /// Files default to k_ERemoteStoragePlatformAll when they are first created.You can use the bitwise OR operator, "|" to specify multiple platforms.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public bool SetSyncPlatforms(FileAddress address, ERemoteStoragePlatform platform)
        {
            return SteamRemoteStorage.SetSyncPlatforms(address.fileName, platform);
        }

        /// <summary>
        /// Allows you to specify which operating systems a file will be synchronized to.
        /// Use this if you have a multiplatform game but have data which is incompatible between platforms.
        /// Files default to k_ERemoteStoragePlatformAll when they are first created.You can use the bitwise OR operator, "|" to specify multiple platforms.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public bool SetSyncPlatforms(string fileName, ERemoteStoragePlatform platform)
        {
            return SteamRemoteStorage.SetSyncPlatforms(fileName, platform);
        }

        /// <summary>
        /// Returns the UTC timestamp from the Steam Remote Storage system
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>UTC time stamp</returns>
        public static DateTime GetFileTimestamp(string fileName)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            if (SteamRemoteStorage.FileExists(fileName))
            {
                var timeStamp = SteamRemoteStorage.GetFileTimestamp(fileName);
                dateTime.AddSeconds(timeStamp);
            }
            return dateTime;
        }

        /// <summary>
        /// Returns the UTC timestamp from the Steam Remote Storage system
        /// </summary>
        /// <param name="address"></param>
        /// <returns>UTC time stamp</returns>
        public static DateTime GetFileTimestamp(FileAddress address)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            if (SteamRemoteStorage.FileExists(address.fileName))
            {
                var timeStamp = SteamRemoteStorage.GetFileTimestamp(address.fileName);
                dateTime = dateTime.AddSeconds(timeStamp);
                address.UtcTimestamp = dateTime;
            }
            return dateTime;
        }

        /// <summary>
        /// Deletes a file from the local disk, and propagates that delete to the cloud.
        /// This is meant to be used when a user actively deletes a file.Use FileForget if you want to remove a file from the Steam Cloud but retain it on the users local disk.
        /// When a file has been deleted it can be re-written with FileWrite to reupload it to the Steam Cloud
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>true if the file exists and has been successfully deleted; otherwise, false if the file did not exist.</returns>
        public static bool FileDelete(string fileName)
        {
            if (files.Exists(p => p.fileName == fileName))
            {
                var address = files.First(p => p.fileName == fileName);

                files.Remove(address);

                if (s_instance != null)
                {
                    var lib = s_instance.GetDataModelLibrary(address.fileName);
                    if (lib != null)
                        lib.availableFiles.Remove(address);
                }
            }

            return SteamRemoteStorage.FileDelete(fileName);
        }

        /// <summary>
        /// Deletes a file from the local disk, and propagates that delete to the cloud.
        /// This is meant to be used when a user actively deletes a file.Use FileForget if you want to remove a file from the Steam Cloud but retain it on the users local disk.
        /// When a file has been deleted it can be re-written with FileWrite to reupload it to the Steam Cloud
        /// </summary>
        /// <param name="address"></param>
        /// <returns>true if the file exists and has been successfully deleted; otherwise, false if the file did not exist.</returns>
        public static bool FileDelete(FileAddress address)
        {
            files.Remove(address);

            if (s_instance != null)
            {
                var lib = s_instance.GetDataModelLibrary(address.fileName);
                if (lib != null)
                    lib.availableFiles.Remove(address);
            }

            return SteamRemoteStorage.FileDelete(address.fileName);
        }

        /// <summary>
        /// Checks whether the specified file exists.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>true if the file exists; otherwise, false</returns>
        public bool FileExists(string fileName)
        {
            return SteamRemoteStorage.FileExists(fileName);
        }

        /// <summary>
        /// Checks whether the specified file exists.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>true if the file exists; otherwise, false</returns>
        public bool FileExists(FileAddress address)
        {
            return SteamRemoteStorage.FileExists(address.fileName);
        }

        /// <summary>
        /// Starts an asynchronous read from a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>A data file containing the binary result of the read</returns>
        public static SteamDataFile FileReadSteamDataFile(string fileName)
        {
            var address = new FileAddress()
            {
                fileIndex = -1,
                fileName = fileName
            };

            if (files.Exists(p => p.fileName == fileName))
                address = files.First(p => p.fileName == fileName);
            else
            {
                GetFileTimestamp(address);
            }

            var buffer = new byte[address.fileSize];
            SteamRemoteStorage.FileRead(address.fileName, buffer, buffer.Length);
            var data = new SteamDataFile()
            {
                address = address,
                binaryData = buffer,
                apiCall = null,
                result = EResult.k_EResultOK
            };

            return data;
        }

        /// <summary>
        /// Reads the data from the file as text
        /// </summary>
        /// <param name="fileName">The name of the file to load</param>
        /// <param name="encoding">The text encoding of the file ... typeically this will be System.TExt.Encoding.UTF8</param>
        /// <returns></returns>
        public static string FileReadString(string fileName, System.Text.Encoding encoding)
        {
            var address = new FileAddress()
            {
                fileIndex = -1,
                fileName = fileName
            };

            if (files.Exists(p => p.fileName == fileName))
                address = files.First(p => p.fileName == fileName);
            else
            {
                GetFileTimestamp(address);
            }

            var buffer = new byte[address.fileSize];
            SteamRemoteStorage.FileRead(address.fileName, buffer, buffer.Length);
            return encoding.GetString(buffer);
        }

        /// <summary>
        /// Reads the data from the file as a JSON object
        /// </summary>
        /// <typeparam name="T">The object type that should be deserialized from the file's JSON string</typeparam>
        /// <param name="fileName">the name of the file to load</param>
        /// <param name="encoding">the text encoding of the file ... typically this will be System.Text.Encoding.UTF8</param>
        /// <returns></returns>
        public static T FileReadJson<T>(string fileName, System.Text.Encoding encoding)
        {
            var address = new FileAddress()
            {
                fileIndex = -1,
                fileName = fileName
            };

            if (files.Exists(p => p.fileName == fileName))
                address = files.First(p => p.fileName == fileName);
            else
            {
                GetFileTimestamp(address);
            }

            var buffer = new byte[address.fileSize];
            SteamRemoteStorage.FileRead(address.fileName, buffer, buffer.Length);
            var JsonString = encoding.GetString(buffer);

            return JsonUtility.FromJson<T>(JsonString);
        }

        /// <summary>
        /// Reads the raw data from Valve's servers for this file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[] FileReadData(string fileName)
        {
            var size = SteamRemoteStorage.GetFileSize(fileName);

            var buffer = new byte[size];
            SteamRemoteStorage.FileRead(fileName, buffer, size);

            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static SteamDataFile FileReadSteamDataFile(FileAddress address)
        {
            var buffer = new byte[address.fileSize];
            SteamRemoteStorage.FileRead(address.fileName, buffer, buffer.Length);
            var data = new SteamDataFile()
            {
                address = address,
                binaryData = buffer,
                apiCall = null,
                result = EResult.k_EResultOK
            };

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string FileReadString(FileAddress address, System.Text.Encoding encoding)
        {
            var buffer = new byte[address.fileSize];
            SteamRemoteStorage.FileRead(address.fileName, buffer, buffer.Length);

            return encoding.GetString(buffer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static T FileReadJson<T>(FileAddress address, System.Text.Encoding encoding)
        {
            var buffer = new byte[address.fileSize];
            SteamRemoteStorage.FileRead(address.fileName, buffer, buffer.Length);

            return JsonUtility.FromJson<T>(encoding.GetString(buffer));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static byte[] FileReadData(FileAddress address)
        {
            var buffer = new byte[address.fileSize];
            SteamRemoteStorage.FileRead(address.fileName, buffer, buffer.Length);

            return buffer;
        }

        /// <summary>
        /// Starts an asynchronious file read
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>SteamDataFile objects can handle asynchronious data reads either by testing the object when the <see cref="SteamworksRemoteStorageManager.FileReadAsyncComplete"/> is invoked or by assigning an action to the <see cref="SteamDataFile.Complete"/> member</returns>
        /// <example>
        /// <para>
        /// Demonstrates assigning a handler to the file read process such that when the process completes the desired code is ran.
        /// This example will cause the system to print the content of the file as an ASCII string when the load has completed.
        /// </para>
        /// <code>
        /// SteamworksReamoteStorageManager.FileReadAsync("The Files Name Goes Here").Complete = result => { Debug.Log(result.FromASCII()); };
        /// </code>
        /// </example>
        public static SteamDataFile FileReadAsync(string fileName)
        {
            var address = new FileAddress()
            {
                fileIndex = -1,
                fileName = fileName
            };

            if (files.Exists(p => p.fileName == fileName))
                address = files.First(p => p.fileName == fileName);
            else
            {
                GetFileTimestamp(address);
            }

            var data = new SteamDataFile()
            {
                address = address
            };
            data.apiCall = SteamRemoteStorage.FileReadAsync(address.fileName, 0, (uint)address.fileSize);
            fileReadAsyncComplete.Set(data.apiCall.Value, (p, f) => { data.HandleFileReadAsyncComplete(p, f); HandleFileReadAsyncComplete(p, f); });
            return data;
        }

        /// <summary>
        /// Starts an asynchronious file read
        /// </summary>
        /// <param name="address"></param>
        /// <returns>SteamDataFile objects can handle asynchronious data reads either by testing the object when the <see cref="SteamworksRemoteStorageManager.FileReadAsyncComplete"/> is invoked or by assigning an action to the <see cref="SteamDataFile.Complete"/> member</returns>
        /// <example>
        /// <para>
        /// Demonstrates assigning a handler to the file read process such that when the process completes the desired code is ran.
        /// This example will cause the system to print the content of the file as an ASCII string when the load has completed.
        /// </para>
        /// <code>
        /// SteamworksReamoteStorageManager.FileReadAsync(myFileAddress).Complete = result => { Debug.Log(result.FromASCII()); };
        /// </code>
        /// </example>
        public static SteamDataFile FileReadAsync(FileAddress address)
        {
            var data = new SteamDataFile()
            {
                address = address
            };
            data.apiCall = SteamRemoteStorage.FileReadAsync(address.fileName, 0, (uint)address.fileSize);
            fileReadAsyncComplete.Set(data.apiCall.Value, (p, f) => { data.HandleFileReadAsyncComplete(p, f); HandleFileReadAsyncComplete(p, f); });
            return data;
        }

        /// <summary>
        /// Deletes the file from remote storage, but leaves it on the local disk and remains accessible from the API.
        /// </summary>
        /// <remarks>
        /// When you are out of Cloud space, this can be used to allow calls to FileWrite to keep working without needing to make the user delete files.
        /// How you decide which files to forget are up to you.It could be a simple Least Recently Used(LRU) queue or something more complicated.
        /// Requiring the user to manage their Cloud-ized files for a game, while is possible to do, it is never recommended.For instance, "Which file would you like to delete so that you may store this new one?" removes a significant advantage of using the Cloud in the first place: its transparency.
        /// Once a file has been deleted or forgotten, calling FileWrite will resynchronize it in the Cloud. Rewriting a forgotten file is the only way to make it persisted again.
        /// </remarks>
        /// <param name="fileName"></param>
        /// <returns>true if the file exists and has been successfully forgotten; otherwise, false.</returns>
        public bool FileForget(string fileName)
        {
            return SteamRemoteStorage.FileForget(fileName);
        }

        /// <summary>
        /// Deletes the file from remote storage, but leaves it on the local disk and remains accessible from the API.
        /// </summary>
        /// <remarks>
        /// When you are out of Cloud space, this can be used to allow calls to FileWrite to keep working without needing to make the user delete files.
        /// How you decide which files to forget are up to you.It could be a simple Least Recently Used(LRU) queue or something more complicated.
        /// Requiring the user to manage their Cloud-ized files for a game, while is possible to do, it is never recommended.For instance, "Which file would you like to delete so that you may store this new one?" removes a significant advantage of using the Cloud in the first place: its transparency.
        /// Once a file has been deleted or forgotten, calling FileWrite will resynchronize it in the Cloud. Rewriting a forgotten file is the only way to make it persisted again.
        /// </remarks>
        /// <param name="address"></param>
        /// <returns>true if the file exists and has been successfully forgotten; otherwise, false.</returns>
        public bool FileForget(FileAddress address)
        {
            return SteamRemoteStorage.FileForget(address.fileName);
        }

        /// <summary>
        /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// May return false under the following conditions:
        /// The file you're trying to write is larger than 100MiB as defined by k_unMaxCloudFileChunkSize.
        /// cubData is less than 0.
        /// pvData is NULL.
        /// You tried to write to an invalid path or filename.Because Steam Cloud is cross platform the files need to have valid names on all supported OSes and file systems. See Microsoft's documentation on Naming Files, Paths, and Namespaces.
        /// The current user's Steam Cloud storage quota has been exceeded. They may have run out of space, or have too many files.
        /// Steam could not write to the disk, the location might be read-only.
        /// </remarks>
        /// <param name="file"></param>
        /// <returns>true if the write was successful. Otherwise, false.
        /// </returns>
        public static bool FileWrite(SteamDataFile file)
        {
            if (file != null && file.binaryData.Length > 0 && !string.IsNullOrEmpty(file.address.fileName))
            {
                //Test for linked library and refresh as required
                if (file.linkedLibrary != null)
                    file.ReadFromLibrary(file.linkedLibrary);

                if (SteamRemoteStorage.FileWrite(file.address.fileName, file.binaryData, file.binaryData.Length))
                {
                    file.address.UtcTimestamp = GetFileTimestamp(file.address);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                //Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (file.binaryData.Length < 0 ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(file.address.fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }

        /// <summary>
        /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// May return false under the following conditions:
        /// The file you're trying to write is larger than 100MiB as defined by k_unMaxCloudFileChunkSize.
        /// cubData is less than 0.
        /// pvData is NULL.
        /// You tried to write to an invalid path or filename.Because Steam Cloud is cross platform the files need to have valid names on all supported OSes and file systems. See Microsoft's documentation on Naming Files, Paths, and Namespaces.
        /// The current user's Steam Cloud storage quota has been exceeded. They may have run out of space, or have too many files.
        /// Steam could not write to the disk, the location might be read-only.
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns>true if the write was successful. Otherwise, false.
        /// </returns>
        public static bool FileWrite(string fileName, byte[] data)
        {
            if (data.Length > 0 && !string.IsNullOrEmpty(fileName))
            {
                //See if file has an existing indexed address, if not create one
                if (!files.Exists(p => p.fileName == fileName))
                {
                    var address = new FileAddress()
                    {
                        fileIndex = -1,
                        fileName = fileName,
                        fileSize = data.Length,
                        UtcTimestamp = DateTime.UtcNow
                    };

                    files.Add(address);
                }

                var result = SteamRemoteStorage.FileWrite(fileName, data, data.Length);
                if (!result)
                {
                    SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
                    if (available < Convert.ToUInt64(data.Length))
                    {
                        Debug.LogWarning("Insufficent storage space available on the Steam Remote Storage target.");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... Please consult your Steamworks documentaiton regarding Steam Remote Storage.");
                    }
                }
                else
                {
                    Debug.Log("File " + fileName + " saved to Steam Remote Storage.");
                }

                return result;
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (data.Length < 0 ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }

        /// <summary>
        /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// May return false under the following conditions:
        /// The file you're trying to write is larger than 100MiB as defined by k_unMaxCloudFileChunkSize.
        /// cubData is less than 0.
        /// pvData is NULL.
        /// You tried to write to an invalid path or filename.Because Steam Cloud is cross platform the files need to have valid names on all supported OSes and file systems. See Microsoft's documentation on Naming Files, Paths, and Namespaces.
        /// The current user's Steam Cloud storage quota has been exceeded. They may have run out of space, or have too many files.
        /// Steam could not write to the disk, the location might be read-only.
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="body">The text to encode and save to the Valve Remote Storage servers</param>
        /// <param name="encoding">The text encoding to use ... usually System.Text.Encoding.UTF8</param>
        /// <returns>true if the write was successful. Otherwise, false.
        /// </returns>
        public static bool FileWrite(string fileName, string body, System.Text.Encoding encoding)
        {
            var data = encoding.GetBytes(body);

            if (data.Length > 0 && !string.IsNullOrEmpty(fileName))
            {
                //See if file has an existing indexed address, if not create one
                if (!files.Exists(p => p.fileName == fileName))
                {
                    var address = new FileAddress()
                    {
                        fileIndex = -1,
                        fileName = fileName,
                        fileSize = data.Length,
                        UtcTimestamp = DateTime.UtcNow
                    };

                    files.Add(address);
                }

                var result = SteamRemoteStorage.FileWrite(fileName, data, data.Length);
                if (!result)
                {
                    SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
                    if (available < Convert.ToUInt64(data.Length))
                    {
                        Debug.LogWarning("Insufficent storage space available on the Steam Remote Storage target.");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... Please consult your Steamworks documentaiton regarding Steam Remote Storage.");
                    }
                }
                else
                {
                    Debug.Log("File " + fileName + " saved to Steam Remote Storage.");
                }

                return result;
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (data.Length < 0 ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }

        /// <summary>
        /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// May return false under the following conditions:
        /// The file you're trying to write is larger than 100MiB as defined by k_unMaxCloudFileChunkSize.
        /// cubData is less than 0.
        /// pvData is NULL.
        /// You tried to write to an invalid path or filename.Because Steam Cloud is cross platform the files need to have valid names on all supported OSes and file systems. See Microsoft's documentation on Naming Files, Paths, and Namespaces.
        /// The current user's Steam Cloud storage quota has been exceeded. They may have run out of space, or have too many files.
        /// Steam could not write to the disk, the location might be read-only.
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="JsonObject">the object to be serialized to a JSON string and saved to the target file. Any type that the UnityEngine.JsonUtility can handle can be used.</param>
        /// <param name="encoding">The text encoding to use ... usually System.Text.Encoding.UTF8</param>
        /// <returns>true if the write was successful. Otherwise, false.
        /// </returns>
        public static bool FileWrite(string fileName, object JsonObject, System.Text.Encoding encoding)
        {
            return FileWrite(fileName, JsonUtility.ToJson(JsonObject), encoding);
        }

        /// <summary>
        /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// May return false under the following conditions:
        /// The file you're trying to write is larger than 100MiB as defined by k_unMaxCloudFileChunkSize.
        /// cubData is less than 0.
        /// pvData is NULL.
        /// You tried to write to an invalid path or filename.Because Steam Cloud is cross platform the files need to have valid names on all supported OSes and file systems. See Microsoft's documentation on Naming Files, Paths, and Namespaces.
        /// The current user's Steam Cloud storage quota has been exceeded. They may have run out of space, or have too many files.
        /// Steam could not write to the disk, the location might be read-only.
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="lib">The data library containing the data to be saved, this will be matched to a Game Data Model entry and if found a Save Data File will be generated and linked</param>
        /// <returns>true if the write was successful. Otherwise, false. 
        /// See https://partner.steamgames.com/doc/api/ISteamRemoteStorage#FileWrite for more information
        /// </returns>
        public static bool FileWrite(string fileName, SteamDataLibrary lib)
        {
            if (lib != null && !string.IsNullOrEmpty(fileName))
            {
                //Test for a data model link
                if (s_instance != null && s_instance.GameDataModel.Exists(p => p == lib))
                {
                    var address = new FileAddress();

                    //Test for an existing address with this name
                    if (files.Exists(p => p.fileName == fileName))
                        address = files.First(p => p.fileName == fileName);
                    else
                    {
                        address.fileIndex = -1;
                        address.fileName = fileName;
                        address.UtcTimestamp = DateTime.UtcNow;
                        files.Add(address);
                        lib.availableFiles.Add(address);
                    }

                    SteamDataFile file = new SteamDataFile()
                    {
                        address = address,
                        linkedLibrary = lib,
                        result = EResult.k_EResultOK
                    };

                    file.ReadFromLibrary(file.linkedLibrary);

                    lib.activeFile = file;

                    lib.SyncToBuffer(out file.binaryData);

                    var result = SteamRemoteStorage.FileWrite(fileName, file.binaryData, file.binaryData.Length);
                    if (!result)
                    {
                        SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
                        if (available < Convert.ToUInt64(file.binaryData.Length))
                        {
                            Debug.LogWarning("Insufficent storage space available on the Steam Remote Storage target.");
                        }
                        else
                        {
                            Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... Please consult your Steamworks documentaiton regarding Steam Remote Storage.");
                        }
                    }
                    else
                    {
                        Debug.Log("File " + fileName + " saved to Steam Remote Storage.");
                    }

                    return result;
                }
                else
                {
                    //Not linked to a model so just save it and move on
                    byte[] data;
                    lib.SyncToBuffer(out data);
                    var result = SteamRemoteStorage.FileWrite(fileName, data, data.Length);
                    if (!result)
                    {
                        SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
                        if (available < Convert.ToUInt64(data.Length))
                        {
                            Debug.LogWarning("Insufficent storage space available on the Steam Remote Storage target.");
                        }
                        else
                        {
                            Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... Please consult your Steamworks documentaiton regarding Steam Remote Storage.");
                        }
                    }
                    else
                    {
                        Debug.Log("File " + fileName + " saved to Steam Remote Storage.");
                    }

                    return result;
                }
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (lib == null ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }

        /// <summary>
        /// Creates a new file and asynchronously writes the raw byte data to the Steam Cloud, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// Note that this will trigger a RemoteStorageFileWriteAsyncComplete_t callback indicating the result state however there is no good way to link this with a specific file write request
        /// </remarks>
        /// <param name="file"></param>
        /// <returns>true if the call was issued succesfuly, false otherwise</returns>
        public static SteamDataFile FileWriteAsync(SteamDataFile file)
        {
            if (file != null && file.binaryData.Length > 0 && !string.IsNullOrEmpty(file.address.fileName))
            {
                var nDataFile = new SteamDataFile
                {
                    address = file.address,
                    binaryData = new List<byte>(file.binaryData).ToArray(),
                    linkedLibrary = file.linkedLibrary,
                };

                //Test for linked library and refresh as required
                if (nDataFile.linkedLibrary != null)
                    nDataFile.ReadFromLibrary(nDataFile.linkedLibrary);

                nDataFile.apiCall = SteamRemoteStorage.FileWriteAsync(nDataFile.address.fileName, nDataFile.binaryData, (uint)nDataFile.binaryData.Length);
                fileWriteAsyncComplete.Set(nDataFile.apiCall.Value, (p, f) => { nDataFile.HandleFileWriteAsyncComplete(p, f); HandleFileWriteAsyncComplete(p, f); });
                return nDataFile;
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (file.binaryData.Length < 0 ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(file.address.fileName) ? "You did not provide a valid file name! " : ""));
                return new SteamDataFile { result = EResult.k_EResultFail };
            }
        }

        /// <summary>
        /// Creates a new file and asynchronously writes the raw byte data to the Steam Cloud, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// Note that this will trigger a RemoteStorageFileWriteAsyncComplete_t callback indicating the result state however there is no good way to link this with a specific file write request
        /// </remarks>
        /// <param name="jsonObject"></param>
        /// <param name="encoding"></param>
        /// <param name="file"></param>
        /// <returns>true if the call was issued succesfuly, false otherwise</returns>
        public static SteamDataFile FileWriteAsync(string fileName, object jsonObject, System.Text.Encoding encoding)
        {
            var file = new SteamDataFile { address = new FileAddress { fileName = fileName } };
            file.SetDataFromObject(jsonObject, encoding);
            return FileWriteAsync(file);
        }

        /// <summary>
        /// Creates a new file and asynchronously writes the raw byte data to the Steam Cloud, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// Note that this will trigger a RemoteStorageFileWriteAsyncComplete_t callback indicating the result state however there is no good way to link this with a specific file write request
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns>true if the call was issued succesfuly, false otherwise</returns>
        public static SteamDataFile FileWriteAsync(string fileName, byte[] data)
        {
            var file = new SteamDataFile { address = new FileAddress { fileName = fileName }, binaryData = data };
            return FileWriteAsync(file);
        }

        /// <summary>
        /// Creates a new file and asynchronously writes the raw byte data to the Steam Cloud, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// Note that this will trigger a RemoteStorageFileWriteAsyncComplete_t callback indicating the result state however there is no good way to link this with a specific file write request
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="library">The data library containing the data to be saved, this will be matched to a Game Data Model entry and if found a Save Data File will be generated and linked</param>
        /// <returns>true if the call was issued succesfuly, false otherwise</returns>
        public static SteamDataFile FileWriteAsync(string fileName, SteamDataLibrary lib)
        {
            if (lib != null && !string.IsNullOrEmpty(fileName))
            {
                var address = new FileAddress();

                //Test for an existing address with this name
                if (files.Exists(p => p.fileName == fileName))
                    address = files.First(p => p.fileName == fileName);
                else
                {
                    address.fileIndex = -1;
                    address.fileName = fileName;
                    address.UtcTimestamp = DateTime.UtcNow;
                    files.Add(address);
                    lib.availableFiles.Add(address);
                }

                SteamDataFile file = new SteamDataFile()
                {
                    address = address,
                    linkedLibrary = lib,
                    result = EResult.k_EResultOK
                };

                lib.activeFile = file;

                lib.SyncToBuffer(out file.binaryData);
                file.apiCall = SteamRemoteStorage.FileWriteAsync(fileName, file.binaryData, (uint)file.binaryData.Length);
                fileWriteAsyncComplete.Set(file.apiCall.Value, (p, f) => { file.HandleFileWriteAsyncComplete(p, f); HandleFileWriteAsyncComplete(p, f); });
                return file;
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (lib == null ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(fileName) ? "You did not provide a valid file name! " : ""));
                return new SteamDataFile { result = EResult.k_EResultFail };
            }
        }
        #endregion
    }
}
#endif
