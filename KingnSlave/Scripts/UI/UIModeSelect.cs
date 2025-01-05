using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIModeSelect : UIPopup
    {
        enum ModeSelectButton
        {
            SinglePracticeModeButton = 1,
            SingleStoryModeButton,
            NormalModeButton,
            RankModeButton,
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<Button>(typeof(ModeSelectButton));
            var singlePracticeButton = GetButton((int)ModeSelectButton.SinglePracticeModeButton);
            singlePracticeButton.gameObject.BindEvent(StartSinglePracticeMode);

            var singleStoryButton = GetButton((int)ModeSelectButton.SingleStoryModeButton);
            singleStoryButton.gameObject.BindEvent(StartSingleStoryMode);

            var normalButton = GetButton((int)ModeSelectButton.NormalModeButton);
            normalButton.gameObject.BindEvent(StartNormalMode);

            var rankButton = GetButton((int)ModeSelectButton.RankModeButton);
            rankButton.gameObject.BindEvent(StartRankMode);
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            GameManager.Instance.CurrentGameMode = Define.GamePlayMode.None;
            UIManager.Instance.CloseUI(prevUICallback);
        }

        private void StartSinglePracticeMode(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(42);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            GameManager.Instance.SingleMode = Define.SingleFirstTurnMode.Random;
            CheckGameMode(Define.GamePlayMode.Practice);
        }

        private void StartSingleStoryMode(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(43);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            GameManager.Instance.SingleMode = Define.SingleFirstTurnMode.Slave;
            CheckGameMode(Define.GamePlayMode.SingleStory);
        }

        private void StartNormalMode(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(44);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            CheckGameMode(Define.GamePlayMode.PVPNormal);
        }

        async private void StartRankMode(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(45);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            await CallAPI.APICheckUserRankPrecondition(UserDataManager.Instance.MySid, (data) =>
            {
                if (data.returnCd == 0)
                    CheckGameMode(Define.GamePlayMode.PVPRank);
                else
                {
                    var currnet = Util.GetLocalizationTableString(Define.InfomationLocalizationTable, "currentRankedGameConditions");
                    var remain = Util.GetLocalizationTableString(Define.InfomationLocalizationTable, "remainRankedGameConditions");
                    var game = Util.GetLocalizationTableString(Define.CommonLocalizationTable, "gamesText");
                    var str = $"[{currnet}]\n{data.normal_total} / {data.rank_precondition} {game}\n[{remain}]\n{data.remain_count} {game}";
                    UIManager.Instance.ShowWarningUI(str, false);
                }
            });

        }

        async private void CheckGameMode(Define.GamePlayMode gameMode = Define.GamePlayMode.PVPNormal)
        {
            if (gameMode == Define.GamePlayMode.None) return;

            UIManager.Instance.CloseUI();            
            GameManager.Instance.CurrentGameMode = gameMode;

            Debug.Log("curr " + gameMode);
            if (GameManager.Instance.IsNetworkGameMode())
                UIManager.Instance.ShowUI<UIMatch>("MatchSideUI");
            else
            {
                if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.SingleStory)
                {
                    var singleDirection_singleGame = ScreenTransitionManager.Instance.ShowDirection<SingleSceneLoadDirection_SingleStory>();
                    await singleDirection_singleGame.StartDirection();
                }
                else
                {
                    GameManager.Instance.PracticeGameWinProbPercent = Define.DEFAULT_WIN_PROBABILITY_PERCENT;
                    var singleDirection = ScreenTransitionManager.Instance.ShowDirection<SingleSceneLoadDirection>();
                    singleDirection.SetTipData();
                    await singleDirection.StartDirection();
                }

                // 내 카드 스킨 설정
                await NetworkManager.Instance.SetMyCardSkin();

                GameManager.Instance.LoadScene(Define.SingleGameSceneName);
            }
        }

        public override void InputEscape() => OnCloseButtonClicked(null);
    }
}
