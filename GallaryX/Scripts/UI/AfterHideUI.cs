using Unity.VisualScripting;
using UnityEngine;

namespace starinc.io.gallaryx
{
    public class AfterHideUI : UIBase
    {
        enum HideUIName
        {
            ShortcutsInfo,
            MobileLandscapeShortcutsInfo,
            MobilePortraitShortcutInfo,
        }

        private GameObject _mainHideUI;

        protected override void OnStart()
        {
            base.OnStart();
            var objName = Util.IsMobileWebPlatform ? (Util.IsLandscape ? HideUIName.MobileLandscapeShortcutsInfo.ToString() : HideUIName.MobilePortraitShortcutInfo.ToString())
                : HideUIName.ShortcutsInfo.ToString();
            _mainHideUI = transform.Find(objName).gameObject;
        }

        protected override void ScreenOrientationChanged(bool isLandscape)
        {
            base.ScreenOrientationChanged(isLandscape);
            var prevObj = _mainHideUI;
            var objName = isLandscape ? HideUIName.MobileLandscapeShortcutsInfo.ToString() : HideUIName.MobilePortraitShortcutInfo.ToString();
            _mainHideUI = transform.Find(objName).gameObject;
            _mainHideUI.SetActive(prevObj.activeSelf);
            prevObj.SetActive(false);
        }

        public void HideUIProcess(bool isHide) => _mainHideUI.SetActive(isHide);
    }
}
