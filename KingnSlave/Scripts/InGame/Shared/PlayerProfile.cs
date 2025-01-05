using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class PlayerProfile : MonoBehaviour
    {
        [SerializeField]
        protected Define.UserDataType profileUIOwner;

        [SerializeField]
        protected RawImage profileImage;

        [SerializeField]
        protected TMP_Text nickname;

        [SerializeField]
        protected TMP_Text userId;

        [SerializeField]
        protected Image country;

        protected virtual void Start()
        {
            if (profileUIOwner == Define.UserDataType.You)
            {
                nickname.text = UserDataManager.Instance.MyData.nickname;
                userId.text = UserDataManager.Instance.MyData.uid;
                profileImage.texture = UserDataManager.Instance.MyProfileImage;
                Debug.Log("my country: " + UserDataManager.Instance.MyData.country_seq);
                country.sprite = ResourceManager.Instance.FlagSpriteList[UserDataManager.Instance.MyData.country_seq];
                if (profileImage == null)
                {
                    NetworkManager.Instance.GetTexture((texture) =>
                    {
                        profileImage.texture = texture;
                        UserDataManager.Instance.MyProfileImage = texture;
                    }, UserDataManager.Instance.MyData.profile_image);
                }
            }
            else
            {
                nickname.text = UserDataManager.Instance.OpponentDataList[0].nickname;
                userId.text = UserDataManager.Instance.OpponentDataList[0].uid;
                profileImage.texture = UserDataManager.Instance.OpponentProfileImageList[0];
                Debug.Log("oppo country: " + UserDataManager.Instance.OpponentDataList[0].country_seq);
                country.sprite = ResourceManager.Instance.FlagSpriteList[UserDataManager.Instance.OpponentDataList[0].country_seq];
                if (profileImage == null)
                {
                    NetworkManager.Instance.GetTexture((texture) =>
                    {
                        profileImage.texture = texture;
                        UserDataManager.Instance.OpponentProfileImageList[0] = texture;
                    }, UserDataManager.Instance.OpponentDataList[0].profile_image);
                }
            }
        }
    }
}