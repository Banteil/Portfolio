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

            // 설탕의 중심(Vector3.zero) 기준으로 반지름 1 내인지 확인
            if (Vector3.Distance(Vector3.zero, position) <= 1f)
            {
                StopDrawing();
                return;
            }

            // 첫 번째 점 처리
            if (_lineRenderer.positionCount == 0)
            {
                _lineRenderer.positionCount++;
                _lineRenderer.SetPosition(0, position);
                _previousPoint = position;
                return;
            }

            // 길이 계산 및 점 추가
            float segmentLength = Vector3.Distance(position, _previousPoint);
            _lineLength += segmentLength;

            if (_lineLength <= _maxLineLength)
            {
                if (!Manager.Sound.IsPlayingSFX("m8sfx_swipe"))
                    Manager.Sound.PlaySFX("m8sfx_swipe");
                // 라인 렌더러 업데이트
                _lineRenderer.positionCount++;
                _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, position);

                // NavMeshObstacle 추가
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
            // 세그먼트 중심 위치 계산
            var midPoint = (start + end) / 2;

            // 장애물 객체 생성
            var obstacle = new GameObject("NavMeshObstacle");
            obstacle.transform.position = midPoint;

            // 장애물 크기 및 방향 설정
            var obstacleComponent = obstacle.AddComponent<NavMeshObstacle>();
            obstacleComponent.carving = true;

            float segmentLength = Vector3.Distance(start, end);
            obstacleComponent.size = new Vector3(segmentLength, _lineWidth * 0.5f, 1f); // 세그먼트 길이와 두께 반영

            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
            obstacle.transform.rotation = Quaternion.Euler(0, 0, angle);
            obstacle.transform.SetParent(transform);
        }

        public void StopDrawing()
        {
            if (!_isDrawing) return;

            _isDrawing = false;

            // 일정 시간 뒤 라인 제거
            _destroyRoutine = StartCoroutine(DestroyAfterLifetime());
        }

        public void DestructionWall()
        {
            if(_destroyRoutine != null) StopCoroutine(_destroyRoutine);

            float duration = 2f; // 깜빡거림 총 시간
            float interval = 0.2f; // 깜빡거림 간격
            var material = _lineRenderer.material;
            Sequence blinkSequence = DOTween.Sequence();
            int loops = Mathf.FloorToInt(duration / interval);
            for (int i = 0; i < loops; i++)
            {
                blinkSequence.Append(material.DOFade(0f, interval / 2)); // Alpha 0으로
                blinkSequence.Append(material.DOFade(1f, interval / 2)); // Alpha 1로
            }
            blinkSequence.OnComplete(() =>
            {
                OnDestructionWall?.Invoke(this);
                OnDestroyWall?.Invoke(this); // 콜백 호출
            });

            // 시퀀스 시작
            blinkSequence.Play();
        }

        private IEnumerator DestroyAfterLifetime()
        {
            yield return new WaitForSeconds(_lifetimeAfterDraw);
            OnDestroyWall?.Invoke(this);
        }
    }
}
