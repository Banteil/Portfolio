using System;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class UserData
    {
        public string keyword;
        public string uid;
        public int pageNum;
        public int pageSize;
        public int startRow;
        public int endRow;
        public int seq;
        public string sid;
        public string nickname;
        public string email;
        public string profile_image;
        public int normal_win;
        public int normal_lose;
        public int normal_total;
        public int rank_win;
        public int rank_lose;
        public int rank_total;
        public int rank_tier;
        public int rank_division;
        public int rank_point;
        public int rank_point_hidden;
        public int mmr;
        public string promo_yn;
        public int promo_win;
        public int promo_lose;
        public int promo_total;
        public string promo_result1;
        public string promo_result2;
        public string promo_result3;
        public string promo_result4;
        public string promo_result5;
        public int add_mmr;
        public int add_rank_point;
        public string ip;
        public string push_agree_yn;
        public string adcd;
        public string auth_key;
        public int login_type;
        public int membership_cd;
        public int item_seq_card_skin;
        public int item_seq_profile_image;
        public int gem_amount;
        public int ad_count;
        public string ad_last_time;
        public int single_stage;
        /// <summary>
        /// List<ItemData>로 역직렬화 필요
        /// </summary>
        public object item_user_list;
        public string del_id;
        public int country_seq;
    }
}