using System;
using System.Collections.Generic;
using UnityEngine;


namespace Zeus
{
    // 루팅아이템 타입. 이펙트 ID와 동일.
    public enum TypeItem
    {
        COIN = 5001,
        WEAPON = 5002,
        CONSUME = 5003,
    }

    //[Serializable]
    //public class ItemTypeAmount
    //{
    //    public TypeItem ItemType;
    //    public int TableID;
    //    public int Amount;
    //}

    //[Serializable]
    //public struct LootItemAmount
    //{
    //    public TypeLootItem ItemType;
    //    public int TableID;
    //    public Vector2 Range;
    //}

    [Serializable]
    public class LootItemData
    {
        public TypeItem ItemType;
        public int ItemID;
        // 개별 확률?
        // 전체 확률?
        // 둘 다 필요함.
        //public float Probability;
        public Vector2Int ValueRange;
        public string LootObject;
    }

    //[Serializable]
    //public struct LootItemInfo_
    //{
    //    public TypeLootItem ItemType;
    //    public int TableID;
    //    public int Amount;
    //}

    [Serializable]
    public class LootTableData
    {
        public int TableID;
        public LootItemData[] Items;

        //    public LootItemInfo_[] GetItems_Each()
        //    {
        //        List<LootItemInfo_> list = new List<LootItemInfo_>();
        //        for (int i = 0; i < Items.Length; i++)
        //        {
        //            var item = Items[i];
        //            var probability = UnityEngine.Random.Range(0f, 100f);
        //            if (probability < item.Probability)
        //            {
        //                // 아이템 획득
        //                list.Add(new LootItemInfo_()
        //                {
        //                    ItemType = item.ItemType,
        //                    TableID = item.TableID,
        //                    Amount = UnityEngine.Random.Range(item.ValueRange.x, item.ValueRange.y),
        //                });
        //            }
        //        }
        //        return list.ToArray();
        //    }

        //    public LootItemInfo_[] GetItems_All()
        //    {
        //        List<LootItemInfo_> list = new List<LootItemInfo_>();
        //        var probability = UnityEngine.Random.Range(0f, 100f);
        //        var weight = 0f;
        //        for (int i = 0; i < Items.Length; i++)
        //        {
        //            var item = Items[i];
        //            weight += item.Probability;
        //        }
        //        return list.ToArray();
        //    }
    }

    //[Serializable]
    //public class LootTableData
    //{
    //    public int ID;
    //    public Vector2Int CoinRange;
    //    //public LootItemAmount[] ItemIDs;
    //    public string LootObject;
    //    public string InteractionAnim;
    //}

    [CreateAssetMenu(fileName = "LootTable", menuName = "Zeus/Table/LootTable")]
    public class LootTable : ScriptableObject
    {
        [SerializeField] private LootTableData[] _tableDatas;
        //[SerializeField] private LootItemData_[] _datas;

        internal LootTableData GetData(int tableID)
        {
            return Array.Find(_tableDatas, x => x.TableID == tableID);
        }

        //private void OnValidate()
        //{
        //    var items = _datas[0].GetItems();
        //    Debug.Log(items);
        //}
    }
}
