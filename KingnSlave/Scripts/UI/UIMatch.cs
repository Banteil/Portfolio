using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIMatch : UISide
    {
        private UILobbyScene lobby;

        private enum MatchTexts
        {
            ConnectingText,
            TimerText,
        }

        private enum MatchButtons
        {
            CloseButton,
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            lobby = FindObjectOfType<UILobbyScene>();
            if (lobby == null)
            {
                UIManager.Instance.CloseUI();
                return;
            }
            Bind<TextMeshProUGUI>(typeof(MatchTexts));
            var text = GetText((int)MatchTexts.ConnectingText);
            text.text = Util.GetLocalizationTableString(Define.InfomationLocalizationTable, "waitMatch");

            Bind<Button>(typeof(MatchButtons));
            var button = GetButton((int)MatchButtons.CloseButton);
            button.gameObject.BindEvent(OnCloseButtonClicked);

            var canvas = GetComponent<Canvas>();
            canvas.sortingOrder = 500;

            NetworkManager.Instance.CancelMatchCallback += CancelEvent;
            StartMatch();
        }

        async private void TimerProcess()
        {
            var timerText = GetText((int)MatchTexts.TimerText);
            var timer = 0f;
            Debug.Log("Start Check TIme");
            while (NetworkManager.Instance.IsMatching)
            {
                await UniTask.Yield();
                var minute = (int)timer / 60;
                var sec = (int)timer % 60;
                var result = string.Format("{0:D2}:{1:D2}", minute, sec);
                timerText.text = result;
                timer += Time.deltaTime;
            }
            Debug.Log("End Check TIme");
        }

        protected void OnCloseButtonClicked(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            CancelMatch();
        }

        async private void StartMatch()
        {
            if (NetworkManager.Instance.IsShutdown)
                await UniTask.WaitUntil(() => !NetworkManager.Instance.IsShutdown);

            Debug.Log("Start Match");
            NetworkManager.Instance.IsMatching = false;
            NetworkManager.Instance.StartMatch();
            TimerProcess();
        }

        private void CancelMatch() => NetworkManager.Instance.CancelMatch();

        private void CancelEvent() => CloseDirection(() =>
        {
            UIManager.Instance.FindCloseUI(this, () =>
            {
                lobby.StartButtonObject().SetActive(true);
            });
        });

        public override void InputEscape()
        {
            if (!isDirecting)
                CancelMatch();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                // 앱이 포그라운드로 돌아온 경우에 실행되는 코드
                Debug.Log("Application gained focus");
                //CheckMatchingStateProcess();
            }
            else
            {
                // 앱이 백그라운드로 이동한 경우에 실행되는 코드
                Debug.Log("Application lost focus");
                CancelMatch();
            }
        }

        private void CheckMatchingStateProcess()
        {
            if (!NetworkManager.Instance.HasRunner)
                NetworkManager.Instance.ResetMatch();
        }

        private void OnDestroy()
        {
            if (NetworkManager.HasInstance)
                NetworkManager.Instance.CancelMatchCallback -= CancelEvent;
        }
    }
}
