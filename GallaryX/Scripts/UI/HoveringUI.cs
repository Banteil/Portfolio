using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class HoveringUI : MonoBehaviour
    {
        private CanvasGroup _frame;
        private TextMeshProUGUI _infoText;

        private void Awake()
        {
            _frame = GetComponent<CanvasGroup>();
            _infoText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public string InfoText
        {
            get { return _infoText.text; }
            set { _infoText.text = value; }
        }

        public void ActiveUI() => _frame.alpha = 1;
    }
}
