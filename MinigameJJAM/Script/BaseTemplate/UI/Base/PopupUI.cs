using UnityEngine.EventSystems;

namespace starinc.io
{
    public class PopupUI : BaseUI
    {
        protected override void OnAwake()
        {
            Manager.UI.SetCanvas(gameObject, true);
        }

        protected virtual void OnCloseButtonClicked(PointerEventData data)
        {
            Manager.Sound.PlaySFX("buttonTouch");
            Manager.UI.FindClosePopup(this);
        }
    }
}
