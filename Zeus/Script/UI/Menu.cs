using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Zeus
{
    public class Menu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] TextMeshProUGUI _menuText;
        public GameObject DataInfoText;
        [SerializeField] UnityEvent _event;
        [SerializeField] private int _stringID;
        [SerializeField] private bool _refreshOnEnable = true;
        [SerializeField] private bool _isEnabled = false;


        protected Toggle _toggle;
        public bool IsOn { get { return _toggle.isOn; } set { _toggle.isOn = value; } }
        public int StringID
        {
            get => _stringID;
            set
            {
                _stringID = value;
                SetText(_stringID);
            }
        }
        public bool IsEnabled => _isEnabled;

        private Color _defaultTextColor;
        public Color DefaultTextColor => _defaultTextColor;


        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
            if (_menuText != null)
                _defaultTextColor = _menuText.color;
        }
        private void OnEnable()
        {
            if (_refreshOnEnable) SetText(_stringID);
        }

        public void Confirm()
        {
            if (IsEnabled) _event?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _toggle.isOn = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _toggle.isOn = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Confirm();
        }

        public void SetText(int stringID)
        {
            if (stringID == -1) return;
            var str = TableManager.GetString(stringID);
            if (string.IsNullOrEmpty(str))
            {
                Debug.LogError($"StringTable에서 [{stringID}]을 찾지 못했습니다.");
            }

            SetText(str);
        }
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }
        public void SetText(string text)
        {
            if (_menuText == null) return;
            _menuText.SetText(text);
        }
        public void SetTextColor(Color color)
        {
            if (_menuText == null) return;
            _menuText.color = color;
        }

        public void AddListener(UnityAction action)
        {
            RemoveListener(action);
            _event?.AddListener(action);
        }
        public void RemoveListener(UnityAction action)
        {
            _event?.RemoveListener(action);
        }
    }
}
