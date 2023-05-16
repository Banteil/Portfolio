using UnityEngine;
using UnityEngine.InputSystem;

namespace Zeus
{
    public class UIBase : MonoBehaviour
    {
        internal bool IsEnabled;

        //private void Start()
        //{
        //    EnableControls(true);
        //}

        public virtual void EnableUI(bool enabled)
        {
            if (IsEnabled == enabled) return;
            IsEnabled = enabled;
            EnableControls(enabled);
        }

        public virtual void OnCancel() { }

        public virtual void OnClick() { SoundManager.Instance.Play((int)TypeUISound.MENU_SELECT); }

        public virtual void OnNavigate(Vector2 value)
        {
            if (value.magnitude != 0)
                SoundManager.Instance.Play((int)TypeUISound.MENU_MOVE);
        }

        public virtual void OnPoint(InputAction.CallbackContext context) { }

        public virtual void OnScrollWheel(InputAction.CallbackContext context) { }
        public virtual void OnSubmit()
        {
            SoundManager.Instance.Play((int)TypeUISound.MENU_SELECT);
        }

        // TODO : Enable, Disable시 해당 이벤트 추가 / 제거 동작 필요할듯
        private void EnableControls(bool enabled)
        {
            if (enabled)
            {
                InputReader.Instance.CallNavigate += OnNavigate;
                InputReader.Instance.CallCancel += OnCancel;
                InputReader.Instance.CallClick += OnClick;
                InputReader.Instance.CallSubmit += OnSubmit;
            }
            else
            {
                InputReader.Instance.CallNavigate -= OnNavigate;
                InputReader.Instance.CallCancel -= OnCancel;
                InputReader.Instance.CallClick -= OnClick;
                InputReader.Instance.CallSubmit -= OnSubmit;
            }
        }

        private void OnDestroy()
        {
            if (InputReader.HasInstance && !InputReader.ApplicationIsQuitting)
                EnableUI(false);
        }
    }
}
