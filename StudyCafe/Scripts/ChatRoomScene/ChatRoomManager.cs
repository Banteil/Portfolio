using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using FreeDraw;
using FrostweepGames.WebGLPUNVoice;
using FrostweepGames.WebGLPUNVoice.Examples;

/// <summary>
/// 늦게 접속 시 현재 상황에 맞춰 동기화를 시키기 위한 정보를 담은 클래스
/// </summary>
[Serializable]
public class SyncObject
{
    public int slideIndex; //슬라이드 인덱스
    public int slidePage; //슬라이드 내부 페이지 인덱스
    public float videoFrame; //슬라이드 비디오 프레임
    public bool isPause; //슬라이드 비디오 상태    
    public List<float> drawXPos = new List<float>(); //그림 그리기 위치 정보 x
    public List<float> drawYPos = new List<float>(); //그림 그리기 위치 정보 y
    public List<char> penColor = new List<char>(); //펜 컬러 정보
    public List<string> postItGroupInfo; //인덱스_그룹명_로컬x_로컬y
    public List<string> postItItemInfo; //인덱스_색깔R_색깔G_색깔B_내용_로컬x_로컬y_부모그룹인덱스(없으면Null)_현재수정중여부Bool
    public bool isPlaying; //미니게임 플레이 여부
}

/// <summary>
/// 교육장의 기능만을 담은 매니저 클래스, RoomManager의 Instance로 사용됨
/// </summary>
public class ChatRoomManager : MonoBehaviourPunCallbacks
{
    [Header("CharRoomObjects")]
    public GameObject voiceChatOptionPanel;
    public GameObject mvpVotePanel;

    [Header("MasterCanvas")]
    public GameObject slideControlPanel;
    public GameObject slideUIControlPanel;
    public Button slideControlButton;

    [Header("GrouperCanvas")]
    public GameObject textBookListPanel;
    public FileViewer textBookViewer;
    public Transform downloadFileListContent;
    public GameObject notepad;

    [Header("SlideObject")]
    public Drawable drawable;
    public InputField slideChatInput;

    [Header("MiniGameObejcts")]
    public GameObject exitButton;

    [Header("PostItObejcts")]
    public GameObject postItPanel;

    [Header("StudyEndObject")]
    public GameObject studyEndPanel;
    public Text studyEndMessage;
    public Text studyEndTimer;
    public Text getKeyValueText;

    [Header("Sync")]
    [HideInInspector]
    public SyncObject syncObject = new SyncObject();
    public bool isSync;

    [Header("StateChecker")]
    public bool isMVP; //해당 클라이언트의 MVP 여부 체크용
    public bool playingMiniGame; //미니게임 실행 여부 체크용
    public bool banMicrophone; //마이크 사용 불가 처리용
    public bool isLecture; //강의 시작 여부 체크용
    public bool isDiscussion; //토론 시작 여부 체크용
    public bool isAnnouncement; //발표 시작 여부 체크용
    public bool initializingInfo = true; //방 입장 시 정보 초기화가 끝났는지 여부 체크용
    public bool studyEnd; //스터디 종료 여부 체크용

    //////////////////////////////////////////////////////////////////////////////////////////////////

    #region 초기화 및 업데이트
    private void Awake()
    {
        RoomManager.Instance.chatRoom = this;
        RoomManager.Instance.isWaitingRoom = false;
        StartCoroutine(CommonInteraction.Instance.FadeIn(1f));
        StartCoroutine(RoomInfoSetting());
    }

    #endregion

    #region 룸 정보 갱신
    IEnumerator RoomInfoSetting()
    {
        //DB에서 해당 방의 대화 정보 로딩
        string token = (string)PhotonNetwork.CurrentRoom.CustomProperties["Token"];
        yield return StartCoroutine(DataManager.Instance.ChatDataTransfer(token));
        InitDBChat();

        //슬라이드 세팅 정보 대입        
        string date = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurriculumDate"];
        yield return StartCoroutine(DataManager.Instance.GetSlideData(DataManager.Instance.currentStudyData.guid, date));
        string jsonData = DataManager.Instance.info;
        if (jsonData != null)
            SlideManager.Instance.SlideSetting = JsonConvert.DeserializeObject<SlideSettingInfo>(jsonData);
        else
        {
            slideControlButton.interactable = false;
            slideUIControlPanel.SetActive(false);
            SlideManager.Instance.masterSlidePanel.SetActive(false);
        }

        RoomManager.Instance.toggleDisplayNamePlate.isOn = CommonInteraction.Instance.displayNamePlate;
        //마이크 새로고침 시도
        gameObject.GetComponent<VoiceChat>().RefreshMicrophonesButtonOnClickHandler();

        initializingInfo = false;
    }

