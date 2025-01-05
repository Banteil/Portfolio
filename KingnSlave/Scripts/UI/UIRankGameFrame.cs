using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIRankGameFrame : UIFrame
    {
        private UIUserProfile profile;

        enum UIRankGameFrameText
        {
            WinCountText,
            LoseCountText,
            TotalCountText,
            WinRateText,
            PromotionCheckText,
            CurrentTierText,
            RPText,
        }

        enum UIRankGameFrameImage
        {
            TierImage,
        }

        protected override void InitializedProcess()
        {
            profile = Util.FindComponentInParents<UIUserProfile>(transform);
            Bind<TextMeshProUGUI>(typeof(UIRankGameFrameText));
            Bind<Image>(typeof(UIRankGameFrameImage));
        }

        public override void ActiveFrame(bool isActive)
        {
            LogManager.Instance.InsertActionLog(9);
            base.ActiveFrame(isActive);
        }

        protected override void SetDataProcess<T>(T data)
        {
            var userData = data as UserData;
            if (userData == null)
            {
                Debug.LogError("유저 데이터가 존재하지 않습니다.");
                return;
            }
            SetRateInfo(userData);
            SetTierInfo(userData);
        }

        private void SetRateInfo(UserData userData)
        {
            GetText((int)UIRankGameFrameText.WinCountText).text = userData.rank_win.ToString();
            GetText((int)UIRankGameFrameText.LoseCountText).text = userData.rank_lose.ToString();
            GetText((int)UIRankGameFrameText.TotalCountText).text = userData.rank_total.ToString();
            var winRate = userData.normal_total == 0 ? 0 : (userData.rank_win / (float)userData.rank_total) * 100f;
            if (float.IsNaN(winRate)) winRate = 0;
            GetText((int)UIRankGameFrameText.WinRateText).text = $"{winRate.ToString("F2")}%";
        }

        private void SetTierInfo(UserData userData)
        {
            Get<Image>((int)UIRankGameFrameImage.TierImage).sprite = ResourceManager.Instance.GetTierSprite(userData.rank_tier, userData.rank_division);
            GetText((int)UIRankGameFrameText.CurrentTierText).text = Util.GetTierName(userData.rank_tier, userData.rank_division);
            GetText((int)UIRankGameFrameText.RPText).text = $"{userData.rank_point} RP";
            var promoText = GetText((int)UIRankGameFrameText.PromotionCheckText);
            if (userData.promo_yn == "N")
                promoText.enabled = false;
            else
                promoText.text = Util.GetPromotionText(userData);
        }
    }
}
