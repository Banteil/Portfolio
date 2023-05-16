using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class InviteCodePanel : MonoBehaviour
{
    public InputField joinCodeInput;

    private void OnEnable()
    {
        joinCodeInput.ActivateInputField();
        InputControl.Instance.preventMenuOperation = true;
    }

    #region 초대 코드
    public void CloseCodePanelButton()
    {
        gameObject.SetActive(false);
        InputControl.Instance.preventMenuOperation = false;
    }

    public void InputInviteCodeButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("입력하신 '" + joinCodeInput.text + "' 코드를 제출하시겠습니까?");
        CommonInteraction.Instance.confirmFunc = InputInviteCodeCheck;
    }

    void InputInviteCodeCheck(bool check)
    {
        if (check)
            StartCoroutine(InputInviteCode());
    }

    IEnumerator InputInviteCode()
    {
        string inputInviteCodeUrl = "https://stubugs.com/php/inputinvitecode.php";
        WWWForm form = new WWWForm();
        form.AddField("Code", joinCodeInput.text);
        form.AddField("MemberGUID", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(inputInviteCodeUrl, form))
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
                        if (check[1].Equals("nostudy"))
                            CommonInteraction.Instance.InfoPanelUpdate("입력하신 코드와 매칭되는 스터디가 없습니다.");
                        else if(check[1].Equals("alreadymember"))
                            CommonInteraction.Instance.InfoPanelUpdate("이미 해당 코드의 스터디에 가입되어 있습니다.");
                        else
                            CommonInteraction.Instance.InfoPanelUpdate("스터디 코드 가입에 실패하였습니다.\n다시 시도해 주세요.");
                        break;
                    case CheckSuccessed.SUCCESS:
                        //멤버 가입 시 댕댕이 공지사항 추가
                        Guid tempGuid = Guid.NewGuid();
                        string tempToken = tempGuid.ToString();
                        string nickName = (string)PhotonNetwork.LocalPlayer.CustomProperties["NickName"];
                        string noticeContent = "안녕하세멍!\n[" + check[1] + "] 스터디에 새로운 멤버인 [" + nickName + "]님이 가입했어멍!\n댓글로 새로 가입한 멤버를 환영해줘멍!";
                        yield return StartCoroutine(DataManager.Instance.AddNotice(check[2], tempToken, "system", noticeContent, ""));

                        CommonInteraction.Instance.InfoPanelUpdate("'" + check[1] + "' 스터디의 멤버 가입에 성공하였습니다!\n"
                            + "라운지의 스터디 참여 -> 스터디 카드 목록을 통해 가입된 스터디를 확인하시길 바랍니다.");
                        gameObject.SetActive(false);
                        break;
                }
            }
        }
    }

    #endregion
    private void OnDisable()
    {
        joinCodeInput.text = "";
        InputControl.Instance.preventMenuOperation = false;
    }
}
