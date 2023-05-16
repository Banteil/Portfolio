
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
                Debug.LogError("AI ��Ʈ�ѷ��� Ȯ�ε��� �ʽ��ϴ�.");
                enabled = false;
                return;
            }
        }
    }
}
