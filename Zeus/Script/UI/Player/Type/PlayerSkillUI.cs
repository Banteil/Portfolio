using UnityEngine;
using UnityEngine.UI;

namespace Zeus
{
    public enum TypeSkillUI { WEAPON_1, WEAPON_2, RUNE }
    public class PlayerSkillUI : MonoBehaviour
    {
        public Image Icon;
        public Image GaugeCover;
        public Image FullFx;
        public TypeSkillUI skillType;

        private void Start()
        {
            Init();
        }

        protected virtual void Init()
        {
            if (Icon == null)
            {
                return;
            }

            Icon.enabled = Icon.sprite != null;
        }

        public void SetIcon(string path)
        {
            if (Icon == null) { return; }
            TableManager.Instance.GetSpriteAsync(path, result =>
            {
                Icon.overrideSprite = result;
                Icon.enabled = true;
            });
        }

        public void UnSetIcon()
        {
            if (Icon == null) { return; }
            Icon.overrideSprite = null;
            Icon.enabled = false;
        }

        public void SetGauge(float gauge)
        {
            GaugeCover.fillAmount = gauge;
            FullFx.enabled = gauge == 1;

            var color = Color.white;
            if (gauge < 1)
            {
                ColorUtility.TryParseHtmlString("#BEBEBE", out color);
            }
            Icon.color = color;
        }
    }
}