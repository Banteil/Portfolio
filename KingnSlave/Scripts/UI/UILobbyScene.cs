using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UILobbyScene : UIScene
    {
        enum LobbySceneButton
        {
            GameStartButton,
            RankingButton,
            ShopButton,
            UserProfileButton,
            MyItemButton,
            HelpButton,
        }

        enum LobbySceneRawImage
        {
            UserProfileImage,
        }

        protected void Start()
        {
            Initialized();
            LogManager.Instance.InsertActionLog(0);
        }

        async protected override void InitializedProcess()
        {
            base.InitializedProcess();
#if UNITY_EDITOR
            if (!GameManager.Instance.CheckCurrentVersion)
                GameManager.Instance.CheckCurrentVersion = true;
#endif
            GameManager.Instance.CurrentGameMode = Define.GamePlayMode.None;
            AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.LobbyScene), 0.5f, true);

            Bind<Button>(typeof(LobbySceneButton));
            var gameStartButton = GetButton((int)LobbySceneButton.GameStartButton);
            gameStartButton.gameObject.BindEvent(StartGame);

            var rankingButton = GetButton((int)LobbySceneButton.RankingButton);
            rankingButton.gameObject.BindEvent(ShowRankingPopup);

            var userProfileButton = GetButton((int)LobbySceneButton.UserProfileButton);
            userProfileButton.gameObject.BindEvent(ShowUserProfilePopup);

            var shopButton = GetButton((int)LobbySceneButton.ShopButton);
            shopButton.gameObject.BindEvent(ShowShopPopup);

            var itemButton = GetButton((int)LobbySceneButton.MyItemButton);
            itemButton.gameObject.BindEvent(MyItemPopup);

            var helpButton = GetButton((int)LobbySceneButton.HelpButton);
            helpButton.gameObject.BindEvent(ClickHelpButton);

            Bind<RawImage>(typeof(LobbySceneRawImage));
            var profileImage = GetRawImage((int)LobbySceneRawImage.UserProfileImage);
            await UniTask.WaitUntil(() => UserDataManager.Instance.MyProfileImage != null);
            profileImage.texture = UserDataManager.Instance.MyProfileImage;

            UserDataManager.Instance.ChangeMyProfileImageCallback += ChangeProfileImage;
        }

        private void StartGame(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(41);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var gameStartButtonObj = GetButton((int)LobbySceneButton.GameStartButton).gameObject;
            gameStartButtonObj.SetActive(false);
            UIManager.Instance.ShowUI<UIModeSelect>("ModeSelectUI", () =>
            {
                gameStartButtonObj.SetActive(true);
            });
        }

        /// <summary>
        /// 랭킹 팝업 UI 오픈 함수
        /// </summary>
        private void ShowRankingPopup(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(13);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            UIManager.Instance.ShowUI<UIRanking>("RankingPopupUI");
        }

        private void ShowUserProfilePopup(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(1);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            UIManager.Instance.ShowUserProfile(UserDataManager.Instance.MyData);
        }

        private void ShowShopPopup(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(17);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            UIManager.Instance.ShowUI<UIShop>("ShopPopupUI");
        }

        private void MyItemPopup(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(23);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            UIManager.Instance.ShowUI<UIMyItem>("MyItemPopupUI");
        }

        private void ClickHelpButton(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(28);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            UIManager.Instance.ShowUI<UILobbyHelpPopup>("LobbyHelpPopupUI");
        }

        public GameObject StartButtonObject() => GetButton((int)LobbySceneButton.GameStartButton).gameObject;

        private void ChangeProfileImage()
        {
            var profileImage = GetRawImage((int)LobbySceneRawImage.UserProfileImage);
            profileImage.texture = UserDataManager.Instance.MyProfileImage;
        }

        private void OnDestroy()
        {
            if (UserDataManager.HasInstance)
                UserDataManager.Instance.ChangeMyProfileImageCallback -= ChangeProfileImage;
        }
    }
}
