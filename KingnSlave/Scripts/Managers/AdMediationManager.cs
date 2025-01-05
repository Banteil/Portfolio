using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class AdMediationManager : Singleton<AdMediationManager>
    {
        public float IntervalTimer { get; set; } = 0;
        public event Action<bool> IntervalCallback;

        async public void CountingInterval()
        {
            IntervalTimer = 5f;
            IntervalCallback?.Invoke(true);
            while (true)
            {
                IntervalTimer -= Time.deltaTime;
                if (IntervalTimer <= 0)
                {
                    IntervalTimer = 0;
                    IntervalCallback?.Invoke(false);
                    return;
                }
                await UniTask.Yield();
            }
        }

        public void ShowRewardedAd(Action admobCallback, Action ironSourceCallback)
        {
            Debug.Log("Show Ad Call");
            bool success = false;
            success = GoogleAdManager.Instance.ShowRewardedAd(admobCallback);

            if (success)
            {
                return;
            }
            else
            {
                success = IronSourceAdManager.Instance.ShowRewardedAd(ironSourceCallback);
            }

            if (success)
            {
                return;
            }
            else
            {
                UIManager.Instance.ShowWarningUI("failedWatchAdText");
            }
        }
    }
}