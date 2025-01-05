using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "FireBossRandomShotPattern", menuName = "Scriptable Objects/Minigame/FireBossRandomShotPattern")]
    public class FireBossRandomShotPattern : AttackPattern
    {
        [SerializeField]
        private float _minBulletSpeed = 10f;
        [SerializeField]
        private float _maxBulletSpeed = 20f;
        [SerializeField]
        private float _bulletLifeTime = 5f;
        [SerializeField]
        private float _angularSpeed = 0f;
        [SerializeField]
        private int _projectileCount = 25;
        [SerializeField]
        private float _firingTime = 3f;
        [SerializeField]
        private float _spawnPosX, _spawnPosY = 0f;

        private IObjectPool<Bullet> _bulletPool;

        public override void ObjectPoolInitialize<T>(IObjectPool<T> pool) => _bulletPool = pool as IObjectPool<Bullet>;

        public override async UniTask PlayPattern()
        {
            var firingDelay = _firingTime / _projectileCount;
            for (int i = 0; i < _projectileCount; i++)
            {
                RandomFire();
                await UniTask.Delay(System.TimeSpan.FromSeconds(firingDelay));
            }

            float delayTime = Random.Range(_minDelayNextPattern, _maxDelayNextPattern);
            await UniTask.Delay(System.TimeSpan.FromSeconds(delayTime));
        }

        private void RandomFire()
        {
            if (_bulletPool == null || Creature == null) return;

            var currentAngle = UnityEngine.Random.Range(0f, 360f);
            Vector2 direction = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad)).normalized;

            Bullet bullet = _bulletPool.Get();
            var spawnPos = Creature.transform.position;
            spawnPos.x += _spawnPosX;
            spawnPos.y += _spawnPosY;
            bullet.transform.position = spawnPos;

            BulletInfo bulletInfo = new BulletInfo
            {
                Damage = Creature.Damage,
                LifeTime = _bulletLifeTime,
                Speed = UnityEngine.Random.Range(_minBulletSpeed, _maxBulletSpeed),
                Direction = direction,
                AngularSpeed = _angularSpeed
            };

            bullet.SetInfo(bulletInfo);
        }
    }
}
