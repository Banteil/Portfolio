using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeus
{
    public class PlayerWeaponUI : PlayerUIType
    {
        public List<PlayerSkillUI> Skills;
        public Image WeaponIcon;

        protected override void Start()
        {
            base.Start();
            RefreshIcon();
            RefreshRuneSkillIcon();
        }

        public List<PlayerSkillUI> GetWeaponSkills()
        {
            return Skills.FindAll(x => x.skillType == TypeSkillUI.WEAPON_1 || x.skillType == TypeSkillUI.WEAPON_2);
        }

        public PlayerSkillUI GetRuneSkillUI()
        {
            return Skills.Find(x => x.skillType == TypeSkillUI.RUNE);
        }

        internal void SelectedWeapon(WeaponTableData data)
        {
            TableManager.Instance.GetSpriteAsync(data.Icon, result =>
            {
                WeaponIcon.overrideSprite = result;
                WeaponIcon.SetNativeSize();
            });
        }

        internal void RefreshIcon()
        {
            var weaponID = EquipManager.Get().EquipedWeaponID == 0 ? TableManager.CurrentPlayerData.GetWeaponID() : EquipManager.Get().EquipedWeaponID;
            var tableData = TableManager.GetWeaponTableData(weaponID);
            if (tableData == null)
            {
                Debug.LogError("Not Found WeaponTableData ID === " + weaponID);
                return;
            }

            GameTimeManager.Instance.ClearCoolTimeUI();
            var skillID = new int[] { TableManager.GetWeaponSkillID(weaponID, (int)TypeSkillUI.WEAPON_1), TableManager.GetWeaponSkillID(weaponID, (int)TypeSkillUI.WEAPON_2) };
            for (int i = 0; i < skillID.Length; i++)
            {
                int tableID = skillID[i];
                if (tableID == 0)
                    continue;

                var skillData = TableManager.GetSkillTableData(tableID);
                if (skillData == null)
                {
                    Debug.LogError($"skill data Null tableID : {tableID}");
                    continue;
                }
                var skillUis = GetWeaponSkills();

                foreach (var item in skillUis)
                {
                    if ((int)item.skillType == i)
                    {
                        var cooltime = GameTimeManager.Instance.GetCoolTime(tableID);
                        if (cooltime != null)
                        {
                            var timer = cooltime as CoolTimer;
                            if (timer != null)
                            {
                                item.SetGauge(timer.FillAmount);
                                timer.UpdateUI = item;
                            }
                        }
                        else
                        {
                            item.SetGauge(1f);
                        }

                        item.SetIcon(skillData.Icon);
                        break;
                    }
                }
            }

            SelectedWeapon(tableData);
        }

        internal void RefreshRuneSkillIcon()
        {
            //아래 코드를 이용해서 룬 테이블의 데이터로 UI 업데이트
            //var tableData = TableManager.GetWeaponTableData(weaponID);
            //TableManager.Instance.GetSpriteAsync(tableData.Icon, result =>
            //{
            //    WeaponIcon.overrideSprite = result;
            //});

            var playerData = TableManager.CurrentPlayerData;
            if (playerData == null) return;

            var runeID = playerData.GetEquipRuneID();
            if (runeID == 0) return;

            var tableData = TableManager.GetRuneTableData(runeID);
            if (tableData == null) return;

            var skillData = TableManager.GetSkillTableData(tableData.SkillID);
            if (skillData == null) return;


            GetRuneSkillUI()?.SetIcon(skillData.Icon);
        }
    }
}