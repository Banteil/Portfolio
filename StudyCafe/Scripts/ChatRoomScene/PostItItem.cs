using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PostItItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    public PostItPanel panel;
    public Transform board;
    public Transform groupTr;

    RectTransform boardRectTr;
    RectTransform itemRectTr;

    public Color ItemColor
    {
        get { return GetComponent<Image>().color; }
        set
        {
            GetComponent<Image>().color = value;
        }
    }

    public string ExampleStr
    {
        get { return transform.GetChild(0).GetComponent<Text>().text; }
        set
        {
            transform.GetChild(0).GetComponent<Text>().text = value;
        }
    }

    public int index; //List에서 해당 객체를 인식하기 위한 index
    public bool isDrag; //현재 드래그 중인지 여부를 판단하기 위한 bool
    public bool isModification; //현재 수정 중인지 여부를 판단하기 위한 bool

    private void Start()
    {
        boardRectTr = board.GetComponent<RectTransform>();
        itemRectTr = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDrag) return;
        if (isModification)
        {
            CommonInteraction.Instance.InfoPanelUpdate("이미 해당 포스트잇 내용이 수정 중에 있습니다.");
            return;
        }

        panel.ModificationProcess(index);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDrag) return;
        if (isModification)
        {
            CommonInteraction.Instance.InfoPanelUpdate("현재 해당 포스트잇 내용이 수정 중에 있습니다.");
            return;
        }

        GetComponent<Image>().raycastTarget = false;
        panel.PostItDrag(index);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isModification) return;

        //마우스 위치와 동기화
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = pos;
        MoveLimit();
        
        //위치 싱크
        panel.PostItMove(index, transform.localPosition.x, transform.localPosition.y);
    }

    void MoveLimit()
    {
        Vector2 pos = transform.localPosition;
        //보드를 벗어나지 않게 처리
        float boardWidth = boardRectTr.rect.width / 2f;
        float boardHeight = boardRectTr.rect.height / 2f;
        float itemWidth = itemRectTr.rect.width / 2f;
        float itemHeight = itemRectTr.rect.height / 2f;
        if (pos.x <= -boardWidth + itemWidth) pos.x = -boardWidth + itemWidth;
        if (pos.x >= boardWidth - itemWidth) pos.x = boardWidth - itemWidth;
        if (pos.y <= -boardHeight + itemHeight) pos.y = -boardHeight + itemHeight;
        if (pos.y >= boardHeight - itemHeight) pos.y = boardHeight - itemHeight;
        transform.localPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<Image>().raycastTarget = true;
        panel.PostItDrag(index);        
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.gameObject.GetComponent<PostItItem>() != null)
        {
            if (groupTr == null)
                panel.CreateGroup(index, eventData.pointerDrag.gameObject.GetComponent<PostItItem>().index);
            else
                panel.AddGroup(groupTr.GetComponent<PostItGroup>().index, eventData.pointerDrag.gameObject.GetComponent<PostItItem>().index);
        }
    }

    public string GetItemInfo()
    {
        //인덱스_색깔R_색깔G_색깔B_내용_로컬x_로컬y_부모그룹인덱스(없으면Null)_현재수정중여부Bool
        string info = index + "_" + ItemColor.r + "_" + ItemColor.g + "_" + ItemColor.b + "_" + ExampleStr + "_" + transform.localPosition.x + "_" + transform.localPosition.y + "_";
        
        if (groupTr != null)
            info += groupTr.GetComponent<PostItGroup>().index + "_";
        else
            info += "null_";

        if (isModification)
            info += "true";
        else
            info += "false";

        return info;
    }

    private void OnDestroy()
    {
        panel.DeleteItem(index);
    }
}
