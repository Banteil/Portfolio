using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    /// <summary>
    /// Message Object to FSM Debug Window
    /// </summary>
    public class FSMDebugObject
    {
        public string Message = string.Empty;
        public Object Sender;

        public FSMDebugObject(string message, Object sender = null)
        {
            if (!string.IsNullOrEmpty(message))
                this.Message = message;
            this.Sender = sender;
        }
    }

    public partial interface IFSMBehaviourController
    {
        Transform transform { get; }

        GameObject gameObject { get; }

        /// <summary>
        /// Use this to stop FSM Update
        /// </summary>
        bool IsStopped { get; set; }

        /// <summary>
        /// Debug mode
        /// </summary>
        bool DebugMode { get; set; }

        /// <summary>
        /// Debug Message List <seealso cref="IFSMBehaviourController.SendDebug(string, Object)"/>
        /// </summary>
        List<FSMDebugObject> DebugList { get; }

        /// <summary>
        /// AI Controller that FSM will to control
        /// </summary>s
        IControlAIZeus IAIController { get; set; }

        ZeusAIController ZeusAIController { get; set; }

        /// <summary>
        /// <seealso cref="FSMBehaviour"/> of the FSMBehaviour
        /// </summary>
        FSMBehaviour FsmBehaviour { get; set; }

        /// <summary>
        /// Any State of the FSM (state that makes updating independent of current)
        /// </summary>
        FSMState AnyState { get; }

        /// <summary>
        /// Last State of the FSM
        /// </summary>
        FSMState LastState { get; }

        /// <summary>
        /// Current State of the FSM
        /// </summary>
        FSMState CurrentState { get; }

        /// <summary>
        /// Retur index of current state  <seealso cref="FSMBehaviour"/>
        /// </summary>
        int IndexOffCurrentState { get; }

        /// <summary>
        /// Retur name of current state <seealso cref="FSMBehaviour"/>
        /// </summary>
        string NameOffCurrentState { get; }

        bool HasTimer(string key);

        void RemoveTimer(string key);

        /// <summary>
        /// Check if timer is greater than FSM timer
        /// </summary>
        /// <param name="timer">timer</param>
        /// <returns></returns>
        float GetTimer(string key);

        /// <summary>
        /// Reset FSM timer to zero. Auto called for <seealso cref="FSMBehaviour"/> when change state
        /// </summary>
        void SetTimer(string key, float timer);

        /// <summary>
        /// Change State
        /// </summary>
        /// <param name="state"> new state</param>
        void ChangeState(FSMState state);

        ///// <summary>
        ///// Change State using index of state in <seealso cref="vFSMBehaviour"/>. If index dont exit,there will be no changes
        ///// </summary>
        ///// <param name="stateIndex"></param>
        //void ChangeState(int stateIndex);

        ///// <summary>
        ///// Change State using name of state in <seealso cref="vFSMBehaviour"/>. If name dont exit, ther will be no changes.
        ///// </summary>
        ///// <param name="stateName">name of the state in <seealso cref="vFSMBehaviour"/></param>
        //void ChangeState(string stateName);
        void ChangeBehaviour(FSMBehaviour behaviour);

        /// <summary>
        /// Start FSM Update
        /// </summary>
        void StartFSM();

        /// <summary>
        /// Stop the FSM Update
        /// </summary>
        void StopFSM();

        /// <summary>
        /// Send debug Message to FSM Debug Window
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="sender">Object sending message</param>
        void SendDebug(string message, Object sender = null);
    }
}