    IEnumerator Sync()
    {
        //초기화 설정 중엔 싱크 처리 미룸
        while (initializingInfo) { yield return null; }
        isSync = true;
        //슬라이드 정보 싱크 처리
        SlideManager.Instance.CurrentIndex = syncObject.slideIndex;
        SlideManager.Instance.currentPage = syncObject.slidePage;
        if (SlideManager.Instance.slideVideoPlayer.enabled)
        {
            while (!SlideManager.Instance.slideVideoPlayer.isPrepared) { yield return null; }
            SlideManager.Instance.SetVideoFrame(syncObject.videoFrame);
            if (syncObject.isPause) SlideManager.Instance.slideVideoPlayer.Pause();
        }

        //슬라이드 드로잉 싱크 처리
        List<Vector2> drawVector = new List<Vector2>();
        for (int i = 0; i < syncObject.drawXPos.Count; i++)
        {
            Vector2 vec = new Vector2(syncObject.drawXPos[i], syncObject.drawYPos[i]);
            drawVector.Add(vec);
        }
        if (drawVector.Count > 0)
            StartCoroutine(drawable.DrawSyncProcess(drawVector));

        //포스트잇 싱크 처리
        //그룹 : 인덱스_그룹명_로컬x_로컬y
        //아이템 : 인덱스_색깔R_색깔G_색깔B_내용_로컬x_로컬y_부모그룹인덱스(없으면Null)_현재수정중여부Bool
        PostItPanel panel = postItPanel.GetComponent<PostItPanel>();
        for (int i = 0; i < syncObject.postItGroupInfo.Count; i++)
        {
            string[] info = syncObject.postItGroupInfo[i].Split('_');
            int index = int.Parse(info[0]);
            string groupName = info[1];
            Vector2 groupPos = new Vector2(float.Parse(info[2]), float.Parse(info[3]));

            GameObject groupObj = Instantiate(panel.groupObj, panel.board, false);
            PostItGroup groupScript = groupObj.GetComponent<PostItGroup>();
            groupScript.index = index;
            groupScript.groupNameField.text = groupName;
            groupScript.panel = panel;
            groupObj.transform.localPosition = groupPos;
            panel.groupList.Add(groupScript);
        }

        for (int i = 0; i < syncObject.postItItemInfo.Count; i++)
        {
            string[] info = syncObject.postItItemInfo[i].Split('_');
            int index = int.Parse(info[0]);
            Color colorRGB = new Color(float.Parse(info[1]), float.Parse(info[2]), float.Parse(info[3]));
            string itemContents = info[4];
            Vector2 itemPos = new Vector2(float.Parse(info[5]), float.Parse(info[6]));
            int? parentIndex;
            if (info[7].Equals("null"))
                parentIndex = null;
            else
                parentIndex = int.Parse(info[7]);
            bool isModification;
            if (info[8].Equals("true"))
                isModification = true;
            else
                isModification = false;

            GameObject itemObj;
            if (parentIndex == null)
            {
                itemObj = Instantiate(panel.itemObj, panel.board, false);
                itemObj.transform.localPosition = itemPos;
            }
            else
            {
                itemObj = Instantiate(panel.itemObj, panel.groupList[(int)parentIndex].transform, false);
                itemObj.GetComponent<PostItItem>().groupTr = panel.groupList[(int)parentIndex].transform;
            }

            PostItItem itemScript = itemObj.GetComponent<PostItItem>();
            itemScript.index = index;
            itemScript.ItemColor = colorRGB;
            itemScript.ExampleStr = itemContents;
            itemScript.panel = panel;
            itemScript.board = panel.board;
            itemScript.isModification = isModification;
            panel.itemList.Add(itemScript);
        }

        //미니게임 중 여부 판단하여 실행
        if (syncObject.isPlaying)
            StartMiniGame();

        isSync = false;
    }

    //마스터 클라이언트를 기준으로 필수 초기화 변수들의 정보를 접속자에게 세팅해주는 함수
    [PunRPC]
    void SetMasterInfo(string data)
    {
        var bf = new BinaryFormatter();
        var ms = new MemoryStream(Convert.FromBase64String(data));
        syncObject = (SyncObject)bf.Deserialize(ms);
        StartCoroutine(Sync());
        //전달된 클래스로 초기화 실행
    }

