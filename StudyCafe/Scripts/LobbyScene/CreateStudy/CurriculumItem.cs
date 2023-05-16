using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 특정 스터디에 등록된 커리큘럼 날짜를 표시하는 아이템 <br />
/// 해당 아이템 선택 시 표시된 커리큘럼 날짜에 맞는 스터디를 실행하는 역할 
/// </summary>
public class CurriculumItem : MonoBehaviour
{
    int index;
    public int ListIndex
    { 
        get { return index; }
        set
        {
            index = value;
            indexText.text = (index + 1).ToString();
        }
    }

    public GameObject deleteButtonObject;
    public Text indexText;
    public Text dateText;   
    CurriculumInfo curriculumInfo;
    public CurriculumInfo CurriculumInfomation
    {
        get { return curriculumInfo; }
        set
        {
            curriculumInfo = value;
            dateText.text = curriculumInfo.startDate + "\n~\n" + curriculumInfo.endDate;            
        }
    }

    public IntFunction itemInfoFunction;
    public IntFunction deleteFunction;

    /// <summary>
    /// 커리큘럼 정보 보기
    /// </summary>
    public void CurriculumInfoButton()
    {
        itemInfoFunction?.Invoke(index);
    }

    public void DeleteItemButton()
    {
        CommonInteraction.Instance.ConfirmPanelUpdate(indexText.text + " 수업 자료를 삭제하시겠습니까?");
        CommonInteraction.Instance.confirmFunc = DeleteItemProcess;
    }

    void DeleteItemProcess(bool check)
    {
        if(check)
        {
            deleteFunction?.Invoke(index);
            Destroy(gameObject);
        }
    }

}
