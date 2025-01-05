using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class RemoveAdsPopupUI : PopupUI
    {
        #region Cache
        private const string REMOVE_ADS_PRODUCT_ID = "jjam_remove_ads";

        private enum RemoveAdsButton
        {
            PurchaseButton,
            RestoreButton,
            BackButton,
        }
        #endregion

        protected override void BindInitialization()
        {
            base.BindInitialization();
            Bind<Button>(typeof(RemoveAdsButton));
            var backButton = GetButton((int)RemoveAdsButton.BackButton);
            backButton.gameObject.BindEvent(OnCloseButtonClicked);

            var purchaseButton = GetButton((int)RemoveAdsButton.PurchaseButton);
            purchaseButton.gameObject.BindEvent(RemoveAdsPurchase);

            var restoreButton = GetButton((int)RemoveAdsButton.RestoreButton);
            restoreButton.gameObject.BindEvent(RestorePurchase);
#if !UNITY_IOS
            restoreButton.gameObject.SetActive(false);
#endif
        }

        private void RemoveAdsPurchase(PointerEventData data)
        {
            Manager.IAP.BuyProduct(REMOVE_ADS_PRODUCT_ID, (success, error) =>
            {
                OnCloseButtonClicked(null);
                var message = Util.GetLocalizedString(Define.LOCALIZATION_TABLE_MESSAGE, success ? "purchaseSuccess" : "purchaseFail");
                if (!success)
                    message += $"\n({error})";
                Manager.UI.ShowMessage(message);
            });
        }

        private void RestorePurchase(PointerEventData data)
        {
            OnCloseButtonClicked(null);
            Manager.IAP.Restore((success, error) =>
            {
                var message = Util.GetLocalizedString(Define.LOCALIZATION_TABLE_MESSAGE, success ? "restoreSuccess" : "restoreFail");
                if (!success)
                    message += $"\n({error})";
                Manager.UI.ShowMessage(message);
            });
        }
    }
}
