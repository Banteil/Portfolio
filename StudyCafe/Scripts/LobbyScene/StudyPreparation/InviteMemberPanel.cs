using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InviteMemberPanel : MonoBehaviour
{
    public InputField emailInputField;
    public Button inviteButton;
    bool isFirstEnable = true;

    private void OnEnable()
    {
        if (isFirstEnable)
        {
            StudyPreparation.Instance.interactFunc += Initialization;
            SettingMemberInfo();
            isFirstEnable = false;
        }
    }

    void SettingMemberInfo()
    {
        if (!StudyPreparation.Instance.powerToEdit[3])
            NonEditableProcess();
    }

    ///<summary>
    ///수정 권한 없을 때 처리
    ///</summary>
    void NonEditableProcess()
    {
        inviteButton.interactable = false;
    }

    public void InviteButton()
    {
        if (!CheckException()) return;

        CommonInteraction.Instance.ConfirmPanelUpdate("'" + emailInputField.text + "이메일로 스터디 초대 코드를 발송하시겠습니까?");
        CommonInteraction.Instance.confirmFunc = InviteCheck;
    }

    void InviteCheck(bool check)
    {
        if(check) StartCoroutine(MemberInviteProcess());
    }

    IEnumerator MemberInviteProcess()
    {
        string subject = "[" + StudyPreparation.Instance.studyData.studyName + "] 스터디 초대 코드 입니다.";
        string content = "안녕하세요.<br>삼삼오오의 [" + StudyPreparation.Instance.studyData.studyName + "] 스터디에서 보낸 초대 메세지입니다." +
            "<br><br><b>초대 코드 : " + StudyPreparation.Instance.studyData.code + "</b>";
        yield return StartCoroutine(DataManager.Instance.SendEMail(emailInputField.text, subject, content));

        emailInputField.text = "";
        CommonInteraction.Instance.InfoPanelUpdate("초대 코드 전송이 완료되었습니다!");
    }

    void Initialization()
    {
        emailInputField.text = "";
        inviteButton.interactable = true;
        isFirstEnable = true;
    }

    #region 유효성 검사
    public bool CheckException()
    {
        if (CommonInteraction.Instance.IsValidEmail(emailInputField.text))
        {
            CommonInteraction.Instance.InfoPanelUpdate("이메일 양식에 맞춰 입력해 주세요.\n예시) abcd1234@gmail.com");
            return false;
        }

        if (CommonInteraction.Instance.LimitEmailID(emailInputField.text))
        {
            CommonInteraction.Instance.InfoPanelUpdate("이메일로 인식할 수 없는 글자 길이입니다.\n다시 입력해 주세요.");
            return false;
        }

        return true;
    }

    #endregion

    private void OnDisable()
    {
        Initialization();
    }
}
