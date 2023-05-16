using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCObjectInteraction : MonoBehaviour
{
    public GameObject masterCanvas, grouperCanvas;
    public GameObject waitingRoomMasterPanel, chatRoomMasterPanel, lobbyMenuPanel;
    public GameObject slideSettingPanel, slideControlPanel, studyOpenPanel, myStudyPanel;
    public GameObject studyListPanel, openedStudyPanel, preparationStudyPanel, tempStudyItem;
    public GameObject emoteListPanel;
    public GameObject participantListPanel;
    public ParticipantInfoItem npcInfoItem, myInfoItem, studyOpenInfoItem;
    public InputField namePlateInputField;
    public Toggle toggleDisplayNamePlate, toggleFreezeChatBox;

    public void EnableEmoteListPanel() => emoteListPanel.SetActive(true);
    public void DisableEmoteListPanel() => emoteListPanel.SetActive(false);
    public void EnableDisplayNamePlate() => toggleDisplayNamePlate.isOn = true;
    public void DisableDisplayNamePlate() => toggleDisplayNamePlate.isOn = false;
    public void EnableParticipantListButton() => participantListPanel.SetActive(true);
    public void DisableParticipantListButton() => participantListPanel.SetActive(false);
    public void EnableFreezeChatBox() => toggleFreezeChatBox.isOn = true;
    public void DisableFreezeChatBox() => toggleFreezeChatBox.isOn = false;
    public void EnableWaitingRoomMasterPanel() => waitingRoomMasterPanel.SetActive(true);
    public void DisableWaitingRoomMasterPanel() => waitingRoomMasterPanel.SetActive(false);
    public void EnableSlideSettingPanel() => slideSettingPanel.SetActive(true);
    public void DisableSlideSettingPanel() => slideSettingPanel.SetActive(false);
    public void EnableSlideControlPanel() => slideControlPanel.SetActive(true);
    public void DisableSlideControlPanel() => slideControlPanel.SetActive(false);
    public void EnableStudyOpenPanel() => studyOpenPanel.SetActive(true);
    public void DisableStudyOpenPanel() => studyOpenPanel.SetActive(false);    
    public void EnableChatRoomMasterPanel() => chatRoomMasterPanel.SetActive(true);
    public void DisableChatRoomMasterPanel() => chatRoomMasterPanel.SetActive(false);
    public void EnableLobbyMenuPanel() => lobbyMenuPanel.SetActive(true);
    public void DisableLobbyMenuPanel() => lobbyMenuPanel.SetActive(false);
    public void EnableMyStudyPanel() => myStudyPanel.SetActive(true);
    public void DisableMyStudyPanel() => myStudyPanel.SetActive(false);
    public void EnableStudyListPanel() => studyListPanel.SetActive(true);
    public void DisableStudyListPanel() => studyListPanel.SetActive(false);
    public void EnableOpenedStudyPanel() => openedStudyPanel.SetActive(true);
    public void DisableOpenedStudyPanel() => openedStudyPanel.SetActive(false);
    public void EnablePreparationStudyPanel() => preparationStudyPanel.SetActive(true);
    public void DisablePreparationStudyPanel() => preparationStudyPanel.SetActive(false);
    public void NextOpenedStudyButton() => openedStudyPanel.GetComponent<CreateStudy>().CompleteButton();
    public void SelectCabinetTap() => preparationStudyPanel.GetComponent<StudyPreparation>().SelectTap(5);
    public void EnableStudyItem() => tempStudyItem.SetActive(true);
    public void EnableMasterStudyListPanel() => LobbyManager.Instance.CreateStudy();
    public void EnableGrouperStudyListPanel() => LobbyManager.Instance.ParticipateStudy();
    public void EnableInviteCodePanel() => LobbyManager.Instance.OpenInviteCodePanelButton();

    public void CheckUserType()
    {
        //마스터, 그루퍼 구분할 때 잔재, 수정 필요
        masterCanvas.SetActive(true);
        grouperCanvas.SetActive(true);       
    }
}
