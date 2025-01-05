using System;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class RankGameResultData
    {
        public string sid;
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
    }
}