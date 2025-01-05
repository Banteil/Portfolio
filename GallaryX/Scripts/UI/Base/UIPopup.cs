using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class UIPopup : UIBase
    {
        protected Action _prevUICallback;

        protected enum PopupButtons
        {
            CloseButton,
        }

        protected override void OnAwake()
        {
            UIManager.Instance.SetCanvas(gameObject, true);

            Bind<Button>(typeof(PopupButtons));
            var button = GetButton((int)PopupButtons.CloseButton);
            if(button != null) button.gameObject.BindEvent(OnCloseButtonClicked);
        }

        protected virtual void OnCloseButtonClicked(PointerEventData data) => UIManager.Instance.CloseLastPopupUI(_prevUICallback);

        public void SetCloseCallback(Action callback)
        {
            if (callback != null)
                _prevUICallback += callback;
        }

        public void ClosePopup() => OnCloseButtonClicked(null);

        public Action GetPrevCallback() => _prevUICallback;

        protected override void OnDestroy()
        {
            _prevUICallback = null;
        }
    }
}
