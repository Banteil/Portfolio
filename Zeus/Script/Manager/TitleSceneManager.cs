using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public struct TitleMenuDisplayInfo
    {
        public TitleSceneManager.TypeTitleMenu DisplayMenuType;
        public Vector3 Position;
    }

    public class TitleSceneManager : ZeusSceneManager
    {
        [Header("Scene Load Data")]
        [SerializeField] string _loadSceneName;
        [SerializeField] SplashDirectionData _newGameSplashData;

        public enum TypeTitleMenu { NONE, MAIN, PLAYANEW, LOADGAME }
        [SerializeField] private Camera _camera;
        [SerializeField] private float _changeTime = 1f;
        [SerializeField] private List<TitleMenuDisplayInfo> _displayInfos;
        public UITitleMenu UITitleMenu;

        protected override void _OnStart()
        {
            var load = TableManager.Instance;
            InputReader.Instance.EnableActionMap(TypeInputActionMap.UI);
            InputReader.Instance.Enable = true;
        }

        public void Initialized()
        {
            FadeManager.Instance.DoFade(false, 1f, 0f, () => { UITitleMenu.GoMain(); });
            SoundManager.Instance.Play(2, true);
            var moviePlayer = GetComponent<MoviePlayer>();
            if (moviePlayer != null)
            {
                moviePlayer.MovieRenderer.enabled = false;
            }
        }

        public bool NewGame()
        {
            PlayerPrefs.SetString("AutoSaveData", string.Empty);
            TableManager.CurrentPlayerData.Initailized();
            TableManager.IsNewGame = true;
            return GameStart(_loadSceneName, _newGameSplashData);
        }

        public void ChangeMenuDisplay(TypeTitleMenu menuType)
        {
            if (!_displayInfos.Exists(x => x.DisplayMenuType == menuType))
                return;

            var displayInfo = _displayInfos.Find(x => x.DisplayMenuType == menuType);
            var cameraTransform = _camera.transform;
            cameraTransform.DOMove(displayInfo.Position, _changeTime).SetEase(Ease.OutCubic);
        }

        internal bool GameStart(string loadsceneName, SplashDirectionData data = null)
        {
            var loadingScene = SceneLoadManager.Instance.LoadScene(loadsceneName, data, 1, 1, false);
            if (loadingScene)
            {
                SoundManager.Instance.Play((int)TypeUISound.MENU_PLAYSTART);
            }
            return loadingScene;
        }
    }
}
