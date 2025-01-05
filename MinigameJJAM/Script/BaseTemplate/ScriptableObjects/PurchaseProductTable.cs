using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "PurchaseProductTable", menuName = "Scriptable Objects/IAP/PurchaseProductTable")]
    public class PurchaseProductTable : ScriptableObject
    {
        public List<PurchaseProduct> PurchaseProductList = new List<PurchaseProduct>();

        public PurchaseProduct GetPurchaseItem(string id) => PurchaseProductList.FirstOrDefault(item => item.ProductId == id);
    }

    public class PurchaseProduct : ScriptableObject
    {
        public string ProductId;
        public ProductType ProductType;
        public virtual void ExecuteAction() { }
    }
}
