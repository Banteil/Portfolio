using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace starinc.io.kingnslave
{
    public class InGamePlayerData
    {
        public bool IsKingPlayer { get; private set; }
        public int Score { get; private set; }
        public Define.InGamePhase PlayerPhase { get; private set; }
        private List<CardHistoryData> cardHistoryDatas = new List<CardHistoryData>();

        private ReadOnlyCollection<CardHistoryData> GetList()
        {
            return cardHistoryDatas.AsReadOnly();
        }
    }
}