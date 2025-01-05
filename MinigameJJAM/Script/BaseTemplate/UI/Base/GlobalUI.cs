using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class GlobalUI : BaseUI
    {
        protected const int GLOBAL_ORDER = 1000;

        protected enum GlobalButtons : int
        {
            SettingButton,
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            Util.DontDestroyObject(gameObject);
            var uiManager = Manager.UI;
            uiManager.SetCanvas(gameObject, false);
            uiManager.SetGlobalUI(this);
            UICanvas.sortingOrder = GLOBAL_ORDER;

            Bind<Button>(typeof(GlobalButtons));
            var settingButton = GetButton((int)GlobalButtons.SettingButton);
            settingButton.gameObject.BindEvent(OnSettingButton);
        }

        public void OnSettingButton(PointerEventData data)
        {

        }        
    }
}
