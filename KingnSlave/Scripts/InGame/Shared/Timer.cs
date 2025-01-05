using Fusion;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class Timer : SimulationBehaviour
    {
        [SerializeField]
        private Image timerImage;

        private TickTimer tickTimer = default;
        private float timer = Define.MAX_ROUND_TIME;
        private bool isStopped = true;
        private NetworkRunner networkRunner;

        private void Awake()
        {
			if (NetworkManager.Instance.HasRunner)
				networkRunner = NetworkManager.Instance.MyRunner;
        }

        private void Start()
        {
            InGameManager.Instance.MainPhaseStart.AddPersistentListener(ResetTimer);
            InGameManager.Instance.BattlePhaseStart.AddPersistentListener(StopTimer);
            InGameManager.Instance.GameOver.AddPersistentListener(GameOver);
        }

        private void FixedUpdate()
        {
            if (isStopped) return;

            if (GameManager.Instance.IsNetworkGameMode())
            {
                if (!tickTimer.ExpiredOrNotRunning(networkRunner))
                {
                    timer = tickTimer.RemainingTime(networkRunner) ?? 0;
                }
                else if (tickTimer.Expired(networkRunner))
                {
                    if (InGameManager.Instance.GetYourPhase() == Define.InGamePhase.MainPhase)
                    {
                        InGameManager.Instance.TimeOver?.Invoke();
                    }
                    timer = 0;
                    isStopped = true;
                }
                //timerText.text = ((int)Mathf.Ceil(timer)).ToString();
                timerImage.fillAmount = timer / Define.MAX_ROUND_TIME;
                timerImage.color = new Color(1f, timerImage.fillAmount + 0.2f, timerImage.fillAmount + 0.2f);
            }
            else
            {
                if (timer > 0)
                {
                    timer -= Time.fixedDeltaTime;
                }
                else
                {
                    if (InGameManager.Instance.GetYourPhase() == Define.InGamePhase.MainPhase)
                    {
                        InGameManager.Instance.TimeOver?.Invoke();
                    }
                    timer = 0;
                    isStopped = true;
                }
                //timerText.text = ((int)Mathf.Ceil(timer)).ToString();
                timerImage.fillAmount = timer / Define.MAX_ROUND_TIME;
                timerImage.color = new Color(1f, timerImage.fillAmount + 0.2f, timerImage.fillAmount + 0.2f);
            }
        }

        public void StopTimer()
        {
            isStopped = true;
        }

        public void ResetTimer()
        {
            timer = Define.MAX_ROUND_TIME;
            if (NetworkManager.Instance.HasRunner)
                tickTimer = TickTimer.CreateFromSeconds(NetworkManager.Instance.MyRunner, Define.MAX_ROUND_TIME);
            isStopped = false;
        }

        public void GameOver(Define.GameResult gameResult)
        {
            StopTimer();
        }
    }
}