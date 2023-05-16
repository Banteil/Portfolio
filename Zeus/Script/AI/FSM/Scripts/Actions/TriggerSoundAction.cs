using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("This is a TriggerSound Action", UnityEditor.MessageType.Info)]
#endif
    public class TriggerSoundAction : StateAction
    {       
       public override string CategoryName
        {
            get { return "Generic/"; }
        }
        public override string DefaultName
        {
            get { return "Trigger Sound"; }
        }
        public TriggerSoundAction()
        {
            ExecutionType = FSMComponentExecutionType.OnStateEnter;
        }

        //public AudioSource source; //use to Example 1

        public AudioClip[] clips;
        public float minVolume=0.5f, maxVolume =1;

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            #region Example 1
            if (executionType == FSMComponentExecutionType.OnStateEnter)
            {
                AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length)], fsmBehaviour.transform.position, Random.Range(minVolume, maxVolume));
            }
            #endregion

            #region Example 2
            //if (executionType == vFSMComponentExecutionType.OnStateEnter)
            //{
            //    var _source = Instantiate(source, fsmBehaviour.transform.position, Quaternion.identity);
            //    _source.volume = Random.Range(minVolume, maxVolume);
            //    _source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            //}
            #endregion
        }
    }
}