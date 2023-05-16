using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class StudyInfoData
{
    //스터디 guid
    public string guid;
    //스터디 이름
    public string studyName;
    //스터디 주제
    public string subject;
    //스터디 목표
    //ex) 2010-01-01,2010-01-02,10,0,5
    public string objectives;
    //초대 코드
    public string code;
    //일정 및 커리큘럼 정보 리스트
    public List<CurriculumInfo> curriculumInfoList = new List<CurriculumInfo>();
    //스터디룸 정보
    public int studyRoomType;
    //규칙 (지각, 결석, 과제)
    //"Tardy" -> 1,0
    //"Absent" -> 1,0
    //"Assignment" -> 1,0
    public Dictionary<string, string> rules = new Dictionary<string, string>()
    {
        {"Tardy","1,0"}, {"Absent","1,0"}, {"Assignment","1,0"}
    };
    //자격 요건
    //Question -> 질문(질문1,질문2,질문3,...)
    //Evidence -> 증빙자료
    //Gender -> 성별 조건(성별 무관, 남성, 여성)
    public Dictionary<string, string> eligibilityRequirements = new Dictionary<string, string>()
    {
        {"Question",""}, {"Evidence",""}, {"Gender","성별 무관"}
    };
}