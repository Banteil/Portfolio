using System;

namespace Zeus
{
    public class UITitlePlayAnew : UIEffectMenu
    {
        public override void Open(Action onFinish = null)
        {
            // 난이도 메뉴
            InitIndex();

            SetMenuItem(0, true);
            SetMenuItem(1, false);
            SetMenuItem(2, false);

            base.Open(onFinish);
        }

        public void NewGame()
        {
            var instance = ZeusSceneManager.Get() as TitleSceneManager;
            if (instance != null && instance.NewGame())
            {
                MenuItemEnabled(false);
            }
        }
    } 
}
