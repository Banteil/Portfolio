using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Zeus
{
    public class ThrowAbility : CharacterAbility
    {
        public ProjectileAttackObject PickUpItem { get; set; }
        public Transform CurrentTarget { get; set; }

        public bool IsPickUp { get; set; }

        private string _itemName;
        private int _handPos;

        [SerializeField]
        protected List<ProjectileAttackObject> _throwItemList = new List<ProjectileAttackObject>();

        private AnimatorParameter _pickUpTriggerHash;
        private AnimatorParameter _throwTriggerHash;

        protected override void Initialize()
        {
            base.Initialize();
            if (_owner.Animator != null)
            {
                _pickUpTriggerHash = new AnimatorParameter(_owner.Animator, "PickUp");
                _throwTriggerHash = new AnimatorParameter(_owner.Animator, "Throw");
            }
            _owner.OnDeadEvent.AddListener(DeadEvent);

            IsPickUp = false;
        }

        void DeadEvent(GameObject gameObject)
        {
            if (!IsPickUp || PickUpItem == null) return;
            PickUpItem.ActiveCollisionEffect();
            Destroy(PickUpItem.gameObject);
            IsProcessing = false;
            PickUpItem = null;
        }

        bool FindItems()
        {
            _throwItemList.Clear();
            var projectiles = FindObjectsOfType<ProjectileAttackObject>().ToList();
            _throwItemList = projectiles.FindAll((x) => x.AttackObjectName.Equals(_itemName));

            var result = _throwItemList.Count > 0;
            if (!result)
            {
                IsProcessing = false;
                enabled = false;
            }
            return result;
        }

        ProjectileAttackObject GetCloseItem()
        {
            try
            {
                var items = _throwItemList.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).ToList();
                if (items.Count <= 0) return null;
                var obstacle = items[0].GetComponent<NavMeshObstacle>();
                if (obstacle != null) obstacle.enabled = false;
                return items[0];
            }
            catch
            {
                if (FindItems())
                {
                    var items = _throwItemList.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).ToList();
                    if (items.Count <= 0) return null;
                    var obstacle = items[0].GetComponent<NavMeshObstacle>();
                    if (obstacle != null) obstacle.enabled = false;
                    return items[0];
                }
                else
                    return null;
            }
        }

        public virtual bool FindToTarget(string itemName, Collider bodyCollider = null)
        {
            IsProcessing = true;
            IsPickUp = false;
            _itemName = itemName;
            if (!FindItems())
            {
                Debug.LogWarning("���� �������� �����ϴ�!");
                return false;
            }
            PickUpItem = GetCloseItem();

            var itemColliders = PickUpItem.transform.GetComponentsInChildren<Collider>();
            foreach (var itemCol in itemColliders)
            {
                Physics.IgnoreCollision(bodyCollider == null ? _owner.CapsuleCollider : bodyCollider, itemCol, true);
            }
            return true;
        }

        public virtual void PickUp(Transform target, int handPos = 0)
        {
            IsPickUp = true;
            CurrentTarget = target;
            _handPos = handPos;
            if (_pickUpTriggerHash != null)
                _owner.Animator.SetTrigger(_pickUpTriggerHash);
            else
                _owner.Animator.SetTrigger("PickUp");
        }

        public virtual void PickUpProcess()
        {
            if (PickUpItem == null)
            {
                IsProcessing = false;
                return;
            }
            var combatManager = _owner.GetComponent<CombatManager>();
            Transform handTr;
            switch (_handPos)
            {
                case 0:
                    handTr = combatManager.Members.Find(x => x.BodyPart.Equals("LeftHand")).Transform;
                    break;
                case 1:
                    handTr = combatManager.Members.Find(x => x.BodyPart.Equals("RightHand")).Transform;
                    break;
                default:
                    handTr = combatManager.Members.Find(x => x.BodyPart.Equals("LeftHand")).Transform;
                    break;
            }
            PickUpItem.transform.SetParent(handTr);
            PickUpItem.transform.localPosition = PickUpItem.PickUpInfo.LocalPos;
            PickUpItem.transform.localRotation = Quaternion.Euler(PickUpItem.PickUpInfo.LocalRot);
            var colliders = PickUpItem.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                if (!col.isTrigger) col.enabled = false;
            }
        }

        public virtual void Throw(Transform target)
        {
            CurrentTarget = target;
            if (_throwTriggerHash != null)
                _owner.Animator.SetTrigger(_throwTriggerHash);
            else
                _owner.Animator.SetTrigger("Throw");
        }

        public virtual void ThrowProcess()
        {
            if (PickUpItem == null)
            {
                IsProcessing = false;
                return;
            }
            var combatManager = _owner.GetComponent<CombatManager>();
            PickUpItem.Type = ProjectileType.TARGETSTRAIGHT;
            PickUpItem.Target = CurrentTarget;
            PickUpItem.DestroyInfo.DestroyTimeDelay = 10f;
            PickUpItem.CombatManager = combatManager;
            PickUpItem.enabled = true;
            var headPart = combatManager.Members.Find(_ => _.BodyPart.Equals(HumanBodyBones.Head.ToString()));
            PickUpItem.DamageInfo.Sender = headPart != null ? headPart.Transform : transform;
            PickUpItem.transform.SetParent(null);

            _throwItemList.Remove(PickUpItem);            
            PickUpItem = null;
            IsProcessing = false;
            if (_throwItemList.Count <= 0)
                enabled = false;
        }
    }
}
