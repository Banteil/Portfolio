using System;
using UnityEngine;

namespace starinc.io
{
    public class IceCream : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidBody;
        private BoxCollider2D _collder;

        private bool _isCollision = false;
        public bool IsInavtive { get; private set; } = false;
        public event Action<IceCream> OnStackSuccess;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rigidBody = GetComponent<Rigidbody2D>();
            _collder = GetComponent<BoxCollider2D>();
        }

        public void InitializeIceCream(IceCreamData data)
        {
            _spriteRenderer.sprite = data.IceCreamSprite;
            _collder.offset = data.Offset;
            _collder.size = data.Size;
        }

        public void DropIceCream()
        {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        }

        public float GetYSize()
        {
            return _collder.size.y;
        }

        public void InactiveIceCream()
        {
            _rigidBody.bodyType = RigidbodyType2D.Static;
            IsInavtive = true;
        }

        private void Update()
        {
            if(IsInavtive && Util.IsOutsideViewportPosition(transform.position))
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_isCollision || collision.rigidbody == null) return;
            OnStackSuccess?.Invoke(this);
            OnStackSuccess = null;
            _isCollision = true;
            Debug.Log($"{gameObject.name} connected to {collision.gameObject.name}");
        }
    }
}
