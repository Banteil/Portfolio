using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Photon.Realtime;
using Photon.Pun;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public enum CheckSuccessed { FAIL, SUCCESS }
public enum Purpose { GET, SET, DEL, UPDATE }
public enum InfoType { ID, NICKNAME, AVATAR, INVENTORY }
public enum StudyComposition { MASTER, CODE, NAME, STUDYJSON, ANNOUNCEMENT }
public enum FileType { IMAGE, VIDEO, DOCUMENT, AUDIO, ETC }
public enum MailType { SIGNUP, INVITE }

public class DataManager : Singleton<DataManager>
{
    //php를 통해 넘겨받은 string 정보를 임시 저장하는 변수
    [HideInInspector]
    public string info;
    //php를 통해 넘겨받은 데이터 정보를 임시 저장하는 변수
    byte[] dataInfo;
    public byte[] DataInfo
    {
        get
        {
            byte[] temp = dataInfo;
            dataInfo = null;
            return temp;
        }
        set
        {
            Array.Resize(ref dataInfo, value.Length);
            dataInfo = value;
        }
    }

    public struct LoadData
    {
        public string roomName;
        public string code;
        public string email;
        public RoomOptions roomOptions;
        public Hashtable hashtable;
    }
    //로드 진행 시 로드 매니저에게 넘겨줄 정보를 보관하는 변수
    public static LoadData loadData = new LoadData();

    /// <summary>
    ///웹 페이지를 처음 방문했는지 여부를 판단하는 변수 (처음 한번만 체크하여 로그인 등 처리)
    /// </summary>
    public static bool isFirstCheck = true;
    /// <summary>
    ///첫 사용자인지, 기존 사용자인지 여부를 쿠키를 통해 판단하는 변수
    /// </summary>
    public static bool isFirstVisit = true;
    //현재 로그인 중인지 여부를 판단하는 변수
    public static bool isLogin = false;
    //웹에서 enter키를 입력했는지 여부를 판단하는 변수
    public static int isWebInput = 0;
    //마스터인지 여부를 판단하는 변수
    public static bool isMaster = false;     
    //아바타 생성 여부 필요 체크용 bool
    public static bool needToCreateAvatar = false;
    //임시 파일 업로드 여부 체크
    public static bool isTempFileUpload = false;
    //과제 파일 업로드 여부 체크
    public static bool isAssignmentFileUpload = false;

    //php경로 저장 변수
    string infoUrl, idExistCheckUrl, chatUrl, studyUrl, mailUrl, checkUrl, memberInfoUrl, getGUIDUrl, getCurrentStudyInfoUrl,
        getFileUrl, getFileDataUrl, checkFileUrl, pdfUrl,
        attendanceUrl, checkAttendanceUrl, checkAssignmentUrl, quizOrTestUrl, reviewQuizUrl, approachStudyCompositionUrl;

    string _fileTypesEntered;
    string[] _availableTypes = new string[]
    {
        "txt", "rtf", "doc", "docx", "html", "pdf", "ttf", "rar", "zip", "xls", "xlsx", "mp3", "avi", "mp4", "mkv", "wma", "wav", "cda",
        "mpg", "mpeg", "flv", "wmv", "bmp", "jpg", "jpeg", "tiff", "png"
    };

    string[] _movieTypes = new string[]
    {
        "avi", "mp4", "mkv", "mpg", "mpeg", "flv", "wmv", "asf", "asx", "ogm", "ogv", "mov", "webm"
    };

    string[] _imageTypes = new string[]
    {
        "bmp", "jpeg", "jpg", "png", "tiff", "raw"
    };

    string[] _documentTypes = new string[]
    {
        "pdf"
    };

    string[] _audioTypes = new string[]
    {
         "wav", "mp3", "ogg", "aac"
    };

    string[] _fileNameExceptionCharacters = new string[]
    {
        ",", "|", "@", ";"
    };

    public List<RoomInfo> roomList = new List<RoomInfo>();
    //현재 참가하는 스터디룸 정보 임시 저장
    public StudyInfoData currentStudyData;

    /// <summary>
    /// 상호작용 작업 진행 시 필요한 정보를 저장하는 구조체.<br /> 
    /// [Master] 마스터 GUID<br /> 
    /// [GUID] 스터디 GUID<br /> 
    /// [Token] 스터디 임시 토큰<br /> 
    /// [Type] 룸 타입<br /> 
    /// [Code] 접속 코드(실제 방 이름)<br /> 
    /// [DisplayName] 방 이름(보여주기용)<br /> 
    /// [EMail] 그루퍼들 이메일<br /> 
    /// [IsReady] 스터디 시작 여부 판단용<br /> 
    /// </summary>
    public struct InteractionData
    {
        public string studyGUID;
        public string studyDate;
        public string type;
        public string fileName;
        public string curriculumDate;
    }

