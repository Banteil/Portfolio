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

            // ���⺤��
            var toTarget = contactTarget.position - triggerTransform.position;
            // ���̰�
            var angle = Vector3.Angle(triggerTransform.forward, toTarget);

            //Debug.Log($"[{_target.name}] �þ߰� : {_interactableAngle} / ��ǥ ���̰� : {_interactableAngle * 0.5f} / [{contactTarget.name}] ���̰� : {angle}");

            // �þ߰Ÿ� �ۿ� ����
            if (toTarget.magnitude > _interactableDistance) return false;
            // �þ߰� �ۿ� ����
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