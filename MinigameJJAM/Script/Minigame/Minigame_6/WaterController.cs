using UnityEngine;
using UnityEngine.U2D;

namespace starinc.io
{
    public class WaterShapeController : MonoBehaviour
    {
        [SerializeField] private string _sfxName = "m6sfx_plop";
        private SpriteShapeController _spriteShapeController;
        [SerializeField] private float _springStrength = 0.1f; // 스프링 강도
        [SerializeField] private float _damping = 0.05f; // 댐핑 값
        [SerializeField] private float _spread = 0.1f; // 물결 확산 정도

        private float[] _velocities; // 물결의 속도
        private float[] _heights; // 표면 점의 현재 높이
        private float[] _originalHeights; // 표면 점의 초기 높이 (원래 모양)

        private int _surfaceStartIndex; // 표면 시작 인덱스
        private int _surfaceEndIndex; // 표면 끝 인덱스
        private bool _isInitialize = false;

        private void Awake()
        {
            _spriteShapeController = GetComponent<SpriteShapeController>();
        }

        private void Update()
        {
            if (!_isInitialize) return;
            SimulateWaterWaves();
            UpdateSplinePoints();
        }

        public void InitializeWater()
        {
            Spline spline = _spriteShapeController.spline;

            int pointCount = spline.GetPointCount();
            if (pointCount < 2)
            {
                Debug.LogError("Spline must have at least 2 points.");
                return;
            }

            // 표면 점 구분
            _surfaceStartIndex = 0; // 첫 번째 점
            _surfaceEndIndex = pointCount - 1; // 마지막 점

            // 배열 초기화
            _velocities = new float[pointCount];
            _heights = new float[pointCount];
            _originalHeights = new float[pointCount]; // 초기 높이 저장

            for (int i = 0; i < pointCount; i++)
            {
                // 로컬 좌표를 월드 좌표로 변환
                Vector3 worldPosition = transform.TransformPoint(spline.GetPosition(i));
                _heights[i] = worldPosition.y;        // 표면 점의 초기 높이
                _originalHeights[i] = worldPosition.y; // 모든 점의 초기 높이 저장
                _velocities[i] = 0f;                  // 초기 속도는 0
            }
            transform.localPosition = Vector3.zero;
            _isInitialize = true;
        }


        private void SimulateWaterWaves()
        {
            // 상단 표면 점만 업데이트
            for (int i = _surfaceStartIndex + 1; i < _surfaceEndIndex; i++)
            {
                float force = -_springStrength * (_heights[i] - _originalHeights[i]); // 원래 높이로 돌아가는 힘
                _velocities[i] += force - _velocities[i] * _damping;
                _heights[i] += _velocities[i];
            }

            // 물결 확산
            float[] leftDeltas = new float[_heights.Length];
            float[] rightDeltas = new float[_heights.Length];

            for (int i = _surfaceStartIndex + 1; i < _surfaceEndIndex; i++)
            {
                if (i > _surfaceStartIndex + 1)
                {
                    leftDeltas[i] = _spread * (_heights[i] - _heights[i - 1]);
                    _velocities[i - 1] += leftDeltas[i];
                }
                if (i < _surfaceEndIndex - 1)
                {
                    rightDeltas[i] = _spread * (_heights[i] - _heights[i + 1]);
                    _velocities[i + 1] += rightDeltas[i];
                }
            }
        }

        private void UpdateSplinePoints()
        {
            Spline spline = _spriteShapeController.spline;

            for (int i = 0; i < spline.GetPointCount(); i++)
            {
                Vector3 position = spline.GetPosition(i);

                if (IsSurfacePoint(i))
                {
                    // 상단 점(표면)의 Y 값을 업데이트
                    position.y = _heights[i];
                }
                else
                {
                    // 하단 점은 초기 위치를 유지
                    position.y = _originalHeights[i];
                }

                spline.SetPosition(i, position);
            }
        }

        private bool IsSurfacePoint(int index)
        {
            // 상단(표면)에 해당하는 점 확인
            return index > _surfaceStartIndex && index < _surfaceEndIndex;
        }

        public void AddSplash(Vector3 position, float force)
        {
            Spline spline = _spriteShapeController.spline;

            // 충돌 위치와 가장 가까운 표면 점 찾기
            int closestIndex = -1;
            float closestDistance = float.MaxValue;

            for (int i = _surfaceStartIndex + 1; i < _surfaceEndIndex; i++)
            {
                // Spline 점을 월드 좌표로 변환
                Vector3 splinePoint = transform.TransformPoint(spline.GetPosition(i));
                float distance = Mathf.Abs(position.x - splinePoint.x);

                if (distance < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance;
                }
            }

            if (closestIndex != -1)
            {
                _velocities[closestIndex] += force; // 물결 효과 적용
            }
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Object"))
            {
                // 충돌 물체의 속도를 기반으로 물결 효과 계산
                Manager.Sound.PlaySFX(_sfxName);
                Rigidbody2D rb = collision.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 collisionPoint = collision.ClosestPoint(transform.position);
                    AddSplash(collisionPoint, rb.linearVelocity.y * 0.03f);
                }
            }
        }
    }
}