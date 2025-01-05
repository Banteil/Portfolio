using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class UIWarning : UIPopup
    {
        private enum WarnningText
        {
            InfomationText,
        }

        protected override void Awake()
        {
            base.Awake();
            Initialized();
        }

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TextMeshProUGUI>(typeof(WarnningText));
            GetComponent<Canvas>().sortingOrder = 500;
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));            
            UIManager.Instance.CloseUI();
            prevUICallback?.Invoke();
        }

        public void SetInfomationText(string text)
        {
            var infoText = Get<TextMeshProUGUI>((int)WarnningText.InfomationText);
            infoText.text = text;
        }
    }
}
