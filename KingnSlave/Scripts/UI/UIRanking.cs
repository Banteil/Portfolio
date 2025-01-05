using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIRanking : UIPopup
    {
        private const float COLOR_MULTIPLIER = 0.7f;
        private List<UserData> rankerList = new List<UserData>();
        private Define.GamePlayMode currentCachedMode = Define.GamePlayMode.None;

        public enum TapButton
        {
            RankTap,
            NormalTap,
            SingleTap
        }

        public enum RankingUIScrollView
        {
            RankingScrollView,
        }

        private void Start() => Initialized();

        async protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<ScrollRect>(typeof(RankingUIScrollView));
            Bind<Tap>(typeof(TapButton));
            var rankTap = Get<Tap>((int)TapButton.RankTap);
            var normalTap = Get<Tap>((int)TapButton.NormalTap);
            var singleTap = Get<Tap>((int)TapButton.SingleTap);
            BindEvent(rankTap.gameObject, delegate { SetRankingList(Define.GamePlayMode.PVPRank); });
            BindEvent(normalTap.gameObject, delegate { SetRankingList(Define.GamePlayMode.PVPNormal); });
            BindEvent(singleTap.gameObject, delegate { SetRankingList(Define.GamePlayMode.SingleStory); });
            rankTap.isOn = true;

            currentCachedMode = Define.GamePlayMode.PVPRank;
            await CallAPI.APISelectRankRankingsTopList(UserDataManager.Instance.MySid, (userDataList) =>
            {
                rankerList = userDataList.ToList();
                var maxCount = rankerList.Count > 100 ? 100 : rankerList.Count;
                var scrollRect = GetScrollRect((int)RankingUIScrollView.RankingScrollView) as InfinityScrollRect;

                //scrollRect.DestroyAllListData();
                scrollRect.MaxCount = maxCount;
                scrollRect.CreatePoolingList<UIRankerInfo>("RankerInfo");
            });
        }

        /// <summary>
        /// 랭킹 리스트 세팅 함수
        /// </summary>
        async private void SetRankingList(Define.GamePlayMode mode)
        {
            Debug.Log("mode:" + mode);
            if (mode == currentCachedMode)
                return;

            switch(mode)
            {
                case Define.GamePlayMode.PVPRank:
                    LogManager.Instance.InsertActionLog(14);
                    break;
                case Define.GamePlayMode.PVPNormal:
                    LogManager.Instance.InsertActionLog(15);
                    break;
                case Define.GamePlayMode.SingleStory:
                    LogManager.Instance.InsertActionLog(16);
                    break;
            }

            currentCachedMode = mode;
            var scrollRect = GetScrollRect((int)RankingUIScrollView.RankingScrollView) as InfinityScrollRect;
            scrollRect.velocity = Vector2.zero;
            scrollRect.content.position = new Vector2(scrollRect.content.position.x, 0);

            if (mode == Define.GamePlayMode.PVPRank)
            {
                var rankTap = Get<Tap>((int)TapButton.RankTap);
                rankTap.isOn = true;               

                await CallAPI.APISelectRankRankingsTopList(UserDataManager.Instance.MySid, (userDataList) =>
                {
                    rankerList = userDataList.ToList();
                    var maxCount = rankerList.Count > 100 ? 100 : rankerList.Count;
                    var scrollRect = GetScrollRect((int)RankingUIScrollView.RankingScrollView) as InfinityScrollRect;

                    //scrollRect.DestroyAllListData();
                    scrollRect.MaxCount = maxCount;
                    scrollRect.ResetListData();
                });
            }
            else if (mode == Define.GamePlayMode.PVPNormal)
            {
                var normalTap = Get<Tap>((int)TapButton.NormalTap);
                normalTap.isOn = true;

                await CallAPI.APISelectNormalRankingsTopList(UserDataManager.Instance.MySid, (userDataList) =>
                {
                    rankerList = userDataList.ToList();
                    var maxCount = rankerList.Count > 100 ? 100 : rankerList.Count;
                    var scrollRect = GetScrollRect((int)RankingUIScrollView.RankingScrollView) as InfinityScrollRect;

                    //scrollRect.DestroyAllListData();
                    scrollRect.MaxCount = maxCount;
                    scrollRect.ResetListData(); //NormalRankerInfo
                });
            }
            else
            {
                var singleTap = Get<Tap>((int)TapButton.SingleTap);
                singleTap.isOn = true;

                await CallAPI.APISelectSingleRankingsTopList(UserDataManager.Instance.MySid, (userDataList) =>
                {
                    rankerList = userDataList.ToList();
                    var maxCount = rankerList.Count > 100 ? 100 : rankerList.Count;
                    var scrollRect = GetScrollRect((int)RankingUIScrollView.RankingScrollView) as InfinityScrollRect;

                    //scrollRect.DestroyAllListData();
                    scrollRect.MaxCount = maxCount;
                    scrollRect.ResetListData(); //SingleRankerInfo
                });
            }
        }

        public override void SetListData(UIList list)
        {
            var rankerInfoList = list as UIRankerInfo;
            var index = rankerInfoList.GetIndex();
            rankerInfoList.SetListData(rankerList[index], currentCachedMode);
        }
    }
}
