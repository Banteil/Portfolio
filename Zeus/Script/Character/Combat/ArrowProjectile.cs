using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class ArrowProjectile : MonoBehaviour
    {
        [Header("Transform")]
        public Transform Handle;
        public Transform LookPoint;
        public CombatManager Owner;
        [Header("LifeTime")]
        private float _currentLifeTime;
        //발사 후 라이프 타임
        public float FlyLifeTime = 5f;
        //대상에게 맞은 후 라이프 타임
        public float HitLifeTime = 3f;

        internal AE_PhysicsMotion ProjectileMovement;
        private AttackObject _arrowObject;
        private GameTimeManager _timeManager;
        private Material _dissolveMaterial;

        [Header("ArrowType")]
        public List<int> SkillEffect = new List<int>();
        public bool UseIgnoreHitLayer;

        internal void Initialized()
        {
            ProjectileMovement = GetComponentInChildren<AE_PhysicsMotion>();
            ProjectileMovement.Owner = Owner;
            ProjectileMovement.CollisionEffectDestroyAfter = HitLifeTime;
            _arrowObject = ProjectileMovement.GetComponent<AttackObject>();
            _arrowObject.CombatManager = Owner;
            _arrowObject.SetActiveDamage(false);
            SetProjectileAbility();
            ProjectileMovement.IgnoreHitLayer = UseIgnoreHitLayer;
            if (GameTimeManager.Instance != null)
            {
                _timeManager = GameTimeManager.Instance;
            }

            var mateiralFind = false;
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var item in renderers)
            {
                foreach (var data in item.materials)
                {
                    if (data.HasProperty("_UseSoftCutout"))
                    {
                        data.DOKill();
                        _dissolveMaterial = data;
                        _dissolveMaterial.SetFloat("_UseSoftCutout", 6f);
                        _dissolveMaterial.DOFloat(0, "_UseSoftCutout", 0.5f).SetEase(Ease.InExpo);
                        break;
                    }
                }

                if (mateiralFind)
                    break;
            }
        }

        private void OnDisable()
        {
            if(ProjectileMovement == null) { return; }
            ProjectileMovement.HitAfterEvents -= ProjectileCollisionEvent;
        }

        private void SetProjectileAbility()
        {
            if (SkillEffect.Count != 0)
            {
                ProjectileMovement.HitAfterEvents += ProjectileCollisionEvent;
            }
        }

        private void ProjectileCollisionEvent(Vector3 rot)
        {
            for (int i = 0; i < SkillEffect.Count; i++)
            {
               SkillManager.Get().FireSkill(SkillEffect[i], ProjectileMovement.transform.position, rot, _arrowObject.CombatManager,transform);
            }
        }

        public void Fire(float powerMultiplier)
        {
            Handle = null;
            LookPoint = null;
            ProjectileMovement.hold = false;
            var damageValue = TableManager.GetWeaponDamage();
            damageValue += (int)(damageValue * powerMultiplier);
            _arrowObject.DamageInfo.DamageValue = damageValue;
            _arrowObject.tag = tag;
            _arrowObject.SetActiveDamage(true);
            _currentLifeTime = FlyLifeTime;
            if (_dissolveMaterial != null)
            {
                _dissolveMaterial.SetFloat("_UseSoftCutout", 0);
            }
        }

        private void FixedUpdate()
        {
            if (_currentLifeTime == 0f)
                return;

            if (LookPoint != null)
            {
                transform.LookAt(LookPoint);
            }

            //발사 된 상태면
            if (!ProjectileMovement.hold)
            {
                _currentLifeTime -= Time.fixedDeltaTime * (_timeManager != null ? _timeManager.WorldTimeScale : 1);

                if (_currentLifeTime <= 0)
                {
                    if (_dissolveMaterial != null)
                    {
                        _dissolveMaterial.DOFloat(6f, "_UseSoftCutout", 1f).SetEase(Ease.Linear).onComplete = () =>
                        {
                            _dissolveMaterial.DOKill();
                            _dissolveMaterial = null;
                            Destroy(gameObject);
                        };
                    }
                    else
                    {
                        Destroy(gameObject);
                    }

                    _currentLifeTime = 0f;
                }
            }
        }

        private void OnDestroy()
        {
            if (_dissolveMaterial != null)
            {
                _dissolveMaterial.DOKill();
            }
        }
    }
}