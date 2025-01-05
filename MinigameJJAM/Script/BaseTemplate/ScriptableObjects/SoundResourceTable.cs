using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "SoundResourceTable", menuName = "Scriptable Objects/SoundResourceTable")]
    public class SoundResourceTable : ScriptableObject
    {
        public List<AudioClip> BGMClips = new List<AudioClip>();
        public List<AudioClip> SFXClips = new List<AudioClip>();

        public AudioClip GetBGMClip(int index)
        {
            if (index < 0 || index >= BGMClips.Count) return null;
            return BGMClips[index];
        }

        public AudioClip GetBGMClip(string clipName)
        {
            AudioClip clip = BGMClips.FirstOrDefault(c => c.name.Equals(clipName, System.StringComparison.OrdinalIgnoreCase));
            if (clip == null)
            {
                Debug.LogWarning($"AudioClip with name '{clipName}' not found in BGMClipList.");
            }

            return clip;
        }

        public AudioClip GetSFXClip(int index)
        {
            if (index < 0 || index >= SFXClips.Count) return null;
            return SFXClips[index];
        }

        public AudioClip GetSFXClip(string clipName)
        {
            AudioClip clip = SFXClips.FirstOrDefault(c => c.name.Equals(clipName, System.StringComparison.OrdinalIgnoreCase));
            if (clip == null)
            {
                Debug.LogWarning($"AudioClip with name '{clipName}' not found in BGMClipList.");
            }

            return clip;
        }

        // BGM 리스트를 딕셔너리로 반환
        public Dictionary<string, AudioClip> GetBGMDictionary()
        {
            return BGMClips.ToDictionary(clip => clip.name, clip => clip);
        }

        // SFX 리스트를 딕셔너리로 반환
        public Dictionary<string, AudioClip> GetSFXDictionary()
        {
            return SFXClips.ToDictionary(clip => clip.name, clip => clip);
        }
    }
}
