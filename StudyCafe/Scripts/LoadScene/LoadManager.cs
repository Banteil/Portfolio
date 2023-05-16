using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadManager : MonoBehaviourPunCallbacks
{
    public Canvas canvas;
    public GameObject progressCircle;
    public Image resourceProgressFill;
    public Image photonProgressFill;
    public Image background;
    public Text lodingMessageText;
    public Text stateText;

    const int ready = 0;
    const int photonStart = 1;
    const int resource = 2;
    const int complete = 3;

    public enum Progress { READY, PHOTONSTART, PHOTONEND, RESOURCE, COMPLETE }
    public static Progress progress;
    string[] progressMessage = new string[5];

    public enum WorkType { LOGIN, LOGOUT, OFFLINEGUIDE, JOINSTUDY, CODEENTRY, LEAVEROOM }
    public static WorkType workType;
    public static string nextScene;
    public static string inputCode;
    public static bool isSceneLoading = false;
    bool state; //포톤 상태 체크용 bool (삭제 예정)

    int dotNumber = 0;
    AsyncOperation op;

    [DllImport("__Internal")]
    private static extern void SetCookie(string name, string value, int exp);

    [DllImport("__Internal")]
    private static extern string GetCookie(string name);

    enum LoginSuccessed { IDNOTFOUND, PASSNOTCORRECT, SUCCESS }

    [Header("Temp Login Panel")]
    public GameObject loginPanel;
    public InputField idInputField;
    public InputField passInputField;

    private void Awake()
    {
        if(DataManager.Instance == null)
        {
            GameObject manager = Instantiate(Resources.Load<GameObject>("Prefabs/DataManager"));
            manager.name = "DataManager";
        }
    }

    private void Start()
    {
        if (DataManager.isFirstCheck)
            FirstCheckProcess();
        else
            LoadStart();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Backslash))
        {
            if (!state) state = true;
            else state = false;
        }

        if (state)
            stateText.text = PhotonNetwork.NetworkClientState.ToString();
        else
            stateText.text = "";
    }

    #region 첫 방문 여부 및 쿠키 체크
    /// <summary>
    /// 메인 씬에 처음 진입했는지 여부와 웹페이지에 첫 방문인지 여부를 쿠키 정보로 체크하는 함수 
    /// </summary>
    void FirstCheckProcess()
    {
        string cookieData = "false";
        if (Application.platform.Equals(RuntimePlatform.WebGLPlayer))
        {
            cookieData = GetCookie("isFirst");
            if (cookieData == null || cookieData.Equals(""))
            {
                SetCookie("isFirst", "true", 9999);
                cookieData = GetCookie("isFirst");
            }
        }

        if (cookieData.Equals("true"))
        {
            //첫 방문 시 처리 필요, 현재는 false와 동일하게 처리
            SetCookie("isFirst", "false", 9999);
            DataManager.isFirstVisit = false;
        }
        else
        {
            DataManager.isFirstVisit = false;
        }

        DataManager.isFirstCheck = false;

        ////웹에서 받은 정보로 바로 로그인 루틴 진행(미완성)
        //if (Application.platform.Equals(RuntimePlatform.WebGLPlayer))
        //{
        //    StartCoroutine(LoginRoutine(idInputField.text, passInputField.text));
        //}
        //웹 연동 전까진 로그인 패널 무조건 표시
        loginPanel.SetActive(true);
    }

    void LoadStart()
    {
        InputControl.Instance.Initialization(false);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("LoadScene"));
        StartCoroutine(LoadSceneRoutine());
        StartCoroutine(TextUpdate());
    }

    #endregion

    #region 로그인 동작
    /// <summary>
    /// 로그인 실행 버튼
    /// </summary>
    public void LoginButton() => StartCoroutine(LoginRoutine(idInputField.text, passInputField.text));

    /// <summary>
    /// 로그인 과정 코루틴 함수
    /// </summary>
    IEnumerator LoginRoutine(string id, string password)
    {
        string loginUrl = "https://stubugs.com/php/login.php";
        WWWForm form = new WWWForm();
        form.AddField("Input_Id", id);
        form.AddField("Input_Password", password);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(loginUrl, form))
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

                //성공 여부를 enum으로 형변환하여 switch로 구분 실행
                LoginSuccessed loginSuccessed = (LoginSuccessed)Enum.Parse(typeof(LoginSuccessed), check[0]);

                switch (loginSuccessed)
                {
                    case LoginSuccessed.IDNOTFOUND:
                        CommonInteraction.Instance.InfoPanelUpdate("입력하신 ID가 확인되지 않습니다");
                        break;
                    case LoginSuccessed.PASSNOTCORRECT:
                        CommonInteraction.Instance.InfoPanelUpdate("패스워드가 알맞지 않습니다");
                        break;
                    case LoginSuccessed.SUCCESS:
                        //로그인 성공
                        //씬 전환 전 DataManager에 유저 정보 저장
                        string uniqueNumber = check[1];
                        string token = check[2];
                        StartCoroutine(UserDataPropertySetting(uniqueNumber, token));
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 로비 연결 성공 시 DB에서 필요 데이터 수신 후 정보를 표기하는 코루틴
    /// </summary>
    IEnumerator UserDataPropertySetting(string uniqueNum, string token)
    {
        Hashtable hash = new Hashtable();
        hash.Add("GUID", uniqueNum);

        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.ID, uniqueNum));
        hash.Add("ID", DataManager.Instance.info);

        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.NICKNAME, uniqueNum));
        hash.Add("NickName", DataManager.Instance.info);
        if(DataManager.Instance.info != null)
            PhotonNetwork.LocalPlayer.NickName = DataManager.Instance.info;

        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.AVATAR, uniqueNum));
        hash.Add("AvatarInfo", DataManager.Instance.info);
        if (DataManager.Instance.info == null)
            DataManager.needToCreateAvatar = true;

        hash.Add("Token", token);

        //고유번호, 닉네임, 아바타 정보, 로그인 토큰 정보가 담긴 해시테이블을 전달
        DataManager.loadData.hashtable = hash;        
        nextScene = "LobbyScene";
        workType = WorkType.LOGIN;
        LoadStart();
        loginPanel.SetActive(false);
    }
    #endregion

    /// <summary>
    /// 로딩 씬을 통해 로드할 때 호출되는 함수
    /// </summary>
    public static void LoadScene(string sceneName, WorkType type)
    {
        nextScene = sceneName;
        workType = type;
        SceneManager.LoadScene("LoadScene", LoadSceneMode.Additive);
    }

    IEnumerator LoadSceneRoutine()
    {
        isSceneLoading = true;
        progress = Progress.READY;
        yield return StartCoroutine(FadeOut(background, 1f));
        CommonInteraction.Instance.RemoveFade();

        progressCircle.SetActive(true);
        float timer = 0.0f;

        progress = Progress.PHOTONSTART;
        Debug.Log(workType);
        switch(workType)
        {
            case WorkType.LOGIN:
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.LocalPlayer.SetCustomProperties(DataManager.loadData.hashtable);
                PhotonNetwork.LocalPlayer.NickName = (string)PhotonNetwork.LocalPlayer.CustomProperties["NickName"];
                break;
            case WorkType.LOGOUT:
                yield return StartCoroutine(DataManager.Instance.Logout());
                PhotonNetwork.Disconnect();
                break;
            case WorkType.OFFLINEGUIDE:                
                progress = Progress.PHOTONEND;
                break;
            case WorkType.JOINSTUDY:
                PhotonNetwork.JoinOrCreateRoom(DataManager.loadData.code, DataManager.loadData.roomOptions, null);
                break;
            case WorkType.CODEENTRY:
                PhotonNetwork.JoinOrCreateRoom(DataManager.loadData.code, DataManager.loadData.roomOptions, null);
                break;
            case WorkType.LEAVEROOM:
                PhotonNetwork.LeaveRoom();
                break;
            default:
                SceneManager.LoadSceneAsync(nextScene);
                yield break;
        }

        while (true)
        {
            yield return null;

            timer += Time.deltaTime;

            if (!progress.Equals(Progress.PHOTONEND))
            {
                photonProgressFill.fillAmount = Mathf.Lerp(photonProgressFill.fillAmount, 0.9f, timer);
                if (photonProgressFill.fillAmount >= 0.9f)
                    timer = 0f;
            }
            else
            {
                photonProgressFill.fillAmount = Mathf.Lerp(photonProgressFill.fillAmount, 1f, timer);
                if (photonProgressFill.fillAmount.Equals(1.0f))
                {
                    break;
                }
            }
        }

        progress = Progress.RESOURCE;
        op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;
        
        while (!op.isDone)
        {
            yield return null;

            timer += Time.deltaTime;           

            if (op.progress < 0.9f)
            {
                resourceProgressFill.fillAmount = Mathf.Lerp(resourceProgressFill.fillAmount, op.progress, timer);
                if (resourceProgressFill.fillAmount >= op.progress)
                    timer = 0f;
            }
            else
            {
                resourceProgressFill.fillAmount = Mathf.Lerp(resourceProgressFill.fillAmount, 1f, timer);
                if (resourceProgressFill.fillAmount.Equals(1.0f))
                {
                    break;
                }
            }
        }

        progress = Progress.COMPLETE;
        DataManager.loadData = new DataManager.LoadData();
        yield return new WaitForSeconds(1f);
        if (workType.Equals(WorkType.LOGIN)) DataManager.isLogin = true;
        isSceneLoading = false;
        op.allowSceneActivation = true;
    }    

    #region 포톤 콜백 함수
    /// <summary>
    /// 마스터 서버에 접속되었을 때 실행하는 함수
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("로드 씬에서 포톤 마스터 서버 연결 완료!");
        PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// 로비에 접속 완료하였을 때 실행하는 함수
    /// </summary>
    public override void OnJoinedLobby()
    {
        Debug.Log("로드 씬에서 포톤 로비 입장 완료!");
        RoomOptions option = new RoomOptions();
        option.MaxPlayers = 0;
        option.IsVisible = false;
        PhotonNetwork.JoinOrCreateRoom("Lounge", option, null);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("룸 생성 완료");
    }

    /// <summary>
    /// 룸 생성 실패 시 실행되는 함수
    /// </summary>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CommonInteraction.Instance.InfoPanelUpdate("스터디 룸 입장에 문제가 발생하였습니다.");
        LobbyManager.Instance.mainCamera.SetActive(true);
        InputControl.Instance.isValid = true;
        InputControl.Instance.myAvatar = LobbyManager.Instance.myAvatar.GetComponent<AvatarAct>();
        SceneManager.UnloadSceneAsync("LoadScene");
    }

    /// <summary>
    /// 룸 접속이 완료되었을 때 실행되는 함수.
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("룸 접속 성공! : " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.IsMessageQueueRunning = false;
        if(PhotonNetwork.CurrentRoom.CustomProperties["IsReady"] != null)
        {
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsReady"])
                nextScene = "ChatRoomScene";
        }

        progress = Progress.PHOTONEND;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(returnCode + ", " + message);
        CommonInteraction.Instance.InfoPanelUpdate("입력하신 코드가 틀렸거나, 스터디가 아직 열리지 않았습니다.");
        //DataManager.Instance.RemoveRoom(inputCode);
        LobbyManager.Instance.mainCamera.SetActive(true);
        InputControl.Instance.isValid = true;
        InputControl.Instance.myAvatar = LobbyManager.Instance.myAvatar.GetComponent<AvatarAct>();
        SceneManager.UnloadSceneAsync("LoadScene");        
    }

    public override void OnLeftRoom()
    {
        Debug.Log("로드 씬에서 룸 퇴장 완료!");
        CommonInteraction.Instance.LeaveRoomInitialization();        
    }
    #endregion

    #region 연출용
    IEnumerator TextUpdate()
    {
        switch (workType)
        {
            case WorkType.LOGIN:
                progressMessage[ready] = "카페 입장 준비";
                progressMessage[photonStart] = "입구를 통과하는 중";
                progressMessage[resource] = "카페의 로비를 확인하는 중";
                progressMessage[complete] = "입장 완료!";
                break;
            case WorkType.LOGOUT:
                progressMessage[ready] = "카페 퇴장 준비";
                progressMessage[photonStart] = "문을 나서는 중";
                progressMessage[resource] = "짐을 제대로 챙겼는지 확인 중";
                progressMessage[complete] = "다음에 또 와주세요 (_ _)";
                break;
            case WorkType.JOINSTUDY:
                progressMessage[ready] = "교육장 이동 준비";
                progressMessage[photonStart] = "교육장으로 이동 중";
                progressMessage[resource] = "교육장의 가구 배치를 정리하는 중";
                progressMessage[complete] = "이동 완료!";
                break;
            case WorkType.CODEENTRY:
                progressMessage[ready] = "교육장 이동 준비";
                progressMessage[photonStart] = "초대 코드를 직원에게 보이는 중";
                progressMessage[resource] = "직원의 안내에 따라 이동 중";
                progressMessage[complete] = "참가 완료!";
                break;
            case WorkType.LEAVEROOM:
                progressMessage[ready] = "교육장 퇴장 준비";
                progressMessage[photonStart] = "교육장 뒷정리 중";
                progressMessage[resource] = "로비로 이동 중";
                progressMessage[complete] = "이동 완료!";
                break;
            default:
                progressMessage[ready] = "WorkType 에러!";
                progressMessage[photonStart] = "WorkType 에러!";
                progressMessage[resource] = "WorkType 에러!";
                progressMessage[complete] = "WorkType 에러!";
                yield break;
        }

        while (true)
        {
            switch (progress)
            {
                case Progress.READY:
                    lodingMessageText.text = progressMessage[ready];
                    break;
                case Progress.PHOTONSTART:
                    lodingMessageText.text = progressMessage[photonStart];
                    break;
                case Progress.RESOURCE:
                    lodingMessageText.text = progressMessage[resource];
                    break;
                case Progress.COMPLETE:
                    lodingMessageText.text = progressMessage[complete];
                    yield break;
            }

            for (int i = 0; i < dotNumber; i++)
            {
                lodingMessageText.text += ".";
            }
            dotNumber++;
            if (dotNumber > 3) 
                dotNumber = 0;

            yield return new WaitForSeconds(0.5f);
        }
    }

    ///<summary>
    ///Image객체에 페이드 아웃 효과를 주는 함수
    ///</summary>
    IEnumerator FadeOut(Image fadeImage, float fadeTime)
    {
        fadeImage.raycastTarget = true;
        Color fadecolor = fadeImage.color;
        float timer = 0f;

        while (fadecolor.a < 1f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(0, 1, timer);
            fadeImage.color = fadecolor;
            yield return null;
        }
    }

    ///<summary>
    ///Image객체에 페이드 인 효과를 주는 함수
    ///</summary>
    IEnumerator FadeIn(Image fadeImage, float fadeTime)
    {
        Color fadecolor = fadeImage.color;
        float timer = 0f;

        while (fadecolor.a > 0f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(1, 0, timer);
            fadeImage.color = fadecolor;
            yield return null;
        }
        fadeImage.raycastTarget = false;
    }
    #endregion
}
