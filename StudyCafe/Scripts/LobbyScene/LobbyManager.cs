using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using Photon.Realtime;

public enum PurposeLeavingLounge { LOGOUT, STUDYSTART, CODEENTRY };

public class LobbyManager : Singleton<LobbyManager>
{
    [Header("LobbyPanel")]
    public GameObject mainCamera;
    public GameObject lobbyPanel;
    public GameObject menuPanel;    
    
    public RoomObject roomObj;
    public ExtraAct[] extras;
    public GameObject createAvatarPanel;
    public GameObject myAvatar;

    [Header("MyAvatarInfoPanel")]
    public GameObject myInfoPanel;

    [Header("CreateStudyPanel")]
    public GameObject createStudyCanvas;

    [Header("StudyRecruitmentPanel")]
    public GameObject studyRecruitmentCanvas;

    [Header("MyStudyListPanel")]
    public GameObject myStudyListCanvas;

    [Header("CodePanel")]
    public GameObject codeCanvas;

    [Header("Alarm")]
    public Alarm alarm;

    [Header("Extra")]
    public PurposeLeavingLounge purpose;
    public PhotonView pV;

    [Header("OneOnOnePanel")]
    public Transform OneOnOneCanvas;
    public List<OneOnOneConversation> conversationPanels = new List<OneOnOneConversation>();

    [HideInInspector]
    public GameObject indexButtonObj, keywordButtonObj, imageItemObj,
        memberIDItemObj, dateFileItemObj, studyCardObj, subscriptionStudyItemObj, 
        myChatArea, otherChatArea;

    #region 초기 정보 세팅
    private void Start()
    {
        //플레이어의 아바타 생성 여부 먼저 체크
        //생성이 필요하다면 라운지 세팅 전 아바타 생성부터 진행
        if (DataManager.needToCreateAvatar)
            createAvatarPanel.SetActive(true);
        else
            FirstLoungeSetting();
    }

    public void FirstLoungeSetting()
    {
        Resources.UnloadUnusedAssets();
        UseResourcesLoad();
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.IsMessageQueueRunning = true;

        //아바타 생성
        myAvatar.transform.position = roomObj.spawnPos.position;

        ////카메라 사이즈, 이동 관련 세팅
        float height = roomObj.topPos.position.y - roomObj.bottomPos.position.y;
        Camera.main.GetComponent<CameraController>().cameraTarget = myAvatar.transform;
        Camera.main.GetComponent<CameraController>().SetHeight(height);
        Camera.main.GetComponent<CameraController>().minBound = roomObj.GetRoomBoundsMin();
        Camera.main.GetComponent<CameraController>().maxBound = roomObj.GetRoomBoundsMax();
        roomObj.backgroundCollider.enabled = false;

        //조작 여부 및 단축키 세팅
        InputControl.Instance.SceneSetting(SceneManager.GetActiveScene().buildIndex);
        InputControl.Instance.isValid = true;
        InputControl.Instance.myAvatar = myAvatar.GetComponent<AvatarAct>();
        InputControl.Instance.cancel = LogoutButton;

        //메뉴 버튼 단축키 대입
        for (int i = 0; i < menuPanel.transform.childCount; i++)
        {
            InputControl.Instance.menu[i] = menuPanel.transform.GetChild(i).gameObject.GetComponent<Button>().onClick.Invoke;
        }

        ////로비 NPC 초기화
        //npc.NPCTypeInfo = NPCAct.NPCType.LOBBY;
        //if (!CommonInteraction.Instance.lobbyEntryComplete)
        //{
        //    StartCoroutine(npc.TalkingDialog(PhotonNetwork.LocalPlayer.NickName + "님, 삼삼오오에 오신것을 환영합니다"));
        //    CommonInteraction.Instance.lobbyEntryComplete = true;
        //}

        //엑스트라 세팅
        //라운지에 있는 모든 엑스트라 객체 일단 비활성화
        for (int i = 0; i < extras.Length; i++)
        {
            extras[i].gameObject.SetActive(false);
        }
        int maxLoungeMember = PhotonNetwork.PlayerList.Length; //라운지에 있는 인원 get
        int maxExtra;
        //표시할 수 있는 최대 엑스트라 수 보다 라운지 인원이 적으면 maxExtra는 라운지에 있는 인원만큼 대입
        //라운지 인원이 더 많으면 maxExtra에 최대 엑스트라 수 대입
        if (maxLoungeMember < extras.Length) maxExtra = maxLoungeMember; 
        else maxExtra = extras.Length;
        //중복 없는 랜덤한 인원을 골라내기 위한 int 리스트, 라운지에 있는 멤버 수만큼 반복
        List<int> randRangeList = new List<int>();        
        for (int i = 0; i < maxLoungeMember; i++)
        {
            randRangeList.Add(i);
        }

        //maxExtra번 만큼 반복
        for (int i = 0; i < maxExtra; i++)
        {
            //랜덤 리스트에서 뽑아낼 인원이 없다면 종료
            if (randRangeList.Count.Equals(0)) break;
            //랜덤 리스트에 있는 인덱스 중 하나를 랜덤하게 픽하여 플레이어 정보 받아옴.
            //이후 랜덤 리스트에서 해당 인덱스를 제거하여 가능한 중복 인원 뽑히지 않게 처리
            int randNum = Random.Range(0, randRangeList.Count);
            string guid = (string)PhotonNetwork.PlayerList[randRangeList[randNum]].CustomProperties["GUID"];            
            //뽑힌 사람이 나라면?
            if(guid.Equals((string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]))
            {
                //재투표를 위해 i--
                i--;
                randRangeList.RemoveAt(randNum);
                continue;
            }
            else
            {
                //아니라면 아바타 정보, 닉네임 정보 받아온 후 엑스트라 세팅
                string avatarInfo = (string)PhotonNetwork.PlayerList[randRangeList[randNum]].CustomProperties["AvatarInfo"];
                string nickName = (string)PhotonNetwork.PlayerList[randRangeList[randNum]].CustomProperties["NickName"];
                extras[i].gameObject.SetActive(true);
                extras[i].CreationInitialization(guid, avatarInfo, nickName);
                extras[i].linkedPlayer = PhotonNetwork.PlayerList[randRangeList[randNum]];
                randRangeList.RemoveAt(randNum);
            }
        }
    }

