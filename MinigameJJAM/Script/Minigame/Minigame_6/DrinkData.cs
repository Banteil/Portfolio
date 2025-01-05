using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "DrinkData", menuName = "Scriptable Objects/Minigame/DrinkData")]
    public class DrinkData : ScriptableObject
    {
        [SerializeField]
        private string _drinkName;
        public string DrinkName { get { return _drinkName; } }

        [SerializeField, TextArea]
        private string _drinkDescription;
        public string DrinkDescription { get { return _drinkDescription; } }

        [SerializeField]
        private Color _drinkColor;
        public Color DrinkColor { get { return _drinkColor; } }

        [SerializeField]
        private Sprite _cupSprite;
        public Sprite CupSprite { get { return _cupSprite; } }

        [SerializeField]
        private Sprite _bottomDetailSprite;
        public Sprite BottomDetail { get { return _bottomDetailSprite; } }

        [SerializeField]
        private Sprite _topDetailSprite;
        public Sprite TopDetail { get { return _topDetailSprite; } }

        [SerializeField]
        private bool _isHotDrink = false;
        public bool IsHotDrink { get { return _isHotDrink; } }

        [SerializeField]
        private int _phase = 1;
        public int Phase { get { return _phase; } }

        [SerializeField]
        private int _score = 0;
        public int Score { get { return _score; } }

        [SerializeField]
        private int _requiredIceCount = 0;
        public int RequiredIceCount { get { return _requiredIceCount; } }

        [SerializeField]
        private Sprite _phaseUISprite;
        public Sprite PhaseUISprite { get { return _phaseUISprite; } }
    }
}
