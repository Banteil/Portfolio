using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class WaitingRoomManager : MonoBehaviourPunCallbacks
{
    [Header("MasterCanvas")]
    public GameObject studyOpenPanel; //스터디 오픈 준비 판넬
    [HideInInspector]
    public bool isOpening; //스터디 오픈 준비 중인지 여부 판단 bool

    [Header("GroperCanvas")]
    public GameObject reviewQuizPanel; //복습 퀴즈 판넬
    Coroutine readyTextRoutine; //슬라이드 대기 연출용 루틴

    private void Awake()
    {
        RoomManager.Instance.waitingRoom = this;
        RoomManager.Instance.isWaitingRoom = true;
        //마스터 클라이언트와 씬을 동기화 하기 위한 설정
        //마스터가 교육장으로 이동했다면 늦게 접속한 사람도 곧바로 이동
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #region 룸 정보 갱신
    /// <summary>
    /// 대기실일 경우 슬라이드 클릭 시 처리
    /// </summary>
    public void ShowSlideReady()
    {
        if (readyTextRoutine != null)
            StopCoroutine(readyTextRoutine);
        readyTextRoutine = StartCoroutine(RoomManager.Instance.roomObj.ShowReadyText());
    }
    #endregion

    #region 티처 UI
    /// <summary>
    /// 대기실에서 준비를 마쳤을 때 교육장으로 이동하는 함수
    /// </summary>
    public void ConfirmStudyOpen()
    {        
        RoomManager.Instance.pV.RPC("OpenStudyRPC", RpcTarget.AllViaServer);
    }

    /// <summary>
    /// 스터디를 실행하는 RPC 함수
    /// </summary>
    [PunRPC]
    void OpenStudyRPC()
    {
        InputControl.Instance.isValid = false;
        StartCoroutine(OpenStudyProcess());
    }

    /// <summary>
    /// 대기실에서 준비를 마쳤을 때 교육장으로 이동하는 함수
    /// </summary>
    IEnumerator OpenStudyProcess()
    {
        RoomManager.Instance.mainCamera.SetActive(false);        
        if (DataManager.isMaster)
        {
            Hashtable hashtable = PhotonNetwork.CurrentRoom.CustomProperties;
            hashtable["IsReady"] = true;
            PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);

            PhotonNetwork.DestroyAll();
            yield return StartCoroutine(CommonInteraction.Instance.FadeOut(1f));
            PhotonNetwork.LoadLevel("ChatRoomScene");
        }
        else
        {
            StartCoroutine(CommonInteraction.Instance.FadeOut(1f));
        }
    }

    public void OpenStudyButton(bool check)
    {
        studyOpenPanel.SetActive(check);
    }
    #endregion

    /// <summary>
    /// 다른 플레이어가 입장했을 때 실행되는 함수
    /// </summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomManager.Instance.RoomRenewal();
        if (DataManager.isMaster)
        {
            if (isOpening)
                studyOpenPanel.GetComponent<StudyOpenPanel>().IntrusionCheck(newPlayer);
            RoomManager.Instance.AddParticipantList(newPlayer);
        }

        RoomManager.Instance.CreateChatText("<color=yellow>" + newPlayer.NickName + "님이 입장하셨습니다</color>");
    }

    /// <summary>
    /// 다른 플레이어가 퇴장했을 때 실행되는 함수
    /// </summary>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        string otherNum = (string)otherPlayer.CustomProperties["GUID"];
        string masterNum = (string)PhotonNetwork.CurrentRoom.CustomProperties["Teacher"];
        if (otherNum.Equals(masterNum))
        {
            CommonInteraction.Instance.InfoPanelUpdate("티처가 퇴장하여 스터디가 종료됩니다.");
            RoomManager.Instance.LeaveRoom(true);
            return;
        }

        RoomManager.Instance.RoomRenewal();
        if (DataManager.isMaster)
        {
            if (isOpening)
                studyOpenPanel.GetComponent<StudyOpenPanel>().ExitCheck(otherPlayer);
            RoomManager.Instance.DeleteParticipantList(otherPlayer);
        }
        RoomManager.Instance.CreateChatText("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }
}
