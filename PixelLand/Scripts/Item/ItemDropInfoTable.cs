using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new ItemDropInfoTable", menuName = "PixelLand/Data/ItemDropInfoTable", order = int.MinValue)]
public class ItemDropInfoTable : ScriptableObject
{
    [SerializeField]
    List<MyItemDropInfo> _infoTable;
    public List<MyItemDropInfo> InfoTable => _infoTable;
}
