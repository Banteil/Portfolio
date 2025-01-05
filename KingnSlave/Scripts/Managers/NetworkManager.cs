using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using UnityEngine;
using UnityEngine.Networking;
using static starinc.io.Define;

namespace starinc.io.kingnslave
{
    public enum CustomSessionProperty
    {
        IsMatched,
        KeySid,
        MMR,
    }

    [Serializable]
    public class ResponseData
    {
        public string resultCd;
        public string resultMsg;
        public int returnCd;
        public object returnValue;
    }

    public class NetworkManager : NetworkSingleton<NetworkManager>
    {
        private const string apiURL = "https://kingnslave.com/api";

        [SerializeField]
        private MultiPlayer player;
        private int initializedPlayers;
        public int loadedPlayers;

        private string region = "us";

        private PhotonRunner photonRunner;
        public NetworkRunner MyRunner
        {
            get
            {
                if (photonRunner == null)
                {
                    Debug.Log("photonRunner is Null");
                    return null;
                }
                return photonRunner.MyRunner;
            }
        }

        public bool HasRunner
        {
            get
            {
                if (photonRunner == null || photonRunner?.MyRunner == null) return false;
                return true;
            }
        }

        public string RoomId
        {
            get
            {
                if (MyRunner == null || MyRunner.IsShutdown)
                    return string.Empty;
                return MyRunner.SessionInfo.Name;
            }
        }

        public bool IsMatching { get; set; } = false;
        public bool IsShutdown { get; set; } = false;

        public event Action CancelMatchCallback;

        protected override void OnAwake()
        {
            Util.DontDestroyObject(gameObject);
            //CheckRegionProcess();
        }

        #region Photon

        async private void CheckRegionProcess()
        {
            StartGameResult result = null;
            await CreateRunner();
            do
            {
                result = await MyRunner.JoinSessionLobby(SessionLobby.Custom, Define.GamePlayMode.None.ToString());
            }
            while (!result.Ok);
            region = MyRunner.LobbyInfo.Region;
            await Shutdown();
            Debug.Log($"확인된 현재 지역 : {region}");
        }

        /// <summary>
        /// 포톤 서버 연결에 필요한 앱 기본 설정을 가져오는 함수
        /// </summary>
        /// <returns></returns>
        public AppSettings GetAppSetting()
        {
            var appSettings = PhotonAppSettings.Instance.AppSettings.GetCopy();

            appSettings.UseNameServer = true;
            //appSettings.AppVersion = Application.version;
            appSettings.AppVersion = "1";

            if (string.IsNullOrEmpty(region) == false)
            {
                appSettings.FixedRegion = region.ToLower();
            }

            // If the Region is set to China (CN),
            // the Name Server will be automatically changed to the right one
            // appSettings.Server = "ns.photonengine.cn";

            return appSettings;
        }

        /// <summary>
        /// 포톤 서버 연결을 위한 러너 생성
        /// </summary>
        /// <returns></returns>
        async public UniTask CreateRunner()
        {
            if (photonRunner == null)
                photonRunner = new GameObject("PhotonRunner").GetOrAddComponent<PhotonRunner>();
            else
            {
                try
                {
                    if (photonRunner.MyRunner.State != NetworkRunner.States.Shutdown)
                        await Shutdown();
                    else
                        Destroy(photonRunner.gameObject);

                    await UniTask.Yield();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    if (photonRunner != null)
                        Destroy(photonRunner.gameObject);
                }
                photonRunner = new GameObject("PhotonRunner").GetOrAddComponent<PhotonRunner>();
            }
        }

        /// <summary>
        /// 포톤 서버 연결 끊기
        /// </summary>
        /// <returns></returns>
        async public UniTask Shutdown()
        {
            Debug.Log("Start Shutdown");
            IsShutdown = true;
            try
            {
                if (HasRunner)
                    await MyRunner.Shutdown();
                GameManager.Instance.ClearGame();
                //혹시나 셧다운을 했는데 photonRunner 객체가 살아있으면 파괴
                if (photonRunner != null)
                    Destroy(photonRunner.gameObject);
                photonRunner = null;
                IsShutdown = false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                photonRunner = null;
                IsShutdown = false;
            }
            Debug.Log("End Shutdown");
        }

