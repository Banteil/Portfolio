using System;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class GameResultData
    {
        public string room_id;
        public int room_no;
        public int game_type;
        public int status;
        public string title;
        public string sid_blue;
        public int rank_tier_blue;
        public int rank_division_blue;
        public int mmr_blue;
        public string sid_red;
        public int rank_tier_red;
        public int rank_division_red;
        public int mmr_red;
        public string sid_observer;
        public int is_open;
        public int is_valid;
        public int is_visible;
        public int max_players;
        public int player_count;
        public string region;
        public string sid_winner;
        public string sid_loser;
        public int score_winner;
        public int score_loser;
        public int rank_point_winner;
        public int rank_point_loser;
        public int mmr_winner;
        public int mmr_loser;
        public string reg_id;
        public string mod_id;
        public string uid_blue;
        public string nickname_blue;
        public string profile_image_blue;
        public string uid_red;
        public string nickname_red;
        public string profile_image_red;
        public string my_team;
        public string my_result;
        public string mod_time;
        public int country_seq_blue;
        public int country_seq_red;
    }
}