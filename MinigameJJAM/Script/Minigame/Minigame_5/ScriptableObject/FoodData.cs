using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "FoodData", menuName = "Scriptable Objects/Minigame/FoodData")]
    public class FoodData : ScriptableObject
    {
        [SerializeField]
        private string _name;
        public string Name { get { return _name; } }
        [SerializeField]
        private Sprite _raw, _cook, _over;
        [SerializeField]
        private float _minCookingTime, _maxCookingTime;
        [SerializeField]
        private float _timerIncrease;
        public float TimerIncrease { get { return _timerIncrease; } }
        [SerializeField]
        private float _speed = 5f;
        public float Speed { get { return _speed; } }
        [SerializeField]
        private float _radius = 1f;
        public float Radius { get { return _radius; } }
        [SerializeField]
        private int _score = 100;
        public int Score { get { return _score; } }
        [SerializeField]
        private float _minTrailDistance = 0f;
        [SerializeField]
        private float _maxTrailDistance = 1f;
        public float MinTrailDistance { get { return _minTrailDistance; } }
        public float MaxTrailDistance { get { return _maxTrailDistance; } }

        public CookingState GetCurrentState(float timer)
        {
            if (timer < _minCookingTime) return CookingState.Raw;
            else if (timer >= _minCookingTime && timer < _maxCookingTime) return CookingState.Cook;
            else return CookingState.Over;
        }

        public Sprite GetFoodSprite(CookingState state)
        {
            switch (state)
            {
                case CookingState.Raw:
                    return _raw;
                case CookingState.Cook:
                    return _cook;
                case CookingState.Over:
                    return _over;
                default:
                    return null;
            }
        }
    }
    public enum CookingState
    {
        Raw,
        Cook,
        Over,
    }
}
