#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using System.IO;
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using UnityEngine;

namespace HeathenEngineering.SteamApi.GameServices
{
    /// <summary>
    /// Handler for Workshop items
    /// This object can be used to simplify the create, update, delete and display of Workshop Items
    /// </summary>
    /// <example>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <para>This example is shown in a class however it would be more common to add this funcitonality to a behaviour script on a UI component.</para>
    /// </description>
    /// <code>
    ///public class ExampleSteamworksWOrkshopItemEditorToolCreate
    ///    {
    ///        public SteamSettings settings;
    ///
    ///    public void Foo()
    ///    {
    ///        var ItemEditor = new SteamworksWorkshopItemEditorTool(settings.ApplicationId)
    ///        {
    ///            Author = settings.UserData,
    ///            Title = "My first workshop item!",
    ///            Description = "An example Steam Workshop item aka UGC item.",
    ///            ContentLocation = "This should be a **folder path** to the location where my content is stored",
    ///            PreviewImageLocation = "This should be a **file path** to the location where my preview image is stored",
    ///            Visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic,
    ///            FileType = EWorkshopFileType.k_EWorkshopFileTypeCommunity,
    ///            Tags = new List<string>(new string[] { "Tag #1", "Tag #2", "Tag #3" })
    ///        };
    ///
    ///        ItemEditor.Created.AddListener(HandleCreatedCallback);
    ///        ItemEditor.CreateFailed.AddListener(HandleCreatFailedCallback);
    ///        ItemEditor.Updated.AddListener(HandleUpdatedCallback);
    ///        ItemEditor.UpdateFailed.AddListener(HandleUpdateFailedCallback);
    ///
    ///        ItemEditor.CreateAndUpdate("This is a change note ... and in this step we will create a new file ID and then update it with the data provided.");
    ///    }
    ///
    ///    private void HandleUpdatedCallback(SubmitItemUpdateResult_t result)
    ///    {
    ///        if (result.m_eResult == EResult.k_EResultOK)
    ///        {
    ///            Debug.Log("The file " + result.m_nPublishedFileId.m_PublishedFileId + " has been updated ... the data is now available on Steam Workshop!");
    ///
    ///            if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
    ///            {
    ///                Debug.Log("THIS IS IMPORTANT...");
    ///                Debug.Log("Your user needs to accept the Workshop Legal Agreement ... so pop the overlay and have them do that.");
    ///                settings.Overlay.OpenWebPage("steam://url/CommunityFilePage/" + result.m_nPublishedFileId);
    ///            }
    ///        }
    ///        else
    ///        {
    ///            Debug.Log("This should never happen.");
    ///        }
    ///    }
    ///
    ///    private void HandleUpdateFailedCallback(SubmitItemUpdateResult_t result)
    ///    {
    ///        if (result.m_eResult != EResult.k_EResultOK)
    ///        {
    ///            Debug.Log("The file " + result.m_nPublishedFileId.m_PublishedFileId + " failed to update ... the specific EResult you got back will say why!");
    ///
    ///            switch (result.m_eResult)
    ///            {
    ///                case EResult.k_EResultOK:
    ///                    Debug.Log("Should never happen as results that return OK call back the Created not CreateFailed event.");
    ///                    break;
    ///                case EResult.k_EResultFail:
    ///                    Debug.Log("Generic fialure.");
    ///                    break;
    ///                case EResult.k_EResultInvalidParam:
    ///                    Debug.Log("Either the provided app ID is invalid or doesn't match the consumer app ID of the item or, you have not enabled ISteamUGC for the provided app ID on the Steam Workshop Configuration App Admin page. The preview file is smaller than 16 bytes.");
    ///                    break;
    ///                case EResult.k_EResultAccessDenied:
    ///                    Debug.Log("The user doesn't own a license for the provided app ID.");
    ///                    break;
    ///                case EResult.k_EResultFileNotFound:
    ///                    Debug.Log("Failed to get the workshop info for the item or failed to read the preview file. or the content folder is not valid");
    ///                    break;
    ///                case EResult.k_EResultLockingFailed:
    ///                    Debug.Log("Failed to aquire UGC Lock.");
    ///                    break;
    ///                case EResult.k_EResultLimitExceeded:
    ///                    Debug.Log("The preview image is too large, it must be less than 1 Megabyte; or there is not enough space available on the users Steam Cloud.");
    ///                    break;
    ///                default:
    ///                    Debug.Log("For all other EResult types its not supposed to return ... if it does something new changed on the Steam API backend ... you'll have to read up on that to know whats going on.");
    ///                    break;
    ///            }
    ///        }
    ///        else
    ///        {
    ///            Debug.Log("This should never happen.");
    ///        }
    ///    }
    ///
    ///    public void HandleCreatedCallback(CreateItemResult_t result)
    ///    {
    ///        if (result.m_eResult == EResult.k_EResultOK)
    ///        {
    ///            Debug.Log("The file " + result.m_nPublishedFileId.m_PublishedFileId + " has been created ... the data isn't uploaded yet though!");
    ///
    ///            if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
    ///            {
    ///                Debug.Log("THIS IS IMPORTANT...");
    ///                Debug.Log("Your user needs to accept the Workshop Legal Agreement ... so pop the overlay and have them do that.");
    ///                settings.Overlay.OpenWebPage("steam://url/CommunityFilePage/" + result.m_nPublishedFileId);
    ///            }
    ///        }
    ///        else
    ///        {
    ///            Debug.Log("This should never happen.");
    ///        }
    ///    }
    ///
    ///    public void HandleCreatFailedCallback(CreateItemResult_t result)
    ///    {
    ///        if (result.m_eResult != EResult.k_EResultOK)
    ///        {
    ///            Debug.Log("The file " + result.m_nPublishedFileId.m_PublishedFileId + " failed to create ... the specific EResult you got back will say why!");
    ///
    ///            switch (result.m_eResult)
    ///            {
    ///                case EResult.k_EResultOK:
    ///                    Debug.Log("Should never happen as results that return OK call back the Created not CreateFailed event.");
    ///                    break;
    ///                case EResult.k_EResultInsufficientPrivilege:
    ///                    Debug.Log(" The user is currently restricted from uploading content due to a hub ban, account lock, or community ban. They would need to contact Steam Support.");
    ///                    break;
    ///                case EResult.k_EResultBanned:
    ///                    Debug.Log("The user doesn't have permission to upload content to this hub because they have an active VAC or Game ban.");
    ///                    break;
    ///                case EResult.k_EResultTimeout:
    ///                    Debug.Log("The operation took longer than expected. Have the user retry the creation process.");
    ///                    break;
    ///                case EResult.k_EResultNotLoggedOn:
    ///                    Debug.Log("The user is not currently logged into Steam.");
    ///                    break;
    ///                case EResult.k_EResultServiceUnavailable:
    ///                    Debug.Log("The workshop server hosting the content is having issues - have the user retry.");
    ///                    break;
    ///                case EResult.k_EResultInvalidParam:
    ///                    Debug.Log("One of the submission fields contains something not being accepted by that field.\nNo Steam doens't tell us which one :)");
    ///                    break;
    ///                case EResult.k_EResultAccessDenied:
    ///                    Debug.Log("There was a problem trying to save the title and description. Access was denied.");
    ///                    break;
    ///                case EResult.k_EResultLimitExceeded:
    ///                    Debug.Log("The user has exceeded their Steam Cloud quota. Have them remove some items and try again.");
    ///                    break;
    ///                case EResult.k_EResultFileNotFound:
    ///                    Debug.Log("The uploaded file could not be found. This can also happen if you use a file name not a folder name in the content location ... content is a FOLDER path not a file.");
    ///                    break;
    ///                case EResult.k_EResultDuplicateRequest:
    ///                    Debug.Log("The file was already successfully uploaded. The user just needs to refresh.");
    ///                    break;
    ///                case EResult.k_EResultDuplicateName:
    ///                    Debug.Log("The user already has a Steam Workshop item with that name.");
    ///                    break;
    ///                case EResult.k_EResultServiceReadOnly:
    ///                    Debug.Log("Due to a recent password or email change, the user is not allowed to upload new content. Usually this restriction will expire in 5 days, but can last up to 30 days if the account has been inactive recently.");
    ///                    break;
    ///                default:
    ///                    Debug.Log("For all other EResult types its not supposed to return ... if it does something new changed on the Steam API backend ... you'll have to read up on that to know whats going on.");
    ///                    break;
    ///            }
    ///        }
    ///        else
    ///        {
    ///            Debug.Log("This should never happen.");
    ///        }
    ///    }
    ///}
    /// </code>
    /// </item>
    /// </list>
    /// </example>
    [Serializable]
    public class SteamworksWorkshopItemEditorTool
    {
        /// <summary>
        /// The app ID that the resut item will belong to.
        /// </summary>
        public AppId_t TargetApp;
        /// <summary>
        /// The file ID of the resulting item
        /// </summary>
        public PublishedFileId_t FileId;
        /// <summary>
        /// The <see cref="SteamUserData"/> object of the user that authored this item.
        /// </summary>
        public SteamUserData Author;
        /// <summary>
        /// <para>The type of file to create</para>
        /// <para>Note this tool is designed to work with k_EWorkshopFileTypeCommunity but may be used with other types.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamRemoteStorage#EWorkshopFileType">https://partner.steamgames.com/doc/api/ISteamRemoteStorage#EWorkshopFileType</a>
        /// </summary>
        public EWorkshopFileType FileType = EWorkshopFileType.k_EWorkshopFileTypeCommunity;
        /// <summary>
        /// The title of the item to be created or if updating the string to update the item title to.
        /// </summary>
        public string Title;
        /// <summary>
        /// The description of the item to be created or if updating the string to change the description to.
        /// </summary>
        public string Description;
        /// <summary>
        /// <para>The visibility of the item ... this can be changed by the user in Steam client later.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamRemoteStorage#ERemoteStoragePublishedFileVisibility">https://partner.steamgames.com/doc/api/ISteamRemoteStorage#ERemoteStoragePublishedFileVisibility</a>
        /// </summary>
        public ERemoteStoragePublishedFileVisibility Visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;
        /// <summary>
        /// The tags to be applied to the item
        /// </summary>
        public List<string> Tags = new List<string>();
        /// <summary>
        /// This should be set to a valid folder path in which the content you wish to upload can be found
        /// </summary>
        public string ContentLocation;
        /// <summary>
        /// This will be populated with the image located at the review image location or can be used to create an image at the location.
        /// </summary>
        public Texture2D previewImage;
        /// <summary>
        /// This should be set to a valid file path for the preview image you wish to upload
        /// </summary>
        public string PreviewImageLocation;
        /// <summary>
        /// Occures when the item is created successfully
        /// </summary>
        public UnityWorkshopItemCreatedEvent Created = new UnityWorkshopItemCreatedEvent();
        /// <summary>
        /// Occures when the item creation failed
        /// </summary>
        public UnityWorkshopItemCreatedEvent CreateFailed = new UnityWorkshopItemCreatedEvent();
        /// <summary>
        /// Occures when the item is updated successfully
        /// </summary>
        public UnityWorkshopSubmitItemUpdateResultEvent Updated = new UnityWorkshopSubmitItemUpdateResultEvent();
        /// <summary>
        /// Occures when the item update failes.
        /// </summary>
        public UnityWorkshopSubmitItemUpdateResultEvent UpdateFailed = new UnityWorkshopSubmitItemUpdateResultEvent();

