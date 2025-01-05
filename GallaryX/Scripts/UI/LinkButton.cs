using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class LinkButton : MonoBehaviour
    {
        private string _url;
        public string URL
        {
            get { return _url; }
            set
            {
                _url = value;
                SetIconImage();
            }
        }
        [SerializeField]
        private Image _iconImage;

        private void SetIconImage()
        {
            if(_url.Contains("youtube"))
                _iconImage.sprite = ResourceManager.Instance.GetSprite("youtube");
            else if (_url.Contains("instagram"))
                _iconImage.sprite = ResourceManager.Instance.GetSprite("instagram");
        }

        public void OpenLinkButton() => Application.OpenURL(URL);
    }
}
