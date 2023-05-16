using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    [RequireComponent(typeof(Collider))]
    public class TriggerEnterObject : MonoBehaviour
    {
        public string TagName = "Player";
        public UnityEvent CompleteEvent;
        public bool EternalTrigger;

        private void Start()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        public virtual void CheckCondition() { CompleteEvent?.Invoke(); }

        protected void OnTriggerEnter(Collider other)
        {
            TriggerEnterEvent(other);
        }

        protected virtual void TriggerEnterEvent(Collider other)
        {
            if (other.transform.CompareTag(TagName))
            {
                Debug.Log("Trigger Enter!");
                CheckCondition();
                if (!EternalTrigger) gameObject.SetActive(false);
            }
        }
    }
}
