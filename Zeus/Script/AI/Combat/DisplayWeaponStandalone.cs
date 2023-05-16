using UnityEngine;
using UnityEngine.UI;

namespace Zeus
{
    public class DisplayWeaponStandalone : MonoBehaviour
    {
        [Header("Weapon Display source")]
        public Image WeaponIcon;
        public Text WeaponText;
        [Header("Weapon unarmed sources")]
        public Sprite DefaultIcon;
        public string DefaultText;

        protected virtual void Start()
        {
            RemoveWeaponIcon();
            RemoveWeaponText();
        }

        public virtual void SetWeaponIcon(Sprite icon)
        {
            if (!WeaponIcon) return;
            WeaponIcon.sprite = icon;
            if (!WeaponIcon.gameObject.activeSelf)
                WeaponIcon.gameObject.SetActive(true);
        }

        public virtual void SetWeaponText(string text)
        {
            if (!WeaponText) return;
            WeaponText.text = text;
            if (!WeaponText.gameObject.activeSelf)
                WeaponText.gameObject.SetActive(true);
        }

        public virtual void RemoveWeaponIcon()
        {
            if (!WeaponIcon) return;
            WeaponIcon.sprite = DefaultIcon;
            if (WeaponIcon.gameObject.activeSelf && WeaponIcon.sprite == null)
                WeaponIcon.gameObject.SetActive(false);
        }

        public virtual void RemoveWeaponText()
        {
            if (!WeaponText) return;
            WeaponText.text = DefaultText;
        }

    }
}