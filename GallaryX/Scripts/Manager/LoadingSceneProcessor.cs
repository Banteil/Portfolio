using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace starinc.io.gallaryx
{
    public class LoadingSceneProcessor : Singleton<LoadingSceneProcessor>
    {
        [SerializeField] private TextMeshProUGUI _versionText;

        protected override void OnAwake()
        {
            if (string.IsNullOrEmpty(GameManager.Instance.NextScene))
                GameManager.Instance.NextScene = Define.MainSceneName;
            _versionText.text = Application.version;
        }

        private void Start()
        {
            Debug.Log($"Loading Start Memory Use : {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()}");
            NextSceneLoadingProcess();
        }

        private async UniTask UnloadAllAdditiveScene()
        {
            var count = SceneManager.sceneCount;
            var sceneQueue = new Queue();
            for (int i = 0; i < count; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name.Equals(Define.LoadingSceneName))
                    continue;
                sceneQueue.Enqueue(scene);
            }

            while (sceneQueue.Count > 0)
            {
                var scene = (Scene)sceneQueue.Dequeue();
                await SceneManager.UnloadSceneAsync(scene);
            }
        }

        private async void NextSceneLoadingProcess()
        {
            await UnloadAllAdditiveScene();
            await SceneManager.LoadSceneAsync(GameManager.Instance.NextScene, LoadSceneMode.Additive);

            var targetScene = SceneManager.GetSceneByName(GameManager.Instance.NextScene);
            SceneManager.SetActiveScene(targetScene);
        }

        public void LoadingFinished()
        {
            Debug.Log($"Loading End Memory Use : {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()}");
            DOTween.Clear();
            UIManager.Instance.FadeOut(1.5f, () =>
            {             
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(Define.LoadingSceneName));
                InputManager.Instance.MobileVirtualController.EnableController(Util.IsMobileWebPlatform);
                UIManager.Instance.FadeIn(1.5f);
                GameManager.Instance.IsLoading = false;
            });
        }
    }
}