        private CallResult<CreateItemResult_t> m_CreatedItem;
        private CallResult<SubmitItemUpdateResult_t> m_SubmitItemUpdateResult;
        private CallResult<RemoteStorageDownloadUGCResult_t> m_RemoteStorageDownloadUGCResult;
        /// <summary>
        /// Does this item have an ID ... this can be used after create or if updating to insure the file ID is available and valid
        /// </summary>
        public bool HasFileId
        {
            get { return FileId != PublishedFileId_t.Invalid; }
        }
        /// <summary>
        /// Does this item have an app ID ... this can be used to insure the app ID applied is valid
        /// </summary>
        public bool HasAppId
        {
            get { return TargetApp != AppId_t.Invalid; }
        }

        #region Internals
        private bool processingCreateAndUpdate = false;
        private string processingChangeNote = "";
        /// <summary>
        /// WARNING used for internal and debug only
        /// </summary>
        public UGCUpdateHandle_t updateHandle;
        #endregion
        /// <summary>
        /// Creates a new editor tool for a specified app
        /// </summary>
        /// <param name="targetApp">The ID of the app to create or update an item for.</param>
        public SteamworksWorkshopItemEditorTool(AppId_t targetApp)
        {
            TargetApp = targetApp;
            m_SubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(HandleItemUpdated);
            m_CreatedItem = CallResult<CreateItemResult_t>.Create(HandleItemCreate);
            m_RemoteStorageDownloadUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(HandleUGCDownload);
        }

