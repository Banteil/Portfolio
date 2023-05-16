using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Zeus
{
    public class GaugeUI : MonoBehaviour
    {
        public Image HP;
        public Image DelayHP;
        public RectTransform HPEndImage;

        protected float _smoothDamageDelay = 0.05f;
        Coroutine _delayRoutine;
        bool _displayDelay;

        private void OnEnable()
        {
            _displayDelay = DelayHP != null;
        }

        public float Value
        {
            get { return HP.fillAmount; }
            set
            {
                if (_displayDelay) DelayHP.fillAmount = HP.fillAmount;
                HP.fillAmount = value;

                if (_delayRoutine != null)
                {
                    StopCoroutine(_delayRoutine);
                }
                _delayRoutine = StartCoroutine(DamageDelay());
                if (HPEndImage != null)
                {
                    SetEndPoint();
                }
            }
        }

        private void SetEndPoint()
        {
            //HP게이지 맨 왼쪽값 구함
            var hpPos = HP.transform.localPosition.x - HP.rectTransform.sizeDelta.x/2;
            //HP바 맨 끝 부분 위치 구함
            var imagePos = hpPos + (HP.rectTransform.sizeDelta.x * HP.fillAmount);
            HPEndImage.localPosition = new Vector3(imagePos, 0, 0);
        }

        IEnumerator DamageDelay()
        {
            if(!_displayDelay)
            {
                _delayRoutine = null;
                yield break;
            }

            while (DelayHP.fillAmount > HP.fillAmount)
            {
                DelayHP.fillAmount -= _smoothDamageDelay * GameTimeManager.Instance.DeltaTime;
                yield return null;
            }
            DelayHP.fillAmount = HP.fillAmount;
            _delayRoutine = null;
        }
    }
}