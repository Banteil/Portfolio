using UnityEngine;
using System;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace starinc.io
{
    public class AppTrackingTransparency : MonoBehaviour
    {
#if UNITY_IOS
        public event Action SentTrackingAuthorizationRequest;
#endif

        public void Awake()
        {
#if UNITY_IOS
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
                ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                SentTrackingAuthorizationRequest?.Invoke();
            }
#endif
        }
    }
}