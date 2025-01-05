using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class LogoSceneManager : Singleton<LogoSceneManager>
    {
        private const string animationName = "Direction";

        [SerializeField] private List<Animator> logoDirectionAnimators = new List<Animator>();

        private void Start()
        {
            for (int i = 0; i < logoDirectionAnimators.Count; i++)
            {
                logoDirectionAnimators[i].StopPlayback();
            }
            LogoDirection();
        }

        async private void LogoDirection()
        {
            for (int i = 0; i < logoDirectionAnimators.Count; i++)
            {
                logoDirectionAnimators[i].SetTrigger(animationName);
                await UniTask.WaitUntil(() => logoDirectionAnimators[i].IsAnimationPlaying());
                await UniTask.WaitUntil(() => !logoDirectionAnimators[i].IsAnimationPlaying());
            }
            CheckRuntimeSettingState();
        }

        async private void CheckRuntimeSettingState()
        {
            await UniTask.WaitUntil(() => GameManager.Instance.RuntimeComplete);
            GameManager.Instance.LoadScene(Define.LobbySceneName);
        }
    }
}
