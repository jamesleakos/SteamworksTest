#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Scriptable;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static HeathenEngineering.SteamApi.PlayerServices.SteamworksRemoteStorageManager;

namespace HeathenEngineering.SteamApi.PlayerServices.UI
{
    /// <summary>
    /// Displays a list of <see cref="SteamDataFile"/> entries related to a specific <see cref="SteamDataLibrary"/>.
    /// </summary>
    public class SteamDataFileList : MonoBehaviour
    {
        /// <summary>
        /// <para>The library the list will map files for.</para>   
        /// <para>Note that this list will sort and display only files related to this library as determined by the prefix defined in the library.</para>
        /// </summary>
        public SteamDataLibrary Library;
        /// <summary>
        /// <para>The prefab used to display <see cref="SteamDataFile"/> records to the UI</para>
        /// </summary>
        public SteamDataFileRecord RecordPrefab;
        /// <summary>
        /// The container which newly spawned instances of the <see cref="SteamDataFileList.RecordPrefab"/> will be parented to.
        /// </summary>
        public RectTransform Container;
        /// <summary>
        /// If true then the name display used for each file will remove the prefix from the file name e.g. system_MyFile.dat becomes MyFile.dat assumign a prefix of system_
        /// </summary>
        public BoolReference RemovePrefix = new BoolReference(true);
        /// <summary>
        /// The display format of the files date time ... see C# .NET DateTime ToString formating options for more information.
        /// </summary>
        public StringReference DateDisplayFormat = new StringReference("F");
        /// <summary>
        /// This event is raised when a new <see cref="SteamDataFile"/> object is selected in the UI.
        /// </summary>
        [Header("Events")]
        public UnityEvent SelectionChanged;
        /// <summary>
        /// A pointer to the currently selected <see cref="SteamDataFile"/>.
        /// </summary>
        public SteamDataFile Active
        {
            get
            {
                return Library.activeFile;
            }
        }
        private FileAddress? s_SelectedFile;
        /// <summary>
        /// The address if any of the currenltly selected <see cref="SteamDataFile"/> object.
        /// </summary>
        public FileAddress? SelectedFile
        {
            get
            {
                return s_SelectedFile;
            }
            set
            {
                if(s_SelectedFile.HasValue && value.HasValue)
                {
                    if (s_SelectedFile.Value != value.Value)
                    {
                        s_SelectedFile = value;
                        SelectionChanged.Invoke();
                    }
                }
                else if (s_SelectedFile.HasValue != value.HasValue)
                {
                    s_SelectedFile = value;
                    SelectionChanged.Invoke();
                }
            }
        }

        /// <summary>
        /// Updates the list from the library values sorted on the time stamp of the record
        /// </summary>
        public void Refresh()
        {
            RefreshFileList();
            var temp = new List<GameObject>();
            foreach(Transform child in Container)
            {
                temp.Add(child.gameObject);
            }

            while(temp.Count > 0)
            {
                var t = temp[0];
                temp.Remove(t);
                Destroy(t);
            }

            Library.availableFiles.Sort((p1, p2) => { return p1.UtcTimestamp.CompareTo(p2.UtcTimestamp); });
            Library.availableFiles.Reverse();

            foreach (var address in Library.availableFiles)
            {
                var go = Instantiate(RecordPrefab.gameObject, Container);
                var r = go.GetComponent<SteamDataFileRecord>();
                r.parentList = this;
                r.Address = address;
                if (RemovePrefix.Value && address.fileName.StartsWith(Library.filePrefix))
                    r.FileName.text = address.fileName.Substring(Library.filePrefix.Length);
                else
                    r.FileName.text = address.fileName;
                r.Timestamp.text = address.LocalTimestamp.ToString(DateDisplayFormat, Thread.CurrentThread.CurrentCulture);
            }
        }

        /// <summary>
        /// Returns the address if any of the most resent file saved.
        /// </summary>
        /// <returns></returns>
        public FileAddress? GetLatest()
        {
            if (Library.availableFiles.Count > 0)
                return Library.availableFiles[0];
            else
                return null;
        }

        /// <summary>
        /// Clears the selected file pointer.
        /// </summary>
        public void ClearSelected()
        {
            SelectedFile = null;
        }