        /// <summary>
        /// Creates a new editor tool to update a given UGC item.
        /// This will assume the file ID and AppID of the itemDetails provided.
        /// </summary>
        /// <param name="itemDetails">The details of the item to update</param>
        public SteamworksWorkshopItemEditorTool(SteamUGCDetails_t itemDetails)
        {
            m_SubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(HandleItemUpdated);
            m_CreatedItem = CallResult<CreateItemResult_t>.Create(HandleItemCreate);
            m_RemoteStorageDownloadUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(HandleUGCDownload);

            TargetApp = itemDetails.m_nConsumerAppID;
            FileId = itemDetails.m_nPublishedFileId;
            Title = itemDetails.m_rgchTitle;
            Description = itemDetails.m_rgchDescription;
            Visibility = itemDetails.m_eVisibility;
            Author = SteamSettings.current.client.GetUserData(itemDetails.m_ulSteamIDOwner);
            var previewCall = SteamRemoteStorage.UGCDownload(itemDetails.m_hPreviewFile, 1);
            m_RemoteStorageDownloadUGCResult.Set(previewCall, HandleUGCDownload);
        }

        /// <summary>
        /// Creates a new editor tool to update a given UGC item.
        /// This will assume the file ID and AppID of the itemDetails provided.
        /// </summary>
        /// <param name="itemDetails">The details of the item to update</param>
        public SteamworksWorkshopItemEditorTool(HeathenWorkshopReadCommunityItem itemDetails)
        {
            m_SubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(HandleItemUpdated);
            m_CreatedItem = CallResult<CreateItemResult_t>.Create(HandleItemCreate);
            m_RemoteStorageDownloadUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(HandleUGCDownload);

            TargetApp = itemDetails.TargetApp;
            FileId = itemDetails.FileId;
            Title = itemDetails.Title;
            Description = itemDetails.Description;
            Visibility = itemDetails.Visibility;
            Author = SteamSettings.current.client.GetUserData(itemDetails.Author);
            previewImage = itemDetails.previewImage;
            PreviewImageLocation = itemDetails.PreviewImageLocation;
        }

