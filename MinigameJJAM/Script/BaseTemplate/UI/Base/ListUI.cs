using UnityEngine.EventSystems;

namespace starinc.io
{
    public interface IListUI
    {
        public void SetListData(ListUI listUI);
    }

    public class ListUI : BaseUI, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        protected int _index;
        protected bool _isDrag;
        public bool IsDrag { get { return _isDrag; } set { _isDrag = value; } }
        protected BaseUI _parentUI;

        public virtual void SetIndex(int index)
        {
            _index = index;
        }

        public int GetIndex() => _index;
        public void SetParent<T>() where T : BaseUI { _parentUI = Util.FindComponentInParents<T>(transform); }
        public void SetParent(BaseUI parentUI) { _parentUI = parentUI; }
        protected T GetParent<T>() where T : BaseUI { return _parentUI as T; }

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

        public virtual void Reset() { }
    }
}
