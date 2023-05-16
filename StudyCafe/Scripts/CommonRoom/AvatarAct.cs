using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 아바타의 동작 및 애니메이션, sprite 관리를 담당하는 클래스<br /> 
/// 조작하는 아바타 객체에 포함되어야 함<br /> 
/// </summary>
public class AvatarAct : MonoBehaviour
{
    public enum AvatarState { IDLE, MOVE, SIT }
    public AvatarState state;

    [Header("PartsConstant")]
    const int skin = 0;
    const int pants = 1;
    const int top = 2;
    const int hair = 3;    
    const int accessory = 4;

    [Header("DirectionConstant")]
    const int d = 0;
    const int l = 9;
    const int lu = 18;
    const int u = 27;
    const int r = 36;
    const int ru = 45;
    const int ds = 0;
    const int ls = 2;
    const int lus = 4;
    const int us = 6;
    const int rs = 8;
    const int rus = 10;

    [Header("AvatarUI")]
    public GameObject boxTail;
    public GameObject chatBox;
    public Text chatText;
    public Text nameText;

    [Header("EmoteUI")]
    public GameObject emoteBox;

    [Header("NamePlateUI")]
    public GameObject namePlate;
    public Text namePlateText;
    public GameObject boxInPlate;
    public Text boxInPlateText;

    [Header("AvatarMenuUI")]
    public GameObject menuCanvas;

    [Header("ETCObject")]
    public PhotonView pV;
    public Rigidbody2D rB;
    public BoxCollider2D physicsBox;
    public AudioSource avatarSound;
    [HideInInspector]
    public TableObject sitTable;

    [Header("AppearanceInformation")]
    //아바타의 현재 방향을 판단하는 변수
    int direction = 0, eyeDirection = 0, mouseDirection = 0;
    int spriteIndex = 1;
    float speed = 100f;
    //아바타의 현재 입 상태를 판단하는 변수
    int mouseState = 0;
    //아바타 눈 깜빡임 여부 판단 bool
    bool isBlink;
    //아바타 말하는 여부 판단 bool
    bool isTalk;
    //아바타 각 파츠 별 SpriteRenderer
    public SpriteRenderer[] parts;
    public SpriteRenderer eyeParts = new SpriteRenderer();
    public SpriteRenderer mouseParts = new SpriteRenderer();
    //아바타 각 파츠 별 Sprite
    //[파츠 배열][멀티플 스프라이트 배열]
    Sprite[][] partsSprites;
    Sprite[] mouseSprites, eyeSprites;
    Sprite[] bodySitSprites, topSitSprites, pantsSitSprites;

    //실행중인 코루틴을 조작하기 위한 변수
    Coroutine chatBoxRoutine, emoteRoutine;
    public bool isntStudyRoom; //스터디 룸에 있는지 여부 체크
    bool isFreeze; //채팅 동결 여부 체크
    bool isEquipmentAccessory = false;

    const float maxChatBoxSize = 0.02f;

    private void OnEnable()
    {
        CreateAvatarSetting();
    }

    public void CreateAvatarSetting()
    {
        //아바타 객체의 자식으로 있는 spriteRenderer 대입
        partsSprites = new Sprite[transform.GetChild(1).childCount - 2][];
        direction = d;
        eyeDirection = d;
        mouseDirection = d;

        string avatarInfo = "";
        //플레이어 객체 대입 및 이름 표시 텍스트에 닉네임 대입
        if (!isntStudyRoom)
        {
            nameText.text = pV.Owner.NickName;
            avatarInfo = (string)pV.Owner.CustomProperties["AvatarInfo"];

            DelegateSetting(true);
            string plateName = (string)pV.Owner.CustomProperties["PlateName"];
            if (plateName == null || plateName.Equals(""))
            {
                namePlateText.text = "이름";
                boxInPlateText.text = "이름";
            }
            else
            {
                namePlateText.text = plateName;
                boxInPlateText.text = plateName;
            }
            if (CommonInteraction.Instance.displayNamePlate)
                DisplayNamePlate(true);

            RoomManager.Instance.joinAvatarList.Add(gameObject);
        }
        else
        {
            gameObject.GetComponent<PhotonTransformView>().enabled = false;
            nameText.text = PhotonNetwork.LocalPlayer.NickName;
            avatarInfo = (string)PhotonNetwork.LocalPlayer.CustomProperties["AvatarInfo"];
            namePlateText.text = "이름";
            boxInPlateText.text = "이름";
        }

        //아바타 UI Canvas에 메인 카메라 대입
        transform.GetChild(2).GetComponent<Canvas>().worldCamera = Camera.main;
        //아바타 이미지 세팅
        SettingAvatarImage(avatarInfo);
        StartCoroutine(BlinkEye());
    }

