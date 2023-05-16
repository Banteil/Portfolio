using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

///<summary>
///1인칭 시점에서 이동 방향을 나타내는 enum
///</summary>
public enum MoveWay
{
    FORWARD, RIGHT, BACK, LEFT
}

///<summary>
///1인칭 시점에서 회전 방향을 나타내는 enum
///</summary>
public enum RotateWay
{
    LEFT, RIGHT
}

///<summary>
///탐색 씬 내에서 플레이어의 이동, 상호작용 등 조작을 담당하는 클래스
///</summary>
public class PlayerMovement : MonoBehaviour
{
    const int up = 0;
    const int right = 1;
    const int down = 2;
    const int left = 3;
    //0,1,2,3으로 판단하는 방향을 직관적으로 표시하기위해 만든 const 변수

    public int currDirection; //현재 캐릭터가 정면으로 보는 방향을 나타내는 int 변수
    bool isMove = false; //현재 이동 중 여부를 체크하여 조작 입력을 받지 않게 하기 위한 bool
    bool isDoor = false; //정면에 문과 같은 통과 오브젝트가 있는지 여부를 체크하는 bool
    bool isWall = false;
    Vector2 touchBeganPositon; 
    Vector2 touchEndPosition;
    //스와이프 조작 여부 및 결과를 판단하기 위한 Vector2 변수
    RaycastHit hit; //레이캐스트에 충돌한 오브젝트를 판별하기 위한 hit 변수
    LayerMask objectLayer;
    LayerMask floorLayer;
    LayerMask wallLayer;
    LayerMask underLayer;
    //자주 쓰이는 레이어마스크를 미리 저장

    AudioClip[] footstep = new AudioClip[5];
    AudioClip[] breakWall = new AudioClip[3];
    AudioClip doorOpen;
    AudioClip doorClose;
    AudioClip doorLock;
    AudioClip doorSwitchOn;
    AudioSource playerSound;
    EventTrigger uiEvent;

    public int x, y; //캐릭터 현재 좌표
    public GameObject interactionUI;
    public RectTransform arrow;

    void Start()
    {
        ResourceLoad();        
        CheckMiniMapTile();
    }    

    void ResourceLoad()
    {
        objectLayer = 1 << LayerMask.NameToLayer("OBJECT");
        floorLayer = 1 << LayerMask.NameToLayer("FLOOR");
        wallLayer = 1 << LayerMask.NameToLayer("WALL");
        underLayer = LayerMask.NameToLayer("UNDERGROUND");

        playerSound = GetComponent<AudioSource>();
        uiEvent = interactionUI.GetComponent<EventTrigger>();

        for (int i = 0; i < 5; i++)
        {
            footstep[i] = Resources.Load("Sound/SFX/footstep_stage" + DataPassing.stageNum + "_" + (i + 1)) as AudioClip;
        }

        doorOpen = Resources.Load("Sound/SFX/dooropen_stage" + DataPassing.stageNum) as AudioClip;
        doorClose = Resources.Load("Sound/SFX/doorclose_stage" + DataPassing.stageNum) as AudioClip;
        doorLock = Resources.Load("Sound/SFX/doorlock_stage" + DataPassing.stageNum) as AudioClip;
        doorSwitchOn = Resources.Load("Sound/SFX/doorlockopen_stage" + DataPassing.stageNum) as AudioClip;
    }

    ///<summary>
    ///1인칭 화면 기준 앞으로 이동
    ///</summary>
    public void MoveForward()
    {
        if (isMove) return;

        StartCoroutine(Move(MoveWay.FORWARD));
    }

    ///<summary>
    ///1인칭 화면 기준 뒤로 이동
    ///</summary>
    public void MoveBack()
    {
        if (isMove) return;

        StartCoroutine(Move(MoveWay.BACK));
    }

    ///<summary>
    ///1인칭 화면 기준 왼쪽으로 이동
    ///</summary>
    public void MoveLeft()
    {
        if (isMove) return;

        StartCoroutine(Move(MoveWay.LEFT));
    }

    ///<summary>
    ///1인칭 화면 기준 오른쪽으로 이동
    ///</summary>
    public void MoveRight()
    {
        if (isMove) return;

        StartCoroutine(Move(MoveWay.RIGHT));
    }

