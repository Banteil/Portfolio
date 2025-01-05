using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace starinc.io
{
    [RequireComponent(typeof(LineRenderer))]
    public class AntWall : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        private float _lineLength = 0f;
        private Vector3 _previousPoint;
        private bool _isDrawing = true;
        private Coroutine _destroyRoutine;

        [SerializeField] private float _maxLineLength = 5f;
        [SerializeField] private float _lifetimeAfterDraw = 5f;
        [SerializeField] private float _lineWidth = 0.1f;

        public event Action<AntWall> OnDestroyWall;
        public event Action<AntWall> OnDestructionWall;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            _lineRenderer.positionCount = 0;
        }

        private void Update()
        {
            if (_isDrawing && _lineLength > _maxLineLength)
            {
                StopDrawing();
            }
        }

        public void AddPoint(Vector3 position)
        {
            if (!_isDrawing) return;

            // ������ �߽�(Vector3.zero) �������� ������ 1 ������ Ȯ��
            if (Vector3.Distance(Vector3.zero, position) <= 1f)
            {
                StopDrawing();
                return;
            }

            // ù ��° �� ó��
            if (_lineRenderer.positionCount == 0)
            {
                _lineRenderer.positionCount++;
                _lineRenderer.SetPosition(0, position);
                _previousPoint = position;
                return;
            }

            // ���� ��� �� �� �߰�
            float segmentLength = Vector3.Distance(position, _previousPoint);
            _lineLength += segmentLength;

            if (_lineLength <= _maxLineLength)
            {
                if (!Manager.Sound.IsPlayingSFX("m8sfx_swipe"))
                    Manager.Sound.PlaySFX("m8sfx_swipe");
                // ���� ������ ������Ʈ
                _lineRenderer.positionCount++;
                _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, position);

                // NavMeshObstacle �߰�
                AddObstacle(_previousPoint, position);

                _previousPoint = position;
            }
            else
            {
                StopDrawing();
            }
        }

        private void AddObstacle(Vector3 start, Vector3 end)
        {
            // ���׸�Ʈ �߽� ��ġ ���
            var midPoint = (start + end) / 2;

            // ��ֹ� ��ü ����
            var obstacle = new GameObject("NavMeshObstacle");
            obstacle.transform.position = midPoint;

            // ��ֹ� ũ�� �� ���� ����
            var obstacleComponent = obstacle.AddComponent<NavMeshObstacle>();
            obstacleComponent.carving = true;

            float segmentLength = Vector3.Distance(start, end);
            obstacleComponent.size = new Vector3(segmentLength, _lineWidth * 0.5f, 1f); // ���׸�Ʈ ���̿� �β� �ݿ�

            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
            obstacle.transform.rotation = Quaternion.Euler(0, 0, angle);
            obstacle.transform.SetParent(transform);
        }

        public void StopDrawing()
        {
            if (!_isDrawing) return;

            _isDrawing = false;

            // ���� �ð� �� ���� ����
            _destroyRoutine = StartCoroutine(DestroyAfterLifetime());
        }

        public void DestructionWall()
        {
            if(_destroyRoutine != null) StopCoroutine(_destroyRoutine);

            float duration = 2f; // �����Ÿ� �� �ð�
            float interval = 0.2f; // �����Ÿ� ����
            var material = _lineRenderer.material;
            Sequence blinkSequence = DOTween.Sequence();
            int loops = Mathf.FloorToInt(duration / interval);
            for (int i = 0; i < loops; i++)
            {
                blinkSequence.Append(material.DOFade(0f, interval / 2)); // Alpha 0����
                blinkSequence.Append(material.DOFade(1f, interval / 2)); // Alpha 1��
            }
            blinkSequence.OnComplete(() =>
            {
                OnDestructionWall?.Invoke(this);
                OnDestroyWall?.Invoke(this); // �ݹ� ȣ��
            });

            // ������ ����
            blinkSequence.Play();
        }

        private IEnumerator DestroyAfterLifetime()
        {
            yield return new WaitForSeconds(_lifetimeAfterDraw);
            OnDestroyWall?.Invoke(this);
        }
    }
}
