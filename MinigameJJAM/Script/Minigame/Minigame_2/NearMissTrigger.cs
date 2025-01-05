using UnityEngine;

namespace starinc.io
{
    public class NearMissTrigger : MonoBehaviour
    {
        [SerializeField]
        private int _increaseScore = 10;
        [SerializeField]
        private float _triggerAdditionalRange = 1f;

        private void Start()
        {
            var parentCollider = GetComponentInParent<Collider2D>();
            if (parentCollider != null)
            {
                // 부모 Collider와 동일한 타입의 Collider를 자식에 추가
                Collider2D triggerCollider = null;
                if (parentCollider is CircleCollider2D)
                {
                    triggerCollider = gameObject.AddComponent<CircleCollider2D>();
                    ((CircleCollider2D)triggerCollider).radius = ((CircleCollider2D)parentCollider).radius + _triggerAdditionalRange;
                }
                else if (parentCollider is BoxCollider2D)
                {
                    triggerCollider = gameObject.AddComponent<BoxCollider2D>();
                    ((BoxCollider2D)triggerCollider).size = ((BoxCollider2D)parentCollider).size + new Vector2(_triggerAdditionalRange, _triggerAdditionalRange);
                }

                if (triggerCollider != null)
                {
                    triggerCollider.offset = parentCollider.offset;
                    triggerCollider.isTrigger = true;
                }
            }
            else
            {
                Debug.LogWarning("Parent Collider2D not found!");
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                var data = Manager.Game.GetCurrentMinigameData();
                if (data == null) return;
                data.Score += _increaseScore;
                Debug.Log("Near Miss!");
            }
        }
    }
}
