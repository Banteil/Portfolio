using System.Linq;
using UnityEngine;

namespace Zeus
{
    public class AdvancedStateMachineBehaviour : StateMachineBehaviour
    {
        protected AnimatorStateInfo stateInfo;
        public AnimatorStateInfo StateInfo
        {
            get { return stateInfo; }
        }

        // Use this for initialization

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            this.stateInfo = stateInfo;
        }
    }

    public static class Utilities
    {
        public static T GetBehaviour<T>(this Animator animator, AnimatorStateInfo stateInfo) where T : AdvancedStateMachineBehaviour
        {
            var behaviours = animator.GetBehaviours<T>();
            if (behaviours == null)
            {
                Debug.LogError($"������� �ִϸ����Ϳ� {typeof(T).Name} Behaviour Ÿ���� �������� �ʽ��ϴ�.");
                return null;
            }

            var first = behaviours.ToList().FirstOrDefault
                (
                    behaviour => behaviour.StateInfo.fullPathHash == stateInfo.fullPathHash
                );

            return first;
        }
    }
}

