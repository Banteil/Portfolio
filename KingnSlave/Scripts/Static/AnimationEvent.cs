using UnityEngine;

namespace starinc.io.kingnslave
{
    public class AnimationEvent : MonoBehaviour
    {
        [SerializeField] private AudioClip sfxClip;

        public void PlaySFX(string clipName) => AudioManager.Instance.PlaySFX(sfxClip);
    }
}
