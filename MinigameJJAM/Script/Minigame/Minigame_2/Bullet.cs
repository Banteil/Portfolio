using System;
using UnityEngine;

namespace starinc.io
{
    public class Bullet : MonoBehaviour
    {
        #region Cache
        [SerializeField]
        protected string _tagName = "Player";
        protected BulletInfo _bulletInfo;

        private float _elapsedTime = 0;
        private float _currentAngle = 0;

        private Rigidbody2D _rigidbody2D;
        #endregion

        #region Callback
        public event Action<Bullet> OnReturnToPool;
        #endregion

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void SetInfo(BulletInfo info) => _bulletInfo = info;

        private void FixedUpdate()
        {
            if (_bulletInfo == null) return;

            _elapsedTime += Time.fixedDeltaTime;
            if (_elapsedTime >= _bulletInfo.LifeTime)
            {
                Release();
                return;
            }

            Move();
        }

        private void Move()
        {
            Vector2 position = _rigidbody2D.position;
            Vector2 direction = _bulletInfo.Direction.normalized;

            _currentAngle += _bulletInfo.AngularSpeed * Time.fixedDeltaTime;
            float radianAngle = _currentAngle * Mathf.Deg2Rad;

            direction = new Vector2(
                direction.x * Mathf.Cos(radianAngle) - direction.y * Mathf.Sin(radianAngle),
                direction.x * Mathf.Sin(radianAngle) + direction.y * Mathf.Cos(radianAngle)
            ).normalized;

            position += direction * _bulletInfo.Speed * Time.fixedDeltaTime;

            _rigidbody2D.MovePosition(position);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.CompareTag(_tagName))
            {
                var creture = collision.GetComponent<CreatureController>();
                creture?.Hit(_bulletInfo.Damage);
                Release();
            }
        }

        private void Release()
        {
            OnReturnToPool?.Invoke(this);
            _bulletInfo = null;
            _elapsedTime = 0;
            _currentAngle = 0;
        }
    }

    [Serializable]
    public class BulletInfo
    {
        public int Damage;
        public float LifeTime;
        public float Speed;
        public Vector2 Direction;
        public float AngularSpeed;
    }
}
