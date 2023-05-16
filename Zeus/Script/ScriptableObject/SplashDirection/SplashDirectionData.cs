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
        [Tooltip("���� ��ŵ ���� ����")]
        public bool SkipPossible = false;
        [Tooltip("�ε� �ۼ�Ʈ ǥ�� ����")]
        public bool DisplayProcess = true;
    }
}
