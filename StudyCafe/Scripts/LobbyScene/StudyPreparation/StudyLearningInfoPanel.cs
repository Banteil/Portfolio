using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudyLearningInfoPanel : MonoBehaviour
{
    const int learningInfo = 0;
    const int curriculum = 1;
    const int qualificationRequirement = 2;
    const int roomInfo = 3;

    [Header("StudyLearningInfoPanel")]
    public GameObject[] infoPanel;
    public Toggle[] tap;

    private void OnEnable()
    {
        SelectTap(learningInfo);
    }

    ///<summary>
    ///스터디 준비 Tap을 선택했을 시 실행되는 함수<br />
    ///해당 탭에 맞는 패널이 active됨
    ///</summary>
    public void SelectTap(int index)
    {
        if(index.Equals(qualificationRequirement))
        {
            if(!StudyPreparation.Instance.powerToEdit[1])
            {
                CommonInteraction.Instance.InfoPanelUpdate("자격 요건에 대한 조회 권한이 없습니다.");
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
