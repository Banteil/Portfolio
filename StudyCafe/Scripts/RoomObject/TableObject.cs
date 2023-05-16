using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableObject : MonoBehaviour
{
    public enum TableType { DOWN, UP, LEFT, RIGHT };
    public TableType tableType;
    Player master = null;
    public Player Master
    {
        get { return master; }
        set
        {
            master = value;
            //마스터 정보 대입 후 해당 마스터에 맞는 노트, 음료수 오브젝트 배치
            if (master != null)
            {
                if (tableType.Equals(TableType.DOWN)) return;
                StartCoroutine(SettingItems());
            }            
        }
    }

    public GameObject notepad;
    public GameObject drink;
    public Transform chairTr;
    public BoxCollider2D triggerBox;
    public bool isSit;

    private void Update()
    {
        if (RoomManager.Instance.myAvatar == null) return;
        if(!isSit) ColliderAdjustment();
    }

    /// <summary>
    /// 테이블 위의 노트, 컵의 정보를 자리 주인에게서 받아오는 함수
    /// </summary>
    IEnumerator SettingItems()
    {
        string guid = (string)master.CustomProperties["GUID"];
        yield return StartCoroutine(DataManager.Instance.InfoTransfer(InfoType.INVENTORY, guid));
        InventoryInfo invenData = JsonConvert.DeserializeObject<InventoryInfo>(DataManager.Instance.info);

        Sprite notepadSprite = Resources.Load<Sprite>("UISprite/TableItem/Notepad_" + invenData.noteType);
        Sprite drinkSprite = Resources.Load<Sprite>("UISprite/TableItem/Cup_" + invenData.cupType);
        notepad.GetComponent<SpriteRenderer>().sprite = notepadSprite;
        drink.GetComponent<SpriteRenderer>().sprite = drinkSprite;

        if (master.Equals(PhotonNetwork.LocalPlayer))
        {
            notepad.GetComponent<BoxCollider2D>().enabled = true;
            drink.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    private void OnMouseUp()
    {
        AvatarAct myAvatar = RoomManager.Instance.myAvatar.GetComponent<AvatarAct>();
        //클릭 대상이 해당 책상의 주인일 경우에만 작동
        if (PhotonNetwork.LocalPlayer.Equals(master))
            SitProcess(myAvatar);
        else
            myAvatar.SetEmote(16);
    }

    public void SitProcess(AvatarAct sitAvatar)
    {
        sitAvatar.Sitting(tableType, chairTr.position);
        sitAvatar.sitTable = this;
        triggerBox.enabled = false;
        isSit = true;
    }

    /// <summary>
    /// 테이블과 플레이어 거리에 따라 boxCollider 활성, 비활성화 처리(상호작용 가능 여부 변환하기 위함)
    /// </summary>
    void ColliderAdjustment()
    {
        //책상과 아바타의 거리가 1.7 이하일 때 상호작용 할 수 있음
        float distance = (RoomManager.Instance.myAvatar.transform.position - transform.position).magnitude;
        if (distance > 1.7f)
            triggerBox.enabled = false;
        else
            triggerBox.enabled = true;
    }
}