    /// <summary>
    /// DB의 채팅 정보를 삽입하는 함수
    /// </summary>
    void InitDBChat()
    {
        if (DataManager.Instance.info == null)
            return;

        string[] msg = DataManager.Instance.info.Split('♨');
        for (int i = 0; i < msg.Length - 1; i++)
        {
            string[] splitMsg = msg[i].Split(':');
            string name = "<color=yellow>" + splitMsg[0] + "</color> : ";
            RoomManager.Instance.CreateChatText(name + splitMsg[1]);
        }
    }
    #endregion

    #region 채팅
    /// <summary>
    /// 채팅 전송 실행 시 호출되는 함수
    /// </summary>
    public void SlideSend()
    {
        if (slideChatInput.text.Equals("")) return;

        //귓속말 여부 체크
        if (slideChatInput.text[0].Equals('@'))
        {
            string[] temp = slideChatInput.text.Split(' ');
            string opponentName = temp[0].Replace("@", "");
            string chatContent = slideChatInput.text.Replace((temp[0] + " "), "");
            if (chatContent.Equals(""))
            {
                slideChatInput.ActivateInputField();
                return;
            }

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i].NickName.Equals(opponentName))
                {
                    RoomManager.Instance.myAvatar.GetComponent<AvatarAct>().ChatBoxRPC(chatContent, PhotonNetwork.LocalPlayer);
                    RoomManager.Instance.myAvatar.GetComponent<AvatarAct>().ChatBoxRPC(chatContent, PhotonNetwork.PlayerList[i]);
                    PrivateChatSendProcess(opponentName, chatContent, PhotonNetwork.PlayerList[i]);
                    slideChatInput.ActivateInputField();
                    return;
                }
            }

            string msg = "<color=#FF0000>귓속말 대상이 없습니다.</color>";
            ChatRPC(msg, "시스템");
            slideChatInput.ActivateInputField();
        }
        else
        {
            RoomManager.Instance.myAvatar.GetComponent<AvatarAct>().ChatBoxRPC(slideChatInput.text, null);
            StartCoroutine(ChatSendProcess(slideChatInput.text));
            slideChatInput.ActivateInputField();
        }
        
    }

    IEnumerator ChatSendProcess(string chatText)
    {
        string studyGUID = (string)PhotonNetwork.CurrentRoom.CustomProperties["StudyGUID"];
        string token = (string)PhotonNetwork.CurrentRoom.CustomProperties["Token"];
        yield return DataManager.Instance.ChatDataTransfer(studyGUID, token, PhotonNetwork.NickName, chatText);

        //이름 색깔 적용
        string nameColor = (string)PhotonNetwork.LocalPlayer.CustomProperties["NameColor"];
        string msg = "<color=" + nameColor + ">" + PhotonNetwork.NickName + "</color> : " + chatText;

        //채팅 RPC 전송 및 인풋 필드 정리
        RoomManager.Instance.pV.RPC("ChatRPC", RpcTarget.All, msg, PhotonNetwork.NickName);
        slideChatInput.text = "";
    }

    void PrivateChatSendProcess(string name, string chat, Player receiver)
    {
        string msg = "<color=#6F00CC>" + PhotonNetwork.NickName + "→" + name + " : " + chat + "</color>";
        RoomManager.Instance.pV.RPC("ChatRPC", PhotonNetwork.LocalPlayer, msg);
        RoomManager.Instance.pV.RPC("ChatRPC", receiver, msg);
        RoomManager.Instance.CreateChatText(msg);
        slideChatInput.text = "@" + name + " ";
    }

    [PunRPC]
    void ChatRPC(string msg, string name)
    {
        //슬라이드 채팅 적용
        string[] splitMsg = msg.Split(':');
        bool isMine = name.Equals(PhotonNetwork.NickName) ? true : false;
        SlideManager.Instance.SettingSlideChat(isMine, name, splitMsg[1]);

        //일반 채팅 적용
        RoomManager.Instance.CreateChatText(msg);
    }

    #endregion

    #region 채팅 룸 UI 조작
    /// <summary>
    /// 음성채팅 설정 UI 활성화 함수
    /// </summary>
    public void VoiceChatSettingButton()
    {
        if (voiceChatOptionPanel.activeSelf)
            voiceChatOptionPanel.SetActive(false);
        else
            voiceChatOptionPanel.SetActive(true);
    }

    public void PostItButton()
    {        
        if (!postItPanel.GetComponent<PostItPanel>().isSetting)
            iTween.MoveTo(postItPanel, iTween.Hash("islocal", true, "y", 0f, "time", 1f, "oncomplete", "CompleteSetting", "oncompletetarget", postItPanel));
        else
            iTween.MoveTo(postItPanel, iTween.Hash("islocal", true, "y", -1100f, "time", 1f, "oncomplete", "CompleteSetting", "oncompletetarget", postItPanel));
    }
    #endregion

    #region 채팅 룸 마스터 UI 조작
    /// <summary>
    /// 강의 모드 전환 함수
    /// </summary>
    public void LectureModeButton()
    {
        SlideManager.Instance.endLectureButton.SetActive(true);
        RoomManager.Instance.pV.RPC("StartLecture", RpcTarget.AllViaServer);
    }

    [PunRPC]
    void StartLecture()
    {
        isLecture = true;        
        SlideManager.Instance.OnSlideScreen();
    }

    /// <summary>
    /// 토론 모드 전환 함수
    /// </summary>
    public void DiscussionModeButton()
    {
        if (!isDiscussion)
        {
            RoomManager.Instance.pV.RPC("BanMicrophone", RpcTarget.Others);
            GameObject[] avatars = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < avatars.Length; i++)
            {
                if (avatars[i].GetComponent<PhotonView>().IsMine || avatars[i].GetComponent<PhotonView>().Owner.IsMasterClient) continue;

                avatars[i].GetComponent<AvatarAct>().StartAvatarGrayscale();
            }
            RoomManager.Instance.roomObj.gameObject.GetComponent<Grayscale>().StartGrayScale(1f);
            isDiscussion = true;
        }
        else
        {
            RoomManager.Instance.pV.RPC("BanMicrophone", RpcTarget.Others);
            GameObject[] avatars = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < avatars.Length; i++)
            {
                if (avatars[i].GetComponent<PhotonView>().IsMine || avatars[i].GetComponent<PhotonView>().Owner.IsMasterClient) continue;

                avatars[i].GetComponent<AvatarAct>().ResetAvatarGrayscale();
            }
            RoomManager.Instance.roomObj.gameObject.GetComponent<Grayscale>().ResetGrayScale(1f);
            isDiscussion = false;
        }
    }

    /// <summary>
    /// 발표 모드 전환 함수
    /// </summary>
    public void AnnouncementModeButton()
    {
        RoomManager.Instance.pV.RPC("BanMicrophone", RpcTarget.Others);
        GameObject[] avatars = GameObject.FindGameObjectsWithTag("Player");

        if (!isAnnouncement)
        {            
            for (int i = 0; i < avatars.Length; i++)
            {
                if (avatars[i].GetComponent<PhotonView>().IsMine || avatars[i].GetComponent<PhotonView>().Owner.IsMasterClient) continue;

                avatars[i].GetComponent<AvatarAct>().StartAvatarGrayscale();
            }
            RoomManager.Instance.roomObj.gameObject.GetComponent<Grayscale>().StartGrayScale(1f);
            isAnnouncement = true;
        }
        else
        {
            for (int i = 0; i < avatars.Length; i++)
            {
                if (avatars[i].GetComponent<PhotonView>().IsMine || avatars[i].GetComponent<PhotonView>().Owner.IsMasterClient) continue;

                avatars[i].GetComponent<AvatarAct>().ResetAvatarGrayscale();
            }
            RoomManager.Instance.roomObj.gameObject.GetComponent<Grayscale>().ResetGrayScale(1f);
            isAnnouncement = false;
        }
    }

    /// <summary>
    /// 음성채팅 중지 및 권한을 빼앗는 함수
    /// </summary>
    [PunRPC]
    void BanMicrophone()
    {
        if (!banMicrophone)
        {
            gameObject.GetComponent<VoiceChat>().MuteMyClientToggleValueChanged(false);
            banMicrophone = true;
        }
        else
            banMicrophone = false;
    }

    /// <summary>
    /// banMicrophone 여부와 관계없이 음성 채팅 기능을 켜는 함수<br/>
    /// 발표 등 상황에서 교사의 권한으로 특정 인원의 음성 채팅 기능을 켤 때 사용함
    /// </summary>
    [PunRPC]
    void OnMicrophone()
    {
        VoiceChat voice = gameObject.GetComponent<VoiceChat>();
        voice.muteRemoteClientsToggle.isOn = false;
        voice.recorder.StartRecord();
        voice.mutoRemoteImage.color = new Color(255, 255, 255, 0);
    }

    /// <summary>
    /// 슬라이드 컨트롤 바 활성화 함수
    /// </summary>
    public void SlideControlActive()
    {
        if (!slideControlPanel.activeSelf)
            slideControlPanel.SetActive(true);
        else
            slideControlPanel.SetActive(false);
    }

    /// <summary>
    /// 미니게임 실행 버튼
    /// </summary>
    public void StartMiniGameButton() => RoomManager.Instance.pV.RPC("StartMiniGame", RpcTarget.AllViaServer);

    [PunRPC]
    void StartMiniGame()
    {
        playingMiniGame = true;
        if (DataManager.isMaster)
            syncObject.isPlaying = true;
        exitButton.SetActive(false);
        slideControlPanel.SetActive(false);
        RoomManager.Instance.masterCanvas.SetActive(false);
        RoomManager.Instance.grouperCanvas.SetActive(false);
        SlideManager.Instance.onSlide = false;
        SlideManager.Instance.slideCamera.targetTexture = SlideManager.Instance.slideRenderTexture;
        SlideManager.Instance.slideUICanvas.SetActive(false);
        MiniGameManager.Instance.miniGameCanvas.SetActive(true);
        if (DataManager.isMaster)
            MiniGameManager.Instance.masterPanel.SetActive(true);
        else
            StartCoroutine(MiniGameManager.Instance.InfoTextPresentation());
    }

    /// <summary>
    /// 미니게임 종료 버튼
    /// </summary>
    public void EndMiniGameButton() => RoomManager.Instance.pV.RPC("EndMiniGame", RpcTarget.AllViaServer);

    [PunRPC]
    void EndMiniGame()
    {
        playingMiniGame = false;
        if (DataManager.isMaster)
            syncObject.isPlaying = false;
        MiniGameManager.Instance.Initialization(true);
        MiniGameManager.Instance.miniGameCanvas.SetActive(false);
        exitButton.SetActive(true);
        if (DataManager.isMaster)
            RoomManager.Instance.masterCanvas.SetActive(true);
        else
            RoomManager.Instance.grouperCanvas.SetActive(true);
    }

    /// <summary>
    /// 스터디 종료 버튼
    /// </summary>
    public void EndStudyButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("스터디를 종료하시겠습니까?");
        CommonInteraction.Instance.confirmFunc = CheckPlayMVPVote;
    }

    /// <summary>
    /// MVP 투표 여부를 체크하는 함수
    /// </summary>
    void CheckPlayMVPVote(bool playVote)
    {
        if (playVote)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
                RoomManager.Instance.LeaveRoom(true);
            else
            {
                CommonInteraction.Instance.ConfirmPanelUpdate("MVP 투표를 실행하시겠습니까?");
                CommonInteraction.Instance.confirmFunc = StartVoteCheck;
            }
        }
    }

    /// <summary>
    /// MVP 투표 여부를 체크하는 함수. 전역변수인 startMVPVote에 값을 부여함
    /// </summary>
    public void StartVoteCheck(bool isEnd)
    {
        if (isEnd)
            RoomManager.Instance.pV.RPC("StartVote", RpcTarget.AllViaServer);
        else
            RoomManager.Instance.pV.RPC("StudyEnd", RpcTarget.AllViaServer);
    }

    [PunRPC]
    void StartVote()
    {
        RoomManager.Instance.grouperCanvas.SetActive(false);
        RoomManager.Instance.masterCanvas.SetActive(false);
        mvpVotePanel.SetActive(true);
    }

    /// <summary>
    /// 등록된 그루퍼에게 다시 초대코드 메일을 보내는 함수
    /// </summary>
    public void SendInvitationCode() => StartCoroutine(SendInvitationProcess());

    IEnumerator SendInvitationProcess()
    {
        string email = (string)PhotonNetwork.CurrentRoom.CustomProperties["EMail"];
        string code = (string)PhotonNetwork.CurrentRoom.CustomProperties["Code"];
        string studyName = (string)PhotonNetwork.CurrentRoom.CustomProperties["DisplayName"];

        string subject = "[" + studyName + "] 스터디 초대 코드 입니다.";
        string content = "안녕하세요.<br>삼삼오오의 [" + studyName + "] 스터디에서 보낸 초대 메세지입니다." +
            "<br><br><b>초대 코드 : " + code + "</b>";

        yield return StartCoroutine(DataManager.Instance.SendEMail(email, subject, content));
        if (DataManager.Instance.info != null)
            CommonInteraction.Instance.InfoPanelUpdate("초대장 재발송 완료");
        else
            CommonInteraction.Instance.InfoPanelUpdate("초대장 발송 실패...");
    }

    [PunRPC]
    public void StudyEnd() => StartCoroutine(StudyEndProcess());

    /// <summary>
    /// 스터디 종료 연출 코루틴
    /// </summary>
    IEnumerator StudyEndProcess()
    {
        studyEnd = true;
        mvpVotePanel.SetActive(false);
        RoomManager.Instance.grouperCanvas.SetActive(false);
        RoomManager.Instance.masterCanvas.SetActive(false);
        float endTimer = 4f;
        string roomName = (string)PhotonNetwork.CurrentRoom.CustomProperties["DisplayName"];
        studyEndMessage.text = roomName + " 스터디를 마치겠습니다.\n수고하셨습니다.";
        studyEndPanel.SetActive(true);
        while (endTimer > 0f)
        {
            endTimer -= Time.deltaTime;
            int intTimer = (int)endTimer;
            studyEndTimer.text = intTimer.ToString("D2");
            yield return null;
        }
        DataManager.interactionData.studyGUID = (string)PhotonNetwork.CurrentRoom.CustomProperties["StudyGUID"];
        DataManager.interactionData.curriculumDate = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurriculumDate"];
        yield return StartCoroutine(DataManager.Instance.DeleteReviewQuiz());
        RoomManager.Instance.uiCanvas.SetActive(false);
        RoomManager.Instance.LeaveRoom(true);
    }

    #endregion

    #region 채팅 룸 그루퍼 UI 조작
    /// <summary>
    /// 해당 스터디에 설정된 교재를 다운받는 함수
    /// </summary>
    public void DownloadTextBook()
    {
        if (!textBookListPanel.activeSelf)
            textBookListPanel.SetActive(true);
        else
            textBookListPanel.SetActive(false);
    }

    /// <summary>
    /// 메모장을 오픈하는 함수
    /// </summary>
    public void OnMemoNotepad()
    {
        if (!notepad.activeSelf)
            notepad.SetActive(true);
        else
            notepad.SetActive(false);
    }
    #endregion

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomManager.Instance.RoomRenewal();
        //대기실이 아니고, 자신이 마스터일 경우, 새롭게 접속한 상대의 싱크를 맞출 수 있도록 RPC로 설정
        if (DataManager.isMaster)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, syncObject);
            string data = Convert.ToBase64String(ms.GetBuffer());
            //syncObject 클래스를 string 직렬화하여 전달
            RoomManager.Instance.pV.RPC("SetMasterInfo", newPlayer, data);

            string guid = (string)newPlayer.CustomProperties["GUID"];
            DataManager.interactionData.studyGUID = (string)PhotonNetwork.CurrentRoom.CustomProperties["StudyGUID"];
            DataManager.interactionData.curriculumDate = (string)PhotonNetwork.CurrentRoom.CustomProperties["CurriculumDate"];
            StartCoroutine(DataManager.Instance.CheckAttendance(guid, "pass"));
        }

        RoomManager.Instance.AddParticipantList(newPlayer);
        RoomManager.Instance.CreateChatText("<color=yellow>" + newPlayer.NickName + "님이 입장하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //스터디 종료 시 아무것도 하지 않고 퇴장
        if (studyEnd) return;

        //마스터 퇴장 시 처리
        string otherNum = (string)otherPlayer.CustomProperties["GUID"];
        string masterNum = (string)PhotonNetwork.CurrentRoom.CustomProperties["Teacher"];
        if (otherNum.Equals(masterNum))
        {
            CommonInteraction.Instance.InfoPanelUpdate("티처가 퇴장하여 스터디가 종료됩니다.");
            RoomManager.Instance.LeaveRoom(true);
            return;
        }

        RoomManager.Instance.RoomRenewal();
        //퇴장 메세지 처리
        RoomManager.Instance.DeleteParticipantList(otherPlayer);
        RoomManager.Instance.CreateChatText("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    private void OnDestroy()
    {
        gameObject.GetComponent<Recorder>().StopRecord();
    }
}
