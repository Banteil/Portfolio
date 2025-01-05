using System;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "PhaseTable", menuName = "Scriptable Objects/Minigame/PhaseTable")]
    public class PhaseTable : ScriptableObject
    {
        [SerializeField]
        private List<PhaseData> _phaseDatas;
        
        public int GetPhaseData(int score)
        {
            foreach (var phaseData in _phaseDatas)
            {
                if (score >= phaseData.MinScore && score < phaseData.MaxScore)
                {
                    return phaseData.Phase;
                }
            }
            return 0;
        }
    }

    [Serializable]
    public class PhaseData
    {
        [SerializeField]
        private int _phase = 1;
        public int Phase { get { return _phase; } }

        [SerializeField]
        public int _minScore = 0;
        public int MinScore { get { return _minScore; } }

        [SerializeField]
        private int _maxScore = 100;
        public int MaxScore { get { return _maxScore;} }
    }
}
