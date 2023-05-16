using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoPanel : MonoBehaviour
{
    const int roomTypeCount = 4;

    public Image roomImage;
    public Text roomInfoText;
    public Button leftButton, rightButton;

    Sprite[] roomTypeImage = new Sprite[roomTypeCount];
    string[] roomTypeName = { "6인 강의실 (교실)", "6인 강의실 (발표장)", "6인 강의실 (카페)", "대강의실" };
    int roomType = 0;

    public GameObject saveButtonObject;
    bool isFirstEnable;

    private void Awake()
    {
        for (int i = 0; i < roomTypeCount; i++)
        {
            roomTypeImage[i] = Resources.Load<Sprite>("RoomSprite/room_" + i);
        }
    }

    private void OnEnable()
    {
        saveButtonObject.GetComponent<Button>().onClick.RemoveAllListeners();
        saveButtonObject.GetComponent<Button>().onClick.AddListener(ModifyCompleteButton);
        if (isFirstEnable)
        {
            StudyPreparation.Instance.interactFunc += Initialization;
            SettingRoomInfo();
            isFirstEnable = false;
        }
    }

    void SettingRoomInfo()
    {
        if (!StudyPreparation.Instance.powerToEdit[1])
            NonEditableProcess();

        roomType = StudyPreparation.Instance.studyData.studyRoomType;
        roomImage.sprite = roomTypeImage[roomType];
    }

    void NonEditableProcess()
    {
        leftButton.interactable = false;
        rightButton.interactable = false;
        saveButtonObject.GetComponent<Button>().interactable = false;
        saveButtonObject.transform.GetChild(0).GetComponent<Text>().text = "학습 내용의 수정 권한이 없습니다.";
    }

    public void ChangeButton(int type)
    {
        if(type.Equals(0))
        {
            roomType--;
            if (roomType < 0) roomType = 0;
        }
        else
        {
            roomType++;
            if (roomType > roomTypeCount - 1) roomType = roomTypeCount - 1;
        }

        roomInfoText.text = roomTypeName[roomType];
        roomImage.sprite = roomTypeImage[roomType];
    }

    public void ModifyCompleteButton() => StartCoroutine(ModifyCompleteProcess());

    IEnumerator ModifyCompleteProcess()
    {
        StudyInfoData tempData = StudyPreparation.Instance.studyData;
        tempData.studyRoomType = roomType;

        yield return StartCoroutine(DataManager.Instance.UpdateStudyRegistration(tempData));
        if (DataManager.Instance.info.Equals("SUCCESS"))
        {
            CommonInteraction.Instance.InfoPanelUpdate("수정된 룸 정보가 저장되었습니다.");
            StudyPreparation.Instance.studyData = tempData;
        }
        else
        {
            CommonInteraction.Instance.InfoPanelUpdate("룸 정보 수정 과정에 문제가 발생하였습니다.\n다시 시도해 주세요.");
        }
    }

    public void Initialization()
    {
        leftButton.interactable = true;
        rightButton.interactable = true;
        roomType = 0;
        roomImage.sprite = roomTypeImage[roomType];        
        roomInfoText.text = roomTypeName[roomType];

        saveButtonObject.GetComponent<Button>().interactable = true;
        saveButtonObject.transform.GetChild(0).GetComponent<Text>().text = "수정한 내용 저장";
        isFirstEnable = true;
    }
}
