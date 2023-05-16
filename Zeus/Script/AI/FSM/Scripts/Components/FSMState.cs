using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Zeus
{
    [SerializeField]
    public class FSMState : ScriptableObject
    {
        #region Editor

#if UNITY_EDITOR

        #region Editor Variables  
        [Multiline]
        public string Description = "FSM State";
        [SerializeField, HideInInspector]
        public int SelectedDecisionIndex;

        [SerializeField, HideInInspector]
        public bool CanRemove = true;
        [SerializeField, HideInInspector]
        public bool CanTranstTo = true;
        [SerializeField, HideInInspector]
        public bool CanSetAsDefault = true;
        [SerializeField, HideInInspector]
        public bool CanEditName = true;
        [SerializeField, HideInInspector]
        public bool CanEditColor = true;
        [SerializeField, HideInInspector]
        public bool IsOpen;
        [SerializeField, HideInInspector]
        public bool IsSelected;
        [SerializeField]
        public Rect NodeRect;
        [SerializeField, HideInInspector]
        public Vector2 PositionRect;
        [SerializeField, HideInInspector]
        public float RectWidth;
        [SerializeField, HideInInspector]
        public bool EditingName;
        [SerializeField, HideInInspector]
        public Color NodeColor = Color.green;
        [SerializeField, HideInInspector]
        public bool ResizeLeft = false;
        [SerializeField, HideInInspector]
        public bool ResizeRight = false;
        [SerializeField, HideInInspector]
        public bool InDrag;
        #endregion

#endif

        #endregion

        #region Public Variables       

        public bool ResetCurrentDestination = true;
        public List<StateTransition> Transitions = new List<StateTransition>();
        public List<StateAction> Actions = new List<StateAction>();
        public FSMComponent Components;
        [SerializeField, HideInInspector]
        public bool UseActions = true;
        [SerializeField, HideInInspector]
        public bool UseDecisions = true;
        public FSMBehaviour ParentGraph;
        public FSMState DefaultTransition;
       

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public virtual Type RequiredType { get { return typeof(IControlAI); } }
        #endregion

        #region Main Methods

        public virtual void OnStateEnter(IFSMBehaviourController fsmBehaviour)
        {
            if (ResetCurrentDestination && fsmBehaviour.IAIController!=null)
                fsmBehaviour.IAIController.Stop();

            if (Components == null)
                Components = new FSMComponent(Actions);
            if (UseActions && Components != null)
                Components.DoActions(fsmBehaviour, FSMComponentExecutionType.OnStateEnter);           
        }

        public virtual void UpdateState(IFSMBehaviourController fsmBehaviour)
        {
            if (Components == null)
                Components = new FSMComponent(Actions);
            if (UseActions && Components != null)
                Components.DoActions(fsmBehaviour, FSMComponentExecutionType.OnStateUpdate);  

            fsmBehaviour.ChangeState(TransitTo(fsmBehaviour));          
        }

        public virtual void OnStateExit(IFSMBehaviourController fsmBehaviour)
        {
            if (Components == null)
                Components = new FSMComponent(Actions);
            if (UseActions && Components != null)
                Components.DoActions(fsmBehaviour, FSMComponentExecutionType.OnStateExit);           
        }

        public FSMState TransitTo(IFSMBehaviourController fsmBehaviour)
        {
            FSMState node = DefaultTransition;
            for (int i = 0; i < Transitions.Count; i++)
            {
                node = Transitions[i].TransitTo(fsmBehaviour);
                if (node) break;
            }
            return node;
        }
        #endregion    
    }
}
