using UnityEngine;

namespace starinc.io.kingnslave
{
    public class InGameInitializer : MonoBehaviour
    {
        public virtual void InitializeInGame()
        {
            SetPlayerData();
            SpawnPlayer();
            DetermineFirstPlayer();
        }

        public virtual void SpawnPlayer()
        {

        }

        public virtual void SetPlayerData()
        {

        }

        public virtual void DetermineFirstPlayer()
        {

        }
    }
}