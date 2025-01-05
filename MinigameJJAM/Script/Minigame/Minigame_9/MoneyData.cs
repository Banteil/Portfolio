using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "MoneyData", menuName = "Scriptable Objects/Minigame/MoneyData")]
    public class MoneyData : ScriptableObject
    {
        [SerializeField]
        private Sprite _moneySprite;
        public Sprite MoneySprite { get { return _moneySprite; } }

        [SerializeField]
        private int _settlementScore;
        public int SettlementScore { get { return _settlementScore; } }

        [SerializeField]
        private int _minCount, _maxCount;

        public bool IsMatchTheCount(int count) => count >= _minCount && count <= _maxCount;
    }
}
