using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class CurriculumInfo
{
    ///<summary>
    ///n주차 정보
    ///</summary>
    public int weekIndex;
    ///<summary>
    ///시작 날짜 및 시간
    ///</summary>
    public string startDate;
    ///<summary>
    ///종료 날짜 및 시간
    ///</summary>
    public string endDate;
    ///<summary>
    ///커리큘럼 설명
    ///</summary>
    public string description;
    ///<summary>
    ///교재, 발표자료, 과제, 퀴즈, 쪽지시험 유무 표기용 정보<br/>
    ///기본값은 순서대로 T,T,T,T,T<br/>
    ///없을 시 F로 정보 변경
    ///</summary>
    public string announcementInfo = "T,T,T,T,T";
    ///<summary>
    ///해당 커리큘럼의 선생님 GUID
    ///</summary>
    public string teacherGUID = "";
}
