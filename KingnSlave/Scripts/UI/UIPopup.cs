using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIPopup : UIBase
    {
        [SerializeField] protected bool fullScreenPopup = true;
        protected Action prevUICallback;

        protected enum PopupButtons
        {
            CloseButton,
        }

        protected virtual void Awake()
        {
            //OpenDirection();
        }

        protected void OpenDirection()
        {
            var safeArea = transform.Find("SafeArea");
            if (safeArea != null)
            {
                safeArea.localScale = Vector3.zero;
                safeArea.DOScale(Vector3.one, 0.1f)
                .SetEase(Ease.Linear);
            }
        }

        protected override void InitializedProcess()
        {
            UIManager.Instance.SetCanvas(gameObject, true);
            if (fullScreenPopup)
            {
                var canvas = gameObject.GetComponent<Canvas>();
                if (UIManager.Instance.GlobalUI != null)
                    canvas.sortingOrder += UIManager.Instance.GlobalUI.GlobalOrder;
            }

            Bind<Button>(typeof(PopupButtons));
            var button = GetButton((int)PopupButtons.CloseButton);
            button.gameObject.BindEvent(OnCloseButtonClicked);
        }

        protected virtual void OnCloseButtonClicked(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            UIManager.Instance.CloseUI();
        }

        public override void SetCallback(Action callback)
        {
            if (callback != null)
                prevUICallback += callback;
        }

        public override void InputEscape()
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            UIManager.Instance.CloseUI();
        }
    }
}
