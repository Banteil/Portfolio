using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIListButton : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        protected UIList parentListUI;

        protected override void Awake()
        {
            base.Awake();
            SetParent();
        }

        protected void SetParent() 
        { 
            parentListUI = Util.FindComponentInParents<UIList>(transform);
            if (parentListUI == null)
                enabled = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            parentListUI.IsDrag = true;
            var scrollRect = parentListUI.ParentUI?.GetScrollRect(0);
            scrollRect?.OnBeginDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            parentListUI.IsDrag = false;
            var scrollRect = parentListUI.ParentUI?.GetScrollRect(0);
            scrollRect?.OnEndDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var scrollRect = parentListUI.ParentUI?.GetScrollRect(0);
            scrollRect?.OnDrag(eventData);
        }
    }
}
