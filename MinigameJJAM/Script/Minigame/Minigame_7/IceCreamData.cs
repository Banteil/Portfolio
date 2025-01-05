using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "IceCreamData", menuName = "Scriptable Objects/Minigame/IceCreamData")]
    public class IceCreamData : ScriptableObject
    {
        [SerializeField]
        private Sprite _iceCreamSprite;
        public Sprite IceCreamSprite { get { return _iceCreamSprite; } }
        [SerializeField]
        private Vector2 _colliderOffset;
        public Vector2 Offset { get { return _colliderOffset; } }
        [SerializeField]
        private Vector2 _colliderSize;
        public Vector2 Size { get { return _colliderSize; } }
    }
}
