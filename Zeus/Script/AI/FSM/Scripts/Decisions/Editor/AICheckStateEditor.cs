using UnityEditor;
using UnityEngine;
namespace Zeus
{
    [CustomEditor(typeof(AICheckState), true)]
    public class AICheckStateEditor : StateDecisionEditor
    {
        protected string[] stateList = new string[0];
        protected override void DrawProperties()
        {
            base.DrawProperties();
            if(serializedObject!=null)
            {
                var stateIndex = serializedObject.FindProperty("_stateIndex");
                if(stateIndex!=null)
                {
                    var stateList = GetStateList();
                    if (stateList.Length>0)
                    {                        
                        stateIndex.intValue = EditorGUILayout.Popup("FSM State Equals",stateIndex.intValue, stateList);
                    }
                    else
                    {
                        if (decision.ParentFSM)
                            GUILayout.Box("No States In FSM", skin.box);
                        else
                            EditorGUILayout.HelpBox("State selector will appear when it is in a State", MessageType.Info);
                    }
                }
            }
        }

        protected virtual string[] GetStateList()
        {
            if(decision && decision.ParentFSM)
            {
                if(stateList==null || stateList.Length != decision.ParentFSM.States.Count-2)
                stateList = new string[decision.ParentFSM.States.Count];
                for(int i=0;i<decision.ParentFSM.States.Count-2;i++)
                {
                    stateList[i] = decision.ParentFSM.States[i+2].Name;
                }
            }
            return stateList;
        }
    }
}