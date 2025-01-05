using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace starinc.io
{
    public class CatGame : MinigameBase
    {
        #region Cache
        private const string HEART_FILLED = "heart_filled";
        private const string HEART_EMPTY = "heart_empty";

        private const int BASE_SCORE = 100;
        private const int COMBO_SCORE = 10;
        private const int MAX_COUNT = 6;
        private const int MAX_PHASE = 16;
        private const float ACTION_TIME_REDUCTION = 0.1f;

        private float _currentActionTime = 0f;
        private float _actionTime = 2f;

        private int _currentPatternCount = 0;
        private int _patternCount = 5;
        private int _phase = 1;

        private int _combo = 0;

        private int _hp = 3;
        public int HP
        {
            get { return _hp; }
            set
            {
                _hp = value;
                ChangedHP(_hp);
                if(_hp <= 0)
                    EndProcess();
            }
        }

        private Tween _comboTween;

        [SerializeField]
        private List<Animator> _objectAnimator = new List<Animator>();

        private List<Image> _hearts = new List<Image>();

        private enum CatGameText
        {
            ComboText,
        }

        private enum CatGameCanvasGroup
        {
            HeartUI,
        }
        #endregion

        public override void Initialization()
        {
            Bind<CanvasGroup>(typeof(CatGameCanvasGroup));
            var heartUI = Get<CanvasGroup>((int)CatGameCanvasGroup.HeartUI).transform;
            for (int i = 0; i < heartUI.childCount; i++)
            {
                var heartImage = heartUI.GetChild(i).GetComponent<Image>();
                _hearts.Add(heartImage);
            }

            Bind<TextMeshProUGUI>(typeof(CatGameText));
            var comboText = GetText((int)CatGameText.ComboText);
            comboText.alpha = 0;
        }

        public override void StartProcess()
        {
            base.StartProcess();
            Manager.Input.OnClickEvent += TouchEvent;
            var heartUI = Get<CanvasGroup>((int)CatGameCanvasGroup.HeartUI);
            heartUI.alpha = 1;
        }

        public override void EndProcess()
        {
            Manager.Input.OnClickEvent -= TouchEvent;
            base.EndProcess();
        }

        protected override void UpdateProcess()
        {
            CatActionEvent();
        }

        #region Input Action
        private void TouchEvent(InputAction.CallbackContext context)
        {
            Vector2 touchPosition = Vector2.zero;
            var isInputValid = false;
#if UNITY_EDITOR
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    touchPosition = Mouse.current.position.ReadValue();
                    isInputValid = true;
                }
            }
#else
            if (Touchscreen.current != null)
            {
                var primaryTouch = Touchscreen.current.primaryTouch;
                if (primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    touchPosition = primaryTouch.position.ReadValue();
                    isInputValid = true;
                }
            }
#endif
            if (isInputValid && Util.IsPointerWithinScreenBounds(touchPosition))
            {
                if (Util.IsPointerOverUI(touchPosition, true)) return;

                int index = GetIndexFromTouchPosition(touchPosition);
                CleaningAction(index);
            }
        }

        private int GetIndexFromTouchPosition(Vector2 position)
        {
            // 화면의 크기를 가져옵니다.
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // 화면을 가로 2줄, 세로 3줄로 나눈 경계선
            float sectionWidth = screenWidth / 2;
            float sectionHeight = screenHeight / 3;

            // 인덱스 계산
            int row = (int)((screenHeight - position.y) / sectionHeight); // Y축을 반전
            int column = (int)(position.x / sectionWidth); // X축은 그대로 사용

            return (row * 2) + column; // 총 6칸
        }

        private void CleaningAction(int index)
        {
            var isMessedUp = _objectAnimator[index].GetBool("IsMessUp");
            var nameHash = _objectAnimator[index].GetCurrentAnimatorStateInfo(0).shortNameHash;
            if (!isMessedUp)
            {
                PlaySFX("miss");
                Score -= (int)(BASE_SCORE * 0.5f);
                if (Score < 0) Score = 0;
                _combo = 0;
                HP--;
            }
            else
            {
                var isStateMessedUp = nameHash == Animator.StringToHash("MessUp") || nameHash == Animator.StringToHash("MessUpLoop");
                if (!isStateMessedUp) return;
                PlaySFX("correct");
                _objectAnimator[index].SetTrigger("Restoration");
                Score += BASE_SCORE + (_combo * COMBO_SCORE);
                _combo++;
            }
            ComboProcess();
        }

        private void ComboProcess()
        {
            if (_comboTween != null)
            {
                _comboTween.Kill();
                _comboTween = null;
            }

            var comboText = GetText((int)CatGameText.ComboText);
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
        #endregion

        #region Cat Action
        private void CatActionEvent()
        {
            _currentActionTime += Time.deltaTime;
            if (_currentActionTime >= _actionTime)
            {
                var selectIndex = GetRandomFalseIndex();
                if (selectIndex == -1) return;

                _objectAnimator[selectIndex].SetTrigger("MessUp");
                PhaseCheck();
                _currentActionTime = 0;
            }
        }

        private void PhaseCheck()
        {
            if (_phase >= MAX_PHASE) return;

            _currentPatternCount++;
            if (_currentPatternCount >= _patternCount)
            {
                _patternCount++;
                _currentPatternCount = 0;
                _actionTime -= ACTION_TIME_REDUCTION;
                _phase++;
            }
        }

        private int GetRandomFalseIndex()
        {
            List<int> falseIndexes = _objectAnimator
                .Select((animator, index) => new { animator, index }) // 인덱스와 함께 애니메이터 선택
                .Where(x => !x.animator.GetBool("IsMessUp")) // IsMessUp이 false인 경우
                .Select(x => x.index) // 해당 인덱스 선택
                .ToList();

            if (falseIndexes.Count == 0)
            {
                Debug.LogWarning("No false values found.");
                return -1; // 혹은 다른 적절한 기본값
            }

            int randomIndex = falseIndexes[UnityEngine.Random.Range(0, falseIndexes.Count)];
            return randomIndex;
        }

        public void OnMessUp()
        {
            var messUpCount = _objectAnimator.Count(animator => animator.GetBool("IsMessUp"));
            if (messUpCount >= MAX_COUNT)
                EndProcess();
        }
        #endregion

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
