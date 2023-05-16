using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Realtime;

/// <summary>
/// 룸(대기실, 교육장 등)의 기본적인 기능들을 모은 매니저
/// </summary>
public class RoomManager : Singleton<RoomManager>
{
    //룸 타입 번호
    const int solitaryRoom1 = 0;
    const int solitaryRoom2 = 1;
    const int solitaryRoom3 = 2;
    const int openStudyRoom = 3;

    [Header("BaseObjects")]
    public PhotonView pV; //룸 매니저의 포톤 뷰 변수
    public GameObject mainCamera; //메인 카메라
    public GameObject uiCanvas; //기본 UI 캔버스
    public Text roomInfoText; //스터디 룸 정보 텍스트
    public StudyPreparation studyPreparationPanel;

    [HideInInspector]
    public GameObject myAvatar; //자신의 아바타 객체
    [HideInInspector]
    public List<GameObject> joinAvatarList = new List<GameObject>(); //참가 아바타 리스트
    [HideInInspector]
    public ChatRoomManager chatRoom;
    [HideInInspector]
    public WaitingRoomManager waitingRoom;
    [HideInInspector]
    public bool isWaitingRoom;

    [Header("participantListObjects")]
    public GameObject participantListPanel; //참가자 리스트 판넬
    public Transform participantContent; //참가자 리스트 아이템의 부모 트랜스폼
    [HideInInspector]
    public List<ParticipantInfoItem> participantList = new List<ParticipantInfoItem>(); //참가자 리스트

    [Header("RoomObjects")]
    public Transform roomTr; //룸의 부모 트랜스폼
    [HideInInspector]
    public RoomObject roomObj; //룸 정보 스크립트
    public GameObject roomInfoPanel; //룸 정보 패널;

    [Header("ChatObjects")]
    public GameObject chatPanel; //채팅 판넬
    public Transform chatContent; //채팅 텍스트 오브젝트의 부모 트랜스폼
    public InputField chatInput; //채팅 입력 인풋 필드
    public EmoteListPanel emoteList; //이모트 리스트 오브젝트

    public Toggle toggleFreezeChatBox; //채팅 동결 토글 버튼
    public BoolenFunction freezeFunc; //아바타의 채팅 동결 함수 델리게이트
    bool isSizeChange; //채팅 판넬의 크기 조절 여부 체크용 bool
    bool isDrag; //채팅 판넬 위에서 드래그 조작 여부 체크용 bool

    [Header("TypeCanvas")]
    public GameObject masterCanvas; //교사 전용 UI 캔버스
    public GameObject grouperCanvas; //학생 전용 UI 캔버스    

    [Header("NamePlateObjects")]
    public Toggle toggleDisplayNamePlate; //명찰 토글 버튼
    public InputField namePlateInputField; //명찰 이름 입력 필드
    public BoolenFunction displayNamePlateFunc; //아바타 이름 표시 함수 델리게이트

    [Header("ResourceObjects")]
    GameObject chatTextObj, participantInfoItemObj, textBookItemObj;

    #region 룸 정보 세팅
    void UseResourcesLoad()
    {
        chatTextObj = Resources.Load<GameObject>("Prefabs/ChatText");
        participantInfoItemObj = Resources.Load<GameObject>("Prefabs/UI/ParticipantInfoItem");
        textBookItemObj = Resources.Load<GameObject>("Prefabs/UI/DownloadTextBookItem");        
    }

    void Start()
    {
        Resources.UnloadUnusedAssets(); //미사용 리소스 메모리 정리
        UseResourcesLoad();
        pV = GetComponent<PhotonView>(); //포톤 뷰 대입
        InputControl.Instance.SceneSetting(SceneManager.GetActiveScene().buildIndex);
        InputControl.Instance.isValid = true;
        InputControl.Instance.enterKey = Send;
        InputControl.Instance.cancel = LeaveRoomButton;

        //씬 이동 시 문제 발생 방지를 위해 false로 변경했던 메세지 큐 러닝 true로 변경
        PhotonNetwork.IsMessageQueueRunning = true;
        //마스터 클라이언트와 씬을 동기화 하기 위한 설정
        //마스터가 교육장으로 이동했다면 늦게 접속한 사람도 곧바로 이동
        PhotonNetwork.AutomaticallySyncScene = true;

        string myNum = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        string masterNum = (string)PhotonNetwork.CurrentRoom.CustomProperties["Teacher"];
        if (myNum.Equals(masterNum))
        {
            masterCanvas.SetActive(true);
            DataManager.isMaster = true;
            DataManager.loadData.code = PhotonNetwork.CurrentRoom.Name;
            PhotonNetwork.CurrentRoom.SetMasterClient(PhotonNetwork.LocalPlayer);
        }
        else
        {
            grouperCanvas.SetActive(true);
            DataManager.isMaster = false;
        }

        //현재 스터디가 시작된 상태인가 체크
        //아닐 경우 룸 정보 세팅, 맞을 경우 대기실 여부 파악하여 교육장으로 보냄
        if(isWaitingRoom)
        {
            bool isReady = (bool)PhotonNetwork.CurrentRoom.CustomProperties["IsReady"];
            if (!isReady)
                RoomInfoSetting();
        }
        else
            RoomInfoSetting();
    }

