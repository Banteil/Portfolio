using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Zeus
{
    public class FadeManager : UnitySingleton<FadeManager>
    {
        protected CanvasGroup _canvasGroup;
        protected bool _isFading;

        protected override void _OnAwake()
        {
            Instance.gameObject.layer = LayerMask.NameToLayer("UI");

            var canvasObj = new GameObject("FadeCanvas");
            canvasObj.layer = LayerMask.NameToLayer("UI");

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 1f;

            _canvasGroup = canvasObj.AddComponent<CanvasGroup>();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;

            canvasObj.transform.SetParent(Instance.transform);

            var fadeCanvasRect = canvasObj.GetComponent<RectTransform>();
            fadeCanvasRect.anchorMin = Vector2.zero;
            fadeCanvasRect.anchorMax = Vector2.one;

            var fadeImageObj = new GameObject("FadeImage");
            fadeImageObj.transform.SetParent(canvasObj.transform);
            fadeImageObj.layer = LayerMask.NameToLayer("UI");

            var fadeImage = fadeImageObj.AddComponent<Image>();
            fadeImage.color = Color.black;

            var fadeImageRect = fadeImageObj.GetComponent<RectTransform>();
            fadeImageRect.anchorMin = Vector2.zero;
            fadeImageRect.anchorMax = Vector2.one;
            fadeImageRect.offsetMin = Vector2.zero;
            fadeImageRect.offsetMax = Vector2.zero;
        }

        public bool IsFadeOut { get { return _canvasGroup.alpha >= 1f - float.Epsilon; } }
        public bool IsFading { get { return _isFading; } }

        public void DoFade(bool fadeOut, float duration = 1f, float delay = 0f, UnityAction callBack = null)
        {
            _isFading = true;
            _canvasGroup.DOKill();
            transform.DOKill();
            var delayTween = transform.DOMove(transform.position, delay).Pause();
            delayTween.timeScale = DOTween.unscaledTimeScale / DOTween.timeScale;
            delayTween.onComplete = () =>
             {
                 var destValue = fadeOut ? 1 : 0;
                 _canvasGroup.blocksRaycasts = true;
                 var fadeTween = _canvasGroup.DOFade(destValue, duration).SetEase(Ease.Linear).Pause();
                 fadeTween.timeScale = DOTween.unscaledTimeScale / DOTween.timeScale;
                 fadeTween.onComplete = () =>
                 {
                     _canvasGroup.blocksRaycasts = false;
                     _isFading = false;
                     callBack?.Invoke();
                 };
                 fadeTween.Play();
             };
            delayTween.Play();
        }
    }
}