        /// <summary>
        /// 매칭 시작 함수
        /// </summary>
        async public void StartMatch()
        {
            //매칭 중이면 리턴
            if (IsMatching) return;
            //매칭 상태 true
            IsMatching = true;
            //CCU 대기 시간까지 반복 트라이
            var checkCcuCount = 0;
            while (true)
            {
                try
                {
                    await CreateRunner();
                    var appSettings = GetAppSetting();
                    Debug.Log($"StartMatch Game Mode : {GameManager.Instance.CurrentGameMode}");
                    //세션 로비에 참가 시도, 로비는 현재 게임 모드에 따라 서로 매칭이 되지 않도록 나뉨
                    var result = await MyRunner.JoinSessionLobby(SessionLobby.Custom, GameManager.Instance.CurrentGameMode.ToString(), null, appSettings);
                    if (result.Ok)
                    {
                        //성공했으면 return으로 탈출, 이후 PhotonRunner OnSessionListUpdated로
                        Debug.Log($"JoinSessionLobby!");
                        return;
                    }
                    else
                    {
                        //CCU가 최대라서 JoinSession 실패했을 경우
                        if (result.ShutdownReason == ShutdownReason.MaxCcuReached)
                        {
                            checkCcuCount++;
                            //5초 * 120번 = 600초 대기했는데도 여전히 CCU 문제가 있다면
                            if (checkCcuCount >= Define.CheckCcuLimitCount)
                            {
                                //CCU 제한 에러메세지 표시 후 매칭 취소
                                UIManager.Instance.ShowWarningUI("ccuLimit");
                                CancelMatch();
                                return;
                            }

                            //5초 대기
                            await UniTask.Delay(5000);
                        }
                        else //그 외 문제로 매칭이 취소됐다면
                        {
                            //매칭 에러 메세지 표시 후 매칭 취소
                            UIManager.Instance.ShowWarningUI("Matching Error", false);
                            CancelMatch();
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    UIManager.Instance.ShowWarningUI("Matching Error", false);
                    ResetMatch();
                    return;
                }
            }
        }

        /// <summary>
        /// 매칭 취소 함수
        /// </summary>
        /// <returns></returns>
        async public void CancelMatch()
        {
            IsMatching = false;
            await Shutdown();
            GameManager.Instance.ClearGame();
            CancelMatchCallback?.Invoke();
        }

        /// <summary>
        /// 매칭 재시도 함수
        /// </summary>
        async public void ResetMatch()
        {
            Debug.Log("Reset Match");
            await Shutdown();
            IsMatching = false;
            StartMatch();
        }

        /// <summary>
        /// 직접 세션을 만들어 참가하는 함수
        /// </summary>
        /// <param name="playerType"></param>
        async public void CreateSession()
        {
            Debug.Log("Create Session!");
            //IsMatched 프로퍼티 FALSE 설정 후 게임 시작(세션 생성)
            try
            {
                var customProps = new Dictionary<string, SessionProperty>();
                customProps[CustomSessionProperty.IsMatched.ToString()] = (int)Define.Boolean.FALSE;
                customProps[CustomSessionProperty.MMR.ToString()] = UserDataManager.Instance.MyData.mmr;
                customProps[KEY_SID] = UserDataManager.Instance.MySid; // 20231216_2300 YD 세션 정보 분석을 위해 프로퍼티에 SID 추가.

                var result = await MyRunner.StartGame(new StartGameArgs()
                {
                    GameMode = GameMode.Shared,
                    SceneManager = photonRunner,
                    SessionProperties = customProps,
                    PlayerCount = 2,
                });

                //세션이 제대로 생성됐는지 여부 체크
                if (result.Ok)
                {
                    Debug.Log($"{MyRunner.SessionInfo.Name} Room Hosting!");
                }
                else
                {
                    //실패했다면 리매치
                    Debug.LogError($"Failed to Start: {result.ShutdownReason}");
                    ResetMatch();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (IsMatching)
                    ResetMatch();
            }
        }

        /// <summary>
        /// 참가할 세션이 있다면, roomName을 통해 참가하는 함수
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="playerType"></param>
        async public void JoinSession(string roomName)
        {
            //찾은 세션 네임으로 게임 참가
            try
            {
                var result = await MyRunner.StartGame(new StartGameArgs()
                {
                    GameMode = GameMode.Shared,
                    SessionName = roomName,
                    SceneManager = photonRunner,
                });

                if (result.Ok)
                {
                    Debug.Log($"{MyRunner.SessionInfo.Name} Room Join!");
                    Debug.Log($"<color=white>{UserDataManager.Instance.MySid}</color>");
                    Debug.Log($"Joined Sesstion Player Count : {MyRunner.SessionInfo.PlayerCount}");

                    // RPC로 사용자 아이디를 상대방에게 전달
                    //await Task.Delay(1000);
                    RPC_GiveUserData(MyRunner, UserDataManager.Instance.MyData.sid);
                    RPC_LoadScene(MyRunner, MultiGameSceneName);
                }
                else
                {
                    //참가에 실패했다면 리매치
                    Debug.LogError($"Failed to Start: {result.ShutdownReason}");
                    ResetMatch();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (IsMatching)
                    ResetMatch();
            }
        }

        /// <summary>
        /// 다른 유저의 데이터(sid)를 받을 때까지 대기하는 메서드
        /// </summary>
        /// <returns></returns>
        private async UniTask<bool> IsOhterUserDataInitialized()
        {
            const int maxOtherPlayerCount = 1;
            const int maxMsTime = 15000; // 15초
            int MsTimer = maxMsTime / 2; // 7.5초
            int delayMs = 100;

            //while (selectUserDataCompletedCount < maxOtherPlayerCount && timeOverMs > 0)
            //상대 유저 데이터 세팅 대기
            while (UserDataManager.Instance.OpponentDataList.Count < maxOtherPlayerCount &&
                UserDataManager.Instance.OpponentProfileImageList.Count < maxOtherPlayerCount && MsTimer > 0)
            {
                await UniTask.Delay(delayMs);
                MsTimer -= delayMs;
            }
            if (MsTimer <= 0) { return false; }

            //상대 텍스쳐 세팅 대기
            MsTimer = maxMsTime / 2; // 7.5초
            while (UserDataManager.Instance.OpponentProfileImageList[0] == null && MsTimer > 0)
            {
                await UniTask.Delay(delayMs);
                MsTimer -= delayMs;
            }
            if (MsTimer <= 0) { return false; }

            return true;
        }

        /// <summary>
        /// 게임 룸이 생성될 때까지 대기하는 메서드
        /// </summary>
        /// <returns></returns>
        async private UniTask<bool> IsRoomInitialized()
        {
            const int oneSecond = 1000; // 1초
            int counter = 10;

            Debug.Log($"OpponentDataList[0]: {UserDataManager.Instance.OpponentDataList[0]}");
            if (UserDataManager.Instance.MySid == UserDataManager.Instance.OpponentDataList[0].sid) { return false; }

            await CallAPI.APIGameRoomInitGame(UserDataManager.Instance.MyData, UserDataManager.Instance.OpponentDataList[0]);
            Debug.Log("RoomData: " + GameManager.Instance.RoomData);

            //null일 때 게임룸 초기화 API를 재호출
            while (GameManager.Instance.RoomData == null && counter > 0)
            {
                Debug.Log($"[Error] CurrentRoomData: {GameManager.Instance.RoomData}");
                await UniTask.Delay(oneSecond);
                await CallAPI.APIGameRoomInitGame(UserDataManager.Instance.MyData, UserDataManager.Instance.OpponentDataList[0]);
                --counter;
            }

            if (GameManager.Instance.RoomData == null) { return false; }
            return true;
        }

        public async void CallGameOverAPI(BattleWinner battleWinner)
        {
            if (!GameManager.Instance.IsNetworkGameMode()) { return; }

            if (battleWinner == BattleWinner.You)
            {
                await CallAPI.APIGameRoomFinishGame(UserDataManager.Instance.MyData, UserDataManager.Instance.OpponentDataList[0], UserDataManager.Instance.MySid, UserDataManager.Instance.OpponentDataList[0].sid, InGameManager.Instance.GetYourScore(), InGameManager.Instance.GetOpponentScore());
            }
            else if (battleWinner == BattleWinner.Opponent)
            {
                await CallAPI.APIGameRoomFinishGame(UserDataManager.Instance.MyData, UserDataManager.Instance.OpponentDataList[0], UserDataManager.Instance.OpponentDataList[0].sid, UserDataManager.Instance.MySid, InGameManager.Instance.GetOpponentScore(), InGameManager.Instance.GetYourScore());
            }
        }

        public async UniTask CallGameOverWhenOtherPlayerLeft()
        {
            if (!GameManager.Instance.IsNetworkGameMode()) { return; }

            if (GameManager.Instance.CurrentGameMode == GamePlayMode.PVPRank)
            {
                await CallAPI.APIGameRoomFinishGame(UserDataManager.Instance.MyData, UserDataManager.Instance.OpponentDataList[0], UserDataManager.Instance.MySid, UserDataManager.Instance.OpponentDataList[0].sid, InGameManager.Instance.GetYourScore(), InGameManager.Instance.GetOpponentScore());
                UserDataManager.Instance.UpdateMyRankGameData();
                InGameManager.Instance.GameOver?.Invoke(GameResult.Victory);
            }
            else if (GameManager.Instance.CurrentGameMode == GamePlayMode.PVPNormal)
            {
                await CallAPI.APIGameRoomFinishGame(UserDataManager.Instance.MyData, UserDataManager.Instance.OpponentDataList[0], UserDataManager.Instance.MySid, UserDataManager.Instance.OpponentDataList[0].sid, InGameManager.Instance.GetYourScore(), InGameManager.Instance.GetOpponentScore());
                UserDataManager.Instance.UpdateMyNormalGameData();
                InGameManager.Instance.GameOver?.Invoke(GameResult.Victory);
            }
        }

        public async void CallGameOverWhenPlayerLeft(bool hasResultWindow = false)
        {
            if (!GameManager.Instance.IsNetworkGameMode()) { return; }

            if (GameManager.Instance.CurrentGameMode == GamePlayMode.PVPRank)
            {
                await CallAPI.APIGameRoomFinishGame(UserDataManager.Instance.MyData, UserDataManager.Instance.OpponentDataList[0], UserDataManager.Instance.OpponentDataList[0].sid, UserDataManager.Instance.MySid, InGameManager.Instance.GetYourScore(), InGameManager.Instance.GetOpponentScore());
                UserDataManager.Instance.UpdateMyRankGameData();
            }
            else if (GameManager.Instance.CurrentGameMode == GamePlayMode.PVPNormal)
            {
                await CallAPI.APIGameRoomFinishGame(UserDataManager.Instance.MyData, UserDataManager.Instance.OpponentDataList[0], UserDataManager.Instance.OpponentDataList[0].sid, UserDataManager.Instance.MySid, InGameManager.Instance.GetYourScore(), InGameManager.Instance.GetOpponentScore());
                UserDataManager.Instance.UpdateMyNormalGameData();
            }

            if (hasResultWindow)
            {
                InGameManager.Instance.GameOver?.Invoke(GameResult.Defeat);
            }
            await Shutdown();
        }

        /// <summary>
        /// 세션에 참가한 모든 플레이어에게 씬 이동을 명령하는 함수
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="sceneName"></param>
        [Rpc]
        async public static void RPC_LoadScene(NetworkRunner runner, string sceneName)
        {
            Debug.Log("RPC_LoadScene");
            try
            {
                var exception = await Instance.CheckExceptionsBeforeLoadScene();
                if (exception)
                {
                    RPC_ResetMatch(Instance.MyRunner);
                    return;
                }

                if (runner.IsSharedModeMasterClient)
                {
                    var customProps = new Dictionary<string, SessionProperty>();
                    customProps[CustomSessionProperty.IsMatched.ToString()] = (int)Define.Boolean.TRUE;
                    var isUpdate = runner.SessionInfo.UpdateCustomProperties(customProps);
                    Debug.Log($"Host UpdateCustomProperties : {isUpdate}");
                }
                Instance.IsMatching = false;

                //로딩 씬 연출
                var direction = ScreenTransitionManager.Instance.ShowDirection<MultiSceneLoadDirection>();
                direction.SetUserData();
                await direction.StartDirection();

                runner.SetActiveScene(sceneName);
                if (GameManager.HasInstance)
                    GameManager.Instance.LoadScene(sceneName);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                RPC_ResetMatch(Instance.MyRunner);
            }
        }

        async private UniTask<bool> CheckExceptionsBeforeLoadScene()
        {
            var getDataResult = await Instance.IsOhterUserDataInitialized();
            if (getDataResult == false)
            {
                Debug.LogError($"FAIL: Can't get Opponent User Data");
                return true;
            }
            Debug.Log($"<color=white>Get Opponent sid: {UserDataManager.Instance.OpponentDataList[0].sid}</color>");

            getDataResult = await Instance.IsRoomInitialized();
            if (getDataResult == false)
            {
                Debug.LogError($"FAIL: Can't Initialize Game Room");
                return true;
            }

            if (!MyRunner.SessionInfo.IsValid || MyRunner.SessionInfo.PlayerCount < 2)
            {
                Debug.LogError($"FAIL: SessionInfo Error");
                return true;
            }

            return false;
        }

        public void ResetInitializedPlayers()
        {
            initializedPlayers = 0;
        }

        public void ResetLoadedePlayers()
        {
            loadedPlayers = 0;
        }

        public void SpawnYouPlayer() => MyRunner.Spawn(player, Vector3.zero, Quaternion.identity, MyRunner.LocalPlayer);

        [Rpc]
        public static void RPC_ResetMatch(NetworkRunner runner, RpcInfo rpcInfo = default) => Instance.ResetMatch();

        [Rpc]
        public static void RPC_InitializeDone(NetworkRunner runner, RpcInfo rpcInfo = default)
        {
            Debug.Log($"RPCInfo Source : {rpcInfo.Source}, Runner ID : {runner.LocalPlayer.PlayerId}");
            if (rpcInfo.Source == runner.LocalPlayer.PlayerId)
            {
                Debug.Log("Player Spwan");

            }
            ++Instance.initializedPlayers;
#if UNITY_EDITOR
            Debug.Log($"참여된 플레이어의 수: {Instance.initializedPlayers} [{Instance.MyRunner.LocalPlayer.PlayerId}]");
#endif
            if (Instance.initializedPlayers == MAX_GAME_PLAYER)
            {
                RPC_GameStart(runner, rpcInfo);
            }
        }

        [Rpc]
        public static void RPC_GameStart(NetworkRunner runner, RpcInfo rpcInfo = default)
        {
            Debug.Log($"GameStart : {runner.LocalPlayer.PlayerId}");
            GameObject.Find("InGameInitializer").GetComponent<InGameInitializer>().InitializeInGame();
        }

        [Rpc]
        public static void RPC_LoadSceneDone(NetworkRunner runner, RpcInfo rpcInfo = default)
        {
            ++Instance.loadedPlayers;
#if UNITY_EDITOR
            Debug.Log($"씬 로딩 완료된 플레이어의 수: {Instance.loadedPlayers} [{Instance.MyRunner.LocalPlayer.PlayerId}]");
#endif
            if (Instance.loadedPlayers == MAX_GAME_PLAYER)
            {
                LoadingSceneManager.Instance.IsLoadingDone = true;
                Debug.Log($"IsLoadingDone : {LoadingSceneManager.Instance.IsLoadingDone}");
            }
        }

        [Rpc(Channel = RpcChannel.Reliable, HostMode = RpcHostMode.SourceIsHostPlayer, InvokeLocal = false, TickAligned = false)]
        public static void RPC_GiveUserData(NetworkRunner runner, string systemUserId, RpcInfo rpcInfo = default)
        {
            Debug.Log($"<color=red>local:{runner.LocalPlayer.PlayerId}, source:{rpcInfo.Source.PlayerId} systemUserId:{systemUserId}</color>");

            RPC_ReplyUserData(runner, rpcInfo.Source, UserDataManager.Instance.MyData.sid);
            Instance.SetOpponentUserData(systemUserId);
        }

        [Rpc(Channel = RpcChannel.Reliable, HostMode = RpcHostMode.SourceIsHostPlayer, InvokeLocal = false, TickAligned = false)]
        public static void RPC_ReplyUserData(NetworkRunner runner, [RpcTarget] PlayerRef targetPlayer, string systemUserId, RpcInfo rpcInfo = default)
        {
            Debug.Log($"<color=red>local:{runner.LocalPlayer.PlayerId}, source:{rpcInfo.Source.PlayerId} systemUserId:{systemUserId}</color>");
            Instance.SetOpponentUserData(systemUserId);
        }

        /// <summary>
        /// 상대 정보 세팅 함수
        /// </summary>
        /// <param name="opponentSid"></param>
        async private void SetOpponentUserData(string opponentSid)
        {
            await CallAPI.APISelectUser(UserDataManager.Instance.MySid, opponentSid, async (userData) =>
            {
                UserDataManager.Instance.OpponentDataList.Add(userData);
                UserDataManager.Instance.OpponentProfileImageList.Add(null);
                UserDataManager.Instance.OpponentCardSkinImageList.Add(null);
                int index = UserDataManager.Instance.OpponentProfileImageList.Count - 1;

                // 내 카드 스킨 설정
                await SetMyCardSkin();

                // 상대 카드 스킨 설정
                ItemData cardSkinData = ShopManager.Instance.GetItemTypeList(ItemType.CardSkin).Find((data) => data.seq == userData.item_seq_card_skin);
                UserDataManager.Instance.OpponentCardSkinImageList[index] = await GetSpriteTask(cardSkinData?.image_url ?? null);

                // 상대 프로필 사진 설정
                await GetTextureTask((texture) =>
                {
                    UserDataManager.Instance.OpponentProfileImageList[index] = texture;
                }, userData.profile_image);
            });
        }

        async public UniTask SetMyCardSkin()
        {
            ItemData myCardSkin = UserDataManager.Instance.GetItemTypeList(ItemType.CardSkin).Find((data) => data.item_seq == UserDataManager.Instance.MyData.item_seq_card_skin);
            string myCardSkinUrl = myCardSkin?.image_url ?? null;
            if (UserDataManager.Instance.MyCardSkinImageUrl != myCardSkinUrl && myCardSkinUrl != null)
            {
                UserDataManager.Instance.MyCardSkinImageUrl = myCardSkinUrl;
                UserDataManager.Instance.MyCardSkinImage = await Instance.GetSpriteTask(myCardSkinUrl);
            }
        }
        #endregion

        #region Web Request
        public IEnumerator GetRequest(Action<UnityWebRequest> callback, string getURL, params string[] param)
        {
            if (param != null || param.Length > 0)
            {
                for (int i = 0; i < param.Length; i++)
                {
                    getURL += ((i == 0 ? "?" : "&") + $"{param[i]}");
                }
            }

            using (UnityWebRequest webRequest = UnityWebRequest.Get($"{apiURL}{getURL}"))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{webRequest.result}, {webRequest.error}");
                    callback(null);
                }
                else
                {
                    callback(webRequest);
                }
            }
        }

        async public UniTask GetRequestTask(Action<UnityWebRequest> callback, string getURL, params string[] param)
        {
            if (param != null || param.Length > 0)
            {
                for (int i = 0; i < param.Length; i++)
                {
                    getURL += ((i == 0 ? "?" : "&") + $"{param[i]}");
                }
            }

            UnityWebRequest webRequest = UnityWebRequest.Get($"{apiURL}{getURL}");
            var operation = webRequest.SendWebRequest();
            await UniTask.WaitUntil(() => operation.isDone);

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"{webRequest.result}, {webRequest.error}");
                callback(null);
            }
            else
            {
                callback(webRequest);
            }
        }

