using System;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

namespace starinc.io
{
    public class Drink : MonoBehaviour
    {
        #region Cache
        private PolygonCollider2D _polygonCollider;
        private Rigidbody2D _rigidBody2D;
        public Rigidbody2D RigidBody { get {  return _rigidBody2D; } }
        private DrinkData _currentData;

        [SerializeField]
        private SpriteRenderer _cup;
        [SerializeField]
        private SpriteShapeRenderer _base;
        [SerializeField]
        private WaterShapeController _waterController;
        [SerializeField]
        private SpriteRenderer _bottomDetail;
        [SerializeField]
        private SpriteRenderer _topDetail;

        public Transform Ices;

        private int _currentIceCount = 0;
        public int CurrentIceCount
        {
            get { return _currentIceCount; }
            set { _currentIceCount = value; }
        }

        public int Score { get { return _currentData?.Score ?? 0; } }
        public bool IsCorrectIceCount { get { return _currentIceCount == _currentData?.RequiredIceCount; } }

        public event Action OnFailure;
        #endregion

        private void Awake()
        {
            _polygonCollider = GetComponent<PolygonCollider2D>();
            _rigidBody2D = GetComponent<Rigidbody2D>();
        }

        public void InitializeWithData(DrinkData data)
        {
            _currentData = data;
            _cup.sprite = _currentData.CupSprite;
            UpdateCollider();
            if (_currentData.IsHotDrink)
            {
                for(int i = 0; i < 3; i++)
                {
                    var childObj = transform.GetChild(i);
                    childObj.gameObject.SetActive(false);
                }
                return;
            }
            _base.color = _currentData.DrinkColor;
            _bottomDetail.sprite = _currentData.BottomDetail;
            _topDetail.sprite = _currentData.TopDetail;
        }

        public void ReadyDrink()
        {
            _waterController.InitializeWater();
        }        

        private void UpdateCollider()
        {
            if (_cup.sprite == null)
            {
                Debug.LogWarning("SpriteRenderer does not have a sprite assigned.");
                return;
            }

            // Physics Shape 데이터 가져오기
            var sprite = _cup.sprite;   
            _polygonCollider.pathCount = sprite.GetPhysicsShapeCount(); // Shape 경로 개수 설정

            for (int i = 0; i < _polygonCollider.pathCount; i++)
            {
                var path = new Vector2[sprite.GetPhysicsShapePointCount(i)].ToList();
                sprite.GetPhysicsShape(i, path); // 각 경로의 점 정보 가져오기
                _polygonCollider.SetPath(i, path); // Polygon Collider에 경로 설정
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(_currentData.IsHotDrink)
            {
                OnFailure?.Invoke();
            }
        }
    }
}
