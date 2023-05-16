using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PostItGroup : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    public int index;
    public bool isDrag;
    public InputField groupNameField;
    [HideInInspector]
    public PostItPanel panel;
    BoxCollider2D box;
    RectTransform boardRectTr;
    RectTransform rectTr;

    private void Start()
    {
        if (!DataManager.isMaster)
            groupNameField.interactable = false;

        box = GetComponent<BoxCollider2D>();
        rectTr = GetComponent<RectTransform>();
        boardRectTr = transform.parent.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CheckGroupInItemModification())
            CommonInteraction.Instance.InfoPanelUpdate("그룹 내 포스트잇이 수정 중에 있습니다.");

        panel.GroupDrag(index);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (CheckGroupInItemModification()) return;

        //마우스 위치와 동기화
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = pos;
        MoveLimit();

        panel.GroupMove(index, transform.localPosition.x, transform.localPosition.y);
    }

    void MoveLimit()
    {
        Vector2 pos = transform.localPosition;
        //보드를 벗어나지 않게 처리
        float boardWidth = boardRectTr.rect.width / 2f;
        float boardHeight = boardRectTr.rect.height / 2f;
        float itemWidth = rectTr.rect.width / 2f;
        float itemHeight = rectTr.rect.height / 2f;
        if (pos.x <= -boardWidth + itemWidth) pos.x = -boardWidth + itemWidth;
        if (pos.x >= boardWidth - itemWidth) pos.x = boardWidth - itemWidth;
        if (pos.y <= -boardHeight + itemHeight) pos.y = -boardHeight + itemHeight;
        if (pos.y >= boardHeight - itemHeight) pos.y = boardHeight - itemHeight;
        transform.localPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData) => panel.GroupDrag(index);

    public void OnDrop(PointerEventData eventData)
    {
        if(eventData.pointerDrag.gameObject.GetComponent<PostItItem>() != null)
        {
            panel.AddGroup(index, eventData.pointerDrag.gameObject.GetComponent<PostItItem>().index);
        }
    }

    bool CheckGroupInItemModification()
    {
        for (int i = 1; i < transform.childCount; i++)
        {
            PostItItem item = transform.GetChild(i).GetComponent<PostItItem>();
            if (item.isModification)
                return true;
        }

        return false;
    }

    public void InvokeInfo() => Invoke("UpdateInfo", 0.05f);

    void UpdateInfo()
    {
        box.size = new Vector2(rectTr.rect.width, rectTr.rect.height);
        MoveLimit();
    }

    public void SettingGroupName() => panel.GroupNaming(index, groupNameField.text);

    public string GetGroupInfo()
    {
        //인덱스_그룹명_로컬x_로컬y
        string info = index + "_" + groupNameField.text + "_" + transform.localPosition.x + "_" + transform.localPosition.y + "_";

        return info;
    }

    private void OnDestroy()
    {
        panel.DeleteGroup(index);
    }
}
