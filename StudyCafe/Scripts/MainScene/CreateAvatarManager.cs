using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CreateAvatarManager : MonoBehaviourPunCallbacks
{
    [Header("Avatar Panel")]
    public GameObject avatarCreatePanel;
    public InputField nickNameInputField;
    public GameObject confirmButton;
    public SelectAvatarParts avatarBodyParts, avatarClothParts;
    public AvatarImage avatarImage;

    string createAvatarUrl;
    //유효성 검사용 bool
    bool validityFailCheck;
    //아바타 이동 연출 여부 확인(초기화 안됨, 1번만 실행되도록)
    bool isAvatardirectingEnd;

    void Start()
    {
        createAvatarUrl = "https://stubugs.com/php/createavatar.php";
        StartAvatarCreate();
    }

    #region 아바타 생성 프로세스
    void StartAvatarCreate()
    {
        //가입 정보 삽입
        avatarImage.SetInfo();
        avatarBodyParts.AddPartsList();
        avatarClothParts.AddPartsList();
        avatarCreatePanel.SetActive(true);
        if (!isAvatardirectingEnd)
        {
            avatarImage.AvatarMove();
            isAvatardirectingEnd = true;
        }
        else
        {
            InputControl.Instance.cancel = null;
            InputControl.Instance.enterKey = ConfirmCreateAvatarButton;
        }
    }

    public void DisplayCreateAvatarMenu()
    {
        avatarBodyParts.gameObject.SetActive(true);
        avatarClothParts.gameObject.SetActive(true);
        nickNameInputField.gameObject.SetActive(true);
        confirmButton.SetActive(true);
    }

    /// <summary>
    /// 닉네임 유효성 체크
    /// </summary>
    IEnumerator NickNameInfoValidation()
    {
        if (!CommonInteraction.Instance.IsValidNickName(nickNameInputField.text))
        {
            validityFailCheck = true;
            CommonInteraction.Instance.InfoPanelUpdate("최소 1글자, 최대 8글자의 닉네임만 입력하실 수 있습니다.");
            nickNameInputField.ActivateInputField();
            yield break;
        }

        validityFailCheck = false;
    }

    /// <summary>
    /// 아바타 생성 완료 버튼
    /// </summary>
    public void ConfirmCreateAvatarButton() => StartCoroutine(ConfirmCreateAvatarProcess());

    /// <summary>
    /// 아바타 생성 완료 버튼 입력 시 프로세스
    /// </summary>
    IEnumerator ConfirmCreateAvatarProcess()
    {
        yield return StartCoroutine(NickNameInfoValidation());
        if (validityFailCheck)
        {
            validityFailCheck = false;
            yield break;
        }

        CommonInteraction.Instance.ConfirmPanelUpdate("아바타 만들기를 완료하시겠습니까?");
        CommonInteraction.Instance.confirmFunc = ConfirmAvatar;
    }

    public void ConfirmAvatar(bool check)
    {
        if (check)
        {
            StartCoroutine(CreateAvatarRoutine(nickNameInputField.text, avatarImage.GetPartsInfo()));
        }
    }

    IEnumerator CreateAvatarRoutine(string nickName, string avatarInfo)
    {
        WWWForm form = new WWWForm();
        form.AddField("Input_Id", (string)PhotonNetwork.LocalPlayer.CustomProperties["ID"]);
        form.AddField("Input_NickName", nickName);
        form.AddField("Avatar_Info", avatarInfo);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(createAvatarUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
                CommonInteraction.Instance.InfoPanelUpdate("Network or Http Error");
            }
            else
            {
                string[] check = webRequest.downloadHandler.text.Split(';');
                CheckSuccessed signUpSuccessed = (CheckSuccessed)Enum.Parse(typeof(CheckSuccessed), check[0]);

                switch (signUpSuccessed)
                {
                    case CheckSuccessed.FAIL:
                        CommonInteraction.Instance.InfoPanelUpdate("아바타 생성 실패");                        
                        break;
                    case CheckSuccessed.SUCCESS:
                        //아바타 생성 완료 시 아바타 생성 필요 여부 false 전환 및 커스텀 프로퍼티에 정보 대입
                        DataManager.needToCreateAvatar = false;
                        Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
                        hash["NickName"] = nickName;
                        hash["AvatarInfo"] = avatarInfo;
                        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
                        PhotonNetwork.LocalPlayer.NickName = nickName;

                        CommonInteraction.Instance.InfoPanelUpdate("아바타 생성 완료");
                        LobbyManager.Instance.FirstLoungeSetting();
                        gameObject.SetActive(false);                        
                        break;
                }
            }
        }
    }
    #endregion
}
