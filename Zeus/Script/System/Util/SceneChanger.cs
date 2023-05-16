using System.Collections;
using UnityEngine;

namespace Zeus
{
    public class SceneChanger : MonoBehaviour
    {
        [Header("Logo Data")]
        public float StartDelay;
        public SplashDirectionData SplashData;
        public string LoadSceneName;
        public float FadeInDuration;
        public float FadeOutDuration;
        public bool AutoFade = true;

        protected virtual IEnumerator Start()
        {
            var load = TableManager.Instance;
            yield return new WaitForSeconds(StartDelay);
            DoChange();
        }

        protected void DoChange()
        {
            SceneLoadManager.Instance.LoadScene(LoadSceneName, SplashData, FadeInDuration, FadeOutDuration, AutoFade);
        }
    }
}