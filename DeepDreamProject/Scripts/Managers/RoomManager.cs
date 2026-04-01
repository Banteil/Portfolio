using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : Singleton<RoomManager>
{
    const int STARTEVENTID = 0;
    const int SLEEPEVENTID = 4;

    [Header("Bind")]
    public Transform BindRoomsTr;
    public Room CurrentRoom;
    public MinimapCameraController MinimapCC;

    [Header("Room Event Random Percent")]
    public float[] Percentage = new float[RoomEventData.MaxEventGrade];

    List<Room> _rooms = new List<Room>();
    List<RoomEventData>[] _listOfEventsByGrade = new List<RoomEventData>[RoomEventData.MaxEventGrade];
    RoomEventData startRoomEvent;

    protected override void Awake()
    {
        base.Awake();
        if (BindRoomsTr == null)
            BindRoomsTr = GameObject.Find("___ROOMS").transform;

        MinimapCC = FindObjectOfType<MinimapCameraController>();
        System.Array.Sort(Percentage);
        System.Array.Reverse(Percentage);
    }

    protected virtual void Start()
    {
        InitializationRoomEventData();
        InitializationRoom();
        RequiredRoomSetup();
    }

    void InitializationRoomEventData()
    {
        for (int i = 0; i < _listOfEventsByGrade.Length; i++)
        {
            _listOfEventsByGrade[i] = new List<RoomEventData>();
        }

        for (int i = 0; i < DataManager.Instance.RoomEventDataList.Count; i++)
        {
            if(DataManager.Instance.RoomEventDataList[i].EventID.Equals(STARTEVENTID))
            {
                startRoomEvent = DataManager.Instance.RoomEventDataList[i];
                continue;
            }
            _listOfEventsByGrade[DataManager.Instance.RoomEventDataList[i].Grade].Add(DataManager.Instance.RoomEventDataList[i]);
        }
    }

    void InitializationRoom()
    {
        for (int i = 0; i < BindRoomsTr.childCount; i++)
        {
            Room room = BindRoomsTr.GetChild(i).GetComponent<Room>();
            if (room == null) continue;

            room.Data = DataManager.Instance.GetRandomRoomData();
            room.SettingRoom();
            room.RoomEvent = GetRandomRoomEvent();
            if (room.RoomEvent == null) room.RoomEvent = GetRoomEventSameID(1);
            room.ActiveRoom(false);
            _rooms.Add(room);
        }
    }

    void RequiredRoomSetup()
    {
        if (_rooms.Count < 1) return;
        //시작 룸 세팅
        int startRoomIndex = Random.Range(0, _rooms.Count);
        CurrentRoom = _rooms[startRoomIndex];
        CurrentRoom.RoomEvent = startRoomEvent;
        CurrentRoom.ActiveRoom(true);

        //만약 슬립 오브젝트 룸이 하나도 생성되어 있지 않다면, 체크해서 생성
        bool requireSleepObject = true;
        for (int i = 0; i < _rooms.Count; i++)
        {
            if (_rooms[i].RoomEvent.EventID.Equals(SLEEPEVENTID))
            {
                requireSleepObject = false;
                return;
            }
        }
        if(requireSleepObject)
        {
            List<int> roomIndexList = new List<int>();
            for (int i = 0; i < _rooms.Count; i++)
            {
                if (i.Equals(startRoomIndex)) continue;
                roomIndexList.Add(i);
            }

            int sleepRoomIndex = roomIndexList[Random.Range(0, roomIndexList.Count)];
            _rooms[sleepRoomIndex].RoomEvent = GetRoomEventSameID(SLEEPEVENTID);
        }

        InitializeRoomEvent();
        CurrentRoom.EntranceRoom();
        GameManager.Instance.PlayerCharacter.transform.position = CurrentRoom.transform.position;
    }

    RoomEventData GetRandomRoomEvent()
    {
        int index = DataManager.Instance.GetRandomValue(Percentage);
        if (index.Equals(-1)) return null;
        if (_listOfEventsByGrade[index].Count <= 0) return null;

        RoomEventData roomEventData = _listOfEventsByGrade[index][Random.Range(0, _listOfEventsByGrade[index].Count)];
        return roomEventData;
    }

    RoomEventData GetRoomEventSameID(int id)
    {
        for (int i = 0; i < _listOfEventsByGrade.Length; i++)
        {
            for (int j = 0; j < _listOfEventsByGrade[i].Count; j++)
            {
                if (_listOfEventsByGrade[i][j].EventID.Equals(id))
                    return _listOfEventsByGrade[i][j];
            }
        }
        return null;
    }

    void InitializeRoomEvent()
    {
        for (int i = 0; i < _rooms.Count; i++)
        {
            _rooms[i].RoomEvent?.InitializationEvent(_rooms[i]);
        }
    }
}
