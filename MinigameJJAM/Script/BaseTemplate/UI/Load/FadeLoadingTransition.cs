using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace starinc.io
{
    public class FadeLoadingTransition : LoadingTransition
    {
        [SerializeField]
        private float _fadeDuration = 0.1f;
        private CanvasGroup _fadeCanvasGroup;

        private void Awake()
        {
            _fadeCanvasGroup = Util.GetOrAddComponent<CanvasGroup>(gameObject);
        }

        public override async UniTask PlayLoadingStartAsync()
        {
            if (_fadeCanvasGroup != null)
            {
                _fadeCanvasGroup.gameObject.SetActive(true);
                _fadeCanvasGroup.alpha = 0;
                await _fadeCanvasGroup.DOFade(1, _fadeDuration).AsyncWaitForCompletion();
            }
            LoadingStartComplete();
        }

        public override async UniTask PlayLoadingEndAsync()
        {
            if (_fadeCanvasGroup != null)
            {
                await _fadeCanvasGroup.DOFade(0, _fadeDuration).AsyncWaitForCompletion();
                _fadeCanvasGroup.gameObject.SetActive(false);
            }
            LoadingEndComplete();
        }
    }
}
