using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///씬 전환 시 넘겨받거나 확인해야하는 변수를 저장하는 클래스
///</summary>
public static class DataPassing
{
    public static int stageNum = 0; //정비 씬에서 몇번째 스테이지를 선택하였는지를 저장하는 변수
    public static string stageName = "???"; //선택된 스테이지의 이름

    public static bool isBattle; //현재 전투씬 재생 중인지 여부를 확인하기 위한 bool
    public static bool isEncounter; //전투 씬 진입 시 인카운트로 만난것인지, 이벤트로 만난것인지 확인
    public static DungeonMonster tempDM = null; //인카운트 한 몬스터의 정보를 임시로 담는 공간
    public static bool playerDie; //전투, 이벤트 결과로 플레이어가 사망하였는지 여부를 판단하는 변수
    public static bool isBoss; //보스와의 전투인지 여부를 판단하는 변수
    public static bool isClear; //현재 스테이지가 클리어 되었는지 여부를 판단하는 변수
    public static bool[] stageClear = new bool[6];

    public static string getSceneName;
    public static bool isStory; //스토리 씬 연출이 진행중인지 여부를 판단하는 bool
    public static string storyKind; //스토리DB에서 호출할 종류를 저장하는 변수
    public static bool tutorialEnd;

    public static void Reset()
    {
        stageNum = 0;
        stageName = null;
        isBattle = false;
        isEncounter = false;
        tempDM = null;
        playerDie = false;
        isBoss = false;
        isClear = false;
        isStory = false;
        storyKind = null;
    }
}
