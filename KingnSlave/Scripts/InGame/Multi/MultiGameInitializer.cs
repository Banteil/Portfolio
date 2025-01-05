using UnityEngine;

namespace starinc.io.kingnslave
{
    public class MultiGameInitializer : InGameInitializer
    {
        public override void SpawnPlayer() {}

        public override void DetermineFirstPlayer()
        {
            if (GameManager.Instance.BlueTeamPlayerSid == UserDataManager.Instance.MySid)
            {
                MultiGameManager.Instance.SetKingPlayer(true);
            }
            else
            {
                MultiGameManager.Instance.SetKingPlayer(false);
            }
        }
    }
}