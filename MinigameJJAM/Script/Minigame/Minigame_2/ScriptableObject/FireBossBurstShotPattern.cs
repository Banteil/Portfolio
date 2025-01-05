using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "FireBossBurstShotPattern", menuName = "Scriptable Objects/Minigame/FireBossBurstShotPattern")]
    public class FireBossBurstShotPattern : AttackPattern
    {
        [SerializeField]
        private float _bulletSpeed = 10f;
        [SerializeField]
        private float _bulletLifeTime = 5f;
        [SerializeField]
        private float _angularSpeed = 0f;
        [SerializeField]
        private int _numberOfShotsFired = 3;
        [SerializeField]
        private int _numberOfProjectilesPerFire = 3;
        [SerializeField]
        private int _shotsPerDirection = 8; // 8방향으로 발사
        [SerializeField]
        private float _firingDelay = 0.2f;
        [SerializeField]
        private float _projectileDelay = 0.1f; // 같은 방향 내에서 각 발사 간 딜레이
        [SerializeField]
        private float _angleAdjustmentFigures = 0f;
        [SerializeField]
        private float _spawnPosX, _spawnPosY = 0f;

        private float _initialAngle = 0f;       

        private IObjectPool<Bullet> _bulletPool;

        public override void ObjectPoolInitialize<T>(IObjectPool<T> pool) => _bulletPool = pool as IObjectPool<Bullet>;

        public override async UniTask PlayPattern()
        {
            Reset();
            for (int i = 0; i < _numberOfShotsFired; i++)
            {
                await FireInAllDirections();
                await UniTask.Delay(System.TimeSpan.FromSeconds(_firingDelay));
                _initialAngle += _angleAdjustmentFigures;
            }

            float delayTime = Random.Range(_minDelayNextPattern, _maxDelayNextPattern);
            await UniTask.Delay(System.TimeSpan.FromSeconds(delayTime));
        }

        private async UniTask FireInAllDirections()
        {
            if (_bulletPool == null || Creature == null) return;

            float angleStep = 360f / _shotsPerDirection;
            float currentAngle = _initialAngle;

            for (int i = 0; i < _numberOfProjectilesPerFire; i++)
            {
                for (int j = 0; j < _shotsPerDirection; j++)
                {
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
                        Speed = _bulletSpeed,
                        Direction = direction,
                        AngularSpeed = _angularSpeed
                    };

                    bullet.SetInfo(bulletInfo);
                    currentAngle += angleStep; // 다음 방향으로 각도 증가
                }
                await UniTask.Delay(System.TimeSpan.FromSeconds(_projectileDelay));
            }            
        }

        private void Reset()
        {
            _initialAngle = 0f;
        }
    }
}
