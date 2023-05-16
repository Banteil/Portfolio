using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MVPVote : MonoBehaviour
{
    [HideInInspector]
    public List<GrouperVoteItem> mvpInfoList = new List<GrouperVoteItem>();
    List<int> mvpIndex = new List<int>();
    public bool mvpSelectComplete;
    float limitTime = 21f;

    [Header("UIObject")]
    public Text limitTimeText;
    public Transform grouperListContent;
    public PhotonView pV;

    private void OnEnable() => SetVoteItemList();

    /// <summary>
    /// 투표 제한시간 체크용 코루틴
    /// </summary>
    IEnumerator LimitTimer()
    {
        while(limitTime > 0f)
        {
            limitTime -= Time.deltaTime;
            int intTime = (int)limitTime;
            limitTimeText.text = intTime.ToString("D2");            
            yield return null;
        }
        limitTimeText.text = "00";
        StartCoroutine(MVPSelectionDirecting());
    }

    /// <summary>
    /// MVP 선정 연출용 코루틴
    /// </summary>
    IEnumerator MVPSelectionDirecting()
    {
        List<GrouperVoteItem> mvpList = new List<GrouperVoteItem>();
        if (DataManager.isMaster)
            SelectingMVP();
        while (!mvpSelectComplete) { yield return null; }

        if (mvpIndex.Count.Equals(0))
        {
            limitTimeText.text = "MVP가 없습니다...";
            for (int i = 0; i < mvpInfoList.Count; i++)
            {
                Destroy(mvpInfoList[i].gameObject);
            }
        }
        else
        {            
            int minus = 0;
            //mvp 리스트만 별도로 임시 저장
            for (int i = 0; i < mvpIndex.Count; i++)
            {
                mvpList.Add(mvpInfoList[mvpIndex[i]]);
                mvpInfoList.RemoveAt(mvpIndex[i] - minus);
                minus++;
            }

            //mvp 리스트 제외한 나머지 삭제
            for (int i = 0; i < mvpInfoList.Count; i++)
            {
                Destroy(mvpInfoList[i].gameObject);
            }

            limitTimeText.text = "MVP 그루퍼!";
            while (true)
            {
                for (int i = 0; i < mvpList.Count; i++)
                {
                    mvpList[i].transform.localScale += new Vector3(0.01f, 0.01f);
                }

                if (mvpList[0].transform.localScale.x > 1.5f)
                {
                    for (int i = 0; i < mvpList.Count; i++)
                    {
                        mvpList[i].transform.localScale = new Vector3(1.5f, 1.5f);
                    }
                    break;
                }
                yield return null;
            }

            if (DataManager.isMaster)
            {
                //마스터가 선택된 MVP들에게 정보 전달
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    for (int j = 0; j < mvpList.Count; j++)
                    {
                        if (PhotonNetwork.PlayerList[i].NickName.Equals(mvpList[j].nameText.text))
                        {
                            pV.RPC("SetMVP", PhotonNetwork.PlayerList[i]);
                            break;
                        }
                    }
                }

                pV.RPC("EndSetMVP", RpcTarget.AllViaServer);
            }

            while (mvpSelectComplete) { yield return null; }
        }

        yield return new WaitForSeconds(5f);
        RoomManager.Instance.chatRoom.StudyEnd();
    }

    [PunRPC]
    void SetMVP() => RoomManager.Instance.chatRoom.isMVP = true;

    [PunRPC]
    void EndSetMVP() => mvpSelectComplete = false;

    /// <summary>
    /// 투표용 그루퍼 리스트를 세팅하는 함수
    /// </summary>
    void SetVoteItemList()
    {
        int masterCheck = 0;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsMasterClient)
            {
                masterCheck = 1;
                continue;
            }
            
            GameObject voteItem = Instantiate(Resources.Load<GameObject>("Prefabs/UI/GrouperVoteItem"), grouperListContent, false);
            GrouperVoteItem voteItemScript = voteItem.GetComponent<GrouperVoteItem>();
            voteItemScript.nameText.text = PhotonNetwork.PlayerList[i].NickName;
            voteItemScript.index = i - masterCheck;
            voteItemScript.voteScript = this;
            string avatarInfo = (string)PhotonNetwork.PlayerList[i].CustomProperties["AvatarInfo"];
            voteItemScript.SetAvatarImage(avatarInfo);
            mvpInfoList.Add(voteItemScript);
        }

        StartCoroutine(LimitTimer());
    }

    /// <summary>
    /// 투표를 실행한 경우 추가 투표를 방지하기 위해 실행하는 함수
    /// </summary>
    public void EndVote(int index)
    {
        for (int i = 0; i < mvpInfoList.Count; i++)
        {
            mvpInfoList[i].voteButton.interactable = false;
        }

        pV.RPC("AddVoteCount", RpcTarget.All, index);
    }

    /// <summary>
    /// 투표 대상 리스트에 count를 더하는 함수
    /// </summary>
    [PunRPC]
    void AddVoteCount(int index) => mvpInfoList[index].voteCount++;

    /// <summary>
    /// 투표 종료 시 마스터 클라이언트에서 결과 계산 후, 모두에게 반환하기 위한 함수
    /// </summary>
    public void SelectingMVP()
    {
        string indexStr = "";
        int maxCount = 0;
        for (int i = 0; i < mvpInfoList.Count; i++)
        {
            if (maxCount < mvpInfoList[i].voteCount)
                maxCount = mvpInfoList[i].voteCount;
        }

        for (int i = 0; i < mvpInfoList.Count; i++)
        {
            if (mvpInfoList[i].voteCount.Equals(maxCount) && !maxCount.Equals(0))
            {
                indexStr += i.ToString() + ",";
            }
        }

        pV.RPC("MVPSelectionCompleted", RpcTarget.AllViaServer, indexStr);
    }

    /// <summary>
    /// 투표 결과가 나왔다는 정보를 뿌리기 위한 RPC 함수
    /// </summary>
    [PunRPC]
    void MVPSelectionCompleted(string indexStr)
    {
        if(indexStr.Equals(""))
        {
            mvpSelectComplete = true;
            return;
        }

        string[] index = indexStr.Split(',');
        for (int i = 0; i < index.Length - 1; i++)
        {
            mvpIndex.Add(int.Parse(index[i]));
        }

        for (int i = 0; i < mvpIndex.Count; i++)
        {
            for (int j = i + 1; j < mvpIndex.Count; j++)
            {
                if(mvpIndex[i] > mvpIndex[j])
                {
                    int temp = mvpIndex[i];
                    mvpIndex[i] = mvpIndex[j];
                    mvpIndex[j] = temp;
                }
            }
        }
        mvpSelectComplete = true;
    }
}
