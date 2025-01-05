using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIItemList : UIList
    {
        protected enum ItemListText
        {
            ItemNameText,
        }

        protected enum ItemListImage
        {
            ItemImage,
        }

        protected Button itemListButton;
        protected Image itemListImage;

        private void Awake() => Initialized();

        protected override void InitializedProcess()
        {
            SetParent<UIItemFrame>();
            Bind<TextMeshProUGUI>(typeof(ItemListText));
            Bind<Image>(typeof(ItemListImage));
            itemListButton = GetComponent<Button>();
            itemListImage = GetComponent<Image>();
            gameObject.BindEvent(SelectItem);
        }

        public virtual void SetListData(ItemData data)
        {
            GetText((int)ItemListText.ItemNameText).text = data.name;

            NetworkManager.Instance.GetSprite((sprite) =>
            {
                var image = GetImage((int)ItemListImage.ItemImage);
                if (image != null)
                    image.sprite = sprite;
            }, data.image_url);
        }

        public virtual void SelectItem(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
        }
    }
}
