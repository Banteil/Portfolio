using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "RemoveAdsProduct", menuName = "Scriptable Objects/IAP/RemoveAdsProduct")]
    public class RemoveAdsProduct : PurchaseProduct
    {
        public override void ExecuteAction() => Manager.User.ActiveRemoveAds();
    }
}