        /// <summary>
        /// Selectes a specific file by address
        /// </summary>
        /// <param name="address"></param>
        public void Select(FileAddress address)
        {
            SelectedFile = address;
        }

        /// <summary>
        /// Selects the most resent file.
        /// </summary>
        public void SelectLatest()
        {
            SelectedFile = GetLatest();
        }

        /// <summary>
        /// Loads the selected file ... this will deserialize the data of the file and populate the fileds of the related <see cref="SteamDataLibrary"/>
        /// </summary>
        public void LoadSelected()
        {
            if (SelectedFile.HasValue)
                Library.Load(SelectedFile.Value);
        }

        /// <summary>
        /// Loads the selected file ... this will deserialize the data of the file and populate the fileds of the related <see cref="SteamDataLibrary"/>
        /// </summary>
        public void LoadSelectedAsync()
        {
            if (SelectedFile.HasValue)
                Library.LoadAsync(SelectedFile.Value);
        }

        /// <summary>
        /// Removes the selected file from the Steam Remote Storage system
        /// </summary>
        public void DeleteSelected()
        {
            if (SelectedFile.HasValue)
                FileDelete(SelectedFile.Value);

            Refresh();
        }

        /// <summary>
        /// Instructs the Steam Remote Storage system to 'forget' the file e.g. to purge it from the cloud and no longer sync it.
        /// </summary>
        public void ForgetSelected()
        {
            if (SelectedFile.HasValue)
                SteamRemoteStorage.FileForget(SelectedFile.Value.fileName);
        }

        /// <summary>
        /// Saves the selected file if any... This will attempt to serialize the related libraries data and store it to the Steam Remote Storage system to the address of the currently selected file.
        /// </summary>
        public void SaveActive()
        {
            if (SelectedFile.HasValue)
            {
                Library.Save();
                Refresh();
            }
            else
            {
                Debug.LogWarning("[SteamDataFileList.SaveActive] Attempted to save the active file but no file is active.");
            }
        }

        /// <summary>
        /// This saves the data of the related library to the Steam Remote Storage system with the indicated name... Note that the prifix as defined in the library will be added if missing.
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveAs(string fileName)
        {
            Library.SaveAs(fileName);
            Refresh();

            SelectLatest();
        }

        /// <summary>
        /// This saves the data of the related library to the Steam Remote Storage system with the indicated name... Note that the prifix as defined in the library will be added if missing.
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveAs(InputField fileName)
        {
            if (fileName == null || string.IsNullOrEmpty(fileName.text))
            {
                Debug.LogWarning("[SteamDataFileList.SaveAs] Attempted to SaveAs but was not provided with a file name ... will attempt to save the active file instead.");
                SaveActive();
            }
            else
            {
                Library.SaveAs(fileName.text);
                Refresh();

                SelectLatest();
            }
        }

        /// <summary>
        /// Saves the selected file if any... This will attempt to serialize the related libraries data and store it to the Steam Remote Storage system to the address of the currently selected file.
        /// </summary>
        public void SaveActiveAsync()
        {
            if (SelectedFile.HasValue)
                Library.SaveAsync();
        }

        /// <summary>
        /// This saves the data of the related library to the Steam Remote Storage system with the indicated name... Note that the prifix as defined in the library will be added if missing.
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveAsAsync(string fileName)
        {
            Library.SaveAsAsync(fileName);

            string fName = fileName.StartsWith(Library.filePrefix) ? fileName : Library.filePrefix + fileName;

            if (Library.availableFiles.Exists(p => p.fileName == fName))
                SelectedFile = Library.availableFiles.First(p => p.fileName == fName);
        }

        /// <summary>
        /// This saves the data of the related library to the Steam Remote Storage system with the indicated name... Note that the prifix as defined in the library will be added if missing.
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveAsAsync(InputField fileName)
        {
            Library.SaveAsAsync(fileName.text);

            string fName = fileName.text.StartsWith(Library.filePrefix) ? fileName.text : Library.filePrefix + fileName.text;

            if (Library.availableFiles.Exists(p => p.fileName == fName))
                SelectedFile = Library.availableFiles.First(p => p.fileName == fName);
        }
    }
}
#endif