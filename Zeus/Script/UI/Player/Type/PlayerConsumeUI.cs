using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

namespace Zeus
{
    public class PlayerConsumeUI : PlayerUIType
    {
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Image _cooldownMask;
        [SerializeField] private Image _icon;

        [SerializeField] private Color _useableColor;
        [SerializeField] private Color _unuseableColor;

        private int _consumeID;
        private bool _updateFillAmount = true;

        protected override void Start()
        {
            base.Start();

            UIType = TypePlayerUI.CONSUMABLE;

            RefreshConsume();
        }

        public void RefreshConsume(bool updateFillAmount = true)
        {
            var consumeID = TableManager.CurrentPlayerData.GetEquipConsumeID();
            var tableData = TableManager.GetConsumeTableData(consumeID);
            if (tableData == null) return;

            if (_consumeID != consumeID)
            {
                TableManager.Instance.GetSpriteAsync(tableData.Icon, result =>
                {
                    _icon.overrideSprite = result;
                });
                _consumeID = consumeID;
            }

            var consumeData = TableManager.CurrentPlayerData.GetConsumeData(consumeID);
            if (consumeData == null) return;

            _updateFillAmount = updateFillAmount;

            var fillamount = 0f;

            if (consumeData.Amount == 0) fillamount = 0f;
            else if (GameTimeManager.Instance.GetCoolTime(consumeID) is ConsumeCoolTimer timer)
                fillamount = timer.FillAmount;
            else fillamount = 1f;

            SetCooldown(fillamount);
            _amountText.SetText(consumeData.Amount == 0 ? "0" : consumeData.Amount.ToString());
        }

        public void SetCooldown(float fillAmount)
        {
            _cooldownMask.fillAmount = _updateFillAmount ? fillAmount : 0f;
            _icon.color = Color.Lerp(_useableColor, _unuseableColor, _updateFillAmount ? 1f - fillAmount : 1f);
        }
    }
}
