using TMPro;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class MultiSceneLoadDirection : BaseDirection
    {
        enum MultiSceneLoadDirectionRawImage
        {
            RedProfileImage,
            BlueProfileImage,
        }

        enum MultiSceneLoadDirectionImage
        {
            RedTierImage,
            BlueTierImage,
            RedCountryInfo,
            BlueCountryInfo,
        }

        enum MultiSceneLoadDirectionText
        {
            RedNickNameText,
            BlueNickNameText,
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            Bind<RawImage>(typeof(MultiSceneLoadDirectionRawImage));
            Bind<Image>(typeof(MultiSceneLoadDirectionImage));
            Bind<TextMeshProUGUI>(typeof(MultiSceneLoadDirectionText));
        }

        public void SetUserData()
        {
            var isBlue = GameManager.Instance.BlueTeamPlayerSid == UserDataManager.Instance.MySid;
            var blueProfileImage = isBlue ? UserDataManager.Instance.MyProfileImage : UserDataManager.Instance.OpponentProfileImageList[0];
            var redProfileImage = isBlue ? UserDataManager.Instance.OpponentProfileImageList[0] : UserDataManager.Instance.MyProfileImage;
            var blueTierImage = isBlue ? ResourceManager.Instance.GetTierSprite(UserDataManager.Instance.MyData.rank_tier, UserDataManager.Instance.MyData.rank_division) :
                ResourceManager.Instance.GetTierSprite(UserDataManager.Instance.OpponentDataList[0].rank_tier, UserDataManager.Instance.OpponentDataList[0].rank_division);
            var redTierImage = isBlue ? ResourceManager.Instance.GetTierSprite(UserDataManager.Instance.OpponentDataList[0].rank_tier, UserDataManager.Instance.OpponentDataList[0].rank_division) :
                ResourceManager.Instance.GetTierSprite(UserDataManager.Instance.MyData.rank_tier, UserDataManager.Instance.MyData.rank_division);
            var blueNickName = isBlue ? UserDataManager.Instance.MyData.nickname : UserDataManager.Instance.OpponentDataList[0].nickname;
            var redNickName = isBlue ? UserDataManager.Instance.OpponentDataList[0].nickname : UserDataManager.Instance.MyData.nickname;
            var blueCountryImage = isBlue ? ResourceManager.Instance.FlagSpriteList[UserDataManager.Instance.MyData.country_seq] : ResourceManager.Instance.FlagSpriteList[UserDataManager.Instance.OpponentDataList[0].country_seq];
            var redCountryImage = isBlue ? ResourceManager.Instance.FlagSpriteList[UserDataManager.Instance.OpponentDataList[0].country_seq] : ResourceManager.Instance.FlagSpriteList[UserDataManager.Instance.MyData.country_seq];

            Get<RawImage>((int)MultiSceneLoadDirectionRawImage.BlueProfileImage).texture = blueProfileImage;
            Get<RawImage>((int)MultiSceneLoadDirectionRawImage.RedProfileImage).texture = redProfileImage;
            Get<Image>((int)MultiSceneLoadDirectionImage.BlueTierImage).sprite = blueTierImage;
            Get<Image>((int)MultiSceneLoadDirectionImage.RedTierImage).sprite = redTierImage;
            Get<Image>((int)MultiSceneLoadDirectionImage.BlueCountryInfo).sprite = blueCountryImage;
            Get<Image>((int)MultiSceneLoadDirectionImage.RedCountryInfo).sprite = redCountryImage;
            Get<TextMeshProUGUI>((int)MultiSceneLoadDirectionText.BlueNickNameText).text = blueNickName;
            Get<TextMeshProUGUI>((int)MultiSceneLoadDirectionText.RedNickNameText).text = redNickName;
        }
    }
}
