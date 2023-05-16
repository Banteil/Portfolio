using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public struct EffectInfo
    {
        public float StartTime;
        public float EndTime;
        public float Interval;
        public float Scale;

        public EffectInfo(Vector2 normalizedTime, float interval, float scale)
        {
            StartTime = normalizedTime.x;
            EndTime = normalizedTime.y;
            Interval = interval;
            Scale = scale;
        }
    }

    public class GroundEffectControlState : StateMachineBehaviour
    {
        public int Index;
        public List<EffectInfo> EffectInfos = new List<EffectInfo>() { new EffectInfo(new Vector2(0.0f, 1.0f), 0.1f, 1f) };
        public List<GameObject> AdditionalEffects = new List<GameObject>();

        GroundEffecter _groundEffecter;
        int _currentIndex = 0;
        float _time;
        bool _isActive;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _groundEffecter ??= animator.GetComponent<GroundEffecter>();
            _currentIndex = 0;
            _time = float.PositiveInfinity;
            _isActive = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (EffectInfos.Count <= 0) return;

            if (stateInfo.normalizedTime % 1 >= EffectInfos[_currentIndex].StartTime && stateInfo.normalizedTime % 1 <= EffectInfos[_currentIndex].EndTime && !_isActive)
            {
                _isActive = true;
                _time += GameTimeManager.Instance.DeltaTime;
                if (_time >= EffectInfos[_currentIndex].Interval)
                {
                    _groundEffecter.CheckGround(Index, EffectInfos[_currentIndex].Scale, AdditionalEffects);
                    _time = 0f;
                }
            }
            else if (stateInfo.normalizedTime % 1 > EffectInfos[_currentIndex].EndTime && _isActive)
            {
                _isActive = false;
                _time = 0f;
                if (_currentIndex < EffectInfos.Count - 1)
                    _currentIndex++;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }
    }
}
