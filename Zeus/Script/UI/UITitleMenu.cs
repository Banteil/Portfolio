using System.Collections;
using UnityEngine;

namespace Zeus
{
    public class UITitleMenu : MonoBehaviour
    {
        [SerializeField] private UITitleMain _mainMenu;
        [SerializeField] private UITitlePlayAnew _newPlayMenu;
        [SerializeField] private UITitleLoadGame _loadGame;

        private UIEffectMenu _curMenu;
        private bool _isPlaying = false;
        private IEnumerator _gotoCoroutine;

        private void Start()
        {
            _mainMenu.gameObject.SetActive(true);
            _newPlayMenu.gameObject.SetActive(false);
            _loadGame.gameObject.SetActive(false);
        }

        private void GoTo(UIEffectMenu openMenu, TitleSceneManager.TypeTitleMenu menuType)
        {
            if (_isPlaying) return;
            if (_curMenu == openMenu) return;

            var _instacne = ZeusSceneManager.Get() as TitleSceneManager;
            if (_instacne != null)
                _instacne.ChangeMenuDisplay(menuType);

            if (_gotoCoroutine != null)
                StopCoroutine(_gotoCoroutine);
            _gotoCoroutine = GoToCO(openMenu);
            StartCoroutine(_gotoCoroutine);
        }

        private IEnumerator GoToCO(UIEffectMenu openMenu)
        {
            _isPlaying = true;

            bool isDone = false;

            if (_curMenu != null)
            {
                // 열려있는 메뉴 Close
                _curMenu.Close(() => isDone = true);

                // 닫힐때까지 대기
                yield return new WaitUntil(() => isDone);

                _curMenu.gameObject.SetActive(false);
            }

            _curMenu = openMenu;
            isDone = false;

            openMenu.gameObject.SetActive(true);

            // 열려야하는 메뉴 Open
            openMenu.Open(() => isDone = true);

            _isPlaying = false;

            // 열릴때까지 대기
            yield return new WaitUntil(() => isDone);
        }

        public void GoMain()
        {
            GoTo(_mainMenu, TitleSceneManager.TypeTitleMenu.MAIN);
        }

        public void GoNewPlay()
        {
            GoTo(_newPlayMenu, TitleSceneManager.TypeTitleMenu.PLAYANEW);
        }

        public void GoLoadGame()
        {
            GoTo(_loadGame, TitleSceneManager.TypeTitleMenu.LOADGAME);
        }

        //[SerializeField] private UIEffectPanel _main;
        //[SerializeField] private UIEffectPanel _playNew;

        //private UIEffectPanel _curPanel;

        //private void Start()
        //{
        //    _main.gameObject.SetActive(false);
        //    _playNew.gameObject.SetActive(false);
        //    GotoMain();
        //}

        //private IEnumerator GotoCO(UIEffectPanel effectPanel, UnityAction onFinish, TitleSceneManager.TypeTitleMenu menuType)
        //{
        //    TitleSceneManager.Get().ChangeMenuDisplay(menuType);

        //    bool isDone = false;
        //    ClosePanel(_curPanel, () => { isDone = true; });
        //    yield return new WaitWhile(() => !isDone);

        //    if (_curPanel != null)
        //    {
        //        yield return new WaitForSeconds(0.1f);
        //        _curPanel.gameObject.SetActive(false);
        //    }

        //    //if (effectPanel != null)
        //    //{
        //    //    effectPanel.gameObject.SetActive(true);
        //    //    yield return new WaitForSeconds(0.1f);
        //    //}

        //    _curPanel = effectPanel;
        //    isDone = false;
        //    OpenPanel(effectPanel, () => { isDone = true; });
        //    yield return new WaitWhile(() => !isDone);

        //    onFinish?.Invoke();
        //}
        //private void OpenPanel(UIEffectPanel effectPanel, UnityAction onFinish)
        //{
        //    if (effectPanel != null)
        //    {
        //        effectPanel.CallFinishOpen += (effectPanel) => 
        //        {
        //            onFinish?.Invoke(); 
        //        };
        //        // Open Effect
        //        effectPanel.gameObject.SetActive(true);
        //        effectPanel.Open();
        //    }
        //    else
        //        onFinish?.Invoke();
        //}
        //private void ClosePanel(UIEffectPanel effectPanel, UnityAction onFinish)
        //{
        //    if (effectPanel != null)
        //    {
        //        effectPanel.CallFinishClose += (effectPanel) =>
        //        {
        //            onFinish?.Invoke(); 
        //        };
        //        // Close Effect
        //        effectPanel.Close();
        //    }
        //    else
        //        onFinish?.Invoke();
        //}

        //public void GotoMain()
        //{
        //    StartCoroutine(GotoCO(_main, null, TitleSceneManager.TypeTitleMenu.MAIN));
        //}
        //public void GotoPlayAnew()
        //{
        //    StartCoroutine(GotoCO(_playNew, null, TitleSceneManager.TypeTitleMenu.PLAYANEW));
        //}
        //public void GotoPlay()
        //{
        //    StartCoroutine(GotoCO(null, () => { TitleSceneManager.Get().NewGame(); }, TitleSceneManager.TypeTitleMenu.NONE));
        //}
    }
}
