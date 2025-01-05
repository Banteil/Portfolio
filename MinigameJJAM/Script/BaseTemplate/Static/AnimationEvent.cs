using UnityEngine;

namespace starinc.io
{
    public class AnimationEvent : MonoBehaviour
    {
        public void PlaySFX(string clipName) => Manager.Sound.PlaySFX(clipName);

        public void PlaySFXLoop(string clipName) => Manager.Sound.PlaySFX(clipName, true);

        public void PlaySceneSFX(string clipName)
        {
            var soundTable = Manager.UI.SceneUI.SoundTable;
            if (soundTable != null)
            {
                var clip = soundTable.GetSFXClip(clipName);
                Manager.Sound.PlaySFX(clip);
            }
        }

        public void PlaySceneSFXLoop(string clipName)
        {
            var soundTable = Manager.UI.SceneUI.SoundTable;
            if (soundTable != null)
            {
                var clip = soundTable.GetSFXClip(clipName);
                Manager.Sound.PlaySFX(clip, true);
            }
        }

        public void StopSFX(string clipName) => Manager.Sound.StopSFX(clipName);
    }
}
