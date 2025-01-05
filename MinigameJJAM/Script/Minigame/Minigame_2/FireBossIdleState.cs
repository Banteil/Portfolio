using UnityEngine;

namespace starinc.io
{
    public class FireBossIdleState : StateMachineBehaviour
    {
        [SerializeField]
        private float _minWaitingTime = 2f;
        [SerializeField]
        private float _maxWatingTime = 3f;

        private FireBossController _boss;
        private float _timer = 0f;
        private float _exitTime;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _boss ??= animator.GetComponent<FireBossController>();
            _boss.State = "Idle";
            _timer = 0f;
            _exitTime = Random.Range(_minWaitingTime, _maxWatingTime);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _timer += Time.deltaTime;
            if (_timer >= _exitTime) 
            {
                animator.SetTrigger("Move");
            }
        }

        //public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{

        //}
    }
}
