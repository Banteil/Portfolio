using UnityEngine;

namespace Zeus
{
    [CreateAssetMenu(menuName = "Zeus/Splash Direction/Splash Direction Data")]
    [System.Serializable]
    public class SplashDirectionData : ScriptableObject
    {
        [Header("Base Info")]
        public SplashEffectGroup SplashGroup;
        public string Name;
        public int ID;

        [Header("Loading Option")]
        [Tooltip("연출 스킵 가능 여부")]
        public bool SkipPossible = false;
        [Tooltip("로딩 퍼센트 표시 여부")]
        public bool DisplayProcess = true;
    }
}
