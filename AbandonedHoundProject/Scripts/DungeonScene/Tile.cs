using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///타일의 타입을 나타내는 enum<br/>
///0 EMPTY : 타일 없음 (빈 공간)<br/>
///1 NORMAL : 일반 타일 (이벤트 없음)<br/>
///2 START : 시작 타일 (특정 구역의 시작 지점, 처음 시작할 때와 다음 지역으로 넘어갈 때 해당 위치에서 시작 )<br/>
///3 END : 종점 타일 (특정 구역의 마지막 지점, 이전 구역으로 돌아갈때 해당 위치에서 시작)<br/>
///4 TREASURE : 보물 타일 (아이템 획득 상자가 배치되는 타일)<br/>
///5 BOSS : 보스 타일 (보스가 배치되는 타일)<br/>
///6 SWITCH : 스위치 (특정 문과 연결된 스위치가 배치되는 타일)<br/>
///7 EVENT : 트리거 이벤트 (대화 이벤트 등 트리거 이벤트 발동)
///</summary>
public enum TileType
{
    EMPTY, NORMAL, START, END, TREASURE, BOSS, SWITCH, EVENT
}

///<summary>
///벽의 타입을 나타내는 enum<br/>
///0 EMPTY : 벽 없음<br/>
///1 NORMAL : 일반 벽 (지나가지 못하며, 부수기 가능)<br/>
///2 DOOR : 문 (그냥 지나가지 못하며, 문 열기 액션을 통해 지나가는 것이 가능)<br/>
///3 ENTRANCE : 입구 (이전 구역으로 가거나, 스테이지를 벗어날 때 사용)<br/>
///4 EXIT : 출구 (다음 구역으로 갈 때 사용)<br/>
///5 BROKEN : 부서진 이펙트가 있는 벽 (비밀 방 숨김용)
///6 TERRAIN : 길 막기용, 또는 단순 지형을 나타내기 위한 부서지지 않는 벽
///</summary>
public enum WallType
{
    EMPTY, NORMAL, DOOR, ENTRANCE, EXIT, BROKEN, TERRAIN
}

///<summary>
///타일의 정보가 담긴 클래스<br/>
///tile : 해당 타일의 타입을 나타내는 TileType 변수<br/>
///wall : 해당 타일의 상하좌우 위치에 배치되는 벽의 종류를 나타내는 [4]크기의 wallType 배열 변수<br/>
///activeTileEvent : 타일 이벤트가 실행되었는지 여부를 체크하는 bool 변수<br/>
///miniMapCheck : 미니맵에 표시될 타일인지 여부를 체크하는 bool 변수 <br/>
///eventType : 이벤트 유형을 나타내는 int 변수 (0일 경우 이벤트 없음)
///</summary>
public class Tile
{
    public TileType tile;
    public WallType[] wall = new WallType[4];
    public bool activeTileEvent = false;
    public bool miniMapCheck = false;
    public int eventType = 0;
}
