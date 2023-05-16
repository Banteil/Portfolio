using UnityEngine;
namespace Zeus
{
    public class RemoveParent : MonoBehaviour
    {
        public bool removeOnStart = true;

        private void Start()
        {
            if (removeOnStart)
            {
                ParentRemove();
            }
        }

        public void RemoveParentOfOtherTransform(Transform target)
        {
            target.SetParent(null);
        }

        public void ParentRemove()
        {
            transform.SetParent(null);
        }
    }
}