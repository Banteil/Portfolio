using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Zeus
{
    public class PlayerQuickTabItemUI : MonoBehaviour
    {
        internal Vector2 IconPosition { get { return _icon != null ? _icon.rectTransform.position :  Vector2.zero; } }

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Image _frame;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _selectedImage;

        [SerializeField] private Color _selectColor;
        [SerializeField] private Color _unSelectColor;


        internal bool ShowCount;
        internal int ItemID;

        private Action<int> _onUse;

        public void Set(int id, string name, int count, Sprite icon, Action<int> onUseAction)
        {
            ItemID = id;

            _nameText.gameObject.SetActive(!string.IsNullOrEmpty(name));
            _nameText.SetText(name);

            _countText.gameObject.SetActive(ShowCount);
            _countText.SetText(count.ToString());

            _icon.overrideSprite = icon;

            _onUse = onUseAction;
        }

        public void Select()
        {
            _canvasGroup.alpha = 1f;
            _selectedImage.enabled = true;

            _frame.color = _selectColor;
            _icon.color = _selectColor;
            _nameText.color = _selectColor;
            _countText.color = _selectColor;
        }
        public void UnSelect()
        {
            _canvasGroup.alpha = 1f;
            _selectedImage.enabled = false;

            _frame.color = _unSelectColor;
            _icon.color = _unSelectColor;
            _nameText.color = _unSelectColor;
            _countText.color = _unSelectColor;
        }

        public void OnUse()
        {
            if (ItemID == -1) return;
            if (_onUse == null) return;
            _onUse.Invoke(ItemID);
        }
    }
}