    public static InteractionData interactionData = new InteractionData();

    //임시 델리게이트 함수(다른 객체, 클래스에 있는 void 함수를 임시로 실행할 때 사용)
    public delegate void InteractActive();
    public InteractActive interactFunc;
    public delegate void StringTransfer(string info);
    public StringTransfer stringTransfer;
    bool downloadComplete;

    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = -1;
        DontDestroyOnLoad(gameObject);
        idExistCheckUrl = "https://stubugs.com/php/idexistcheck.php";
        infoUrl = "https://stubugs.com/php/infotransfer.php";
        chatUrl = "https://stubugs.com/php/chatlog.php";
        memberInfoUrl = "https://stubugs.com/php/memberinfo.php";
        getGUIDUrl = "https://stubugs.com/php/getguidinfo.php";
        studyUrl = "https://stubugs.com/php/studyregistration.php";
        getCurrentStudyInfoUrl = "https://stubugs.com/php/getcurrentstudyinfo.php";
        mailUrl = "https://stubugs.com/php/email.php";
        checkUrl = "https://stubugs.com/php/logincheck.php";
        getFileUrl = "https://stubugs.com/php/getfilelist.php";
        getFileDataUrl = "https://stubugs.com/php/getfiledata.php";
        checkFileUrl = "https://stubugs.com/php/checkfile.php";        
        pdfUrl = "https://stubugs.com/php/pdftoimage.php";
        attendanceUrl = "https://stubugs.com/php/attendance.php";
        checkAttendanceUrl = "https://stubugs.com/php/checkattendance.php";
        checkAssignmentUrl = "https://stubugs.com/php/checkassignment.php";
        quizOrTestUrl = "https://stubugs.com/php/quizortestinfo.php";
        reviewQuizUrl = "https://stubugs.com/php/reviewquiz.php";
        approachStudyCompositionUrl = "https://stubugs.com/php/approachstudycomposition.php";
        StartCoroutine(LoginCheck());
    }

    //FPS 표시 테스트용
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Keypad0))
        {
            if (gameObject.GetComponent<FPSDisplay>().enabled)
                gameObject.GetComponent<FPSDisplay>().enabled = false;
            else
                gameObject.GetComponent<FPSDisplay>().enabled = true;
        }
    }

    #region 유저 정보 DB 연결
    /// <summary>
    /// 로그인 중복 여부 체크하는 코루틴
    /// </summary>
    IEnumerator LoginCheck()
    {
        const int purposeCheck = 0;
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (LoadManager.isSceneLoading) continue;

            if (isLogin)
            {
                WWWForm form = new WWWForm();
                form.AddField("Purpose", purposeCheck);
                form.AddField("UniqueNumber", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);
                form.AddField("Token", (string)PhotonNetwork.LocalPlayer.CustomProperties["Token"]);

                using (UnityWebRequest webRequest = UnityWebRequest.Post(checkUrl, form))
                {
                    yield return webRequest.SendWebRequest();                    

                    if (webRequest.isNetworkError || webRequest.isHttpError)
                    {
                        Debug.Log(webRequest.error);
                        info = null;
                    }
                    else
                    {
                        string[] check = webRequest.downloadHandler.text.Split(';');
                        CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                        switch (checkSuccessed)
                        {
                            case CheckSuccessed.FAIL:
                                Debug.Log("로그인 중복!");
                                PhotonNetwork.Disconnect();
                                CommonInteraction.Instance.InfoPanelUpdate("동일 ID 접속이 감지되었습니다.");
                                yield break;
                            case CheckSuccessed.SUCCESS:
                                //Debug.Log("로그인 정상 유지 중");
                                break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// user_info 정보 get 코루틴
    /// </summary>
    public IEnumerator InfoTransfer(InfoType type, string uniqueNum)
    {
        WWWForm form = new WWWForm();
        form.AddField("InputNumber", uniqueNum);
        form.AddField("SelectType", (int)type);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(infoUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                //echo로 전달된 텍스트를 ; 문자 기준으로 split
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 로그인 정보 set 코루틴
    /// </summary>
    public IEnumerator Logout()
    {
        const int purposeLogout = 1;
        WWWForm form = new WWWForm();
        form.AddField("Purpose", purposeLogout);
        form.AddField("UniqueNumber", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);
        form.AddField("Token", (string)PhotonNetwork.LocalPlayer.CustomProperties["Token"]);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(checkUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("로그아웃 실패");
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("로그아웃 성공");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// ID 정보를 DB 정보와 비교하여 중복 여부 체크
    /// </summary>
    public IEnumerator IDExistCheck(string id)
    {
        WWWForm form = new WWWForm();
        form.AddField("Input_Id", id);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(idExistCheckUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                CommonInteraction.Instance.InfoPanelUpdate("Network or Http Error");
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 그루퍼 정보 대입 코루틴
    /// </summary>
    public IEnumerator SetMemberInfo(string studyGUID, string userGUID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("StudyGUID", studyGUID);
        form.AddField("MemberGUID", userGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(memberInfoUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 그루퍼 정보 삭제 코루틴
    /// </summary>
    public IEnumerator DeleteMemberInfo(string studyGUID, string memberGUID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.DEL);
        form.AddField("StudyGUID", studyGUID);
        form.AddField("MemberGUID", memberGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(memberInfoUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 멤버 정보 반환 코루틴<br/>
    /// member_guid, member_id, phone_number, power, status 순으로 반환
    /// </summary>
    public IEnumerator GetMemberInfo(string studyGUID, string memberGUID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("StudyGUID", studyGUID);
        form.AddField("MemberGUID", memberGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(memberInfoUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }

    #region 멤버 정보 GET 코루틴
    public IEnumerator GetJoinedMemberInfo(string studyGUID)
    {
        string getJoinedMemberUrl = "https://stubugs.com/php/getjoinedmemberinfo.php";
        WWWForm form = new WWWForm();
        form.AddField("StudyGUID", studyGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(getJoinedMemberUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }
    #endregion

    #endregion

    #region 채팅 정보 DB 연결
    public IEnumerator ChatDataTransfer(string studyGUID, string token, string playerName, string chatData)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("Study_GUID", studyGUID);
        form.AddField("Token", token);
        form.AddField("NickName", playerName);
        form.AddField("Chat_Data", chatData);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(chatUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                Debug.Log("DB 채팅 데이터 반환 정보 : " + webRequest.downloadHandler.text);

                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("데이터 업데이트 실패");
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("데이터 업데이트 성공");
                        break;
                }
            }
        }
    }

    public IEnumerator ChatDataTransfer(string token)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("Token", token);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(chatUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                //Debug.Log("DB 채팅 데이터 반환 정보 : " + webRequest.downloadHandler.text);

                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("채팅 데이터 업데이트 실패 or 없음");
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("채팅 데이터 업데이트 성공");
                        string temp = "";
                        for (int i = 1; i < check.Length - 1; i++)
                            temp += check[i] + ";";

                        info = temp;
                        break;
                }
            }
        }
    }
    #endregion

    #region 스터디 정보 DB 연결
    ///<summary>
    ///스터디 정보 세팅 시 사용
    ///</summary>
    public IEnumerator SetStudyRegistration(StudyInfoData studyInfo)
    {
        string studyJsonData = JsonConvert.SerializeObject(studyInfo);

        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("StudyGUID", studyInfo.guid);
        form.AddField("Master", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);
        form.AddField("Code", studyInfo.code);
        form.AddField("Name", studyInfo.studyName);
        form.AddField("StudyJson", studyJsonData);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(studyUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    ///<summary>
    ///스터디 정보 수정 시 사용
    ///</summary>
    public IEnumerator UpdateStudyRegistration(StudyInfoData data)
    {
        string jsonData = JsonConvert.SerializeObject(data);
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.UPDATE);
        form.AddField("StudyGUID", data.guid);
        form.AddField("StudyJson", jsonData);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(studyUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    ///<summary>
    ///스터디 정보 삭제 시 사용
    ///</summary>
    public IEnumerator StudyRegistration(string stydyGUID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.DEL);
        form.AddField("StudyGUID", stydyGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(studyUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("스터디 삭제 실패");
                        break;
                    case CheckSuccessed.SUCCESS:
                        CommonInteraction.Instance.InfoPanelUpdate("스터디 삭제 완료");
                        break;
                }
            }
        }
    }

    ///<summary>
    ///특정 대상의 스터디 정보를 반환 시 사용
    ///</summary>
    public IEnumerator StudyRegistration()
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("Master", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(studyUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("스터디 정보 반환 실패");
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("스터디 정보 반환 성공");
                        info = "";
                        for (int i = 1; i < check.Length; i++)
                        {
                            info += check[i];
                            if (i < check.Length - 2) info += ";";
                        }
                        break;
                }
            }
        }
    }

    public IEnumerator GetCurrentStudyInfo()
    {
        WWWForm form = new WWWForm();
        form.AddField("StudyGUID", interactionData.studyGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(getCurrentStudyInfoUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("스터디 정보 반환 실패");
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("스터디 정보 반환 성공");
                        info = check[1];
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 특정 스터디에 공지사항을 추가하는 코루틴
    /// </summary>
    public IEnumerator AddNotice(string studyGUID, string tempToken, string guid, string content, string fileName)
    {
        string noticeInfoUrl = "https://stubugs.com/php/noticeinfo.php";

        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("StudyGUID", studyGUID);
        form.AddField("Token", tempToken);
        form.AddField("UserGUID", guid);
        form.AddField("Content", content);
        form.AddField("FileName", fileName);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(noticeInfoUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("공지사항 세팅 실패");
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("공지사항 세팅 성공");
                        info = check[1];
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 슬라이드 세팅 정보를 DB에서 가져오는 코루틴. StudyGUID는 interactionData 정보로 참고함
    /// </summary>
    public IEnumerator GetSlideData(string studyGUID, string date)
    {
        string transferSlideDataUrl = "https://stubugs.com/php/transferslidedata.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("StudyGUID", studyGUID);
        form.AddField("StudyDate", date);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(transferSlideDataUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("슬라이드 정보 습득 실패");
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("슬라이드 정보 습득 성공");
                        info = check[1];
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 슬라이드 세팅 정보를 DB에 대입하는 코루틴. StudyGUID는 interactionData 정보로 참고함
    /// </summary>
    public IEnumerator SetSlideData(string studyGUID, string date, string jsonData)
    {
        string transferSlideDataUrl = "https://stubugs.com/php/transferslidedata.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("StudyGUID", studyGUID);
        form.AddField("StudyDate", date);
        form.AddField("SlideData", jsonData);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(transferSlideDataUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = "FAIL";                        
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = "SUCCESS";
                        Debug.Log("슬라이드 정보 저장 성공");
                        break;
                }
            }
        }
    }

    public IEnumerator GetGUIDInfo(string id)
    {
        WWWForm form = new WWWForm();
        form.AddField("ID", id);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(getGUIDUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        CommonInteraction.Instance.InfoPanelUpdate("데이터 통신 오류");
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 출석 체크 정보 대입
    /// </summary>
    public IEnumerator SetAttendance(string curriculumDate)
    {
        WWWForm form = new WWWForm();
        form.AddField("StudyGUID", interactionData.studyGUID);
        form.AddField("CurriculumDate", curriculumDate);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(attendanceUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("출석 체크 세팅 실패 or 그루퍼 정보 없음");
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("출석 체크 세팅 완료");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 특정 유저 출석체크 정보 확인
    /// </summary>
    public IEnumerator CheckAttendance(string guid)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("StudyGUID", interactionData.studyGUID);
        form.AddField("CurriculumDate", interactionData.curriculumDate);
        form.AddField("GrouperGUID", guid);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(checkAttendanceUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);
                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = check[1];
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 특정 유저 출석체크 판정
    /// </summary>
    public IEnumerator CheckAttendance(string guid, string quizCheck)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("StudyGUID", interactionData.studyGUID);
        form.AddField("CurriculumDate", interactionData.curriculumDate);
        form.AddField("GrouperGUID", guid);
        form.AddField("QuizCheck", quizCheck);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(checkAttendanceUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("출석 체크 실패");
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("출석 체크 완료");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 과제 제출 여부 정보 반환
    /// </summary>
    public IEnumerator CheckAssignmentInfo(string grouperGUID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("StudyGUID", interactionData.studyGUID);
        form.AddField("GrouperGUID", grouperGUID);
        form.AddField("CurriculumDate", interactionData.curriculumDate);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(checkAssignmentUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                Debug.Log(check[1]);
                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = check[1];
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }

    public IEnumerator GetQuizOrTestInfo(int category, string studyGUID, string curriculumDate, string type)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("Category", category);
        form.AddField("StudyGUID", studyGUID);
        form.AddField("CurriculumDate", curriculumDate);
        form.AddField("Type", type);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(quizOrTestUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }

    public IEnumerator SetQuizOrTestInfo(int category, string guid, string studyGUID, string curriculumDate, string type, string jsonData)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("Category", category);
        form.AddField("GUID", guid);
        form.AddField("StudyGUID", studyGUID);
        form.AddField("CurriculumDate", curriculumDate);
        form.AddField("Type", type);
        form.AddField("Data", jsonData);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(quizOrTestUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    public IEnumerator DeleteQuizOrTestInfo(int category, string guid)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.DEL);
        form.AddField("Category", category);
        form.AddField("GUID", guid);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(quizOrTestUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    public IEnumerator UpdateQuizOrTestInfo(int category, string guid, string jsonData)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.UPDATE);
        form.AddField("Category", category);
        form.AddField("GUID", guid);
        form.AddField("Data", jsonData);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(quizOrTestUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    public IEnumerator GetReviewQuiz(string type)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("StudyGUID", interactionData.studyGUID);
        form.AddField("Type", type);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(reviewQuizUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = check[1];
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }

    public IEnumerator SetReviewQuiz(string type, string question, string answer, string options)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("StudyGUID", interactionData.studyGUID);
        form.AddField("CurriculumDate", interactionData.curriculumDate);
        form.AddField("Type", type);
        form.AddField("Question", question);
        form.AddField("Answer", answer);
        form.AddField("Options", options);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(reviewQuizUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = null;
                        break;
                }
            }
        }
    }

    public IEnumerator DeleteReviewQuiz()
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.DEL);
        form.AddField("StudyGUID", interactionData.studyGUID);
        form.AddField("CurriculumDate", interactionData.curriculumDate);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(reviewQuizUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = null;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 업로드 파일에 커리큘럼 및 타입 정보 대입
    /// </summary>
    public IEnumerator SetFileType(string fileType, string studyGUID, string curriculumDate, string fileName)
    {
        string fileTypeUrl = "https://stubugs.com/php/settingfiletype.php";
        WWWForm form = new WWWForm();
        form.AddField("FileType", fileType);
        form.AddField("StudyGUID", studyGUID);
        form.AddField("CurriculumDate", curriculumDate);
        form.AddField("FileName", fileName);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(fileTypeUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);
                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        if (check[1].Equals("insert"))
                            Debug.Log("파일 타입 추가 성공");
                        else
                            Debug.Log("동일 파일 있음");
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    public IEnumerator GetStudyCompositionInfo(StudyComposition type, string studyGUID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.GET);
        form.AddField("StudyComposition", (int)type);
        form.AddField("StudyGUID", studyGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(approachStudyCompositionUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }

    public IEnumerator SetStudyCompositionInfo(StudyComposition type, string data, string studyGUID)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.SET);
        form.AddField("StudyComposition", (int)type);
        form.AddField("StudyGUID", studyGUID);
        form.AddField("Data", data);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(approachStudyCompositionUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("스터디 정보 수정 실패");
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("스터디 정보 수정 완료");
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    #endregion

    #region 이메일 발송
    public IEnumerator SendEMail(string mailAddress, string subject, string content)
    {
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)MailType.SIGNUP);
        form.AddField("Address", mailAddress);
        form.AddField("Subject", subject);
        form.AddField("Content", content);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(mailUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                //echo로 전달된 텍스트를 ; 문자 기준으로 split
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        info = check[1];
                        break;
                }
            }
        }
    }
    #endregion

    #region 파일 브라우저 및 DB 연결
    public void OpenFileDialogButtonOnClickHandler()
    {
        string[] types = GetFilteredFileTypes();

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!string.IsNullOrEmpty(types[0]))
            {
                types[0] = types[0].Insert(0, ".");
            }
            FileBrowserDialogLib.FileWasOpenedEvent += FileWasOpenedEventHandler;
            FileBrowserDialogLib.FilePopupWasClosedEvent += FilePopupWasClosedEventHandler;
            FileBrowserDialogLib.OpenFileDialog(string.Join(",.", types));
#endif
    }

    private void FileWasOpenedEventHandler(byte[] data, string name, string resolution)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if(!isTempFileUpload)
            StartCoroutine(FileUpload(data, name));
        else
            StartCoroutine(TempFileUpload(data, name));
        FileBrowserDialogLib.FileWasOpenedEvent -= FileWasOpenedEventHandler;
        FileBrowserDialogLib.FilePopupWasClosedEvent -= FilePopupWasClosedEventHandler;        
#endif
    }

    private void FilePopupWasClosedEventHandler()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
            FileBrowserDialogLib.FileWasOpenedEvent -= FileWasOpenedEventHandler;
            FileBrowserDialogLib.FilePopupWasClosedEvent -= FilePopupWasClosedEventHandler;
#endif
    }

    /// <summary>
    /// 필터링 된 파일 형식을 가져오는 함수
    /// </summary>
    private string[] GetFilteredFileTypes()
    {
        string[] types = new string[] { "" };
        if (!string.IsNullOrEmpty(_fileTypesEntered))
        {
            _fileTypesEntered = _fileTypesEntered.Replace(" ", string.Empty);
            if (_fileTypesEntered.Split(new[] { ',' }).Length > 0)
            {
                types = _fileTypesEntered.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                  .Where(type => _availableTypes.Contains(type))
                  .ToArray();
            }
            else if (_availableTypes.Contains(_fileTypesEntered))
            {
                types = new string[] { _fileTypesEntered };
            }
        }
        return types;
    }

    /// <summary>
    /// 브라우저에서 선택된 파일을 서버에 업로드하는 코루틴. StudyGUID, Type은 interactionData 정보로 참고함
    /// </summary>
    public IEnumerator FileUpload(byte[] data, string name)
    {
        //로딩 처리 시작
        CommonInteraction.Instance.StartLoding();
        name = GetLowerExtensionName(name);

        //이름 유효성 검사
        string decodeName = UnityWebRequest.EscapeURL(name, System.Text.Encoding.UTF8);
        for (int i = 0; i < _fileNameExceptionCharacters.Length; i++)
        {
            if (decodeName.Contains(_fileNameExceptionCharacters[i]))
            {
                CommonInteraction.Instance.isLoading = false;
                CommonInteraction.Instance.InfoPanelUpdate("파일명에 '" + _fileNameExceptionCharacters[i] + "'문자가 포함되어 있으면 업로드가 불가능 합니다.");
                yield break;
            }
        }

        string uploadUrl = "";
        WWWForm form = new WWWForm();
        form.AddBinaryData("Data", data, name);
        form.AddField("StudyGUID", interactionData.studyGUID);
        form.AddField("UploaderGUID", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);
        if (isAssignmentFileUpload)
        {
            form.AddField("CurriculumDate", interactionData.curriculumDate);
            uploadUrl = "https://stubugs.com/php/assignmentfileupload.php";
        }
        else
            uploadUrl = "https://stubugs.com/php/fileupload.php";

        using (UnityWebRequest webRequest = UnityWebRequest.Post(uploadUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("업로드 실패");
                        interactFunc = null;
                        CommonInteraction.Instance.InfoPanelUpdate("파일 업로드 실패");
                        break;
                    case CheckSuccessed.SUCCESS:
                        string[] extension = check[1].Split('.');
                        if (extension[1].Equals("pdf"))
                        {
                            string fileDir = "/../storage/data/" + interactionData.studyGUID + "/";
                            yield return StartCoroutine(PDFToImage(interactionData.studyGUID, check[1], fileDir));
                        }
                        interactionData.fileName = check[1];
                        info = check[2];
                        interactFunc?.Invoke();
                        interactFunc = null;
                        CommonInteraction.Instance.InfoPanelUpdate("파일 업로드 성공");
                        break;
                }
            }
        }

        isAssignmentFileUpload = false;
        CommonInteraction.Instance.isLoading = false;
    }

    IEnumerator PDFToImage(string studyGUID, string fileName, string fileDir)
    {
        WWWForm form = new WWWForm();
        form.AddField("StudyGUID", studyGUID);
        form.AddField("FileName", fileName);
        form.AddField("FileDir", fileDir);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(pdfUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("PDF 파일 이미지 변환 실패");
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("PDF 파일 이미지 변환 성공");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 브라우저에서 선택된 파일을 서버의 임시 공간에 업로드하는 코루틴.
    /// </summary>
    public IEnumerator TempFileUpload(byte[] data, string name)
    {
        string tempUploadUrl = "https://stubugs.com/php/tempfileupload.php";
        isTempFileUpload = false;
        //로딩 처리 시작
        CommonInteraction.Instance.StartLoding();
        name = GetLowerExtensionName(name);
        //이름 유효성 검사
        string decodeName = UnityWebRequest.EscapeURL(name, System.Text.Encoding.UTF8);
        for (int i = 0; i < _fileNameExceptionCharacters.Length; i++)
        {
            if (decodeName.Contains(_fileNameExceptionCharacters[i]))
            {
                CommonInteraction.Instance.isLoading = false;
                CommonInteraction.Instance.InfoPanelUpdate("파일명에 '" + _fileNameExceptionCharacters[i] + "'문자가 포함되어 있으면 업로드가 불가능 합니다.");
                yield break;
            }
        }

        WWWForm form = new WWWForm();
        form.AddBinaryData("Data", data, name);
        form.AddField("FilePath", interactionData.type);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(tempUploadUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("업로드 실패");
                        CommonInteraction.Instance.InfoPanelUpdate("파일 업로드 실패");
                        stringTransfer = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        stringTransfer?.Invoke(check[1]);
                        stringTransfer = null;
                        CommonInteraction.Instance.InfoPanelUpdate("파일 업로드 성공");
                        break;
                }
            }
        }

        CommonInteraction.Instance.isLoading = false;
    }

    public void DeleteTempFileProcess(string filePath)
    {
        if (filePath == null || filePath.Equals("")) return;
        StartCoroutine(TempFileDelete(filePath));
    }

    /// <summary>
    /// 임시 공간에 있는 파일을 삭제하는 코루틴.
    /// </summary>
    IEnumerator TempFileDelete(string filePath)
    {
        string tempDeleteUrl = "https://stubugs.com/php/tempfiledelete.php";
        WWWForm form = new WWWForm();
        form.AddField("FilePath", filePath);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(tempDeleteUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("임시 파일 삭제 실패");
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("임시 파일 삭제 성공");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 특정 스터디로 구분된 파일을 파일명, 타입 정보를 토대로 체크하여 다운로드하는 함수
    /// </summary>
    public IEnumerator FileDownload(string fileDir, string fileName)
    {
        FileBrowserDialogLib.SaveUrlToFile(fileDir, fileName);
        while (true)
        {
            yield return null;
            if (downloadComplete)
            {
                downloadComplete = false;
                break;
            }
        }
        yield return new WaitForSeconds(0.1f);
    }

    public void CheckDownloadComplete() => downloadComplete = true;

    /// <summary>
    /// 특정 스터디에서 업로드한 파일 리스트를 DB에서 가져오는 코루틴.
    /// </summary>
    public IEnumerator GetFileList(string studyGUID, string curriculumDate, string type)
    {
        WWWForm form = new WWWForm();
        form.AddField("StudyGUID", studyGUID);
        form.AddField("CurriculumDate", curriculumDate);
        form.AddField("Type", type);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(getFileUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        //Debug.Log("파일 리스트 습득 실패");
                        info = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        //Debug.Log("파일 리스트 습득 성공");
                        info = check[1];
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 스터디에서 업로드한 파일의 데이터를 가져오는 코루틴. FileName, StudyGUID는 interactionData 정보로 참고함
    /// </summary>
    public IEnumerator GetFileData(string fileDir, string fileName)
    {
        WWWForm form = new WWWForm();
        form.AddField("FileDir", fileDir);
        form.AddField("FileName", fileName);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(getFileDataUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                if (webRequest.downloadHandler.data == null || webRequest.downloadHandler.data.Length.Equals(0))
                {
                    Debug.Log("파일 데이터 습득 실패");
                }
                else
                {
                    DataInfo = webRequest.downloadHandler.data;
                    Debug.Log("파일 데이터 습득 성공");
                }
            }
        }
    }

    /// <summary>
    /// 서버에 저장된 파일을 삭제하는 코루틴. FileName, StudyGUID, Type은 interactionData 정보로 참고함
    /// </summary>
    public IEnumerator FileDelete(string fileName, string studyGUID)
    {
        string delFileUrl = "https://stubugs.com/php/filedelete.php";
        WWWForm form = new WWWForm();
        form.AddField("FileName", fileName);
        form.AddField("StudyGUID", studyGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(delFileUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                Debug.Log("파일 삭제 정보 : " + webRequest.downloadHandler.text);
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("파일 삭제 실패");
                        CommonInteraction.Instance.InfoPanelUpdate("선택 파일 삭제 실패");
                        info = "FAIL";
                        break;
                    case CheckSuccessed.SUCCESS:
                        CommonInteraction.Instance.InfoPanelUpdate("선택 파일 삭제 완료");
                        info = "SUCCESS";
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 특정 유저가 특정 스터디에서 업로드한 파일의 데이터를 가져오는 코루틴. FileName, StudyGUID는 interactionData 정보로 참고함
    /// </summary>
    public IEnumerator CheckFile(string fileName, string studyGUID)
    {
        WWWForm form = new WWWForm();
        form.AddField("FileName", fileName);
        form.AddField("StudyGUID", studyGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(checkFileUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                info = null;
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        Debug.Log("파일 체크 실패");
                        info = "F";
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("파일 체크 성공");
                        info = "S";
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 파일 확장자 체크 후 이미지, 비디오, 문서 등 파일 타입을 판단, 반환하는 함수
    /// </summary>
    public FileType CheckFileType(string extension)
    {
        FileType fileType = FileType.ETC;
        foreach (string ex in _imageTypes)
        {
            if (extension.Equals(ex))
                return FileType.IMAGE;
        }

        foreach (string ex in _movieTypes)
        {
            if (extension.Equals(ex))
                return FileType.VIDEO;
        }

        foreach (string ex in _documentTypes)
        {
            if (extension.Equals(ex))
                return FileType.DOCUMENT;
        }

        foreach (string ex in _audioTypes)
        {
            if (extension.Equals(ex))
                return FileType.AUDIO;
        }

        return fileType;
    }
    #endregion

    /// <summary>
    /// 포톤 룸 프로퍼티 key에 value를 저장하는 함수
    /// </summary>
    public void SetPlayerProperties(string key, object value)
    {
        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
        if (hash[key] != null)
            hash[key] = value;
        else
            hash.Add(key, value);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }


    /// <summary>
    /// 포톤 룸 프로퍼티 key에 value를 저장하는 함수
    /// </summary>
    public void SetRoomProperties(string key, object value)
    {
        Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
        if (hash[key] != null)
            hash[key] = value;
        else
            hash.Add(key, value);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    /// <summary>
    /// 웹에서 엔터 인풋이 들어왔는지 체크하여 해당 정보를 전달하는 함수 
    /// </summary>
    public void InputComplete(int input)
    {
        isWebInput = input;
    }

    /// <summary>
    /// 포톤 접속이 끊어지거나 종료되었을 때 실행되는 함수
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("포톤 접속 끊김 : " + cause.ToString());
        isLogin = false;
        //https://doc-api.photonengine.com/en/pun/v2/namespace_photon_1_1_realtime.html#ad61b1461cf60ad9e8d86923d111d5cc9        
        if (cause.Equals(DisconnectCause.DisconnectByClientLogic)) //클라이언트 호출로 연결 종료 시(로그아웃)
        {
            if (LoadManager.isSceneLoading)
                LoadManager.progress = LoadManager.Progress.PHOTONEND;
            else
            {
                //현재는 메인 웹이 없어서 임시 구현
                Debug.Log("메인 웹 이동");
                Application.OpenURL("");
            }
        }
        else
        {
            //비정상 끊김 시 재접속 시도
            Debug.Log("비정상 포톤 연결 끊김");
            while (!PhotonNetwork.ReconnectAndRejoin()) ;
        }
    }

    /// <summary>
    /// 로비에 접속하거나, 룸 정보가 업데이트 될 때 실행되는 함수 <br/>
    /// 정보가 업데이트 된 룸 정보를 매개변수로 받음(ex. 룸 열기, 닫기)
    /// </summary>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (roomList.Count.Equals(0))
            this.roomList = roomList;

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
            {
                Debug.Log("룸 리스트에서 삭제");
                for (int j = 0; j < this.roomList.Count; j++)
                {
                    if (this.roomList[j].Equals(roomList[i]))
                    {
                        this.roomList.RemoveAt(j);
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("룸 리스트에 추가");
                if (!this.roomList.Contains(roomList[i]))
                    this.roomList.Add(roomList[i]);
            }
        }

        Debug.Log("현재 오픈 룸 리스트 개수 : " + this.roomList.Count);

    }

    public void RemoveRoom(string code)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            Debug.Log(roomList[i].IsOpen);
            if (roomList[i].Name.Equals(code))
            {
                roomList.RemoveAt(i);
                return;
            }
        }
    }

    public string ChangeNumberCharacters(string original, int num)
    {
        string digit = "";
        for (int i = 0; i < num; i++)
        {
            digit += "0";
        }
        int dummy = int.Parse(original);
        string value = dummy.ToString(digit);

        return value;
    }

    string GetLowerExtensionName(string name)
    {
        string value = "";
        string[] splitName = name.Split('.');
        string extension = splitName[splitName.Length - 1].ToLower();
        for (int i = 0; i < splitName.Length; i++)
        {
            if (i < splitName.Length - 1)
                value += splitName[i] + ".";
            else if (i.Equals(splitName.Length - 1))
                value += extension;
        }
        return value;
    }
}
