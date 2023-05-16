using System.Collections;
using UnityEngine;

namespace Zeus
{
    public class ZeusSceneManager : BaseObject<ZeusSceneManager>
    {
        public int SceneBgmID;

        protected virtual IEnumerator Start()
        {
            yield return new WaitUntil(() => SceneLoadManager.IsLoading == false);

            _OnStart();
        }

        protected virtual void _OnStart() { }

        internal void PlaySceneBGM()
        {
            SoundManager.Instance.PlayAsync(SceneBgmID, Vector3.zero, true);
        }

    }
}