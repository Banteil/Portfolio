using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public enum BoxType { BELONGING, EQUIPMENTS, SHORTCUT, COMBINATION }

public class ItemBox : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    protected BoxType _itemBoxType;
    public BoxType ItemBoxType { get { return _itemBoxType; } }

    protected int _index;
    public int Index { get { return _index; } set { _index = value; } }

    protected Image _boxImage;
    public Image BoxImage { get { return _boxImage; } }
    protected ItemObject _itemObject;
    public ItemObject ItemObject { get { return _itemObject; } set { _itemObject = value; } }

    Coroutine _itemInfoCheckRoutine;

    Inventory _inventory;
    ItemInfomation _itemInfo;
    WeaponInfoUI _weaponInfo;

    private void Awake()
    {
        _inventory = UIManager.Instance.GetUI("Inventory").GetComponent<Inventory>();
        _itemInfo = UIManager.Instance.GetUI("ItemInfoUI").GetComponent<ItemInfomation>();
        _weaponInfo = UIManager.Instance.GetUI("WeaponInfo").GetComponent<WeaponInfoUI>();
    }

    /// <summary>
    /// 박스 정보를 세팅, 초기화 할 때 사용하는 함수
    /// </summary>
    /// <param name="index"></param>
    /// <param name="type"></param>
    public void SettingBox(int index, BoxType type, List<Item> link)
    {
        _index = index;
        _itemBoxType = type;
        _boxImage = GetComponent<Image>();
        _itemObject = transform.GetChild(0).GetComponent<ItemObject>();
        _itemObject.ObjectSetting();
        if (link != null)
        {
            _itemObject.LinkedItem =
                () =>
                {
                    link[_index] = _itemObject.Item;
                    if (_itemObject.ParentBox.ItemBoxType.Equals(BoxType.EQUIPMENTS) && _index.Equals(EquipmentType.Weapon))
                        _weaponInfo.SetWeaponInfo(link[_index]);
                };
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_itemObject.Item != null)
        {
            if (_itemInfoCheckRoutine == null)
                _itemInfoCheckRoutine = StartCoroutine(ItemInfoCheckProcess());
        }
    }

    /// <summary>
    /// 아이템 위에 포인터를 1초 이상 둘 시 아이템 정보창을 표시하게 함
    /// </summary>
    /// <returns></returns>
    IEnumerator ItemInfoCheckProcess()
    {
        float timer = 0f;
        while (timer < 1f) { timer += Time.deltaTime; yield return null; }

        //아이템 정보 확인
        _itemInfo.SetItemInfo(_itemObject.Item, Input.mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_itemObject.Item != null)
            CloseItemInfo();
    }

    /// <summary>
    /// 재생 중인 아이템 정보 창 코루틴 종료 및 아이템 창을 닫을 때 사용되는 함수
    /// </summary>
    void CloseItemInfo()
    {
        if (_itemInfoCheckRoutine != null)
        {
            StopCoroutine(_itemInfoCheckRoutine);
            _itemInfoCheckRoutine = null;
            _itemInfo.gameObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{gameObject.name} 클릭!");
        if (eventData.button.Equals(PointerEventData.InputButton.Left)) //왼쪽 버튼 클릭
            LeftClickAction();
        else if (eventData.button == PointerEventData.InputButton.Right) //오른쪽 클릭 동작
            RightClickAction();
    }

    /// <summary>
    /// 왼쪽 마우스 버튼 클릭시 발동하는 액션
    /// </summary>
    void LeftClickAction()
    {
        //혹시 켜진 아이템 정보 UI가 있으면 끔
        CloseItemInfo();
        if (UIManager.Instance.DragObject == null) //드래그 오브젝트가 없을 때(첫 클릭)
        {
            if (_itemObject.Item == null) return;

            if (Input.GetKey(KeyCode.LeftControl)) //단축창으로 슝
                SetShortcut();
            else if (Input.GetKey(KeyCode.LeftAlt)) //낱개 선택
                StartItemMove(1);
            else //그냥 선택
                StartItemMove(_itemObject.Item.CurrentCount);
        }
        else //드래그 오브젝트가 있을 때(드롭 액션)
        {
            if (Input.GetKey(KeyCode.LeftAlt)) //낱개 더 받아오기
            {
                if (_inventory.isCombination || _itemObject.Item == null) return;
                if (_itemObject.Item.Info.ID.Equals(UIManager.Instance.DragObject.Item.Info.ID))
                {
                    if (UIManager.Instance.DragObject.Item.CurrentCount < UIManager.Instance.DragObject.Item.Info.MaxStackCount)
                    {
                        UIManager.Instance.DragObject.Item.CurrentCount++;
                        _itemObject.Item.CurrentCount--;
                        if (_itemObject.Item.CurrentCount <= 0)
                        {
                            _itemObject.Item = null;
                            UIManager.Instance.DragObject.IsIndividual = false;
                        }

                        _itemObject.LinkedItem?.Invoke();
                    }
                }
            }
            else //크게 3가지 동작
                ItemDrop();
        }
    }

    void ItemDrop()
    {
        //원래 본인이 있던 자리 클릭
        if (UIManager.Instance.DragObject.ParentBox.Equals(this))
        {
            if (!ItemManager.Instance.CheckOverlap(UIManager.Instance.DragObject, _itemObject))
            {
                _itemObject.Item = UIManager.Instance.DragObject.Item;
                _itemObject.LinkedItem?.Invoke();
            }
            UIManager.Instance.DragObject.IsSelect = false;
        }
        //장비 아이템 박스 관련 처리
        else if (_itemBoxType.Equals(BoxType.EQUIPMENTS))
        {
            if (!UIManager.Instance.DragObject.Item.ConversionItemType().Equals(ItemType.EQUIPMENT)) return;

            if (_index.Equals(UIManager.Instance.DragObject.Item.ConversionEquipmentType())) ItemSwap();
        }
        //그 외 아이템 박스 클릭 처리
        else
        {
            if (!ItemManager.Instance.CheckOverlap(UIManager.Instance.DragObject, _itemObject))
                ItemSwap();
        }

        if (_itemBoxType.Equals(BoxType.COMBINATION))
            CheckCombinationPossible();
    }

    void ItemSwap()
    {
        if (!UIManager.Instance.DragObject.IsIndividual) //낱개 선택 중이 아닐 때
        {
            ItemObject startObject = UIManager.Instance.DragObject.ParentBox.ItemObject;
            Item tempItem = _itemObject.Item;
            _itemObject.Item = UIManager.Instance.DragObject.Item;
            startObject.Item = tempItem;

            UIManager.Instance.DragObject.IsSelect = false;

            startObject.LinkedItem?.Invoke();
            _itemObject.LinkedItem?.Invoke();
        }
        else //낱개 선택 중일 때
        {
            Item tempItem = _itemObject.Item;
            _itemObject.Item = UIManager.Instance.DragObject.Item;
            UIManager.Instance.DragObject.Item = tempItem;

            if (UIManager.Instance.DragObject.Item == null)
                UIManager.Instance.DragObject.IsSelect = false;

            _itemObject.LinkedItem?.Invoke();
        }
    }

    /// <summary>
    /// 드래그 시작할 때 아이템 오브젝트를 마우스 이동할 수 있도록 처리하는 함수
    /// </summary>
    /// <param name="_count"></param>
    protected void StartItemMove(int _count)
    {
        if (_inventory.isCombination) _count = 1;
        bool isIndividual = false;
        _inventory.DropPanel.PanelImage.raycastTarget = true;

        ItemObject _individualObject = Instantiate(_itemObject.gameObject, _inventory.transform.parent, true).GetComponent<ItemObject>();
        _individualObject.ParentBox = this;
        Item _tempItem = new Item(_itemObject.Item);
        _tempItem.CurrentCount = _count;
        _individualObject.Item = _tempItem;

        _itemObject.Item.CurrentCount -= _count;
        if (_itemObject.Item.CurrentCount <= 0)
            _itemObject.Item = null;
        else
            isIndividual = true;

        UIManager.Instance.DragObject = _individualObject;
        UIManager.Instance.DragObject.IsIndividual = isIndividual;
        UIManager.Instance.DragObject.StartClickMove();
    }

    /// <summary>
    /// 인벤토리 내의 아이템을 곧바로 단축 아이템 창에 옮길 때 사용하는 함수
    /// </summary>
    void SetShortcut()
    {
        if (_itemBoxType.Equals(BoxType.SHORTCUT) || _inventory.isCombination) return;

        CloseItemInfo();
        if (!ItemManager.Instance.CheckOverlapList(_inventory.ShortcutBoxes, this))
        {
            for (int i = 0; i < _inventory.ShortcutBoxes.Count; i++)
            {
                if (_inventory.ShortcutBoxes[i].ItemObject.Item == null)
                {
                    _inventory.ShortcutBoxes[i].ItemObject.Item = _itemObject.Item;
                    _itemObject.Item = null;

                    _inventory.ShortcutBoxes[i].ItemObject.LinkedItem?.Invoke();
                    _itemObject.LinkedItem?.Invoke();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 오른쪽 마우스 버튼 클릭시 발동하는 액션
    /// </summary>
    protected void RightClickAction()
    {
        if (_itemObject.Item == null) return;

        CloseItemInfo();
        if (_itemBoxType.Equals(BoxType.COMBINATION))
        {
            for (int i = 0; i < _inventory.BelongingBoxes.Count; i++)
            {
                if (_inventory.BelongingBoxes[i].ItemObject.Item != null)
                {
                    if (_inventory.BelongingBoxes[i].ItemObject.Item.Info.ID.Equals(_itemObject.Item.Info.ID))
                    {
                        _inventory.BelongingBoxes[i].ItemObject.Count++;
                        break;
                    }
                }
            }
            _itemObject.Item = null;
        }
        else
            _itemObject.Item.UseItem();
    }

    void CheckCombinationPossible()
    {
        _inventory.CombinationItemImage.enabled = false;

        string combinationData;
        if (_inventory.CombinationBoxes[_index].ItemObject.Item != null)
            combinationData = _inventory.CombinationBoxes[_index].ItemObject.Item.Info.ID;
        else
            combinationData = "";

        _inventory.CombinationResultList = ItemManager.Instance.GetItemInfoSameCombination(_inventory.CombinationResultList, combinationData, _index);
        if (_inventory.CombinationResultList.Count.Equals(1))
        {
            string[] checkInfo = new string[9];
            for (int i = 0; i < checkInfo.Length; i++)
            {
                if (_inventory.CombinationBoxes[i].ItemObject.Item == null)
                    checkInfo[i] = "";
                else
                    checkInfo[i] = _inventory.CombinationBoxes[i].ItemObject.Item.Info.ID;
            }

            if (CheckSameInfo(_inventory.CombinationResultList[0].CombinationInfo, checkInfo))
            {
                _inventory.CombinationItemImage.sprite = _inventory.CombinationResultList[0].Sprite;
                _inventory.CombinationItemImage.enabled = true;
            }
        }
    }

    bool CheckSameInfo(string[] infoA, string[] infoB)
    {
        for (int i = 0; i < 9; i++)
        {
            if (!infoA[i].Equals(infoB[i]))
                return false;
        }
        return true;
    }
}
