using System;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class ItemData
    {
        public int seq;
        public int type;
        public string name;
        public int price;
        public int price_gem;
        public int value;
        public int order_no;
        public int required_tier;
        public int repurchaseable;
        public int useable_min;
        public string detail;
        public string image_url;
        public string usage_image_url;
        public string display_yn;
        /// <summary>
        /// Y == 구매 가능, N == 구매 불가능
        /// </summary>
        public string purchasable_yn;
        public int item_seq;
        /// <summary>
        /// 0(미사용), 1(사용중)
        /// </summary>
        public int in_use;
        public string use_start_time;
        public string use_end_time;
    }
}
