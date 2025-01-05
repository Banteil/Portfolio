using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "CustomerData", menuName = "Scriptable Objects/Minigame/CustomerData")]
    public class CustomerData : ScriptableObject
    {
        [SerializeField]
        private List<Sprite> _customerSprites;

        [SerializeField]
        private int _minCount, _maxCount;

        [SerializeField]
        private float _timePerCount = 15f;
        public float TimePerCount { get { return _timePerCount; } }

        public Sprite GetRandomSprite()
        {
            var rand = Random.Range(0, _customerSprites.Count);
            return _customerSprites[rand];
        }

        public int GetRandomOrderCount()
        {
            var rand = Random.Range(_minCount, _maxCount + 1);
            return rand;
        }
    }
}
