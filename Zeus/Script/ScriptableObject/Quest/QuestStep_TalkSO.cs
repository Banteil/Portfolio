using System.Collections;
using UnityEngine;

namespace Zeus
{
    [CreateAssetMenu(menuName = "Zeus/Table/Quest Table/Step/Talk Data")]
    [System.Serializable]
    public class QuestStep_TalkSO : QuestStepSO
    {
        public int TargetActorId = default;
    }
}