using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MemberReviewPanel : MonoBehaviour
{
    GameObject memberItemObject;

    public Transform panelContent;
    public GameObject reviewInfoPanel;
    List<MemberItem> memberItemList = new List<MemberItem>();

    string info;

    private void Awake()
    {
        memberItemObject = Resources.Load<GameObject>("Prefabs/UI/MemberItem");
    }

    private void OnEnable()
    {
        StartCoroutine(SettingMemberReviewInfo());
    }

    IEnumerator SettingMemberReviewInfo()
    {
        CommonInteraction.Instance.StartLoding();

        yield return StartCoroutine(GetMemberReviewInfo(StudyPreparation.Instance.studyData.guid));
        if (info == null)
        {
            CommonInteraction.Instance.InfoPanelUpdate("멤버 심사 정보 획득 중 오류가 발생했습니다.\n다시 시도해 주세요.");
            CommonInteraction.Instance.isLoading = false;
            yield break;
        }
        else
        {
            string[] members = info.Split('¶');
            for (int i = 0; i < members.Length - 1; i++)
            {                                
                GameObject item = Instantiate(memberItemObject, panelContent, false);
                MemberItem itemScript = item.GetComponent<MemberItem>();
                itemScript.index = i;
                itemScript.ReviewInfo = members[i];
                if (StudyPreparation.Instance.powerToEdit[3])
                    itemScript.infoDisplayFunction = OpenReviewInfoPanel;
                memberItemList.Add(itemScript);
            }
        }

        CommonInteraction.Instance.isLoading = false;
    }

    void OpenReviewInfoPanel(int index)
    {
        reviewInfoPanel.SetActive(true);
        ReviewInfoPanel script = reviewInfoPanel.GetComponent<ReviewInfoPanel>();
        script.resetPanel = ResetContent;
        script.SetReviewInfo(memberItemList[index]);
    }

    void Initialization()
    {
        for (int i = 0; i < panelContent.childCount; i++)
        {
            Destroy(panelContent.GetChild(i).gameObject);
        }    
    }

    void ResetContent()
    {
        Initialization();
        StartCoroutine(SettingMemberReviewInfo());
    }

    #region 유효성 검사
    public bool CheckException()
    {
        

        return true;
    }

    #endregion

    #region 멤버 정보 GET 코루틴
    public IEnumerator GetMemberReviewInfo(string studyGUID)
    {
        string getMemberReviewUrl = "https://stubugs.com/php/getmemberreviewinfo.php";
        WWWForm form = new WWWForm();
        form.AddField("StudyGUID", studyGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(getMemberReviewUrl, form))
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

    private void OnDisable()
    {
        Initialization();
    }
}
