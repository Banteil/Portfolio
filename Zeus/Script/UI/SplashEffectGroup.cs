using System.Collections;
using UnityEngine;

namespace Zeus
{
    public class SplashEffectGroup : MonoBehaviour
    {
        public int SoundID = -1;
        public float StartDelay = 1;
        public float EndDelay = 1;
        public bool FadeOutSameTime;

        [SerializeField] SplashEffectInfo[] _effectInfoArray;
        WaitUntil _waitEffectsDirecting;

        public bool IsDirecting { get; private set; }

        private void Start()
        {
            _waitEffectsDirecting = new WaitUntil(() => !CheckEffectsDirecting());
            foreach (var effect in _effectInfoArray) effect.ActiveObjects(false);
        }

        public void StartDirecting() => StartCoroutine(GroupEffectDirecting());

        IEnumerator GroupEffectDirecting()
        {
            IsDirecting = true;
            yield return new WaitForSeconds(StartDelay);

            if (SoundID != -1)
            {
                SoundManager.Instance.Play(SoundID);
            }

            if (!FadeOutSameTime)
            {
                foreach (var effect in _effectInfoArray) StartCoroutine(effect.Directing());

                yield return _waitEffectsDirecting;
            }
            else
            {
                foreach (var effect in _effectInfoArray) StartCoroutine(effect.FadeIn());
                yield return _waitEffectsDirecting;

                foreach (var effect in _effectInfoArray) StartCoroutine(effect.FadeOut());
                yield return _waitEffectsDirecting;
            }

            yield return new WaitForSeconds(EndDelay);
            IsDirecting = false;
        }

        bool CheckEffectsDirecting()
        {
            foreach (var effect in _effectInfoArray)
            {
                if (effect.IsDirecting) return true;
            }
            return false;
        }
    }

    [System.Serializable]
    public class SplashEffectInfo
    {
        public float FadeInTime;
        public float FadeOutDelay;
        public StorytellingSplashEffect SplashEffect;
        public GameObject[] FadeObjects;

        public bool IsDirecting { get; set; }
        WaitUntil _waitEffectDirecting;        

        public IEnumerator Directing()
        {
            _waitEffectDirecting = new WaitUntil(() => !SplashEffect.IsDirecting);

            IsDirecting = true;
            yield return new WaitForSeconds(FadeInTime);
            SplashEffect.FadeIn();
            ActiveObjects(true);
            yield return _waitEffectDirecting;

            yield return new WaitForSeconds(FadeOutDelay);
            SplashEffect.FadeOut();
            ActiveObjects(false);
            yield return _waitEffectDirecting;
            IsDirecting = false;
        }

        public IEnumerator FadeIn()
        {
            _waitEffectDirecting = new WaitUntil(() => !SplashEffect.IsDirecting);

            IsDirecting = true;
            yield return new WaitForSeconds(FadeInTime);
            SplashEffect.FadeIn();
            ActiveObjects(true);
            yield return _waitEffectDirecting;
            IsDirecting = false;
        }

        public IEnumerator FadeOut()
        {
            _waitEffectDirecting = new WaitUntil(() => !SplashEffect.IsDirecting);

            IsDirecting = true;
            yield return new WaitForSeconds(FadeOutDelay);
            SplashEffect.FadeOut();
            ActiveObjects(false);
            yield return _waitEffectDirecting;
            IsDirecting = false;
        }

        public void ActiveObjects(bool active)
        {
            foreach (var obj in FadeObjects) obj.SetActive(active);
        }
    }
}
