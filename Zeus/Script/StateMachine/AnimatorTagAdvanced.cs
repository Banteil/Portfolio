using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class AnimatorTagAdvanced : AnimatorTagBase
    {
        public enum AnimatorEventTriggerType
        {
            AllByNormalizedTime, EnterStateExitByNormalized, EnterByNormalizedExitState, EnterStateExitState
        }
        [System.Serializable]

        public class AdvancedTags
        {
            public string TagName;
            public AnimatorEventTriggerType TagType = AnimatorEventTriggerType.AllByNormalizedTime;

            public Vector2 NormalizedTime = new Vector2(0.1f, 0.8f);
            private int _loopCount;
            public AdvancedTags(string tag)
            {
                this.TagName = tag;
                this.TagType = AnimatorEventTriggerType.AllByNormalizedTime;
            }

            bool _isEnter;
            bool _isExit;
            
            public void UpdateEventTrigger(float normalizedTime, List<AnimatorStateInfos> stateInfos, int layer, float speed = 1, bool looping = false, bool inExit = false, bool debug = false)
            {
                var normalizedTimeClamped = normalizedTime % 1;
                if (!_isEnter && !inExit && TagType != AnimatorEventTriggerType.EnterStateExitByNormalized &&
                                     TagType != AnimatorEventTriggerType.EnterStateExitState && normalizedTimeClamped >= this.NormalizedTime.x)
                {
                    if (debug) Debug.Log("ADD TAG " + TagName + " in  " + normalizedTime);

                    AddTag(stateInfos, layer);
                }

                if (!_isExit && _isEnter && TagType != AnimatorEventTriggerType.EnterByNormalizedExitState &&
                                               TagType != AnimatorEventTriggerType.EnterStateExitState && (normalizedTimeClamped >= this.NormalizedTime.y || inExit))
                {
                    RemoveTag(stateInfos, layer);
                    if (debug) Debug.Log("REMOVE TAG " + TagName + " in  " + normalizedTime);
                }

                if (_isEnter && (normalizedTimeClamped < this.NormalizedTime.x)){
                    RemoveTag(stateInfos, layer);
                }

                if (looping && normalizedTime > _loopCount + 1)
                {
                    _isEnter = false;
                    _isExit = false;
                    _loopCount++;
                }
            }

            public void AddTag(List<AnimatorStateInfos> stateInfos, int layer)
            {
                for (int i = 0; i < stateInfos.Count; i++)
                    stateInfos[i].AddStateInfo(TagName, layer);

                _isEnter = true;
            }

            public void RemoveTag(List<AnimatorStateInfos> stateInfos, int layer)
            {
                for (int i = 0; i < stateInfos.Count; i++)
                {
                    stateInfos[i].RemoveStateInfo(TagName, layer); _isExit = true;
                }
            }

            public void Init(List<AnimatorStateInfos> stateInfos, int layer)
            {
                if (_isEnter && !_isExit)
                {
                    RemoveTag(stateInfos, layer);
                }
                _isEnter = false;
                _isExit = false;
                _loopCount = 0;
            }
        }
        public bool DebugMode;
        public List<AdvancedTags> Tags = new List<AdvancedTags>() { new AdvancedTags("CustomAction") };

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (StateInfos != null)
            {
                for (int i = 0; i < Tags.Count; i++)
                {
                    Tags[i].Init(StateInfos, layerIndex);
                    if (DebugMode) Debug.Log("Init " + Tags[i].TagName + " OnStateEnter  ");
                    if (Tags[i].TagType == AnimatorEventTriggerType.EnterStateExitState || Tags[i].TagType == AnimatorEventTriggerType.EnterStateExitByNormalized)
                    {
                        if (DebugMode) Debug.Log("ADD TAG " + Tags[i].TagName + " OnStateEnter  ");
                        Tags[i].AddTag(StateInfos, layerIndex);
                    }
                    else
                        Tags[i].UpdateEventTrigger(stateInfo.normalizedTime, StateInfos, layerIndex, animator.speed, stateInfo.loop, false, DebugMode);
                }

            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (StateInfos != null)
            {
                for (int i = 0; i < Tags.Count; i++)
                {
                    if (Tags[i].TagType != AnimatorEventTriggerType.EnterStateExitState)
                        Tags[i].UpdateEventTrigger(stateInfo.normalizedTime, StateInfos, layerIndex, animator.speed, stateInfo.loop, false, DebugMode);
                }

                for (int a = 0; a < StateInfos.Count; a++)
                {
                    StateInfos[a].UpdateStateInfo(layerIndex, stateInfo.normalizedTime, stateInfo.shortNameHash);
                }
            }
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (StateInfos != null)
            {
                for (int i = 0; i < Tags.Count; i++)
                {
                    if (Tags[i].TagType == AnimatorEventTriggerType.EnterStateExitState || Tags[i].TagType == AnimatorEventTriggerType.EnterByNormalizedExitState)
                    {
                        if (DebugMode) Debug.Log("REMOVE TAG " + Tags[i].TagName + " OnStateExit  ");
                        Tags[i].RemoveTag(StateInfos, layerIndex);
                    }
                    else
                    {
                        Tags[i].UpdateEventTrigger(stateInfo.normalizedTime, StateInfos, layerIndex, animator.speed, stateInfo.loop, true, DebugMode);
                    }
                }
            }
            base.OnStateExit(animator, stateInfo, layerIndex);
        }
    }
}
