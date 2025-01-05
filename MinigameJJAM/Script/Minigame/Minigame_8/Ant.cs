using System;
using UnityEngine;
using UnityEngine.AI;

namespace starinc.io
{
    public class Ant : MonoBehaviour
    {
        private float _maxStayTimer = 8f;
        private float _stayTimer = 0;

        private Vector3 _startPosition;
        private NavMeshAgent _agent;

        private event Action _onUpdateAction;
        
        public event Action OnPathPartial;
        public event Action OnArrived;
        public event Action<Ant> OnDestroyAnt;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
        }

        public void InitializeAnt(float maxStayTimer)
        {
            _maxStayTimer = maxStayTimer;
            _startPosition = transform.position;

            _agent.SetDestination(Vector3.zero);

            _onUpdateAction += CheckStayTime;
            _onUpdateAction += CheckDestination;
        }

        public void StopAct()
        {
            var animator = GetComponent<Animator>();
            animator.enabled = false;
            _agent.enabled = false;            
            enabled = false;
        }

        private void Update()
        {
            CheckPathPartial();
            RotateToDirection();
            _onUpdateAction?.Invoke();
        }

        private void CheckPathPartial()
        {
            if (_agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                OnPathPartial?.Invoke();
            }
        }

        private void RotateToDirection()
        {
            if (_agent.velocity.sqrMagnitude > 0.01f)
            {
                Vector3 direction = _agent.velocity.normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void CheckStayTime()
        {
            _stayTimer += Time.deltaTime;
            if(_stayTimer >= _maxStayTimer)
            {
                _agent.SetDestination(_startPosition);
                _onUpdateAction -= CheckStayTime;
                _onUpdateAction -= CheckDestination;
                _onUpdateAction += CheckOutsideViewport;
            }
        }

        private void CheckDestination()
        {
            float distanceToTarget = Vector3.Distance(transform.position, Vector3.zero);
            if (distanceToTarget <= 1f)
            {
                OnArrived?.Invoke();
            }
        }

        private void CheckOutsideViewport()
        {
            if(Util.IsOutsideViewportPosition(transform.position))
            {
                OnDestroyAnt?.Invoke(this);
                _onUpdateAction -= CheckOutsideViewport;
            }
        }
    }
}
