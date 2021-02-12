#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.GameServices
{
    public class HeathenWorkshopItemQuery : IDisposable
    {
        public UGCQueryHandle_t handle;
        public uint matchedRecordCount = 0;
        public uint PageCount = 1;
        private bool isAllQuery = false;
        private bool isUserQuery = false;
        private List<PublishedFileId_t> FileIds = new List<PublishedFileId_t>();
        private EUserUGCList listType;
        private EUGCQuery queryType;
        private EUGCMatchingUGCType matchingType;
        private EUserUGCListSortOrder sortOrder;
        private AppId_t creatorApp;
        private AppId_t consumerApp;
        private AccountID_t account;
        private uint _Page = 1;
        public uint Page { get { return _Page; } private set { _Page = value; } }
        private UnityAction<HeathenWorkshopItemQuery> Callback;

        public CallResult<SteamUGCQueryCompleted_t> m_SteamUGCQueryCompleted;

        public List<HeathenWorkshopReadCommunityItem> ResultsList = new List<HeathenWorkshopReadCommunityItem>();

        public HeathenWorkshopItemQuery()
        {
            m_SteamUGCQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(HandleQueryCompleted);
        }

        public static HeathenWorkshopItemQuery Create(EUGCQuery queryType, EUGCMatchingUGCType matchingType, AppId_t creatorApp, AppId_t consumerApp)
        {
            HeathenWorkshopItemQuery nQuery = new HeathenWorkshopItemQuery
            {
                matchedRecordCount = 0,
                PageCount = 1,
                isAllQuery = true,
                isUserQuery = false,
                queryType = queryType,
                matchingType = matchingType,
                creatorApp = creatorApp,
                consumerApp = consumerApp,
                Page = 1,
                handle = SteamUGC.CreateQueryAllUGCRequest(queryType, matchingType, creatorApp, consumerApp, 1)
            };

            return nQuery;
        }

        public static HeathenWorkshopItemQuery Create(IEnumerable<PublishedFileId_t> fileIds)
        {
            var list = new List<PublishedFileId_t>(fileIds);
            HeathenWorkshopItemQuery nQuery = new HeathenWorkshopItemQuery
            {
                matchedRecordCount = 0,
                PageCount = 1,
                isAllQuery = true,
                isUserQuery = false,
                FileIds = list,
                Page = 1,
                handle = SteamUGC.CreateQueryUGCDetailsRequest(list.ToArray(), (uint)list.Count)
            };

            return nQuery;
        }

        public static HeathenWorkshopItemQuery Create(AccountID_t account, EUserUGCList listType, EUGCMatchingUGCType matchingType, EUserUGCListSortOrder sortOrder, AppId_t creatorApp, AppId_t consumerApp)
        {
            HeathenWorkshopItemQuery nQuery = new HeathenWorkshopItemQuery
            {
                matchedRecordCount = 0,
                PageCount = 1,
                isAllQuery = false,
                isUserQuery = true,
                listType = listType,
                sortOrder = sortOrder,
                matchingType = matchingType,
                creatorApp = creatorApp,
                consumerApp = consumerApp,
                account = account,
                Page = 1,
                handle = SteamUGC.CreateQueryUserUGCRequest(account, listType, matchingType, sortOrder, creatorApp, consumerApp, 1)
            };

            return nQuery;
        }

        public bool SetNextPage()
        {
            return SetPage((uint)Mathf.Clamp((int)Page + 1, 1, int.MaxValue));
        }

        public bool SetPreviousPage()
        {
            return SetPage((uint)Mathf.Clamp((int)Page - 1, 1, int.MaxValue));
        }

        public bool SetPage(uint page)
        {
            Page = page > 0 ? page : 1;
            if (isAllQuery)
            {
                ReleaseHandle();
                handle = SteamUGC.CreateQueryAllUGCRequest(queryType, matchingType, creatorApp, consumerApp, Page);
                matchedRecordCount = 0;
                return true;
            }
            else if (isUserQuery)
            {
                ReleaseHandle();
                handle = SteamUGC.CreateQueryUserUGCRequest(account, listType, matchingType, sortOrder, creatorApp, consumerApp, Page);
                matchedRecordCount = 0;
                return true;
            }
            else
            {
                Debug.LogError("Pages are not supported by detail queries e.g. searching for specific file Ids");
                return false;
            }
        }

        public bool Execute(UnityAction<HeathenWorkshopItemQuery> callback)
        {
            if(handle == UGCQueryHandle_t.Invalid)
            {
                Debug.LogError("Invalid handle, you must call CreateAll");
                return false;
            }

            ResultsList.Clear();
            Callback = callback;
            var call = SteamUGC.SendQueryUGCRequest(handle);
            m_SteamUGCQueryCompleted.Set(call, HandleQueryCompleted);

            return true;
        }

        private void HandleQueryCompleted(SteamUGCQueryCompleted_t param, bool bIOFailure)
        {
            if (!bIOFailure)
            {
                if (param.m_eResult == EResult.k_EResultOK)
                {
                    matchedRecordCount = param.m_unTotalMatchingResults;

                    PageCount = (uint)Mathf.Clamp((int)matchedRecordCount / 50, 1, int.MaxValue);
                    if (PageCount * 50 < matchedRecordCount)
                        PageCount++;

                    for (int i = 0; i < param.m_unNumResultsReturned; i++)
                    {
                        SteamUGCDetails_t details;
                        SteamUGC.GetQueryUGCResult(param.m_handle, (uint)i, out details);
                        var nRecord = new HeathenWorkshopReadCommunityItem(details);
                        ResultsList.Add(nRecord);
                    }
                    ReleaseHandle();
                    if (Callback != null)
                        Callback.Invoke(this);
                }
                else
                {
                    Debug.LogError("HeathenWorkitemQuery|HandleQueryCompleted Unexpected results, state = " + param.m_eResult.ToString());
                }
            }
            else
            {
                Debug.LogError("HeathenWorkitemQuery|HandleQueryCompleted failed.");
            }
        }

        public void ReleaseHandle()
        {
            if (handle != UGCQueryHandle_t.Invalid)
            {
                SteamUGC.ReleaseQueryUGCRequest(handle);
                handle = UGCQueryHandle_t.Invalid;
            }
        }

        public void Dispose()
        {
            try
            {
                ReleaseHandle();
            }
            catch { }
        }
    }
}
#endif