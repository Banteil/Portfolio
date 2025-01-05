using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace starinc.io
{
    public class SpeechBubble : MonoBehaviour
    {
        #region Cache
        private const float DELAY_BETWEEN_WORDS = 0.01f;

        [SerializeField]
        private float _delayInactiveBubble = 1f;

        [SerializeField]
        private TextMeshProUGUI _speechText, _screamText;
        [SerializeField]
        private CanvasGroup _commonBubble, _screamBubble;
        private WaitForSeconds _waitInactiveBubble;
        private Coroutine _speakRoutine;
        #endregion

        #region Callback
        public event Action OnSpeechEnd;
        #endregion

        private void Awake()
        {
            _speechText.text = "";
            _screamText.text = "";
            _waitInactiveBubble = new WaitForSeconds(_delayInactiveBubble);

            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        public void Speak(string message, bool mumbling)
        {
            if (_speakRoutine != null)
                StopCoroutine(_speakRoutine);
            _speakRoutine = StartCoroutine(ShowTextByCharacter(message, true, mumbling));
        }

        public void Scream(string message)
        {
            if (_speakRoutine != null)
                StopCoroutine(_speakRoutine);
            _commonBubble.alpha = 0f;
            
            _screamBubble.transform.DOScale(1.5f, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo);
            _speakRoutine = StartCoroutine(ShowTextByCharacter(message, false, false));
        }

        private IEnumerator ShowTextByCharacter(string message, bool isSpeak, bool mumbling = false)
        {
            var textUI = isSpeak ? _speechText : _screamText;
            textUI.text = "";
            var bubble = isSpeak ? _commonBubble : _screamBubble;
            bubble.alpha = 1;

            var delay = _delayInactiveBubble / message.Length;
            var delayBetweenWords = delay < DELAY_BETWEEN_WORDS ? delay : DELAY_BETWEEN_WORDS;
            if (mumbling)
            {
                Manager.Sound.StopSFX("mumbling");
                Manager.Sound.PlaySFX("mumbling", true);
            }
            foreach (var word in message)
            {
                textUI.text += word;
                yield return new WaitForSeconds(delayBetweenWords);
            }
            if (mumbling)
                Manager.Sound.StopSFX("mumbling");
            yield return _waitInactiveBubble;
            bubble.alpha = 0f;
            _speakRoutine = null;
            OnSpeechEnd?.Invoke();
        }

        private void OnLocaleChanged(Locale locale)
        {
            if (_speakRoutine != null)
            {
                StopCoroutine(_speakRoutine);
                _commonBubble.alpha = 0;
                _speakRoutine = null;
                OnSpeechEnd?.Invoke();
            }
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
    }
}
