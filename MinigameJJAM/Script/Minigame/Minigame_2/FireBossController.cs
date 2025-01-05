using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace starinc.io
{
    public class FireBossController : EnemyController
    {
        #region Cache
        private const string FIRE_BULLET = "FireBullet";

        public SpriteRenderer EyesRenderer { get; private set; }
        private Vector2 _baseEyesPos;

        #region Move
        public float MovementLimitDistance = 10f;
        public float MoveDuration = 1f;
        private float _elapsedTime = 0;
        private Vector3 _moveStartPos, _moveTargetPos;
        #endregion

        #region Attack
        private int _attackCount;
        [SerializeField]
        private List<AttackPattern> _patterns;

        private Transform _bulletRoot;
        public IObjectPool<Bullet> BulletPool { get; private set; }
        #endregion
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            EyesRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            _baseEyesPos = EyesRenderer.transform.localPosition;
            OnActionProcess += EyesAct;

            _bulletRoot = new GameObject("FireBulletRoot").transform;
            BulletPool = new ObjectPool<Bullet>(CreateBullet, OnGetBullet, OnReleaseBullet, OnDestroyBullet);
            foreach (var pattern in _patterns)
            {
                pattern.Creature = this;
                pattern.ObjectPoolInitialize(BulletPool);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                Target = playerObj.GetComponent<CreatureController>();
        }

        private void EyesAct()
        {
            if (State != "Move")
            {
                Vector2 direction = (Target.transform.position - transform.position).normalized;
                Vector2 newPos = _baseEyesPos + direction;
                EyesRenderer.transform.localPosition = newPos;
            }
            else
            {
                var pos = _baseEyesPos;
                pos.x = Renderer.flipX ? -1 : 1;
                EyesRenderer.transform.localPosition = pos;
            }
        }

        #region Move State

        public void InitializeMoveState()
        {
            _moveStartPos = transform.position;
            Vector3 direction = (Target.transform.position - _moveStartPos).normalized;
            _moveTargetPos = _moveStartPos + direction * MovementLimitDistance;

            Renderer.flipX = direction.x < 0;
            OnActionProcess += MoveToTarget;
        }

        public void MoveToTarget()
        {
            _elapsedTime += Time.deltaTime;
            float t = _elapsedTime / MoveDuration;
            float smoothStepT = Mathf.SmoothStep(0, 1, t);

            Vector3 movePos = Vector3.Lerp(_moveStartPos, _moveTargetPos, smoothStepT);
            Rigid2D.MovePosition(movePos);

            if (_elapsedTime >= MoveDuration)
            {
                _elapsedTime = 0;
                ControllerAnimator.SetTrigger("Attack");
                Renderer.flipX = false;
                OnActionProcess -= MoveToTarget; // 이동 완료 후 더 이상 업데이트되지 않도록
            }
        }
        #endregion

        #region Attack State
        public async void AttackProcess()
        {
            if (_patterns.Count > 0)
            {
                _attackCount = UnityEngine.Random.Range(1, 5);
                for (int i = 0; i < _attackCount; i++)
                {
                    Manager.Sound.PlaySFX("m2sfx_fire_move");
                    var selectPattern = UnityEngine.Random.Range(0, _patterns.Count);
                    await _patterns[selectPattern].PlayPattern();
                }
            }
            if (ControllerAnimator != null)
                ControllerAnimator.SetTrigger("Idle");
        }
        #endregion

        #region ItemPoolEvent
        private Bullet CreateBullet()
        {
            var gameData = Manager.Game.GetCurrentMinigameData();
            var obj = gameData.GetPrefabObject(FIRE_BULLET, _bulletRoot, false);
            var bullet = obj.GetComponent<Bullet>();
            bullet.OnReturnToPool += BulletPool.Release;
            return bullet;
        }

        private void OnGetBullet(Bullet bullet)
        {
            bullet.gameObject.SetActive(true);
        }
        private void OnReleaseBullet(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
        }
        private void OnDestroyBullet(Bullet bullet)
        {
            bullet.OnReturnToPool -= OnReleaseBullet;
            Destroy(bullet.gameObject);
        }
        #endregion

        protected override void EndGame()
        {
            ControllerAnimator.SetBool("EndGame", true);
            base.EndGame();
        }

        protected override void DisableController()
        {
            base.DisableController();
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                var player = collision.GetComponent<CreatureController>();
                player.Hit(Damage);
            }
        }
    }
}
