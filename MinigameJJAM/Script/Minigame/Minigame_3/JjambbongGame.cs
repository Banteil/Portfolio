using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace starinc.io
{
    public class JjambbongGame : MinigameBase
    {
        #region Localization Key
        private const string ACTION_READY = "surveillance_ready";
        private const string ACTION_START = "surveillance_";
        private const string ACTION_END = "surveillance_end";
        private const string DETECTION = "detection";
        private const string WARNING = "warning_";
        #endregion

        #region Cache
        private const int KNIT_SCORE = 100;
        private const float WARNING_TIME = 3f;

        private enum JjambbongGameButton
        {
            SwitchButton,
            KnitButton,
        }

        [SerializeField]
        private LocalizationDatabase _localizationDB;

        [SerializeField]
        private SpeechBubble _speechBubble;

        [SerializeField]
        private Hands _hands;

        private int _lifeCount = 3;
        public int LifeCount
        {
            get { return _lifeCount; }
            set
            {
                var prevLifeCount = _lifeCount;
                _lifeCount = value;
                if (prevLifeCount > _lifeCount && _lifeCount >= 0)
                {
                    PlaySFX("miss");
                    var message = _localizationDB.GetLocalizedString($"{WARNING}{prevLifeCount}");
                    _speechBubble.Speak(message, true);
                }
                else if (_lifeCount < 0)
                {
                    EndProcess();
                }
            }
        }

        [SerializeField]
        private float _hideTime = 1f;
        private float _hidingTimer = 0f;

        #region Watcher
        [SerializeField]
        private float _minDetectionTime = 5f;
        [SerializeField]
        private float _maxDetectionTime = 10f;
        [SerializeField]
        private float _warningDetectionTime = 1.6f;
        private enum WatcherActMode
        {
            IDLE,
            DETECT_READY,
            DETECTING,
            DETECT_END,
        }
        private WatcherActMode _mode = WatcherActMode.IDLE;
        private float _detectionTime;
        private float _modeChangeTimer = 0f;
        private float _noActionWarningTimer = 0f;
        #endregion
        #endregion

        public override void Initialization()
        {
            Bind<Button>(typeof(JjambbongGameButton));
            var switchButton = GetButton((int)JjambbongGameButton.SwitchButton);
            switchButton.gameObject.BindEvent(SwitchHand);
            var knitButton = GetButton((int)JjambbongGameButton.KnitButton);
            knitButton.gameObject.BindEvent(Knitting);
            ActiveButtons(false);

            _hands.OnKnittingSuccess += KnittingSuccess;
            _hands.OnKnittingFail += KnittingFail;
            _hands.OnSwitchSuccess += SwitchSuccess;
            _hands.OnSwitchFail += SwitchFail;

            _localizationDB.OnLocaleChanged(LocalizationSettings.SelectedLocale);
            LocalizationSettings.SelectedLocaleChanged += _localizationDB.OnLocaleChanged;
        }

        public override void StartProcess()
        {
            base.StartProcess();
            ActiveButtons(true);

            _detectionTime = UnityEngine.Random.Range(_minDetectionTime, _maxDetectionTime);
        }

        private void ActiveButtons(bool active)
        {
            var switchButton = GetButton((int)JjambbongGameButton.SwitchButton);
            switchButton.gameObject.SetActive(active);
            var knitButton = GetButton((int)JjambbongGameButton.KnitButton);
            knitButton.gameObject.SetActive(active);
        }

        protected override void UpdateProcess()
        {
            Hiding();
            WatcherActionProcess();
        }

        private void Hiding()
        {
            if (_hands.IsHiding) return;
            if (_hands.IsKnitting)
            {
                _hidingTimer = 0f;
                return;
            }

            _hidingTimer += Time.deltaTime;
            if (_hidingTimer >= _hideTime)
            {
                _hands.HideHands();
                _hidingTimer = 0;
            }
        }

        #region Watcher Action
        private void WatcherActionProcess()
        {
            switch (_mode)
            {
                case WatcherActMode.IDLE:
                    WatcherIdleAction();
                    break;
                case WatcherActMode.DETECT_READY:
                    WatcherDetectReadyAction();
                    break;
                case WatcherActMode.DETECTING:
                    WatcherDetectingAction();
                    break;
                case WatcherActMode.DETECT_END:
                    WatcherDetectingEndAction();
                    break;
            }
        }

        private bool WatcherNoActionCheckAction()
        {
            _noActionWarningTimer += Time.deltaTime;
            if (_noActionWarningTimer >= WARNING_TIME)
            {
                LifeCount--;
                _noActionWarningTimer = 0;
                _modeChangeTimer = 0;
                return true;
            }
            return false;
        }

        private void WatcherIdleAction()
        {
            if (WatcherNoActionCheckAction()) return;
            _modeChangeTimer += Time.deltaTime;

            if (_modeChangeTimer >= _detectionTime - _warningDetectionTime)
            {
                var message = _localizationDB.GetLocalizedString(ACTION_READY);
                _speechBubble.Speak(message, false);
                _noActionWarningTimer = 0f;
                _mode = WatcherActMode.DETECT_READY;
            }
        }

        private void WatcherDetectReadyAction()
        {
            _modeChangeTimer += Time.deltaTime;
            if (_modeChangeTimer >= _detectionTime)
            {
                if (_hands.IsHiding)
                {
                    var rand = UnityEngine.Random.Range(1, 3);
                    var message = _localizationDB.GetLocalizedString($"{ACTION_START}{rand}");
                    _speechBubble.Speak(message, true);
                    _modeChangeTimer = 0f;
                    _noActionWarningTimer = 0f;
                    _mode = WatcherActMode.DETECTING;
                }
                else
                    EndProcess();
            }
        }

        private void WatcherDetectingAction()
        {
            if (_hands.IsHiding)
            {
                _modeChangeTimer += Time.deltaTime;
                if (_modeChangeTimer >= 1f)
                {
                    var message = _localizationDB.GetLocalizedString(ACTION_END);
                    _speechBubble.Speak(message, true);
                    _mode = WatcherActMode.DETECT_END;
                }
            }
            else
                EndProcess();
        }

        private void WatcherDetectingEndAction()
        {
            if (_hands.IsHiding)
            {
                _modeChangeTimer += Time.deltaTime;
                if (_modeChangeTimer >= 2f)
                {
                    _modeChangeTimer = 0f;
                    _detectionTime = UnityEngine.Random.Range(_minDetectionTime, _maxDetectionTime);
                    _mode = WatcherActMode.IDLE;
                }
            }
            else
                EndProcess();
        }
        #endregion

        public override async void EndProcess()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            _onUpdateProcess -= UpdateProcess;
            OnEndGame?.Invoke();
            CheckHighScore();

            ActiveButtons(false);

            Manager.Sound.StopBGM(0.5f);
            Manager.Sound.StopAllSFX(0.5f);

            PlaySFX("m3sfx_detect");
            _hands.StopHands();
            var message = _localizationDB.GetLocalizedString(DETECTION);
            _speechBubble.Scream(message);            

            await UniTask.WaitForSeconds(2f);
            Manager.UI.ShowPopupUI<GameOverPopupUI>(null, "gameOver");
        }


        #region BindEvent
        private void SwitchHand(PointerEventData data)
        {
            if (_hands.IsHiding)
                _hands.ShowHands();

            if (_mode == WatcherActMode.DETECTING)
            {
                EndProcess();
                return;
            }

            _hands.SwitchHand();
        }

        private void Knitting(PointerEventData data)
        {
            if (_hands.IsHiding)
                _hands.ShowHands();

            if (_mode == WatcherActMode.DETECTING)
            {
                EndProcess();
                return;
            }

            _hands.Knitting();
        }
        #endregion

        #region Callback
        private void KnittingSuccess()
        {
            PlaySFX("correct");
            Score += KNIT_SCORE;
            _noActionWarningTimer = 0f;
        }

        private void KnittingFail()
        {
            LifeCount--;
            _noActionWarningTimer = 0f;
        }

        private void SwitchSuccess()
        {
            PlaySFX("m3sfx_switch");
            _noActionWarningTimer = 0f;
        }

        private void SwitchFail()
        {
            LifeCount--;
            _noActionWarningTimer = 0f;
        }
        #endregion

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= _localizationDB.OnLocaleChanged;
        }
    }
}
