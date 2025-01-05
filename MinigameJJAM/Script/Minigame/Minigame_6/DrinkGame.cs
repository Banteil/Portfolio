using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;


namespace starinc.io
{
    public class DrinkGame : MinigameBase
    {
        #region Cache
        private const float MIN_TIMER = 1.6f;
        private const float COMBO_TIMER = 2f;
        private const float DECREASE_TIMER = 0.02f;
        private const int COMBO_SCORE = 50;

        private const string HEART_FILLED = "heart_filled";
        private const string HEART_EMPTY = "heart_empty";
        private const float SPAWN_WIDTH = 10f;
        private const float SPAWN_HEIGHT = 18f;

        private enum DrinkGameImage
        {
            Gauge,
        }

        private enum DrinkGameCanvasGroup
        {
            HeartUI,
            GaugeUI,
        }

        private enum DrinkGameText
        {
            ComboText,
        }

        [SerializeField]
        private DrinkTable _drinkTable;
        [SerializeField]
        private PhaseTable _phaseTable;
        [SerializeField]
        private PhaseInfoUI _phaseInfoUI;
        [SerializeField]
        private Camera _drinkCamera;

        private Drink _currentDrink;

        private int _currentPhase = 0;
        private float _maxTimer = 5f;
        private float _currentTimer = 0f;

        private Vector3 _spwanPosition;
        private Vector2 _startPosition;
        private bool _isSwiping = false;

        private int _hp = 3;
        public int HP
        {
            get { return _hp; }
            set
            {
                _hp = value;
                ChangedHP(_hp);
                if (_hp <= 0)
                    EndProcess();
            }
        }
        private List<Image> _hearts = new List<Image>();
        private int _combo = 0;

        private Action _timerProcess;
        private Tween _comboTween;
        #endregion

        public override void Initialization()
        {
            Bind<Image>(typeof(DrinkGameImage));
            Bind<CanvasGroup>(typeof(DrinkGameCanvasGroup));
            Bind<TextMeshProUGUI>(typeof(DrinkGameText));

            Bind<TextMeshProUGUI>(typeof(DrinkGameText));
            var comboText = GetText((int)DrinkGameText.ComboText);
            comboText.alpha = 0;

            var heartUI = Get<CanvasGroup>((int)DrinkGameCanvasGroup.HeartUI).transform;
            for (int i = 0; i < heartUI.childCount; i++)
            {
                var heartImage = heartUI.GetChild(i).GetComponent<Image>();
                _hearts.Add(heartImage);
            }

            _spwanPosition = _drinkCamera.transform.position;
            _spwanPosition.y = 0;
            _spwanPosition.z = 0;

            _currentTimer = _maxTimer;
        }

        public override void StartProcess()
        {
            base.StartProcess();
            var heartUI = Get<CanvasGroup>((int)DrinkGameCanvasGroup.HeartUI);
            heartUI.alpha = 1;
            var gaugeUI = Get<CanvasGroup>((int)DrinkGameCanvasGroup.GaugeUI);
            gaugeUI.alpha = 1;
            CheckPhase();
        }

        private void CheckPhase()
        {
            var phase = _phaseTable.GetPhaseData(_score);
            if (phase < 1 || _currentPhase == phase)
            {
                SpawnDrink();
            }
            else
            {
                _currentPhase = phase;
                NextPhaseDirection();
            }
        }