    ///<summary>
    ///문 객체와 상호작용을 진행하는 함수
    ///</summary>
    public void OpenDoor()
    {
        if (isMove) return;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 3f, wallLayer))
        {
            if (hit.transform.CompareTag("DOOR"))
            {
                DoorAct door = hit.transform.gameObject.GetComponent<DoorAct>();
                if (door.direction.Equals((currDirection + 2) % 4)) door.doorLock = false;
                //문 뒤쪽에서 문을 열 경우 잠금 해제

                if (door.doorLock)
                {
                    StartCoroutine(TryMove(MoveWay.FORWARD));
                    playerSound.clip = doorLock;
                    playerSound.Play();
                    //문이 잠겨있을 경우 TryMove 실행
                }
                else
                {
                    hit.transform.gameObject.GetComponentInChildren<Animator>().SetTrigger("Open");
                    isDoor = true;
                    StartCoroutine(Move(MoveWay.FORWARD));
                    //문 잠금이 해제되어 있을 경우 문 열림 애니메이션 실행 후 이동 진행 
                }                
            }
        }        
    }

    ///<summary>
    ///스위치 객체와 상호작용을 진행하는 함수
    ///</summary>
    public void ActiveSwitch()
    {
        if (isMove) return;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 3f, objectLayer))
        {
            if (hit.transform.CompareTag("SWITCH"))
            {
                SwitchAct tempSwitch = hit.transform.gameObject.GetComponent<SwitchAct>();
                hit.transform.gameObject.GetComponent<Animator>().SetTrigger("Use");
                tempSwitch.active = true; //스위치 작동 완료
                tempSwitch.door.doorLock = false; //문 잠금 일시 해제 처리
                foreach (SwitchAct sa in tempSwitch.door.connectSwitchActList)
                {
                    if (!sa.active) //문이 보유하고 있는 스위치 리스트 중 작동이 되지 않은 스위치가 있다면
                    {
                        tempSwitch.door.doorLock = true; //문 잠금 처리 후 break
                        break;
                    }
                    playerSound.clip = doorSwitchOn;
                    playerSound.Play();
                }
            }
        }

        StartCoroutine(TryMove(MoveWay.FORWARD));
    }

    ///<summary>
    ///상자 객체와 상호작용을 진행하는 함수
    ///</summary>
    public void ChestOpen()
    {
        if (isMove) return;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 3f, objectLayer))
        {
            if (hit.transform.CompareTag("CHEST"))
            {
                hit.transform.gameObject.GetComponent<Animator>().SetTrigger("Open");
                hit.transform.GetComponent<ChestAct>().open = true;
                int temp = Random.Range(0, 100);
                if (temp < 60) hit.transform.GetComponent<ChestAct>().GetGem();
                else hit.transform.GetComponent<ChestAct>().GetMoney();
            }
        }

        StartCoroutine(TryMove(MoveWay.FORWARD));
    }

    ///<summary>
    ///벽을 부수는 행동이며, 벽의 Wall 클래스 내 attackWall 함수를 통해 데미지 전달
    ///</summary>
    public void AttackWall()
    {
        if (isMove) return;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 3f, wallLayer))
        {
            Debug.Log(PlayerState.Instance.WallAtk);
            if (hit.transform.gameObject.GetComponent<Wall>().attackWall(PlayerState.Instance.WallAtk))
            {
                //벽 파괴 이펙트 생성
                hit.transform.gameObject.SetActive(false);
                GameObject animObject = Instantiate(Resources.Load("DungeonAnimPrefabs/Anim_WallDestory") as GameObject);
                animObject.transform.parent = gameObject.transform;
                animObject.transform.localPosition = new Vector3(0, 0, 2f);
                animObject.transform.LookAt(Camera.main.transform);
                Destroy(animObject, 0.6f);

                string[] location = hit.transform.parent.name.Split(',');
                int hitY = int.Parse(location[0]);
                int hitX = int.Parse(location[1]);
                //벽이 묶여있는 실제 좌표를 구함

                if(hitY == y && hitX == x) DungeonManager.Instance.currTileInfo[y, x].wall[currDirection] = WallType.EMPTY; 
                //좌표가 같다면 현재 좌표 벽의 정보를 저장
                else
                {
                    int directionY = hitY - y;
                    int directionX = hitX - x;

                    switch(directionY)
                    {
                        case -1:
                            DungeonManager.Instance.currTileInfo[hitY, hitX].wall[down] = WallType.EMPTY;
                            break;
                        case 1:
                            DungeonManager.Instance.currTileInfo[hitY, hitX].wall[up] = WallType.EMPTY;
                            break;
                    }

                    switch (directionX)
                    {
                        case -1:
                            DungeonManager.Instance.currTileInfo[hitY, hitX].wall[right] = WallType.EMPTY;
                            break;
                        case 1:
                            DungeonManager.Instance.currTileInfo[hitY, hitX].wall[left] = WallType.EMPTY;
                            break;
                    }
                }
                //좌표가 다르다면 실제 벽이 속한 좌표를 기준으로 어느 방향의 벽을 부숴야할지 계산 후 정보 저장
            }

            PlayerState.Instance.SetStamina(-5);
            DungeonManager.Instance.MoveUIUpdate();
        }

        StartCoroutine(TryMove(MoveWay.FORWARD));

        CheckMiniMapTile();
    }

    ///<summary>
    ///다음 구역으로 넘어가는 함수
    ///</summary>
    public void NextArea()
    {
        if (isMove) return;

        TurnOffTextUI();
        playerSound.clip = footstep[Random.Range(0, 5)];
        playerSound.Play();
        StartCoroutine(DungeonManager.Instance.AreaMove(true));        
    }

    ///<summary>
    ///이전 구역으로 돌아가거나, 최초 구역일 경우 마을로 돌아가는지 여부를 물어보는 함수
    ///</summary>
    public void PrevArea()
    {
        if (isMove) return;

        if (DungeonManager.Instance.areaNum != 1)
        {
            TurnOffTextUI();
            playerSound.clip = footstep[Random.Range(0, 5)];
            playerSound.Play();
            StartCoroutine(DungeonManager.Instance.AreaMove(false));            
        } else
        {
            TurnOffTextUI();
            StartCoroutine(DungeonManager.Instance.AccountResult());
        }
    }

    ///<summary>
    ///스와이프 조작으로 캐릭터 회전 방향을 확인하기 위해 실행되는 함수<br/>
    ///SwipeZone UI 터치를 시작한 위치를 기억함
    ///</summary>
    public void RotationReady()
    {
        if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            touchBeganPositon = Input.mousePosition;
        }
        else if(Application.platform == RuntimePlatform.Android)
        {
            touchBeganPositon = Input.GetTouch(0).position;
        }

        arrow.gameObject.SetActive(true);
        arrow.position = touchBeganPositon;
    }

    ///<summary>
    ///스와이프 조작으로 캐릭터 회전을 실행하는 함수<br/>
    ///SwipeZone UI 터치를 시작한 위치와 종료한 위치를 비교하여 회전 방향을 결정함
    ///</summary>
    public void RotationStart()
    {
        if (isMove) return;

        arrow.gameObject.SetActive(false);

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            touchEndPosition = Input.mousePosition;
            Vector2 swipeDirection = (touchEndPosition - touchBeganPositon).normalized;

            if (swipeDirection.y >= 0.5f || swipeDirection.y <= -0.5f) return;
            //좌, 우로 스와이프했다기엔 애매한 조작일 경우 제외 

            if (swipeDirection.x < 0) StartCoroutine(Move(RotateWay.LEFT));
            else if(swipeDirection.x > 0) StartCoroutine(Move(RotateWay.RIGHT));
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            touchEndPosition = Input.GetTouch(0).position;
            Vector2 swipeDirection = (touchEndPosition - touchBeganPositon).normalized;

            if (swipeDirection.y >= 0.5f || swipeDirection.y <= -0.5f) return;

            if (swipeDirection.x < 0) StartCoroutine(Move(RotateWay.LEFT));
            else if (swipeDirection.x > 0) StartCoroutine(Move(RotateWay.RIGHT));
        }        
    }

    ///<summary>
    ///포션 사용 기능
    ///</summary>
    public void UsePotion()
    {
        if (isMove) return;

        if (PlayerState.Instance.CurrHp < PlayerState.Instance.MaxHp && PlayerState.Instance.GetPotion > 0)
        {
            PlayerState.Instance.SetHp(Expendables.UsePotion);
            PlayerState.Instance.GetPotion -= 1;
            DungeonManager.Instance.MoveUIUpdate();
            DungeonManager.Instance.ItemUIUpdate();
        }
    }

    ///<summary>
    ///음식 사용 기능
    ///</summary>
    public void UseFood()
    {
        if (isMove) return;

        if (PlayerState.Instance.CurrStamina < PlayerState.Instance.MaxStamina && PlayerState.Instance.GetFood > 0)
        {
            PlayerState.Instance.SetStamina(Expendables.UseFood);
            PlayerState.Instance.GetFood -= 1;
            DungeonManager.Instance.MoveUIUpdate();
            DungeonManager.Instance.ItemUIUpdate();
        }
    }

    ///<summary>
    ///폭탄 사용 기능
    ///</summary>
    public void UseBomb()
    {
        if (isMove) return;

        if (isWall && PlayerState.Instance.GetBomb > 0)
        {
            if (Physics.Raycast(transform.position, transform.forward, out hit, 3f, wallLayer))
            {
                if (hit.transform.gameObject.GetComponent<Wall>().attackWall(999))
                {
                    hit.transform.gameObject.SetActive(false);
                    GameObject animObject = Instantiate(Resources.Load("DungeonAnimPrefabs/Anim_WallDestory") as GameObject);
                    animObject.transform.parent = gameObject.transform;
                    animObject.transform.localPosition = new Vector3(0, 0, 2f);
                    animObject.transform.LookAt(Camera.main.transform);
                    Destroy(animObject, 0.6f);
                    string[] location = hit.transform.parent.name.Split(',');
                    int hitY = int.Parse(location[0]);
                    int hitX = int.Parse(location[1]);
                    //벽이 묶여있는 실제 좌표를 구함

                    if (hitY == y && hitX == x) DungeonManager.Instance.currTileInfo[y, x].wall[currDirection] = WallType.EMPTY;
                    //좌표가 같다면 현재 좌표 벽의 정보를 저장
                    else
                    {
                        int directionY = hitY - y;
                        int directionX = hitX - x;

                        switch (directionY)
                        {
                            case -1:
                                DungeonManager.Instance.currTileInfo[hitY, hitX].wall[down] = WallType.EMPTY;
                                break;
                            case 1:
                                DungeonManager.Instance.currTileInfo[hitY, hitX].wall[up] = WallType.EMPTY;
                                break;
                        }

                        switch (directionX)
                        {
                            case -1:
                                DungeonManager.Instance.currTileInfo[hitY, hitX].wall[right] = WallType.EMPTY;
                                break;
                            case 1:
                                DungeonManager.Instance.currTileInfo[hitY, hitX].wall[left] = WallType.EMPTY;
                                break;
                        }
                    }
                }

                PlayerState.Instance.GetBomb -= 1;
                DungeonManager.Instance.ItemUIUpdate();
                TurnOffTextUI();
                TurnOnTextUI();
            }            
        }
    }

    ///<summary>
    ///표시되는 Text UI(ex.문 열기 등)를 표시되게 하는 함수
    ///</summary>
    public void TurnOnTextUI()
    {
        EventTrigger.Entry interactionUI_Entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };

        if (Physics.Raycast(transform.position, transform.forward, out hit, 3f, wallLayer + objectLayer))
        {
            if (hit.transform.gameObject.CompareTag("NORMAL"))
            {
                isWall = true;
                interactionUI.GetComponentInChildren<Text>().text = "부수기\n(Touch)";                
                interactionUI_Entry.callback.AddListener((data) => { AttackWall(); });
                uiEvent.triggers.Add(interactionUI_Entry);
                interactionUI.gameObject.SetActive(true);
            }
            else if (hit.transform.gameObject.CompareTag("DOOR"))
            {
                interactionUI.GetComponentInChildren<Text>().text = "문 열기\n(Touch)";
                interactionUI_Entry.callback.AddListener((data) => { OpenDoor(); });
                uiEvent.triggers.Add(interactionUI_Entry);
                interactionUI.gameObject.SetActive(true);
            }
            else if (hit.transform.gameObject.CompareTag("SWITCH"))
            {
                if (!hit.transform.GetComponent<SwitchAct>().active)
                {
                    interactionUI.GetComponentInChildren<Text>().text = "작동\n(Touch)";
                    interactionUI_Entry.callback.AddListener((data) => { ActiveSwitch(); });
                    uiEvent.triggers.Add(interactionUI_Entry);
                    interactionUI.gameObject.SetActive(true);
                }
            }
            else if (hit.transform.gameObject.CompareTag("CHEST"))
            {
                if (!hit.transform.GetComponent<ChestAct>().open)
                {
                    interactionUI.GetComponentInChildren<Text>().text = "열기\n(Touch)";
                    interactionUI_Entry.callback.AddListener((data) => { ChestOpen(); });
                    uiEvent.triggers.Add(interactionUI_Entry);
                    interactionUI.gameObject.SetActive(true);
                }
            }
            else if (hit.transform.gameObject.CompareTag("EXIT"))
            {
                interactionUI.GetComponentInChildren<Text>().text = "다음 구역\n(Touch)";
                interactionUI_Entry.callback.AddListener((data) => { NextArea(); });
                uiEvent.triggers.Add(interactionUI_Entry);
                interactionUI.gameObject.SetActive(true);
            }
            else if (hit.transform.gameObject.CompareTag("ENTRANCE"))
            {
                interactionUI.GetComponentInChildren<Text>().text = "이전 구역\n(Touch)";
                interactionUI_Entry.callback.AddListener((data) => { PrevArea(); });
                uiEvent.triggers.Add(interactionUI_Entry);
                interactionUI.gameObject.SetActive(true);
            }
        }        
    }

    ///<summary>
    ///표시되는 Text UI(ex.문 열기 등)를 모두 표시되지 않게 하기 위한 함수
    ///</summary>
    public void TurnOffTextUI()
    {
        isWall = false;
        uiEvent.triggers.Clear();
        interactionUI.gameObject.SetActive(false);
    }

    ///<summary>
    ///진행하려는 방향이 길인지 아닌지 체크하는 함수
    ///</summary>
    bool NotRoadCheck(MoveWay moveWay)
    {
        bool notRoad = false;

        Vector3 moveDirection = Vector3.zero; 
        switch(moveWay)
        {
            case MoveWay.FORWARD:
                moveDirection = transform.forward;
                break;
            case MoveWay.BACK:
                moveDirection = -transform.forward;
                break;
            case MoveWay.LEFT:
                moveDirection = -transform.right;
                break;
            case MoveWay.RIGHT:
                moveDirection = transform.right;
                break;
        }

        if (Physics.Raycast(transform.position, moveDirection, 3f, wallLayer))
            notRoad = true;
        else if (Physics.Raycast(transform.position + (moveDirection * 4), -transform.up, out hit, 4f))
            if (hit.transform.gameObject.layer == underLayer) notRoad = true;

        return notRoad;
    }

    ///<summary>
    ///캐릭터의 이동 및 이동 애니메이션을 실행하는 코루틴 함수<br/>
    ///</summary>
    IEnumerator Move(MoveWay moveWay)
    {
        isMove = true; //코루틴 실행 시 isMove를 true 체크하여 추가 입력 방지

        if (!isDoor && NotRoadCheck(moveWay))
        {
            StartCoroutine(TryMove(moveWay));
            yield break;
        }

        TurnOffTextUI();//움직임을 시작했을 때 텍스트 UI를 모두 표시되지 않게 전환
        Vector3 end = Vector3.zero; //이동이 완료되는 포지션 = 다음 타일 중앙
        float sqrRemainingDistance; //현재 위치와 end 포지션 사이의 거리

        switch (moveWay)
        {
            case MoveWay.FORWARD:
                if (isDoor)
                {
                    playerSound.clip = doorOpen;
                    playerSound.Play();
                    yield return new WaitForSeconds(1f);
                }
                //문을 여는 상황일 경우 1초 대기
                end = transform.position + (transform.forward * 4f);
                break;
            case MoveWay.BACK:
                end = transform.position + (-transform.forward * 4f);
                break;
            case MoveWay.LEFT:
                end = transform.position + (-transform.right * 4f);
                break;
            case MoveWay.RIGHT:
                end = transform.position + (transform.right * 4f);
                break;
        } //MoveWay에 따라 목적지(end)설정

        playerSound.clip = footstep[Random.Range(0,5)];
        playerSound.Play(); //발소리 재생

        sqrRemainingDistance = (transform.position - end).sqrMagnitude; //현재 위치와 목적지 간 거리 구함
        while (sqrRemainingDistance > float.Epsilon) 
        {
            transform.position = Vector3.MoveTowards(transform.position, end, 10f * Time.deltaTime);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        } //현 위치와 목적지 간 거리가 엡실론보다 작아질 때 까지 이동

        if(isDoor)
        {
            playerSound.clip = doorClose;
            playerSound.Play();
            isDoor = false;
        }

        CheckUnderTile();
        CheckMiniMapTile();

        DungeonManager.Instance.MonstersMove();
        while (DungeonManager.Instance.IsMonsterMove() || DataPassing.isBattle) yield return null;

        TurnOnTextUI();
        DungeonManager.Instance.SetMoveInfo();        

        isMove = false; //코루틴 종료 시 isMove를 false 처리하여 조작 가능하게 변경
    }

    ///<summary>
    ///캐릭터의 회전 및 회전 애니메이션을 실행하는 코루틴 함수<br/>
    ///</summary>
    IEnumerator Move(RotateWay rotateWay)
    {
        isMove = true; //코루틴 실행 시 isMove를 true 체크하여 추가 입력 방지

        TurnOffTextUI();//움직임을 시작했을 땐 표시되는 텍스트 UI를 모두 표시되지 않게 전환      
        float timer = 0f; //회전을 천천히 진행하기 위한 단계 체크 변수

        switch (rotateWay)
        {            
            case RotateWay.LEFT:
                while (timer < 9)
                {
                    transform.rotation = transform.rotation * Quaternion.Euler(0f, -10f, 0f);
                    timer++;
                    yield return null;
                }
                currDirection--;
                if (currDirection < 0) currDirection = 3;
                break;
            case RotateWay.RIGHT:
                while (timer < 9)
                {
                    transform.rotation = transform.rotation * Quaternion.Euler(0f, 10f, 0f);
                    timer++;
                    yield return null;
                }
                currDirection++;
                if (currDirection > 3) currDirection = 0;
                break;
        }

        TurnOnTextUI();

        isMove = false; //코루틴 종료 시 isMove를 false 처리하여 조작 가능하게 변경
    }

    ///<summary>
    ///캐릭터가 이동하지 못하는 곳으로 이동하려 할 시 이동 실패 애니메이션을 실행하는 코루틴 함수<br/>
    ///</summary>
    IEnumerator TryMove(MoveWay moveWay)
    {
        isMove = true;

        TurnOffTextUI();//움직임을 시작했을 땐 표시되는 텍스트 UI를 모두 표시되지 않게 전환     
        Vector3 start = transform.position;
        Vector3 end = Vector3.zero; //이동이 완료되는 포지션 = 다음 타일 중앙
        float sqrRemainingDistance; //현재 위치와 end 포지션 사이의 거리

        switch (moveWay)
        {
            case MoveWay.FORWARD:
                end = start + (transform.forward);
                break;
            case MoveWay.BACK:
                end = start + (-transform.forward);
                break;
            case MoveWay.LEFT:
                end = start + (-transform.right); 
                break;
            case MoveWay.RIGHT:
                end = start + (transform.right);
                break;
        }

        sqrRemainingDistance = (start - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, end, 10f * Time.deltaTime);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }

        end = start;
        start = transform.position;
        sqrRemainingDistance = (start - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, end, 10f * Time.deltaTime);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }

        TurnOnTextUI();

        isMove = false; //코루틴 종료 시 isMove를 false 처리하여 조작 가능하게 변경
    }

    ///<summary>
    ///캐릭터 시야에 들어온 타일을 미니맵에 표시하기 위한 함수
    ///</summary>
    public void CheckMiniMapTile()
    {
        if (Physics.Raycast(transform.position, -transform.up, out hit, 3f, floorLayer))
        {
            if(!DungeonManager.Instance.currTileInfo[y, x].miniMapCheck)
            {
                DungeonManager.Instance.currTileInfo[y, x].miniMapCheck = true;
                hit.transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case up:
                    {
                        Vector3 checkPoint = transform.position + (Vector3.forward * 4);
                        if (Physics.Raycast(transform.position, Vector3.forward, out hit, 3f, wallLayer))
                            hit.transform.GetChild(0).gameObject.SetActive(true);
                        else if (y != 0 && Physics.Raycast(checkPoint, -Vector3.up, out hit, 3f, floorLayer))
                        {
                            if (!DungeonManager.Instance.currTileInfo[y - 1, x].miniMapCheck)
                            {
                                DungeonManager.Instance.currTileInfo[y - 1, x].miniMapCheck = true;
                                hit.transform.GetChild(0).gameObject.SetActive(true);
                                FourWayWallCheck(checkPoint);
                            }
                        }
                    }
                    break;
                case down:
                    {
                        Vector3 checkPoint = transform.position + (-Vector3.forward * 4);
                        if (Physics.Raycast(transform.position, -Vector3.forward, out hit, 3f, wallLayer))
                            hit.transform.GetChild(0).gameObject.SetActive(true);
                        else if (y != 29 && Physics.Raycast(checkPoint, -Vector3.up, out hit, 3f, floorLayer))
                        {
                            if (!DungeonManager.Instance.currTileInfo[y + 1, x].miniMapCheck)
                            {
                                DungeonManager.Instance.currTileInfo[y + 1, x].miniMapCheck = true;
                                hit.transform.GetChild(0).gameObject.SetActive(true);
                                FourWayWallCheck(checkPoint);
                            }
                        }
                    }
                    break;
                case left:
                    {
                        Vector3 checkPoint = transform.position + (-Vector3.right * 4);
                        if (Physics.Raycast(transform.position, -Vector3.right, out hit, 3f, wallLayer))
                            hit.transform.GetChild(0).gameObject.SetActive(true);
                        else if (x != 0 && Physics.Raycast(checkPoint, -Vector3.up, out hit, 3f, floorLayer))
                        {
                            if (!DungeonManager.Instance.currTileInfo[y, x - 1].miniMapCheck)
                            {
                                DungeonManager.Instance.currTileInfo[y, x - 1].miniMapCheck = true;
                                hit.transform.GetChild(0).gameObject.SetActive(true);
                                FourWayWallCheck(checkPoint);
                            }
                        }
                    }
                    break;
                case right:
                    {
                        Vector3 checkPoint = transform.position + (Vector3.right * 4);
                        if (Physics.Raycast(transform.position, Vector3.right, out hit, 3f, wallLayer))
                            hit.transform.GetChild(0).gameObject.SetActive(true);
                        else if (x != 29 && Physics.Raycast(checkPoint, -Vector3.up, out hit, 3f, floorLayer))
                        {
                            if (!DungeonManager.Instance.currTileInfo[y, x + 1].miniMapCheck)
                            {
                                DungeonManager.Instance.currTileInfo[y, x + 1].miniMapCheck = true;
                                hit.transform.GetChild(0).gameObject.SetActive(true);
                                FourWayWallCheck(checkPoint);
                            }
                        }
                    }
                    break;
            }
        }
    }

    ///<summary>
    ///상하좌우 4방향의 벽을 검사하여 벽이 가진 미니맵용 스프라이트를 활성화 시키는 함수
    ///</summary>
    void FourWayWallCheck(Vector3 checkPoint)
    {
        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case up:
                    if (Physics.Raycast(checkPoint, Vector3.forward, out hit, 3f, wallLayer))
                        hit.transform.GetChild(0).gameObject.SetActive(true);
                    break;
                case down:
                    if (Physics.Raycast(checkPoint, -Vector3.forward, out hit, 3f, wallLayer))
                        hit.transform.GetChild(0).gameObject.SetActive(true);
                    break;
                case left:
                    if (Physics.Raycast(checkPoint, -Vector3.right, out hit, 3f, wallLayer))
                        hit.transform.GetChild(0).gameObject.SetActive(true);
                    break;
                case right:
                    if (Physics.Raycast(checkPoint, Vector3.right, out hit, 3f, wallLayer))
                        hit.transform.GetChild(0).gameObject.SetActive(true);
                    break;
            }
        }
    }

    ///<summary>
    ///하단의 타일 정보를 체크하여 이동이 완료된 위치의 좌표 확인 및 이벤트를 발동시키기 위한 함수
    ///</summary>
    void CheckUnderTile()
    {
        if (Physics.Raycast(transform.position, -transform.up, out hit, 3f, floorLayer))
        {
            string floorHolder = hit.transform.parent.name;
            string[] location = floorHolder.Split(',');
            x = int.Parse(location[1]);
            y = int.Parse(location[0]);
            DungeonManager.Instance.x = x;
            DungeonManager.Instance.y = y;
        }
    }
}
