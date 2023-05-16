using UnityEngine;

namespace Zeus
{
    [CreateAssetMenu(fileName = "Function_CurHP", menuName = "Zeus/Consume Function/CurHP")]
    public class Function_CurHP : ConsumeFunction
    {
        public override void OnEffect(int value)
        {
            var player = CharacterObjectManager.Get().GetPlayerbleCharacter();
            if (player == null) return;

            player.AddHealth(value);
        }
    }
}