        /// <summary>
        /// Post 방식으로 URL 요청을 진행할 때 사용하는 코루틴
        /// </summary>
        /// <returns></returns>
        public IEnumerator PostRequest(Action<UnityWebRequest> callback, string postUrl, WWWForm form = null)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Post($"{apiURL}{postUrl}", form))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{webRequest.result}, {webRequest.error}");
                    callback = null;
                }
                else
                {
                    callback(webRequest);
                }
            }
        }

        public Coroutine PostRequestCoroutine(Action<UnityWebRequest> callback, string postUrl, WWWForm form = null)
        {
            return StartCoroutine(PostRequest(callback, postUrl, form));
        }

        async public UniTask PostRequestTask(Action<UnityWebRequest> callback, string postUrl, WWWForm form = null)
        {
            UnityWebRequest webRequest = UnityWebRequest.Post($"{apiURL}{postUrl}", form);
            var operation = webRequest.SendWebRequest();
            await UniTask.WaitUntil(() => operation.isDone);

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"{webRequest.result}, {webRequest.error}");
                callback(null);
            }
            else
            {
                callback(webRequest);
            }
        }

        async public UniTask GetTextureTask(Action<Texture> callback, string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            var filePath = Util.URLToFilePath(url);
            var isFileExist = File.Exists(filePath);

            if (isFileExist)
            {
                try
                {
                    var fileTexture = await Util.LoadTextureFile(filePath);
                    fileTexture.name = url;
                    callback(fileTexture);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    var fileTexture = await GetWebTextureProcess(url);
                    callback(fileTexture);
                }
            }
            else
            {
                var tempTexture = await GetWebTextureProcess(url);
                if (tempTexture != null)
                {
                    callback(tempTexture);
                    Util.SaveTextureFile(tempTexture, filePath);
                }
                else
                    callback(null);
            }
        }

        async public void GetTexture(Action<Texture> callback, string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            var filePath = Util.URLToFilePath(url);
            var isFileExist = File.Exists(filePath);

            if (isFileExist)
            {
                try
                {
                    var fileTexture = await Util.LoadTextureFile(filePath);
                    fileTexture.name = url;
                    callback(fileTexture);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    var webTexture = await GetWebTextureProcess(url);
                    callback(webTexture);
                }
            }
            else
            {
                var webTexture = await GetWebTextureProcess(url);
                if (webTexture != null)
                {
                    callback(webTexture);
                    Util.SaveTextureFile(webTexture, filePath);
                }
                else
                    callback(null);
            }
        }

        async private UniTask<Texture2D> GetWebTextureProcess(string url)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
            var operation = webRequest.SendWebRequest();
            await UniTask.WaitUntil(() => operation.isDone);

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var tempTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                tempTexture.name = url;
                return tempTexture;
            }
            else
            {
                Debug.Log($"{webRequest.result}, {webRequest.error}");
                return null;
            }
        }

        async public UniTask<Sprite> GetSpriteTask(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            var tcs = new UniTaskCompletionSource<Sprite>();

            await GetTextureTask((texture) =>
            {
                if (texture != null)
                {
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    var sprite = Sprite.Create((Texture2D)texture, rect, new Vector2(0.5f, 0.5f));
                    tcs.TrySetResult(sprite);
                }
                else
                    tcs.TrySetResult(null);
            }, url);

            return await tcs.Task;
        }

        public void GetSprite(Action<Sprite> callback, string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            GetTexture((texture) =>
            {
                if (texture != null)
                {
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    var sprite = Sprite.Create((Texture2D)texture, rect, new Vector2(0.5f, 0.5f));
                    callback(sprite);
                }
                else
                    callback(null);
            }, url);
        }
        #endregion        
    }
}