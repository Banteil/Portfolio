using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace starinc.io
{
    public class IAPManager : BaseManager, IDetailedStoreListener
    {
        #region Cache
        private IStoreController _storeController;
        private IExtensionProvider _extensionProvider;
        private PurchaseProductTable _purchaseProductTable;
        #endregion

        #region Callback
        private event Action<bool, string> _onPurchase;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            _purchaseProductTable = Resources.Load<PurchaseProductTable>("PurchaseItemTable");
            if (_storeController == null)
            {
                InitializePurchasing();
            }
        }

        private bool IsInitialized { get { return _storeController != null && _extensionProvider != null; } }

        public void InitializePurchasing()
        {
            if (IsInitialized) return;
            try
            {
                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                foreach (var product in _purchaseProductTable.PurchaseProductList)
                {
                    builder.AddProduct(product.ProductId, product.ProductType);
                }
                UnityPurchasing.Initialize(this, builder);
                Debug.Log("IAP Initialize Compelte!");
            }
            catch(Exception ex)
            {
                Debug.Log($"IAP Initialize Fail : {ex}");
            }            
        }

        public void BuyProduct(string productId, Action<bool, string> onPurchase)
        {
            if (IsInitialized)
            {
                var product = _storeController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    _onPurchase = onPurchase;
                    _storeController.InitiatePurchase(product);
                }
                else
                {
                    Debug.LogWarning($"Product {productId} is not available for purchase");
                    onPurchase?.Invoke(false, $"Product {productId} is not available for purchase");
                }
            }
            else
            {
                Debug.LogWarning("IAP not initialized");
                onPurchase?.Invoke(false, "IAP not initialized");
            }
        }

        public void Restore(Action<bool, string> onRestore)
        {
            if (_extensionProvider == null)
            {
                Debug.LogWarning("Extension provider is not initialized");
                onRestore?.Invoke(false, "IAP not initialized");
                return;
            }
#if UNITY_IOS
            var appleExtension = _extensionProvider.GetExtension<IAppleExtensions>();
            appleExtension.RestoreTransactions((success, error) =>
            {
                onRestore?.Invoke(success, error);
            });
#else
            Debug.LogWarning("Restore purchases is not supported on this platform");
            onRestore?.Invoke(false, "Restore purchases not supported on this platform");
#endif
        }

        #region IDetailedStoreListener Event
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensionProvider = extensions;
            Debug.Log("IAP initialized successfully");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"IAP initialization failed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"IAP initialization failed: {error}, Message: {message}");
        }

        void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogError($"Purchase failed: {product.definition.id}, Reason: {failureDescription.reason}");
            _onPurchase?.Invoke(false, failureDescription.reason.ToString());
            _onPurchase = null;
        }        

        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogError($"Purchase failed: {product.definition.id}, Reason: {failureReason}");
            _onPurchase?.Invoke(false, failureReason.ToString());
            _onPurchase = null;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var productId = purchaseEvent.purchasedProduct.definition.id;
            var product = _purchaseProductTable.GetPurchaseItem(productId);
            product?.ExecuteAction();
            _onPurchase?.Invoke(true, productId);
            _onPurchase = null;
            CallAPI.InsertPaymentInfo(Manager.User.SID, productId, purchaseEvent.purchasedProduct.receipt, Util.GetCurrentPlatform());
            return PurchaseProcessingResult.Complete;
        }
        #endregion
    }
}