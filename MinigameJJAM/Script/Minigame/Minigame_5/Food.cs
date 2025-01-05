using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    public class Food : MonoBehaviour
    {
        #region Cache
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigid2D;
        private CircleCollider2D _collider;
        private TrailRenderer _trailRenderer;

        private FoodData _data;
        public FoodData CurrentData { get { return _data; } }
        private Dictionary<CookingSide, CookingStateData> _sideData;
        private CookingSide _currentSide = CookingSide.Bottom;

        private Vector3 _basePos;
        private Vector2 _trailVelocity;

        public bool IsInteraction = false;
        #endregion

        #region Callback
        public event Action<Dictionary<CookingSide, CookingStateData>> OnCooked;
        #endregion

        private void Awake()
        {
            _basePos = transform.position;            
            _rigid2D = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _trailRenderer = GetComponentInChildren<TrailRenderer>();

            _sideData = new Dictionary<CookingSide, CookingStateData>
            {
                { CookingSide.Top, new CookingStateData() },
                { CookingSide.Bottom, new CookingStateData() }
            };
        }

        public void SetFoodData(FoodData data)
        {
            _data = data;
            _currentSide = CookingSide.Bottom;
            _sideData[CookingSide.Top].Reset();
            _sideData[CookingSide.Bottom].Reset();
            _spriteRenderer.sprite = _data.GetFoodSprite(_sideData[_currentSide].state);
            _collider.radius = _data.Radius;
        }

        public void Spwan(Action callback)
        {
            if (IsInteraction) return;
            IsInteraction = true;
            callback?.Invoke();

            transform.position = _basePos;
            transform.rotation = Quaternion.identity;
            _spriteRenderer.transform.rotation = Quaternion.identity;
            var blurRadius = 3f;
            var instanceMaterial = _spriteRenderer.material;
            _spriteRenderer.transform.localScale = new Vector3(2f, 2f);

            Sequence spwanSequence = DOTween.Sequence();
            spwanSequence.Append(_spriteRenderer.transform.DOScale(1f, 0.5f))
                .Join(DOTween.To(() => blurRadius, x => blurRadius = x, 0f, 0.5f)
                .OnUpdate(() => instanceMaterial.SetFloat("_BlurRadius", blurRadius)));
            spwanSequence.OnComplete(() =>
            {
                Manager.Sound.PlaySFX("m5sfx_flip_done");
                Manager.Sound.PlaySFX("m5sfx_sizzle_loop", true);
                _collider.enabled = true;
                _trailRenderer.enabled = true;
                IsInteraction = false;
            });
            spwanSequence.Play();
        }

        public void Remove(Action callback)
        {
            if (IsInteraction) return;
            IsInteraction = true;

            _collider.enabled = false;
            _trailRenderer.Clear();
            _trailRenderer.enabled = false;

            Manager.Sound.StopSFX("m5sfx_sizzle_loop");
            Sequence removeSequence = DOTween.Sequence();
            removeSequence.Append(transform.DOMoveY(transform.position.y + 10f, 0.5f));
            removeSequence.Join(_spriteRenderer.transform.DORotate(Vector3.forward * 360f, 0.5f, RotateMode.FastBeyond360));
            removeSequence.OnComplete(() =>
            {
                callback?.Invoke();
                IsInteraction = false;
            });
            removeSequence.Play();
        }

        public void MovementFood(Vector2 velocity)
        {
            Vector2 newPosition = _rigid2D.position + (velocity * _data.Speed * Time.deltaTime);
            _rigid2D.MovePosition(newPosition);
            UpdateTrailPosition(velocity);
        }

        private void UpdateTrailPosition(Vector2 velocity)
        {
            if (velocity.magnitude == 0) return;

            Vector2 oppositeDirection = -velocity.normalized;
            float trailDistance = Mathf.Clamp(velocity.magnitude * 0.1f, _data.MinTrailDistance, _data.MaxTrailDistance);
            Vector2 targetPosition = oppositeDirection * trailDistance;

            Vector2 smoothPosition = Vector2.SmoothDamp(
                _trailRenderer.transform.localPosition,
                targetPosition,
                ref _trailVelocity,
                0.1f
            );

            _trailRenderer.transform.localPosition = new Vector3(smoothPosition.x, smoothPosition.y, _trailRenderer.transform.localPosition.z);
        }

        public void FlipOver()
        {
            if (IsInteraction) return;
            IsInteraction = true;

            var blurRadius = 0f;
            var instanceMaterial = _spriteRenderer.material;

            _trailRenderer.Clear();
            _trailRenderer.enabled = false;
            instanceMaterial.SetFloat("_BlurRadius", blurRadius);

            Manager.Sound.StopSFX("m5sfx_sizzle_loop");
            Manager.Sound.PlaySFX("m5sfx_flip");
            Sequence flipSequence = DOTween.Sequence();
            flipSequence.Append(_spriteRenderer.transform.DOScale(2f, 0.5f))
                .Join(DOTween.To(() => blurRadius, x => blurRadius = x, 3f, 0.5f)
                .OnUpdate(() => instanceMaterial.SetFloat("_BlurRadius", blurRadius)));

            flipSequence.AppendCallback(() =>
            {
                var currentData = _sideData[_currentSide];
                currentData.state = _data.GetCurrentState(currentData.timer);
                if (currentData.state == CookingState.Raw)
                {
                    currentData.timer -= 1f;
                    if (currentData.timer < 0f)
                        currentData.timer = 0f;
                }
                _spriteRenderer.sprite = _data.GetFoodSprite(currentData.state);
                _currentSide = _currentSide == CookingSide.Bottom ? CookingSide.Top : CookingSide.Bottom;
                _spriteRenderer.flipX = !_spriteRenderer.flipX;
            });

            // 3. 다시 원래 위치로 돌아오면서 스케일과 블러 감소
            flipSequence.Append(_spriteRenderer.transform.DOScale(1f, 0.5f))
                .Join(DOTween.To(() => blurRadius, x => blurRadius = x, 0f, 0.5f)
                .OnUpdate(() => instanceMaterial.SetFloat("_BlurRadius", blurRadius)));

            // 4. 연출이 끝나면 실제 요리 상태 변경
            flipSequence.OnComplete(() =>
            {
                Manager.Sound.PlaySFX("m5sfx_flip_done");
                Manager.Sound.PlaySFX("m5sfx_sizzle_loop", true);
                _trailRenderer.enabled = true;
                IsInteraction = false;
                OnCooked?.Invoke(_sideData);
            });
            flipSequence.Play();
        }

        public void Cooking()
        {
            if (IsInteraction) return;
            _sideData[_currentSide].timer += Time.deltaTime;
        }
    }

    public enum CookingSide
    {
        Top,
        Bottom,
    }

    [System.Serializable]
    public class CookingStateData
    {
        public float timer = 0f;
        public CookingState state = CookingState.Raw;

        public void Reset()
        {
            timer = 0f;
            state = CookingState.Raw;
        }
    }
}
