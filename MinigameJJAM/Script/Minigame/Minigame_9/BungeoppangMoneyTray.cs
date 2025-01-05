using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    public class BungeoppangMoneyTray : MonoBehaviour
    {
        [SerializeField]
        private MoneyTable _table;
        [SerializeField]
        private Transform _tray;

        private BoxCollider2D _collider;
        private List<MoneyData> _paidMoneys = new List<MoneyData>();

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        public void PayMoney(int requestCount)
        {
            var data = _table.GetMoneyData(requestCount);

            Vector2 randomPosition = GetRandomPositionWithinCollider(_tray);
            GameObject moneyObject = new GameObject("Money");
            moneyObject.transform.SetParent(_tray);
            moneyObject.transform.localPosition = randomPosition;
            float randomRotationZ = Random.Range(0f, 360f);
            moneyObject.transform.localRotation = Quaternion.Euler(0f, 0f, randomRotationZ);

            SpriteRenderer spriteRenderer = moneyObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = data.MoneySprite;
            spriteRenderer.sortingOrder = 1;
            Manager.Sound.PlaySFX("m9sfx_paidMoney");

            _paidMoneys.Add(data);
        }

        private Vector2 GetRandomPositionWithinCollider(Transform parent)
        {
            Bounds bounds = _collider.bounds;

            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 randomWorldPosition = new Vector2(randomX, randomY);

            Vector2 randomLocalPosition = parent.InverseTransformPoint(randomWorldPosition);
            return randomLocalPosition;
        }

        public int ConvertToScore()
        {
            if (_paidMoneys.Count == 0) return 0;

            Manager.Sound.PlaySFX("m9sfx_getMoney");
            var score = 0;
            foreach (var money in _paidMoneys)
            {
                score += money.SettlementScore;
            }
            _paidMoneys.Clear();

            for (int i = 0; i < _tray.childCount; i++)
            {
                Destroy(_tray.GetChild(i).gameObject);
            }
            return score;
        }
    }
}
