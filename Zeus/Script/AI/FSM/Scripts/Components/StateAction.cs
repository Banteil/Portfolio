using System;
using UnityEngine;

namespace Zeus
{
    public enum CombatRangeState { Infighter, CombatDistance, Outfighter }

    public abstract class StateAction : ScriptableObject
    {
        public abstract string CategoryName { get; }
        public abstract string DefaultName { get; }
        public virtual Type RequiredType { get { return typeof(IControlAI); } }
        public FSMBehaviour ParentFSM;
        [HideInInspector]
        protected CombatRangeState _combatRange;

        public FSMComponentExecutionType ExecutionType = FSMComponentExecutionType.OnStateUpdate;
        public abstract void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate);

        protected virtual bool InTimer(IFSMBehaviourController fsmBehaviour, float compareTimer = 1f, string timerTag = "")
        {
            var tag = string.IsNullOrEmpty(timerTag) ? name : timerTag;
            float timer = fsmBehaviour.GetTimer(tag);
            fsmBehaviour.SetTimer(tag, timer + Time.deltaTime);
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

        protected virtual bool MoveForward(IControlAIZeus controller)
        {
            switch (_combatRange)
            {
                case CombatRangeState.Infighter:
                    return controller.TargetDistance > controller.MinDistanceOfTheTarget;
                case CombatRangeState.CombatDistance:
                    return controller.TargetDistance > controller.CombatRange * 0.8f;
                case CombatRangeState.Outfighter:
                    return controller.TargetDistance > controller.AttackDistance * 0.8f;
                default:
                    return true;
            }
        }

#if UNITY_EDITOR
        public event UnityEngine.Events.UnityAction<StateAction> OnDestroy;

        public bool EditingName;

        public void DestroyImmediate()
        {
            DestroyImmediate(this, true);
            if (OnDestroy != null)
                OnDestroy.Invoke(this);
        }
#endif
    }
}