using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UINormalGameFrame : UIFrame
    {
        private UIUserProfile profile;

        enum UINormalGameFrameText
        {
            WinCountText,
            LoseCountText,
            TotalCountText,
            WinRateText,
        }

        enum UINormalGameFrameImage
        {
            RateStateImage,
        }

        protected override void InitializedProcess()
        {
            profile = Util.FindComponentInParents<UIUserProfile>(transform);
            Bind<TextMeshProUGUI>(typeof(UINormalGameFrameText));
            Bind<Image>(typeof(UINormalGameFrameImage));
        }

        public override void ActiveFrame(bool isActive)
        {
            LogManager.Instance.InsertActionLog(8);
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
        }

        private void SetRateInfo(UserData userData)
        {
            GetText((int)UINormalGameFrameText.WinCountText).text = userData.normal_win.ToString();
            GetText((int)UINormalGameFrameText.LoseCountText).text = userData.normal_lose.ToString();
            GetText((int)UINormalGameFrameText.TotalCountText).text = userData.normal_total.ToString();
            var winRate = userData.normal_total == 0 ? 0 : (userData.normal_win / (float)userData.normal_total) * 100f;
            if(float.IsNaN(winRate)) winRate = 0;
            GetText((int)UINormalGameFrameText.WinRateText).text = $"{winRate.ToString("F2")}%";
            GetImage((int)UINormalGameFrameImage.RateStateImage).sprite = ResourceManager.Instance.GetSprite(GetImageName(winRate));
        }

        private string GetImageName(float winRate)
        {
            if (winRate >= 60)
                return "NormalGameFrame_King";
            else if (winRate < 60 && winRate >= 30)
                return "NormalGameFrame_Citizen";
            else
                return "NormalGameFrame_Slave";
        }
    }
}