        private void NextPhaseDirection()
        {
            var phaseDrinks = _drinkTable.GetPhaseDrinks(_currentPhase);
            _phaseInfoUI.SetInfos(phaseDrinks);
            var rectPhaseInfo = (RectTransform)_phaseInfoUI.transform;
            var canvasTr = (RectTransform)transform;
            var posX = (rectPhaseInfo.rect.width * 0.5f) + (canvasTr.rect.width * 0.5f);
            rectPhaseInfo.anchoredPosition = new Vector3(-posX, 0f);
            var phaseCanvasGroup = _phaseInfoUI.GetComponent<CanvasGroup>();
            phaseCanvasGroup.alpha = 1f;

            Sequence sequence = DOTween.Sequence();
            sequence
                .Append(rectPhaseInfo.DOAnchorPosX(0, 0.5f).SetEase(Ease.InOutQuad))
                .AppendCallback(() =>
                {
                    Manager.Sound.PlaySFX("m6sfx_order");
                })
                .AppendInterval(3f)
                .Append(rectPhaseInfo.DOAnchorPosX(posX, 0.5f).SetEase(Ease.InOutQuad))
                .OnComplete(() =>
                {
                    phaseCanvasGroup.alpha = 0f;
                    SpawnDrink();
                });
        }

        private void SpawnDrink()
        {
            var spwanPos = _spwanPosition + new Vector3(SPAWN_WIDTH, 0);
            var obj = _drinkTable.CreateDrink(_currentPhase, ReleaseDrink, spwanPos);
            _currentDrink = obj.GetComponent<Drink>();
            obj.transform.DOMove(_spwanPosition, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _currentDrink.ReadyDrink();
                    Manager.Input.OnClickEvent += ClickEvent;
                    
                    ResetTimer();
                    _timerProcess = CheckTimer;
                });
        }

        private void ClickEvent(InputAction.CallbackContext context)
        {
            SwipeEvent(context);
            TouchEvent(context);
            _isSwiping = false;
        }

        private void TouchEvent(InputAction.CallbackContext context)
        {
            if (_isSwiping) return;

            if (_currentDrink.CurrentIceCount >= 15)
            {
                ReleaseDrink();
                return;
            }

            Vector2 touchPosition = Vector2.zero;
            bool isInputValid = false;
#if UNITY_EDITOR
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    touchPosition = Mouse.current.position.ReadValue();
                    isInputValid = true;
                }
            }
#else
            if (Touchscreen.current != null)
            {
                // 터치가 Ended 단계일 때만 처리
                var touch = Touchscreen.current.primaryTouch;
                Debug.Log($"TouchEvent : {touch.phase.ReadValue()}");
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    touchPosition = touch.position.ReadValue();
                    isInputValid = true;
                }
            }
#endif
            if (isInputValid && Util.IsPointerWithinScreenBounds(touchPosition))
            {
                if (Util.IsPointerOverUI(touchPosition, true)) return;
                SpwanIce();
            }
        }

        private void SpwanIce()
        {
            var spwanPos = _spwanPosition + new Vector3(Random.Range(-1f, 1f), SPAWN_HEIGHT);
            float randomZRotation = Random.Range(0f, 360f);
            Quaternion randomRotation = Quaternion.Euler(0f, 0f, randomZRotation);

            var iceObj = _gameObjectTable.GetPrefabObject("Ice", spwanPos, randomRotation);
            var iceRenderer = iceObj.GetComponent<SpriteRenderer>();
            iceRenderer.sprite = _spriteTable.GetSprite($"ice_{Random.Range(1, 6)}");
            iceObj.transform.SetParent(_currentDrink.Ices);

            _currentDrink.CurrentIceCount++;
        }

        private void SwipeEvent(InputAction.CallbackContext context)
        {
            // 터치 또는 마우스 포지션 변수 초기화
            Vector2 touchPosition = Vector2.zero;

#if UNITY_EDITOR
            // 마우스 입력 처리
            if (Mouse.current != null)
            {
                // 마우스 왼쪽 버튼이 눌리는 순간 (Input Down)
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    touchPosition = Mouse.current.position.ReadValue();
                    if (Util.IsPointerWithinScreenBounds(touchPosition) && !Util.IsPointerOverUI(touchPosition, true))
                    {
                        _startPosition = touchPosition;
                    }
                }

                // 마우스 왼쪽 버튼이 떼지는 순간 (Input Up)
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    touchPosition = Mouse.current.position.ReadValue();
                    HandleSwipe(touchPosition);
                }
            }
