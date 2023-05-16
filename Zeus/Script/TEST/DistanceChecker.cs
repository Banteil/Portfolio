using UnityEngine;

namespace Zeus
{
    public class DistanceChecker : MonoBehaviour
    {
        private float elapsedTime = 0;
        private Vector3 _startPosition;
        public Transform Target;
        // Start is called before the first frame update
        void Start()
        {
            if (Target == null) { Target = transform; }
            Application.targetFrameRate = 60;
            _startPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > 1f)
            {
                Debug.Log(Vector3.Distance(_startPosition, Target.position));
                elapsedTime -= 1f;
            }
        }
    }
}