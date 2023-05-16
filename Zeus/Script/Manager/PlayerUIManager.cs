using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public enum TypePlayerUI
    {
        HP, STAMINA, WEAPON, SKILL, CONSUMABLE, BUFF, AREA, QUEST, SAVE, QUICKTAB, AIM, REWARD, INTERACTION, GOLD, BOSSHP, STAGECLEAR
    }
    [System.Serializable]
    public class PlayerUI
    {
        public PlayerUIType UI;
        public TypePlayerUI UIType;

        public PlayerUI(PlayerUIType ui, TypePlayerUI type)
        {
            UI = ui;
            UIType = type;
        }
    }
    public class PlayerUIManager : BaseObject<PlayerUIManager>
    {
        public List<PlayerUI> UIList = new List<PlayerUI>();

        [Header("UI")]
        public CanvasGroup HUD;
        public PlayerPauseUI PauseUI;

        public void AddUI(PlayerUIType ui, TypePlayerUI type)
        {
            PlayerUI playerUI = new PlayerUI(ui, type);
            UIList.Add(playerUI);
        }

        public T GetUI<T>(TypePlayerUI type) where T : class
        {
            var ui = UIList.Find(x => x.UIType == type);
            if (ui == null)
            {
                return null;
            }
            return ui.UI as T;
        }

        public void UIVisible(TypePlayerUI type, bool visible)
        {
            var ui = UIList.Find(x => x.UIType == type);
            ui.UI.SetVisible(visible);
        }

        public void HUDVisible(bool value, float fadeTime = 1)
        {
            HUD.DOFade(value ? 1 : 0, fadeTime);
        }

        public void PauseUIvisible()
        {
            if (PauseUI != null)
                PauseUI.ToggleUI();
        }

        internal void RefreshWeaponIcon()
        {
            var ui = GetUI<PlayerWeaponUI>(TypePlayerUI.WEAPON);
            if (ui == null)
                return;

            ui.RefreshIcon();
        }

        internal void OnCheckPointStart()
        {
            var player = CharacterObjectManager.Get().GetPlayerbleCharacter();
            player.IsImmortal = true;
            FadeManager.Instance.DoFade(true, 2f, 0, () => 
            {
                player.IsImmortal = false;
                if (ZeusSceneManager.Get() != null)
                {
                    ZeusSceneManager.Get().PlaySceneBGM();
                }

                ZoneDataManager.Get().Initialized();
            });
        }

        internal void SetCoolTime(int skillID, int index, float cooltime)
        {
            /// Tony
            /// 룬스킬의 인덱스를 임의로 2번이라 지정했습니다
            var ui = GetUI<PlayerWeaponUI>(TypePlayerUI.WEAPON);
            if (ui == null)
                return;

            //무기 스킬일 경우
            if(index < 2)
            {
                var skillUIs = ui.GetWeaponSkills();
                if (skillUIs == null)
                    return;

                foreach (var item in skillUIs)
                {
                    if (item.skillType == (TypeSkillUI)index)
                    {
                        var coolTimer = new CoolTimer(skillID, cooltime);
                        coolTimer.UpdateUI = item;
                        break;
                    }
                }
            }
            else
            {
                var skillUIs = ui.GetRuneSkillUI();
                var coolTimer = new CoolTimer(skillID, cooltime);
                coolTimer.UpdateUI = skillUIs;
            }
        }

        private void OnDestroy()
        {
            HUD.DOKill();
        }
    }

}
