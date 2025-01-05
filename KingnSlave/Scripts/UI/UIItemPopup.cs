using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public enum LocaleItemTypeKey
    {
        expressionType,
        cardSkinType,
        profileImageType,
    }

    public class UIItemPopup : UIPopup
    {
        protected const int MAX_TRY_COUNT = 5;
        protected int limitValue = -1;
        protected ItemData selectedData;

        protected List<UIItemFrame> itemFrames = new List<UIItemFrame>();
        protected UIItemFrame currentFrame;
        private CancellationTokenSource processCancel = new CancellationTokenSource();

        protected enum ItemPopupText
        {
            GemValueText,
            GetGemLimitText,
            IntervalText,
        }

        protected enum ItemPopupImage
        {
            UsageImage,
        }

        protected enum ItemPopupDropdown
        {
            ItemTypeDropdown,
        }

        protected enum ItemPopupButton
        {
            GetGemButton = 1,
        }

        protected enum ItemPopupFrame
        {
            ExpressionFrame,
            CardSkinFrame,
            ProfileImageFrame,
        }

        protected enum ItemPopupGameObject
        {
            SelectInfo,
        }

        public float InfoHeight
        {
            get
            {
                var rectTr = Get<GameObject>((int)ItemPopupGameObject.SelectInfo).GetComponent<RectTransform>();
                return rectTr.rect.height;
            }
        }
        public event Action<bool> ActiveInfoCallback;

        protected void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TextMeshProUGUI>(typeof(ItemPopupText));
            Bind<TMP_Dropdown>(typeof(ItemPopupDropdown));
            Bind<Button>(typeof(ItemPopupButton));
            var getGemButton = GetButton((int)ItemPopupButton.GetGemButton);
            getGemButton.gameObject.BindEvent(GetGemButton);
            Bind<UIItemFrame>(typeof(ItemPopupFrame));
            Bind<Image>(typeof(ItemPopupImage));
            Bind<GameObject>(typeof(ItemPopupGameObject));

            ActiveInfoUI(false);
            SetFrameData();
            SettingDropDownOption();
            ChangeGemText();

            UserDataManager.Instance.SetMyGemCallback += ChangeGemText;
            AdMediationManager.Instance.IntervalCallback += ActiveInterval;
            if (AdMediationManager.Instance.IntervalTimer > 0)
                ActiveInterval(true);
        }

        protected void SettingDropDownOption()
        {
            var dropdown = Get<TMP_Dropdown>((int)ItemPopupDropdown.ItemTypeDropdown);
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                var type = (LocaleItemTypeKey)i;
                dropdown.options[i].text = Util.GetLocalizationTableString(Define.CommonLocalizationTable, type.ToString());
            }
            dropdown.onValueChanged.AddListener(OpenFrame);
            OpenFrame(0);
        }

        protected void SetFrameData()
        {
            var frames = GetAll<UIItemFrame>();
            foreach (var frame in frames)
            {
                if (frame == null) break;
                frame.Initialized();
                frame.SetData(UserDataManager.Instance.MyData);
                itemFrames.Add(frame);
            }
        }

        public virtual void OpenFrame(int index)
        {
            if (index < 0 || index >= Get<TMP_Dropdown>((int)ItemPopupDropdown.ItemTypeDropdown).options.Count) return;
            ActiveInfoUI(false);
            if (currentFrame != null) currentFrame.ActiveFrame(false);
            itemFrames[index].ActiveFrame(true);
            currentFrame = itemFrames[index];
        }

        #region AD Funtion
        async protected virtual void GetGemButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var checkException = await AdLimitExceptionHandling();
            if (checkException)
            {
                UIManager.Instance.ShowWarningUI("advertisingLimits");
                return;
            }
            if (AdMediationManager.Instance.IntervalTimer > 0)
            {
                UIManager.Instance.ShowWarningUI("adIntervalText");
                return;
            }

            AdMediationManager.Instance.ShowRewardedAd(() =>
            {
                AdmobAdRewardProcess();
            },
            () => 
            {
                IronSourceAdRewardProcess();
            });
        }

        private void ActiveInterval(bool isActive)
        {
            if (isActive)
                IntervalProcess(cancellationToken: processCancel.Token);
            else
                processCancel.Cancel();
            GetText((int)ItemPopupText.IntervalText).gameObject.SetActive(isActive);
        }

        async private void IntervalProcess(CancellationToken cancellationToken)
        {
            var intervalText = GetText((int)ItemPopupText.IntervalText);
            while (AdMediationManager.Instance.IntervalTimer > 0)
            {
                if (cancellationToken.IsCancellationRequested) break;
                var interval = AdMediationManager.Instance.IntervalTimer;
                var minute = (int)interval / 60;
                var sec = (int)interval % 60;
                var result = string.Format("{0:D2}:{1:D2}", minute, sec);
                intervalText.text = result;
                await UniTask.Yield();
            }
            processCancel = new CancellationTokenSource();
        }

        async private UniTask<bool> AdLimitExceptionHandling()
        {
            UIManager.Instance.ShowConnectingUI();
            var tcs = new UniTaskCompletionSource<bool>();

            var count = 0;
            var sid = UserDataManager.Instance.MySid;
            await CallAPI.APISelectUser(sid, sid, (data) =>
            {
                if (data != null)
                    count = data.ad_count;
                else
                    count = UserDataManager.Instance.MyData.ad_count;
            });

            var cdKey = Define.CDKey.ad_day_limit.ToString();
            await CallAPI.APISelectKeyValue(UserDataManager.Instance.MySid, cdKey, (obj) =>
            {
                if (obj != null)
                    limitValue = Convert.ToInt32(obj);
            });

            tcs.TrySetResult(count >= limitValue);
            UIManager.Instance.CloseConnectingUI();
            return await tcs.Task;
        }

        async protected void AdmobAdRewardProcess()
        {
            int count = 0;
            do
            {
                Debug.Log($"Shop Admob AD try count: {count}");
                bool success = false;
                await UniTask.Yield();
                await CallAPI.APIUpdateUserAdReward(UserDataManager.Instance.MySid, (data) =>
                {
                    Debug.Log($"Shop Admob AD API Called. data: {data}, gem: {data.gem_amount}");
                    if (data != null)
                    {
                        UserDataManager.Instance.MyData.ad_count = data.ad_count;
                        UserDataManager.Instance.MyGem = data.gem_amount;
                        success = true;
                    }
                });

                if (success)
                {
                    AdMediationManager.Instance.CountingInterval();
                    UIManager.Instance.ShowWarningUI(Define.AD_REWARD_RECIEVED_KEY, true);
                    return;
                }
                count++;
            }
            while (count < MAX_TRY_COUNT);
            UIManager.Instance.ShowWarningUI("Failure to obtain Ad rewards", false);
        }

        async protected void IronSourceAdRewardProcess()
        {
            int count = 0;
            do
            {
                Debug.Log($"Shop IronSource AD try count: {count}");
                bool success = false;
                await UniTask.Yield();
                await CallAPI.APIUpdateUserAdReward(UserDataManager.Instance.MySid, (data) =>
                {
                    Debug.Log($"Shop IronSource AD API Called. data: {data}, gem: {data.gem_amount}");
                    if (data != null)
                    {
                        UserDataManager.Instance.MyData.ad_count = data.ad_count;
                        UserDataManager.Instance.MyGem = data.gem_amount;
                        success = true;
                    }
                });

                if (success)
                {
                    AdMediationManager.Instance.CountingInterval();
                    UIManager.Instance.ShowWarningUI(Define.AD_REWARD_RECIEVED_KEY, true);
                    return;
                }
                count++;
            }
            while (count < MAX_TRY_COUNT);
            UIManager.Instance.ShowWarningUI("Failure to obtain Ad rewards", false);
        }

        async protected void SettingLimitAdViewingValue() => await SettingLimitAdViewingValueProcess();

        async protected UniTask SettingLimitAdViewingValueProcess()
        {
            var valueText = $"{UserDataManager.Instance.MyData.ad_count}/";
            if (limitValue < 0)
            {
                var cdKey = Define.CDKey.ad_day_limit.ToString();
                await CallAPI.APISelectKeyValue(UserDataManager.Instance.MySid, cdKey, (obj) =>
                {
                    limitValue = Convert.ToInt32(obj);
                });
            }
            valueText += limitValue;

            GetText((int)ItemPopupText.GetGemLimitText).text = valueText;
        }
#endregion

        protected void ChangeGemText()
        {
            GetText((int)ItemPopupText.GemValueText).text = UserDataManager.Instance.MyGem.ToString();
            SettingLimitAdViewingValue();
        }

        public virtual void SelectItem(int index, Define.ItemType itemType) { }

        public void ActiveInfoUI(bool isActive)
        {
            Get<GameObject>((int)ItemPopupGameObject.SelectInfo).SetActive(isActive);
            if (!isActive) selectedData = null;
            ActiveInfoCallback?.Invoke(isActive);
        }

        protected void OnDestroy()
        {
            if (UserDataManager.HasInstance)
                UserDataManager.Instance.SetMyGemCallback -= ChangeGemText;

            if (AdMediationManager.HasInstance)
                AdMediationManager.Instance.IntervalCallback -= ActiveInterval;
            for (int i = 0; i < itemFrames.Count; i++)
            {
                if (itemFrames[i].gameObject.activeSelf) continue;
                itemFrames[i].OnDestroy();
            }
        }
    }
}
