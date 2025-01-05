using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace starinc.io
{
    public class AntGame : MinigameBase
    {
        #region Cache
        private const float SPAWN_MIN_DISTANCE = 1f;
        private const float SPAWN_MAX_DISTANCE = 3f;
        private const float STAY_TIME = 8f;
        private const float MAX_STAY_TIME = 15f;
        private const float INCREASE_STAY_TIME = 0.5f;

        private float _scoreTimer = 0f;
        private int _phase = 1;
        private bool _isSwipe = false;
        private bool _isPathPartial = false;

        private List<Ant> _ants = new List<Ant>();
        private List<AntWall> _walls = new List<AntWall>();
        private List<AntWall> _wallsToDestruction = new List<AntWall>();
        private AntWall _currentWall;
        #endregion

        public override void StartProcess()
        {
            base.StartProcess();
            PhaseProcess();
            Manager.Input.OnClickEvent += SwipeEvent;
            Manager.Input.OnDragEvent += CreateLine;
        }

        private void PhaseProcess()
        {
            Manager.Sound.PlaySFX("m8sfx_phase");
            for (int i = 0; i < _phase; i++)
            {
                SpawnAnt();
            }
        }

        private void SpawnAnt()
        {
            var antObj = _gameObjectTable.GetPrefabObject("Ant", GetRandomSpawnPosition(), Quaternion.identity);
            var ant = antObj.GetComponent<Ant>();
            var stayTime = STAY_TIME + ((_phase - 1) * INCREASE_STAY_TIME);
            if (stayTime > MAX_STAY_TIME)
                stayTime = MAX_STAY_TIME;
            ant.InitializeAnt(stayTime);
            ant.OnArrived += EndProcess;
            ant.OnDestroyAnt += DestroyAnt;
            ant.OnPathPartial += PathPartial;

            _ants.Add(ant);
        }

        private void DestroyAnt(Ant ant)
        {
            _ants.Remove(ant);
            Destroy(ant.gameObject);
            if (_ants.Count <= 0)
            {
                _phase++;
                PhaseProcess();
            }
        }

        private void PathPartial()
        {
            if (_isPathPartial) return;

            _isPathPartial = true;
            _wallsToDestruction = new List<AntWall>(_walls);
            if(_wallsToDestruction.Count == 0)
            {
                _isPathPartial = false;
                return;
            }

            foreach (var wall in _wallsToDestruction)
            {
                wall.OnDestructionWall += HandleWallDestruction;
                wall.DestructionWall();
            }
        }
        
        private void HandleWallDestruction(AntWall wall)
        {
            _wallsToDestruction.Remove(wall);
            if (_wallsToDestruction.Count == 0)
            {                
                _isPathPartial = false;
            }
            wall.OnDestructionWall -= HandleWallDestruction;
        }

        private Vector3 GetRandomSpawnPosition()
        {
            // 카메라 중심 위치를 기준으로 뷰포트 크기 계산
            float viewportWidth = Camera.main.orthographicSize * Camera.main.aspect;
            float viewportHeight = Camera.main.orthographicSize;

            // 뷰포트 밖 원형 범위의 최소/최대 반경 계산
            float minRadius = Mathf.Max(viewportWidth, viewportHeight) + SPAWN_MIN_DISTANCE;
            float maxRadius = minRadius + (SPAWN_MAX_DISTANCE - SPAWN_MIN_DISTANCE);

            // 랜덤 각도 (360도)
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            // 원형 범위 내에서 거리 랜덤화
            float randomRadius = Random.Range(minRadius, maxRadius);

            // X, Y 좌표 계산 (폴라 좌표 -> 데카르트 좌표 변환)
            float x = Mathf.Cos(randomAngle) * randomRadius;
            float y = Mathf.Sin(randomAngle) * randomRadius;

            // 월드 좌표로 반환 (카메라 중심 기준)
            Vector3 spawnPosition = new Vector3(
                Camera.main.transform.position.x + x,
                Camera.main.transform.position.y + y,
                0f // 2D 환경에서는 Z축 고정
            );

            return spawnPosition;
        }

        protected override void UpdateProcess()
        {
            _scoreTimer += Time.deltaTime;
            Score = (int)_scoreTimer;
        }

        private void CreateLine(InputAction.CallbackContext context)
        {
            if (_isSwipe)
            {
#if UNITY_EDITOR
                var touchPosition = Mouse.current.position.ReadValue();
#else
                var touch = Touchscreen.current.primaryTouch;
                var touchPosition = touch.position.ReadValue();
#endif
                if (Util.IsPointerWithinScreenBounds(touchPosition) && !Util.IsPointerOverUI(touchPosition, true))
                {
                    var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, Mathf.Abs(Camera.main.transform.position.z)));
                    worldPosition.z = 0f;
                    _currentWall?.AddPoint(worldPosition);
                }
            }
        }

        private void DestroyWall(AntWall wall)
        {
            _walls.Remove(wall);
            Destroy(wall.gameObject);
        }

        private void SwipeEvent(InputAction.CallbackContext context)
        {
            // 터치 또는 마우스 포지션 변수 초기화
            Vector2 touchPosition = Vector2.zero;

#if UNITY_EDITOR
            // 마우스 입력 처리
            if (Mouse.current != null)
            {
                // 마우스 왼쪽 버튼이 눌리는 순간 (Input Down)
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    touchPosition = Mouse.current.position.ReadValue();
                    if (Util.IsPointerWithinScreenBounds(touchPosition) && !Util.IsPointerOverUI(touchPosition, true))
                    {
                        var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, Mathf.Abs(Camera.main.transform.position.z)));
                        worldPosition.z = 0f;
                        var wallObj = _gameObjectTable.GetPrefabObject("AntWall", worldPosition, Quaternion.identity);
                        var antWall = wallObj.GetComponent<AntWall>();
                        antWall.OnDestroyWall += DestroyWall;
                        _currentWall = antWall;
                        _walls.Add(antWall);
                        _isSwipe = true;
                    }
                }

                // 마우스 왼쪽 버튼이 떼지는 순간 (Input Up)
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    _currentWall?.StopDrawing();
                    _currentWall = null;
                    _isSwipe = false;
                }
            }
