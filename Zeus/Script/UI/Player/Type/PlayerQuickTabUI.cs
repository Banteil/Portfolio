using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Zeus
{
    public class PlayerQuickTabUI : PlayerUIType
    {
        [SerializeField] private List<PlayerQuickTabTabUI> _tabs;
        [SerializeField] private List<PlayerQuickTabItemUI> _items;

        private int _selectedTabIndex;
        private int _selectedItemIndex;
        public Image SelecTionFx;

        private Dictionary<TypeQuickTab, List<ItemIconData>> _itemIcons = new Dictionary<TypeQuickTab, List<ItemIconData>>();
        private bool _isVisible;

        //SoundID
        private const int QuickTabCategoryChangeSoundID = 118;
        private const int QuickTabItemChangeSoundID = 119;
        private const int QuickTabItemSelectSoundID = 120;

        public struct ItemIconData
        {
            public int ID;
            public Sprite Icon;
        }

        protected override void Start()
        {
            base.Start();

            _canvas.alpha = 0;
            _isVisible = false;

            foreach (var tab in _tabs) tab.UnSelect();
            foreach (var item in _items) item.UnSelect();
        }

        private void RefreshItemIcons()
        {
            _itemIcons.Clear();

            void AddItemIcon(TypeQuickTab tabType, Func<int, string> getIconPath, params int[] ids)
            {
                if (!_itemIcons.ContainsKey(tabType))
                    _itemIcons.Add(tabType, new List<ItemIconData>());

                for (int i = 0; i < ids.Length; i++)
                {
                    var id = ids[i];
                    var iconPath = getIconPath.Invoke(id);
                    var icon = TableManager.Instance.GetSprite(iconPath);
                    _itemIcons[tabType].Add(new ItemIconData()
                    {
                        ID = id,
                        Icon = icon,
                    });
                }
            }

            for (int tabIndex = 0; tabIndex < _tabs.Count; tabIndex++)
            {
                var tab = _tabs[tabIndex];
                int[] ids = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                switch (tab.TabType)
                {
                    case TypeQuickTab.CONSUMABLE:
                        ids = new int[] { 6102, 6103, 6104, 6105, 6106, 6107, 6108, 6109 };
                        AddItemIcon(tab.TabType, (id) =>
                        {
                            var consumeData = TableManager.GetConsumeTableData(id);
                            if (consumeData == null) return string.Empty;
                            return consumeData.Icon;
                        }, ids);
                        break;
                    case TypeQuickTab.RUNESKILL:
                        ids = new int[] { 4101, 4102, 4103, 4104, 4105, 4106, 4107, 4108 };
                        AddItemIcon(tab.TabType, (id) =>
                        {
                            var runeData = TableManager.GetRuneTableData(id);
                            if (runeData == null) return string.Empty;
                            return runeData.Icon;
                        }, ids);
                        break;
                    case TypeQuickTab.WEAPONSKILL1:
                    case TypeQuickTab.WEAPONSKILL2:
                        var playerData = TableManager.CurrentPlayerData;
                        var weaponID = playerData.GetWeaponID();
                        var weaponData = TableManager.GetWeaponTableData(weaponID);
                        if (weaponData != null)
                        {
                            List<int> skills = weaponData.SkillDatas.Select(x => x.SkillID).ToList();
                            if (tab.TabType == TypeQuickTab.WEAPONSKILL1)
                                ids = skills.GetRange(0, 8).ToArray();
                            if (tab.TabType == TypeQuickTab.WEAPONSKILL2)
                                ids = skills.GetRange(8, 8).ToArray();
                        }

                        AddItemIcon(tab.TabType, (id) =>
                        {
                            var skillData = TableManager.GetSkillTableData(id);
                            if (skillData == null) return string.Empty;
                            return skillData.Icon;
                        }, ids);
                        break;
                }
            }
        }

        public void SetVisible(bool visible)
        {
            _isVisible = visible;

            Cursor.visible = visible;
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            //InputReader.Instance.EnableQuickTabUI(visible, visible);
            InputReader.Instance.EnableActionMap(visible ? TypeInputActionMap.UI_QUICKTAB : TypeInputActionMap.NONE);
            _canvas.DOFade(visible ? 1 : 0, 0.5f);
            SelecTionFx.DOFade(0, 0f);
            if (visible)
            {
                RefreshItemIcons();
                SelectTab(0);
            }
            if (!visible) TableManager.AutoSave();
        }

        public void MoveTab(int moveIndex)
        {
            int selectIndex = _selectedTabIndex + moveIndex;
            if (selectIndex < 0)
                selectIndex = _tabs.Count - 1;
            else if (selectIndex > _tabs.Count - 1)
                selectIndex = 0;

            SoundManager.Instance.Play(QuickTabCategoryChangeSoundID);
            SelectTab(selectIndex);
        }
        public void SelectTab(int selectIndex)
        {
            if (_selectedTabIndex != selectIndex)
                _tabs[_selectedTabIndex].UnSelect();
            _selectedTabIndex = selectIndex;

            RefreshCurrentTab();

            var playerData = TableManager.CurrentPlayerData;
            int itemIndex = 0;

            int GetIconIndex(TypeQuickTab tabType, int id)
            {
                var dataIndex = _itemIcons[tabType].FindIndex(x => x.ID == id);
                if (dataIndex == -1) return 0;
                return dataIndex;
            }

            if (playerData != null)
            {
                var currentTab = _tabs[_selectedTabIndex];
                int dataID = -1;
                switch (currentTab.TabType)
                {
                    case TypeQuickTab.CONSUMABLE:
                        dataID = playerData.GetEquipConsumeID();
                        itemIndex = GetIconIndex(currentTab.TabType, dataID);
                        break;
                    case TypeQuickTab.RUNESKILL:
                        dataID = playerData.GetEquipRuneID();
                        itemIndex = GetIconIndex(currentTab.TabType, dataID);
                        break;
                    case TypeQuickTab.WEAPONSKILL1:
                        var weaponID = playerData.GetWeaponID();
                        itemIndex = playerData.WeaponSkillSelectData[weaponID][0];
                        break;
                    case TypeQuickTab.WEAPONSKILL2:
                        weaponID = playerData.GetWeaponID();
                        itemIndex = playerData.WeaponSkillSelectData[weaponID][1] - 8;
                        break;
                }
            }
            SelectItem(itemIndex);
        }

        public void MoveItem(int moveIndex)
        {
            int selectIndex = _selectedItemIndex + moveIndex;
            if (selectIndex < 0)
                selectIndex = _items.Count - 1;
            else if (selectIndex > _items.Count - 1)
                selectIndex = 0;

            SoundManager.Instance.Play(QuickTabItemChangeSoundID);
            SelectItem(selectIndex);
        }
        public void SelectItem(int selectIndex)
        {
            if (_selectedItemIndex != selectIndex)
                _items[_selectedItemIndex].UnSelect();
            _selectedItemIndex = selectIndex;
            _items[_selectedItemIndex].Select();
        }

        public void RefreshCurrentTab()
        {
            if (!_isVisible) return;

            var currentTab = _tabs[_selectedTabIndex];
            var icons = _itemIcons[currentTab.TabType];
            currentTab.Select(_items, icons);
        }

        public void UseCurrentItem()
        {
            var currentTab = _tabs[_selectedTabIndex];
            if (currentTab == null) return;

            SoundManager.Instance.Play(QuickTabItemSelectSoundID);

            int skillID = 0;
            if (currentTab.TabType == TypeQuickTab.RUNESKILL)
            {
                var runeID = TableManager.CurrentPlayerData.GetEquipRuneID();
                var tableData = TableManager.GetRuneTableData(runeID);
                if (tableData != null) skillID = tableData.SkillID;
            }
            else if (currentTab.TabType == TypeQuickTab.WEAPONSKILL1)
            {
                var weaponID = TableManager.CurrentPlayerData.GetWeaponID();
                var tableData = TableManager.GetWeaponTableData(weaponID);
                if (tableData != null) skillID = tableData.SkillDatas[0].SkillID;
            }
            else if (currentTab.TabType == TypeQuickTab.WEAPONSKILL2)
            {
                var weaponID = TableManager.CurrentPlayerData.GetWeaponID();
                var tableData = TableManager.GetWeaponTableData(weaponID);
                if (tableData != null) skillID = tableData.SkillDatas[1].SkillID;
            }

            var skillCooltime = GameTimeManager.Instance.GetCoolTime(skillID);
            if (skillCooltime == null)
            {
                var item = _items[_selectedItemIndex];
                if (item == null) return;
                item.OnUse();

                SelecTionFx.color = Color.white;
            }
            else
                SelecTionFx.color = Color.red;

            SelecTionFx.rectTransform.position = _items[_selectedItemIndex].IconPosition;
            SelecTionFx.DOKill();
            SelecTionFx.DOFade(0, 0f).onComplete = () =>
            {
                SelecTionFx.DOFade(0.6f, 0.05f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InFlash).onComplete = () =>
                {
                    SelecTionFx.DOKill();
                };
            };
        }
    }
}