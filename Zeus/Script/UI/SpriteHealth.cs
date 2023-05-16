using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Zeus
{
    [ClassHeader("SpriteHealth", "Assign your canvas object in the 'healthBar' field to hide and only display when receive damage - leave it empty if you want to display the healthbar all the time.", OpenClose = false)]
    public class SpriteHealth : zMonoBehaviour
    {
        [Tooltip("UI to show on receive damage")]
        [Header("UI properties")]

        public float ActiveDistance = 5f;
        public float HeightAdjustableVariable = 0f;
        public float BoundHeight = 0f;
        public float MinDistance = 5f;
        public float DisplayTimeWhenHit = 2f;

        [SerializeField] protected RectTransform _healthBar;
        [SerializeField] protected GaugeUI _gauge;
        [SerializeField] protected Text _damageCounter;
        [SerializeField] protected float _damageCounterTimer = 1.5f;
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected bool _persistentDisplayOffScreen = true;

        float _distance;
        float _maxHeight;
        float _baseHeight;
        Vector3 _scale;
        ZeusAIController _zeusAI;
        Coroutine _routine;
        bool _forcedTargetVisible;

        void Start()
        {
            _zeusAI = transform.GetComponentInParent<ZeusAIController>();
            if (_zeusAI == null)
            {
                Debug.LogWarning("The character must have a ICharacter Interface");
                Destroy(this.gameObject);
                return;
            }

            _zeusAI.onChangeHealth.AddListener(HPChange);
            _zeusAI.onReceiveDamage.AddListener(Damage);
            _damageCounter.text = string.Empty;

            var canvasHeight = _healthBar.transform.parent.GetComponent<RectTransform>().rect.height;
            _maxHeight = (canvasHeight * 0.5f) - _healthBar.rect.height;

            if (BoundHeight.Equals(0f))
                BoundHeight = _zeusAI.CapsuleCollider.height * 0.5f;
            _baseHeight = _zeusAI.CapsuleCollider.height;
            _canvasGroup.alpha = 0f;
            _scale = Vector3.one;

            if (_zeusAI.CurrentHealth != 0)
            {
                _gauge.Value = _zeusAI.CurrentHealth / _zeusAI.MaxHealth;
            }
        }

        public bool IsTargetVisible
        {
            get
            {
                if (_forcedTargetVisible) return true;
                if (_zeusAI.CurrentTarget == null || _zeusAI.CurrentTarget.Transform == null) return false;

                var planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
                var point = _zeusAI.transform.position;
                point.y += BoundHeight;
                foreach (var plane in planes)
                {
                    if (plane.GetDistanceToPoint(point) < 0)
                        return false;
                }

                if (_distance > ActiveDistance) return false;

                int layerMask = (1 << LayerMask.NameToLayer("Character")) + (1 << LayerMask.NameToLayer("Ground"));
                var dir = (point - Camera.main.transform.position).normalized;
                RaycastHit[] hits = Physics.RaycastAll(Camera.main.transform.position, dir, _distance, layerMask);
                Array.Sort(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider.CompareTag("Player")) continue;
                    else if ((i != hits.Length - 1) && !hits[i].transform.Equals(transform))
                        return false;
                }

                return true;
            }
        }

        void SetUIPosition()
        {
            var pos = _zeusAI.transform.position;
            pos.y += _baseHeight + HeightAdjustableVariable;
            _healthBar.position = Camera.main.WorldToScreenPoint(pos);
            if (_persistentDisplayOffScreen && _healthBar.anchoredPosition.y > _maxHeight)
            {
                var healthPos = _healthBar.anchoredPosition;
                healthPos.y = _maxHeight;
                _healthBar.anchoredPosition = healthPos;
            }
        }

        public void SetVisible(bool value, float fadeTime = 0.2f, UnityAction callBack = null)
        {
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(value ? 1 : 0, fadeTime).onComplete = () =>
            {
                callBack?.Invoke();
            };
        }

        void SetScale()
        {
            float scale = MinDistance / _distance;
            if (scale > 1f) scale = 1f;
            _scale.x = _scale.y = scale;
            _healthBar.localScale = _scale;
        }

        void SpriteBehaviour()
        {
            if (_zeusAI == null || _zeusAI.CurrentHealth <= 0)
                Destroy(gameObject);

            _distance = (_zeusAI.transform.position - Camera.main.transform.position).magnitude;
            if (_canvasGroup.alpha > 0)
            {
                SetUIPosition();
                SetScale();
            }
            SetVisible(IsTargetVisible);
        }

        void Update()
        {
            SpriteBehaviour();
        }

        public void Damage(Damage damage)
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(DamageViewProcess());
        }

        IEnumerator DamageViewProcess()
        {
            SetUIPosition();
            SetScale();
            _forcedTargetVisible = true;
            yield return new WaitForSeconds(DisplayTimeWhenHit);
            _forcedTargetVisible = false;
            _routine = null;
        }

        public void HPChange(float currentHP)
        {
            try
            {
                _gauge.Value = _zeusAI.CurrentHealth / _zeusAI.MaxHealth;
            }
            catch
            {
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            _canvasGroup.DOKill();
        }
    }
}