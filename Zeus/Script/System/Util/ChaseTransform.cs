using UnityEngine;

namespace Zeus
{
    public class ChaseTransform : MonoBehaviour
    {
        public Transform FollowTarget;
        public Vector3 Offset = Vector3.zero;

        // Update is called once per frame
        void FixedUpdate()
        {
            if (FollowTarget == null) return;

            var newPosition = FollowTarget.position;
            newPosition.x += Offset.x;
            newPosition.y += Offset.y;
            newPosition.z += Offset.z;

            transform.position = newPosition;
        }
    }
}