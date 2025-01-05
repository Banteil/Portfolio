using UnityEngine;

namespace starinc.io.kingnslave
{
    [CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData", order = 1)]
    public class CardData : ScriptableObject
    {
        [field: SerializeField]
        public Define.CardType Type { get; private set; }
        [field: SerializeField]
        public Sprite FrontSideSprite { get; private set; }
    }
}