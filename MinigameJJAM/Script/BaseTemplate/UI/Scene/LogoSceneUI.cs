using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    public class LogoSceneUI : SceneUI
    {
        private const string _animationName = "Direction";
        [SerializeField] private List<Animator> _logoDirectionAnimators = new List<Animator>();

        protected override void OnStart()
        {
            base.OnStart();
            for (int i = 0; i < _logoDirectionAnimators.Count; i++)
            {
                _logoDirectionAnimators[i].StopPlayback();
            }
            LogoDirection();
        }

        private async void LogoDirection()
        {
            for (int i = 0; i < _logoDirectionAnimators.Count; i++)
            {
                _logoDirectionAnimators[i].SetTrigger(_animationName);
                await UniTask.WaitUntil(() => _logoDirectionAnimators[i].IsAnimationPlaying());
                await UniTask.WaitUntil(() => !_logoDirectionAnimators[i].IsAnimationPlaying());
            }
            CheckRuntimeSettingState();
        }

        private void CheckRuntimeSettingState()
        {
            Manager.Load.SceneLoad(Define.TITLE_SCENE_NAME, SceneLoadType.Async);
        }
    }
}
