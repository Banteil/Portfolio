using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class SideButtonSelectUI : Selectable
    {
        [SerializeField] private Button leftButton, rightButton;

        public int MinValue = 0;
        public int MaxValue = 10;
        [SerializeField] private int currentValue = 0;
        public int Value
        {
            get { return currentValue; }
            set 
            { 
                currentValue = value;
                OnValueChanged?.Invoke();
            }
        }

        public void SetValue(int value) => currentValue = value; 

        protected override void Start()
        {
            base.Start();
            leftButton.gameObject.BindEvent(OnLeftButtonClick);
            rightButton.gameObject.BindEvent(OnRightButtonClick);
        }

        private void OnLeftButtonClick(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var tempValue = currentValue - 1;
            if (tempValue < MinValue)
                tempValue = MaxValue;
            Value = tempValue;
        }

        private void OnRightButtonClick(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var tempValue = currentValue + 1;
            if (tempValue > MaxValue)
                tempValue = MinValue;
            Value = tempValue;
        }

        public UnityEvent OnValueChanged;
    }
}
