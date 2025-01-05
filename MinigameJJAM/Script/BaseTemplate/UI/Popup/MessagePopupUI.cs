using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class MessagePopupUI : PopupUI
    {
        #region Cache
        private const int MESSAGE_ORDER = 9000;
        private const float MIN_FRAME_WIDTH_SIZE = 700f;
        private const float MIN_FRAME_HEIGHT_SIZE = 500f;

        private const string LOCALIZE_CONFIRM_KEY = "message_confirm";
        private const string LOCALIZE_NO_KEY = "message_no";

        private enum MessagePopupText
        {
            MessageText,
            CloseButtonText,
        }

        private enum MessagePopupButton
        {
            ConfirmButton,
            CloseButton,
        }

        private enum MessagePopupRectTransform
        {
            Frame,
        }
        #endregion

        #region Callback
        public event Action OnConfirm;
        #endregion

        protected override void OnStart()
        {
            base.OnStart();
            UICanvas.sortingOrder = MESSAGE_ORDER;
        }

        protected override void BindInitialization()
        {
            Bind<TextMeshProUGUI>(typeof(MessagePopupText));
            Bind<Button>(typeof(MessagePopupButton));
            Bind<RectTransform>(typeof(MessagePopupRectTransform));

            var confirmButton = GetButton((int)MessagePopupButton.ConfirmButton);
            confirmButton.gameObject.BindEvent(OnConfirmButton);
            var closeButton = GetButton((int)MessagePopupButton.CloseButton);
            closeButton.gameObject.BindEvent(OnCloseButtonClicked);
        }

        public void SetMessageInfo(string message, Action confirmCallback = null, Vector2 frameSize = default)
        {
            frameSize = frameSize == default
                ? new Vector2(MIN_FRAME_WIDTH_SIZE, MIN_FRAME_HEIGHT_SIZE)
                : new Vector2(Mathf.Max(frameSize.x, MIN_FRAME_WIDTH_SIZE), Mathf.Max(frameSize.y, MIN_FRAME_HEIGHT_SIZE));

            var frame = Get<RectTransform>((int)MessagePopupRectTransform.Frame);
            frame.sizeDelta = frameSize;

            var messageText = GetText((int)MessagePopupText.MessageText);
            messageText.text = message;

            var closeButtonText = GetText((int)MessagePopupText.CloseButtonText);
            if (confirmCallback == null)
            {
                var confirmButton = GetButton((int)MessagePopupButton.ConfirmButton);
                confirmButton.gameObject.SetActive(false);
                closeButtonText.text = Util.GetLocalizedString(Define.LOCALIZATION_TABLE_UI, LOCALIZE_CONFIRM_KEY);
            }
            else
            {
                OnConfirm += confirmCallback;
                closeButtonText.text = Util.GetLocalizedString(Define.LOCALIZATION_TABLE_UI, LOCALIZE_NO_KEY);
            }
        }

        #region BindEvent
        private void OnConfirmButton(PointerEventData data)
        {
            OnConfirm?.Invoke();
            OnCloseButtonClicked(null);
        }
        #endregion
    }
}