        /// <summary>
        /// Creates a new file with the currently applied information
        /// </summary>
        /// <param name="changeNote">This will be applied as a change note on update of the file</param>
        /// <returns>Returns instantly, true indicates request submited, false indicates an error in create process, note that the file is created empty and update will happen asynchroniously</returns>
        public bool CreateAndUpdate(string changeNote)
        {
            if (TargetApp == AppId_t.Invalid)
            {
                Debug.LogError("HeathenWorkshopItem|CreateAndUpdate ... Create operation aborted, the current AppId is invalid.");
                return false;
            }

            if (string.IsNullOrEmpty(Title))
            {
                Debug.LogError("HeathenWorkshopItem|CreateAndUpdate ... operation aborted, Title is null or empty and must have a value.");
                return false;
            }

            if (string.IsNullOrEmpty(ContentLocation))
            {
                Debug.LogError("HeathenWorkshopItem|CreateAndUpdate ... operation aborted, Content location is null or empty and must have a value.");
                return false;
            }

            if (string.IsNullOrEmpty(PreviewImageLocation))
            {
                Debug.LogError("HeathenWorkshopItem|CreateAndUpdate ... operation aborted, Preview image location is null or empty and must have a value.");
                return false;
            }

            processingChangeNote = changeNote;
            processingCreateAndUpdate = true;

            var call = SteamUGC.CreateItem(TargetApp, FileType);
            m_CreatedItem.Set(call, HandleItemCreate);

            return true;
        }

