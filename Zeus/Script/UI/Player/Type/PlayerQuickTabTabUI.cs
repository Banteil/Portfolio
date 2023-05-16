using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeus
{
    public enum TypeQuickTab { CONSUMABLE, RUNESKILL, WEAPONSKILL1, WEAPONSKILL2 }
    public class PlayerQuickTabTabUI : MonoBehaviour
    {
        [SerializeField] private TypeQuickTab _tabType;
        [SerializeField] private Image _selectedImage;
        [SerializeField] private Image _icon;

        [SerializeField] private Color _selectColor;
        [SerializeField] private Color _unSelectColor;

        internal TypeQuickTab TabType => _tabType;

        public void Select(List<PlayerQuickTabItemUI> list, List<PlayerQuickTabUI.ItemIconData> icons)
        {
            _selectedImage.enabled = true;

            _icon.color = _selectColor;

            UpdateTabItem(list, icons);
        }
        public void UnSelect()
        {
            _selectedImage.enabled = false;

            _icon.color = _unSelectColor;
        }
        
        private void UpdateTabItem(List<PlayerQuickTabItemUI> list, List<PlayerQuickTabUI.ItemIconData> icons)
        {
            switch (_tabType)
            {
                case TypeQuickTab.CONSUMABLE:
                    for (int i = 0; i < list.Count; i++)
                        SetConsume(list[i], icons[i]);
                    break;
                case TypeQuickTab.RUNESKILL:
                    for (int i = 0; i < list.Count; i++)
                        SetRuneSkill(list[i], icons[i]);
                    break;
                case TypeQuickTab.WEAPONSKILL1:
                    for (int i = 0; i < list.Count; i++)
                        SetWeaponSkill(list[i], icons[i]);
                    break;
                case TypeQuickTab.WEAPONSKILL2:
                    for (int i = 0; i < list.Count; i++)
                        SetWeaponSkill(list[i], icons[i]);
                    break;
            }
        }

        private void SetConsume(PlayerQuickTabItemUI item, PlayerQuickTabUI.ItemIconData iconData)
        {
            var tableData = TableManager.GetConsumeTableData(iconData.ID);
            if (tableData == null)
                item.Set(-1, string.Empty, 0, null, null);
            else
            {
                var consumeData = TableManager.CurrentPlayerData.GetConsumeData(tableData.ID);
                var amount = consumeData == null ? 0 : consumeData.Amount;
                item.ShowCount = true;
                item.Set(tableData.ID, TableManager.GetString(tableData.ID), amount, iconData.Icon, (itemID) =>
                {
                    TableManager.CurrentPlayerData.EquipConsume(itemID);
                    PlayerUIManager.Get().GetUI<PlayerConsumeUI>(TypePlayerUI.CONSUMABLE).RefreshConsume(consumeData?.Amount > 0);
                });
            }
        }
        private void SetRuneSkill(PlayerQuickTabItemUI item, PlayerQuickTabUI.ItemIconData iconData)
        {
            var tableData = TableManager.GetRuneTableData(iconData.ID);
            if (tableData == null)
                item.Set(-1, string.Empty, 0, null, null);
            else
            {
                var runeData = TableManager.CurrentPlayerData.GetRuneData(tableData.ID);
                var amount = runeData == null ? 0 : runeData.Amount;
                item.ShowCount = true;
                item.Set(tableData.ID, TableManager.GetString(tableData.ID), amount, iconData.Icon, (runeID) =>
                {
                    // Equip Logic
                    TableManager.CurrentPlayerData.EquipRune(runeID);

                    // Refresh UI
                    var weaponUI = PlayerUIManager.Get().GetUI<PlayerWeaponUI>(TypePlayerUI.WEAPON);
                    weaponUI?.RefreshRuneSkillIcon();
                });
            }
        }
        private void SetWeaponSkill(PlayerQuickTabItemUI item, PlayerQuickTabUI.ItemIconData iconData)
        {
            var tableData = TableManager.GetSkillTableData(iconData.ID);
            if (tableData == null)
                item.Set(-1, string.Empty, 0, null, null);
            else
            {
                item.ShowCount = false;
                item.Set(tableData.ID, TableManager.GetString(tableData.ID), 0, iconData.Icon, (skillID) =>
                {
                    TableManager.SelectWeaponSkill(tableData.ID);
                    PlayerUIManager.Get().RefreshWeaponIcon();
                });
            }
        }
    }
}

