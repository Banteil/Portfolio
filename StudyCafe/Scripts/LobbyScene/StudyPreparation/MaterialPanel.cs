using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialPanel : MonoBehaviour
{
    const int classMaterials = 0;
    const int organizingNotes = 1;
    const int fileManagement = 2;

    public GameObject[] infoPanel;
    public Toggle[] tap;

    private void OnEnable()
    {
        SelectTap(classMaterials);
    }

    ///<summary>
    ///스터디 준비 Tap을 선택했을 시 실행되는 함수<br />
    ///해당 탭에 맞는 패널이 active됨
    ///</summary>
    public void SelectTap(int index)
    {
        if (!StudyPreparation.Instance.powerToEdit[4])
        {
            if (index.Equals(fileManagement))
            {
                CommonInteraction.Instance.InfoPanelUpdate("파일 관리에 대한 권한이 없습니다.");
                return;
            }
        }

        for (int i = 0; i < infoPanel.Length; i++)
        {
            if (i.Equals(index))
                infoPanel[i].SetActive(true);
            else
                infoPanel[i].SetActive(false);
        }
    }

    ///<summary>
    ///스터디 준비 UI 정보를 초기화 하는 함수
    ///</summary>
    void Initialization()
    {
        for (int i = 0; i < tap.Length; i++)
        {
            if (i.Equals(0)) tap[i].isOn = true;
            else tap[i].isOn = false;
        }

        for (int i = 0; i < infoPanel.Length; i++)
        {
            infoPanel[i].SetActive(false);
        }
    }

    void OnDisable() => Initialization();
}
