using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class PlayerAimTypeUI : PlayerUIType
    {
        public List<PlayerAimBulletIcon> BulletIcon;

        [Header("Bow")]
        public RectTransform BowCrossHair;
        protected override void Start()
        {
            base.Start();
            foreach (PlayerAimBulletIcon icon in GetComponentsInChildren<PlayerAimBulletIcon>())
            {
                BulletIcon.Add(icon);
            }
        }

        public void SetBullet(int count)
        {
            BulletIcon[count].Icon.enabled = false;
        }

        public void ReloadBullet()
        {
            for(int i = 0; i < BulletIcon.Count; i++)
            {
                BulletIcon[i].Icon.enabled = true;
            }
        }
    }

}

