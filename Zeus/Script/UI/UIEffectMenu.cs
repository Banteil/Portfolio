using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class UIEffectMenu : VerticalMenuUI
    {
        [SerializeField] protected StorytellingSplashEffect _effect;
        [SerializeField] protected CanvasGroup _canvasGroup;

        private TweenerCore<float, float, FloatOptions> _floatTween = default;

        public UnityEvent OnFinishFadeInEvent;
        public UnityEvent OnFinishFadeOutEvent;

        public UnityEvent OnCalcelEvent;

        public virtual void Open(System.Action onFinish = null)
        {
            _canvasGroup.alpha = 0f;

            EnableUI(true);

            void OnFinish(StorytellingSplashEffect effect)
            {
                effect.onFadeInFinished -= OnFinish;

                OnFinishFadeInEvent?.Invoke();
                onFinish?.Invoke();
            }
            _effect.onFadeInFinished -= OnFinish;
            _effect.onFadeInFinished += OnFinish;
            _effect.FadeIn();

            TweenGroup(1f, 1f);
        }
        public virtual void Close(System.Action onFinish = null)
        {
            EnableUI(false);

            void OnFinish(StorytellingSplashEffect effect)
            {
                effect.onFadeOutFinished -= OnFinish;

                OnFinishFadeOutEvent?.Invoke();
                onFinish?.Invoke();
            }
            _effect.onFadeOutFinished -= OnFinish;
            _effect.onFadeOutFinished += OnFinish;
            _effect.FadeOut();

            TweenGroup(0f, 0.5f);
        }

        protected void OnFinishFadeIn(StorytellingSplashEffect effect)
        {
            OnFinishFadeInEvent.Invoke();
        }
        protected void OnFinishFadeOut(StorytellingSplashEffect effect)
        {
            OnFinishFadeOutEvent.Invoke();
        }

        private void TweenGroup(float endAlpha, float duration, Ease tweenEase = Ease.Linear)
        {
            if (_floatTween != null && _floatTween.active && _floatTween.IsPlaying())
                _floatTween.Kill();
            if (_canvasGroup != null)
            {
                _floatTween = _canvasGroup.DOFade(endAlpha, duration).SetEase(tweenEase);
            }
        }

        protected void SetMenuItem(int index, bool isEnabled)
        {
            if (index < 0 || index >= _menuList.Count) return;

            var menuItem = _menuList[index];
            menuItem.SetEnabled(isEnabled);
            menuItem.SetTextColor(isEnabled ? menuItem.DefaultTextColor : Color.gray);
        }
        protected void MenuItemEnabled(bool enabled)
        {
            for (int i = 0; i < _menuList.Count; i++)
            {
                var menuItem = _menuList[i];
                menuItem.SetEnabled(enabled);
            }
        }

        public override void OnCancel()
        {
            OnCalcelEvent?.Invoke();
        }
    } 
}
