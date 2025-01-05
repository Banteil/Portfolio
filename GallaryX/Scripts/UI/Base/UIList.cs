using Unity.VisualScripting;
using UnityEngine.EventSystems;

namespace starinc.io
{
    public interface IUIList<T>
    {
        public void SetListData(in T data);
    }


    public class UIList : UIBase, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        protected int _index;
        protected bool _isDrag;
        public bool IsDrag { get { return _isDrag; } set { _isDrag = value; } }
        protected UIBase _parentUI;

        public virtual void SetIndex(int index)
        {
            _index = index;
        }

        public int GetIndex() => _index;

        protected void SetParent<T>() where T : UIBase { _parentUI = Util.FindComponentInParents<T>(transform); }
        protected T GetParent<T>() where T : UIBase { return _parentUI as T; }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            _isDrag = true;
            var scrollRect = _parentUI?.GetScrollRect(0);
            scrollRect?.OnBeginDrag(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            _isDrag = false;
            var scrollRect = _parentUI?.GetScrollRect(0);
            scrollRect?.OnEndDrag(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            var scrollRect = _parentUI?.GetScrollRect(0);
            scrollRect?.OnDrag(eventData);
        }
    }
}
