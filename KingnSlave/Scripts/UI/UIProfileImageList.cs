using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIProfileImageList : UIList
    {
        private bool isPurchase;
        private bool isRequiredTier;

        enum ProfileImageListRawImage
        {
            ProfileRawImage,
        }

        enum ProfileImageListToggle
        {
            ProfileImageListUI,
        }

        enum ProfileImageListText
        {
            GemPriceInfoText,
        }

        enum ProfileImageListGameObject
        {
            GemPriceInfo,
        }

        protected override void InitializedProcess()
        {
            SetParent<UIModifyProfileImage>();
            Bind<RawImage>(typeof(ProfileImageListRawImage));
            Bind<Toggle>(typeof(ProfileImageListToggle));
            Bind<TextMeshProUGUI>(typeof(ProfileImageListText));
            Bind<GameObject>(typeof(ProfileImageListGameObject));
            var toggle = Get<Toggle>((int)ProfileImageListToggle.ProfileImageListUI);
            toggle.group = transform.parent.GetComponent<ToggleGroup>();
            toggle.gameObject.BindEvent(SelectProfileImage);
        }

        private void SelectProfileImage(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));

            var parent = GetParent<UIModifyProfileImage>();
            if (isDrag || parent.SelectedIndex == index)
            {
                parent.ResetToggle(this);
                return;
            }

            if (!isRequiredTier)
            {
                parent.ResetToggle(this);
                var exceptionNames = Enum.GetNames(typeof(Define.ProfileImageExceptionHandling));
                UIManager.Instance.ShowWarningUI(exceptionNames[(int)Define.ProfileImageExceptionHandling.profileImageBelowTier]);
                return;
            }

            if (isPurchase)
            {
                parent.ResetToggle(this);
                var itemData = parent.GetData(index);
                if (UserDataManager.Instance.MyGem < itemData.price_gem)
                {
                    UIManager.Instance.ShowWarningUI("notEnoughGems");
                    return;
                }
                else
                {
                    UIManager.Instance.ShowYesOrNoUI(Util.GetLocalizationTableString(Define.InfomationLocalizationTable, "purchaseSelectedItems"), async () =>
                    {
                        UIManager.Instance.ShowConnectingUI();
                        var i18n = Util.GetLocalei18n(LocalizationSettings.SelectedLocale);
                        await CallAPI.APIBuyItem(UserDataManager.Instance.MySid, itemData.seq, 1, i18n, async (data) =>
                        {
                            if (data == null)
                            {
                                UIManager.Instance.CloseConnectingUI();
                                UIManager.Instance.ShowWarningUI("purchaseFailed");
                            }
                            else
                            {
                                UserDataManager.Instance.MyGem = data.gem_amount;
                                await ShopManager.Instance.RecachingList();

                                var itemUserList = Util.CastingJsonObject<List<ItemData>>(data.item_user_list);
                                UserDataManager.Instance.LocalRecachingList(itemUserList);
                                UIManager.Instance.CloseConnectingUI();
                                UIManager.Instance.ShowWarningUI("purchaseSuccess");
                            }
                        });
                    });
                    return;
                }
            }
            GetParent<UIModifyProfileImage>().SelectedIndex = index;
        }

        public void SetListData(ItemData data, UserData userData)
        {
            var userTier = userData.rank_tier;
            isRequiredTier = userTier >= data.required_tier;
            isPurchase = data.purchasable_yn == "Y";

            GetText((int)ProfileImageListText.GemPriceInfoText).text = data.price_gem.ToString();
            Get<GameObject>((int)ProfileImageListGameObject.GemPriceInfo).SetActive(isPurchase);
            Get<Toggle>((int)ProfileImageListToggle.ProfileImageListUI).interactable = isRequiredTier;

            NetworkManager.Instance.GetTexture((texture) =>
            {
                var profileImage = Get<RawImage>((int)ProfileImageListRawImage.ProfileRawImage);
                if (profileImage != null)
                    profileImage.texture = texture;
            }, data.image_url);
        }

        public Toggle GetToggle() => Get<Toggle>((int)ProfileImageListToggle.ProfileImageListUI);
    }
}
