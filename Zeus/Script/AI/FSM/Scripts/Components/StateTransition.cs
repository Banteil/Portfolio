
using System.Collections.Generic;
using UnityEngine;
namespace Zeus
{
    [System.Serializable]
    public class StateTransition
    {
        public List<StateDecisionObject> Decisions = new List<StateDecisionObject>();
        public FSMState TrueState, FalseState;
        public bool MuteTrue, MuteFalse;
        public TransitionOutputType TransitionType = TransitionOutputType.Default;
        //[vEnumFlag]
        //public vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate;
        //  public bool validate;
        public float TransitionDelay;

        public StateTransition(StateDecision decision)
        {
            if (decision)
                Decisions.Add(new StateDecisionObject(decision));
        }

        public FSMState parentState;

        Dictionary<IFSMBehaviourController, float> transitionTimers;
        public FSMState TransitTo(IFSMBehaviourController fsmBehaviour)
        {
            var val = true;
            FSMState returState = null;
            for (int i = 0; i < Decisions.Count; i++)
            {
                bool value = Decisions[i].Validate(fsmBehaviour);

                if (!value)
                {
                    val = false;
                }
            }
            if (val && TrueState) returState = useTruState && !MuteTrue ? TrueState : null;
            else if (!val && FalseState) returState = useFalseState && !MuteFalse ? FalseState : null;

            if (transitionTimers == null) transitionTimers = new Dictionary<IFSMBehaviourController, float>();
            if (!transitionTimers.ContainsKey(fsmBehaviour)) transitionTimers.Add(fsmBehaviour, 0f);

            if (transitionTimers[fsmBehaviour] < TransitionDelay && returState)
            {
                transitionTimers[fsmBehaviour] += Time.deltaTime;
                if (fsmBehaviour.DebugMode) fsmBehaviour.SendDebug("<color=green>" + parentState.name + " Delay " + (TransitionDelay - transitionTimers[fsmBehaviour]).ToString("00") + " To Enter in " + returState.Name + "</color>", parentState);
                return null;
            }
            else
            {
                transitionTimers[fsmBehaviour] = 0;
                if (fsmBehaviour.DebugMode && returState) fsmBehaviour.SendDebug("<color=yellow>" + parentState.name + " Transited to " + returState.name + "</color>", parentState);
            }
            return returState;
        }

        public bool useTruState
        {
            get { return (TransitionType == TransitionOutputType.TrueFalse || TransitionType == TransitionOutputType.Default); }
        }

        public bool useFalseState
        {
            get { return (TransitionType == TransitionOutputType.TrueFalse); }
        }


        #region Editor
#if UNITY_EDITOR
        public Rect TrueRect, FalseRect;
        public bool SelectedTrue, SelectedFalse;
        public bool TrueSideRight;
        public bool FalseSideRight;
        public UnityEditor.Editor DecisionEditor;
        public bool IsOpen;
        public Vector3 ScroolView;
        public int SameTargetCount;
        public void SetTrueState(FSMState node)
        {
            if (node.CanTranstTo)
                TrueState = node;
        }

        public void SetFalseState(FSMState node)
        {
            if (node.CanTranstTo)
                FalseState = node;
        }
#endif
        #endregion
    }
}
