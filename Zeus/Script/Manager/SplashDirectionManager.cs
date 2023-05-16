using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class SplashDirectionManager : UnitySingleton<SplashDirectionManager>
    {
        public bool SkipCheck { get; private set; }

        public UnityAction OnSkipEnvet = null;
        public UnityAction OnEndDirectingEvent = null;

        private bool _skip;

        public void StartDirection(SplashDirectionData splashDatas, Transform canvasTr)
        {
            if (splashDatas == null || canvasTr == null) return;

            var dataObj = Instantiate(splashDatas.SplashGroup.gameObject, canvasTr, false);
            var group = dataObj.GetComponent<SplashEffectGroup>();
            _skip = false;
            SkipCheck = splashDatas.SkipPossible;
            StartCoroutine(DisplaySplashDirecting(group));
        }

        IEnumerator DisplaySplashDirecting(SplashEffectGroup splashGroup)
        {
            if (SkipCheck)
            {
                InputReader.Instance.CallSubmit += OnSkip;
                InputReader.Instance.Enable = true;
            }

            splashGroup.StartDirecting();
            do
            {
                if (_skip) break;
                yield return null;
            } while (splashGroup.IsDirecting);

            OnEndDirectingEvent?.Invoke();
            Destroy(splashGroup.gameObject);

            InputReader.Instance.Enable = false;

            if (SkipCheck)
                InputReader.Instance.CallSubmit -= OnSkip;

            SkipCheck = false;
        }

        private void OnSkip()
        {
            if (!LoadingSceneManager.Get().SyncComplete)
                return;

            _skip = true;
        }
    }
}
