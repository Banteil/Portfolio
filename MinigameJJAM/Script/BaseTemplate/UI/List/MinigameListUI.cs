using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class MinigameListUI : ListUI
    {
        #region Cache
        public const int BASIC_HEIGHT = 220;
        public const int DETAIL_HEIGHT = 520;
        private bool _isFocused;
        private int _address;

        private enum MinigameListImage
        {
            IconImage,
            ThumbnailImage,
        }

        private enum MinigameListText
        {
            TitleText,
            ScoreText,
        }

        private enum MinigameListCanvasGroup
        {
            DetailInfo,
        }

        #endregion

        #region Callback
        public event Action<int> OnFocusing;
        public event Action<int> OnDefocusing;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            Manager.Game.OnChangedLocale += ChangedLocale;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Manager.Game.OnChangedLocale -= ChangedLocale;
        }

        private void ChangedLocale()
        {
            var minigameEntry = Manager.Game.Minigames.GetEntryByIndex(_index);
            var title = GetText((int)MinigameListText.TitleText);
            title.text = minigameEntry.name;
        }

        protected override void BindInitialization()
        {
            Bind<Image>(typeof(MinigameListImage));
            Bind<TextMeshProUGUI>(typeof(MinigameListText));
            Bind<CanvasGroup>(typeof(MinigameListCanvasGroup));
            gameObject.BindEvent(OnSelectList);
        }

        public override void Reset()
        {
            if (_isFocused)
                HideDetailInfo();
        }

        #region BindEvent
        private void OnSelectList(PointerEventData data)
        {
            if (_isDrag) return;

            if (!_isFocused)
            {
                Manager.Sound.PlaySFX("buttonTouch");
                ShowDetailInfo();
            }
            else
            {
                Manager.Sound.PlayOneShotSFX("changeScene");
                GoToMinigameScene();
            }
        }

        private void OnInputDetection()
        {
            if (EventSystem.current == null) return;
            var selectedObj = EventSystem.current.currentSelectedGameObject;
            if (selectedObj == null || selectedObj != gameObject)
                HideDetailInfo();
        }
        #endregion

        private void ShowDetailInfo()
        {
            if (_isFocused) return;
            var rectTr = (RectTransform)transform;
            Vector2 size = rectTr.sizeDelta;
            size.y = DETAIL_HEIGHT;
            rectTr.sizeDelta = size;

            var content = (RectTransform)rectTr.parent;
            var parentSize = content.sizeDelta;
            parentSize.y += DETAIL_HEIGHT - BASIC_HEIGHT;
            content.sizeDelta = parentSize;

            var detail = Get<CanvasGroup>((int)MinigameListCanvasGroup.DetailInfo);
            detail.alpha = 1;

            EventSystem.current.SetSelectedGameObject(gameObject);
            _isFocused = true;
            OnFocusing?.Invoke(_index);
            Manager.Input.OnInputDetection += OnInputDetection;
        }

        private void HideDetailInfo()
        {
            if (!_isFocused) return;
            var rectTr = (RectTransform)transform;
            Vector2 size = rectTr.sizeDelta;
            size.y = BASIC_HEIGHT;
            rectTr.sizeDelta = size;

            var content = (RectTransform)rectTr.parent;
            var parentSize = content.sizeDelta;
            parentSize.y -= DETAIL_HEIGHT - BASIC_HEIGHT;
            content.sizeDelta = parentSize;

            var detail = Get<CanvasGroup>((int)MinigameListCanvasGroup.DetailInfo);
            detail.alpha = 0;

            EventSystem.current.SetSelectedGameObject(null);
            _isFocused = false;
            OnDefocusing?.Invoke(_index);
            Manager.Input.OnInputDetection -= OnInputDetection;
        }

        private void GoToMinigameScene()
        {
            EventSystem.current.SetSelectedGameObject(null);
            Manager.Game.CurrentGameAddress = _address;
            Manager.Input.OnInputDetection -= OnInputDetection;
            Manager.Load.SceneLoad($"{Define.MINIGAME_SCENE_ADDRESS}{_address}", SceneLoadType.AddressableAsync);
        }

        public async void SetListData(MinigameEntry info, int highScore)
        {
            _address = info.address;
            var title = GetText((int)MinigameListText.TitleText);
            title.text = info.name;
            var score = GetText((int)MinigameListText.ScoreText);
            score.text = highScore.ToString("N0");
            var icon = GetImage((int)MinigameListImage.IconImage);
            icon.sprite = await CallAPI.GetSprite(info.iconUrl);
            var thumbnail = GetImage((int)MinigameListImage.ThumbnailImage);
            thumbnail.sprite = await CallAPI.GetSprite(info.thumbnailUrl);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            Reset();
        }
    }
}
