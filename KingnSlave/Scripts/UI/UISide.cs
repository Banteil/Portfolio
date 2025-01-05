using DG.Tweening;
using System;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public enum SideDirection
    {
        UP, DOWN, LEFT, RIGHT
    }

    public class UISide : UIBase
    {
        [SerializeField] protected SideDirection direction;
        [SerializeField] protected float size;
        [SerializeField] protected bool isOnGlobalOrder;

        protected RectTransform areaRectTr;
        protected float AreaSizeX { get { return areaRectTr.sizeDelta.x; } }
        protected float AreaSizeY { get { return areaRectTr.sizeDelta.y; } }

        protected bool IsVertical { get { return direction == SideDirection.UP || direction == SideDirection.DOWN; } }
        protected bool isDirecting = false;

        protected Action prevUICallback = null;

        protected virtual void Awake()
        {
            areaRectTr = (RectTransform)transform.Find("SideArea");
            if (areaRectTr == null)
            {
                UIManager.Instance.CloseUI();
                return;
            }
            OpenDirection();
        }

        protected virtual void OpenDirection()
        {
            isDirecting = true;
            areaRectTr.sizeDelta = new Vector2(IsVertical ? AreaSizeX : 0f, IsVertical ? 0f : AreaSizeY);
            var endValue = new Vector2(IsVertical ? AreaSizeX : size + GetSafeSize(), IsVertical ? size + GetSafeSize() : AreaSizeY);
            areaRectTr.DOSizeDelta(endValue, 0.1f)
                .SetEase(Ease.Linear)
                .OnComplete(() => isDirecting = false);
        }

        protected virtual void CloseDirection(Action callback = null)
        {
            isDirecting = true;
            var endValue = new Vector2(IsVertical ? AreaSizeX : 0f, IsVertical ? 0f : AreaSizeY);
            areaRectTr.DOSizeDelta(endValue, 0.1f)
                .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                callback?.Invoke();
                isDirecting = false;
            });
        }

        private float GetSafeSize()
        {
            var safeArea = Screen.safeArea;
            var minAnchor = safeArea.position;
            var maxAnchor = minAnchor + safeArea.size;
            var screenHeight = Screen.height;

            if (direction == SideDirection.UP)
                return minAnchor.y;
            else if (direction == SideDirection.DOWN)
                return screenHeight - maxAnchor.y;
            else
                return minAnchor.x;
        }

        protected override void InitializedProcess()
        {
            UIManager.Instance.SetCanvas(gameObject, true);

            if (isOnGlobalOrder)
            {
                var canvas = gameObject.GetComponent<Canvas>();
                canvas.sortingOrder += UIManager.Instance.GlobalUI.GlobalOrder;
            }
        }

        public override void SetCallback(Action callback)
        {
            if (callback != null)
                prevUICallback += callback;
        }
    }
}
