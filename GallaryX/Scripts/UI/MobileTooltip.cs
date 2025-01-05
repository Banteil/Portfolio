using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.gallaryx
{
    public class MobileTooltip : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public string TooltipKey;
        [SerializeField] private GameObject _tooltipObj;
        [SerializeField] private TextMeshProUGUI _tooltipText;

        public void OnPointerDown(PointerEventData eventData)
        {
            var text = Util.GetLocalizedString("UITable", TooltipKey);
            var regexText = Regex.Replace(text, @"\[[^\]]*\]", string.Empty);
            _tooltipText.text = regexText;
            _tooltipObj.SetActive(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _tooltipObj.SetActive(false);
        }
    }
}