    void UseResourcesLoad()
    {
        indexButtonObj = Resources.Load<GameObject>("Prefabs/UI/IndexButton");
        keywordButtonObj = Resources.Load<GameObject>("Prefabs/UI/KeywordButton");        
        imageItemObj = Resources.Load<GameObject>("Prefabs/UI/ImageItem");
        memberIDItemObj = Resources.Load<GameObject>("Prefabs/UI/memberIDItem");
        dateFileItemObj = Resources.Load<GameObject>("Prefabs/UI/DateFileItem");
        studyCardObj = Resources.Load<GameObject>("Prefabs/UI/StudyCard");        
        subscriptionStudyItemObj = Resources.Load<GameObject>("Prefabs/UI/SubscriptionStudyItem");
        myChatArea = Resources.Load<GameObject>("Prefabs/UI/MyOneChatArea");
        otherChatArea = Resources.Load<GameObject>("Prefabs/UI/OtherOneChatArea");
    }
    #endregion

    #region 로비 UI, 오브젝트 상호작용
    /// <summary>
    /// 접속 종료용 함수
    /// </summary>
    public void LogoutButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("삼삼오오를 떠나시겠습니까?");
        CommonInteraction.Instance.confirmFunc = Logout;        
    }

    void Logout(bool check)
    {
        if (check)
        {
            StartCoroutine(LogoutProcess());
        }
    }

    IEnumerator LogoutProcess()
    {
        purpose = PurposeLeavingLounge.LOGOUT;
        yield return StartCoroutine(CommonInteraction.Instance.FadeOut(1f));
        PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// 본인 정보 UI를 표시하는 함수
    /// </summary>
    public void MyInfoButton()
    {
        if (!myInfoPanel.activeSelf)
            myInfoPanel.SetActive(true);
        else
            myInfoPanel.SetActive(false);
    }

    public void OpenInviteCodePanelButton()
    {
        codeCanvas.SetActive(true);        
    }

    /// <summary>
    /// 스터디 모집
    /// </summary>
    public void StudyRecruitment() => studyRecruitmentCanvas.SetActive(true);

    /// <summary>
    /// 스터디 만들기
    /// </summary>
    public void CreateStudy() => createStudyCanvas.SetActive(true);

    /// <summary>
    /// 스터디 참가
    /// </summary>
    public void ParticipateStudy() => myStudyListCanvas.SetActive(true);

    #endregion    

    #region 1:1 대화 프로세스
    public void SendMessageProcess(Player target, string guid, string name, string chat)
    {
        //상대방에게 채팅 신호 및 내용 전달
        pV.RPC("OpenChatWindowRPC", target, guid, name, chat);
    }

    [PunRPC]
    void OpenChatWindowRPC(string guid, string name, string chat)
    {
        for (int i = 0; i < conversationPanels.Count; i++)
        {
            //열린 대화창 중 대화 상대의 guid가 같다면 이미 대화 중인 상황으로 판단
            if (conversationPanels[i].listnerGuid.Equals(guid))
            {
                conversationPanels[i].OneOnOneChat(chat, name, false);
                return;
            }
        }

        //돌려봤는데 대화창에 대입된게 없다면 == 대화중이지 않았다면
        //채팅창을 생성하고 정보 삽입
        GameObject chatWindow = Instantiate(Resources.Load<GameObject>("Prefabs/UI/OneOnOneConversationPanel"), OneOnOneCanvas, false);
        OneOnOneConversation panel = chatWindow.GetComponent<OneOnOneConversation>();

        //상대 Player를 guid정보와 대입하여 찾음
        Player linkedPlayer = null;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            string checkGuid = (string)PhotonNetwork.PlayerList[i].CustomProperties["GUID"];
            if (guid.Equals(checkGuid))
            {
                linkedPlayer = PhotonNetwork.PlayerList[i];
                break;
            }

            //리스트 다 뒤졌는데 없으면 나갔거나 문제 있는거니 대화 신호 전달 취소
            if (i.Equals(PhotonNetwork.PlayerList.Length - 1)) return;
        }

        //신호를 준 플레이어의 닉네임, 아바타 정보 저장
        string nickName = (string)linkedPlayer.CustomProperties["NickName"];
        string avatarInfo = (string)linkedPlayer.CustomProperties["AvatarInfo"];
        panel.PreparationSettings(linkedPlayer, nickName, guid, avatarInfo);
        panel.OneOnOneChat(chat, name, false);
    }

    #endregion

    #region 포톤 이벤트 함수
    public override void OnConnectedToMaster()
    {
        Debug.Log("라운지 씬에서 포톤 마스터 서버 연결 완료!");        
        switch (purpose)
        {
            case PurposeLeavingLounge.LOGOUT:
                {
                    Debug.Log("삼삼오오 로그아웃");
                }
                break;
            case PurposeLeavingLounge.STUDYSTART:
                {                    
                    LoadManager.LoadScene("WaitingRoomScene", LoadManager.WorkType.JOINSTUDY);
                }
                break;
            case PurposeLeavingLounge.CODEENTRY:
                {
                    LoadManager.LoadScene("WaitingRoomScene", LoadManager.WorkType.CODEENTRY);
                }
                break;
        }
    }

    /// <summary>
    /// 다른 플레이어가 입장했을 때 실행되는 함수
    /// </summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        for (int i = 0; i < extras.Length; i++)
        {
            if (extras[i].gameObject.activeSelf) continue;

            extras[i].gameObject.SetActive(true);
            string guid = (string)newPlayer.CustomProperties["GUID"];
            string avatarInfo = (string)newPlayer.CustomProperties["AvatarInfo"];
            string nickName = (string)newPlayer.CustomProperties["NickName"];
            extras[i].CreationInitialization(guid, avatarInfo, nickName);
            extras[i].linkedPlayer = newPlayer;
            break;
        }
    }

    /// <summary>
    /// 다른 플레이어가 퇴장했을 때 실행되는 함수
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        for (int i = 0; i < extras.Length; i++)
        {
            if (!extras[i].gameObject.activeSelf) continue;

            if(extras[i].guid.Equals((string)otherPlayer.CustomProperties["GUID"]))
            {
                extras[i].gameObject.SetActive(false);
                if(conversationPanels.Count > 0)
                {
                    for (int j = 0; j < conversationPanels.Count; j++)
                    {
                        if (conversationPanels[j].linkedPlayer.Equals(otherPlayer))
                        { 
                            conversationPanels[j].LeftRoomOtherPlayer();
                            conversationPanels.RemoveAt(j);
                            break;
                        }
                    }
                }
                break;
            }
        }
    }
    #endregion
}