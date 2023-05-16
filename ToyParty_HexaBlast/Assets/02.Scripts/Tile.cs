using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerEnterHandler ,IPointerDownHandler, IPointerUpHandler
{    
    public Location location;
    public GameObject blockObject;
    public Tile[] adjacentTile = new Tile[6];

    public void OnPointerEnter(PointerEventData eventData)
    {
        //선택된 타일이 없거나 타일 선택 후 동일 타일에 스와이프, 또는 블록 이동 처리중, 게임 종료 상태라면 리턴
        if (GameManager.Instance.selectTile == null || GameManager.Instance.enterOtherTile || GameManager.Instance.stageEnd) return;
        else if (GameManager.Instance.selectTile.Equals(this)) return;

        //블록 이동 처리 여부 체크
        GameManager.Instance.enterOtherTile = true;

        //연결된 타일들 중 처음 선택한 블록이 있는 타일의 정보와 스왑 처리 진행
        for (int i = 0; i < adjacentTile.Length; i++)
        {
            if (adjacentTile[i] == null) continue;

            if(GameManager.Instance.selectTile.Equals(adjacentTile[i]))
            {
                StartCoroutine(GameManager.Instance.BlockSwapProcess(this));
                break;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //게임 종료 중이면 리턴
        if (GameManager.Instance.stageEnd) return;
        
        if (!GameManager.Instance.enterOtherTile && GameManager.Instance.selectTile == null)
            GameManager.Instance.selectTile = this;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (GameManager.Instance.stageEnd) return;

        if (!GameManager.Instance.enterOtherTile)
            GameManager.Instance.selectTile = null;
    }

    ///<summary>
    ///해당 타일 블록의 이동 리스트에 이동 포지션을 대입하는 함수 
    ///</summary>
    public void AddMoveBlockPosition(Vector3 position)
    {        
        Block block = blockObject.GetComponent<Block>();
        block.movePositionList.Add(position);
    }
}
