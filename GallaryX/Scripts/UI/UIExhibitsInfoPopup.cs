using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class UIExhibitsInfoPopup : UIPopup
    {
        protected enum InfoText
        {
            NameText,
            SubnameText,
            InfoText,
            SubInfoText,
            PriceText,
        }

        protected enum InfoImage
        {
            Background,
        }

        protected enum InfoCanvasGroup
        {
            LandscapeInfoFrame,
            PortraitInfoFrame,
        }

        private CanvasGroup _mainGroup;
        private RawImage _textureUI;
        private Button _buyButton;
        private TextMeshProUGUI _nameText, _subNameText, _infoText, _subInfoText, _priceText;

        private float _maxWidth, _maxHeight;
        private string _url;

        protected override void OnAwake()
        {
            UIManager.Instance.SetCanvas(gameObject, true);
            UICanvas.sortingOrder = UIManager.Instance.GlobalUI.UICanvas.sortingOrder + UIManager.Instance.Order;
            UIManager.Instance.InteractUI = true;
            _prevUICallback += () => UIManager.Instance.InteractUI = false;

            Bind<Image>(typeof(InfoImage));
            var background = GetImage((int)InfoImage.Background);
            background.gameObject.BindEvent(OnCloseButtonClicked);

            Bind<CanvasGroup>(typeof(InfoCanvasGroup));
            if (Util.IsMobileWebPlatform)
            {
                _mainGroup = Get<CanvasGroup>((int)(Util.IsLandscape ? InfoCanvasGroup.LandscapeInfoFrame : InfoCanvasGroup.PortraitInfoFrame));
                Get<CanvasGroup>((int)(Util.IsLandscape ? InfoCanvasGroup.PortraitInfoFrame : InfoCanvasGroup.LandscapeInfoFrame)).gameObject.SetActive(false);
            }
            else
            {
                _mainGroup = Get<CanvasGroup>((int)InfoCanvasGroup.LandscapeInfoFrame);
                Get<CanvasGroup>((int)InfoCanvasGroup.PortraitInfoFrame).gameObject.SetActive(false);
            }
            _mainGroup.alpha = 1f;
            _mainGroup.blocksRaycasts = true;
            _mainGroup.interactable = true;

            var frame = _mainGroup.transform.Find("TextureFrame");
            frame.gameObject.BindEvent(PopupImage);
            var frameRect = frame.GetComponent<RectTransform>();
            _maxWidth = frameRect.rect.width;
            _maxHeight = frameRect.rect.height;

            _textureUI = frame.GetComponentInChildren<RawImage>();


            _nameText = _mainGroup.transform.FindChildRecursive(InfoText.NameText.ToString()).GetComponent<TextMeshProUGUI>();
            _subNameText = _mainGroup.transform.FindChildRecursive(InfoText.SubnameText.ToString()).GetComponent<TextMeshProUGUI>();
            _infoText = _mainGroup.transform.FindChildRecursive(InfoText.InfoText.ToString()).GetComponent<TextMeshProUGUI>();
            _subInfoText = _mainGroup.transform.FindChildRecursive(InfoText.SubInfoText.ToString()).GetComponent<TextMeshProUGUI>();
            _priceText = _mainGroup.transform.FindChildRecursive(InfoText.PriceText.ToString()).GetComponent<TextMeshProUGUI>();

            _buyButton = _mainGroup.transform.FindChildRecursive("BuyButton").GetComponent<Button>();
        }

        protected override void ScreenOrientationChanged(bool isLandscape)
        {
            base.ScreenOrientationChanged(isLandscape);
            var prevGroup = _mainGroup;
            var prevName = _nameText.text;
            var prevSubName = _subNameText.text;
            var prevInfo = _infoText.text;
            var prevSubInfo = _subInfoText.text;
            var prevPrice = _priceText.text;
            var prevTextureUI = _textureUI;

            _mainGroup = Get<CanvasGroup>((int)(isLandscape ? InfoCanvasGroup.LandscapeInfoFrame : InfoCanvasGroup.PortraitInfoFrame));
            _mainGroup.alpha = prevGroup.alpha;
            _mainGroup.interactable = prevGroup.interactable;
            _mainGroup.blocksRaycasts = prevGroup.blocksRaycasts;
            _mainGroup.gameObject.SetActive(prevGroup.gameObject.activeSelf);
            prevGroup.gameObject.SetActive(false);

            var frame = _mainGroup.transform.Find("TextureFrame");
            frame.gameObject.BindEvent(PopupImage);
            var frameRect = frame.GetComponent<RectTransform>();
            _maxWidth = frameRect.rect.width;
            _maxHeight = frameRect.rect.height;

            _textureUI = frame.GetComponentInChildren<RawImage>();
            _textureUI.texture = prevTextureUI.texture;
            var rectImageUI = _textureUI.GetComponent<RectTransform>();
            var width = _textureUI.texture.width / (float)_textureUI.texture.height * _maxHeight;
            var height = _textureUI.texture.height / (float)_textureUI.texture.width * _maxWidth;

            if (height > width)
                rectImageUI.sizeDelta = new Vector2(width, _maxHeight);
            else
                rectImageUI.sizeDelta = new Vector2(_maxWidth, height);
            prevTextureUI.texture = null;

            _nameText = _mainGroup.transform.FindChildRecursive(InfoText.NameText.ToString()).GetComponent<TextMeshProUGUI>();
            _nameText.text = prevName;
            _subNameText = _mainGroup.transform.FindChildRecursive(InfoText.SubnameText.ToString()).GetComponent<TextMeshProUGUI>();
            _subNameText.text = prevSubName;
            _infoText = _mainGroup.transform.FindChildRecursive(InfoText.InfoText.ToString()).GetComponent<TextMeshProUGUI>();
            _infoText.text = prevInfo;
            _subInfoText = _mainGroup.transform.FindChildRecursive(InfoText.SubInfoText.ToString()).GetComponent<TextMeshProUGUI>();
            _subInfoText.text = prevSubInfo;
            _priceText = _mainGroup.transform.FindChildRecursive(InfoText.PriceText.ToString()).GetComponent<TextMeshProUGUI>();
            _priceText.text = prevPrice;

            _buyButton = _mainGroup.transform.FindChildRecursive("BuyButton").GetComponent<Button>();
            _buyButton.gameObject.SetActive(!string.IsNullOrEmpty(_url));
        }

        public void SetInfo(Exhibits exhibits)
        {
            _textureUI.texture = exhibits.GetTexture();
            var rectImageUI = _textureUI.GetComponent<RectTransform>();
            var width = _textureUI.texture.width / (float)_textureUI.texture.height * _maxHeight;
            var height = _textureUI.texture.height / (float)_textureUI.texture.width * _maxWidth;

            if (height > width)
                rectImageUI.sizeDelta = new Vector2(width, _maxHeight);
            else
                rectImageUI.sizeDelta = new Vector2(_maxWidth, height);

            _nameText.text = exhibits.ExhibitionData.title;
            _subNameText.text = exhibits.ExhibitionData.subtitle;
            _infoText.text = Util.FilterUnsupportedTags(exhibits.ExhibitionData.description);
            
            var price = exhibits.ExhibitionData.price;
            if (string.IsNullOrEmpty(price))
                _priceText.gameObject.SetActive(false);
            else
                _priceText.text = $"${price}";
            _subInfoText.text = $"{exhibits.ExhibitionData.created_at}, {exhibits.ExhibitionData.size}, {exhibits.ExhibitionData.materials}";

            _url = exhibits.ExhibitionData.nft_market_url != null ? exhibits.ExhibitionData.nft_market_url : "";
            _buyButton.gameObject.SetActive(!string.IsNullOrEmpty(_url));
        }

        public void BuyButton()
        {
            Application.OpenURL(_url);
        }

        public void CloseButton() => OnCloseButtonClicked(null);

        private void PopupImage(PointerEventData data)
        {
            var popupUI = UIManager.Instance.ShowPopupUI<FullscreenExhibitsUI>("FullscreenExhibitsUI");
            popupUI.SetInfo(_textureUI.texture);
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            UIManager.Instance.CloseSpecificPopupUI<UIExhibitsInfoPopup>(_prevUICallback);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (MainSceneManager.HasInstance)
                MainSceneManager.Instance.Player.IsInteraction = false;
        }
    }
}
