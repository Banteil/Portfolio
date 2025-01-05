using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace starinc.io
{
    public class CookingGame : MinigameBase
    {
        #region Cache        
        private enum CookingGameText
        {
            TimerText,
            InfoText,
        }
        private float _timer = 60f;
        public float Timer
        {
            get { return _timer; }
            set
            {
                _timer = value;
                GetText((int)CookingGameText.TimerText).text = _timer.ToString("F0");
            }
        }
        private int _combo = 0;

        private Vector3 _baseAcceleration = new Vector3(0, 0, -1f);

        private Vector3 _acceleration, _previousAcceleration = Vector3.zero;
        private float _pitch = 0f;

        [SerializeField]
        private FoodDataTable _foodDataTable;
        [SerializeField]
        private Food _currentFood;
        [SerializeField]
        private AccelerometerWrapperScriptable _accel;
        [SerializeField]
        private AttitudeSensorWrapperScriptable _attitudeSensor;

        [SerializeField] private float _snapThreshold = 1f;
        [SerializeField] private float _deltaPitchThreshold = -0.4f;
        #endregion

        public override async void Initialization()
        {
            Bind<TextMeshProUGUI>(typeof(CookingGameText));
            var infoText = GetText((int)CookingGameText.InfoText);
            Timer = _timer;
            _currentFood.OnCooked += CheckCookingState;
            _currentFood.gameObject.SetActive(false);

            await _accel.InitializeAsync();
            _accel.Enable(true);
            await _attitudeSensor.InitializeAsync();
            _attitudeSensor.Enable(true);
        }

        private bool CheckRequiredInputs()
        {
            Debug.Log($"_accel.IsAvailable : {_accel.IsAvailable}, _attitudeSensor.IsAvailable : {_attitudeSensor.IsAvailable}");
            if (!_accel.IsAvailable || !_attitudeSensor.IsAvailable)
            {
                var message = Util.GetLocalizedString(Define.LOCALIZATION_TABLE_MESSAGE, "requiredInputs");
                Manager.UI.ShowMessage(message);
                EndProcess();
                return false;
            }
            return true;
        }

        private void OnDestroy()
        {
            _accel.Enable(false);
            _attitudeSensor.Enable(false);
            _currentFood.OnCooked -= CheckCookingState;
        }

        public override void StartProcess()
        {
            if (!CheckRequiredInputs()) return;
            base.StartProcess();
            _currentFood.gameObject.SetActive(true);
            SpawnFood();
        }

        protected override void Update()
        {
            if (!_attitudeSensor.IsEnabled) _attitudeSensor.Enable(true);
            if (!_accel.IsEnabled) _accel.Enable(true);
            base.Update();
        }

        protected override void UpdateProcess()
        {
            TimeCheck();
            SensorInputProcess();
            _currentFood?.Cooking();
        }

        private void TimeCheck()
        {
            Timer -= Time.deltaTime;
            if (Timer <= 0)
            {
                Timer = 0;
                EndProcess();
            }
        }

        private void SensorInputProcess()
        {
            _previousAcceleration = _acceleration;
            _acceleration = _accel.ReadValue();
            var attitude = _attitudeSensor.ReadValue();
            _pitch = attitude.y;

            if (_currentFood.IsInteraction) return;
            MovementFood();
            FlipFood();
        }

        private void MovementFood()
        {
            Vector2 moveVelocity = new Vector2(
                (_acceleration.x - _baseAcceleration.x),
                (_acceleration.y - _baseAcceleration.y)
            );
            _currentFood?.MovementFood(moveVelocity);
        }

        private void FlipFood()
        {
            var deltaAcceleration = _acceleration - _previousAcceleration;
            var snapForce = Mathf.Abs(deltaAcceleration.magnitude);
            Debug.Log($"snapForce: {snapForce}, _pitch: {_pitch}");
            if (snapForce > _snapThreshold && Mathf.Abs(_pitch) > _deltaPitchThreshold)
            {
                _currentFood?.FlipOver();
            }
        }

        private void SpawnFood()
        {
            _currentFood.Spwan(() =>
            {
                var foodData = _foodDataTable.GetRandomFoodData();
                _currentFood.SetFoodData(foodData);
            });
        }

        private void RemoveFood()
        {
            _currentFood.Remove(async () =>
            {
                await UniTask.Yield();
                SpawnFood();
            });
        }

        private void CheckCookingState(Dictionary<CookingSide, CookingStateData> sideData)
        {
            var topState = sideData[CookingSide.Top].state;
            var bottomState = sideData[CookingSide.Bottom].state;
            var data = _currentFood.CurrentData;

            if (topState == CookingState.Cook && bottomState == CookingState.Cook)
            {
                Manager.Sound.PlaySFX("correct");
                Score += data.Score + (_combo * 100);
                var comboTimer = _combo > data.TimerIncrease ? data.TimerIncrease : _combo;
                var increase = data.TimerIncrease + comboTimer;
                Timer += increase;
                _combo++;
                ShowComboText(increase);
                RemoveFood();
            }
            else if (topState == CookingState.Over || bottomState == CookingState.Over)
            {
                Manager.Sound.PlaySFX("miss");
                _combo = 0;
                RemoveFood();
            }
        }

        private void ShowComboText(float increase)
        {
            var infoText = GetText((int)CookingGameText.InfoText);
            infoText.text = $"+ {increase} Second\n{_combo} Combo!";
            Sequence sequence = DOTween.Sequence();
            sequence.Append(infoText.DOFade(1, 0f));
            sequence.Append(infoText.transform.DORotate(new Vector3(0, 0, 10f), 0.2f)
                .SetEase(Ease.InOutQuad).SetLoops(5, LoopType.Yoyo));
            sequence.Append(infoText.DOFade(0, 0.5f));
            sequence.Play();
        }

        public override void EndProcess()
        {
            base.EndProcess();
        }
    }
}