    /// <summary>
    /// 룸의 초기 필요 정보 세팅
    /// </summary>
    void RoomInfoSetting()
    {
        //채팅 정보 초기화 및 세팅
        RandomSettingNameColor();
        chatInput.text = "";
        RoomRenewal();
        //채팅 창 크기 조절
        RectTransform rect = chatPanel.GetComponent<RectTransform>();
        float panelHeight = (uiCanvas.GetComponent<RectTransform>().rect.height / 4) * 3;
        rect.offsetMax = new Vector2(rect.offsetMax.x, -panelHeight);

        DataManager.interactionData.curriculumDate = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurriculumDate"];
        //타입에 따른 룸 생성 및 정보 세팅
        SettingRoom(DataManager.Instance.currentStudyData.studyRoomType);

        studyPreparationPanel.studyData = DataManager.Instance.currentStudyData;
        studyPreparationPanel.isStudyRoom = true;

        //아바타 생성(동기화)
        myAvatar = PhotonNetwork.Instantiate("Prefabs/Avatar", Vector3.zero, Quaternion.identity);
        myAvatar.GetComponent<AvatarAct>().CreateAvatarSetting();
        if (isWaitingRoom)
            myAvatar.transform.position = roomObj.spawnPos.position;
        else
        {
            if (DataManager.isMaster)
                myAvatar.transform.position = new Vector3(0f, -2f);
            else
                roomObj.myTable.SitProcess(myAvatar.GetComponent<AvatarAct>());
        }        
        Camera.main.GetComponent<CameraController>().cameraTarget = myAvatar.transform;        
        InputControl.Instance.myAvatar = myAvatar.GetComponent<AvatarAct>();

        //이모티콘 세팅
        emoteList.SettingEmoteList();

        //명찰 표시가 필요하다면 토글 실행
        toggleDisplayNamePlate.isOn = CommonInteraction.Instance.displayNamePlate;
        
        //참가자 리스트 초기화
        SetParticipantList();
        
        //업데이트문 시작
        StartCoroutine(StudyUpdate());
    }

