using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InviteMember : MonoBehaviour
{
    public Text inviteCodeText;
    public InputField emailInput;
    public Transform content;

    List<MemberIDItem> mailList = new List<MemberIDItem>(); 

    void OnEnable()
    {
        inviteCodeText.text = "초대코드 : " + DataManager.loadData.code;
    }

    public void AddEmailButton()
    {
        if (!CommonInteraction.Instance.IsValidEmail(emailInput.text))
        {
            CommonInteraction.Instance.InfoPanelUpdate("이메일 양식에 맞춰 입력해 주세요.\n예시) abcd1234@gmail.com");
            return;
        }

        if(!CommonInteraction.Instance.LimitEmailID(emailInput.text))
        {
            CommonInteraction.Instance.InfoPanelUpdate("이메일로 인식할 수 없는 글자 길이입니다.\n다시 입력해 주세요.");
            return;
        }

        GameObject emailObject = Instantiate(Resources.Load<GameObject>("Prefabs/UI/InviteMemberEmailItem"), content, false);
        MemberIDItem emailScript = emailObject.GetComponent<MemberIDItem>();
        emailScript.mailText.text = emailInput.text;
        emailScript.index = mailList.Count;
        emailScript.invite = this;
        mailList.Add(emailScript);
        emailInput.text = "";
        emailInput.ActivateInputField();
    }

    public void InviteMemberButton()
    {
        if (mailList.Count.Equals(0))
        {
            CommonInteraction.Instance.InfoPanelUpdate("초대 목록에 추가된 메일이 없습니다!");
            return;
        }

        StartCoroutine(SendInviteMailProcess());
    }

    IEnumerator SendInviteMailProcess()
    {        
        for (int i = 0; i < mailList.Count; i++)
        {
            string subject = "[" + DataManager.loadData.roomName + "] 스터디 초대 코드 입니다.";
            string content = "안녕하세요.<br>삼삼오오의 [" + DataManager.loadData.roomName + "] 스터디에서 보낸 초대 메세지입니다." +
                "<br><br><b>초대 코드 : " + DataManager.loadData.code + "</b>";
            yield return StartCoroutine(DataManager.Instance.SendEMail(mailList[i].mailText.text, subject, content));
        }

        CommonInteraction.Instance.InfoPanelUpdate("초대 코드 전송이 완료되었습니다!");
        CancelButton();
    }

    public void CancelButton() => gameObject.SetActive(false);

    public void DeleteMail(int index)
    {
        Destroy(mailList[index].gameObject);
        mailList.RemoveAt(index);
        for (int i = 0; i < mailList.Count; i++)
        {
            mailList[i].index = i;
        }
    }

    private void OnDisable()
    {
        emailInput.text = "";
        mailList = new List<MemberIDItem>();
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

}
