using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 선택한 스터디 정보를 확인 및 수정하는 역할을 담당하는 클래스
/// </summary>
public class StudyPreparation : Singleton<StudyPreparation>
{
    [Header("StudyPreparationPanel")]
    public GameObject[] infoPanel;
    public Toggle[] tap;
    [HideInInspector]
    public bool[] powerToEdit = new bool[5];

    [Header("ETC")]
    public StudyInfoData studyData;
    public int tapNum = 0;
    public bool isStudyRoom;

    public delegate void InteractActive();
    public InteractActive interactFunc;

    private void OnEnable()
    {
        StartCoroutine(CheckPermissionToEdit());        
    }

    IEnumerator CheckPermissionToEdit()
    {
        string memberGUID = (string)PhotonNetwork.LocalPlayer.CustomProperties["GUID"];
        yield return StartCoroutine(DataManager.Instance.GetMemberInfo(studyData.guid, memberGUID));
        string[] data = DataManager.Instance.info.Split('☞');              
        string[] power = data[3].Split(',');
        for (int i = 0; i < power.Length; i++)
        {
            if (power[i].Equals("T"))
                powerToEdit[i] = true;
            else
                powerToEdit[i] = false;
        }
        
        SelectTap(tapNum);
    }

    ///<summary>
    ///스터디 준비 Tap을 선택했을 시 실행되는 함수<br />
    ///해당 탭에 맞는 패널이 active됨
    ///</summary>
    public void SelectTap(int index)
    {
        tap[index].isOn = true;
        for (int i = 0; i < infoPanel.Length; i++)
        {
            if (i.Equals(index))
                infoPanel[i].SetActive(true);
            else
                infoPanel[i].SetActive(false);
        }
    }       

    public void InteractFunction()
    {
        interactFunc?.Invoke();
        interactFunc = null;
    }

    ///<summary>
    ///스터디 준비 UI 정보를 초기화 하는 함수
    ///</summary>
    void Initialization()
    {
        InteractFunction();
        for (int i = 0; i < tap.Length; i++)
        {
            if (i.Equals(0)) tap[i].isOn = true;
            else tap[i].isOn = false;
        }

        for (int i = 0; i < infoPanel.Length; i++)
        {
            infoPanel[i].SetActive(false);
        }

        if (!isStudyRoom)
        {
            if (studyData != null)
                MyStudyManagement.Instance.EndStudyModify(studyData);
        }
    }

    public void CloseButton() => gameObject.SetActive(false);

    void OnDisable() => Initialization();
}
