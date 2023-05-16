using UnityEngine;
namespace Zeus
{
    public class TonyTestSOundPlay : BaseObject<TonyTestSOundPlay>
    {
        private const int _bossBGMID = 103;
        private const int _sceneBGM = 112;
        private const int _bossDieSoundID = 113;

        private int bgmHash;
        private void Start()
        {
            SoundManager.Instance.PlayAsync(_sceneBGM, Vector3.zero, true);
        }

        public void PlayBGM()
        {
            SoundManager.Instance.BGMFade(true, 3f, () =>
            {
                SoundManager.Instance.PlayAsync(_bossBGMID, Vector3.zero, true);
            });
        }

        private void Play(int id)
        {
            SoundManager.Instance.PlayAsync(id, Vector3.zero);
        }

        public void BossDie()
        {
            SoundManager.Instance.Play(_bossDieSoundID);
            SoundManager.Instance.BGMFade(true, 3f, () =>
            {
                SoundManager.Instance.PlayAsync(_sceneBGM, Vector3.zero, true);
            });
        }
    }
}