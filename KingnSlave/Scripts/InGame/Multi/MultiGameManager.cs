namespace starinc.io.kingnslave
{
    public class MultiGameManager : InGameManager
    {
        public override void SetKingPlayer(bool youreKing)
        {
            You.SetFirstTurnCard(youreKing);
        }
    }
}