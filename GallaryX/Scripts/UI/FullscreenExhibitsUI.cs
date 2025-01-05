using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class FullscreenExhibitsUI : UIPopup
    {
        enum FullscreenRawImage
        {
            TextureUI,
        }

        enum FullscreenImage
        {
            Background,
        }

        protected override void OnAwake()
        {
            UIManager.Instance.SetCanvas(gameObject, true);
            Bind<RawImage>(typeof(FullscreenRawImage));
            Bind<Image>(typeof(FullscreenImage));
            var close = GetImage((int)FullscreenImage.Background);
            close.gameObject.BindEvent(OnCloseButtonClicked);
        }

        protected override void ScreenOrientationChanged(bool isLandscape)
        {
            base.ScreenOrientationChanged(isLandscape);
            var textureUI = GetRawImage((int)FullscreenRawImage.TextureUI);
            SetInfo(textureUI.texture);
        }

        public void SetInfo(Texture texture)
        {
            var textureUI = GetRawImage((int)FullscreenRawImage.TextureUI);
            textureUI.texture = texture;
            var imageUIRect = textureUI.GetComponent<RectTransform>();

            // 캔버스의 너비와 높이
            float canvasWidth = Util.IsLandscape ? 1920 : 1080;
            float canvasHeight = Util.IsLandscape ? 1080 : 1920;

            // 이미지의 너비와 높이
            float textureWidth = textureUI.texture.width;
            float textureHeight = textureUI.texture.height;

            // 이미지 비율
            float textureAspect = textureWidth / textureHeight;
            float canvasAspect = canvasWidth / canvasHeight;

            // 비율에 따라 크기를 조정
            if (textureAspect > canvasAspect)
            {
                // 이미지가 캔버스보다 넓은 경우
                float newHeight = canvasWidth / textureAspect;
                imageUIRect.sizeDelta = new Vector2(canvasWidth, newHeight);
            }
            else
            {
                // 이미지가 캔버스보다 높은 경우
                float newWidth = canvasHeight * textureAspect;
                imageUIRect.sizeDelta = new Vector2(newWidth, canvasHeight);
            }
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            UIManager.Instance.CloseSpecificPopupUI<FullscreenExhibitsUI>(_prevUICallback);
        }

        protected override void OnDestroy()
        {            
            base.OnDestroy();
        }
    }
}
