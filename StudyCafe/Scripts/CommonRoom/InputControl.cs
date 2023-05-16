using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CurrentScene { LOBBY, WAITING, CHAT, OPENSTUDY }

public class InputControl : Singleton<InputControl>
{
    int layerMask; //OBJECT, AVATAR 마우스 조작 시 움직임을 막기 위한 레이어 마스크

    public CurrentScene scene; //현재 씬
    public AvatarAct myAvatar; //내 아바타의 AvatarAct
    public AvatarAct selectedAvatar;
    public ExtraAct selectedExtra;

    public VoidFunction[] menu = new VoidFunction[9]; //1~9까지의 단축키 조작에 대응하는 델리게이트
    public VoidFunction enterKey; //엔터 키 입력에 대응하는 델리게이트
    public VoidFunction cancel; //ESC 키 입력에 대응하는 델리게이트
    public VoidFunction popupEnter, popupCancel; //CommonInteraction의 팝업 창 취소, 확인 버튼 조작에 대응하는 델리게이트
    /// <summary>
    /// 마우스 조작 유효성 여부를 판단하는 bool
    /// </summary>
    public bool isValid = true;
    /// <summary>
    /// ESC, Enter 외 단축키 조작 가능 여부를 판단하는 bool, true일 때 단축키 조작 불가
    /// </summary>
    public bool preventMenuOperation; 

    bool isKeyMove, isMouseMove;

    private void Start()
    {
        layerMask = (1 << LayerMask.NameToLayer("OBJECT")) + (1 << LayerMask.NameToLayer("AVATAR"));
    }

    void Update()
    {
        ScreenInteraction();
        Shortcut();
    }

    static long prevKeyInputTime = 0;
    /// <summary>
    /// 키 입력 쿨타임 체크용 함수(float으로 계산할 수 있게 수정 필요)
    /// </summary>
    bool EnableKeyInput()
    {
        long GAP = 1;

        long curTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        bool enable = (curTime - prevKeyInputTime >= GAP);

        if (enable) prevKeyInputTime = curTime;
        return enable;
    }

    /// <summary>
    /// 단축키 및 마우스 조작 유효성 정보를 초기화 하는 함수
    /// </summary>
    public void Initialization(bool valid)
    {
        isValid = valid;
        myAvatar = null;
        enterKey = null;
        cancel = null;
        popupEnter = null;
        popupCancel = null;
        ResetMenuInfo();
    }

    /// <summary>
    /// 씬 정보를 세팅하는 함수
    /// </summary>
    public void SceneSetting(int sceneNum) => scene = (CurrentScene)sceneNum;

    /// <summary>
    /// 마우스 조작 대응 함수
    /// </summary>
    void ScreenInteraction()
    {
        //마우스가 UI 위에 있는지 여부 체크
        if (EventSystem.current.IsPointerOverGameObject())
            CommonInteraction.Instance.isUIControl = true;
        else
            CommonInteraction.Instance.isUIControl = false;

        //조작 유효성 체크
        if (IsNotValid()) return;

        Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 클릭한 좌표를 가져 옴
        Ray2D ray = new Ray2D(wp, Vector2.zero); // 원점에서 터치한 좌표 방향으로 Ray를 쏨
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue, layerMask);

        //마우스 버튼 입력 유지 시 아바타 이동, 버튼 업 시 움직임 멈춤, 또는 기타 오브젝트 상호작용
        if (Input.GetMouseButton(0) && !isKeyMove)
        {
            if (myAvatar != null)
            {
                if (hit || CommonInteraction.Instance.isUIControl)
                    myAvatar.StopMove();
                else
                {
                    isMouseMove = true;
                    myAvatar.Move(wp);
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (myAvatar != null)
            {
                isMouseMove = false;
                myAvatar.StopMove();
            }

            if (!CommonInteraction.Instance.isUIControl) //UI등 터치 Ray에 걸리는 게 없을 때
            {
                if (hit = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue))
                {
                    if (hit.collider.tag.Equals("Player"))
                    {
                        hit.collider.GetComponent<AvatarAct>().SelectAvatar();
                    }
                    else if (hit.collider.tag.Equals("Extra"))
                    {
                        hit.collider.GetComponent<ExtraAct>().SelectExtra();
                    }
                    else if (hit.collider.tag.Equals("NPC"))
                    {
                        hit.collider.GetComponent<NPCAct>().Conversation();
                    }
                    else if (hit.collider.tag.Equals("Slide"))
                    {
                        SlideManager.Instance.OnSlideScreen();
                    }
                }
            }
        }
    }

    void Shortcut()
    {
        if (!isValid) return;

        //ESC 입력 처리
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (EnableKeyInput())
            {
                if (popupCancel != null)
                    popupCancel?.Invoke();
                else
                    cancel?.Invoke();
            }
        }

        //Enter키 입력 처리
        if (Application.platform.Equals(RuntimePlatform.WebGLPlayer))
        {
            if (DataManager.isWebInput.Equals(1) || Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
            {
                if (EnableKeyInput())
                {
                    if (popupEnter != null)
                        popupEnter?.Invoke();
                    else
                        enterKey?.Invoke();
                }

                DataManager.isWebInput = 0;
            }
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
            {
                if (EnableKeyInput())
                {
                    if (popupEnter != null)
                        popupEnter?.Invoke();
                    else
                        enterKey?.Invoke();
                }
            }
        }

        //아바타 키보드 조작 
        AvatarKeyMove();

        //inputField 객체 활성화 시 번호 및 글자 단축키 입력 패스
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            var script = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            if (script != null) return;
        }

        //메뉴 조작 방지 처리가 되어 있다면 리턴
        if (preventMenuOperation) return;

        //1~0까지 번호 단축키 처리
        for (int i = 0; i < menu.Length; i++)
        {
            int codeNum = 49 + i;
            if (Input.GetKeyUp((KeyCode)codeNum))
            {
                if (EnableKeyInput())
                    menu[i]?.Invoke();
            }
        }

        //특정 씬에만 있는 단축키 처리
        switch (scene)
        {
            case CurrentScene.LOBBY:
                if (Input.GetKeyUp(KeyCode.I))
                {
                    if (EnableKeyInput())
                        LobbyManager.Instance.MyInfoButton();
                }
                break;
            case CurrentScene.WAITING:

                break;
            case CurrentScene.CHAT:

                break;
            case CurrentScene.OPENSTUDY:

                break;
        }
    }

    void AvatarKeyMove()
    {
        if (isMouseMove) return;

        if (myAvatar != null)
        {
            isKeyMove = false;
            Vector2 wp = myAvatar.transform.position;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                isKeyMove = true;
                wp += new Vector2(0, 2);
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                isKeyMove = true;
                wp += new Vector2(0, -2);
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                isKeyMove = true;
                wp += new Vector2(2, 0);
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                isKeyMove = true;
                wp += new Vector2(-2, 0);
            }

            if (isKeyMove)
                myAvatar.Move(wp);
            else
                myAvatar.StopMove();
        }
    }

    public void ResetMenuInfo()
    {
        preventMenuOperation = false;
        for (int i = 0; i < menu.Length; i++)
        {
            menu[i] = null;
        }
    }

    bool IsNotValid()
    {
        bool check = false;

        if (myAvatar == null) check = true;
        if (Camera.main == null) check = true;
        if (!isValid) check = true;

        return check;
    }
}
