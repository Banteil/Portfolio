using System;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class GameRoomData
    {
        public int seq;
        public string id;
        public string title;
        public string sid_blue;
        public string sid_red;
        public string sid_observer;
        public bool is_open;
        public string is_valid;
        public string is_visible;
        public int max_players;
        public int player_count;
        public string region;
        public string reg_id;
    }
}