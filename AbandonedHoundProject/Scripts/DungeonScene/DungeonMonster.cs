using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMonster : MonoBehaviour
{
    const int up = 0;
    const int right = 1;
    const int down = 2;
    const int left = 3;
    //0,1,2,3으로 판단하는 방향을 직관적으로 표시하기위해 만든 const 변수
    const int maxTileAry = 30;

    public int x, y; //현재 몬스터 좌표
    public int moveWay; //이동하려는 방향을 나타내는 int 변수
    public bool isMove; //이동 중인지 여부를 판단하는 bool

    bool isDead;
    LayerMask floorLayer;
    LayerMask wallLayer;
    PlayerMovement player;
    //자주 쓰이는 레이어마스크를 미리 저장

    void Start()
    {
        DungeonManager.Instance.AddMonsterToList(this); //던전 몬스터는 시작 시 스스로를 리스트에 삽입
        floorLayer = 1 << LayerMask.NameToLayer("FLOOR");
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
        moveWay = Random.Range(0, 4); //시작 시 이동하려는 방향을 랜덤으로 설정
        GetCurrentLocation(); //시작 시 본인 좌표 파악
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    void Update()
    {
        transform.GetChild(0).LookAt(Camera.main.transform);
        //몬스터 이미지가 항상 카메라를 보게 함
    }

    ///<summary>
    ///몬스터 이동을 처리하는 코루틴 함수
    ///</summary>
    public IEnumerator MonsterMove()
    {
        if (PlayerEncounterCheck())
        {
            yield break;
        }

        isMove = true;

        bool obstacleEncounter = ObstacleEncounter(); //진행하려는 방향에 장애물이 있는지 여부 파악
        if (obstacleEncounter) MoveWayReset(); //장애물이 있다면 없는 곳으로 랜덤하게 경로 변경

        Vector3 end = Vector3.zero; //이동이 완료되는 포지션 = 다음 타일 중앙
        float sqrRemainingDistance; //현재 위치와 end 포지션 사이의 거리

        switch (moveWay)
        {
            case up:
                end = transform.position + (transform.forward * 4f);
                break;
            case right:
                end = transform.position + (transform.right * 4f);
                break;
            case down:
                end = transform.position + (-transform.forward * 4f);
                break;
            case left:
                end = transform.position + (-transform.right * 4f);
                break;
            default:
                moveWay = Random.Range(0, 4);
                isMove = false;
                yield break;
                //사방이 막혀있는 곳에 갇힌 경우 이동하지 않고 종료
        }

        sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, end, 40f * Time.deltaTime);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }

        GetCurrentLocation();
        
        isMove = false;

        if (!PlayerEncounterCheck() && DataPassing.isBattle)
        {
            switch (moveWay)
            {
                case up:
                    end = transform.position + (-transform.forward * 4f);
                    break;
                case right:
                    end = transform.position + (-transform.right * 4f);
                    break;
                case down:
                    end = transform.position + (transform.forward * 4f);
                    break;
                case left:
                    end = transform.position + (transform.right * 4f);
                    break;                    
            }

            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            while (sqrRemainingDistance > float.Epsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, end, 40f * Time.deltaTime);
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                yield return null;
            }

            GetCurrentLocation();
        } //플레이어 이동 이후에 움직여서 인카운트 했으나, 이미 다른 몬스터와 전투중인 상황일 경우 
          //기존에 있던 장소로 되돌아가도록 처리
    }

    ///<summary>
    ///몬스터 이동 경로에 장애물(벽, 빈 공간, 맵 끝)이 있는지 여부를 확인하는 함수
    ///</summary>
    bool ObstacleEncounter()
    {
        switch(moveWay)
        {
            case up:
                if (y == 0) return true;
                else if (DungeonManager.Instance.currTileInfo[y - 1, x].tile == TileType.EMPTY || Physics.Raycast(transform.position, transform.forward, 3f, wallLayer))
                {
                    return true;
                }
                break;
            case down:
                if (y == maxTileAry - 1) return true;
                else if (DungeonManager.Instance.currTileInfo[y + 1, x].tile == TileType.EMPTY || Physics.Raycast(transform.position, -transform.forward, 3f, wallLayer))
                {
                    return true;
                }
                break;
            case left:
                if (x == 0) return true;
                else if (DungeonManager.Instance.currTileInfo[y, x - 1].tile == TileType.EMPTY || Physics.Raycast(transform.position, -transform.right, 3f, wallLayer))
                {
                    return true;
                }
                break;
            case right:
                if (x == maxTileAry - 1) return true;
                else if (DungeonManager.Instance.currTileInfo[y, x + 1].tile == TileType.EMPTY || Physics.Raycast(transform.position, transform.right, 3f, wallLayer))
                {
                    return true;
                }
                break;
        }

        return false;
    }

    ///<summary>
    ///매개변수로 받은 이동 경로에 장애물(벽, 빈 공간, 맵 끝)이 있는지 여부를 확인하는 함수
    ///</summary>
    bool ObstacleEncounter(int serchWay)
    {
        switch (serchWay)
        {
            case up:
                if (y == 0) return true;
                else if (DungeonManager.Instance.currTileInfo[y - 1, x].tile == TileType.EMPTY || Physics.Raycast(transform.position, transform.forward, 3f, wallLayer))
                {
                    return true;
                }
                break;
            case down:
                if (y == maxTileAry - 1) return true;
                else if (DungeonManager.Instance.currTileInfo[y + 1, x].tile == TileType.EMPTY || Physics.Raycast(transform.position, -transform.forward, 3f, wallLayer))
                {
                    return true;
                }
                break;
            case left:
                if (x == 0) return true;
                else if (DungeonManager.Instance.currTileInfo[y, x - 1].tile == TileType.EMPTY || Physics.Raycast(transform.position, -transform.right, 3f, wallLayer))
                {
                    return true;
                }
                break;
            case right:
                if (x == maxTileAry - 1) return true;
                else if (DungeonManager.Instance.currTileInfo[y, x + 1].tile == TileType.EMPTY || Physics.Raycast(transform.position, transform.right, 3f, wallLayer))
                {
                    return true;
                }
                break;
        }

        return false;
    }

    ///<summary>
    ///장애물이 없는 경로를 랜덤하게 설정하는 함수
    ///</summary>
    void MoveWayReset()
    {
        List<int> range = new List<int>();
        for(int i = 0; i < 4; i++)
        {
            if (!ObstacleEncounter(i)) range.Add(i);
        }

        if (range.Count == 0)
        {
            moveWay = 4;
            return;
        }
        int rand = Random.Range(0, range.Count);
        moveWay = range[rand];
    }

    ///<summary>
    ///현재 위치한 장소의 좌표를 받아내는 함수
    ///</summary>
    void GetCurrentLocation()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 3f, floorLayer))
        {
            string floorHolder = hit.transform.parent.name;
            string[] location = floorHolder.Split(',');
            x = int.Parse(location[1]);
            y = int.Parse(location[0]);
        }
    }

    ///<summary>
    ///플레이어와 인카운터 했는지 여부를 확인하여 전투를 실행하는 함수
    ///</summary>
    bool PlayerEncounterCheck()
    {
        if (DataPassing.isBattle) return false;
        //플레이어가 이미 전투중이라면 false 처리

        if(x == player.x && y == player.y)
        {            
            DungeonManager.Instance.BattleStart(true);
            DataPassing.tempDM = this;
            return true;
        } 
        //플레이어와 몬스터의 좌표가 동일(같은 타일에 있음)하다면 전투 시작
        //직후 데이터 패싱 클래스에 자신의 정보를 올려 던전 매니저가 전투를 실행한 몬스터를 리스트에서 제외 및 삭제하도록 진행

        return false;
    }
}
