using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class ExpressionButton : UIList
    {
        Image image;
        MultiYourExpression parentExpressionUI;

        protected override void InitializedProcess()
        {
            SetParent<MultiYourExpression>();
            image = GetComponent<Image>();
            parentExpressionUI = parentUI.GetComponent<MultiYourExpression>();
            gameObject.BindEvent(OnClick);
        }

        public void SetListData(ItemData data)
        {
            NetworkManager.Instance.GetSprite((sprite) =>
            {
                image.sprite = sprite;
            }, data.image_url);
        }

        public void OnClick(PointerEventData eventData)
        {
            parentExpressionUI.ClickExpression(GetIndex());
        }
    }
}