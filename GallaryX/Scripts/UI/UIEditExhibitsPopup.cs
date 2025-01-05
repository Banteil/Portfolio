using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class UIEditExhibitsPopup : UIPopup
    {
        public float ScrollThreshold { get; set; } = 50f; // 자동 스크롤 범위 (픽셀)
        public float ScrollSpeed { get; set; } = 0.5f; // 스크롤 속도

        enum EditScrollRect
        {
            ExhibitsScrollView,
        }

        enum EditToggle
        {
            GuideToggle,
        }

        enum EditGameObject
        {
            GuideObjects,
            ArrowImage,
        }

        enum EditButton
        {
            HideButton = 1,
        }

        public bool IsEnable
        {
            get { return UICanvas.enabled; }
            set
            {
                UICanvas.enabled = value;
                if(value)
                {
                    var guideObjTr = (RectTransform)Get<GameObject>((int)EditGameObject.GuideObjects).transform;
                    var hideButtonTr = (RectTransform)GetButton((int)EditButton.HideButton).transform;
                    if (Util.IsLandscape)
                    {
                        guideObjTr.localScale = Vector3.one;
                        guideObjTr.anchoredPosition -= new Vector2(guideObjTr.sizeDelta.x * 0.25f, 400f);
                        hideButtonTr.anchorMax = new Vector2(1, 0.5f);
                        hideButtonTr.anchorMin = new Vector2(1, 0.5f);
                        hideButtonTr.pivot = new Vector2(1, 0.5f);
                        hideButtonTr.anchoredPosition = new Vector2(80f, 0f);
                    }
                    else
                    {
                        guideObjTr.localScale = new Vector3(1.5f, 1.5f);
                        guideObjTr.anchoredPosition += new Vector2(guideObjTr.sizeDelta.x * 0.25f, 400f);
                        hideButtonTr.anchorMax = new Vector2(1, 0);
                        hideButtonTr.anchorMin = new Vector2(1, 0);
                        hideButtonTr.pivot = new Vector2(1, 0);
                        hideButtonTr.anchoredPosition = new Vector2(80f, 100f);
                    }
                }
                else
                {
                    UIManager.Instance.RemovePopupUI<UIEditExhibitsPopup>();
                    var menuRect = (RectTransform)transform;
                    menuRect.anchoredPosition = Vector2.zero;
                    var arrow = Get<GameObject>((int)EditGameObject.ArrowImage).transform;
                    arrow.transform.rotation = Quaternion.Euler(0, 0, 0);
                    _isHide = false;
                }
                EnableCallback?.Invoke(value);
            }
        }

        private bool _isHide;
        private Tween _slideTween;

        public Action<bool> EnableCallback;

        protected override void OnAwake()
        {
            Bind<Button>(typeof(PopupButtons));
            var button = GetButton((int)PopupButtons.CloseButton);
            if (button != null) button.gameObject.BindEvent(OnCloseButtonClicked);
            Bind<Button>(typeof(EditButton));
            var hideButton = GetButton((int)EditButton.HideButton);
            hideButton.gameObject.BindEvent(HideEditMenu);

            Bind<ScrollRect>(typeof(EditScrollRect));
            Bind<Toggle>(typeof(EditToggle));
            Get<Toggle>((int)EditToggle.GuideToggle).onValueChanged.AddListener(ViewGuide);
            Bind<GameObject>(typeof(EditGameObject));

            GameManager.Instance.RequireObjectsSpawnCallback += SetExhibitsInfo;
        }

        protected override void ScreenOrientationChanged(bool isLandscape)
        {
            base.ScreenOrientationChanged(isLandscape);
            var guideObjTr = (RectTransform)Get<GameObject>((int)EditGameObject.GuideObjects).transform;
            var hideButtonTr = (RectTransform)GetButton((int)EditButton.HideButton).transform;
            if (isLandscape)
            {
                guideObjTr.localScale = Vector3.one;
                guideObjTr.anchoredPosition -= new Vector2(guideObjTr.sizeDelta.x * 0.25f, 400f);
                hideButtonTr.anchorMax = new Vector2(1, 0.5f);
                hideButtonTr.anchorMin = new Vector2(1, 0.5f);
                hideButtonTr.pivot = new Vector2(1, 0.5f);
                hideButtonTr.anchoredPosition = new Vector2(80f, 0f);
            }
            else
            {
                guideObjTr.localScale = new Vector3(1.5f, 1.5f);
                guideObjTr.anchoredPosition += new Vector2(guideObjTr.sizeDelta.x * 0.25f, 400f);
                hideButtonTr.anchorMax = new Vector2(1, 0);
                hideButtonTr.anchorMin = new Vector2(1, 0);
                hideButtonTr.pivot = new Vector2(1, 0);
                hideButtonTr.anchoredPosition = new Vector2(80f, 100f);
            }
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            UIManager.Instance.RemovePopupUI<UISettingPopup>();
            IsEnable = false;
        }

        private void HideEditMenu(PointerEventData data)
        {
            if (_slideTween != null && _slideTween.IsPlaying()) return;
            var menuRect = (RectTransform)transform;

            _slideTween = menuRect.DOAnchorPosX(_isHide ? 0f : -menuRect.sizeDelta.x, 0.3f).SetEase(Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        _isHide = !_isHide;
                        var arrow = Get<GameObject>((int)EditGameObject.ArrowImage).transform;
                        arrow.transform.rotation = Quaternion.Euler(0, _isHide ? 180 : 0, 0); 
                        _slideTween = null;
                    });
        }

        private void ViewGuide(bool isView)
        {
            Get<GameObject>((int)EditGameObject.GuideObjects).SetActive(isView);
        }

        private void SetExhibitsInfo()
        {
            if (!MainSceneManager.HasInstance) return;
            var exhibitsList = MainSceneManager.Instance.Exhibition.ExhibitsList;
            var content = GetScrollRect((int)EditScrollRect.ExhibitsScrollView).content;
            for (int i = 0; i < exhibitsList.Count; i++)
            {
                var exhibitsListUI = UIManager.Instance.AddListUI<UIExhibitsList>(i, content, "ExhibitsListUI");
                if (exhibitsList[i].ExhibitsType == Define.FileType.EMPTY) continue;
                exhibitsListUI.SetTexture(exhibitsList[i].GetTexture());
            }
        }
    }
}