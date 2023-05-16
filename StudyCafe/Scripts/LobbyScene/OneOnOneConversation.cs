using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OneOnOneConversation : MonoBehaviour
{
    const int body = 0;
    const int eyes = 1;
    const int top = 2;
    const int hair = 3;

    [Header("StatusBar")]
    RectTransform uiCanvasRect;
    Vector2 distance;    

    [Header("UIObject")]    
    public Image[] avatarFaceParts;
    public InputField messageInputField;
    public Transform contents;
    public Text nameInfoText;
    public Player linkedPlayer;
    public string speakerGuid, listnerGuid;

    private void OnEnable()
    {
        uiCanvasRect = transform.parent.GetComponent<RectTransform>();
        InputControl.Instance.enterKey = SendMessageButton;
        InputControl.Instance.cancel = CloseButton;
    }

    public void PreparationSettings(Player linkedPlayer, string name, string otherGuid, string avatarInfo)
    {
        LobbyManager.Instance.conversationPanels.Add(this);
        this.linkedPlayer = linkedPlayer;
        nameInfoText.text = name;
        speakerGuid = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        listnerGuid = otherGuid;
        SetAvatarImage(avatarInfo);
    }

    public void SetAvatarImage(string avatarInfo)
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
                        avatarFaceParts[hair].sprite = sprites[0];
                    break;
                case "e":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + num);
                    if (!sprites.Length.Equals(0))
                        avatarFaceParts[eyes].sprite = sprites[0];
                    break;
                case "t":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num);
                    if (!sprites.Length.Equals(0))
                        avatarFaceParts[top].sprite = sprites[0];
                    break;
                case "sk":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num);
                    if (!sprites.Length.Equals(0))
                        avatarFaceParts[body].sprite = sprites[0];
                    break;
                default:
                    continue;
            }
        }
    }

    public void SendMessageButton()
    {
        string guid = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        string name = (string)linkedPlayer.CustomProperties["NickName"];
        string chat = messageInputField.text;
        //내 채팅창 업데이트
        OneOnOneChat(chat, null, true);
        LobbyManager.Instance.SendMessageProcess(linkedPlayer, guid, name, chat);
        //인풋 필드 정보 초기화
        messageInputField.text = "";
        messageInputField.ActivateInputField();
    }

    public void OneOnOneChat(string chat, string name, bool isMine)
    {
        if (contents.childCount > 200)
        {
            Destroy(contents.GetChild(0).gameObject);
        }               

        GameObject chatArea = Instantiate(isMine ? LobbyManager.Instance.myChatArea : LobbyManager.Instance.otherChatArea, contents);
        OneOnOneChatBox boxScript = chatArea.GetComponent<OneOnOneChatBox>();
        boxScript.boxRect.sizeDelta = new Vector2(350, boxScript.boxRect.sizeDelta.y);
        boxScript.msg.text = chat;
        if (!isMine)
            chatArea.GetComponent<OneOnOneChatBox>().userName.text = name;
        LayoutRebuilder.ForceRebuildLayoutImmediate(boxScript.boxRect);

        float X = boxScript.msgRect.sizeDelta.x + 20;
        float Y = boxScript.msgRect.sizeDelta.y;
        if (Y > 49)
        {
            for (int i = 0; i < 200; i++)
            {
                boxScript.boxRect.sizeDelta = new Vector2(X - i * 2, boxScript.boxRect.sizeDelta.y);
                LayoutRebuilder.ForceRebuildLayoutImmediate(boxScript.boxRect);

                if (Y != boxScript.msgRect.sizeDelta.y)
                {
                    boxScript.boxRect.sizeDelta = new Vector2(X - (i * 2) + 2, Y);
                    break;
                }
            }
        }
        else
            boxScript.boxRect.sizeDelta = new Vector2(X, Y);
    }

    public void CloseButton()
    {
        InputControl.Instance.enterKey = null;
        InputControl.Instance.cancel = LobbyManager.Instance.LogoutButton;
        for (int i = 0; i < LobbyManager.Instance.conversationPanels.Count; i++)
        {
            if (LobbyManager.Instance.conversationPanels[i].Equals(this))
            {
                LobbyManager.Instance.conversationPanels.RemoveAt(i);
                break;
            }
        }
        Destroy(gameObject);
    }

    public void LeftRoomOtherPlayer()
    {
        CommonInteraction.Instance.EventPanelUpdate("상대가 라운지를 떠났습니다.");
        CommonInteraction.Instance.eventFunc = CloseButton;
    }

    /// <summary>
    /// 뷰어 위치 조절 드래그를 시작할 때 판정
    /// </summary>
    public void StartDrag()
    {
        InputControl.Instance.isValid = false;
        RectTransform rT = gameObject.GetComponent<RectTransform>();
        distance = rT.anchoredPosition - (Vector2)Input.mousePosition;
    }

    /// <summary>
    /// 뷰어 위치 조절 중 판정
    /// </summary>
    public void Drag()
    {
        RectTransform rT = gameObject.GetComponent<RectTransform>();
        rT.anchoredPosition = (Vector2)Input.mousePosition + distance;

        float xSize = uiCanvasRect.rect.width - rT.rect.width;
        if (rT.anchoredPosition.x > 0f)
            rT.anchoredPosition = new Vector2(0f, rT.anchoredPosition.y);
        else if (rT.anchoredPosition.x < -xSize)
            rT.anchoredPosition = new Vector2(-xSize, rT.anchoredPosition.y);

        float ySize = uiCanvasRect.rect.height - 40f;
        if (rT.anchoredPosition.y > 0f)
            rT.anchoredPosition = new Vector2(rT.anchoredPosition.x, 0f);
        else if (rT.anchoredPosition.y < -ySize)
            rT.anchoredPosition = new Vector2(rT.anchoredPosition.x, -ySize);
    }

    public void EndDrag()
    {
        InputControl.Instance.isValid = true;
    }
}
