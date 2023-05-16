using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class AmazingDissolveController : MonoBehaviour
    {
        public UnityEvent CallStartDissolveComplete;
        public UnityEvent CallEndDissolveComplete;
        protected Renderer[] _renderers;

        void Start()
        {
            _renderers = gameObject.GetComponentsInChildren<Renderer>();
        }

        public void StartDissolve(float duration)
        {
            if (_renderers != null)
            {
                foreach (var renderer in _renderers)
                {
                    foreach (var item in renderer.materials)
                    {
                        if (item.HasProperty("_AdvancedDissolveCutoutStandardClip"))
                        {
                            item.DOKill();
                            item.DOFloat(1f, "_AdvancedDissolveCutoutStandardClip", duration).SetEase(Ease.InSine).onComplete = () =>
                            {
                                CallStartDissolveComplete?.Invoke();
                            };
                        }
                    }
                }
            }
        }

        public void EndDissolve(float duration)
        {
            if (_renderers != null)
            {
                foreach (var renderer in _renderers)
                {
                    foreach (var item in renderer.materials)
                    {
                        if (item.HasProperty("_AdvancedDissolveCutoutStandardClip"))
                        {
                            item.DOKill();
                            item.DOFloat(0f, "_AdvancedDissolveCutoutStandardClip", duration).SetEase(Ease.InSine).onComplete = () =>
                            {
                                CallEndDissolveComplete?.Invoke();
                            };
                        }
                    }
                }
            }
        }
    }
}
