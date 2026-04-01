using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new ItemDataTable", menuName = "PixelLand/Data/ItemTable", order = int.MinValue)]
public class ItemDataTable : ScriptableObject
{
    [SerializeField]
    private List<ItemData> _itemTable;
    public List<ItemData> ItemTable => _itemTable;


    [SerializeField]
    private List<ShopItemData> _shopItemTable;
    public List<ShopItemData> ShopItemTable => _shopItemTable;
}