        /// <summary>
        /// Starts and Item Update process updating all standard fields of the item
        /// </summary>
        /// <param name="changeNote">The change note to be applied to the update entry</param>
        /// <returns>True if update was submitted without error, false otherwise</returns>
        public bool Update(string changeNote)
        {
            if (TargetApp == AppId_t.Invalid)
            {
                Debug.LogError("HeathenWorkshopItem|Update ... Update operation aborted, the current AppId is invalid.");
                return false;
            }

            if (FileId == PublishedFileId_t.Invalid)
            {
                Debug.LogError("HeathenWorkshopItem|Update ... Update operation aborted, the current FileId is invalid.");
                return false;
            }

            if (!Directory.Exists(ContentLocation))
            {
                Debug.LogError("HeathenWorkshopItem|Update ... Failed to update item content location, [" + ContentLocation + "] does not exist, this must be a valid folder path.");
                return false;
            }

            if (!File.Exists(PreviewImageLocation))
            {
                Debug.LogError("HeathenWorkshopItem|Update ... Failed to update item preview, [" + PreviewImageLocation + "] does not exist, this must be a valid file path.");
                return false;
            }

            updateHandle = SteamUGC.StartItemUpdate(TargetApp, FileId);

            if (!SteamUGC.SetItemTitle(updateHandle, Title))
            {
                Debug.LogError("HeathenWorkshopItem|Update ... Failed to update item title, item has not been updated.");
                return false;
            }

            if (!SteamUGC.SetItemDescription(updateHandle, Description))
            {
                Debug.LogError("HeathenWorkshopItem|Update ... Failed to update item description, item has not been updated.");
                return false;
            }

            if (!SteamUGC.SetItemVisibility(updateHandle, Visibility))
            {
                Debug.LogError("HeathenWorkshopItem|Update ... Failed to update item visibility, item has not been updated.");
                return false;
            }

            if (!SteamUGC.SetItemTags(updateHandle, Tags))
            {
                Debug.LogError("HeathenWorkshopItem|Update ... Failed to update item tags, item has not been updated.");
                return false;
            }

            if (!SteamUGC.SetItemContent(updateHandle, ContentLocation))
            {
                Debug.LogError("HeathenWorkshopItem|Update ... Failed to update item content location, item has not been updated.");
                return false;
            }

            if (!SteamUGC.SetItemPreview(updateHandle, PreviewImageLocation))
            {
                Debug.LogError("HeathenWorkshopItem|Update ... Failed to update item preview, item has not been updated.");
                return false;
            }

            var call = SteamUGC.SubmitItemUpdate(updateHandle, changeNote);
            m_SubmitItemUpdateResult.Set(call, HandleItemUpdated);

            return true;
        }

        /// <summary>
        /// Fetch the status of the current update operation
        /// Note that this depends on a valid updateHandle
        /// </summary>
        /// <param name="bytesProcessed"></param>
        /// <param name="bytesTotal"></param>
        /// <returns></returns>
        public EItemUpdateStatus GetItemUpdateProgress(out ulong bytesProcessed, out ulong bytesTotal)
        {
            if (updateHandle != UGCUpdateHandle_t.Invalid)
            {
                return SteamUGC.GetItemUpdateProgress(updateHandle, out bytesProcessed, out bytesTotal);
            }
            else
            {
                bytesProcessed = 0;
                bytesTotal = 0;
                return EItemUpdateStatus.k_EItemUpdateStatusInvalid;
            }
        }

        #region Handlers
        private void HandleItemUpdated(SubmitItemUpdateResult_t param, bool bIOFailure)
        {
            if (bIOFailure)
                UpdateFailed.Invoke(param);
            else
                Updated.Invoke(param);
        }

        private void HandleItemCreate(CreateItemResult_t param, bool bIOFailure)
        {
            if (bIOFailure)
                CreateFailed.Invoke(param);
            else
            {
                Author = SteamSettings.current.client.GetUserData(SteamUser.GetSteamID());
                FileId = param.m_nPublishedFileId;
                Created.Invoke(param);
            }

            if (processingCreateAndUpdate)
            {
                processingCreateAndUpdate = false;
                Update(processingChangeNote);
                processingChangeNote = string.Empty;
            }
        }

        private void HandleUGCDownload(RemoteStorageDownloadUGCResult_t param, bool bIOFailure)
        {
            //TODO: we shoudl probably setup a unique handler for each type of file .. at the moment we are assuming we only ever load the preview image
            PreviewImageLocation = param.m_pchFileName;
            Texture2D image;
            if (SteamUtilities.LoadImageFromDisk(param.m_pchFileName, out image))
            {
                previewImage = image;
            }
            else
            {
                Debug.LogError("Failed to load preview image (" + param.m_pchFileName + ") from disk!");
            }
        }
        #endregion
    }
}
#endif