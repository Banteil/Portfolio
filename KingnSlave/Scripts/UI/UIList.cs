using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public abstract class UIList : UIBase, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] protected int index;
        protected bool isDrag;
        public bool IsDrag { get { return isDrag; } set { isDrag = value; } }
        protected UIBase parentUI;
        public UIBase ParentUI { get { return parentUI; } }

        public virtual void SetIndex(int index)
        {
            this.index = index;
        }

        public int GetIndex() => index;

        protected void SetParent<T>() where T : UIBase { parentUI = Util.FindComponentInParents<T>(transform); } 
        protected T GetParent<T>() where T : UIBase { return parentUI as T; }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            isDrag = true;
            var scrollRect = parentUI?.GetScrollRect(0);
            scrollRect?.OnBeginDrag(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            isDrag = false;
            var scrollRect = parentUI?.GetScrollRect(0);
            scrollRect?.OnEndDrag(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            var scrollRect = parentUI?.GetScrollRect(0);
            scrollRect?.OnDrag(eventData);
        }
    }
}
