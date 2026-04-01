using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShortcutBox : ItemBox
{
//    public void UseShortcut()
//    {
//        itemObject.UseItem();
//    }

//    public override void OnPointerClick(PointerEventData eventData)
//    {
//        if(UIManager.Instance.DragObject != null)
//            InputShortcut();
//    }

//    public override void OnDrop(PointerEventData eventData)
//    {            
//        InputShortcut();
//    }

//    public void InputShortcut()
//    {
//        //아이템 조합 창이 켜져있으면 == 조합 중이면 이동 못하게 막음
//        if (UIManager.Instance.GetUI("Inventory").GetComponent<Inventory>().Infos[1].activeSelf)
//        {
//            UIManager.Instance.DragObject.IsSelect = false;
//            return;
//        }

//        //1. 장비 칸에서 드래그 한 것인지, 인벤 칸에서 드래그 한 것인지 확인    
//        Item tempItem;
//        switch (UIManager.Instance.DragObject.ParentBox.ItemBoxType)
//        {
//            case BoxType.EQUIPMENTS:
//                //장비 칸에서 이동(해제 or 스왑)
//                if (itemObject.Item == null)
//                {
//                    //드롭 시 빈 칸이었으면 이동
//                    GameManager.Instance.Player.ShortcutItems[index] = UIManager.Instance.DragObject.Item;
//                    itemObject.Item = UIManager.Instance.DragObject.Item;
//                    ItemType key = ItemManager.Instance.GetMountingKey(UIManager.Instance.DragObject.Item.Info.ItemType);
//                    ItemManager.Instance.EquipmentsMounting[key](null, GameManager.Instance.Player);
//                    UIManager.Instance.DragObject.Item = null;
//                }
//                else
//                {
//                    //인벤 칸의 아이템이 장비 칸과 종류가 동일해서 스왑 가능하면 스왑, 아니면 취소
//                    if (itemObject.Item.Info.ItemType.Equals(UIManager.Instance.DragObject.Item.Info.ItemType))
//                    {
//                        tempItem = itemObject.Item; //드롭 받는 박스의 아이템 정보 임시 보관
//                        GameManager.Instance.Player.ShortcutItems[index] = UIManager.Instance.DragObject.Item; //플레이어 실제 소지 정보에서 현재 인덱스 위치에 드래그 한 객체의 아이템 정보 대입
//                        ItemType key = ItemManager.Instance.GetMountingKey(UIManager.Instance.DragObject.Item.Info.ItemType);
//                        ItemManager.Instance.EquipmentsMounting[key](tempItem, GameManager.Instance.Player);
//                        itemObject.Item = UIManager.Instance.DragObject.Item;
//                        UIManager.Instance.DragObject.Item = tempItem;
//                    }
//                }
//                break;
//            default:
//                //인벤 칸 or 단축키 칸 사이의 이동이니 단순 스왑
//                ItemBox startBox = UIManager.Instance.DragObject.ParentBox;
//                tempItem = itemObject.Item; //드롭 받는 박스의 아이템 정보 임시 보관

//                switch (UIManager.Instance.DragObject.ParentBox.ItemBoxType)
//                {
//                    case BoxType.BELONGING:
//                        GameManager.Instance.Player.ShortcutItems[index] = UIManager.Instance.DragObject.Item; //플레이어 실제 소지 정보에서 현재 인덱스 위치에 드래그 한 객체의 아이템 정보 대입
//                        GameManager.Instance.Player.Items.Belongings[startBox.Index] = tempItem; //플레이어 실제 소지 정보에서 드래그 한 인덱스 위치에 현재 아이템 정보 대입
//                        break;
//                    case BoxType.SHORTCUT:
//                        GameManager.Instance.Player.ShortcutItems[index] = UIManager.Instance.DragObject.Item; //플레이어 실제 소지 정보에서 현재 인덱스 위치에 드래그 한 객체의 아이템 정보 대입
//                        GameManager.Instance.Player.ShortcutItems[startBox.Index] = tempItem; //플레이어 실제 소지 정보에서 드래그 한 인덱스 위치에 현재 아이템 정보 대입
//                        break;
//                    default:
//                        break;
//                }

//                itemObject.Item = UIManager.Instance.DragObject.Item;
//                UIManager.Instance.DragObject.Item = tempItem;
//                break;
//        }

//        UIManager.Instance.DragObject.IsSelect = false;
//    }
}
