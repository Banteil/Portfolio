using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Zeus
{
    public interface IContactable
    {
        bool IsContacted { get; }
    }
    public interface IInteractable : IContactable
    {
        void OnInteractEnter();
        void OnInteractLeave();
    }

    public enum TypeContact
    {
        NONE,
        OTHER,
        LOCAL,
        BOTH,
    }

    [System.Serializable]
    public class InteractionBase : MonoBehaviour
    {
        [SerializeField] protected float _interactableAngle;
        [SerializeField] protected float _interactableDistance;

        public float InteractableAngle => _interactableAngle;
        public float InteractableDistance => _interactableDistance;

        //protected void FixedUpdate()
        //{
        //    DrawSightRay();
        //}

        //protected virtual void DrawSightRay()
        //{
        //    var sightLeft = Quaternion.AngleAxis(-_interactableAngle * 0.5f, Vector3.up) * _transform.forward;
        //    var sightRight = Quaternion.AngleAxis(_interactableAngle * 0.5f, Vector3.up) * _transform.forward;

        //    Debug.DrawRay(_transform.position, sightLeft * _interactableDistance, Color.yellow, Time.fixedDeltaTime);
        //    Debug.DrawRay(_transform.position, sightRight * _interactableDistance, Color.yellow, Time.fixedDeltaTime);
        //}

        public bool IsInSight(Transform contactTarget)
        {
            var triggerTransform = this.transform;

            // 방향벡터
            var toTarget = contactTarget.position - triggerTransform.position;
            // 사이각
            var angle = Vector3.Angle(triggerTransform.forward, toTarget);

            //Debug.Log($"[{_target.name}] 시야각 : {_interactableAngle} / 목표 사이각 : {_interactableAngle * 0.5f} / [{contactTarget.name}] 사이각 : {angle}");

            // 시야거리 밖에 있음
            if (toTarget.magnitude > _interactableDistance) return false;
            // 시야각 밖에 있음
            if (_interactableAngle * 0.5f < angle) return false;

            return true;
        }
    }

    public class InteractionTrigger : InteractionBase
    {
        [SerializeField] protected TypeContact _contactType;

        public TypeContact ContactType => _contactType;

        public bool IsRecognized(Transform target, bool isIn)
        {
            bool recognized = false;
            switch (_contactType)
            {
                case TypeContact.NONE:
                    recognized = true;
                    break;
                case TypeContact.OTHER:
                    recognized = isIn;
                    break;
                case TypeContact.LOCAL:
                    recognized = IsInSight(target);
                    break;
                case TypeContact.BOTH:
                    recognized = isIn && IsInSight(target);
                    break;
            }
            return recognized;
        }

        public virtual void OnEnter(InteractionActor actor) { }
        public virtual void OnContact() { }
        public virtual void OnLeave() { }
    }
}