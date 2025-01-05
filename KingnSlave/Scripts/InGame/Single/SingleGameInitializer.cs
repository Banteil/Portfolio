using UnityEngine;

namespace starinc.io.kingnslave
{
    public class SingleGameInitializer : InGameInitializer
    {
        [SerializeField]
        protected SinglePlayer player;
        [SerializeField]
        protected SingleOpponent opponentAI;

        private void Awake()
        {
            InitializeInGame();
        }

        public override void SpawnPlayer()
        {
            SinglePlayer yourPlayer = Instantiate(player);
            InGameManager.Instance.SetPlayerYou(yourPlayer);
            SingleOpponent opponentPlayer = Instantiate(opponentAI);
            InGameManager.Instance.SetPlayerOpponent(opponentPlayer);
        }

        public override void DetermineFirstPlayer()
        {
            if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.SingleStory)
            {
                GameManager.Instance.SingleMode = Define.SingleFirstTurnMode.Slave;
            }

            switch (GameManager.Instance.SingleMode)
            {
                case Define.SingleFirstTurnMode.King:
                    SingleGameManager.Instance.SetKingPlayer(true);
                    break;
                case Define.SingleFirstTurnMode.Slave:
                    SingleGameManager.Instance.SetKingPlayer(false);
                    break;
                default:
                    int randomNum =  Random.Range(0, 1 + 1);
                    if (randomNum == 0)
                    {
                        SingleGameManager.Instance.SetKingPlayer(true);
                    }
                    else
                    {
                        SingleGameManager.Instance.SetKingPlayer(false);
                    }
                    break;
            }
        }
    }
}