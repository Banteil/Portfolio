using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스터디 시작 여부를 체크, 실행하는 클래스
/// </summary>
public class StudyOpenPanel : MonoBehaviour
{
    public Text totalParticipantsNum, currentParticipantsNum, nonParticipantNum;
    public Transform content;
    List<string> subscriberList = new List<string>();
    List<string> participantList = new List<string>();
    List<string> nonParticipantList = new List<string>();
    List<GameObject> itemList = new List<GameObject>();

    private void OnEnable()
    {
        StartCoroutine(InfoSetting());
    }

    IEnumerator InfoSetting()
    {
        CommonInteraction.Instance.StartLoding();
        string studyGUID = DataManager.Instance.currentStudyData.guid;
        yield return StartCoroutine(DataManager.Instance.GetJoinedMemberInfo(studyGUID));
        if (DataManager.Instance.info == null)
        {
            CommonInteraction.Instance.InfoPanelUpdate("가입 멤버 정보 획득 중 오류가 발생했습니다.\n다시 시도해 주세요.");
            CommonInteraction.Instance.isLoading = false;
            yield break;
        }

        string[] members = DataManager.Instance.info.Split('¶');
        for (int i = 0; i < members.Length - 1; i++)
        {
            string[] infos = members[i].Split('☞');
            subscriberList.Add(infos[1]);
        }
        totalParticipantsNum.text = "총 멤버 : " + subscriberList.Count + "명";

        for (int i = 0; i < RoomManager.Instance.participantList.Count; i++)
        {
            if (RoomManager.Instance.participantList[i].Index.Equals(1)) continue;
            participantList.Add(RoomManager.Instance.participantList[i].emailID);
        }
        currentParticipantsNum.text = "현재 참가자 : " + participantList.Count + "명";

        for (int i = 0; i < subscriberList.Count; i++)
        {
            bool isAttend = false;
            for (int j = 0; j < participantList.Count; j++)
            {
                Debug.Log(subscriberList[i] + ", " + participantList[j]);
                if (subscriberList[i].Equals(participantList[j]))
                {
                    isAttend = true;
                    break;
                }
            }
            if (!isAttend)
                nonParticipantList.Add(subscriberList[i]);
        }
        nonParticipantNum.text = "비참가자 : " + nonParticipantList.Count + "명";

        for (int i = 0; i < subscriberList.Count; i++)
        {
            yield return StartCoroutine(DataManager.Instance.GetGUIDInfo(subscriberList[i]));
            string guid = DataManager.Instance.info;
            yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.NICKNAME, guid));
            string nickName = DataManager.Instance.info;
            yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.AVATAR, guid));
            string avatarInfo = DataManager.Instance.info;

            GameObject item = Instantiate(Resources.Load<GameObject>("Prefabs/UI/ParticipantInfoItem"), content, false);
            ParticipantInfoItem script = item.GetComponent<ParticipantInfoItem>();
            script.emailID = subscriberList[i];
            script.nickNameText.text = nickName;
            script.typeText.text = subscriberList[i];
            script.Index = i + 1;
            script.SetAvatarImage(avatarInfo);
            if (NonParticipantCheck(subscriberList[i]))
            {
                Color32 background = Color.red;
                background.a = 100;
                item.GetComponent<Image>().color = background;
            }
            itemList.Add(item);
        }

        RoomManager.Instance.waitingRoom.isOpening = true;
        CommonInteraction.Instance.isLoading = false;
    }
    public void ConfirmButton()
    {
        RoomManager.Instance.waitingRoom.ConfirmStudyOpen();
        RoomManager.Instance.waitingRoom.isOpening = false;
        RoomManager.Instance.waitingRoom.OpenStudyButton(false);
    }
    public void CancelButton()
    {
        RoomManager.Instance.waitingRoom.isOpening = false;
        RoomManager.Instance.waitingRoom.OpenStudyButton(false);
    }

    bool NonParticipantCheck(string id)
    {
        for (int i = 0; i < nonParticipantList.Count; i++)
        {
            if (id.Equals(nonParticipantList[i]))
                return true;
        }

        return false;
    }

    public void IntrusionCheck(Player player)
    {
        string email = (string)player.CustomProperties["ID"];
        for (int i = 0; i < nonParticipantList.Count; i++)
        {
            if(nonParticipantList[i].Equals(email))
            {
                participantList.Add(email);
                nonParticipantList.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].GetComponent<ParticipantInfoItem>().emailID.Equals(email))
            {
                Color32 background = Color.black;
                background.a = 100;
                itemList[i].GetComponent<Image>().color = background;
            }
        }
        currentParticipantsNum.text = "현재 참가자 : " + participantList.Count + "명";
        nonParticipantNum.text = "비참가자 : " + nonParticipantList.Count + "명";
    }

    public void ExitCheck(Player player)
    {
        string email = (string)player.CustomProperties["ID"];
        for (int i = 0; i < participantList.Count; i++)
        {
            if (participantList[i].Equals(email))
            {
                nonParticipantList.Add(email);
                participantList.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].GetComponent<ParticipantInfoItem>().emailID.Equals(email))
            {
                Color32 background = Color.red;
                background.a = 100;
                itemList[i].GetComponent<Image>().color = background;
            }
        }
        currentParticipantsNum.text = "현재 참가자 : " + participantList.Count + "명";
        nonParticipantNum.text = "비참가자 : " + nonParticipantList.Count + "명";
    }

    private void OnDisable()
    {
        subscriberList = new List<string>();
        participantList = new List<string>();
        nonParticipantList = new List<string>();
        itemList = new List<GameObject>();
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}
