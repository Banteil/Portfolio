using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinedMemberInfoPanel : MonoBehaviour
{
    const int body = 0;
    const int eyes = 1;
    const int top = 2;
    const int hair = 3;

    public Image[] faceImages;
    public Text nameText;

    public Toggle[] powerToggle;
    public Transform content;
    public Text[] totalStatusInfoText;

    public string memberGUID;
    //emailID ☞ phoneNumber ☞ power ☞ status
    public string joinedInfo;

    private void OnEnable()
    {
        StartCoroutine(SettingUserInfo());
    }

    IEnumerator SettingUserInfo()
    {
        //닉네임, 아바타 정보 get 및 세팅
        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.NICKNAME, memberGUID));
        string name = DataManager.Instance.info;
        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.AVATAR, memberGUID));
        string avatarInfo = DataManager.Instance.info;
        nameText.text = name;
        SetFaceImage(avatarInfo);
        //권한 정보 세팅
        string[] infos = joinedInfo.Split('☞');
        string[] powerInfo = infos[2].Split(',');
        SetPowerToggle(powerInfo);
        //출석 정보, 과제 제출 정보 토대로 날짜별 현황 세팅
        //구현 필요

        //총 현황 정보 세팅
        string[] statusInfo = infos[3].Split(',');
        SetTotalStatus(statusInfo);
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

    void SetPowerToggle(string[] powerInfo)
    {
        for (int i = 0; i < powerInfo.Length; i++)
        {
            if (powerInfo[i].Equals("T"))
                powerToggle[i].isOn = true;
            else
                powerToggle[i].isOn = false;
        }
    }

    void SetTotalStatus(string[] statusInfo)
    {
        if (statusInfo.Length.Equals(0)) return;

        for (int i = 0; i < statusInfo.Length; i++)
        {
            string info = "";
            switch (i)
            {
                case 0:
                    info += "출석: ";
                    break;
                case 1:
                    info += "지각: ";
                    break;
                case 2:
                    info += "결석: ";
                    break;
                case 3:
                    info += "과제 제출: ";
                    break;
            }
            info += statusInfo + "회";
        }
    }

    public void Authorization(int index)
    {

    }

    void Initialization()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        for (int i = 0; i < faceImages.Length; i++)
        {
            faceImages[i].sprite = null;
        }
        nameText.text = "";

        for (int i = 0; i < powerToggle.Length; i++)
        {
            powerToggle[i].isOn = false;
        }

        string[] statusInfo = { "0", "0", "0", "0" };
        SetTotalStatus(statusInfo);
    }

    private void OnDisable()
    {
        Initialization();
    }
}