    /// <summary>
    /// 이름 색깔 랜덤 부여 함수
    /// </summary>
    void RandomSettingNameColor()
    {
        string[] code = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
        string nameColor = "#";

        for (int i = 0; i < 6; i++)
        {
            nameColor += code[Random.Range(0, code.Length)];
        }
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
        if (hash["NameColor"] == null)
            hash.Add("NameColor", nameColor);
        else
            hash["NameColor"] = nameColor;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    void SettingRoom(int type)
    {
        GameObject room;
        //전달받은 타입에 따라 룸 생성 및 설정 스크립트 체크
        switch (type)
        {
            case solitaryRoom1:
                room = Instantiate(Resources.Load<GameObject>("Room/SolitaryRoom_1"), roomTr);
                break;
            case solitaryRoom2:
                room = Instantiate(Resources.Load<GameObject>("Room/SolitaryRoom_2"), roomTr);
                break;
            case solitaryRoom3:
                room = Instantiate(Resources.Load<GameObject>("Room/SolitaryRoom_3"), roomTr);
                break;
            case openStudyRoom:
                room = Instantiate(Resources.Load<GameObject>("Room/OpenStudyRoom"), roomTr);
                break;
            default:
                roomObj = null;
                room = null;
                break;
        }

        if (room == null) return;
        roomObj = room.GetComponent<RoomObject>();
        roomObj.slideCanvas.worldCamera = Camera.main;

        //카메라 줌 인, 아웃 조작을 위한 룸 최대 크기 세팅
        float height = roomObj.topPos.position.y - roomObj.bottomPos.position.y;
        Camera.main.GetComponent<CameraController>().SetHeight(height);
        Camera.main.GetComponent<CameraController>().minBound = roomObj.GetRoomBoundsMin();
        Camera.main.GetComponent<CameraController>().maxBound = roomObj.GetRoomBoundsMax();
        roomObj.backgroundCollider.enabled = false;

        //동적 슬라이드 객체에 이벤트 추가
        EventTrigger eV = roomObj.slideScreen.GetComponent<EventTrigger>();
        EventTrigger.Entry entry_PointerUp = new EventTrigger.Entry();
        entry_PointerUp.eventID = EventTriggerType.PointerUp;
        if (isWaitingRoom)
            entry_PointerUp.callback.AddListener((data) => { waitingRoom.ShowSlideReady(); });
        else
            entry_PointerUp.callback.AddListener((data) => { SlideManager.Instance.OnSlideScreen(); });
        eV.triggers.Add(entry_PointerUp);

        //첫 접속 시 테이블 세팅
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsMasterClient)
            {
                roomObj.teacherTable.Master = PhotonNetwork.PlayerList[i];
                if(PhotonNetwork.PlayerList[i].Equals(PhotonNetwork.LocalPlayer))
                    roomObj.myTable = roomObj.teacherTable;
            }
            else
            {
                for (int j = 0; j < roomObj.studentTable.Length; j++)
                {
                    if (roomObj.studentTable[j].Master == null)
                    {
                        roomObj.studentTable[j].Master = PhotonNetwork.PlayerList[i];
                        if (PhotonNetwork.PlayerList[i].Equals(PhotonNetwork.LocalPlayer))
                            roomObj.myTable = roomObj.studentTable[j];
                        break;
                    }
                }
            }
        }
    }

    #endregion

    #region 스터디 정보 업데이트
    /// <summary>
    /// Update문 대신 사용하는 코루틴
    /// </summary>
    IEnumerator StudyUpdate()
    {
        while (true)
        {
            if (!isSizeChange)
            {
                RectTransform rect = chatPanel.GetComponent<RectTransform>();
                float panelHeight = (uiCanvas.GetComponent<RectTransform>().rect.height / 4) * 3;
                rect.offsetMax = new Vector2(rect.offsetMax.x, -panelHeight);
            }

            yield return null;
        }
    }

    /// <summary>
    /// 룸 정보 표시 텍스트를 업데이트하는 함수
    /// </summary>
    public void RoomRenewal()
    {
        string roomName = (string)PhotonNetwork.CurrentRoom.CustomProperties["DisplayName"];
        string roomCategory = "카테고리 : " + (string)PhotonNetwork.CurrentRoom.CustomProperties["Category"];
        string currentNum = "현재 참가 인원 : " + PhotonNetwork.CurrentRoom.PlayerCount + "명";
        string maxNum = "최대 참가 인원 : " + PhotonNetwork.CurrentRoom.MaxPlayers + "명";
        roomInfoText.text = roomName + "\n" + roomCategory + "\n" + currentNum + "\n" + maxNum;
    }

    /// <summary>
    /// 첫 입장 시 참가자 리스트 초기화
    /// </summary>
    void SetParticipantList()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            GameObject participantItem = Instantiate(participantInfoItemObj, participantContent, false);
            ParticipantInfoItem info = participantItem.GetComponent<ParticipantInfoItem>();
            info.nickNameText.text = PhotonNetwork.PlayerList[i].NickName;
            info.Index = i + 1;
            string avatarInfo = (string)PhotonNetwork.PlayerList[i].CustomProperties["AvatarInfo"];
            info.emailID = (string)PhotonNetwork.PlayerList[i].CustomProperties["ID"];
            info.SetAvatarImage(avatarInfo);
            if (PhotonNetwork.MasterClient.Equals(PhotonNetwork.PlayerList[i]))
                info.typeText.text = "마스터";
            else
            {
                info.typeText.text = "그루퍼";
                if (DataManager.isMaster)
                    info.banButton.SetActive(true);
            }

            participantList.Add(info);
        }
    }

    public void AddParticipantList(Player player)
    {
        GameObject participantItem = Instantiate(participantInfoItemObj, participantContent, false);
        ParticipantInfoItem info = participantItem.GetComponent<ParticipantInfoItem>();
        info.nickNameText.text = player.NickName;
        info.Index = PhotonNetwork.CurrentRoom.Players.Count;
        string avatarInfo = (string)player.CustomProperties["AvatarInfo"];
        info.emailID = (string)player.CustomProperties["ID"];
        info.SetAvatarImage(avatarInfo);
        if (PhotonNetwork.MasterClient.Equals(player))
            info.typeText.text = "마스터";
        else
        {
            info.typeText.text = "그루퍼";
            if (DataManager.isMaster)
                info.banButton.SetActive(true);
        }

        participantList.Add(info);

        //책상 추가해줌
        for (int i = 0; i < roomObj.studentTable.Length; i++)
        {
            if (roomObj.studentTable[i].Master == null)
            {
                roomObj.studentTable[i].Master = player;                
                break;
            }
        }
    }

    /// <summary>
    /// 퇴장한 참가자를 리스트에서 삭제하는 함수
    /// </summary>
    public void DeleteParticipantList(Player player)
    {
        int deleteIndex = 0;
        for (int i = 0; i < participantList.Count; i++)
        {
            string listName = participantList[i].nickNameText.text;
            string playerName = (string)player.CustomProperties["NickName"];
            if (listName.Equals(playerName))
            {
                Destroy(participantList[i].gameObject);
                deleteIndex = i;
                participantList.RemoveAt(i);
                break;
            }
        }

        for (int i = deleteIndex; i < participantList.Count; i++)
        {
            participantList[i].Index--;
        }

        //책상 뺌
        for (int i = 0; i < roomObj.studentTable.Length; i++)
        {
            if (roomObj.studentTable[i].Master != null)
            {
                if (roomObj.studentTable[i].Master.Equals(player))
                {
                    roomObj.studentTable[i].Master = null;
                    break;
                }
            }
        }
    }
    #endregion

    #region 채팅

    public void CreateChatText(string chat)
    {
        GameObject textObj = Instantiate(chatTextObj, chatContent, false);
        textObj.GetComponent<Text>().text = chat;
    }

    /// <summary>
    /// 채팅 전송 실행 시 호출되는 함수
    /// </summary>
    public void Send()
    {
        //채팅 내용 없으면 패스
        if (chatInput.text.Equals("")) return;

        //귓속말 여부 체크
        if (chatInput.text[0].Equals('@'))
        {
            string[] temp = chatInput.text.Split(' ');
            string opponentName = temp[0].Replace("@", "");
            string chatContent = chatInput.text.Replace((temp[0] + " "), "");
            if(chatContent.Equals(""))
            {
                chatInput.ActivateInputField();
                return;
            }

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i].NickName.Equals(opponentName))
                {
                    myAvatar.GetComponent<AvatarAct>().ChatBoxRPC(chatContent, PhotonNetwork.LocalPlayer);
                    myAvatar.GetComponent<AvatarAct>().ChatBoxRPC(chatContent, PhotonNetwork.PlayerList[i]);
                    PrivateChatSendProcess(opponentName, chatContent, PhotonNetwork.PlayerList[i]);
                    chatInput.ActivateInputField();
                    return;
                }
            }

            string msg = "<color=#FF0000>귓속말 대상이 없습니다.</color>";
            CreateChatText(msg);
            chatInput.ActivateInputField();
        }
        else
        {
            myAvatar.GetComponent<AvatarAct>().ChatBoxRPC(chatInput.text, null);
            StartCoroutine(ChatSendProcess(chatInput.text));
            chatInput.ActivateInputField();
        }
    }

    IEnumerator ChatSendProcess(string chatText)
    {
        //채팅 정보 DB 전달
        string studyGUID = (string)PhotonNetwork.CurrentRoom.CustomProperties["StudyGUID"];
        string token = (string)PhotonNetwork.CurrentRoom.CustomProperties["Token"];
        yield return DataManager.Instance.ChatDataTransfer(studyGUID, token, PhotonNetwork.NickName, chatText);

        //이름 색깔 적용
        string nameColor = (string)PhotonNetwork.LocalPlayer.CustomProperties["NameColor"];
        string msg = "<color=" + nameColor + ">" + PhotonNetwork.NickName + "</color> : " + chatText;

        if (!isWaitingRoom)
        {
            //미니게임 채팅 입력 정답 시 처리
            if (MiniGameManager.Instance.isPlaying && !DataManager.isMaster)
            {
                if (MiniGameManager.Instance.ConfirmCorrectAnswer(chatText))
                    msg = "<color=green>" + msg + "</color>";
            }
        }

        //채팅 RPC 전송 및 인풋 필드 정리
        pV.RPC("ChatRPC", RpcTarget.All, msg);
        chatInput.text = "";
    }

    void PrivateChatSendProcess(string nameText, string chatText, Player receiver)
    {
        //이름 색깔 적용
        string msg = "<color=#6F00CC>" + PhotonNetwork.NickName + "→" + nameText + " : " + chatText + "</color>";
        pV.RPC("ChatRPC", receiver, msg);
        CreateChatText(msg);
        chatInput.text = "@" + nameText + " ";
    }

    [PunRPC]
    void ChatRPC(string msg) => CreateChatText(msg);

    #endregion

    #region 버튼 등 UI 조작
    /// <summary>
    /// 미니 - 전체 채팅 패널 온 오프
    /// </summary>
    public void ChatPanelSwich()
    {
        if (isDrag)
        {
            isDrag = false;
            return;
        }

        RectTransform rect = chatPanel.GetComponent<RectTransform>();
        if (!isSizeChange)
        {
            rect.offsetMax = new Vector2(rect.offsetMax.x, -10f);
            isSizeChange = true;
        }
        else
        {
            float panelHeight = (uiCanvas.GetComponent<RectTransform>().rect.height / 4) * 3;
            rect.offsetMax = new Vector2(rect.offsetMax.x, -panelHeight);
            isSizeChange = false;
        }
    }

    public void ChatPanelDrag() => isDrag = true;
    public void ChatPanelDragEnd() => isDrag = false;

    /// <summary>
    /// 채팅 동결 옵션 토글 버튼
    /// </summary>
    public void FreezeChatBoxOptionToggle()
    {
        CommonInteraction.Instance.isFreezeOption = toggleFreezeChatBox.isOn;
        freezeFunc(toggleFreezeChatBox.isOn);
    }

    /// <summary>
    /// 명찰 표시 토글 버튼
    /// </summary>
    public void DisplayNamePlate()
    {
        namePlateInputField.gameObject.SetActive(toggleDisplayNamePlate.isOn);
        CommonInteraction.Instance.displayNamePlate = toggleDisplayNamePlate.isOn;
        displayNamePlateFunc(toggleDisplayNamePlate.isOn);
    }

    /// <summary>
    /// 명찰 이름 입력이 완료되었을 때
    /// </summary>
    public void InputNameComplete()
    {
        if (namePlateInputField.text.Equals("")) return;
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
        if (hash["PlateName"] == null)
            hash.Add("PlateName", namePlateInputField.text);
        else
            hash["PlateName"] = namePlateInputField.text;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        myAvatar.GetComponent<AvatarAct>().SetNamePlateText(namePlateInputField.text);
    }

    public void SelectPreparation(int index)
    {
        studyPreparationPanel.tapNum = index;
        studyPreparationPanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 룸 퇴장 버튼
    /// </summary>
    public void LeaveRoomButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("해당 스터디 룸에서 퇴장하시겠습니까?");
        CommonInteraction.Instance.confirmFunc = LeaveRoom;
    }

    [PunRPC]
    public void LeaveRoom(bool check)
    {
        if (check)
        {
            CommonInteraction.Instance.displayNamePlate = false;
            uiCanvas.SetActive(false);
            masterCanvas.SetActive(false);
            grouperCanvas.SetActive(false);
            mainCamera.SetActive(false);
            InputControl.Instance.isValid = false;
            DataManager.Instance.currentStudyData = null;
            LoadManager.LoadScene("LobbyScene", LoadManager.WorkType.LEAVEROOM);
        }
    }

    /// <summary>
    /// 참가자 목록 UI 활성화 함수
    /// </summary>
    public void ParticipantPanelButton()
    {
        if (participantListPanel.activeSelf)
            participantListPanel.SetActive(false);
        else
            participantListPanel.SetActive(true);
    }

    /// <summary>
    /// 이모트 리스트 팝업 함수
    /// </summary>
    public void EmoteListPopupButton()
    {
        if (!emoteList.gameObject.activeSelf)
            emoteList.gameObject.SetActive(true);
        else
            emoteList.gameObject.SetActive(false);
    }

    /// <summary>
    /// 그루퍼를 강퇴할 때 사용되는 함수
    /// </summary>
    public void ForcedExitPlayer(int index)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(index);
        pV.RPC("LeaveRoom", player, true);
    }

    public void OnRoomInfo()
    {
        if (!roomInfoPanel.activeSelf)
            roomInfoPanel.SetActive(true);
        else
            roomInfoPanel.SetActive(false);
    }
    #endregion
}
