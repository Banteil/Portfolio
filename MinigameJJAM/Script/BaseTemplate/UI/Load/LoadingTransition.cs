using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;

namespace starinc.io
{
    public class LoadingTransition : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _loadingText;

        public event Action OnLoadingStartComplete;
        public event Action OnLoadingEndComplete;

        public virtual async UniTask PlayLoadingStartAsync() { await UniTask.Yield(); }

        public virtual async UniTask PlayLoadingEndAsync() { await UniTask.Yield(); }

        protected void LoadingStartComplete() => OnLoadingStartComplete?.Invoke();

        protected void LoadingEndComplete()
        {
            OnLoadingEndComplete?.Invoke();
            Destroy(gameObject);
        }

        public virtual void SetLoadingText(string text)
        {
            if (_loadingText == null) return;
            _loadingText.text = text;
        }
    }
}
