using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Zeus
{
    public class LockOnBehaviour : MonoBehaviour
    {
        [Tooltip("�� �Ÿ��ȿ������� �Ÿ� �켱���� Ÿ�����ϰ�. �Ÿ��� ����� ũ�ν����� ����� ������ Ÿ���� ��´�.")]
        [SerializeField]
        private float _setTargetDistance = 5f;
        //���
        private Transform _watcher;
        //���߿� �����ͷ� �ν�����â�� ����Ҽ� �ֵ��� ����
        public string[] TagsToFind = new string[] { "Enemy" };
        //LockOn ��� ã���� ��ֹ� ���̾�
        public LayerMask LayerOfObstacles = 1 << 0;
        //���� ��ġ�� X Margin
        [Range(0, 1)]
        public float ScreenMarginX = 0.8f;
        [Range(0, 1)]
        public float ScreenMarginY = 0.1f;
        //���� ����
        public float Range = 20f;
        public bool ShowDebug;
        public float TimeToChangeTarget = 0.25f;

        protected Transform _target;
        protected Camera _camera;
        private Rect _rect;
        private bool _inLockOn;

        public virtual void ChangeTarget(TypeTargetSearch searchTyep)
        {
            ChangeTargetRoutine(searchTyep == TypeTargetSearch.RIGHT);
        }

        public virtual Transform CurrentTarget
        {
            get { return _target; }
        }

        public virtual bool IsTargetCharacterAlive()
        {
            if (CurrentTarget == null)
            {
                return false;
            }

            var healthController = CurrentTarget.GetComponent<IHealthController>();

            if (healthController == null)
            {
                //�׽�Ʈ ������ ������
                if (CurrentTarget.GetComponent<IHealthController>() != null)
                {
                    return true;
                }

                return false;
            }
            if (healthController == null)
            {
                Debug.Log(healthController);
                return false;
            }

            if (healthController.CurrentHealth > 0)
            {
                return true;
            }

            return false;
        }

        internal bool IsCharacterAlive(Transform other)
        {
            var healthController = other.GetComponent<IHealthController>();
            if (healthController == null)
            {
                //�׽�Ʈ��
                var testHealthControllerother = other.GetComponent<IHealthController>();
                if (testHealthControllerother != null)
                {
                    if (testHealthControllerother.CurrentHealth > 0)
                    {
                        return true;
                    }

                }

                return false;
            }

            if (healthController.CurrentHealth > 0)
            {
                return true;
            }

            return false;
        }

        protected void ResetLockOn()
        {
            _target = null;
            _inLockOn = false;
        }

        protected bool SetLockOn(bool lockOn)
        {
            if (lockOn)
            {
                var target = GetTarget();
                if (target != null)
                {
                    _target = target;
                }
            }
            else 
            {
                _target = null;
            }

            return _target != null;
        }

        public enum TypeTargetSearch
        {
            LEFT,
            RIGHT,
        }

        //Ÿ�� ��ȯ
        protected void ChangeTargetRoutine(bool right)
        {
            if (_target == null)
                return;

            var list = GetTargetList();
            var newList = new List<TargetData>();
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var targetLR = GetTypeLRPosition(_target, _camera.transform, item.target);

                if (right && targetLR == TypeTargetSearch.LEFT)
                    continue;

                if (!right && targetLR == TypeTargetSearch.RIGHT)
                    continue;

                item.OtherDistance = Vector3.Distance(_target.position, item.target.position);
                newList.Add(item);
            }

            if (newList.Count == 0) 
                return; 

            var selects = newList.OrderBy(x => x.OtherDistance).ToList();
            _target = selects[0].target;
            SetTarget();
        }

        public static TypeTargetSearch GetTypeLRPosition(Transform target, Transform cam, Transform otherTarget)
        {
            var type = TypeTargetSearch.LEFT;

            var fromDir = (cam.position - target.position).normalized;
            var toDir = (otherTarget.position - target.position).normalized;

            var rot = Quaternion.FromToRotation(fromDir, toDir);

            if (rot.eulerAngles.y > 180)
                type = TypeTargetSearch.RIGHT;

            return type;
        }

        protected virtual void SetTarget() {}

        protected void Init()
        {
            if (Camera.main == null)
            {
                //ī�޶� ������ ��� ����
                this.enabled = false;
            }

            _camera = Camera.main;
            var width = Screen.width - (Screen.width * ScreenMarginX);
            var height = Screen.height - (Screen.height * ScreenMarginY);
            var posX = (Screen.width * 0.5f) - (width * 0.5f);
            var posY = (Screen.height * 0.5f) - (height * 0.5f);
            _rect = new Rect(posX, posY, width, height);
        }

        protected void OnGUI()
        {
            if (ShowDebug)
            {
                var width = Screen.width - (Screen.width * ScreenMarginX);
                var height = Screen.height - (Screen.height * ScreenMarginY);
                var posX = (Screen.width * 0.5f) - (width * 0.5f);
                var posY = (Screen.height * 0.5f) - (height * 0.5f);
                _rect = new Rect(posX, posY, width, height);
                GUI.Box(_rect, "");
            }
        }

        struct TargetData
        {
            public float Distance;
            public float OtherDistance;
            public float Angle;
            public Transform target;
        }

        private List<TargetData> GetTargetList()
        {
            var listPrimary = new List<TargetData>();
            //Ÿ�� ���� ������
            var targets = Physics.OverlapSphere(transform.position, Range);
            for (int i = 0; i < targets.Length; i++)
            {
                var hitOther = targets[i];
                //�����ִ� �ѱ��.
                if (_target == hitOther.transform)
                {
                    //Debug.Log("1 === " + hitOther.transform.name);
                    continue;
                }

                //���̾� üũ.
                //var layer = 1 << hitOther.transform.gameObject.layer;
                //if (Util.CheckFlag(LayerOfObstacles.value, layer))
                //    continue;

                //Ÿ�� �±׫n.
                if (!TagsToFind.Contains(hitOther.transform.tag))
                {
                    //Debug.Log("2 === " + hitOther.transform.name);
                    continue;
                }

                //ī�޶� �ȿ� �ֳ�?
                if (!Util.CheckObjectIsInCamera(_camera, hitOther.transform.position))
                {
                    //Debug.Log("3 === " + hitOther.transform.name);
                    continue;
                }

                //����ִ� ���ΰ�?
                if (!IsCharacterAlive(hitOther.transform))
                {
                    //Debug.Log("4 === " + hitOther.transform.name);
                    continue;
                }

                //Debug.Log($"hitOther.transform.tag : {hitOther.transform.tag}");
                var castingPostion = hitOther.transform.position;
                castingPostion.y += 1.8f;
                var startPosition = transform.position;
                startPosition.y += 1.8f;
                var dir = castingPostion - startPosition;
                var hitInfos = Physics.RaycastAll(startPosition, dir.normalized, dir.magnitude);
#if UNITY_EDITOR
                if (ShowDebug)
                {
                    Debug.DrawLine(startPosition, castingPostion, Color.green, 4f);
                }
#endif

                //Ÿ�ٰ� �� ���̿� ��ֹ� üũ.
                var findBlock = false;
                foreach (var item in hitInfos)
                {
                    if (!findBlock)
                    {
                        var layer = 1 << item.transform.gameObject.layer;
                        findBlock = Util.CheckFlag(LayerOfObstacles.value, layer);
                    }

                    if (findBlock)
                        break;
                }

                if (findBlock)
                {
                    //Debug.Log("5 === " + hitOther.transform.name);
                    continue;
                }

                //������ ���ؼ� �����Ϳ� ��´�.
                var t = new TargetData
                {
                    Distance = Vector3.Distance(transform.position, hitOther.transform.position),
                    Angle = Vector3.Angle(_camera.transform.forward, dir.normalized),
                    target = hitOther.transform,
                };

                listPrimary.Add(t);
            }

            return listPrimary;
        }

        internal Transform GetTarget()
        {
            var listPrimary = GetTargetList();
            var list = listPrimary.OrderBy(x => x.Distance).ToArray();
            //Debug.Log("Length === " + list.Length);
            //_setTargetDistance ���ϴ� �ޱۿ� ������� �ش�.
            foreach (var item in list)
            {
                if (item.Distance <= _setTargetDistance)
                {
                    return item.target;
                }
            }

            list = listPrimary.OrderBy(x => x.Angle).ToArray();
            var selectTarget = list.Length > 0 ? list[0].target : null;

            return selectTarget;
        }
    }

    public static class LockOnHelper
    {
        //��� ��ü�� ��� �������� ������(��ũ��)
        public static Vector2 GetScreenPointOffBoundsCenter(this Transform target, Canvas canvas, Camera cam, float _heightOffset)
        {
            var bounds = target.GetComponent<Collider>().bounds;
            var middle = bounds.center;
            var height = Vector3.Distance(bounds.min, bounds.max);

            var point = middle + new Vector3(0, height * _heightOffset, 0);
            var rectTransform = canvas.transform as RectTransform;
            Vector2 ViewportPosition = cam.WorldToViewportPoint(point);
            Vector2 WorldObject_ScreenPosition = new Vector2(
             ((ViewportPosition.x * rectTransform.sizeDelta.x) - (rectTransform.sizeDelta.x * 0.5f)),
             ((ViewportPosition.y * rectTransform.sizeDelta.y) - (rectTransform.sizeDelta.y * 0.5f)));
            return WorldObject_ScreenPosition;
        }
        //��� ��ü�� ��� �������� ������(����)
        public static Vector3 GetPointOffBoundsCenter(this Transform target, float _heightOffset)
        {
            //�� �κ��� �Ű������� ������ ���
            var bounds = target.GetComponent<Collider>().bounds;
            var middle = bounds.center;
            var height = Vector3.Distance(bounds.min, bounds.max);

            var point = middle + new Vector3(0, height + _heightOffset, 0);
            return point;
        }

    }
}