#else
            // 터치 입력 처리
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;

                // 터치 시작 (TouchPhase.Began)
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    touchPosition = touch.position.ReadValue();
                    if (Util.IsPointerWithinScreenBounds(touchPosition) && !Util.IsPointerOverUI(touchPosition, true))
                    {
                        var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, Mathf.Abs(Camera.main.transform.position.z)));
                        worldPosition.z = 0f;
                        var wallObj = _gameObjectTable.GetPrefabObject("AntWall", worldPosition, Quaternion.identity);
                        var antWall = wallObj.GetComponent<AntWall>();
                        antWall.OnDestroyWall += DestroyWall;
                        _currentWall = antWall;
                        _walls.Add(antWall);
                        _isSwipe = true;
                    }
                }

                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    _currentWall?.StopDrawing();
                    _currentWall = null;
                    _isSwipe = false;
                }
            }
#endif
        }

        public override async void EndProcess()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            _onUpdateProcess -= UpdateProcess;
            Manager.Input.OnClickEvent -= SwipeEvent;
            Manager.Input.OnDragEvent -= CreateLine;

            OnEndGame?.Invoke();
            CheckHighScore();

            Manager.Sound.StopBGM(0.5f);
            Manager.Sound.StopAllSFX(0.5f);
            Manager.Sound.PlaySFX("m8sfx_crack");

            foreach (var ant in _ants)
            {
                ant.StopAct();
            }

            await UniTask.WaitForSeconds(1f);

            Manager.UI.ShowPopupUI<GameOverPopupUI>(null, "gameOver");
        }
    }
}
