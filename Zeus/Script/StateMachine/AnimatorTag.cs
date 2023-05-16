using UnityEngine;

namespace Zeus
{
    public class AnimatorTag : AnimatorTagBase
    {
        public string[] Tags = new string[] { "CustomAction" };

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            if (StateInfos != null)
            {
                for (int i = 0; i < Tags.Length; i++)
                {
                    for (int a = 0; a < StateInfos.Count; a++)
                    {
                        StateInfos[a].AddStateInfo(Tags[i], layerIndex);
                    }
                }
            }
            OnStateEnterEvent(Tags.ZToList());
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (StateInfos != null)
            {
                for (int a = 0; a < StateInfos.Count; a++)
                {
                    StateInfos[a].UpdateStateInfo(layerIndex, stateInfo.normalizedTime, stateInfo.shortNameHash);
                }
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (StateInfos != null)
            {
                for (int i = 0; i < Tags.Length; i++)
                {
                    for (int a = 0; a < StateInfos.Count; a++)
                        StateInfos[a].RemoveStateInfo(Tags[i], layerIndex);
                }
            }
            base.OnStateExit(animator, stateInfo, layerIndex);
            OnStateExitEvent(Tags.ZToList());
        }
    }
}