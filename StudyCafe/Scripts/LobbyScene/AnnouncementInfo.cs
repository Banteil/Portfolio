using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AnnouncementInfo : MonoBehaviour
{
    public InputField studyNameInput;
    public InputField studySubjectInput;
    public InputField startDateInput, endDateInput;
    public InputField averageNumInput, maxNumInput;
    public Dropdown unitTimeDropdown;

    public InputField tardyNumInput;
    public Dropdown tardyPenaltyDropdown;
    public InputField absentNumInput;
    public Dropdown absentPenaltyDropdown;
    public InputField assignmentNumInput;
    public Dropdown assignmentPenaltyDropdown;

    public Transform content;

    public GameObject announcementPanel;
    public Text announcementDescriptionText;
    public Toggle[] announcementToggles;

    StudyInfoData studyData;

    bool isParticipated;

    public void SetStudyInfo(StudyInfoData data)
    {
        studyData = data;
        //이름, 주제 대입
        studyNameInput.text = data.studyName;
        studySubjectInput.text = data.subject;
        //스터디 목표 대입
        //ex) 2010-01-01,2010-01-02,10,0,5
        string[] objective = data.objectives.Split(',');
        startDateInput.text = objective[0];
        endDateInput.text = objective[1];
        averageNumInput.text = objective[2];
        unitTimeDropdown.value = int.Parse(objective[3]);
        maxNumInput.text = objective[4];
        //규칙 대입
        string[] tardy = data.rules["Tardy"].Split(',');
        tardyNumInput.text = tardy[0];
        tardyPenaltyDropdown.value = int.Parse(tardy[1]);
        string[] absent = data.rules["Absent"].Split(',');
        absentNumInput.text = absent[0];
        absentPenaltyDropdown.value = int.Parse(absent[1]);
        string[] assignment = data.rules["Assignment"].Split(',');
        assignmentNumInput.text = assignment[0];
        assignmentPenaltyDropdown.value = int.Parse(assignment[1]);
        //커리큘럼 정보 대입
        for (int i = 0; i < data.curriculumInfoList.Count; i++)
        {
            GameObject curriculumItem = Instantiate(Resources.Load<GameObject>("Prefabs/UI/CurriculumItem"), content, false);
            CurriculumItem itemScript = curriculumItem.GetComponent<CurriculumItem>();
            itemScript.CurriculumInfomation = data.curriculumInfoList[i];            
            itemScript.ListIndex = i;
            itemScript.itemInfoFunction = ViewCurriculumInformation;
        }
    }

    /// <summary>
    /// 스터디 참가 버튼
    /// </summary>
    public void StudyParticipationButton() => StartCoroutine(CheckParticipation());

    IEnumerator CheckParticipation()
    {
        yield return StartCoroutine(CheckParticipationApplication());
        if(isParticipated)
        {
            isParticipated = false;
            CommonInteraction.Instance.InfoPanelUpdate("이미 참가 신청이 완료된 스터디입니다.\n심사가 끝날 때 까지 기다려주세요.");
            yield break;
        }

        EligibilityRequirement eligibility = StudyRecruitment.Instance.eligibilityRequirementsPanel.GetComponent<EligibilityRequirement>();
        eligibility.SetEligibilityRequirementInfo(studyData);
        StudyRecruitment.Instance.eligibilityRequirementsPanel.SetActive(true);
    }

    /// <summary>
    /// 공고 정보 전달 루틴
    /// </summary>
    public IEnumerator CheckParticipationApplication()
    {
        string checkparticipationApplicationUrl = "https://stubugs.com/php/checkparticipationapplication.php";
        WWWForm form = new WWWForm();
        form.AddField("StudyGUID", studyData.guid);
        form.AddField("UserGUID", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(checkparticipationApplicationUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                //echo로 전달된 텍스트를 ; 문자 기준으로 split
                string[] check = webRequest.downloadHandler.text.Split(';');

                CheckSuccessed checkSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (checkSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        isParticipated = true;
                        break;
                    case CheckSuccessed.SUCCESS:
                        isParticipated = false;
                        break;
                }
            }
        }
    }

    void ViewCurriculumInformation(int index)
    {
        announcementDescriptionText.text = studyData.curriculumInfoList[index].description;
        string[] splitInfo = studyData.curriculumInfoList[index].announcementInfo.Split(',');
        for (int i = 0; i < splitInfo.Length; i++)
        {
            if (splitInfo[i].Equals("T"))
                announcementToggles[i].isOn = true;
            else
                announcementToggles[i].isOn = false;
        }
        announcementPanel.SetActive(true);
    }

    public void CloseCurriculumInformation()
    {
        announcementDescriptionText.text = "";
        for (int i = 0; i < announcementToggles.Length; i++)
        {
            announcementToggles[i].isOn = true;
        }
        announcementPanel.SetActive(false);
    }

    private void OnDisable()
    {
        studyData = null;
        studyNameInput.text = "";
        studySubjectInput.text = "";
        startDateInput.text = "";
        endDateInput.text = "";
        averageNumInput.text = "";
        unitTimeDropdown.value = 0;
        maxNumInput.text = "";
        tardyNumInput.text = "";
        tardyPenaltyDropdown.value = 0;
        absentNumInput.text = "";
        absentPenaltyDropdown.value = 0;
        assignmentNumInput.text = "";
        assignmentPenaltyDropdown.value = 0;
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}
