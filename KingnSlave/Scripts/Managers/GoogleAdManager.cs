using System;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class GoogleAdManager : Singleton<GoogleAdManager>
    {
        // These ad units are configured to always serve test ads.
        private string getGemRewardID;
        private RewardedAd _rewardedAd;
        private bool AdReady
        {
            get
            {
                var ready = _rewardedAd != null && _rewardedAd.CanShowAd();
                return ready;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            //GoogleMobileAds.Mediation.UnityAds.Api.UnityAds.SetConsentMetaData("gdpr.consent", true);
            //GoogleMobileAds.Mediation.UnityAds.Api.UnityAds.SetConsentMetaData("privacy.consent", true);

            UserDataManager.Instance.CompleteSetMyDataCallback += SetRewardID;
            RequestConfiguration requestConfiguration = new RequestConfiguration
            {
                TagForChildDirectedTreatment = TagForChildDirectedTreatment.True,
                TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.True,
                MaxAdContentRating = MaxAdContentRating.G
            };
            MobileAds.SetRequestConfiguration(requestConfiguration);
            MobileAds.Initialize(initStatus => { });
        }

        async private void SetRewardID()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var keyID = Define.CDKey.android_get_gem_reward_id.ToString();
#elif UNITY_IOS && !UNITY_EDITOR
            var keyID = Define.CDKey.ios_get_gem_reward_id.ToString();
#else
            var keyID = Define.CDKey.test_get_gem_reward_id.ToString();
#endif
            await CallAPI.APISelectKeyValue(UserDataManager.Instance.MySid, keyID, (obj) =>
            {
                if (obj != null)
                {
                    getGemRewardID = (string)obj;
                    LoadRewardedAd();
                }
            });
        }

        public bool ShowRewardedAd(Action callback)
        {
            if (AdReady)
            {
                _rewardedAd.Show((Reward reward) =>
                {
                    const string rewardMsg =
                        "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";
                    Debug.Log(string.Format(rewardMsg, reward.Type, reward.Amount));
                    callback();
                });
                return true;
            }
            else
            {
                //UIManager.Instance.ShowWarningUI("failedWatchAdText");
                LoadRewardedAd();
                return false;
            }
        }

        /// <summary>
        /// Loads the rewarded ad.
        /// </summary>
        private void LoadRewardedAd()
        {
            //새 광고를 로드하기 전에 이전 광고를 정리.
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            // 광고를 로드하는 데 사용되는 요청을 만듦
            var adRequest = new AdRequest();

            // 광고 로드 요청을 보냄
            RewardedAd.Load(getGemRewardID, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // 오류가 null이 아니면 로드 요청이 실패한 것임
                    if (error != null || ad == null)
                    {
                        Debug.LogError($"Rewarded ad failed to load an ad with error : {error}");
                        return;
                    }

                    Debug.Log("Rewarded ad loaded with response : "
                              + ad.GetResponseInfo());

                    _rewardedAd = ad;

                    RegisterEventHandlers(_rewardedAd);
                    RegisterReloadHandler(_rewardedAd);
                });
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("Rewarded ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Rewarded ad full screen content opened.");
                try
                {
                    AudioManager.Instance.SetMasterVolume(0);
                }
                catch (Exception ex)
                {
                    Debug.Log($"exception: OnAdFullScreenContentOpened. [{ex}]");
                }
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
                try
                {
                    float currentVolume = AudioManager.Instance.GetMasterVolume();
                    AudioManager.Instance.SetMasterVolume(currentVolume);
                }
                catch (Exception ex)
                {
                    Debug.Log($"exception: OnAdFullScreenContentClosed. [{ex}]");
                }
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);
                UIManager.Instance.ShowWarningUI("failedWatchAdText");
            };
        }

        private void RegisterReloadHandler(RewardedAd ad)
        {
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded Ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                LoadRewardedAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);

                // Reload the ad so that we can show another as soon as possible.
                LoadRewardedAd();
            };
        }
    }
}
