using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class MinigameSceneUI : SceneUI
    {
        #region Cache
        [SerializeField]
        private MinigameBase _data;
        public MinigameBase Data {  get { return _data; } }

        private enum MinigameSceneText
        {
            ControlInfoText,
            ScoreText,
        }

        private enum MinigameSceneButton
        {
            PauseButton,
        }
        #endregion

        #region Callback
        public event Action OnGameStart;
        #endregion

        protected override void OnAwake()
        {
            Debug.Log($"GameScene {gameObject.GetInstanceID()} Awake");
            SceneInitialization();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Debug.Log($"GameScene {gameObject.GetInstanceID()} Destroy");
        }

        protected override void SceneInitialization()
        {
            base.SceneInitialization();
            _isAddressableScene = true;
        }

        protected override void BindInitialization()
        {
            _data ??= FindAnyObjectByType<MinigameBase>();
            if (_data == null)
            {
                Debug.LogError("MinigameBaseData is Missing");
                Manager.Load.SceneLoad(Define.LOBBY_SCENE_NAME, SceneLoadType.Async);
                return;
            }

            _data.Initialization();
            _data.OnChangedScore += UpdateScore;
            _data.OnEndGame += ReleaseInputEvent;

            Bind<TextMeshProUGUI>(typeof(MinigameSceneText));
            Bind<Button>(typeof(MinigameSceneButton));            
        }

        protected override void OnStart()
        {            
            ControlInformationPlayback();
        }

        protected async void ControlInformationPlayback()
        {
            var controlInfoText = GetText((int)MinigameSceneText.ControlInfoText);
            var textRectTr = (RectTransform)controlInfoText.transform;
            var offScreenSize = RectTr.rect.width * 0.5f + textRectTr.rect.width * 0.5f;
            textRectTr.anchoredPosition = new Vector2(-offScreenSize, textRectTr.anchoredPosition.y);

            await UniTask.WaitUntil(() => Manager.Game.CompletePreparedData);

            var currentEntry = Manager.Game.GetCurrentMinigameEntry();            
            controlInfoText.text = currentEntry.controlInfo;

            textRectTr
                .DOAnchorPosX(0, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    Manager.Sound.PlaySFX("gameInfo");
                textRectTr.DOScale(Vector3.one * 1.5f, 0.5f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        textRectTr.DOScale(Vector3.one, 0.5f)
                            .SetEase(Ease.InQuad)
                            .OnComplete(() => {
                                DOVirtual.DelayedCall(0.2f, () => {
                                    textRectTr.DOAnchorPosX(offScreenSize, 0.5f)
                                        .SetEase(Ease.InQuad)
                                        .OnComplete(() => {
                                            controlInfoText.gameObject.SetActive(false);
                                            Manager.Sound.PlaySFX("gameStart");
                                            DOVirtual.DelayedCall(1f, () => {
                                                GameStart();
                                            });
                                        });
                                });
                            });
                    });
            });
        }

        protected void GameStart()
        {
            Manager.UI.OnEscape += EscapeAction;
            var pauseButton = GetButton((int)MinigameSceneButton.PauseButton);
            pauseButton.gameObject.BindEvent(OnPauseButton);
                        
            _data.StartProcess();
            OnGameStart?.Invoke();
        }

        protected void ReleaseInputEvent()
        {
            Manager.UI.OnEscape -= EscapeAction;
            var pauseButton = GetButton((int)MinigameSceneButton.PauseButton);
            pauseButton.gameObject.RemoveEvent();
            _data.OnEndGame -= ReleaseInputEvent;
        }

        protected override void EscapeAction()
        {
            OnPauseButton(null);
        }

        public int GetCurrentScore() => _data.Score;

        #region BindEvent
        protected virtual void OnPauseButton(PointerEventData data)
        {
            if (_data.IsGameOver) return;
            var soundManager = Manager.Sound;
            soundManager.PauseBGM();
            soundManager.PauseAllSFX();
            Manager.UI.ShowPopupUI<PausePopupUI>();
            Time.timeScale = 0f;
        }

        private void UpdateScore(int score)
        {
            var scoreText = GetText((int)MinigameSceneText.ScoreText);
            scoreText.text = score.ToString("#,##0");
        }
        #endregion

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                OnPauseButton(null);
            }
        }
    }
}
