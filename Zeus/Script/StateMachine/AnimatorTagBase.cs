using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public abstract class AnimatorTagBase : StateMachineBehaviour
    {
        public delegate void OnStateTrigger(List<string> tags);
        public List<AnimatorStateInfos> StateInfos = new List<AnimatorStateInfos>();
        public event OnStateTrigger OnStateTriggerEnter;
        public event OnStateTrigger OnStateTriggerExit;

        public virtual void AddStateInfoListener(AnimatorStateInfos stateInfo)
        {
            if (!StateInfos.Contains(stateInfo))
            {
                StateInfos.Add(stateInfo);
            }
        }

        public virtual void RemoveStateInfoListener(AnimatorStateInfos stateInfo)
        {
            if (StateInfos.Contains(stateInfo))
            {
                StateInfos.Remove(stateInfo);
            }
        }

        protected virtual void OnStateEnterEvent(List<string> tags)
        {
            if(OnStateTriggerEnter != null)
            {
                OnStateTriggerEnter(tags);
            }
        }

        protected virtual void OnStateExitEvent(List<string> tags)
        {
            if(OnStateTriggerEnter != null)
            {
                OnStateTriggerExit(tags);
            }
        }
    }
}

