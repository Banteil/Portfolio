using System;
using UnityEngine;

namespace Zeus
{
    public class UITitleLoadGame : UIEffectMenu
    {
        public override void Open(Action onFinish = null)
        {
            // 세이브 데이터 메뉴
            InitIndex();

            SetMenuItem();

            base.Open(onFinish);
        }

        public void LoadGame(int index)
        {
            var loadSceneNamae = TableManager.CurrentPlayerData.SaveZoneData.SceneName;
            if (string.IsNullOrEmpty(loadSceneNamae))
            {
                Debug.LogError("TableManager.CurrentPlayerData.SaveZoneData.SceneName is Null");
                loadSceneNamae = "3.0_Scene";
            }
            TableManager.IsNewGame = false;

            var _instacne = ZeusSceneManager.Get() as TitleSceneManager;
            if (_instacne != null)
                _instacne.GameStart(loadSceneNamae);
        }

        private void SetMenuItem()
        {
            var saveData = TableManager.GetSaveDatas();
            for (int i = 0; i < _menuList.Count; i++)
            {
                bool isEnable = false;
                if (i < saveData.PlayerDatas.Count)
                {
                    var data = saveData.PlayerDatas[i];
                    var menuItem = _menuList[i];
                    var sceneNameID = SceneLoadManager.GetSceneNameID(data.SaveZoneData.SceneName);
                    menuItem.SetText($"[{TableManager.GetString(sceneNameID)}]");
                    menuItem.gameObject.SetActive(true);
                    isEnable = true;
                }

                SetMenuItem(i, isEnable);
            }
        }
    }
}
