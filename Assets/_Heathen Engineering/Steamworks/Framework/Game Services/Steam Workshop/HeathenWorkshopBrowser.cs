#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.GameServices
{
    public class HeathenWorkshopBrowser : MonoBehaviour
    {
        public AppId_t CreatorAppId;
        public SteamSettings steamSettings;
        //public SteamworksWorkshop SteamworksWorkshop;
        public GameObject WorkshopItemDisplayTemplate;
        public HeathenWorkshopItemQuery ActiveQuery;
        public Transform CollectionRoot;
        public bool GeneralSearchOnStart = true;
        public UnityEngine.UI.InputField searchBox;
        public UnityEngine.UI.Text currentCount;
        public UnityEngine.UI.Text totalCount;
        public UnityEngine.UI.Text currentPage;

        #region Events
        public UnityHeathenWorkshopItemQueryEvent QueryPrepared;
        public UnityEvent ResultsUpdated;
        #endregion

        private string lastSearchString = "";
        
        private void Awake()
        {
            SteamworksWorkshop.RegisterWorkshopSystem();
        }

        private void Start()
        {
            if (GeneralSearchOnStart)
                SearchAll(string.Empty);
        }

        private void Update()
        {
            if(ActiveQuery != null)
            {
                var pageCount = (int)(ActiveQuery.Page * 50);
                if (pageCount < ActiveQuery.matchedRecordCount)
                    currentCount.text = (pageCount - 49).ToString() + "-" + pageCount.ToString();
                else
                {
                    //Must be on last page
                    var remainder = (int)(ActiveQuery.matchedRecordCount % 50);
                    pageCount = (pageCount - 50) + remainder;
                    currentCount.text = (pageCount - (pageCount-1)).ToString() + "-" + pageCount.ToString();
                }
                totalCount.text = ActiveQuery.matchedRecordCount.ToString("N0");
                currentPage.text = ActiveQuery.Page.ToString();
            }
            else
            {
                currentCount.text = "0";
                totalCount.text = "0";
            }
        }

        private void ClearCollectionRoot()
        {
            List<GameObject> targets = new List<GameObject>();
            foreach(Transform child in CollectionRoot)
            {
                targets.Add(child.gameObject);
            }

            while(targets.Count > 0)
            {
                var t = targets[0];
                targets.RemoveAt(0);
                Destroy(t);
            }
        }

        public void SearchAllFromInput()
        {
            SearchAll(searchBox.text);
        }
        
        public void SearchAll(string filter)
        {
            lastSearchString = filter;
            ActiveQuery = HeathenWorkshopItemQuery.Create(EUGCQuery.k_EUGCQuery_RankedByTrend, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, CreatorAppId, steamSettings.applicationId);
            if (!string.IsNullOrEmpty(filter))
            {
                SteamworksWorkshop.WorkshopSetSearchText(ActiveQuery.handle, filter);
            }
            ActiveQuery.Execute(HandleResults);
        }
        
        public void PrepareSearchAll(string filter)
        {
            lastSearchString = filter;
            ActiveQuery = HeathenWorkshopItemQuery.Create(EUGCQuery.k_EUGCQuery_RankedByTrend, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, CreatorAppId, steamSettings.applicationId);
            if (!string.IsNullOrEmpty(filter))
            {
                SteamworksWorkshop.WorkshopSetSearchText(ActiveQuery.handle, filter);
            }
            QueryPrepared.Invoke(ActiveQuery);
        }

        public void SearchFavoritesFromInput()
        {
            SearchFavorites(searchBox.text);
        }

        public void SearchFavorites(string filter)
        {
            lastSearchString = filter;
            ActiveQuery = HeathenWorkshopItemQuery.Create(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Favorited, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, CreatorAppId, steamSettings.applicationId);
            if (!string.IsNullOrEmpty(filter))
            {
                SteamworksWorkshop.WorkshopSetSearchText(ActiveQuery.handle, filter);
            }
            ActiveQuery.Execute(HandleResults);
        }

        public void PrepareSearchFavorites(string filter)
        {
            lastSearchString = filter;
            ActiveQuery = HeathenWorkshopItemQuery.Create(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Favorited, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, CreatorAppId, steamSettings.applicationId);
            if (!string.IsNullOrEmpty(filter))
            {
                SteamworksWorkshop.WorkshopSetSearchText(ActiveQuery.handle, filter);
            }
            QueryPrepared.Invoke(ActiveQuery);
        }

        public void SearchFollowedFromInput()
        {
            SearchFollowed(searchBox.text);
        }

        public void SearchFollowed(string filter)
        {
            lastSearchString = filter;
            ActiveQuery = HeathenWorkshopItemQuery.Create(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Followed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, CreatorAppId, steamSettings.applicationId);
            if (!string.IsNullOrEmpty(filter))
            {
                SteamworksWorkshop.WorkshopSetSearchText(ActiveQuery.handle, filter);
            }
            ActiveQuery.Execute(HandleResults);
        }

        public void PrepareSearchFollowed(string filter)
        {
            lastSearchString = filter;
            ActiveQuery = HeathenWorkshopItemQuery.Create(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Followed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_TitleAsc, CreatorAppId, steamSettings.applicationId);
            if (!string.IsNullOrEmpty(filter))
            {
                SteamworksWorkshop.WorkshopSetSearchText(ActiveQuery.handle, filter);
            }
            QueryPrepared.Invoke(ActiveQuery);
        }

        public void ExeuctePreparedSearch()
        {
            if(ActiveQuery != null)
                ActiveQuery.Execute(HandleResults);
        }

        public void SetNextSearchPage()
        {
            if (ActiveQuery != null)
            {
                ActiveQuery.SetNextPage();

                if (!string.IsNullOrEmpty(lastSearchString))
                    SteamworksWorkshop.WorkshopSetSearchText(ActiveQuery.handle, lastSearchString);

                ActiveQuery.Execute(HandleResults);
            }
        }

        public void SetPreviousSearchPage()
        {
            if (ActiveQuery != null)
            {
                ActiveQuery.SetPreviousPage();

                if (!string.IsNullOrEmpty(lastSearchString))
                    SteamworksWorkshop.WorkshopSetSearchText(ActiveQuery.handle, lastSearchString);

                ActiveQuery.Execute(HandleResults);
            }
        }

        public void SetSearchPage(uint page)
        {
            if(ActiveQuery != null)
            {
                ActiveQuery.SetPage(page);

                if (!string.IsNullOrEmpty(lastSearchString))
                    SteamworksWorkshop.WorkshopSetSearchText(ActiveQuery.handle, lastSearchString);

                ActiveQuery.Execute(HandleResults);
            }
        }

        private void HandleResults(HeathenWorkshopItemQuery query)
        {
            //Only load if this is the current active query ... otherwise this query has been abandoned 
            if (query == ActiveQuery)
            {
                ClearCollectionRoot();

                foreach (var result in query.ResultsList)
                {
                    var go = Instantiate(WorkshopItemDisplayTemplate, CollectionRoot);
                    var trans = go.GetComponent<Transform>();
                    trans.localPosition = Vector3.zero;
                    var record = go.GetComponent<IWorkshopItemDisplay>();
                    record.RegisterData(result);
                }
                ResultsUpdated.Invoke();
            }
            else
            {
                query.Dispose();
            }
        }
    }
}
#endif