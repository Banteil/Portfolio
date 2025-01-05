using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io
{
    public class PhaseInfo : MonoBehaviour
    {
        private Image _drinkImage;
        private TextMeshProUGUI _iceCount;

        private void Awake()
        {
            _drinkImage = GetComponentInChildren<Image>();
            _iceCount = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetInfo(Sprite drinkSprite, int count)
        {
            _drinkImage.sprite = drinkSprite;
            _iceCount.text = count.ToString();
        }
    }
}
