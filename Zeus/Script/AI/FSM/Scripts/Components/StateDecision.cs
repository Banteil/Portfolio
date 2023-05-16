using System;
using UnityEngine;

namespace Zeus
{
    public abstract class StateDecision : ScriptableObject
    {
        public abstract string CategoryName { get; }
        public abstract string DefaultName { get; }
        public FSMBehaviour ParentFSM;
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public virtual Type RequiredType { get { return typeof(IControlAI); } }

        public abstract bool Decide(IFSMBehaviourController fsmBehaviour = null);//{ return true; }

        protected virtual bool InTimer(IFSMBehaviourController fsmBehaviour, float compareTimer = 1f, string timerTag = "")
        {
            var tag = string.IsNullOrEmpty(timerTag) ? name : timerTag;
            float timer = fsmBehaviour.GetTimer(tag);
            fsmBehaviour.SetTimer(tag, timer + GameTimeManager.Instance.DeltaTime);
            if (timer > compareTimer)
            {
                fsmBehaviour.SetTimer(tag, 0);
                return true;
            }
            return false;
        }

        protected virtual bool InRandomTimer(IFSMBehaviourController fsmBehaviour, float minTimer, float maxTimer, string timerTag = "")
        {
            var tag = string.IsNullOrEmpty(timerTag) ? name : timerTag;
            if (!fsmBehaviour.HasTimer(tag))
            {
                fsmBehaviour.SetTimer(tag, UnityEngine.Random.Range(minTimer, maxTimer) + Time.time);
            }
            float timer = fsmBehaviour.GetTimer(tag);
            if (timer < Time.time)
            {
                fsmBehaviour.SetTimer(tag, UnityEngine.Random.Range(minTimer, maxTimer) + Time.time);
                return true;
            }
            return false;
        }
        #region Editor

#if UNITY_EDITOR       
        public bool EditingName;
        public Rect TrueRect = new Rect(0, 0, 10, 10);
        public Rect FalseRect = new Rect(0, 0, 10, 10);
        public bool SelectedTrue, SelectedFalse;
#endif
        #endregion
    }
}
