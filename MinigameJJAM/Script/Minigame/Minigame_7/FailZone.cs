using UnityEngine;

namespace starinc.io
{
    public class FailZone : MonoBehaviour
    {
        [SerializeField]
        private MinigameBase _base;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Object"))
            {
                var iceCream = collision.GetComponent<IceCream>();
                if (!iceCream.IsInavtive)
                    _base.EndProcess();
            }
        }
    }
}
