using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace starinc.io
{
    public class IceCreamGame : MinigameBase
    {
        #region Cache
        private const float SCOOP_POINT = 15f;
        private const int STACK_LIMIT = 8;
        private const float SCOOP_SPEED_LIMIT = 30f;

        [SerializeField]
        private IceCreamTable _table;
        [SerializeField]
        private Transform _scoop;
        [SerializeField]
        private Transform _cone;
        private float _scoopMoveSpeed = 10f;

        private List<IceCream> _stackIceCreams = new List<IceCream>();
        private IceCream _scoopInIceCream;

        private List<GameObject> _clouds = new List<GameObject>();
        private bool _isDrop = false;
        #endregion

        public override void Initialization()
        {

        }

        public override void StartProcess()
        {
            base.StartProcess();
            SpwanIceCream();
            Manager.Input.OnClickEvent += TouchEvent;
        }

        private void TouchEvent(InputAction.CallbackContext context)
        {
            Vector2 touchPosition = Vector2.zero;
            bool isInputValid = false;
#if UNITY_EDITOR
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    touchPosition = Mouse.current.position.ReadValue();
                    isInputValid = true;
                }
            }
#else
            if (Touchscreen.current != null)
            {
                // 터치가 Ended 단계일 때만 처리
                var touch = Touchscreen.current.primaryTouch;
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    touchPosition = touch.position.ReadValue();
                    isInputValid = true;
                }
            }
#endif
            if (isInputValid && Util.IsPointerWithinScreenBounds(touchPosition))
            {
                if (Util.IsPointerOverUI(touchPosition, true)) return;
                DropIceCream();
            }
        }

        private void DropIceCream()
        {
            if (!Util.IsInsideViewportPosition(_scoop.transform.position)) return;
            Manager.Sound.PlaySFX("m7sfx_drop");
            _scoopInIceCream.transform.SetParent(_cone);
            _scoopInIceCream.DropIceCream();
            _scoopInIceCream = null;
            _isDrop = true;
        }

        private void SpwanIceCream()
        {
            if (IsGameOver) return;
            var data = _table.GetRandomIceCream();
            var iceCreamObj = _gameObjectTable.GetPrefabObject("IceCream", _scoop, false);
            _scoopInIceCream = iceCreamObj.GetComponent<IceCream>();
            _scoopInIceCream.InitializeIceCream(data);
            _scoopInIceCream.OnStackSuccess += StackSuccess;
        }

        private void StackSuccess(IceCream iceCream)
        {
            Manager.Sound.PlaySFX("m7sfx_drop");
            Score++;
            StackProcess(iceCream);
            SpwanIceCream();
            _isDrop = false;
        }

        private void StackProcess(IceCream iceCream)
        {            
            _stackIceCreams.Add(iceCream);
            if (_stackIceCreams.Count == STACK_LIMIT)
            {
                var height = 0f;
                for (int i = 0; i < _stackIceCreams.Count; i++)
                {
                    _stackIceCreams[i].InactiveIceCream();
                    height += _stackIceCreams[i].GetYSize();
                }
                _stackIceCreams.Clear();

                MoveCameraUp(height);
                if (_scoopMoveSpeed < SCOOP_SPEED_LIMIT)
                    _scoopMoveSpeed++;
            }
        }

        private void MoveCameraUp(float height)
        {
            Manager.Sound.PlaySFX("correct");
            Vector3 newPosition = Camera.main.transform.position + new Vector3(0, height, 0);
            var prevCloud = new List<GameObject>(_clouds);
            CreateClouds(newPosition.y);
            Camera.main.transform.DOMove(newPosition, 0.3f)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    foreach (var cloud in prevCloud)
                    {
                        if (cloud != null)
                            Destroy(cloud);
                    }
                });
        }

        private void CreateClouds(float targetY)
        {
            for (int i = 0; i < 2; i++)
            {
                var cloud = _gameObjectTable.GetPrefabObject("Cloud");
                var cloudSpriteRenderer = cloud.GetComponent<SpriteRenderer>();

                Vector3 viewportBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
                Vector3 viewportTopRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

                float viewportMinX = viewportBottomLeft.x;
                float viewportMaxX = viewportTopRight.x;

                float viewportMinY = targetY - Camera.main.orthographicSize;
                float viewportMaxY = targetY + Camera.main.orthographicSize;

                float randomX = Random.Range(viewportMinX, viewportMaxX);
                float randomY = Random.Range(viewportMinY, viewportMaxY);

                cloud.transform.position = new Vector3(randomX, randomY, 0f);

                float randomScale = Random.Range(0.5f, 3f);
                cloud.transform.localScale = new Vector3(randomScale, randomScale, 1f);

                cloudSpriteRenderer.flipX = Random.value > 0.5f;
                _clouds.Add(cloud);
            }
        }


        protected override void UpdateProcess()
        {
            MoveScoop();
        }

        private void MoveScoop()
        {
            _scoop.transform.Translate(Vector2.right * _scoopMoveSpeed * Time.deltaTime);

            if (_scoop.transform.position.x > SCOOP_POINT && !_isDrop)
            {
                var pos = _scoop.transform.position;
                pos.x = -SCOOP_POINT;
                _scoop.transform.position = pos;
            }
        }

        public override void EndProcess()
        {
            Manager.Input.OnClickEvent -= TouchEvent;
            base.EndProcess();
        }
    }
}
