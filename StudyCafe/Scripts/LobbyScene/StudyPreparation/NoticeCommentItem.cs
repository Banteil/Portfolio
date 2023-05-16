using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NoticeCommentItem : MonoBehaviour
{
    const int body = 0;
    const int eyes = 1;
    const int top = 2;
    const int hair = 3;

    public string WriterGUID
    {
        set
        {
            string playerGUID = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
            if (playerGUID.Equals(value))
            {
                modifyButtonObject.SetActive(true);
                deleteButtonObject.SetActive(true);
            }
            else
            {
                modifyButtonObject.SetActive(false);
                deleteButtonObject.SetActive(false);
            }            
        }
    }
    [HideInInspector]
    public string token;

    public GameObject commentModifyInputObject;
    public GameObject modifyButtonObject;
    public GameObject deleteButtonObject;
    public NoticeContentItem noticeItem;
    public Image[] faceImages;
    public Text nameText;
    public Text dateText;
    public Text commentText;
    public InputField commentInput;

    public delegate void DeleteFuction(NoticeCommentItem commentItem);
    public DeleteFuction delFuction;

    public void DeleteCommentButton() => StartCoroutine(DeleteCommentInfo());

    public void ModifyCommentButton()
    {
        if (!commentInput.gameObject.activeSelf)
        {
            commentInput.text = commentText.text;
            commentText.gameObject.SetActive(false);
            commentInput.gameObject.SetActive(true);
        }
        else
        {
            commentInput.text = "";
            commentText.gameObject.SetActive(true);
            commentInput.gameObject.SetActive(false);
        }
    }

    public void CommentModifyCompleteButton() => StartCoroutine(ModifyCommentInfo());

    public IEnumerator SettingUserInfo(string guid)
    {
        if (!guid.Equals("system"))
        {
            yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.NICKNAME, guid));
            string name = DataManager.Instance.info;
            yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.AVATAR, guid));
            string avatarInfo = DataManager.Instance.info;
            nameText.text = name;
            SetFaceImage(avatarInfo);
        }
    }

    void SetFaceImage(string avatarInfo)
    {
        string[] partsInfo = avatarInfo.Split(',');

        for (int i = 0; i < partsInfo.Length; i++)
        {
            Sprite[] sprites;
            string[] info = partsInfo[i].Split('_');
            string parts = info[0];
            int num = int.Parse(info[1]);

            switch (parts)
            {
                case "h":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Hair/hair_" + num);
                    if (!sprites.Length.Equals(0))
                        faceImages[hair].sprite = sprites[0];
                    break;
                case "e":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + num);
                    if (!sprites.Length.Equals(0))
                        faceImages[eyes].sprite = sprites[0];
                    break;
                case "t":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num);
                    if (!sprites.Length.Equals(0))
                        faceImages[top].sprite = sprites[0];
                    break;
                case "sk":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num);
                    if (!sprites.Length.Equals(0))
                        faceImages[body].sprite = sprites[0];
                    break;
                default:
                    continue;
            }
        }
    }

    public IEnumerator ModifyCommentInfo()
    {
        string noticeCommentInfoUrl = "https://stubugs.com/php/noticecommentinfo.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.UPDATE);
        form.AddField("Token", token);
        form.AddField("Content", commentInput.text);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(noticeCommentInfoUrl, form))
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
                        CommonInteraction.Instance.InfoPanelUpdate("댓글 수정에 실패하였습니다.");
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("댓글 수정 성공");
                        commentText.text = commentInput.text;
                        ModifyCommentButton();
                        break;
                }
            }
        }
    }

    public IEnumerator DeleteCommentInfo()
    {
        string noticeCommentInfoUrl = "https://stubugs.com/php/noticecommentinfo.php";
        WWWForm form = new WWWForm();
        form.AddField("Purpose", (int)Purpose.DEL);
        form.AddField("Token", token);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(noticeCommentInfoUrl, form))
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
                        CommonInteraction.Instance.InfoPanelUpdate("댓글 삭제에 실패하였습니다.");
                        break;
                    case CheckSuccessed.SUCCESS:
                        Debug.Log("댓글 삭제 성공");
                        delFuction(this);
                        break;
                }
            }
        }
    }
}
