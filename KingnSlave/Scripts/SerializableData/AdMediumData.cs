using System;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class AdMediumData
    {
        public int seq;
        public int medium_type;
        public string medium_url;
        public int client_cd;
        public string paid_time;
        public string start_date;
        public string start_time;
        public string end_date;
        public string end_time;
        public string del_yn;
    }
}
