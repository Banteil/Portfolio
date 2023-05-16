using System.Collections;
using UnityEngine;

namespace Zeus
{
    public enum TypeMove
    {
        LINE,
        RADIUS_RETURN,
    }

    public class ObjectMoveController : MonoBehaviour
    {
        public TypeMove MoveType = TypeMove.RADIUS_RETURN;
        public float MoveDelay;
        public float Speed;
        public GameObject MoveObject;
        public Transform[] Waypoints;

        private int _currentIndex;
        private Vector3 _destPosition;
        private Vector3 _aroundPosition;
        private bool _start;
        private bool _turn;
        private Vector3 _endPosition;
        // Start is called before the first frame update
        IEnumerator Start()
        {
            if (MoveObject == null)
            {
                MoveObject = gameObject;
            }

            yield return new WaitForSeconds(MoveDelay);

            if (MoveType == TypeMove.RADIUS_RETURN )
                MoveRadiusInitialized();
        }

        private void MoveRadiusInitialized()
        {
            _endPosition = MoveObject.transform.position;
            _currentIndex = 0;
            SetDestPosition();
            _start = true;
        }

        private void SetDestPosition()
        {
            if (_currentIndex < 0 || _currentIndex >= Waypoints.Length) 
            {
                return;
            }

            _aroundPosition = Waypoints[_currentIndex].position;
            var dir = Waypoints[_currentIndex].position - MoveObject.transform.position;
            var distance = Vector3.Distance(Waypoints[_currentIndex].position, MoveObject.transform.position) * 2f;
            _destPosition = dir.normalized * distance + MoveObject.transform.position;
        }

        // Update is called once per frame

        private float step;
        void FixedUpdate()
        {
            if (MoveType == TypeMove.LINE)
            {
                var nextMovePosition = MoveObject.transform.forward * Speed * Time.deltaTime + MoveObject.transform.position;
                MoveObject.transform.SetPositionAndRotation(nextMovePosition, MoveObject.transform.rotation);   
                return;
            }

            if (!_start)
                return;

            var prevPosition = MoveObject.transform.position;
            MoveObject.transform.RotateAround(_aroundPosition, Vector3.up, Speed);
            step = Vector3.Distance(prevPosition, MoveObject.transform.position);

            if (!_turn && _destPosition.CompaerEpsilon(MoveObject.transform.position, step))
            {
                var nextIndex = _currentIndex + 1;
                if (nextIndex < Waypoints.Length)
                {
                    _currentIndex = nextIndex;
                    Speed = Speed * -1f;
                    SetDestPosition();
                }
                else
                {
                    _turn = true;
                    SetDestPosition();
                }    
            }
            else if (_turn && _endPosition.CompaerEpsilon(MoveObject.transform.position, step))
            {
                _start = false;
                var particles = GetComponentsInChildren<ParticleSystem>();
                foreach (var item in particles)
                {
                    item.Stop();
                }
                gameObject.DestroyTimer(1f);
            }
            else if (_turn && _destPosition.CompaerEpsilon(MoveObject.transform.position, step))
            {
                var nextIndex = _currentIndex - 1;
                if (nextIndex >= 0)
                {
                    _currentIndex = nextIndex;
                    Speed = Speed * -1f;
                    SetDestPosition();
                }
            }
        }
    }
}