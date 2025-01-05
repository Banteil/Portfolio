using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIRoundInfo : UIPopup
    {
        private List<RoundData> roundDatas = new List<RoundData>();

        private enum RoundInfoButtons
        {
            CloseButton,
        }

        private enum RoundInfoText
        {
            RedNickNameText,
            BlueNickNameText,
        }

        private enum RoundInfoRawImage
        {
            RedProfileImage,
            BlueProfileImage,
        }

        private enum RoundInfoScrollView
        {
            RoundInfoScrollView,
        }

        protected override void Awake() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<Button>(typeof(RoundInfoButtons));
            Bind<ScrollRect>(typeof(RoundInfoScrollView));
            Bind<RawImage>(typeof(RoundInfoRawImage));
            Bind<TextMeshProUGUI>(typeof(RoundInfoText));
            var button = GetButton((int)RoundInfoButtons.CloseButton);
            button.gameObject.BindEvent(OnCloseButtonClicked);
        }

        async public void SetRoundData(string sid, GameResultData roomData)
        {
            await CallAPI.APISelectGameRoundList(sid, roomData.room_id, (roundDataList) =>
            {                
                roundDatas = roundDataList;                
            });
            if (roundDatas == null || roundDatas.Count == 0)
            {
                UIManager.Instance.CloseUI();
                UIManager.Instance.ShowWarningUI("emptyRoundInfo");
                return;
            }

            GetText((int)RoundInfoText.BlueNickNameText).text = roomData.nickname_blue;
            GetText((int)RoundInfoText.RedNickNameText).text = roomData.nickname_red;
            NetworkManager.Instance.GetTexture((texture) =>
            {
                GetRawImage((int)RoundInfoRawImage.BlueProfileImage).texture = texture;
            }, roomData.profile_image_blue);
            NetworkManager.Instance.GetTexture((texture) =>
            {
                GetRawImage((int)RoundInfoRawImage.RedProfileImage).texture = texture;
            }, roomData.profile_image_red);

            var content = GetScrollRect((int)RoundInfoScrollView.RoundInfoScrollView).content;
            for (int i = 0; i < roundDatas.Count; i++)
            {
                var roundList = AddRoundList(content, i);
                roundList.Initialized();
                roundList.SetListData(roundDatas[i]);
            }
        }

        private UIRoundList AddRoundList(Transform parent, int index) => UIManager.Instance.AddListUI<UIRoundList>(parent, "RoundListUI", index);
    }
}
