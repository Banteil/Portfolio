using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIRecordList : UIList
    {
        private Color blueColor = new Color(0f / 255f, 97f / 255f, 237f / 255f, 255f / 255f);
        private Color redColor = new Color(255f / 255f, 0f / 255f, 55f / 255f, 255f / 255f);
        private Color lightBlueColor = new Color(91f / 255f, 146f / 255f, 219f / 255f, 255f / 255f);
        private Color lightRedColor = new Color(210f / 255f, 116f / 255f, 123f / 255f, 255f / 255f);
        private const string NORMAL_TEXT_KEY = "normalText";
        private const string RANK_TEXT_KEY = "rankText";
        private const string WIN_TEXT_KEY = "winText";
        private const string LOSE_TEXT_KEY = "loseText";

        private UIUserProfile profile;
        public UIUserProfile Profile { set { profile = value; } }
        private Image recordListImage;

        private string opponentSid;

        enum RecordListText
        {
            GameModeText,
            ScoreText,
            GameResultText,
            PlayerNickNameText,
            OpponentNickNameText,
            VSText,
            ModTimeText,
        }

        enum RecordListRawImage
        {
            PlayerProfileImage,
            OpponentProfileImage,
        }

        enum RecordListImage
        {
            PlayerNickNameFrame,
            OpponentNickNameFrame,
            PlayerProfileBackground,
            OpponentProfileBackground,
            GameResultFrame,
            PlayerCountryinfo,
            OpponentCountryinfo,
        }

        enum RecordListButton
        {
            SelectButton,
            OpponentProfileImage,
        }

        private void Awake() => Initialized();

        protected override void InitializedProcess()
        {
            SetParent<UIRecordFrame>();
            recordListImage = GetComponent<Image>();
            Bind<TextMeshProUGUI>(typeof(RecordListText));
            Bind<Image>(typeof(RecordListImage));
            Bind<RawImage>(typeof(RecordListRawImage));
            Bind<Button>(typeof(RecordListButton));
            var selectButton = GetButton((int)RecordListButton.SelectButton);
            selectButton.gameObject.BindEvent(ShowRoundInfo);
            var opponentProfileImage = GetButton((int)RecordListButton.OpponentProfileImage);
            opponentProfileImage.gameObject.BindEvent(ShowOpponentUserProfile);
        }

        public void SetListData(GameResultData data)
        {
            var ownerIsWinner = data.my_result == "W";
            var ownerIsBlue = data.my_team == "B";
            opponentSid = ownerIsWinner ? data.sid_loser : data.sid_winner;

            var gameModeText = GetText((int)RecordListText.GameModeText);
            gameModeText.text = data.game_type == (int)Define.APIGameType.Normal ?
                Util.GetLocalizationTableString(Define.CommonLocalizationTable, NORMAL_TEXT_KEY) : Util.GetLocalizationTableString(Define.CommonLocalizationTable, RANK_TEXT_KEY);
            gameModeText.color = ownerIsWinner ? blueColor : redColor;

            var scoreText = GetText((int)RecordListText.ScoreText);
            if (data.game_type == (int)Define.APIGameType.Normal)
            {
                scoreText.enabled = false;
            }
            else
            {
                scoreText.enabled = true;
                scoreText.text = ownerIsWinner ? $"{data.rank_point_winner} RP" : $"{data.rank_point_loser} RP";
                scoreText.color = ownerIsWinner ? lightBlueColor : lightRedColor;
            }

            GetImage((int)RecordListImage.GameResultFrame).sprite = ResourceManager.Instance.GetSprite(ownerIsWinner ? "RecordFrame_Blue" : "RecordFrame_Red");
            var gamResultText = GetText((int)RecordListText.GameResultText);
            gamResultText.text = ownerIsWinner ?
                Util.GetLocalizationTableString(Define.CommonLocalizationTable, WIN_TEXT_KEY) : Util.GetLocalizationTableString(Define.CommonLocalizationTable, LOSE_TEXT_KEY);

            var modTImeText = GetText((int)RecordListText.ModTimeText);
            modTImeText.text = data.mod_time;
            modTImeText.color = ownerIsWinner ? lightBlueColor : lightRedColor;

            GetText((int)RecordListText.VSText).color = ownerIsWinner ? lightBlueColor : lightRedColor;

            recordListImage.sprite = ResourceManager.Instance.GetSprite(ownerIsWinner ? "Record_Win" : "Record_Lose");
            GetImage((int)RecordListImage.PlayerCountryinfo).sprite = ResourceManager.Instance.FlagSpriteList[ownerIsBlue ? data.country_seq_blue : data.country_seq_red];
            GetImage((int)RecordListImage.OpponentCountryinfo).sprite = ResourceManager.Instance.FlagSpriteList[ownerIsBlue ? data.country_seq_red : data.country_seq_blue];
            GetImage((int)RecordListImage.PlayerNickNameFrame).sprite = ResourceManager.Instance.GetSprite(ownerIsBlue ? "RecordFrame_Blue" : "RecordFrame_Red"); 
            GetImage((int)RecordListImage.OpponentNickNameFrame).sprite = ResourceManager.Instance.GetSprite(ownerIsBlue ? "RecordFrame_Red" : "RecordFrame_Blue");
            GetImage((int)RecordListImage.PlayerProfileBackground).sprite = ResourceManager.Instance.GetSprite(ownerIsBlue ? "RecordProfile_Blue" : "RecordProfile_Red");
            GetImage((int)RecordListImage.OpponentProfileBackground).sprite = ResourceManager.Instance.GetSprite(ownerIsBlue ? "RecordProfile_Red" : "RecordProfile_Blue");
            GetButton((int)RecordListButton.SelectButton).image.sprite = ResourceManager.Instance.GetSprite(ownerIsWinner ? "RecordButton_Win" : "RecordButton_Lose");
            SetUserInfo(data);
        }

        private void SetUserInfo(GameResultData data)
        {
            var ownerIsBlue = data.my_team == "B";
            GetText((int)RecordListText.PlayerNickNameText).text = ownerIsBlue ? data.nickname_blue : data.nickname_red;
            GetText((int)RecordListText.OpponentNickNameText).text = ownerIsBlue ? data.nickname_red : data.nickname_blue;

            var playerURL = ownerIsBlue ? data.profile_image_blue : data.profile_image_red;
            var playerRawImage = GetRawImage((int)RecordListRawImage.PlayerProfileImage);
            if (playerRawImage.texture.name != playerURL)
                NetworkManager.Instance.GetTexture((texture) =>
                {
                    playerRawImage.texture = texture;
                }, playerURL);

            var opponentURL = ownerIsBlue ? data.profile_image_red : data.profile_image_blue;
            var opponentRawImage = GetRawImage((int)RecordListRawImage.OpponentProfileImage);
            if (opponentRawImage.texture.name != opponentURL)
                NetworkManager.Instance.GetTexture((texture) =>
                {
                    opponentRawImage.texture = texture;
                }, opponentURL);
        }

        private void ShowRoundInfo(PointerEventData data)
        {
            if (isDrag) return;
            LogManager.Instance.InsertActionLog(12);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var roundInfo = UIManager.Instance.ShowUI<UIRoundInfo>("RoundInfoUI");
            if (roundInfo != null)
            {
                var recordFrame = Util.FindComponentInParents<UIRecordFrame>(transform);
                var gameResultData = recordFrame.GetGameResultData(index);
                roundInfo.SetRoundData(profile.ProfileUserData.sid, gameResultData);
            }
        }

        private void ShowOpponentUserProfile(PointerEventData data)
        {
            if (isDrag) return;
            LogManager.Instance.InsertActionLog(11);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            UIManager.Instance.CloseUI();
            UIManager.Instance.ShowUserProfile(opponentSid);
        }
    }
}
