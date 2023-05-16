using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zeus
{
    [System.Serializable]
    public class StateDecisionObject
    {
        public bool TrueValue = true;
        public StateDecision Decision;
        [SerializeField]
        public bool IsValid;
        public bool Validated;

        public StateDecisionObject(StateDecision decision)
        {
            this.Decision = decision;
        }

        public StateDecisionObject Copy()
        {
            var obj = new StateDecisionObject(this.Decision);
            obj.TrueValue = TrueValue;
            return obj;
        }

        public bool Validate(IFSMBehaviourController fsmBehaviour)
        {           
            if (TrueValue)
            {
                IsValid =  /*if a*/Decision ?
                       /*if b*/Decision.Decide(fsmBehaviour) :
                       /*else b*/ true;
            }
            else
            {
                IsValid = !(/*if a*/Decision ?
                      /*if b*/Decision.Decide(fsmBehaviour) :
                      /*else b*/ false);
            }
#if UNITY_EDITOR
            if (ValidationByController == null) ValidationByController = new Dictionary<IFSMBehaviourController, bool>();
            if (ValidationByController.ContainsKey(fsmBehaviour)) ValidationByController[fsmBehaviour] = IsValid;
            else ValidationByController.Add(fsmBehaviour, IsValid);
#endif
            return IsValid;
        }

#if UNITY_EDITOR

        private Editor _decisionEditor;
        public Dictionary<IFSMBehaviourController, bool> ValidationByController;
        public void DrawDecisionEditor()
        {
            if (!Decision) return;

            if (!_decisionEditor)
                _decisionEditor = Editor.CreateEditor(Decision);

            if (_decisionEditor)
                _decisionEditor.OnInspectorGUI();            
        }
#endif       

    }
}
