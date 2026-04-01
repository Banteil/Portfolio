using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorState { NONE, OPEN, CLOSE }

public class Door : MonoBehaviour
{
    [Header("Door Info")]
    [SerializeField]
    DoorState _state;
    public DoorState State
    {
        get { return _state; }
        set
        {
            if (_state.Equals(DoorState.NONE)) return;
            _state = value;
            _animator.SetInteger("State", (int)_state);
            _collider.enabled = !_state.Equals(DoorState.OPEN);
        }
    }
    public Transform MovePos;
    public Transform Model;

    [Header("Connect Info")]
    public Room OwnerRoom;
    public Door ConnectDoor;

    protected Collider2D _collider;
    protected Animator _animator;

    private void Awake()
    {
        Model = transform.Find("Model");
        _collider = GetComponent<Collider2D>();
        _animator = Model.GetComponent<Animator>();
        _animator.keepAnimatorStateOnDisable = true;
        OwnerRoom = GetComponentInParent<Room>();
        State = DoorState.OPEN;
    }

    private void OnEnable()
    {
        _animator.SetInteger("State", (int)_state);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!State.Equals(DoorState.OPEN) || !collision.gameObject.layer.Equals(LayerMask.NameToLayer("Character"))) return;
        Character character = collision.gameObject.GetComponentInParent<Character>();
        if (character != null)
        {
            if (character.ControlType.Equals(CharacterControlType.AI)) return;
            StartCoroutine(RoomMoveProcess(character));
        }
    }

    IEnumerator RoomMoveProcess(Character character)
    {
        FollowCamera2D followCamera = Camera.main.GetComponent<FollowCamera2D>();
        followCamera.StopFollow = true;
        InputManager.Instance.InputNotAllowed = true;

        ConnectDoor.OwnerRoom.ActiveRoom(true);

        //ĳ���� ��������Ʈ ũ�� �߾� ��ġ�� ����
        Sprite characterSprite = ((SpriteRenderer)character.Renderer).sprite;
        float characterSpriteSizeY = DataManager.Instance.GetAlphaCroppedSpriteSize(characterSprite).y;
        character.transform.position = ConnectDoor.MovePos.position - new Vector3(0f, characterSpriteSizeY * 0.5f);

        Vector3 targetPos = ConnectDoor.OwnerRoom.transform.position;
        targetPos.z = followCamera.transform.position.z;
        while(true)
        {
            followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, targetPos, 10f * Time.deltaTime);
            if (Vector3.Distance(followCamera.transform.position, targetPos) <= float.Epsilon + 0.1f)
                break;

            yield return null;
        }

        ConnectDoor.OwnerRoom.EntranceRoom();
        OwnerRoom.MinimapObjectSR.color = Color.gray;
        OwnerRoom.ActiveRoom(false);
        InputManager.Instance.InputNotAllowed = false;
        followCamera.StopFollow = false;
    }
}
