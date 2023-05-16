using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StudyRecruitment : Singleton<StudyRecruitment>
{
    public Transform content;
    public GameObject announcementInfoPanel;
    public GameObject eligibilityRequirementsPanel;
    List<StudyCard> studyCardList = new List<StudyCard>();

    private void OnEnable()
    {
        //오브젝트를 통해 입장할 수 있으므로 커서 기본으로 초기화
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        StartCoroutine(CheckStudyAnnouncementInfo());
    }

    /// <summary>
    /// 공고 올린 스터디 정보들을 받아오는 코루틴
    /// </summary>
    IEnumerator CheckStudyAnnouncementInfo()
    {
        string checkAnnouncementUrl = "https://stubugs.com/php/checkannouncement.php";

        WWWForm form = new WWWForm();
        form.AddField("GUID", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(checkAnnouncementUrl, form))
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
                        CommonInteraction.Instance.InfoPanelUpdate("스터디 모집 공고 확인 중 문제가 발생했습니다.");
                        CloseButton();
                        break;
                    case CheckSuccessed.SUCCESS:
                        CreateListOfAnnouncements(check[1]);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 전달받은 StudyInfoData 직렬화 string 정보를 토대로 공고문 전용 스터디 카드를 생성하는 함수
    /// </summary>
    void CreateListOfAnnouncements(string data)
    {
        string[] deserializationStudyData = data.Split('☞');
        for (int i = 0; i < deserializationStudyData.Length - 1; i++)
        {
            StudyInfoData infoData = JsonConvert.DeserializeObject<StudyInfoData>(deserializationStudyData[i]);
            GameObject card = Instantiate(LobbyManager.Instance.studyCardObj, content, false);
            StudyCard cardInfo = card.GetComponent<StudyCard>();
            cardInfo.StudyData = infoData;
            cardInfo.IsAnnouncement = true;
            studyCardList.Add(cardInfo);
        }
    }

    public void OpenAnnouncementInfoPanel(StudyInfoData data)
    {
        announcementInfoPanel.GetComponent<AnnouncementInfo>().SetStudyInfo(data);
        announcementInfoPanel.SetActive(true);
    }

    /// <summary>
    /// 스터디 모집 창 종료
    /// </summary>
    public void CloseButton()
    {
        if (eligibilityRequirementsPanel.activeSelf)
            eligibilityRequirementsPanel.SetActive(false);
        else if (announcementInfoPanel.activeSelf)
            announcementInfoPanel.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    /// <summary>
    /// 상태 초기화 함수
    /// </summary>
    public void Initialization()
    {
        studyCardList = new List<StudyCard>();
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    private void OnDisable()
    {
        Initialization();
    }
}
