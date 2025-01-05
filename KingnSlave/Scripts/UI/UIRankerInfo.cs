using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIRankerInfo : UIList
    {
        private const string rankerSpriteName = "Ranker_";
        private UserData rankerData;

        enum RankerTextInfo
        {
            RankerIndexText,
            RankerUserNameText,
            RankerFeatureText
        }

        enum RankerImage
        {
            RankerIndexPanel,
            RankerTierImage,
            CountryInfo,
        }

        enum RankerRawImage
        {
            RankerUserImage,
        }

        enum RankerButton
        {
            RankerProfileIcon,
        }

        private void Awake() => Initialized();

        protected override void InitializedProcess()
        {
            SetParent<UIRanking>();
            Bind<TextMeshProUGUI>(typeof(RankerTextInfo));
            Bind<RawImage>(typeof(RankerRawImage));
            Bind<Image>(typeof(RankerImage));
            Bind<Button>(typeof(RankerButton));
            GetButton((int)RankerButton.RankerProfileIcon).gameObject.BindEvent(ShowRankerProfile);
        }

        public override void SetIndex(int index)
        {
            base.SetIndex(index);
            var indexPanel = GetImage((int)RankerImage.RankerIndexPanel);
            var indexText = GetText((int)RankerTextInfo.RankerIndexText);
            var rankIndex = index + 1;
            if (index > 2)
            {
                indexPanel.sprite = ResourceManager.Instance.GetSprite($"{rankerSpriteName}{4}");
                indexText.enabled = true;
                indexText.text = rankIndex.ToString();
            }
            else
            {
                indexPanel.sprite = ResourceManager.Instance.GetSprite($"{rankerSpriteName}{rankIndex}");
                indexText.enabled = false;
            }
        }

        public void SetListData(UserData data, Define.GamePlayMode mode)
        {
            rankerData = data;
            GetImage((int)RankerImage.CountryInfo).sprite = ResourceManager.Instance.FlagSpriteList[data.country_seq];
            GetText((int)RankerTextInfo.RankerUserNameText).text = rankerData.nickname;
            if (rankerData.rank_tier > 0)
                GetImage((int)RankerImage.RankerTierImage).sprite = ResourceManager.Instance.GetTierSprite(data.rank_tier, data.rank_division);

            var rawImage = GetRawImage((int)RankerRawImage.RankerUserImage);
            if (rawImage.texture.name != rankerData.profile_image)
                NetworkManager.Instance.GetTexture((texture) =>
                {
                    if (texture == null) return;
                    rawImage.texture = texture;
                }, rankerData.profile_image);

            if (mode == Define.GamePlayMode.PVPRank)
            {
                var rpText = GetText((int)RankerTextInfo.RankerFeatureText);
                rpText.text = $"{data.rank_point} RP";
            }
            else if (mode == Define.GamePlayMode.PVPNormal)
            {
                var winText = GetText((int)RankerTextInfo.RankerFeatureText);
                winText.text = $"{Util.GetLocalizationTableString(Define.CommonLocalizationTable, "tipUIWinText")}: {data.normal_win}";
            }
            else
            {
                if (data.single_stage == 0) return;
                var stageText = GetText((int)RankerTextInfo.RankerFeatureText);

                int stageInCycle = data.single_stage % 10;
                if (stageInCycle == 0) stageInCycle = 10;
                int cycle = (data.single_stage - stageInCycle) / 10 + 1;
                stageText.text = $"{cycle} - {stageInCycle}";
            }
        }

        private void ShowRankerProfile(PointerEventData data)
        {
            if (isDrag) return;
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            UIManager.Instance.ShowUserProfile(rankerData);
        }
    }
}
