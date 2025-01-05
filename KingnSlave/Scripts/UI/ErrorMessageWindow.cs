using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class ErrorMessageWindow : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text errorMessage;

        [SerializeField]
        private Button closeButton;

        private void Start()
        {
            closeButton.onClick.AddPersistentListener(ClickCloseButton);
        }

        public void PopUp(string message)
        {
            gameObject.SetActive(true);
            errorMessage.text = message;
        }

        private void ClickCloseButton()
        {
            gameObject.SetActive(false);
        }
    }
}