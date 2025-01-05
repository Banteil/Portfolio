using System;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class RoundData
    {
        public string room_id;
        public int round_no;
        public string sid_blue;
        public string sid_red;
        public string card_array;
        public string submit_blue;
        public string submit_red;
        public string sid_winner;
        public string sid_loser;
        public string reg_id;
        public string mod_id;
    }
}
