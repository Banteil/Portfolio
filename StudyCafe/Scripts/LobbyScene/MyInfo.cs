using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MyInfo : MonoBehaviour
{
    [Header("MyInfoUI")]
    public Text modifyButtonText;
    public InputField nickNameInput;
    public AvatarImage avatarImage;

    [Header("AvatarModifyUI")]
    public Texture2D cursorImage;
    public GameObject avatarModifyPanel;
    public SelectAvatarParts selectAvatarInfo;

    bool isFirst = true;
    string saveAvatarInfo;

    void OnEnable()
    {
        if (isFirst)
        {
            StartCoroutine(SettingInfo());
            selectAvatarInfo.AddPartsList();            
            isFirst = false;
        }
        StartCoroutine(SettingImage());
        InputControl.Instance.cancel = CloseButton;
        InputControl.Instance.preventMenuOperation = true;
    }

    /// <summary>
    /// 유저의 정보대로 내용을 세팅하는 코루틴
    /// </summary>
    IEnumerator SettingInfo()
    {
        string guid = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.NICKNAME, guid));
        nickNameInput.text = DataManager.Instance.info;        
    }

    IEnumerator SettingImage()
    {
        string guid = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.AVATAR, guid));
        string avatarInfo = DataManager.Instance.info;
        avatarImage.SetInfo(avatarInfo);
        saveAvatarInfo = avatarInfo;
    }

    #region UI 조작

    public void CloseButton() => gameObject.SetActive(false);

    public void ModifyAvatarInfoPanelButton()
    {
        if (!avatarModifyPanel.activeSelf)
        {
            avatarModifyPanel.SetActive(true);            
        }
        else
        {
            avatarModifyPanel.SetActive(false);
            avatarImage.SetInfo(saveAvatarInfo);
        }
    }

    public void ModifyAvatarButton()
    {
        StartCoroutine(ModifyAvatarInfo());
    }

    #endregion

    /// <summary>
    /// 수정한 유저 정보로 업데이트하는 코루틴
    /// </summary>
    public IEnumerator ModifyAvatarInfo()
    {
        string avatarInfo = avatarImage.GetPartsInfo();

        string modifyAvatarInfoUrl = "https://stubugs.com/php/modifyavatarinfo.php";
        WWWForm form = new WWWForm();
        form.AddField("UniqueNumber", (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"]);
        form.AddField("AvatarInfo", avatarInfo);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(modifyAvatarInfoUrl, form))
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
                        CommonInteraction.Instance.InfoPanelUpdate("정보 수정에 실패했습니다.");
                        break;
                    case CheckSuccessed.SUCCESS:                        
                        InputControl.Instance.enterKey = null;
                        InputControl.Instance.cancel = CloseButton;                        
                        DataManager.Instance.SetPlayerProperties("AvatarInfo", avatarInfo);
                        LobbyManager.Instance.myAvatar.GetComponent<AvatarAct>().SettingAvatarImage(avatarInfo);
                        saveAvatarInfo = avatarInfo;
                        avatarModifyPanel.SetActive(false);
                        CommonInteraction.Instance.InfoPanelUpdate("아바타 갈아입기 완료!");
                        break;
                }
            }
        }
    }

    #region 마우스 커서 조작
    public void MouseEnter()
    {
        Vector2 cursorHotspot = new Vector2(cursorImage.width / 2, cursorImage.height / 2);
        Cursor.SetCursor(cursorImage, cursorHotspot, CursorMode.Auto);
    }


    public void MouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    #endregion

    private void OnDisable()
    {
        avatarModifyPanel.SetActive(false);
        InputControl.Instance.enterKey = null;
        InputControl.Instance.cancel = LobbyManager.Instance.LogoutButton;
        InputControl.Instance.preventMenuOperation = false;
    }
}
