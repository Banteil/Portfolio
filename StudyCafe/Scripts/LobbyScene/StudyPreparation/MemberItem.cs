using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemberItem : MonoBehaviour
{
    const int body = 0;
    const int eyes = 1;
    const int top = 2;
    const int hair = 3;

    public int index;
    public Image[] faceImages;
    //심사 시 nickNameText에 닉네임, 이메일 둘 다 기재
    public Text nickNameText;
    public GameObject applicationDateTextObject;
    public GameObject masterImageObject;
    public GameObject kickbackButton;

    public IntFunction infoDisplayFunction;
    public IntFunction deleteItemFunction;
    public StringFunction postMessageFunction;

    string memberGUID;
    public string MemberGUID
    {
        get { return memberGUID; }
        set
        {
            memberGUID = value;
            StartCoroutine(SettingUserInfo(value));
        }
    }

    ///<summary>
    ///가입 멤버 정보 프로퍼티<br/>
    ///Get 시 emailID ☞ phoneNumber ☞ power ☞ status 순으로 리턴<br/>
    ///Set 시 memberGUID ☞ emailID ☞ phoneNumber ☞ power ☞ status 순으로 세팅
    ///</summary>
    public string JoinedInfo
    {
        get
        {
            string value = emailID + "☞" + phoneNumber + "☞" + power + "☞" + status;
            return value;
        }
        set
        {
            //guid, id, phone, power, status 순서    
            string[] memberData = value.Split('☞');
            MemberGUID = memberData[0];
            emailID = memberData[1];
            phoneNumber = memberData[2];
            power = memberData[3];
            status = memberData[4];
        }
    }
    string emailID;
    string phoneNumber;
    string power;
    string status;

    ///<summary>
    ///신청 멤버 정보 프로퍼티<br/>
    ///Get 시 nickName ☞ answer ☞ token ☞ fileName 순으로 리턴<br/>
    ///Set 시 token ☞ memberGUID ☞ answer ☞ evidence_file_name ☞ insert_time 순으로 세팅
    ///</summary>
    public string ReviewInfo
    {
        get
        {
            string value = nickNameText.text + "☞" + answer + "☞" + token + "☞" + fileName;
            return value;
        }
        set
        {
            //token, guid, answer, evidence_file_name, insert_time 순서
            string[] memberData = value.Split('☞');
            token = memberData[0];
            MemberGUID = memberData[1];
            answer = memberData[2];
            fileName = memberData[3];
            applicationDateTextObject.SetActive(true);
            applicationDateTextObject.GetComponent<Text>().text = memberData[4];
            kickbackButton.SetActive(false);
        }
    }
    string token;
    string answer;
    string fileName;

    IEnumerator SettingUserInfo(string guid)
    {
        //권한 체크하여 상호작용 UI 변경
        if (StudyPreparation.Instance.powerToEdit[3])
            kickbackButton.SetActive(true);
        else
            GetComponent<Button>().interactable = false;
        //마스터 여부 체크
        string studyGUID = StudyPreparation.Instance.studyData.guid;
        yield return StartCoroutine(DataManager.Instance.GetStudyCompositionInfo(StudyComposition.MASTER, studyGUID));
        string master = DataManager.Instance.info;
        if (master.Equals(guid)) masterImageObject.SetActive(true);
        //닉네임, 아바타 정보 get 및 세팅
        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.NICKNAME, guid));
        string name = DataManager.Instance.info;
        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.AVATAR, guid));
        string avatarInfo = DataManager.Instance.info;
        nickNameText.text = name;
        SetFaceImage(avatarInfo);
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

    public void MemberInformationDisplayButton() => infoDisplayFunction?.Invoke(index);

    public void PostMessageButton() => postMessageFunction?.Invoke(memberGUID);

    public void KickBackButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate("'" + nickNameText.text + "' 님을 스터디 멤버에서 제외하시겠습니까?\n강제 제외 시 해당 멤버는 스터디 재 가입이 불가합니다.");
        CommonInteraction.Instance.confirmFunc = CheckKickBack;
    }

    void CheckKickBack(bool check)
    {
        if (check)
            StartCoroutine(KickBackProcess());
    }

    IEnumerator KickBackProcess()
    {
        CommonInteraction.Instance.StartLoding();
        yield return StartCoroutine(DataManager.Instance.DeleteMemberInfo(StudyPreparation.Instance.studyData.guid, memberGUID));
        if (DataManager.Instance.info.Equals("SUCCESS"))
        {
            CommonInteraction.Instance.InfoPanelUpdate("'" + nickNameText.text + "' 님을 스터디 멤버에서 제외하였습니다.");
            deleteItemFunction?.Invoke(index);
            Destroy(gameObject);
        }
        else
        {
            CommonInteraction.Instance.InfoPanelUpdate("멤버 제외에 실패하였습니다.\n다시 시도해 주세요.");
        }
        CommonInteraction.Instance.isLoading = false;
    }
}
