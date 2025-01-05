using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using System;
using UnityEngine;

namespace starinc.io
{
    public class AdManager : BaseManager
    {
        #region Cache
        private InterstitialAd _interstitialAd;
        private string _interstitialAdUnitId;

        private Action _onShowAd;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            MobileAds.Initialize(initStatus => { SettingAdUnitId(); });
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
        }

        private async void SettingAdUnitId()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _interstitialAdUnitId = await CallAPI.GetAPIKey("android_interstitial_ads_id");
#elif UNITY_IOS && !UNITY_EDITOR
            _interstitialAdUnitId = await CallAPI.GetAPIKey("ios_interstitial_ads_id");
#else
            _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
            await UniTask.Yield();
#endif
            Debug.Log("Setting Ad Unity Id Complete");
            LoadInterstitialAd();
        }

        private void LoadInterstitialAd()
        {
            if (!Util.CheckNetworkReachability()) return;

            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            var adRequest = new AdRequest();            
            InterstitialAd.Load(_interstitialAdUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"Interstitial ad filed to load an ad with error : {error}");
                    return;
                }
                _interstitialAd = ad;
                RegisterEventHandlers(_interstitialAd);
                Debug.Log("Load Interstitial Ad Complete");
            });
        }

        /// <summary>
        /// Shows the interstitial ad.
        /// </summary>
        public async void ShowInterstitialAd(Action callback)
        {
            if(Manager.User.IsRemoveAds)
            {
                callback?.Invoke();
                return;
            }

            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");                
                _interstitialAd.Show();
                _onShowAd = callback;
            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
                if (_interstitialAd == null)
                    LoadInterstitialAd();

                await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
                callback?.Invoke();
            }
        }

        private void RegisterEventHandlers(InterstitialAd interstitialAd)
        {
            // Raised when the ad is estimated to have earned money.
            interstitialAd.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            interstitialAd.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            interstitialAd.OnAdClicked += () =>
            {
                Debug.Log("Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
                Manager.Sound.PauseBGM();
                Manager.Sound.PauseAllSFX();
            };
            // Raised when the ad closed full screen content.
            interstitialAd.OnAdFullScreenContentClosed += async () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                Manager.Sound.PlayBGM();
                Manager.Sound.ReplayAllSFX();                
                LoadInterstitialAd();

                await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
                _onShowAd?.Invoke();
                _onShowAd = null;
            };
            // Raised when the ad failed to open full screen content.
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);
                LoadInterstitialAd();
                _onShowAd = null;
            };
        }
    }
}