using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class GameOverPopupUI : PopupUI
    {
        #region Cache
        private enum GameOverButton
        {
            RetryButton,
            ExitButton,
        }

        private enum GameOverText
        {
            GameTitleText,
            HighScoreText,
            CurrentScoreText,
        }

        private enum GameOverRectTransform
        {
            Frame,
        }

        private MinigameSceneUI _gameScene { get { return Manager.UI.SceneUI as MinigameSceneUI; } }
        #endregion

        protected override void BindInitialization()
        {
            Bind<Button>(typeof(GameOverButton));
            Bind<TextMeshProUGUI>(typeof(GameOverText));
            Bind<RectTransform>(typeof(GameOverRectTransform));

            var retryButton = GetButton((int)GameOverButton.RetryButton);
            retryButton.gameObject.BindEvent(OnRetryButton);
            retryButton.gameObject.SetActive(false);
            var exitButton = GetButton((int)GameOverButton.ExitButton);
            exitButton.gameObject.BindEvent(OnExitButton);
            exitButton.gameObject.SetActive(false);

            var gameInfo = Manager.Game.GetCurrentMinigameEntry();
            var highScore = Manager.Game.Scores.GetScoreByAddress(gameInfo.address);

            var gameTitle = GetText((int)GameOverText.GameTitleText);
            gameTitle.text = gameInfo.name;
            var highScoreText = GetText((int)GameOverText.HighScoreText);
            highScoreText.text = highScore.ToString("#,##0");
            var currentScore = GetText((int)GameOverText.CurrentScoreText);
            currentScore.text = _gameScene.GetCurrentScore().ToString("#,##0");

            var frame = Get<RectTransform>((int)GameOverRectTransform.Frame);
            frame.anchoredPosition = new Vector2(frame.anchoredPosition.x, (RectTr.sizeDelta.y * 0.5f) + (frame.sizeDelta.y * 0.5f));
            frame
                .DOAnchorPosY(0f, 1f)
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    retryButton.gameObject.SetActive(true);
                    exitButton.gameObject.SetActive(true);
                });
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            Manager.UI.DisableEscape = true;
            if(Time.timeScale == 0f)
                Util.UnPause();
        }

        #region BindEvnet   
        private void OnRetryButton(PointerEventData data)
        {
            Manager.Ad.ShowInterstitialAd(() =>
            {
                Manager.Sound.PlayOneShotSFX("changeScene");
                Manager.Load.ReloadCurrentScene();
            });
        }

        private void OnExitButton(PointerEventData data)
        {
            Manager.Ad.ShowInterstitialAd(() =>
            {
                Manager.Sound.PlayOneShotSFX("changeScene");
                Manager.Load.SceneLoad(Define.LOBBY_SCENE_NAME, SceneLoadType.Async);
            });
        }
        #endregion

        protected override void OnDestroy()
        {
            Manager.UI.DisableEscape = false;
        }
    }
}
