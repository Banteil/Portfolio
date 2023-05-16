using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class JoinedMemberPanel : MonoBehaviour
{
    GameObject memberItemObject;

    public Transform panelContent;
    List<MemberItem> memberItemList = new List<MemberItem>();

    public GameObject joinedMemberInfoPanel;

    private void Awake()
    {
        memberItemObject = Resources.Load<GameObject>("Prefabs/UI/MemberItem");
    }

    private void OnEnable()
    {
        StartCoroutine(SettingMemberInfo());
    }

    IEnumerator SettingMemberInfo()
    {
        CommonInteraction.Instance.StartLoding();

        yield return StartCoroutine(DataManager.Instance.GetJoinedMemberInfo(StudyPreparation.Instance.studyData.guid));
        if (DataManager.Instance.info == null)
        {
            CommonInteraction.Instance.InfoPanelUpdate("가입 멤버 정보 획득 중 오류가 발생했습니다.\n다시 시도해 주세요.");
            CommonInteraction.Instance.isLoading = false;
            yield break;
        }
        else
        {
            string[] members = DataManager.Instance.info.Split('¶');
            for (int i = 0; i < members.Length - 1; i++)
            {
                GameObject item = Instantiate(memberItemObject, panelContent, false);
                MemberItem itemScript = item.GetComponent<MemberItem>();
                itemScript.index = i;
                itemScript.JoinedInfo = members[i];
                if (StudyPreparation.Instance.powerToEdit[3])
                {
                    itemScript.infoDisplayFunction = MemberInformationCheck;
                    itemScript.deleteItemFunction = RemoveMemberList;
                }
                else
                    itemScript.kickbackButton.SetActive(false);
                memberItemList.Add(itemScript);
            }
        }
        CommonInteraction.Instance.isLoading = false;
    }

    ///<summary>
    ///멤버 정보 확인 함수
    ///</summary>
    void MemberInformationCheck(int index)
    {
        JoinedMemberInfoPanel panelScript = joinedMemberInfoPanel.GetComponent<JoinedMemberInfoPanel>();
        panelScript.memberGUID = memberItemList[index].MemberGUID;
        panelScript.joinedInfo = memberItemList[index].JoinedInfo;
        joinedMemberInfoPanel.SetActive(true);
    }

    void RemoveMemberList(int index)
    {
        memberItemList.RemoveAt(index);
        for (int i = index; i < memberItemList.Count; i++)
        {
            memberItemList[i].index = i;
        }
    }

    void Initialization()
    {
        for (int i = 0; i < panelContent.childCount; i++)
        {
            Destroy(panelContent.GetChild(i).gameObject);
        }
    }


    private void OnDisable()
    {
        Initialization();
    }
}
