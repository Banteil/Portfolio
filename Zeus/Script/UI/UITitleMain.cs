using System;

namespace Zeus
{
    public class UITitleMain : UIEffectMenu
    {
        public override void Open(Action onFinish = null)
        {
            // 메인메뉴
            SetMenuItem(0, true);
            SetMenuItem(1, TableManager.HasSaveData());
            SetMenuItem(2, false);
            SetMenuItem(3, true);

            base.Open(onFinish);
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
