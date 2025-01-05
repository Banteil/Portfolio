using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIYesOrNo : UIPopup
    {
        private enum YONText
        {
            InfomationText,
        }

        private enum YONButton
        {
            ConfirmButton = 1,
        }

        protected override void Awake()
        {
            base.Awake();
            Initialized();
        }

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TextMeshProUGUI>(typeof(YONText));

            Bind<Button>(typeof(YONButton));
            var confirmButton = GetButton((int)YONButton.ConfirmButton);
            confirmButton.gameObject.BindEvent(SelectConfirmButton);
            GetComponent<Canvas>().sortingOrder = 500;
        }

        protected virtual void SelectConfirmButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            UIManager.Instance.CloseUI(prevUICallback);
        }

        public void SetInfomationText(string text)
        {
            var infoText = Get<TextMeshProUGUI>((int)YONText.InfomationText);
            infoText.text = text;
        }
    }
}
