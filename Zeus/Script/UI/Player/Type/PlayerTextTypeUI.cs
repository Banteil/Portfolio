using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace Zeus
{
    public class PlayerTextTypeUI : PlayerUIType
    {
        [SerializeField]
        protected TextMeshProUGUI _text;
        private Coroutine _fadeCoroutine;
        public float FadeTime;

        protected override void Start()
        {
            base.Start();
        }

        public void SetVisible(string value = null)
        {
            if (_fadeCoroutine == null)
            {
                _text.text = value;
                _canvas.alpha = 0;
                _fadeCoroutine = StartCoroutine(FadeCo());
            }
        }

        private IEnumerator FadeCo()
        {
            _canvas.DOFade(1, FadeTime);
            yield return new WaitForSeconds(FadeTime);
            _canvas.DOFade(0, FadeTime);
            _fadeCoroutine = null;
        }
    }
}