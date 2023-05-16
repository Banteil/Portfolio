using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    ///<summary>
    ///블록의 색상 정보
    ///</summary>
    public enum BlockColor
    {
        RED, BLUE, GREEN, YELLOW, PURPLE, ORANGE, NONE
    }

    ///<summary>
    ///블록의 타입 정보
    ///</summary>
    public enum BlockType
    {
        BASIC, SPECIAL
    }

    [SerializeField]
    BlockColor color;
    [SerializeField]
    BlockType type;
    public BlockColor blockColor
    {
        get { return color; }
        set
        {
            color = value;
            //블록 정보 대입 후 스프라이트 색상 변경
            SpriteRenderer blockSprite = GetComponent<SpriteRenderer>();
            switch (color)
            {
                case BlockColor.RED:
                    blockSprite.color = Color.red;
                    break;
                case BlockColor.BLUE:
                    blockSprite.color = Color.blue;
                    break;
                case BlockColor.GREEN:
                    blockSprite.color = Color.green;
                    break;
                case BlockColor.YELLOW:
                    blockSprite.color = Color.yellow;
                    break;
                case BlockColor.PURPLE:
                    blockSprite.color = new Color32(160, 32, 240, 255);
                    break;
                case BlockColor.ORANGE:
                    blockSprite.color = new Color32(246, 187, 67, 255);
                    break;
                default:
                    blockSprite.color = Color.black;
                    break;
            }
        }
    }
    public BlockType blockType
    {
        get { return type; }
        set
        {
            type = value;
            //타입에 따른 추가 상태 대입
            switch(type)
            {
                case BlockType.SPECIAL:
                    hp = 2;
                    break;
                default:
                    hp = 0;
                    break;
            }
        }
    }

    public List<Vector3> movePositionList = new List<Vector3>();
    public bool isMoving;
    public int hp;

    ///<summary>
    ///리스트에 저장된 좌표 순서대로 움직이게 하는 함수
    ///</summary>
    public IEnumerator MoveBlock()
    {
        isMoving = true;
        for(int i = 0; i < movePositionList.Count; i++)
        {
            yield return StartCoroutine(MoveBlockAnimation(movePositionList[i]));
        }

        movePositionList.Clear();
        isMoving = false;   
    }

    ///<summary>
    ///목표 좌표로 블록을 이동시키는 함수
    ///</summary>
    IEnumerator MoveBlockAnimation(Vector3 end)
    {
        Vector3 start = transform.position;
        float sqrRemainingDistance = (start - end).sqrMagnitude; //현재 블록 위치와 end 포지션 사이의 거리
        while (sqrRemainingDistance > float.Epsilon)
        {
            transform.position = Vector2.MoveTowards(transform.position, end, 10f * Time.deltaTime);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
    }
}
