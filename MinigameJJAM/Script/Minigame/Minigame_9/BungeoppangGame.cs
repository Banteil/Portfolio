using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public interface IBungeoppang
    {
        public enum BungeoppangTool
        {
            Frame,
            Display,
            Wrapper,            
        }

        public BungeoppangTool ToolType { get; set; }
        public bool IsDrag { get; set; }
        public int Count { get; set; }
        public void Reset();
    }

    public class BungeoppangGame : MinigameBase
    {
        #region Cache
        private const float MAX_TIMER = 200f;
        private const int ORDER_FAILED_PENALTY = 100;
        private const float ORDER_SUCCESS_REWARD = 20f;

        public enum BungeoppangGameToggle
        {
            ButterToggle,
            BatterToggle,
            RedBeanToggle,
        }

        private enum BungeoppangGameButton
        {
            MoneyButton,
            WrapperButton,
        }

        private enum BungeoppangGameSlider
        {
            TimerGauge,
        }

        private BungeoppangGameToggle _currentToggle = BungeoppangGameToggle.ButterToggle;
        public BungeoppangGameToggle CurrentToggle { get { return _currentToggle; } }

        [SerializeField]
        private List<BungeoppangFrame> _frames;
        [SerializeField]
        private List<BungeoppangCustomer> _customers;
        [SerializeField]
        private BungeoppangDisplay _display;
        [SerializeField]
        private BungeoppangWrapper _wrapper;
        [SerializeField]
        private BungeoppangWrapperTray _wrapperTray;
        [SerializeField]
        private BungeoppangMoneyTray _moneyTray;

        private float _currentTimer = MAX_TIMER;
        public float CurrentTimer
        {
            get { return _currentTimer; }
            set
            {
                _currentTimer = value;
                var timeSlider = GetSlider((int)BungeoppangGameSlider.TimerGauge);
                var sliderValue = _currentTimer > MAX_TIMER ? MAX_TIMER : _currentTimer;
                timeSlider.value = sliderValue;
            }
        }
        #endregion

        #region Callback

        #endregion

        public override void Initialization()
        {
            base.Initialization();
            Bind<Toggle>(typeof(BungeoppangGameToggle));
            var butter = Get<Toggle>((int)BungeoppangGameToggle.ButterToggle);
            butter.gameObject.BindEvent(SelectButter);
            var batter = Get<Toggle>((int)BungeoppangGameToggle.BatterToggle);
            batter.gameObject.BindEvent(SelectBatter);
            var redBean = Get<Toggle>((int)BungeoppangGameToggle.RedBeanToggle);
            redBean.gameObject.BindEvent(SelectRedBean);
            Bind<Button>(typeof(BungeoppangGameButton));
            var moneyButton = GetButton((int)BungeoppangGameButton.MoneyButton);
            moneyButton.gameObject.BindEvent(MoneySettlement);
            var wrapperButton = GetButton((int)BungeoppangGameButton.WrapperButton);
            wrapperButton.gameObject.BindEvent(WrapperRefills);
            Bind<Slider>(typeof(BungeoppangGameSlider));
            var timeSlider = GetSlider((int)BungeoppangGameSlider.TimerGauge);
            timeSlider.maxValue = MAX_TIMER;
            timeSlider.minValue = 0f;
            timeSlider.value = MAX_TIMER;

            foreach (var frame in _frames)
            {
                frame.BaseGame = this;
            }
            foreach (var customer in _customers)
            {
                customer.BaseGame = this;
                customer.OnOrderSuccess += OrderSuccess;
                customer.OnOrderMistake += OrderFailed;
            }
        }

        public override void StartProcess()
        {
            base.StartProcess();
            _display.SetActionDisplay(true);
            _wrapper.SetActionWrapper(true);
            foreach (var frame in _frames)
            {
                frame.SetActionFrame(true);
            }
            foreach (var customer in _customers)
            {
                customer.SetActionDisplay(true);
                customer.Appear();
            }
        }

        protected override void UpdateProcess()
        {
            CheckTimer();
        }

        private void CheckTimer()
        {
            CurrentTimer -= Time.deltaTime;
            if (CurrentTimer <= 0) EndProcess();
        }

        public override void EndProcess()
        {
            if (IsGameOver) return;
            IsGameOver = true;

            _display.SetActionDisplay(false);
            _wrapper.SetActionWrapper(false);
            foreach (var frame in _frames)
            {
                frame.SetActionFrame(false);
            }
            foreach (var customer in _customers)
            {
                customer.SetActionDisplay(false);
                customer.Disapper();
            }

            _onUpdateProcess -= UpdateProcess;

            OnEndGame?.Invoke();
            CheckHighScore();

            Manager.Sound.StopBGM(0.5f);
            Manager.Sound.StopAllSFX(0.5f);
            Manager.UI.ShowPopupUI<GameOverPopupUI>(null, "gameOver");
        }

        #region Callback Event
        private void OrderSuccess(int requestCount)
        {
            Manager.Sound.PlaySFX("correct");
            _currentTimer += ORDER_SUCCESS_REWARD;
            if(_currentTimer > MAX_TIMER) _currentTimer = MAX_TIMER;
            CurrentTimer = _currentTimer;
            _moneyTray.PayMoney(requestCount);            
        }

        private void OrderFailed()
        {
            Manager.Sound.PlaySFX("miss");
            _score -= ORDER_FAILED_PENALTY;
            if (_score < 0) _score = 0;
            Score = _score;
        }
        #endregion

        #region Bind Event
        private void SelectButter(PointerEventData data)
        {
            Manager.Sound.PlaySFX("m9sfx_toggle");
            _currentToggle = BungeoppangGameToggle.ButterToggle;
        }

        private void SelectBatter(PointerEventData data)
        {
            Manager.Sound.PlaySFX("m9sfx_toggle");
            _currentToggle = BungeoppangGameToggle.BatterToggle;
        }

        private void SelectRedBean(PointerEventData data)
        {
            Manager.Sound.PlaySFX("m9sfx_toggle");
            _currentToggle = BungeoppangGameToggle.RedBeanToggle;
        }

        private void MoneySettlement(PointerEventData data) => Score += _moneyTray.ConvertToScore();

        private void WrapperRefills(PointerEventData data) => _wrapperTray.Reset();
        #endregion
    }
}
