using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class VirtualController : UIBase
    {
        public float InputH { get; set; }
        public float InputV { get; set; }
        public InputState InputRunButton { get; set; } = InputState.None;
        public InputState InputJumpButton { get; set; } = InputState.None;

        [SerializeField] private Button _runButton;

        protected override void OnStart()
        {
            base.OnStart();
            if (Util.IsMobileWebPlatform)
            {
                GameManager.Instance.RequireObjectsSpawnCallback += () =>
                {
                    var editUI = FindAnyObjectByType<UIEditExhibitsPopup>();
                    editUI.EnableCallback += EnableEditUI;
                };
            }
        }

        public void EnableController(bool isMobile)
        {
            Debug.Log($"isMobile : {isMobile}");
            if (!isMobile)
                gameObject.SetActive(false);
            else
            {
                var canvas = gameObject.GetComponent<Canvas>();
                canvas.enabled = true;
            }
        }

        public void OnDragMoveHandle(Vector2 inputVector)
        {
            InputH = inputVector.x;
            InputV = inputVector.y;
        }

        public void ToggleRunButton()
        {
            if(InputRunButton == InputState.None)
            {
                InputRunButton = InputState.Stay;
                _runButton.targetGraphic.color = _runButton.colors.pressedColor;
            }
            else
            {
                InputRunButton = InputState.None;
                _runButton.targetGraphic.color = _runButton.colors.normalColor;
            }
        }

        public void InactiveRunButton()
        {
            InputRunButton = InputState.None;
            _runButton.targetGraphic.color = _runButton.colors.normalColor;
        }

        public void SetInputRunState(InputState state) => InputRunButton = state;
        public void SetInputJumpState(InputState state) => InputJumpButton = state;

        private void EnableEditUI(bool isEnable)
        {
            InactiveRunButton();
            transform.GetChild(1).gameObject.SetActive(!isEnable);

            var controllerRT = (RectTransform)transform.GetChild(0);
            controllerRT.anchorMax = isEnable ? new Vector2(1, 0) : new Vector2(0, 0);
            controllerRT.anchorMin = isEnable ? new Vector2(1, 0) : new Vector2(0, 0);
            controllerRT.pivot = isEnable ? new Vector2(1, 0) : new Vector2(0, 0);
            controllerRT.anchoredPosition = new Vector2(isEnable ? -80 : 120f, 85f);
        }
    }
}
