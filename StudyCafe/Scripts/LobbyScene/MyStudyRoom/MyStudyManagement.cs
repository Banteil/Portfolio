using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 마스터가 준비한 스터디 목록을 체크, 표시 및 관리하는 역할을 담당하는 클래스
/// </summary>
public class MyStudyManagement : Singleton<MyStudyManagement>
{
    public GameObject studyListPanel;
    public GameObject studyPreparationPanel;
    public GameObject assignmentFileListPanel;
    public Transform content;
    public List<StudyCard> studyCardList = new List<StudyCard>();
    StudyCard selectedCard;
    string myStudyListStr;

    void OnEnable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        InputControl.Instance.preventMenuOperation = true;
        StartCoroutine(MyStudyListSetting());
    }

    ///<summary>
    ///특정 대상의 스터디 정보를 반환 시 사용
    ///</summary>
    public IEnumerator GetMyStudy()
    {
        string getMyStudyUrl = "https://stubugs.com/php/getmystudy.php";
        WWWForm form = new WWWForm();
        form.AddField("UserGUID", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(getMyStudyUrl, form))
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
                        Debug.Log("스터디 정보 반환 실패");
                        myStudyListStr = null;
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("스터디 정보 반환 성공");
                        myStudyListStr = check[1];
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 활성화 시 플레이어의 스터디 정보를 Get, 정보를 토대로 리스트에 스터디 카드를 배치함
    /// </summary>
    IEnumerator MyStudyListSetting()
    {
        CommonInteraction.Instance.StartLoding();
        //현재 플레이어의 고유 정보로 DB에 저장된 정보를 획득, info에 정보를 임시 저장
        yield return StartCoroutine(GetMyStudy());
        //정보가 있다면 해당 정보를 토대로 스터디List 정보 세팅
        if (myStudyListStr != null)
        {
            string[] list = myStudyListStr.Split('☞');
            for (int i = 0; i < list.Length - 1; i++)
            {
                StudyInfoData infoData = JsonConvert.DeserializeObject<StudyInfoData>(list[i]);
                GameObject card = Instantiate(LobbyManager.Instance.studyCardObj, content, false);
                StudyCard cardInfo = card.GetComponent<StudyCard>();
                cardInfo.StudyData = infoData;
                
                //스터디 마스터 정보 반환받음
                yield return StartCoroutine(DataManager.Instance.GetStudyCompositionInfo(StudyComposition.MASTER, infoData.guid));
                string masterInfo = DataManager.Instance.info;
                //스터디 마스터가 나라면 카드에 IsMine 필터 추가
                if (masterInfo.Equals((string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]))
                    cardInfo.IsMine = true;
                else
                    cardInfo.IsMine = false;

                studyCardList.Add(cardInfo);
            }
        }

        InputControl.Instance.cancel = CloseButton;
        CommonInteraction.Instance.isLoading = false;
    }

    /// <summary>
    /// 스터디 참여 종료
    /// </summary>
    public void CloseButton()
    {
        gameObject.SetActive(false);
        InputControl.Instance.cancel = LobbyManager.Instance.LogoutButton;
        InputControl.Instance.preventMenuOperation = false;
    }

    /// <summary>
    /// 스터디 정보 수정 시작 시 호출하는 함수, 선택한 카드 정보 대입 및 수정 스크립트에 정보 전달, 수정 panel 오픈
    /// </summary>
    public void StartStudyInfoView(StudyCard selectCard)
    {
        //정보 수정 후 카드 정보도 업데이트 하기 위해 selectedCard에 미리 저장해둠
        selectedCard = selectCard;
        studyPreparationPanel.GetComponent<StudyPreparation>().studyData = selectCard.StudyData;
        studyPreparationPanel.SetActive(true);
    }

    /// <summary>
    /// 스터디 정보 수정 완료 시 호출하는 함수, 정보 수정 완료된 카드의 정보를 업데이트 함
    /// </summary>
    public void EndStudyModify(StudyInfoData info)
    {
        selectedCard.StudyData = info;
        selectedCard = null;
    }

    /// <summary>
    /// 스터디 정보 수정 시작 시 호출하는 함수, 선택한 카드 정보 대입 및 수정 스크립트에 정보 전달, 수정 panel 오픈
    /// </summary>
    public void StartAssignmentUpload(StudyCard selectCard)
    {
        assignmentFileListPanel.GetComponent<AssignmentFileListPanel>().studyGUID = selectCard.StudyData.guid;
        string[] curriculumDate = selectCard.dateDropdown.captionText.text.Split('\n');
        assignmentFileListPanel.GetComponent<AssignmentFileListPanel>().CurriculumDate = curriculumDate[0] + curriculumDate[1] + curriculumDate[2];
        assignmentFileListPanel.SetActive(true);
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

    void OnDisable()
    {
        Initialization();
    }
}
