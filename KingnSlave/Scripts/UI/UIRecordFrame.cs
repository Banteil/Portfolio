using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIRecordFrame : UIFrame
    {
        private UIUserProfile profile;

        [SerializeField] private int pageSize = 10;
        private int lastPage;

        private List<GameResultData> gameResults = new List<GameResultData>();

        private string ProfileSid { get { return profile.ProfileUserData.sid; } }

        enum RecordFrameText
        {
            EmptyListText,
        }

        enum RecordFrameScrollRect
        {
            RecordListScrollView,
        }

        public GameResultData GetGameResultData(int index) => gameResults[index];

        protected override void InitializedProcess()
        {
            profile = Util.FindComponentInParents<UIUserProfile>(transform);
            Bind<TextMeshProUGUI>(typeof(RecordFrameText));
            Bind<ScrollRect>(typeof(RecordFrameScrollRect));
        }

        public override void ActiveFrame(bool isActive)
        {
            LogManager.Instance.InsertActionLog(10);
            base.ActiveFrame(isActive);
        }

        async protected override void SetDataProcess<T>(T data)
        {
            var userData = data as UserData;
            if (userData == null)
            {
                Debug.LogError("유저 데이터가 존재하지 않습니다.");
                return;
            }
            await GetLastPage(userData);
            await SetGameResults();

            var scrollRect = GetScrollRect((int)RecordFrameScrollRect.RecordListScrollView) as InfinityScrollRect;
            scrollRect.MaxCount = gameResults.Count;
            scrollRect.CreatePoolingList<UIRecordList>("RecordListUI");
        }

        async private UniTask GetLastPage(UserData data)
        {
            await CallAPI.APISelectGameRoomHistoryListLastPageNum(data.sid, data.sid, pageSize, (lastPageNum) =>
            {
                lastPage = lastPageNum;
            });
        }

        async private UniTask SetGameResults()
        {
            await CallAPI.APISelectGameRoomHistoryList(ProfileSid, ProfileSid, pageSize * lastPage, 1, (gameResultDataList) =>
            {
                gameResults = gameResultDataList;
            });

            GetText((int)RecordFrameText.EmptyListText).gameObject.SetActive(gameResults.Count <= 0);
        }

        public override void SetListData(UIList list)
        {
            var recordList = list as UIRecordList;
            var index = recordList.GetIndex();
            recordList.Profile = profile;
            recordList.SetListData(gameResults[index]);
        }
    }
}
