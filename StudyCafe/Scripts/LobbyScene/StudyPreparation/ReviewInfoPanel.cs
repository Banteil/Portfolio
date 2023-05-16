using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReviewInfoPanel : MonoBehaviour
{
    GameObject questionObject;

    public Text nameText;
    public Text evidenceText;
    public Text fileNameText;
    public Transform content;

    public GameObject rejectPanel;
    public InputField reasonForRejectionInput;
    public VoidFunction resetPanel;

    string memberGUID;
    string nickName;
    string filePath;
    string fileName;
    string info;

    private void Awake()
    {
        questionObject = Resources.Load<GameObject>("Prefabs/UI/QualificationQuestionItem");
    }

    public void SetReviewInfo(MemberItem item)
    {
        StudyPreparation.Instance.interactFunc += CloseButton;
        memberGUID = item.MemberGUID;
        //이름, 답변, 토큰(파일경로), 파일명
        string[] infos = item.ReviewInfo.Split('☞');
        nickName = infos[0];
        nameText.text = "'" + nickName + "'님의 자격 요건 정보";
        filePath = infos[2];
        fileName = infos[3];
        fileNameText.text = fileName;
        evidenceText.text = StudyPreparation.Instance.studyData.eligibilityRequirements["Evidence"];

        string[] questions = StudyPreparation.Instance.studyData.eligibilityRequirements["Question"].Split(',');
        string[] answers = infos[1].Split(',');
        for (int i = 0; i < questions.Length; i++)
        {
            GameObject questionItem = Instantiate(questionObject, content, false);
            QualificationQuestionItem itemScript = questionItem.GetComponent<QualificationQuestionItem>();
            itemScript.index = i;
            itemScript.questionText.text = questions[i];
            itemScript.answerInput.text = answers[i];
            itemScript.answerInput.interactable = false;
        }
    }

    public void EvidenceDownloadButton()
    {
        string fileDir = "/../storage/data/" + StudyPreparation.Instance.studyData.guid + "/announcementfile/" + filePath + "/";
        StartCoroutine(DataManager.Instance.FileDownload(fileDir, fileName));
    }

    public void CloseButton() => gameObject.SetActive(false);

    public void DeclineToJoinButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("'" + nickName + "'님의 스터디 가입을 거절하시겠습니까?");
        CommonInteraction.Instance.confirmFunc = CheckDeclineToJoin;
    }

    void CheckDeclineToJoin(bool check)
    {
        if (check) rejectPanel.SetActive(true);
    }

    public void InputReasonCompleteButton()
    {
        if(reasonForRejectionInput.text.Equals(""))
        {
            CommonInteraction.Instance.InfoPanelUpdate("거절 사유를 입력해 주세요.");
            return;
        }

        StartCoroutine(DeclineToJoinProcess());
    }

    IEnumerator DeclineToJoinProcess()
    {
        yield return StartCoroutine(RejectParticipationApplication());
        if(info == null)
        {
            CommonInteraction.Instance.InfoPanelUpdate("멤버 가입 거절 절차에 문제가 발생헀습니다.\n다시 시도해 주세요.");
        }
        else
        {
            yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.ID, memberGUID));
            string email = DataManager.Instance.info;
            string subject = "[" + StudyPreparation.Instance.studyData.studyName + "] 스터디 가입이 거절되었습니다.";
            string content = "안녕하세요.<br>[" + StudyPreparation.Instance.studyData.studyName + "] 스터디 가입이 거절되었음을 알립니다.<br><br>"
                + "거절 사유 : " + reasonForRejectionInput.text;
            yield return StartCoroutine(DataManager.Instance.SendEMail(email, subject, content));
            CommonInteraction.Instance.InfoPanelUpdate("멤버 가입 거절 완료.");
            resetPanel?.Invoke();
            CloseButton();
        }
    }

    public void SubscriptionButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("'" + nickName + "'님의 스터디 가입을 승인하시겠습니까?");
        CommonInteraction.Instance.confirmFunc = CheckSubscription;
    }

    void CheckSubscription(bool check)
    {
        if (check) StartCoroutine(SubscriptionProcess());
    }

    IEnumerator SubscriptionProcess()
    {
        //멤버 정보 DB에 등록
        yield return StartCoroutine(DataManager.Instance.SetMemberInfo(StudyPreparation.Instance.studyData.guid, memberGUID));
        if(DataManager.Instance.info.Equals("SUCCESS"))
        {
            yield return StartCoroutine(RejectParticipationApplication());
            //멤버 가입 승인 메일 전송
            yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.ID, memberGUID));
            string email = DataManager.Instance.info;
            string subject = "[" + StudyPreparation.Instance.studyData.studyName + "] 스터디 가입이 승인되었습니다.";
            string content = "안녕하세요.<br>[" + StudyPreparation.Instance.studyData.studyName + "] 스터디 가입이 승인되었습니다!<br>"
                + "멤버가 되신 것을 축하드리며, 라운지의 스터디 참여 -> 스터디 카드 목록을 통해 가입된 스터디를 확인하시길 바랍니다.";
            yield return StartCoroutine(DataManager.Instance.SendEMail(email, subject, content));

            //멤버 가입 시 댕댕이 공지사항 추가
            Guid tempGuid = Guid.NewGuid();
            string tempToken = tempGuid.ToString();            
            string noticeContent = "안녕하세멍!\n[" + StudyPreparation.Instance.studyData.studyName + "] 스터디에 새로운 멤버인 [" + nickName + "]님이 가입했어멍!\n댓글로 새로 가입한 멤버를 환영해줘멍!";
            yield return StartCoroutine(DataManager.Instance.AddNotice(StudyPreparation.Instance.studyData.guid, tempToken, "system", noticeContent, ""));

            CommonInteraction.Instance.InfoPanelUpdate("멤버 가입 승인 완료!");
            resetPanel?.Invoke();
            CloseButton();
        }
        else
        {
            CommonInteraction.Instance.InfoPanelUpdate("멤버 가입 승인 절차에 문제가 발생헀습니다.\n다시 시도해 주세요.");
        }
    }

    void Initialization()
    {
        reasonForRejectionInput.text = "";
        rejectPanel.SetActive(false);
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    private void OnDisable() => Initialization();

    #region 멤버 정보 GET 코루틴
    public IEnumerator RejectParticipationApplication()
    {
        string rejectMemberUrl = "https://stubugs.com/php/rejectparticipationapplication.php";
        WWWForm form = new WWWForm();
        form.AddField("Token", filePath);
        form.AddField("StudyGUID", StudyPreparation.Instance.studyData.guid);
        form.AddField("UserGUID", memberGUID);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(rejectMemberUrl, form))
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

}
