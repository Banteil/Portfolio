using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class TriggerSoundByState : StateMachineBehaviour
    {
        public GameObject AudioSource;
        public List<AudioClip> Sounds;
        public float TriggerTime;
        private FisherYatesRandom _random;
        private bool _isTrigger;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _isTrigger = false;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.normalizedTime % 1 >= TriggerTime && !_isTrigger)
            {
                TriggerSound(animator, stateInfo, layerIndex);
            }
            else if (stateInfo.normalizedTime % 1 < TriggerTime && _isTrigger) _isTrigger = false;
        }

        void TriggerSound(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_random == null)
                _random = new FisherYatesRandom();
            _isTrigger = true;
            GameObject audioObject = null;
            if (AudioSource != null)
                audioObject = Instantiate(AudioSource.gameObject, animator.transform.position, Quaternion.identity) as GameObject;
            else
            {
                audioObject = new GameObject("audioObject");
                audioObject.transform.position = animator.transform.position;
            }
            if (audioObject != null)
            {
                var source = audioObject.gameObject.GetComponent<AudioSource>();
                var clip = Sounds[_random.Next(Sounds.Count)];
                source.PlayOneShot(clip);
            }
        }      
    }
}