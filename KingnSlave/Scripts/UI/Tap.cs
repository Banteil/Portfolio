using System;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class Tap : Toggle
    {        
        private TapMenu _menu;
        public TapMenu Menu { get { return _menu; } }
        public event Action<bool> ActiveCallback; 

        [SerializeField] private GameObject _isOnObject;
        [SerializeField] private GameObject _isOffObject;
        [SerializeField] private bool _activeColorApplication;

        private Color _originalColor;
        private float _originalHeight;
        [SerializeField] private Color _activeColor = Color.black;
        [SerializeField] private float _activeHeight = 0;

        protected override void Awake()
        {
            base.Awake();
            _menu = group.GetComponent<TapMenu>();
            _originalColor = targetGraphic.color;
            var rectTr = (RectTransform)transform;
            _originalHeight = rectTr.sizeDelta.y;
            if (_activeHeight <= 0) _activeHeight = _originalHeight;
        }

        protected override void Start()
        {
            base.Start();
            onValueChanged.AddListener(ActiveObject);
            ActiveObject(isOn);
        }

        public void ActiveObject(bool isOn)
        {
            if (_isOnObject == null || _isOffObject == null) return;
            var rectTr = (RectTransform)transform;
            if(isOn)
            {
                _isOnObject.SetActive(true);
                _isOffObject.SetActive(false);
                rectTr.sizeDelta = new Vector2(_menu.MaxWidth, _activeHeight);
                if (_activeColorApplication)
                {                    
                    targetGraphic.color = _activeColor;
                }
            }
            else
            {
                _isOnObject.SetActive(false);
                _isOffObject.SetActive(true);
                rectTr.sizeDelta = new Vector2(_menu.MinWidth, _originalHeight);
                if (_activeColorApplication)
                {
                    targetGraphic.color = _originalColor;
                }
            }
            ActiveCallback?.Invoke(isOn);
            _menu.ResetLayout();
        }
    }
}
