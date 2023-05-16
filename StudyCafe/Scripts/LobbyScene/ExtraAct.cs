using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExtraAct : MonoBehaviour
{
    public enum DirectionInfo { D, L, LU, U, R, RU };
    public DirectionInfo directionInfo;

    [Header("PartsConstant")]
    const int skin = 0;
    const int eyes = 1;
    const int pants = 2;
    const int top = 3;
    const int hair = 4;    
    const int accessory = 5;

    [Header("DirectionConstant")]
    const int d = 0;
    const int l = 9;
    const int lu = 18;
    const int u = 27;
    const int r = 36; 
    const int ru = 45;

    [Header("ExtraMenuUI")]
    public GameObject menuCanvas;
    public Text nameText;
    public BoxCollider2D physicsBox;

    [Header("ExtraInfo")]  
    //아바타 스프라이트 세팅을 위한 정보 string
    [HideInInspector]
    public string avatarInfo;
    [HideInInspector]
    public string guid;
    //아바타 각 파츠 별 SpriteRenderer
    public SpriteRenderer[] parts;
    Sprite[][] partsSprites;
    public Player linkedPlayer;
    int direction, eyeDirection;
    int[] changeOrderRequiredTops = { 1 };

    /// <summary>
    /// 생성 시 초기화 함수
    /// </summary>
    public void CreationInitialization(string guid, string avatarInfo, string nickName)
    {
        partsSprites = new Sprite[transform.GetChild(1).childCount][];
        menuCanvas.transform.parent.GetComponent<Canvas>().worldCamera = Camera.main;
        this.guid = guid;
        this.avatarInfo = avatarInfo;
        nameText.text = nickName;
        SetDirection();
        SettingAvatarImage();
    }

    void SetDirection()
    {
        switch (directionInfo)
        {
            case DirectionInfo.D:
                {
                    direction = d;
                    eyeDirection = 0;
                }
                break;
            case DirectionInfo.L:
                {
                    direction = l;
                    eyeDirection = 10;
                }
                break;
            case DirectionInfo.LU:
                {
                    direction = lu;
                    eyeDirection = -1;
                }
                break;
            case DirectionInfo.U:
                {
                    direction = u;
                    eyeDirection = -1;
                }
                break;
            case DirectionInfo.RU:
                {
                    direction = ru;
                    eyeDirection = -1;
                }
                break;
            case DirectionInfo.R:
                {
                    direction = r;
                    eyeDirection = 20;
                }
                break;

        }
    }

    /// <summary>
    /// 전달받은 아바타 정보를 토대로 sprite를 세팅하는 함수
    /// </summary>
    void SettingAvatarImage()
    {
        string[] partsInfo = avatarInfo.Split(',');
        int topNum = 0;

        for (int i = 0; i < partsInfo.Length; i++)
        {
            string[] info = partsInfo[i].Split('_');
            string parts = info[0];
            int num = int.Parse(info[1]);

            switch (parts)
            {
                case "h":
                    partsSprites[hair] = Resources.LoadAll<Sprite>("AvatarSprite/Hair/hair_" + num);
                    break;
                case "e":
                    partsSprites[eyes] = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + num);
                    break;
                case "t":
                    partsSprites[top] = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num);
                    topNum = num;
                    break;
                case "p":
                    partsSprites[pants] = Resources.LoadAll<Sprite>("AvatarSprite/Pants/pants_" + num);
                    break;
                case "sk":
                    partsSprites[skin] = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num);
                    break;
                case "a":
                    if(!num.Equals(0))
                        partsSprites[accessory] = Resources.LoadAll<Sprite>("AvatarSprite/Accessory/acc_" + num);
                    break;
            }
        }

        for (int i = 0; i < parts.Length; i++)
        {
            if (partsSprites[i] == null) continue;
            else if (partsSprites[i].Length.Equals(0)) continue;
            parts[i].sprite = partsSprites[i][0];
        }

        //눈, 입 제외 행동
        for (int i = 0; i < parts.Length; i++)
        {
            if (partsSprites[i] == null) continue;
            else if (partsSprites[i].Length.Equals(0) || i.Equals(eyes)) continue;
            parts[i].sprite = partsSprites[i][direction];
        }

        if (eyeDirection >= 0)
            parts[eyes].sprite = partsSprites[eyes][eyeDirection];
        else
            parts[eyes].sprite = null;

        //CheckOrderTopAndPants(topNum);
        StartCoroutine(BlinkEye());
    }

    ///// <summary>
    ///// 상의 디자인이 하의를 감싸는 디자인일 시 하의의 표시 순서를 변경하여 어색함 제거
    ///// </summary>
    //void CheckOrderTopAndPants(int topNum)
    //{
    //    for (int i = 0; i < changeOrderRequiredTops.Length; i++)
    //    {
    //        if (topNum.Equals(changeOrderRequiredTops[i]))
    //        {
    //            parts[pants].transform.SetSiblingIndex(2);
    //            break;
    //        }
    //    }
    //}

    IEnumerator BlinkEye()
    {
        //눈 행동
        if (parts[eyes].sprite != null)
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(3f, 6f));
                int eyeIndex = eyeDirection + 9;
                parts[eyes].sprite = partsSprites[eyes][eyeIndex];

                yield return new WaitForSeconds(0.2f);
                parts[eyes].sprite = partsSprites[eyes][eyeDirection];
            }
        }
    }

    public void SelectExtra()
    {
        if (InputControl.Instance.selectedExtra != null)
        {
            if (!InputControl.Instance.selectedExtra.Equals(this))
                InputControl.Instance.selectedExtra.menuCanvas.SetActive(false);
        }

        if (!menuCanvas.activeSelf)
        {
            menuCanvas.SetActive(true);
        }
        else
        {
            if (menuCanvas.GetComponent<AvatarMenu>().moveEnd)
            {
                menuCanvas.SetActive(false);
            }
        }

        InputControl.Instance.selectedExtra = this;
    }

    public void ExtraInfo()
    {
        Debug.Log("정보 보기");
        menuCanvas.SetActive(false);
    }

    public void OneOnOneConversation()
    {
        GameObject chatWindow = Instantiate(Resources.Load<GameObject>("Prefabs/UI/OneOnOneConversationPanel"), LobbyManager.Instance.OneOnOneCanvas, false);
        OneOnOneConversation chatScript = chatWindow.GetComponent<OneOnOneConversation>();
        chatScript.PreparationSettings(linkedPlayer, nameText.text, guid, avatarInfo);        
        menuCanvas.SetActive(false);
    }


    private void OnDisable()
    {
        menuCanvas.SetActive(false);
        linkedPlayer = null;
    }
}
