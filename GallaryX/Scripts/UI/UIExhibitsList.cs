using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class UIExhibitsList : UIList
    {
        enum ExhibitsListText
        {
            IndexText,
        }

        enum ExhibitsListRawImage
        {
            ExhibitsImage,
        }

        private float _maxWidth, _maxHeight;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Transform _originalParent;
        private ScrollRect _scrollRect;
        private RectTransform _scrollRectTransform;
        private float _spacing;

        protected override void OnAwake()
        {
            SetParent<UIEditExhibitsPopup>();
            Bind<TextMeshProUGUI>(typeof(ExhibitsListText));
            Bind<RawImage>(typeof(ExhibitsListRawImage));

            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _scrollRect = _parentUI.GetScrollRect(0);
            _originalParent = _rectTransform.parent;
            _scrollRectTransform = _scrollRect.GetComponent<RectTransform>();
            _spacing = _scrollRect.content.GetComponent<VerticalLayoutGroup>().spacing;

            var textureRT = (RectTransform)GetRawImage((int)ExhibitsListRawImage.ExhibitsImage).transform.parent;
            _maxWidth = textureRT.rect.width;
            _maxHeight = textureRT.rect.height;

        }

        public override void SetIndex(int index)
        {
            base.SetIndex(index);
            GetText((int)ExhibitsListText.IndexText).text = (index + 1).ToString();
        }

        public void SetTexture(Texture texture)
        {
            if (texture == null)
            {
                Debug.LogWarning($"ExhibitsList Index : {_index} texture is null");
                return;
            }
            var textureUI = GetRawImage((int)ExhibitsListRawImage.ExhibitsImage);
            textureUI.texture = texture;
            var rectImageUI = textureUI.GetComponent<RectTransform>();
            var width = texture.width / (float)texture.height * _maxHeight;
            var height = texture.height / (float)texture.width * _maxWidth;

            if (height > width)
                rectImageUI.sizeDelta = new Vector2(width, _maxHeight);
            else
                rectImageUI.sizeDelta = new Vector2(_maxWidth, height);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            _isDrag = true;

            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;
            _rectTransform.SetParent(_scrollRect.content);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            Vector3 mousePosition = new Vector3(eventData.position.x, eventData.position.y, _rectTransform.position.z);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_scrollRectTransform, mousePosition, eventData.pressEventCamera, out localPoint))
            {
                // Calculate new position, keeping X fixed and allowing Y to move
                Vector3 newPosition = _rectTransform.position;
                newPosition.x = _rectTransform.position.x; // Keep X fixed
                newPosition.y = _scrollRectTransform.TransformPoint(localPoint).y;
                _rectTransform.position = newPosition;

                UpdateItemPosition();
                HandleAutoScroll(localPoint);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            _isDrag = false;

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _rectTransform.SetParent(_originalParent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);

            SetIndexUnderPointer(_rectTransform.GetSiblingIndex());
        }

        private void UpdateItemPosition()
        {
            float itemHeight = _rectTransform.rect.height + _spacing;
            float yPosition = _rectTransform.localPosition.y;
            int newIndex = Mathf.Clamp((int)((-yPosition + itemHeight / 2) / itemHeight), 0, _scrollRect.content.childCount - 1);

            if (newIndex != _rectTransform.GetSiblingIndex())
            {
                _rectTransform.SetSiblingIndex(newIndex);
            }
        }

        private void SetIndexUnderPointer(int index)
        {
            var oldIndex = GetIndex();
            if (oldIndex == index) return;
            MainSceneManager.Instance.Exhibition.SwapExhibitIndices(oldIndex, index);
            MainSceneManager.Instance.Exhibition.SwapSeqDataInfo(oldIndex, index);            

            var seqDataList = MainSceneManager.Instance.Exhibition.SeqDataList;
            var filteredSeqDataList = seqDataList
                .Where(item => !item.type.Equals(Define.FileType.EMPTY.ToString(), StringComparison.OrdinalIgnoreCase))
                .ToList();
            CallAPI.SetMediaData(filteredSeqDataList);

            var childrens = _scrollRect.content.GetComponentsInChildren<UIExhibitsList>();
            for (int i = 0; i < childrens.Length; i++)
            {
                childrens[i].SetIndex(i);
            }
        }

        private void HandleAutoScroll(Vector2 localPoint)
        {
            if (_scrollRect == null || _scrollRectTransform == null) return;

            float scrollRectHeight = _scrollRectTransform.rect.height;
            float mouseY = localPoint.y;

            // Determine the scroll direction
            var scrollThreshold = GetParent<UIEditExhibitsPopup>().ScrollThreshold;
            var scrollSpeed = GetParent<UIEditExhibitsPopup>().ScrollSpeed;
            if (mouseY > scrollRectHeight / 2f - scrollThreshold)
            {
                // Scroll down
                ScrollVertical(scrollSpeed);
            }
            else if (mouseY < -scrollRectHeight / 2f + scrollThreshold)
            {
                // Scroll up
                ScrollVertical(-scrollSpeed);
            }
        }

        private void ScrollVertical(float direction)
        {
            // Adjust scroll position based on the direction
            if (_scrollRect != null)
            {
                float newValue = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition + direction * Time.deltaTime);
                _scrollRect.verticalNormalizedPosition = newValue;
            }
        }
    }
}
