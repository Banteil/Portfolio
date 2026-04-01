using MoreMountains.Feedbacks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Info")]
    public RoomData Data;
    public Transform BindGround;
    public Transform BindWalls;
    public Transform BindDoors;
    public Transform BindObjects;
    public Transform BindMinimapObject;

    [Header("Room Event Info")]
    public RoomEventData RoomEvent;
    public bool IsClear;

    [Header("Doors")]
    public Door TopDoor;
    public Door BottomDoor;
    public Door LeftDoor;
    public Door RightDoor;

    [Header("Connect Rooms")]
    public Room TopRoom;
    public Room BottomRoom;
    public Room LeftRoom;
    public Room RightRoom;

    [Header("Minimap Objects")]
    public SpriteRenderer MinimapObjectSR;

    [Header("Feedbacks")]
    public MMFeedbacks DoorOpenFeedbacks;
    public MMFeedbacks DoorCloseFeedbacks;

    SpriteRenderer _groundSR;
    List<SpriteRenderer> _wallSRList;
    BoxCollider2D _roomBoundCollider;
    public BoxCollider2D RoomBoundCollider { get { return _roomBoundCollider; } }

    protected List<GameObject> _objects = new List<GameObject>();
    public List<GameObject> Objects { get { return _objects; } }
    public int CurrentCount { get; set; }

    private void Awake()
    {
        _groundSR = BindGround.GetComponent<SpriteRenderer>();
        _roomBoundCollider = GetComponent<BoxCollider2D>();
        _wallSRList = BindWalls.GetComponentsInChildren<SpriteRenderer>().ToList();
        BindMinimapObject.gameObject.SetActive(false);
        InitializeFeedbacks();
    }

    void InitializeFeedbacks()
    {
        DoorOpenFeedbacks?.Initialization(gameObject);
        DoorCloseFeedbacks?.Initialization(gameObject);
    }

    private void Start()
    {
        SettingConnectRoom();
    }

    public void SettingRoom()
    {
        if (Data == null) return;

        _groundSR.sprite = Data.GroundSprite;
        for (int i = 0; i < _wallSRList.Count; i++)
        {
            _wallSRList[i].sprite = Data.WallSprite;
        }
    }

    void SettingConnectRoom()
    {
        TopRoom = GetDirectionOtherRoom(Vector2.up);
        BottomRoom = GetDirectionOtherRoom(Vector2.down);
        LeftRoom = GetDirectionOtherRoom(Vector2.left);
        RightRoom = GetDirectionOtherRoom(Vector2.right);

        if (TopRoom == null) TopDoor.State = DoorState.NONE;
        else TopDoor.ConnectDoor = TopRoom.BottomDoor;
        if (BottomRoom == null) BottomDoor.State = DoorState.NONE;
        else BottomDoor.ConnectDoor = BottomRoom.TopDoor;
        if (LeftRoom == null) LeftDoor.State = DoorState.NONE;
        else LeftDoor.ConnectDoor = LeftRoom.RightDoor;
        if (RightRoom == null) RightDoor.State = DoorState.NONE;
        else RightDoor.ConnectDoor = RightRoom.LeftDoor;
    }

    Room GetDirectionOtherRoom(Vector2 direction)
    {
        int layerMask = 1 << LayerMask.NameToLayer("Room");
        RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, direction, _groundSR.bounds.size.x, layerMask);

        if (hit.Length > 1)
        {
            if (hit[1])
                return hit[1].collider.GetComponent<Room>();
            else
                return null;
        }
        else
            return null;
    }

    public void EntranceRoom()
    {
        RoomManager.Instance.CurrentRoom = this;
        //미니맵 관련 처리
        RoomManager.Instance.MinimapCC.SetPosition(transform.position);
        BindMinimapObject.gameObject.SetActive(true);
        MinimapObjectSR.color = Color.white;

        SetCamera();
        if (!IsClear)
        {
            CloseDoors();
            RoomEvent?.MeetingEvent(this);
        }
    }

    public void OpenDoors()
    {
        TopDoor.State = DoorState.OPEN;
        BottomDoor.State = DoorState.OPEN;
        LeftDoor.State = DoorState.OPEN;
        RightDoor.State = DoorState.OPEN;
        DoorOpenFeedbacks?.PlayFeedbacks(transform.position);
    }

    public void CloseDoors()
    {
        TopDoor.State = DoorState.CLOSE;
        BottomDoor.State = DoorState.CLOSE;
        LeftDoor.State = DoorState.CLOSE;
        RightDoor.State = DoorState.CLOSE;
        DoorCloseFeedbacks?.PlayFeedbacks(transform.position);
    }

    public void SetCamera()
    {
        FollowCamera2D followCamera = Camera.main.GetComponent<FollowCamera2D>();
        followCamera.BoundCollider = _roomBoundCollider;
    }

    public void ActiveRoom(bool active)
    {
        BindGround.gameObject.SetActive(active);
        BindWalls.gameObject.SetActive(active);
        BindDoors.gameObject.SetActive(active);
        BindObjects.gameObject.SetActive(active);
    }
}
