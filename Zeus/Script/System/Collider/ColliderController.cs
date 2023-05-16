using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    [Flags]
    public enum TypeColliderEnableOff
    {
        CONTACT = 1, TIME = 2,
    }

    public enum TypeColliderUpdate
    {
        MOVE,
        DELAY,
    }

    [RequireComponent(typeof(Collider))]
    public class ColliderController : MonoBehaviour
    {
        public UnityEvent<Collider, bool> ControlColliderInfo;
        public UnityEvent<Vector3, Collider> TriggerEvent;

        public string[] IgnoreTag;
        public LayerMask LayerMask = 0 >> 1;
        public TypeColliderUpdate TypeColliderUpdate;
        public TypeColliderEnableOff ColliderEnableOffType;
        public float EnableTimeDelay;
        public float DestSize;
        public float MinSize;
        public float OffTimeDelay;

        private Vector3 _prevPosition = Vector3.zero;
        private Collider _collider;
        private BoxCollider _boxCollider;
        private float _elapsedTime;
        private float _remainTime = -1f;

        IEnumerator Start()
        {
            //이동타입에선 켜지기 전까지 대기.
            if (TypeColliderUpdate == TypeColliderUpdate.MOVE)
            {
                yield return new WaitForSeconds(EnableTimeDelay);
            }

            _elapsedTime = 0f;
            _collider = GetComponent<Collider>();
            _boxCollider = GetComponent<BoxCollider>();

            //켜지기 대기시간이 있다면 꺼놓는다.
            if (_collider != null && _collider.enabled)
            {
                _collider.enabled = EnableTimeDelay == 0;
                ControlColliderInfo?.Invoke(_collider, _collider.enabled);
            }


            if (TypeColliderUpdate == TypeColliderUpdate.DELAY)
            {
                Invoke(nameof(ColliderSizeUpdate), EnableTimeDelay);
            }
        }

        private void ColliderSizeUpdate()
        {
            if (_collider == null)
                return;

            if (TypeColliderUpdate == TypeColliderUpdate.DELAY && !_collider.enabled)
            {
                _collider.enabled = true;
                ControlColliderInfo?.Invoke(_collider, _collider.enabled);
                if (ColliderEnableOffType.HasFlag(TypeColliderEnableOff.TIME))
                {
                    _remainTime = OffTimeDelay;
                }
            }

            if (DestSize == 0)
                return;

            if (_boxCollider == null)
                return;

            var size = _boxCollider.size;
            size.z = DestSize > MinSize ? DestSize : MinSize;
            _boxCollider.size = size;
            var position = _boxCollider.center;
            var sizeZ = TypeColliderUpdate == TypeColliderUpdate.MOVE ? (size.z - 1) * -0.5f : (size.z - 1) * 0.5f;
            position.z = sizeZ;
            _boxCollider.center = position;
        }

        private void FixedUpdate()
        {
            if (_collider == null) return;

            if (_remainTime >= 0 && _collider.enabled)
            {
                _elapsedTime += GameTimeManager.Instance.DeltaTime;
                _remainTime = OffTimeDelay - _elapsedTime;
                if (_remainTime <= 0)
                {
                    _collider.enabled = false;
                    ControlColliderInfo?.Invoke(_collider, _collider.enabled);
                    _remainTime = -1f;
                    _elapsedTime = 0f;
                }
            }

            #region MOVE UPDATE
            if (TypeColliderUpdate != TypeColliderUpdate.MOVE)
                return;

            if (_prevPosition == transform.position)
                return;

            var currentPosition = transform.position;
            DestSize = Vector3.Distance(_prevPosition, currentPosition);

            ColliderSizeUpdate();
            _prevPosition = transform.position;
            #endregion  
        }

        private void OnEnable()
        {
            _prevPosition = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            foreach (var item in IgnoreTag)
            {
                if (other.gameObject.CompareTag(item))
                    return;
            }

            var layer = 1 << other.gameObject.layer;
            var check = Util.CheckFlag(LayerMask.value, layer);
            if (!check)
                return;

            if (ColliderEnableOffType.HasFlag(TypeColliderEnableOff.CONTACT))
            {
                _collider.enabled = false;
                ControlColliderInfo?.Invoke(_collider, _collider.enabled);
            }

            TriggerEvent?.Invoke(transform.position, other);
        }
    }
}