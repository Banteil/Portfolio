using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    const int up = 0;
    const int rightUp = 1;
    const int rightDown = 2;
    const int down = 3;
    const int leftDown = 4;
    const int leftUp = 5;

    int stageNum;
    bool blockPop;    
    DataManager dm;
    List<Tile> targetTileList;
    List<Tile> deleteTileList;
    List<Tile> CheckRequiredTileList;
    List<Tile> squareList;
    List<Block> moveList;
    int[] sameBlockCount;

    public bool enterOtherTile;
    public bool stageEnd;
    public Tile selectTile;

    void Awake()
    {
        //스테이지 번호 임시 변수
        stageNum = 21;
        moveList = new List<Block>();
        deleteTileList = new List<Tile>();
        targetTileList = new List<Tile>();
        CheckRequiredTileList = new List<Tile>();
        squareList = new List<Tile>();
        sameBlockCount = new int[3];
        for (int i = 0; i < sameBlockCount.Length; i++)
            sameBlockCount[i] = 0;
        selectTile = null;
        dm = GetComponent<DataManager>();

        StartStage();
    }

    void StartStage()
    {
        dm.SettingVariable(stageNum);
        dm.CreateStage();
    }

    ///<summary>
    ///블록 스왑 후 이벤트를 실행시키는 함수
    ///</summary>
    public IEnumerator BlockSwapProcess(Tile moveTile)
    {
        //선택한 블록과 이동 방향의 블록을 스왑
        GameObject tempBlock = moveTile.blockObject;
        moveTile.blockObject = selectTile.blockObject;
        selectTile.blockObject = tempBlock;
        CheckTargetTile();
        //이동 애니메이션을 위한 리스트에 추가
        AddMoveList(selectTile);
        AddMoveList(moveTile);

        //블록 이동 후 주변 타일을 체크하여 블록이 파괴되는지 여부를 체크
        //파괴되지 않는다면 다시 기존 정보로 스왑 및 이동 애니메이션 리스트 추가
        if (!BlockInteractionCheck(moveTile) && !BlockInteractionCheck(selectTile))
        {
            tempBlock = moveTile.blockObject;
            moveTile.blockObject = selectTile.blockObject;
            selectTile.blockObject = tempBlock;
            CheckTargetTile();
            AddMoveList(selectTile);
            AddMoveList(moveTile);
        }
        else
        {
            dm.moveCount--;
            if (dm.moveCount <= 0) stageEnd = true;
        }

        //실제 정보 이동 및 블록의 이동 경로 리스트 정보 대입이 완료되면 블록 이동 애니메이션 재생
        PlayAnimationOfBlocks();

        Block selectBlock = selectTile.blockObject.GetComponent<Block>();
        Block moveBlock = moveTile.blockObject.GetComponent<Block>();

        //블록 이동 애니메이션 중 대기
        yield return StartCoroutine(CheckAnimationPlaying(selectBlock, moveBlock));
        //삭제 리스트에 저장된 블록들의 삭제 처리 진행
        yield return StartCoroutine(DestroyBlocks());
        //블록 이동으로 인해 상호작용이 필요한 블록들의 처리
        yield return StartCoroutine(CheckRequiredTileProcess());

        //다른 타일 이동 여부, 선택 타일 정보를 초기화
        enterOtherTile = false;
        selectTile = null;        

        //UI정보 업데이트
        dm.UpdateUIInfo();
    }

    ///<summary>
    ///빈칸으로 이동 완료한 블록의 주변을 검사, 조건 만족 시 자동으로 연달아 파괴를 진행하도록 하는 함수
    ///</summary>
    IEnumerator CheckRequiredTileProcess()
    {
        bool checkComplete = false;
        int index = 0;

        //확인이 필요한 타일 개수가 하나라도 있으면 체크 진행
        while (CheckRequiredTileList.Count > 0)
        {
            //체크 중에 리스트의 숫자가 유동적으로 변할 수 있으므로 while로 검사 진행
            while (true)
            {
                //타일이 파괴되지 않는다면 다음 리스트를 검사할 수 있도록 index를 증가
                //만약 리스트의 끝까지 BlockInteractionCheck가 false 판정이 난다면 더 이상 확인이 필요하지 않으므로 break
                if (!BlockInteractionCheck(CheckRequiredTileList[index]))
                {
                    index++;
                    if (index.Equals(CheckRequiredTileList.Count))
                    {
                        checkComplete = true;
                        break;
                    }
                }
                else
                {
                    //애니메이션 재생 및 블록 파괴 진행
                    //블록이 파괴되었으므로 확인 필요 리스트에서도 삭제
                    PlayAnimationOfBlocks();

                    Block checkBlock = CheckRequiredTileList[index].blockObject.GetComponent<Block>();
                    yield return StartCoroutine(CheckAnimationPlaying(checkBlock));
                    yield return StartCoroutine(DestroyBlocks());
                    CheckRequiredTileList.RemoveAt(index);
                    index = 0;
                    break;
                }
            }

            //체크가 완료되었으면 리스트 초기화 후 while 탈출
            if (checkComplete)
            {
                CheckRequiredTileList.Clear();
                break;
            }
            yield return null;
        }
    }

    ///<summary>
    ///목표 오브젝트가 있는 타일을 체크용 리스트에 추가하는 함수
    ///</summary>
    void CheckTargetTile()
    {
        targetTileList.Clear();

        foreach(Tile tile in dm.tileList)
        {
            Block block = tile.blockObject.GetComponent<Block>();
            if (block.blockType.Equals(Block.BlockType.SPECIAL))
                targetTileList.Add(tile);
        }
    }

    ///<summary>
    ///블록 파괴 여부를 체크하는 함수
    ///</summary>
    bool BlockInteractionCheck(Tile mainTile)
    {
        //확인이 필요한 블록의 색상 저장
        Block mainBlock = mainTile.blockObject.GetComponent<Block>();
        Block.BlockColor checkColor = mainBlock.blockColor;
        if (checkColor.Equals(Block.BlockColor.NONE)) return false;

        CheckLinePattern(mainTile, checkColor);
        CheckSquarePattern(mainTile, checkColor);

        //블록이 파괴되는지 여부를 확인
        //파괴되지 않으면 삭제 리스트를 초기화 후 false 리턴
        //파괴되면 기존 블록까지 삭제 리스트에 추가 후 리턴
        if (!blockPop)
        {
            deleteTileList.Clear();
            return false;
        }
        else
        {
            if(!deleteTileList.Contains(mainTile))
                deleteTileList.Add(mainTile);
            blockPop = false;
            return true;
        }
    }

    ///<summary>
    ///일직선 라인 패턴 체크
    ///</summary>
    void CheckLinePattern(Tile mainTile, Block.BlockColor checkColor)
    {
        //이동된 블록이 있는 타일과 연결된 타일들을 검색 및 체크
        //위쪽 타일부터 시계방향으로 검색
        for (int i = 0; i < mainTile.adjacentTile.Length; i++)
        {
            //해당 방향에 연결된 타일이 없다면 패스
            if (mainTile.adjacentTile[i] == null) continue;

            //특정 방향에 동일한 색상이 있는지 여부를 재귀하며 검색
            SerchSameLine(i, checkColor, mainTile.adjacentTile[i]);
        }

        //세로 + 대각선 2방향 == 3방향 체크 실행
        for (int i = 0; i < sameBlockCount.Length; i++)
        {
            //해당 방향의 카운트가 2 이하 == 3매치가 되지 않았는지 여부 체크
            if(sameBlockCount[i] < 2)
            {
                //임시로 삭제 리스트에 넣어둔 블록을 리스트에서 삭제
                for (int j = 0; j < deleteTileList.Count; j++)
                {
                    if(deleteTileList[j].Equals(mainTile.adjacentTile[i]) || deleteTileList[j].Equals(mainTile.adjacentTile[i + 3]))
                    {
                        deleteTileList.RemoveAt(j);
                        break;
                    }
                }

                //해당 방향의 체크 카운트 초기화
                sameBlockCount[i] = 0;
                continue;
            }

            //해당 방향의 체크 카운트 초기화 및 블록 제거 실행 체크
            sameBlockCount[i] = 0;
            blockPop = true;
        }
    }

    ///<summary>
    ///이동한 블록과 동일한 색상인지 여부를 체크하는 함수(라인 패턴용)
    ///</summary>
    void SerchSameLine(int way, Block.BlockColor selectColor, Tile serchTile)
    {
        //체크하는 방향에 연결된 타일이 없거나 이미 삭제 리스트에 들어있다면 리턴
        if (serchTile == null || deleteTileList.Contains(serchTile)) return;

        //체크하는 블록 색상을 checkColor에 저장
        Block checkBlock = serchTile.blockObject.GetComponent<Block>();
        Block.BlockColor checkColor = checkBlock.blockColor;

        //이동한 블록과 체크하는 블록 색상이 같지 않으면 리턴
        //또는 체크하는 블록 색상이 NONE이라면 리턴
        if (!selectColor.Equals(checkColor)) return;
        else if (checkColor.Equals(Block.BlockColor.NONE)) return;

        //해당 방향의 동일 블록 카운트를 ++
        //동일 방향에 연결된 타일을 매개변수로 하여 재귀
        //일단 삭제 리스트에 추가
        sameBlockCount[way % 3]++;
        deleteTileList.Add(serchTile);

        SerchSameLine(way, selectColor, serchTile.adjacentTile[way]);
    }

    ///<summary>
    ///사각형 == 블록 모임 패턴 체크
    ///</summary>
    void CheckSquarePattern(Tile mainTile, Block.BlockColor mainColor)
    {
        squareList.Add(mainTile);

        for (int i = 0; i < mainTile.adjacentTile.Length; i++)
        {
            int wayCount = 0;
            if (mainTile.adjacentTile[i] == null || squareList.Contains(mainTile.adjacentTile[i])) continue;
            else
            {
                //체크하는 블록 색상을 checkColor에 저장
                Block checkBlock = mainTile.adjacentTile[i].blockObject.GetComponent<Block>();
                Block.BlockColor checkColor = checkBlock.blockColor;
                //이동하려는 블록과 메인 블록 색상이 같지 않으면 패스
                //또는 체크하는 블록 색상이 NONE이라면 패스
                if (!mainColor.Equals(checkColor)) continue;
                else if (checkColor.Equals(Block.BlockColor.NONE)) continue;

                SerchSquareTile(i, wayCount, mainColor, mainTile.adjacentTile[i]);
            }
        }

        Debug.Log("사각형 카운트 : " + squareList.Count);
        if(squareList.Count > 3)
        {
            foreach(Tile tile in squareList)
            {
                if(!deleteTileList.Contains(tile))
                    deleteTileList.Add(tile);
            }
            blockPop = true;            
        }
        Debug.Log("사각형 카운트 후 삭제 리스트 수 : " + deleteTileList.Count);

        squareList.Clear();
    }

    ///<summary>
    ///이동한 블록과 동일한 색상인지 여부를 체크하는 함수(블록 모임 패턴용)
    ///</summary>
    void SerchSquareTile(int way, int wayCount, Block.BlockColor selectColor, Tile serchTile)
    {
        //체크용 리스트에 없으면 Add
        if(!squareList.Contains(serchTile))
            squareList.Add(serchTile);

        int checkCount = 0;
        int listCount = 0;
        for (int i = 0; i < serchTile.adjacentTile.Length; i++)
        {
            if (serchTile.adjacentTile[i] == null) continue;
            else if(squareList.Contains(serchTile.adjacentTile[i]))
            {
                listCount++;
                continue;
            }
            else
            {
                //체크하는 블록 색상을 checkColor에 저장
                Block checkBlock = serchTile.adjacentTile[i].blockObject.GetComponent<Block>();
                Block.BlockColor checkColor = checkBlock.blockColor;
                //이동한 블록과 체크하는 블록 색상이 같지 않으면 패스
                //또는 체크하는 블록 색상이 NONE이라면 패스
                if (!selectColor.Equals(checkColor)) continue;
                else if (checkColor.Equals(Block.BlockColor.NONE)) continue;

                //체크 성공 시 카운트 ++
                checkCount++;

                //기존 진행방향과 같다면 wayCount++, 아니라면 초기화
                if (way.Equals(i))
                    wayCount++;
                else
                    wayCount = 0;

                //재귀를 통해 지속 탐색
                SerchSquareTile(i, wayCount, checkColor, serchTile.adjacentTile[i]);
            }
        }

        //기존 진행 방향과 다르고, 주변 어디로도 탐색을 못가는 상황에 리스트에 있는 주변 타일이 1개일 때
        //해당 타일은 파괴되지 말아야 하므로 현재 리스트에서 해당 타일 제거
        if (wayCount.Equals(0) && checkCount.Equals(0) && listCount.Equals(1))
        {
            squareList.RemoveAt(squareList.Count - 1);
        }
            
    }

    ///<summary>
    ///블록 이동 애니메이션을 위해 리스트에 움직일 블록을 추가, 블록에 이동 포지션을 전달하는 함수
    ///타일의 좌표 정보를 이용해 이동 포지션을 전달받고, 리스트에 추가함
    ///</summary>
    void AddMoveList(Tile tile)
    {
        tile.AddMoveBlockPosition(dm.GetTilePosition(tile));
        Block block = tile.blockObject.GetComponent<Block>();

        if (moveList.Contains(block)) return;
        moveList.Add(block);
    }

    ///<summary>
    ///이동 리스트에 저장된 블록들이 이동 경로에 따라 움직이게 하는 함수 
    ///</summary>
    void PlayAnimationOfBlocks()
    {
        for (int i = 0; i < moveList.Count; i++)
        {
            StartCoroutine(moveList[i].MoveBlock());
        }
        moveList.Clear();
    }

    ///<summary>
    ///이동중인 블록의 애니메이션이 끝났는지 여부를 지속 체크하여 홀드시키는 함수
    ///</summary>
    IEnumerator CheckAnimationPlaying(Block checkBlock)
    {
        //애니메이션이 끝나면 체크 종료
        while (true)
        {
            if (!checkBlock.isMoving)
                break;

            yield return null;
        }
    }

    ///<summary>
    ///이동중인 블록의 애니메이션이 끝났는지 여부를 지속 체크하여 홀드시키는 함수
    ///</summary>
    IEnumerator CheckAnimationPlaying(Block selectBlock, Block moveBlock)
    {
        //조작하여 이동중인 블록의 애니메이션이 끝났는지 여부를 지속 체크
        //애니메이션이 끝나면 체크 종료 후 삭제 실행
        while (true)
        {
            if (!selectBlock.isMoving && !moveBlock.isMoving)
                break;

            yield return null;
        }
    }

    ///<summary>
    ///삭제 리스트에 저장된 블록 객체를 삭제시킨 후 주변 특수 블록 이벤트 발동 및 빈칸을 채우는 함수
    ///</summary>
    IEnumerator DestroyBlocks()
    {
        Debug.Log("스페셜 블록 처리 전 삭제 리스트 수 : " + deleteTileList.Count);
        //블록 파괴에 반응하는 특수 블록에 대한 처리 진행
        SpecialBlockEvent();
        int createCount = deleteTileList.Count;
        Debug.Log("스페셜 블록 처리 후 삭제 리스트 수 : " + createCount);

        //삭제 리스트에 들어가있는 타일 블록의 삭제 처리
        //삭제 전 미리 빈칸으로 인식하게 하여 해당 자리를 채워넣음
        //정보 이전이 종료되면 삭제 리스트에서 해당 타일을 제거하고 따로 저장해둔 블록 객체를 Destroy
        while (true)
        {
            if (deleteTileList.Count.Equals(0)) break;

            GameObject delBlockObj = deleteTileList[0].blockObject;
            deleteTileList[0].blockObject = null;

            List<Tile> fillTileList = new List<Tile>();
            if (deleteTileList[0].adjacentTile[up] != null && deleteTileList[0].adjacentTile[up].blockObject != null)
                AddBlankFillingBlockList(fillTileList, deleteTileList[0].adjacentTile[up]);
            else if (deleteTileList[0].adjacentTile[leftUp] != null && deleteTileList[0].adjacentTile[leftUp].blockObject != null)
                AddBlankFillingBlockList(fillTileList, deleteTileList[0].adjacentTile[leftUp]);
            else if (deleteTileList[0].adjacentTile[rightUp] != null && deleteTileList[0].adjacentTile[rightUp].blockObject != null)
                AddBlankFillingBlockList(fillTileList, deleteTileList[0].adjacentTile[rightUp]);

            for (int i = 0; i < fillTileList.Count; i++)
            {
                BlankFilling(fillTileList[i]);                
            }

            deleteTileList.RemoveAt(0);
            Destroy(delBlockObj);
        }

        PlayAnimationOfBlocks();

        Debug.Log("삭제 및 빈칸 처리 후 삭제 수 " + deleteTileList.Count);
        Debug.Log("그 이전 삭제 수 " + createCount);

        //삭제 수만큼 블록 생성
        for (int i = 0; i < createCount; i++)
            yield return StartCoroutine(CreateBlockProcess());              

        //삭제 리스트 완전 초기화
        deleteTileList.Clear();
    }

    ///<summary>
    ///빈칸을 메꾸기 위한 블록들을 체크, 임시 리스트에 저장하는 함수
    ///</summary>
    void AddBlankFillingBlockList(List<Tile> fillTileList, Tile moveTile)
    {
        if (moveTile == null) return;

        List<int> wayList = new List<int>();
        if (moveTile.adjacentTile[up] != null)
            wayList.Add(up);
        if (moveTile.adjacentTile[leftUp] != null)
            wayList.Add(leftUp);
        if (moveTile.adjacentTile[rightUp] != null)
            wayList.Add(rightUp);

        //체크하는 타일이 삭제 리스트에 포함되어 있지 않으면 리스트에 추가
        if (!deleteTileList.Contains(moveTile))
            fillTileList.Add(moveTile);

        for (int i = 0; i < wayList.Count; i++)
        {
            if (moveTile.adjacentTile[wayList[i]].blockObject != null)
            {
                AddBlankFillingBlockList(fillTileList, moveTile.adjacentTile[wayList[i]]);
                break;
            }
        }
    }

    ///<summary>
    ///빈칸을 메꾸기 위한 블록 이동 함수
    ///마지막 칸에 도달하면 주변 타일 체크
    ///</summary>
    void BlankFilling(Tile moveTile)
    {
        List<int> wayList = new List<int>();
        if (moveTile.adjacentTile[down] != null)
            wayList.Add(down);
        if (moveTile.adjacentTile[leftDown] != null && IsFillWay(moveTile.location, leftDown))
            wayList.Add(leftDown);
        if (moveTile.adjacentTile[rightDown] != null && IsFillWay(moveTile.location, rightDown))
            wayList.Add(rightDown);

        for (int i = 0; i < wayList.Count; i++)
        {
            if (moveTile.adjacentTile[wayList[i]].blockObject == null)
            {
                moveTile.adjacentTile[wayList[i]].blockObject = moveTile.blockObject;
                moveTile.blockObject = null;
                AddMoveList(moveTile.adjacentTile[wayList[i]]);
                BlankFilling(moveTile.adjacentTile[wayList[i]]);
                break;
            }

            if(!CheckRequiredTileList.Contains(moveTile))
                CheckRequiredTileList.Add(moveTile);
        }
    }

    ///<summary>
    ///빈칸을 채우러 이동하는 블록의 현재 위치와 방향에 따라 이동 방향을 제한하는 bool 함수
    ///</summary>
    bool IsFillWay(Location location, int moveWay)
    {
        //중앙 row 좌표 대입
        int center = dm.maxRow / 2;

        //검토 방향이 좌하단인데 현재 위치가 중앙 row와 같거나 크면 이동 방지 
        //(단, row가 center와 같고 column이 0일때 == 정확히 정중앙일땐 이동 가능)
        //검토 방향이 우하단인데 현재 위치가 중앙 row보다 작다면 이동 방지
        switch (moveWay)
        {
            case leftDown:
                if (location.row >= center)
                {
                    if (location.row.Equals(center) && (location.column % 2).Equals(0))
                        return true;
                    else
                        return false;
                }
                break;
            case rightDown:
                if (location.row < center)
                    return false;
                break;
        }

        return true;
    }

    ///<summary>
    ///주변에서 블록이 파괴될 때 실행되는 특수 블록 이벤트 함수 
    ///</summary>
    void SpecialBlockEvent()
    {
        List<Tile> checkTargetList = new List<Tile>();
        for (int i = 0; i < deleteTileList.Count; i++)
        {
            for (int j = 0; j < deleteTileList[i].adjacentTile.Length; j++)
            {
                if (deleteTileList[i].adjacentTile[j] == null || checkTargetList.Contains(deleteTileList[i].adjacentTile[j]))
                    continue;

                Block checkBlock = deleteTileList[i].adjacentTile[j].blockObject.GetComponent<Block>();
                if (checkBlock.blockType.Equals(Block.BlockType.SPECIAL))
                    checkTargetList.Add(deleteTileList[i].adjacentTile[j]);
            }
        }

        for(int i = 0; i < checkTargetList.Count; i++)
        {
            for (int j = 0; j < targetTileList.Count; j++)
            {
                if(targetTileList[j].Equals(checkTargetList[i]))
                {
                    Block targetBlock = targetTileList[j].blockObject.GetComponent<Block>();
                    targetBlock.hp--;

                    if (targetBlock.hp > 0)
                    {
                        SpriteRenderer sprite = targetTileList[j].blockObject.GetComponent<SpriteRenderer>();
                        sprite.color = Color.white;
                        break;
                    }
                    else
                    {
                        deleteTileList.Add(targetTileList[j]);
                        targetTileList.RemoveAt(j);
                        break;
                    }
                }
            }
        }

        dm.targetCount = targetTileList.Count;
        if (dm.targetCount.Equals(0)) stageEnd = true;
    }

    ///<summary>
    ///블록 생성 처리를 진행하는 함수 
    ///</summary>
    IEnumerator CreateBlockProcess()
    {
        Tile createTile = dm.CreateBlock();
        BlankFilling(createTile);

        PlayAnimationOfBlocks();
        yield return new WaitForSeconds(0.2f);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
