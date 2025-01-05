using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class IronSourceAdManager : Singleton<IronSourceAdManager>
    {
        private string appKey = string.Empty;
        private bool initialized = false;
        private Action rewardedCallback;

        private bool AdReady
        {
            get
            {
                var ready = IronSource.Agent.isRewardedVideoAvailable();
                return ready;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();

#if UNITY_ANDROID
            appKey = "1d8738add";
#elif UNITY_IOS
            appKey = "1d87470fd";
#endif

            if (appKey == string.Empty)
                return;

            IronSource.Agent.setConsent(false);
            IronSource.Agent.setMetaData("do_not_sell", "true");
            IronSource.Agent.setMetaData("is_deviceid_optout", "true");
            IronSource.Agent.setMetaData("is_child_directed", "true");
            IronSource.Agent.setMetaData("UnityAds_coppa", "true");

            IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;
            IronSource.Agent.init(appKey, IronSourceAdUnits.REWARDED_VIDEO);//, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
            IronSource.Agent.validateIntegration();

            //Add AdInfo Rewarded Video Events
            IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;

            //IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
            //IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
            //IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
            //IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
            //IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
            //IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
            //IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;
            //IronSource.Agent.loadInterstitial();
        }

        public bool ShowRewardedAd(Action callback)
        {
            if (AdReady)
            {
                IronSource.Agent.showRewardedVideo();

                Debug.Log($"rewardedCallback: {rewardedCallback}");
                rewardedCallback = null;
                rewardedCallback = callback;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SdkInitializationCompletedEvent()
        {
            initialized = true;
            Debug.Log("Iron Source SDK Init");
        }

        #region RewardEvents
        /************* RewardedVideo AdInfo Delegates *************/
        // Indicates that there’s an available ad.
        // The adInfo object includes information about the ad that was loaded successfully
        // This replaces the RewardedVideoAvailabilityChangedEvent(true) event
        void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
        {
        }
        // Indicates that no ads are available to be displayed
        // This replaces the RewardedVideoAvailabilityChangedEvent(false) event
        void RewardedVideoOnAdUnavailable()
        {
        }
        // The Rewarded Video ad view has opened. Your activity will loose focus.
        void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            Debug.Log("Rewarded ad full screen content opened.");
            NetworkManager.Instance.CancelMatch();
            try
            {
                AudioManager.Instance.SetMasterVolume(0);
            }
            catch (Exception ex)
            {
                Debug.Log($"exception: OnAdFullScreenContentOpened. [{ex}]");
            }
        }
        // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
        void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
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
        }
        // The user completed to watch the video, and should be rewarded.
        // The placement parameter will include the reward data.
        // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
        void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
            Debug.Log($"rewarded event {GetInstanceID()}");
            //if (GetInstanceID() == 0)
            //    return;

            if (rewardedCallback != null)
            {
                rewardedCallback.Invoke();
            }
        }
        // The rewarded video ad was failed to show.
        void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {
        }
        // Invoked when the video ad was clicked.
        // This callback is not supported by all networks, and we recommend using it only if
        // it’s supported by all networks you included in your build.
        void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
        {
        }
        #endregion

        #region InterstitialEvents
        /************* Interstitial AdInfo Delegates *************/
        // Invoked when the interstitial ad was loaded succesfully.
        void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
        {
            Debug.Log(adInfo.ToString());
        }
        // Invoked when the initialization process has failed.
        void InterstitialOnAdLoadFailed(IronSourceError ironSourceError)
        {
            Debug.LogError(ironSourceError.ToString());
        }
        // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
        void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
        {
            Debug.Log(adInfo.ToString());
        }
        // Invoked when end user clicked on the interstitial ad
        void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo)
        {
            Debug.Log(adInfo.ToString());
        }
        // Invoked when the ad failed to show.
        void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
        {
            Debug.LogError(ironSourceError.ToString());
            Debug.Log(adInfo.ToString());
        }
        // Invoked when the interstitial ad closed and the user went back to the application screen.
        void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
        {
            Debug.Log(adInfo.ToString());
            IronSource.Agent.loadInterstitial();
        }
        // Invoked before the interstitial ad was opened, and before the InterstitialOnAdOpenedEvent is reported.
        // This callback is not supported by all networks, and we recommend using it only if  
        // it's supported by all networks you included in your build. 
        void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo)
        {
            Debug.Log(adInfo.ToString());
            if (rewardedCallback != null)
            {
                rewardedCallback.Invoke();

            }
        }
        #endregion


        void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }
    }
}