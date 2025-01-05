using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static starinc.io.Define;

namespace starinc.io.kingnslave
{
    public class PhotonRunner : MonoBehaviour, INetworkRunnerCallbacks, INetworkSceneManager
    {
        private NetworkRunner myRunner;
        public NetworkRunner MyRunner { get { return myRunner; } }

        private void Awake()
        {
            Util.DontDestroyObject(gameObject);
            myRunner = Util.GetOrAddComponent<NetworkRunner>(gameObject);
            myRunner.ProvideInput = true;
        }

        #region Photon Runner Callback
        public void Initialize(NetworkRunner runner)
        {
            Debug.Log("Initialize");
        }

        public void Shutdown(NetworkRunner runner)
        {
            Debug.Log("Shutdown Complete");
            if (!NetworkManager.Instance.HasRunner && NetworkManager.Instance.IsMatching)
                NetworkManager.Instance.ResetMatch();
        }

        public bool IsReady(NetworkRunner runner)
        {
            //현재 로딩 중이 아니고, 러너의 현재 씬(인게임)이 현재 활성화된 씬과 같다면 IsReady = true
            bool ready = !GameManager.Instance.IsLoading && (SceneManager.GetActiveScene().buildIndex == runner.CurrentScene);
            return ready;
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { Debug.Log("OnHostMigration"); }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"OnPlayerJoined_runner : {player.PlayerId}");
            PlayerJoinedProcess(runner, player);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"OnPlayerLeft_runner: {player.PlayerId}, My ID: {runner.UserId}");
            PlayerLeftProcess(runner, player);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            /*Debug.Log("OnInput");*/
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            Debug.Log("OnInputMissing");
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log($"OnShutdown_RunnerID : {runner.UserId}_ShutdownReason : {shutdownReason}");
            Debug.Log($"PlayerCount : {runner.SessionInfo.PlayerCount}");
        }

        public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("OnConnectedToServer"); }

        public void OnDisconnectedFromServer(NetworkRunner runner) { Debug.Log("OnDisconnectedFromServer"); }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { Debug.Log("OnConnectRequest"); }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.Log("OnConnectFailed"); }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { Debug.Log("OnUserSimulationMessage"); }

        public async void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            Debug.Log($"OnSessionListUpdated : {runner.LobbyInfo.Name}");
            if (runner.LobbyInfo.Name == Define.GamePlayMode.None.ToString())
            {
                await NetworkManager.Instance.Shutdown();
                return;
            }
            MatchingProcess(sessionList);
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { Debug.Log("OnCustomAuthenticationResponse"); }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { Debug.Log("OnReliableDataReceived"); }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log("OnSceneLoadStart");
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log($"OnSceneLoadDone : {runner.UserId}");
        }
        #endregion

        #region Callback Process
        private void PlayerJoinedProcess(NetworkRunner runner, PlayerRef player)
        {
            if (player.PlayerId == runner.LocalPlayer.PlayerId)
            {
                GameManager.Instance.PlayerID = player.PlayerId;
                NetworkManager.Instance.SpawnYouPlayer();
                NetworkManager.RPC_InitializeDone(NetworkManager.Instance.MyRunner);
            }
        }

        async private void PlayerLeftProcess(NetworkRunner runner, PlayerRef player)
        {
            var isOtherPlayerLeft = player.PlayerId != runner.LocalPlayer.PlayerId;
            //매칭 프로세스 중 상대 연결 끊김
            if (isOtherPlayerLeft)
            {
                Debug.Log($"PlayerLeftProcess : {runner.SessionInfo.PlayerCount}");
                if (NetworkManager.Instance.IsMatching)
                {
                    Debug.Log("매칭 중 상대 나감");
                    NetworkManager.Instance.ResetMatch();
                }
                else if (SceneManager.GetActiveScene().name == LoadingSceneName)
                {
                    Debug.Log("로딩 중 상대 나감");
                    await NetworkManager.Instance.Shutdown();
                    GameManager.Instance.ClearGame();
                    SceneManager.LoadScene(LobbySceneName);
                }
                else if (SceneManager.GetActiveScene().name == MultiGameSceneName)
                {
                    if (!gameObject.scene.isLoaded) return;
                    if (InGameManager.Instance.GetOpponentScore() < DEFAULT_WIN_CONDITION_SCORE && InGameManager.Instance.GetYourScore() < DEFAULT_WIN_CONDITION_SCORE)
                    {
                        Debug.Log("상대가 게임에서 나감");
                        await NetworkManager.Instance.CallGameOverWhenOtherPlayerLeft();
                        await NetworkManager.Instance.Shutdown();
                        GameManager.Instance.ClearGame();
                    }
                }
            }
        }

        /// <summary>
        /// 세션 로비 진입 후 sessionList를 쭉 둘러보고 세션 생성, 혹은 참가
        /// </summary>
        /// <param name="sessionList"></param>
        private void MatchingProcess(List<SessionInfo> sessionList)
        {
            //만들어진 세션 개수 체크
            if (sessionList.Count > 0)
            {
                try
                {
                    SessionInfo matchingSession = GetMatchingSession(sessionList);
                    //조회된 세션이 있다면 참가, 없다면 세션 생성
                    if (matchingSession == null)
                    {
                        Debug.Log("매칭할 세션 찾지 못함");
                        NetworkManager.Instance.CreateSession();
                    }
                    else
                    {
                        Debug.Log($"{matchingSession.Name} 세션에 참가");
                        NetworkManager.Instance.JoinSession(matchingSession.Name);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    NetworkManager.Instance.ResetMatch();
                }
            }
            else
            {
                Debug.Log("sessionList count 0");
                //세션이 없을 경우 세션 생성
                NetworkManager.Instance.CreateSession();
            }
        }

        private SessionInfo GetMatchingSession(List<SessionInfo> sessionList)
        {
            SessionInfo matchingSession = null;

            if (GameManager.Instance.CurrentGameMode == GamePlayMode.PVPRank)
            {
                //대기중인 리스트 추리기
                var waitingSessionList = sessionList.FindAll((session) =>
                {
                    var sessionProperties = session.Properties;
                    var isMatched = (int)sessionProperties[CustomSessionProperty.IsMatched.ToString()].PropertyValue;
                    var matchingPossible = session.PlayerCount < 2 && isMatched == (int)Define.Boolean.FALSE;
                    return matchingPossible;
                });

                //MMR 근사값
                var absMMR = waitingSessionList.Min((session) =>
                {
                    var sessionProperties = session.Properties;
                    var mmr = (int)sessionProperties[CustomSessionProperty.MMR.ToString()].PropertyValue;
                    var myMMR = UserDataManager.Instance.MyData.mmr;
                    return Math.Abs(mmr - myMMR);
                });

                //MMR 근사값에 맞는 리스트 추리기
                var mmrApproximationList = waitingSessionList.Where((session) =>
                {
                    var sessionProperties = session.Properties;
                    var mmr = (int)sessionProperties[CustomSessionProperty.MMR.ToString()].PropertyValue;
                    var myMMR = UserDataManager.Instance.MyData.mmr;
                    return Math.Abs(mmr - myMMR) == absMMR;
                }).ToList();

                matchingSession = mmrApproximationList[mmrApproximationList.Count - 1];
            }
            else
            {
                //플레이어 수가 1명이고, isMatched 프로퍼티가 FALSE(0) 인 세션 조회
                matchingSession = sessionList.FindLast((session) =>
                {
                    var sessionProperties = session.Properties;
                    var isMatched = (int)sessionProperties[CustomSessionProperty.IsMatched.ToString()].PropertyValue;
                    var matchingPossible = session.PlayerCount < 2 && isMatched == (int)Define.Boolean.FALSE;
                    return matchingPossible;
                });
            }

            return matchingSession;
        }

        #endregion
    }
}