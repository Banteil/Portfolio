using UnityEngine;

namespace Zeus
{
    public class ControlDisplayWeaponStandalone : MonoBehaviour
    {
        [SerializeField]
        protected DisplayWeaponStandalone LeftDisplay, RightDisplay;

        #region Control Left Display

        public virtual void SetLeftWeaponIcon(Sprite icon)
        {
            if (!LeftDisplay)
            {
                return;
            }

            LeftDisplay.SetWeaponIcon(icon);
        }

        public virtual void SetLeftWeaponText(string text)
        {
            if (!LeftDisplay)
            {
                return;
            }

            LeftDisplay.SetWeaponText(text);
        }

        public virtual void RemoveLeftWeaponIcon()
        {
            if (!LeftDisplay)
            {
                return;
            }

            LeftDisplay.RemoveWeaponIcon();
        }

        public virtual void RemoveLeftWeaponText()
        {
            if (!LeftDisplay)
            {
                return;
            }

            LeftDisplay.RemoveWeaponText();
        }

        #endregion

        #region Control Right Display

        public virtual void SetRightWeaponIcon(Sprite icon)
        {
            if (!RightDisplay)
            {
                return;
            }

            RightDisplay.SetWeaponIcon(icon);
        }

        public virtual void SetRightWeaponText(string text)
        {
            if (!RightDisplay)
            {
                return;
            }

            RightDisplay.SetWeaponText(text);
        }

        public virtual void RemoveRightWeaponIcon()
        {
            if (!RightDisplay)
            {
                return;
            }

            RightDisplay.RemoveWeaponIcon();
        }

        public virtual void RemoveRightWeaponText()
        {
            if (!RightDisplay)
            {
                return;
            }

            RightDisplay.RemoveWeaponText();

        }

        #endregion
    }
}