    /// <summary>
    /// 아바타에 접근하는 기능을 구현하기 위해 각 룸 매니저의 delegate 함수에 AvatarAct 함수 추가 or 제거
    /// </summary>
    void DelegateSetting(bool isInput)
    {
        if (isntStudyRoom) return;

        if (isInput)
        {
            RoomManager.Instance.freezeFunc += SettingFreezeOption;
            RoomManager.Instance.displayNamePlateFunc += DisplayNamePlate;
        }
        else
        {
            RoomManager.Instance.freezeFunc -= SettingFreezeOption;
            RoomManager.Instance.displayNamePlateFunc -= DisplayNamePlate;
        }
    }

    #region 아바타 스프라이트 설정

    /// <summary>
    /// 플레이어가 CustomProperties에 들고 있는 아바타 정보, 성별 정보를 토대로 sprite를 세팅하는 함수
    /// </summary>
    public void SettingAvatarImage(string avatarInfo)
    {        
        string[] partsInfo = avatarInfo.Split(',');

        for (int i = 0; i < partsInfo.Length; i++)
        {
            string[] info = partsInfo[i].Split('_');
            string parts = info[0];
            int num = int.Parse(info[1]);

            mouseSprites = Resources.LoadAll<Sprite>("AvatarSprite/Mouse/mouse");
            mouseParts.sprite = mouseSprites[0];

            switch (parts)
            {
                case "h":
                    partsSprites[hair] = Resources.LoadAll<Sprite>("AvatarSprite/Hair/hair_" + num);
                    break;
                case "e":
                    eyeSprites = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + num);
                    eyeParts.sprite = eyeSprites[0];
                    break;
                case "t":
                    partsSprites[top] = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num);
                    topSitSprites = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num + "_sit");
                    break;
                case "p":
                    partsSprites[pants] = Resources.LoadAll<Sprite>("AvatarSprite/Pants/pants_" + num);
                    pantsSitSprites = Resources.LoadAll<Sprite>("AvatarSprite/Pants/pants_" + num + "_sit");
                    break;
                case "sk":
                    partsSprites[skin] = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num);
                    bodySitSprites = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num + "_sit");
                    break;
                case "a":
                    if (!num.Equals(0))
                    {
                        isEquipmentAccessory = true;
                        partsSprites[accessory] = Resources.LoadAll<Sprite>("AvatarSprite/Accessory/acc_" + num);
                    }
                    break;
            }
        }

        for (int i = 0; i < parts.Length; i++)
        {
            if (i.Equals(accessory) && !isEquipmentAccessory) continue;
            else if (partsSprites[i].Length.Equals(0)) continue;            

            parts[i].sprite = partsSprites[i][0];
        }    
    }

    #endregion

    #region 아바타 행동
    /// <summary>
    /// 넘겨받은 vector2 위치 정보값으로 아바타를 이동시키는 함수<br />
    /// 애니메이션 및 정지는 RPC로 실행, 실제 위치값 변경은 로컬로 진행하며 Photon Transform View로 위치값 동기화 
    /// </summary>
    public void Move(Vector2 wp)
    {        
        Vector2 direction = (wp - (Vector2)transform.position).normalized;

        //테이블에 앉아 있다면 일어나기 처리
        if (state.Equals(AvatarState.SIT))
        {            
            Ray2D ray = new Ray2D(wp, Vector2.zero); // 원점에서 터치한 좌표 방향으로 Ray를 쏨 
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue);
            if (hit.collider == null)
            {
                transform.position += (Vector3)direction * 1.7f;
                physicsBox.enabled = true;
                sitTable.isSit = false;
                sitTable.triggerBox.enabled = true;
                sitTable = null;
            }
            else return;
        }

        state = AvatarState.MOVE;
        Vector2 avatarVelocity = direction * (speed * Time.fixedDeltaTime);
        rB.velocity = avatarVelocity; 
        if (!isntStudyRoom)
            pV.RPC("AnimRPC", RpcTarget.All, wp);
        else
            AnimRPC(wp);
    }

    static float animTimer = 0f;
    /// <summary>
    /// 클릭한 위치의 각도, 방향을 구하여 해당 방향에 맞는 아바타 이동 애니메이션을 실행하는 함수(RPC)
    /// </summary>
    [PunRPC]
    void AnimRPC(Vector2 wp)
    {
        animTimer += Time.deltaTime;
        SetDirection(wp);
        //설정된 방향에 맞는 애니메이션 실행

        //눈, 입 제외 행동
        for (int i = 0; i < parts.Length; i++)
        {
            if (i.Equals(accessory) && !isEquipmentAccessory) continue;
            else if (partsSprites[i].Length.Equals(0)) continue;
            int index = direction + spriteIndex;
            parts[i].sprite = partsSprites[i][index];
        }

        //눈 행동
        if (eyeDirection >= 0)
        {
            if (!isBlink)
            {
                int eyeIndex = eyeDirection + spriteIndex;
                eyeParts.sprite = eyeSprites[eyeIndex];
            }
            else
            {
                int eyeIndex = eyeDirection + 9;
                eyeParts.sprite = eyeSprites[eyeIndex];
            }
        }
        else
            eyeParts.sprite = null;

        //입 행동(이동 시엔 미표시)
        mouseParts.sprite = null;

        if (animTimer >= 0.1f)
        {
            spriteIndex++;
            if (spriteIndex > 8)
                spriteIndex = 1;
            animTimer = 0f;
        }
    }

    void SetDirection(Vector2 wp)
    {
        //방향에 따른 각도를 구한 뒤, 해당 각도를 활용해 방향을 구함
        float ang = Mathf.Atan2(wp.y - transform.position.y, wp.x - transform.position.x) * 180 / Mathf.PI;
        if (ang < 0) ang += 360;

        if (ang >= 247.5f && ang < 292.5f)
        {
            direction = d;
            eyeDirection = 0;
            mouseDirection = 0;
        }
        else if (ang >= 157.5f && ang < 247.5f)
        {
            direction = l;
            eyeDirection = 10;
            mouseDirection = 7;
        }
        else if (ang >= 112.5f && ang < 157.5f)
        {
            direction = lu;
            eyeDirection = -1;
            mouseDirection = -1;
        }
        else if (ang >= 67.5f && ang < 112.5f)
        {
            direction = u;
            eyeDirection = -1;
            mouseDirection = -1;
        }
        else if (ang >= 22.5f && ang < 67.5f)
        {
            direction = ru;
            eyeDirection = -1;
            mouseDirection = -1;
        }
        else
        {
            direction = r;
            eyeDirection = 20;
            mouseDirection = 14;
        }
    }

    void SetDirection(int value)
    {
        switch(value)
        {
            case d:
                direction = d;
                eyeDirection = 0;
                mouseDirection = 0;
                break;
            case lu:
                direction = lu;
                eyeDirection = -1;
                mouseDirection = -1;
                break;
            case ru:
                direction = ru;
                eyeDirection = -1;
                mouseDirection = -1;
                break;
            case u:
                direction = u;
                eyeDirection = -1;
                mouseDirection = -1;
                break;
        }

        StopMoveRPC();
    }

    /// <summary>
    /// 이동 및 애니메이션을 멈추는 함수
    /// </summary>    
    public void StopMove()
    {
        if (!state.Equals(AvatarState.SIT))
            state = AvatarState.IDLE;

        rB.velocity = Vector2.zero;
        if (!isntStudyRoom)
            pV.RPC("StopMoveRPC", RpcTarget.All);
        else
            StopMoveRPC();
    }

    [PunRPC]
    void StopMoveRPC()
    {
        for (int i = 0; i < parts.Length; i++)
        {
            if (i.Equals(accessory) && !isEquipmentAccessory) continue;
            else if (partsSprites[i].Length.Equals(0)) continue;
            parts[i].sprite = partsSprites[i][direction];
        }

        //눈 행동
        if (eyeDirection >= 0)
        {
            if (!isBlink)
                eyeParts.sprite = eyeSprites[eyeDirection];
            else
            {
                int eyeIndex = eyeDirection + 9;
                eyeParts.sprite = eyeSprites[eyeIndex];
            }
        }
        else
            eyeParts.sprite = null;

        //입 행동(말하는 중일 때만 표시)
        if (isTalk)
        {
            if (mouseDirection >= 0)
            {
                int mouseIndex = mouseDirection + mouseState;
                mouseParts.sprite = mouseSprites[mouseIndex];
            }
            else
                mouseParts.sprite = null;
        }
        else
            mouseParts.sprite = null;

        spriteIndex = 1;
    }

    IEnumerator BlinkEye()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 6f));
            isBlink = true;

            yield return new WaitForSeconds(0.2f);
            isBlink = false;
        }
    }

    public void Sitting(TableObject.TableType tableType, Vector2 chairPos) => pV.RPC("SittingRPC", RpcTarget.All, tableType, chairPos.x, chairPos.y);

    [PunRPC]
    void SittingRPC(TableObject.TableType tableType, float posX, float posY)
    {
        state = AvatarState.SIT;
        physicsBox.enabled = false;
        rB.velocity = Vector2.zero;
        Vector2 chairPos = new Vector2(posX, posY);

        switch (tableType)
        {
            case TableObject.TableType.DOWN:
                SetDirection(d);
                parts[skin].sprite = bodySitSprites[ds];
                transform.position = (chairPos - new Vector2(0f, 0.35f));
                break;
            case TableObject.TableType.UP:
                SetDirection(u);
                transform.position = (chairPos - new Vector2(0f, 0.35f));
                break;
            case TableObject.TableType.LEFT:
                SetDirection(lu);
                transform.position = (chairPos - new Vector2(0f, 0.35f));
                break;
            case TableObject.TableType.RIGHT:
                SetDirection(ru);
                transform.position = (chairPos - new Vector2(0f, 0.35f));
                break;
        }
    }
    #endregion

    #region 아바타 클릭 메뉴
    /// <summary>
    /// 플레이어가 아바타를 클릭했을 때 실행되는 함수
    /// </summary>
    public void SelectAvatar()
    {
        //포톤 통신 룸이 아니거나, 자기가 컨트롤 하는 아바타일 경우 패스
        if (isntStudyRoom || pV.IsMine) return;

        if (InputControl.Instance.selectedAvatar != null)
        {
            if (!InputControl.Instance.selectedAvatar.Equals(this))
                InputControl.Instance.selectedAvatar.menuCanvas.SetActive(false);
        }

        if (!menuCanvas.activeSelf)
        {
            menuCanvas.SetActive(true);
        }
        else
        {
            if (menuCanvas.GetComponent<AvatarMenu>().moveEnd)
            {
                menuCanvas.SetActive(false);
            }
        }

        InputControl.Instance.selectedAvatar = this;
    }

    /// <summary>
    /// 아바타 컨트롤러 정보
    /// </summary>
    public void AvatarOwnerInfo()
    {

    }

    /// <summary>
    /// 선택한 아바타에게 귓속말을 할 수 있게 준비하는 함수(@이름 내용)
    /// </summary>
    public void PrivateChat()
    {
        RoomManager.Instance.chatInput.text = "@" + pV.Owner.NickName + " ";
        menuCanvas.SetActive(false);
    }
    #endregion

    #region 아바타 말풍선 채팅
    /// <summary>
    /// 외부에서 말풍선RPC 함수를 실행하기 위한 함수
    /// </summary>
    public void ChatBoxRPC(string chat, Player receiver)
    {
        string msg = AlignTextLines(chat);

        if (!isntStudyRoom)
        {
            if (receiver == null)
                pV.RPC("SetChatBox", RpcTarget.All, msg);
            else
                pV.RPC("SetChatBox", receiver, msg);
        }
        else
            SetChatBox(msg);
    }

    string AlignTextLines(string chat)
    {
        //매개변수 string chat을 일정 글자 수 마다 newline 처리
        string newLine = "";
        int max = chat.Length / 16;
        if (max.Equals(0)) newLine = chat;
        else
        {
            int count = 0;
            for (int i = 0; i < max; i++)
            {
                count++;
                string line = chat.Substring(16 * i, 16);
                if (!i.Equals(max - 1))
                    line += "\n";
                else if (chat.Substring(16 * count).Length > 0)
                {
                    line += "\n";
                    newLine += line + chat.Substring(16 * count);
                    count++;
                    break;
                }

                newLine += line;
            }
        }

        return newLine;
    }

    /// <summary>
    /// 말풍선 연출 RPC 함수
    /// </summary>
    [PunRPC]
    void SetChatBox(string chat)
    {
        if (isFreeze) return;

        //이모트 연출 중이면 연출 종료
        if(emoteRoutine != null)
        {
            StopCoroutine(emoteRoutine);
            emoteBox.transform.localScale = new Vector3(1f, 0f, 1f);
            emoteRoutine = null;
        }

        //말풍선 연출이 진행 중인지 아닌지를 판단 후 실행
        if (chatBoxRoutine == null)
            chatBoxRoutine = StartCoroutine(ChatBoxProduction(chat));
        else
        {
            StopCoroutine(chatBoxRoutine);
            chatBox.transform.localScale = new Vector3(maxChatBoxSize, 0f, 1f);
            nameText.gameObject.SetActive(false);
            chatBoxRoutine = StartCoroutine(ChatBoxProduction(chat));
        }
    }

    /// <summary>
    /// 말풍선 효과 + 표시 코루틴 함수
    /// </summary>
    IEnumerator ChatBoxProduction(string chat)
    {
        isTalk = true;
        StartCoroutine(Talking());
        Image chatBoxImg = chatBox.GetComponent<Image>();
        Image plateImg = boxInPlate.GetComponent<Image>();
        Image tailImg = boxTail.GetComponent<Image>();
        Color chatBoxColor = chatBoxImg.color;
        Color chatTextColor = chatText.color;
        Color plateColor = plateImg.color;
        Color plateTextColor = boxInPlateText.color;
        Color nameTextColor = nameText.color;
        Color tailColor = tailImg.color;
        chatBoxColor.a = 1;
        chatBoxImg.color = chatBoxColor;
        chatTextColor.a = 1;
        chatText.color = chatTextColor;
        plateColor.a = 1;
        plateImg.color = plateColor;
        plateTextColor.a = 1;
        boxInPlateText.color = plateTextColor;
        nameTextColor.a = 1;
        nameText.color = nameTextColor;
        tailColor.a = 1;
        tailImg.color = tailColor;

        //말풍선 텍스트 대입
        chatText.text = chat;
        //이름 표시 텍스트 활성화
        if(CommonInteraction.Instance.displayNamePlate)
        {
            boxInPlate.SetActive(true);
            namePlate.SetActive(false);
            nameText.gameObject.SetActive(false);
        }
        else
        {
            boxInPlate.SetActive(false);
            namePlate.SetActive(false);
            nameText.gameObject.SetActive(true);
        }            

        //말풍선 크기 증가
        while (chatBox.transform.localScale.y < maxChatBoxSize)
        {
            chatBox.transform.localScale += new Vector3(0f, 0.002f, 0f);
            boxTail.transform.localScale += new Vector3(0f, 0.002f, 0f);
            yield return null;
        }
        chatBox.transform.localScale = new Vector3(maxChatBoxSize, maxChatBoxSize, 1f);
        boxTail.transform.localScale = new Vector3(maxChatBoxSize, maxChatBoxSize, 1f);

        //채팅 길이 비례해서 일정 시간 대기
        int waitTime = (chat.Trim().Length / 10) + 2;
        yield return new WaitForSeconds(waitTime);
        if (CommonInteraction.Instance.isFreezeOption)
            yield break;

        //페이드 인 효과 진행
        isTalk = false;
        float timer = 0f;
        while (chatBoxColor.a > 0f)
        {
            timer += Time.deltaTime / 3f;
            chatBoxColor.a = Mathf.Lerp(1, 0, timer);
            chatTextColor.a = Mathf.Lerp(1, 0, timer);
            nameTextColor.a = Mathf.Lerp(1, 0, timer);
            plateColor.a = Mathf.Lerp(1, 0, timer);
            plateTextColor.a = Mathf.Lerp(1, 0, timer);
            tailColor.a = Mathf.Lerp(1, 0, timer);

            chatBoxImg.color = chatBoxColor;
            chatText.color = chatTextColor;
            nameText.color = nameTextColor;
            plateImg.color = plateColor;
            boxInPlateText.color = plateTextColor;
            tailImg.color = tailColor;
            yield return null;
        }

        //말풍선 효과 초기화
        chatBox.transform.localScale = new Vector3(maxChatBoxSize, 0f, 1f);
        boxTail.transform.localScale = new Vector3(maxChatBoxSize, 0f, 1f);
        if (CommonInteraction.Instance.displayNamePlate)
        {
            boxInPlate.SetActive(false);
            namePlate.SetActive(true);
            nameText.gameObject.SetActive(false);
        }
        else
        {
            boxInPlate.SetActive(false);
            namePlate.SetActive(false);
            nameText.gameObject.SetActive(true);
        }

        //말풍선 텍스트 및 루틴 정보 초기화
        chatText.text = "";
        chatBoxRoutine = null;
    }

    IEnumerator Talking()
    {
        while(isTalk)
        {
            mouseState++;
            if (mouseState > 6) mouseState = 0;
            yield return new WaitForSeconds(0.2f);
        }
        mouseState = 0;
    }

    /// <summary>
    /// 말풍선 고정 기능 함수
    /// </summary>
    public void FreezeChatBox()
    {
        InitializeAlphaValueChatBox();
        if (!isFreeze)
        {
            if (chatBoxRoutine != null)
                StopCoroutine(chatBoxRoutine);
            if (!CommonInteraction.Instance.displayNamePlate)
                nameText.gameObject.SetActive(true);
            boxTail.transform.localScale = new Vector3(maxChatBoxSize, maxChatBoxSize, 1f);
            chatBox.transform.localScale = new Vector3(maxChatBoxSize, maxChatBoxSize, 1f);
            isFreeze = true;
            chatBox.GetComponent<Outline>().enabled = true;
            chatBox.GetComponent<Outline>().effectColor = Color.red;            
            chatBoxRoutine = null;
        }
        else
        {
            isFreeze = false;
            if (chatBoxRoutine != null)
                StopCoroutine(chatBoxRoutine);
            nameText.gameObject.SetActive(false);
            boxTail.transform.localScale = new Vector3(maxChatBoxSize, 0f, 1f);
            chatBox.transform.localScale = new Vector3(maxChatBoxSize, 0f, 1f);
            if(CommonInteraction.Instance.isFreezeOption)
                chatBox.GetComponent<Outline>().effectColor = Color.blue;
            else
                chatBox.GetComponent<Outline>().enabled = false;
            if (CommonInteraction.Instance.displayNamePlate)
                namePlate.SetActive(true);
        }
    }

    /// <summary>
    /// 말풍선 얼리기 기능 함수
    /// </summary>
    public void SettingFreezeOption(bool freezeOption)
    {
        if (isFreeze) return;

        if (freezeOption)
        {
            if(chatBoxRoutine != null)
                StopCoroutine(chatBoxRoutine);
            if (!CommonInteraction.Instance.displayNamePlate)
                nameText.gameObject.SetActive(true);
            InitializeAlphaValueChatBox();            
            chatBox.GetComponent<Outline>().enabled = true;
            chatBox.GetComponent<Outline>().effectColor = Color.blue;            
            chatBoxRoutine = null;
        }
        else
        {
            if (chatBoxRoutine != null)
                StopCoroutine(chatBoxRoutine);
            nameText.gameObject.SetActive(false);
            boxTail.transform.localScale = new Vector3(maxChatBoxSize, 0f, 1f);
            chatBox.transform.localScale = new Vector3(maxChatBoxSize, 0f, 1f);
            chatBox.GetComponent<Outline>().enabled = false;
            chatBoxRoutine = null;
            if (CommonInteraction.Instance.displayNamePlate)
                namePlate.SetActive(true);
        }
    }

    /// <summary>
    /// 말풍선 연출 관련 이미지 알파값 초기화 함수
    /// </summary>
    void InitializeAlphaValueChatBox()
    {
        Image chatBoxImg = chatBox.GetComponent<Image>();
        Image tailImg = boxTail.GetComponent<Image>();
        Color chatBoxColor = chatBoxImg.color;
        chatBoxColor.a = 1;
        chatBoxImg.color = chatBoxColor;
        Color tailColor = tailImg.color;
        tailColor.a = 1;
        tailImg.color = tailColor;
        Image plateImg = boxInPlate.GetComponent<Image>();
        Color plateColor = plateImg.color;
        plateColor.a = 1;
        plateImg.color = plateColor;
        Color plateTextColor = boxInPlateText.color;
        plateTextColor.a = 1;
        boxInPlateText.color = plateTextColor;
        Color nameTextColor = nameText.color;
        nameTextColor.a = 1;
        nameText.color = nameTextColor;
        Color chatTextColor = chatText.color;
        chatTextColor.a = 1;
        chatText.color = chatTextColor;
    }
    #endregion

    #region 아바타 이모트 표시
    /// <summary>
    /// 이모트를 선택했을 때 실행되는 함수<br/>
    /// 오프라인 여부에 따라 RPC 실행 여부 나뉨
    /// </summary>
    public void StartDisplayEmote(int index)
    {
        if (!isntStudyRoom)
            pV.RPC("SetEmote", RpcTarget.All, index);
        else
            SetEmote(index);
        
    }

    /// <summary>
    /// 이모트 인덱스, 현재 씬 정보에 따라 이모트 표시 
    /// </summary>
    [PunRPC]
    public void SetEmote(int index)
    {
        emoteBox.transform.GetChild(0).GetComponent<Image>().sprite = RoomManager.Instance.emoteList.emoteSprites[index];

        //말풍선 연출중이면 중단
        if (chatBoxRoutine != null)
        {
            StopCoroutine(chatBoxRoutine);
            chatBox.transform.localScale = new Vector3(maxChatBoxSize, 0f, 1f);
            boxTail.transform.localScale = new Vector3(maxChatBoxSize, 0f, 1f);
            nameText.gameObject.SetActive(false);
            chatText.text = "";
            chatBoxRoutine = null;
        }

        if(emoteRoutine == null)
            emoteRoutine = StartCoroutine(DisplayEmoteProcess());
        else
        {
            StopCoroutine(emoteRoutine);
            emoteBox.transform.localScale = new Vector3(1f, 0f, 1f);
            emoteRoutine = StartCoroutine(DisplayEmoteProcess());
        }
    }

    /// <summary>
    /// 이모트 표시 연출
    /// </summary>
    IEnumerator DisplayEmoteProcess()
    {
        while (emoteBox.transform.localScale.y < 1f)
        {
            emoteBox.transform.localScale += new Vector3(0f, 0.1f, 0f);
            yield return null;
        }
        emoteBox.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(5f);

        //말풍선 크기 감소
        while (emoteBox.transform.localScale.y > 0f)
        {
            emoteBox.transform.localScale -= new Vector3(0f, 0.1f, 0f);
            yield return null;
        }
        emoteBox.transform.localScale = new Vector3(1f, 0f, 1f);

        emoteRoutine = null;
    }
    #endregion

    #region 아바타 명찰 표시
    /// <summary>
    /// 명찰 이름 동기화
    /// </summary>
    public void SetNamePlateText(string name) => pV.RPC("ChangeNamePlateText", RpcTarget.All, name);

    [PunRPC]
    public void ChangeNamePlateText(string name)
    {
        namePlateText.text = name;
        boxInPlateText.text = name;
    }

    /// <summary>
    /// 명찰 표시 함수
    /// </summary>
    public void DisplayNamePlate(bool value)
    {
        if (chatBoxRoutine == null)
            namePlate.SetActive(value);
        else
            boxInPlate.SetActive(value);

        nameText.gameObject.SetActive(!value);
    }

    #endregion

    #region 아바타 흑백 전환
    public void StartAvatarGrayscale() => gameObject.GetComponent<Grayscale>().StartGrayScale(1f);
    public void ResetAvatarGrayscale() => gameObject.GetComponent<Grayscale>().ResetGrayScale(1f);
    #endregion

    private void OnDisable()
    {
        if (isntStudyRoom) return;
        if (RoomManager.Instance == null) return;

        DelegateSetting(false);
        for (int i = 0; i < RoomManager.Instance.joinAvatarList.Count; i++)
        {
            if(RoomManager.Instance.joinAvatarList[i].Equals(gameObject))
            {
                RoomManager.Instance.joinAvatarList.RemoveAt(i);
                break;
            }
        }
    }
    
}