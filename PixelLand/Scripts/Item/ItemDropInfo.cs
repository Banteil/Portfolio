using UnityEngine;

[System.Serializable]
public class ItemDropInfo
{
    [SerializeField]
    ItemData _itemData;
    public ItemData ItemData => _itemData;
    [SerializeField]
    float _percentage;
    public float Percentage { get { return _percentage; } set { _percentage = value; } }
}
