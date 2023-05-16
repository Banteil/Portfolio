using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [Serializable]
    public class AIPattern
    {
        public int Index { get; set; }
        public string PatternName;
        public int AttackID;
        public int MaxAttackCount = 1;
        public float AttackRange = 3;
        public int NextIndex = -1;
        [Range(0, 99)] public int Priority = 0;
        public List<StateDecision> Decisions;
    }
}

