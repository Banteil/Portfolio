
using UnityEngine;

namespace Zeus
{
    public class AIOnlyAbility : CharacterAbility
    {
        protected ZeusAIController _zeusAI;

        protected override void AwakeInitialize()
        {
            base.AwakeInitialize();
            _zeusAI = _owner as ZeusAIController;
            if (_zeusAI == null)
            {
                Debug.LogError("AI 컨트롤러가 확인되지 않습니다.");
                enabled = false;
                return;
            }
        }
    }
}
