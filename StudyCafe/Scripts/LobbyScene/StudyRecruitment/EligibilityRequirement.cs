using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class EligibilityRequirement : MonoBehaviour
{
    public Transform questionContent;
    public Text explainingEvidenceText;
    public Text genderInfoText;
    public Text fileNameText;
    List<QualificationQuestionItem> questionItems = new List<QualificationQuestionItem>();
    string studyGUID;
    string studyName;
    string evidenceFileName = "";
    string filePath;

    private void OnEnable()
    {
        Guid guid = Guid.NewGuid();
        filePath = guid.ToString();
    }

    public void SetEligibilityRequirementInfo(StudyInfoData data)
    {
        studyGUID = data.guid;
        studyName = data.studyName;
        if (!data.eligibilityRequirements["Question"].Equals(""))
        {
            string[] questions = data.eligibilityRequirements["Question"].Split(',');
            for (int i = 0; i < questions.Length; i++)
            {
                GameObject item = Instantiate(Resources.Load<GameObject>("Prefabs/UI/QualificationQuestionItem"), questionContent, false);
                QualificationQuestionItem itemScript = item.GetComponent<QualificationQuestionItem>();
                itemScript.index = i;
                itemScript.questionText.text = (i + 1) + ". " + questions[i];
                questionItems.Add(itemScript);
            }
        }
        explainingEvidenceText.text = data.eligibilityRequirements["Evidence"];
        genderInfoText.text = data.eligibilityRequirements["Gender"];
    }

    public void UploadEvidenceFile()
    {
        DataManager.isTempFileUpload = true;
        DataManager.interactionData.type = filePath;
        DataManager.Instance.OpenFileDialogButtonOnClickHandler();
        DataManager.Instance.stringTransfer = SetFileInfo;
    }

    string GetAnswerInfo()
    {
        Debug.Log(questionItems.Count);
        string answer = "";
        for (int i = 0; i < questionItems.Count; i++)
        {
            Debug.Log(questionItems[i].answerInput.text);
            answer += questionItems[i].answerInput.text;
            if (i < questionItems.Count - 1)
                answer += ",";
        }

        return answer;
    }

    void SetFileInfo(string info)
    {
        evidenceFileName = info;
        fileNameText.text = info;
    }

    /// <summary>
    /// 멤버 신청 완료 버튼
    /// </summary>
    public void ApplicationCompletedButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("'" + studyName + "' 스터디 멤버 신청을 완료하시겠습니까?");
        CommonInteraction.Instance.confirmFunc = ApplicationCompletedCheck;
    }

    void ApplicationCompletedCheck(bool check)
    {
        if(check)
            StartCoroutine(ParticipationApplication());
    }

    /// <summary>
    /// 공고 정보 전달 루틴
    /// </summary>
    public IEnumerator ParticipationApplication()
    {
        Debug.Log(filePath);
        Debug.Log(evidenceFileName);
        string participationApplicationUrl = "https://stubugs.com/php/participationapplication.php";
        WWWForm form = new WWWForm();
        form.AddField("StudyGUID", studyGUID);
        form.AddField("UserGUID", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);
        form.AddField("Answer", GetAnswerInfo());
        form.AddField("FileName", evidenceFileName);
        form.AddField("FilePath", filePath);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(participationApplicationUrl, form))
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
                        CommonInteraction.Instance.InfoPanelUpdate("멤버 신청 실패.\n다시 시도해 주세요.");
                        break;
                    case CheckSuccessed.SUCCESS:
                        yield return StartCoroutine(DataManager.Instance.GetStudyCompositionInfo(StudyComposition.MASTER, studyGUID));
                        string masterGUID = DataManager.Instance.info;
                        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.ID, masterGUID));
                        string email = DataManager.Instance.info;
                        string myName = (string)PhotonNetwork.LocalPlayer.CustomProperties["NickName"];
                        string subject = "[" + myName + "] 스터디 참가 신청";
                        string content = "안녕하세요.<br>[" + myName + "]님께서 스터디 멤버가 되길 원해요!" +
                            "<br>로그인 후 심사를 해주세요.<br><br>[스터디 참여 -> 내 스터디 카드의 정보 -> 멤버 -> 멤버 심사]";
                        yield return StartCoroutine(DataManager.Instance.SendEMail(email, subject, content));
                        CommonInteraction.Instance.InfoPanelUpdate("멤버 신청 완료!\n승인 결과는 메일로 알려드립니댕. 깜빡하지 말아주세요.");
                        gameObject.SetActive(false);
                        break;
                }
            }
        }
    }

    void Initialization()
    {
        DataManager.Instance.DeleteTempFileProcess(filePath);
        fileNameText.text = "(증빙 자료가 여러개일 경우 zip 등으로 압축하여 업로드 해주세요.";
        explainingEvidenceText.text = "";
        genderInfoText.text = "";
        evidenceFileName = "";
        questionItems = new List<QualificationQuestionItem>();
        for (int i = 0; i < questionContent.childCount; i++)
        {
            Destroy(questionContent.GetChild(i).gameObject);
        }
    }

    private void OnDisable()
    {
        Initialization();
    }
}
