using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UICommonFrame : UIFrame
    {
        private UIUserProfile profile;
        private bool IsMyData { get { return UserDataManager.Instance.MySid == profile.ProfileUserData.sid; } }
        private bool IsAccountLinking { get { return !string.IsNullOrEmpty(profile.ProfileUserData.email); } }
        private bool IsNoneGame { get { return GameManager.Instance.CurrentGameMode == Define.GamePlayMode.None; } }        

        private enum CommonFrameButton
        {
            UserProfileButton,
            UserNickNameButton,
            UserUIDButton,
            UserPasswordButton,
            AccountButton,
            CountryButton,
        }

        private enum CommonFrameText
        {
            UserNickNameText,
            UserUIDText,
            CurrentTierText,
            RPText,
            PromotionCheckText,
            AccountEmail
        }

        private enum CommonFrameImage
        {
            ProfileImageModifyIcon,
            NickNameModifyIcon,
            UIDModifyIcon,
            PasswordlModifyIcon,
            TierImage,
            CountryButton,
        }

        private enum CommonFrameRawImage
        {
            UserProfileImage,
        }

        private enum CommonFrameObject
        {
            AccountIcons,
            Star,
            Google,
            Apple,
            Facebook,
            AccountUI,
        }

        protected override void InitializedProcess()
        {
            profile = Util.FindComponentInParents<UIUserProfile>(transform);

            Bind<Button>(typeof(CommonFrameButton));
            var imageModifyButton = GetButton((int)CommonFrameButton.UserProfileButton);
            imageModifyButton.gameObject.BindEvent(ModifyProfileImage);
            var nickNameModifyButton = GetButton((int)CommonFrameButton.UserNickNameButton);
            nickNameModifyButton.gameObject.BindEvent(ModifyNickNameInfo);
            var uidModifyButton = GetButton((int)CommonFrameButton.UserUIDButton);
            uidModifyButton.gameObject.BindEvent(ModifyUIDInfo);
            var passwordModifyButton = GetButton((int)CommonFrameButton.UserPasswordButton);
            passwordModifyButton.gameObject.BindEvent(ModifyPasswordInfo);
            var countrydModifyButton = GetButton((int)CommonFrameButton.CountryButton);
            countrydModifyButton.gameObject.BindEvent(ModifyCountryInfo);

            Bind<TextMeshProUGUI>(typeof(CommonFrameText));
            var accountButton = GetButton((int)CommonFrameButton.AccountButton);
            accountButton.gameObject.BindEvent(InteractAccountToggle);

            Bind<Image>(typeof(CommonFrameImage));
            Bind<RawImage>(typeof(CommonFrameRawImage));
            Bind<GameObject>(typeof(CommonFrameObject));

            UserDataManager.Instance.ChangeMyProfileImageCallback += ChangeProfileImage;
        }

            public override void ActiveFrame(bool isActive)
            {
                LogManager.Instance.InsertActionLog(2);
                base.ActiveFrame(isActive);
            }

        protected override void SetDataProcess<T>(T data)
        {
            UserData userData = data as UserData;
            if (userData == null)
            {
                Debug.LogError("유저 데이터가 존재하지 않습니다.");
                return;
            }
            SetUserInfo(userData);
            SetTierInfo(userData);
        }

        private void SetUserInfo(UserData userData)
        {
            GetText((int)CommonFrameText.UserNickNameText).text = userData.nickname;
            GetText((int)CommonFrameText.UserUIDText).text = $"ID : {userData.uid}";
            GetText((int)CommonFrameText.AccountEmail).text = userData.email;
            GetImage((int)CommonFrameImage.CountryButton).sprite = ResourceManager.Instance.FlagSpriteList[userData.country_seq];
            var profileImage = GetRawImage((int)CommonFrameRawImage.UserProfileImage);
            if (profileImage.texture.name != userData.profile_image)
            {
                NetworkManager.Instance.GetTexture((texture) =>
                {
                    profileImage.texture = texture;
                }, userData.profile_image);
            }
            CheckCommonState();
        }

        private void CheckCommonState()
        {
            var imageModifyIcon = GetImage((int)CommonFrameImage.ProfileImageModifyIcon);
            var nickNameModifyIcon = GetImage((int)CommonFrameImage.NickNameModifyIcon);
            var uidModifyIcon = GetImage((int)CommonFrameImage.UIDModifyIcon);
            var passwordObj = GetButton((int)CommonFrameButton.UserPasswordButton).gameObject;
            if (!IsMyData)
            {
                imageModifyIcon.gameObject.SetActive(false);
                nickNameModifyIcon.gameObject.SetActive(false);
                uidModifyIcon.gameObject.SetActive(false);
                passwordObj.SetActive(false);
                Get<GameObject>((int)CommonFrameObject.AccountUI).SetActive(false);
            }

            var accountEmailText = GetText((int)CommonFrameText.AccountEmail);
            if (!IsAccountLinking)
            {
                passwordObj.SetActive(false);
                accountEmailText.gameObject.SetActive(false);
            }
            else
            {
                GetButton((int)CommonFrameButton.AccountButton).gameObject.SetActive(false);
                accountEmailText.text = profile.ProfileUserData.email;
                accountEmailText.gameObject.SetActive(true);
                Get<GameObject>((int)CommonFrameObject.AccountIcons).SetActive(true);
                var loginType = (CommonFrameObject)profile.ProfileUserData.login_type;
                Get<GameObject>((int)(loginType == 0 ? CommonFrameObject.Star : loginType)).SetActive(true);

                if (loginType > CommonFrameObject.Star)
                    passwordObj.SetActive(false);
                else
                    passwordObj.SetActive(true);
            }

            if (!IsNoneGame)
            {
                imageModifyIcon.gameObject.SetActive(false);
                nickNameModifyIcon.gameObject.SetActive(false);
                uidModifyIcon.gameObject.SetActive(false);
                passwordObj.SetActive(false);
            }
        }

        private void SetTierInfo(UserData userData)
        {
            Get<Image>((int)CommonFrameImage.TierImage).sprite = ResourceManager.Instance.GetTierSprite(userData.rank_tier, userData.rank_division);
            GetText((int)CommonFrameText.CurrentTierText).text = Util.GetTierName(userData.rank_tier, userData.rank_division);
            GetText((int)CommonFrameText.RPText).text = $"{userData.rank_point} RP";
            var promoText = GetText((int)CommonFrameText.PromotionCheckText);
            if (userData.promo_yn == "N")
                promoText.enabled = false;
            else
                promoText.text = Util.GetPromotionText(userData);
        }

        private void ModifyProfileImage(PointerEventData data)
        {
            if (!IsMyData || !IsNoneGame) return;
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var modifyProfileUI = UIManager.Instance.ShowUI<UIModifyProfileImage>("ModifyProfileImageUI");
            modifyProfileUI?.SetProfile(profile);
        }

        private void ModifyNickNameInfo(PointerEventData data)
        {
            if (!IsMyData || !IsNoneGame) return;
            LogManager.Instance.InsertActionLog(3);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var modifyNickNameUI = UIManager.Instance.ShowUI<UIModifyNickName>("ModifyNickNameUI");
            modifyNickNameUI?.SetProfile(profile);
        }

        private void ModifyUIDInfo(PointerEventData data)
        {
            if (!IsMyData || !IsNoneGame) return;
            LogManager.Instance.InsertActionLog(4);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var modifyUIDUI = UIManager.Instance.ShowUI<UIModifyUID>("ModifyUIDUI");
            modifyUIDUI?.SetProfile(profile);
        }

        private void ModifyEmailInfo(PointerEventData data)
        {
            if (!IsMyData || !IsNoneGame) return;
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var modifyEmailUI = UIManager.Instance.ShowUI<UIModifyEmail>("ModifyEmailUI");
            modifyEmailUI?.SetProfile(profile);
        }

        private void ModifyPasswordInfo(PointerEventData data)
        {
            if (!IsMyData || !IsNoneGame) return;
            LogManager.Instance.InsertActionLog(5);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var modifyPasswordUI = UIManager.Instance.ShowUI<UIModifyPassword>("ModifyPasswordUI");
            modifyPasswordUI?.SetProfile(profile);
        }

        private void ModifyCountryInfo(PointerEventData data)
        {
            if (!IsMyData || !IsNoneGame) return;
            LogManager.Instance.InsertActionLog(6);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var modifyCountryUI = UIManager.Instance.ShowUI<UIModifyCountry>("ModifyCountryUI");
            modifyCountryUI?.SetProfile(profile);
        }

        private void InteractAccountToggle(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            if (IsNoneGame)
            {
                LogManager.Instance.InsertActionLog(7);
                UIManager.Instance.ShowUI<LogInMethodSelectionWindow>("LogInMethodSelectionUI");
            }
        }

        private void ChangeProfileImage()
        {
            var profileImage = GetRawImage((int)CommonFrameRawImage.UserProfileImage);
            profileImage.texture = UserDataManager.Instance.MyProfileImage;
        }

        private void OnDestroy()
        {
            if (UserDataManager.HasInstance)
                UserDataManager.Instance.ChangeMyProfileImageCallback -= ChangeProfileImage;
        }
    }
}
