using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace Zeus
{
    public class OlympusSceneManager : SceneChanger
    {
        public PlayableDirector TimeLineDirector;

        protected override IEnumerator Start()
        {
            yield return new WaitUntil(() => SceneLoadManager.IsLoading == false);

            TimeLineDirector.Play();

            yield return new WaitForSeconds((float)TimeLineDirector.duration);

            DoChange();
        }
    }
}