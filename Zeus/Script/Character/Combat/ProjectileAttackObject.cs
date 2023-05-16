using System.Collections;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public struct PickUpInfo
    {
        public Vector3 LocalPos;
        public Vector3 LocalRot;
    }

    [System.Serializable]
    public class ProjectileDestroyInfo
    {
        public bool DestoryOnCollision = true;
        public float DestroyTimeDelay = 0f;
        public GameObject[] EffectsOnCollision;
        public int EffectSoundID = -1;
    }

    public enum ProjectileType { NOMOVEMENT, FRONTSTRAIGHT, TARGETSTRAIGHT, TARGETGUIDED }

    [ClassHeader("Projectile Attack Object", OpenClose = false)]
    public class ProjectileAttackObject : AttackObject
    {
        public ProjectileType Type = ProjectileType.FRONTSTRAIGHT;
        public float Speed = 30f;
        public ProjectileDestroyInfo DestroyInfo;
        public PickUpInfo PickUpInfo;

        [HideInInspector] public Transform Target;

        protected Rigidbody _rigidBody;
        protected Animator _animator;
        protected CapsuleCollider _targetCollider;

        protected override void Initialization()
        {
            base.Initialization();
            SetActiveDamage(true);

            if (Target == null)
            {
                if (Type > ProjectileType.FRONTSTRAIGHT)
                    Type = ProjectileType.FRONTSTRAIGHT;
            }
            else
            {
                if (Type > ProjectileType.FRONTSTRAIGHT)
                {
                    Vector3 targetPos = Target.transform.position;
                    _targetCollider = Target.GetComponent<CapsuleCollider>();
                    targetPos.y += _targetCollider != null ? _targetCollider.height * 0.5f : 0f;
                    transform.LookAt(targetPos);
                }
            }

            _rigidBody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            if (DestroyInfo.DestroyTimeDelay > 0f)
                gameObject.DestroyTimer(DestroyInfo.DestroyTimeDelay);
            else if (_animator != null) StartCoroutine(CheckAnimatorNormalized());
        }

        IEnumerator CheckAnimatorNormalized()
        {
            yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
            Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            if (Type.Equals(ProjectileType.NOMOVEMENT)) return;

            Vector3 dir = transform.forward;
            if (Type.Equals(ProjectileType.TARGETGUIDED))
            {
                Vector3 targetPos = Target.transform.position;
                targetPos.y += _targetCollider != null ? _targetCollider.height * 0.5f : 0f;
                dir = (targetPos - transform.position).normalized;
            }

            var nextPos = transform.position + dir * Speed * GameTimeManager.Instance.DeltaTime;
            if (_rigidBody != null)
                _rigidBody.position = nextPos;
            else
                transform.SetPositionAndRotation(nextPos, transform.rotation);
        }

        public override void OnHit(HitBox hitBox, Collider other)
        {
            //히트가 가능한지 체크
            //공격 가능 상태이며, hitbox안에 충돌된 GameObect가 존재하며, Combatmanager가 존재하며, 충돌된 객체가 자기가 아닐시
            var hitCollider = other.GetComponentInParent<HitCollider>();
            if (hitCollider == null)
            {
                //Debug.LogError($"Not Found HitCollider other.name : {other.name} / rootname = {other.transform.root.name}");
                return;
            }

            bool isIgnoreObject = IgnoreGameObjects.Contains(hitCollider.Owner.gameObject);
            if (CanApplyDamage && !isIgnoreObject)
            {
                Debug.Log(gameObject.name + " 데미지 : " + DamageInfo.DamageValue);
                base.OnHit(hitBox, other);
            }

            if (!isIgnoreObject && hitCollider.Owner.CharacterState.HasFlag(TypeCharacterState.PARRY_READY) && DamageInfo.AttackerHitProperties.DamageTypeID == DamageType.PARRY)
            {
                //반사.
                transform.LookAt(DamageInfo.Sender.position);
                IgnoreGameObjects.Clear();
                IgnoreGameObjects.Add(hitCollider.Owner.gameObject);
                return;
            }

            if (!isIgnoreObject)
            {
                ActiveCollisionEffect();
                if (DestroyInfo.DestoryOnCollision) Destroy(gameObject);
            }
        }

        public void ActiveCollisionEffect()
        {
            for (int i = 0; i < DestroyInfo.EffectsOnCollision.Length; i++)
            {                
                var obj = Instantiate(DestroyInfo.EffectsOnCollision[i]);
                EffectsManager.Get().AddParticle(obj);
                obj.transform.position = transform.position;
                obj.transform.forward = transform.forward;
                obj.DestroyTimer(10f);
                if (DestroyInfo.EffectSoundID >= 0)
                    SoundManager.Instance.Play(DestroyInfo.EffectSoundID);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Ground")))
            {
                ActiveCollisionEffect();
                if (DestroyInfo.DestoryOnCollision) Destroy(gameObject);
            }
        }
    }
}