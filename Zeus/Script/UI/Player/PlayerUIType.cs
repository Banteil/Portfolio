using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class PlayerUIType : MonoBehaviour
    {
        public TypePlayerUI UIType;
        protected CanvasGroup _canvas;

        protected virtual void Start()
        {
            AddUI();
            _canvas = GetComponent<CanvasGroup>();
        }

        protected virtual void AddUI()
        {
            PlayerUIManager.Get().AddUI(this, UIType);
        }

        public virtual void SetVisible(bool value, float fadeTime = 1, UnityAction callBack = null, float delayTime = 0f)
        {
            _canvas.DOKill();
            transform.DOMove(transform.position, delayTime).onComplete = () =>
            {
                _canvas.DOFade(value ? 1 : 0, fadeTime).onComplete = () =>
                {
                    callBack?.Invoke();
                };
            };
        }

        private void OnDestroy()
        {
            _canvas.DOKill();
        }
    }
}