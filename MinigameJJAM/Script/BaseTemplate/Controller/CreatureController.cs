using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

namespace starinc.io
{
    public class CreatureController : BaseController
    {
        #region Cache
        public virtual int HP { get; set; } = 1;
        public virtual float Speed { get; set; } = 10;
        public virtual int Damage { get; set; } = 1;

        public float IgnoreHitTime = 1f;
        protected bool _ignoreHit, _invisible;
        
        protected Coroutine _invisibleRoutine;
        #endregion

        #region Callback
        public event Action<int> OnChangedHP;
        #endregion

        public virtual void Hit(int damage)
        {
            if (_ignoreHit || _invisible) return;
            Manager.Sound.PlaySFX("m2sfx_hit");
            HP -= damage;

            if (IgnoreHitTime > 0)
            {
                _ignoreHit = true;
                float blinkDuration = Mathf.Min(0.3f, IgnoreHitTime / 2f);
                float holdDuration = IgnoreHitTime - blinkDuration;

                Renderer.DOColor(new Color(1f, 0f, 0f, 0.5f), blinkDuration)
                    .SetLoops(1, LoopType.Yoyo)
                    .OnComplete(() =>
                    {
                        Renderer.DOFade(0.5f, holdDuration)
                            .OnComplete(() =>
                            {
                                Renderer.color = Color.white;
                                _ignoreHit = false;
                            });
                    });
            }
        }

        protected virtual void ChangedHP(int hp) => OnChangedHP?.Invoke(hp);

        public virtual void SetInvisibleState(float time)
        {
            if(_invisibleRoutine != null)
            {
                StopCoroutine( _invisibleRoutine);
                _invisibleRoutine = null;
            }

            _invisibleRoutine = StartCoroutine(InvisibleProcess(time));
        }

        protected IEnumerator InvisibleProcess(float time)
        {
            _invisible = true;
            var timer = 0f;
            while (timer < time)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            _invisible = false;
            _invisibleRoutine = null;
        }
    }
}