#else
            // 터치 입력 처리
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;

                // 터치 시작 (TouchPhase.Began)
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    touchPosition = touch.position.ReadValue();
                    if (Util.IsPointerWithinScreenBounds(touchPosition) && !Util.IsPointerOverUI(touchPosition, true))
                    {
                        _startPosition = touchPosition;
                    }
                }

                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    touchPosition = touch.position.ReadValue();
                    HandleSwipe(touchPosition);                  
                }
            }
#endif
        }

        private void HandleSwipe(Vector2 endPosition)
        {            
            if (!Util.IsPointerWithinScreenBounds(endPosition) || Util.IsPointerOverUI(endPosition, true))
                return;

            Vector2 swipeDelta = endPosition - _startPosition;
            if (swipeDelta.x < -80f) //&& Mathf.Abs(swipeDelta.y) < 50
            {
                ReleaseDrink();
                _isSwiping = true;
            }
        }

        private void ReleaseDrink()
        {
            Manager.Input.OnClickEvent -= ClickEvent;
            _timerProcess = null;

            // 화면의 왼쪽 밖 좌표 계산
            Manager.Sound.PlaySFX("m6sfx_release");
            Vector3 leftOutsidePosition = _spwanPosition - new Vector3(SPAWN_WIDTH, 0);
            _currentDrink.transform.DOMove(leftOutsidePosition, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    Destroy(_currentDrink.gameObject);
                    if (_currentDrink.IsCorrectIceCount && _currentTimer > 0)
                    {
                        Manager.Sound.PlaySFX("correct");
                        Score += _currentDrink.Score + (COMBO_SCORE * _combo);
                        if (_maxTimer - _currentTimer <= COMBO_TIMER)
                        {
                            ComboProcess();
                            _combo++;
                        }
                        else
                            _combo = 0;
                    }
                    else
                    {
                        Manager.Sound.PlaySFX("miss");
                        HP--;
                        _combo = 0;
                    }

                    if (HP > 0)
                        CheckPhase();
                });
        }

        private void ComboProcess()
        {
            if (_comboTween != null)
            {
                _comboTween.Kill();
                _comboTween = null;
            }

            var comboText = GetText((int)DrinkGameText.ComboText);
            comboText.alpha = 0;
            if (_combo > 0)
            {
                comboText.text = $"{_combo} Combo!";

                var sequence = DOTween.Sequence();
                sequence.Append(comboText.DOFade(1f, 0.5f))
                        .AppendInterval(1f)
                        .Append(comboText.DOFade(0f, 0.5f))
                        .OnComplete(() =>
                        {
                            _comboTween = null;
                        });
                _comboTween = sequence;
            }
        }

        private void CheckTimer()
        {
            _currentTimer -= Time.deltaTime;
            var gauge = GetImage((int)DrinkGameImage.Gauge);
            gauge.fillAmount = _currentTimer / _maxTimer;

            if (_currentTimer <= 0)
            {
                ReleaseDrink();
            }
        }

        private void ResetTimer()
        {
            _maxTimer -= DECREASE_TIMER;
            if (_maxTimer < MIN_TIMER)
                _maxTimer = MIN_TIMER;
            _currentTimer = _maxTimer;
            var gauge = GetImage((int)DrinkGameImage.Gauge);
            gauge.fillAmount = _currentTimer / _maxTimer;
        }

        protected override void UpdateProcess()
        {
            _timerProcess?.Invoke();
        }

        public override void EndProcess()
        {
            base.EndProcess();
        }

        private void ChangedHP(int hp)
        {
            for (int i = 0; i < _hearts.Count; i++)
            {
                var sprite = _spriteTable.GetSprite(i < hp ? HEART_FILLED : HEART_EMPTY);
                _hearts[i].sprite = sprite;
            }
        }
    }
}
