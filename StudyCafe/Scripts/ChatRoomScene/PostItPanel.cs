using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class PostItSyncObject
{

}

public class PostItPanel : MonoBehaviour
{
    [Header("Panel Object")]
    public GameObject postItUI;
    public Transform board;
    public bool isSetting;
    PhotonView pV;

    [Header("PostIt Object")]
    public Transform colorPickerTr;
    public InputField postItInput;

    [Header("PostIt Item")]
    public PostItItem modificationItem;
    public List<PostItItem> itemList = new List<PostItItem>();
    public List<PostItGroup> groupList = new List<PostItGroup>();

    [Header("Resource Objects")]
    [HideInInspector]
    public GameObject groupObj;
    [HideInInspector]
    public GameObject itemObj;

    private void Awake()
    {
        itemObj = Resources.Load<GameObject>("Prefabs/UI/PostItItem");
        groupObj = Resources.Load<GameObject>("Prefabs/UI/PostItGroup");
        pV = GetComponent<PhotonView>();
    }

    public void CompleteSetting()
    {
        if (!isSetting)
        {
            isSetting = true;
            InputControl.Instance.isValid = false;
        }
        else
        {
            isSetting = false;
            InputControl.Instance.isValid = true;
        }
    }

    #region 패널 관련 함수
    void Initialization()
    {
        PostItInitialization();
        postItUI.SetActive(false);
    }
    /// <summary>
    /// 포스트잇 패널 닫기 버튼 함수
    /// </summary>
    public void CloseButton()
    {
        Initialization();
        iTween.MoveTo(gameObject, iTween.Hash("islocal", true, "y", -1100f, "time", 1f, "oncomplete", "CompleteSetting", "oncompletetarget", gameObject));
    }

    /// <summary>
    /// 포스트잇 추가 버튼 함수
    /// </summary>
    public void AddButton()
    {
        if (modificationItem != null)
        {
            postItUI.GetComponent<Image>().color = modificationItem.ItemColor;
            postItInput.text = modificationItem.ExampleStr;
        }
        postItUI.SetActive(true);
    }
    #endregion

    #region 포스트잇 UI

    void PostItInitialization()
    {
        if (modificationItem != null)
        {
            pV.RPC("EndModification", RpcTarget.All, modificationItem.index);
            modificationItem = null;
        }
        postItInput.text = "";
        ColorPickerButton(2);
    }

    /// <summary>
    /// 포스트잇 추가 취소 버튼 함수
    /// </summary>
    public void CancelButton()
    {
        PostItInitialization();
        postItUI.SetActive(false);
    }

    /// <summary>
    /// 포스트잇 색상 픽 버튼 함수
    /// </summary>
    public void ColorPickerButton(int type)
    {
        Image postItUIImage = postItUI.GetComponent<Image>();
        Color32 pickerColor = colorPickerTr.GetChild(type).GetComponent<Image>().color;
        postItUIImage.color = pickerColor;
    }

    public void PlusPostIt()
    {
        if (modificationItem == null)
        {
            float[] randPos = new float[2];
            for (int i = 0; i < randPos.Length; i++)
                randPos[i] = Random.Range(-100f, 100f);

            float[] colorInfo = new float[3];
            colorInfo[0] = postItUI.GetComponent<Image>().color.r;
            colorInfo[1] = postItUI.GetComponent<Image>().color.g;
            colorInfo[2] = postItUI.GetComponent<Image>().color.b;

            pV.RPC("CreateItem", RpcTarget.All, randPos, colorInfo, postItInput.text);
        }
        else
        {
            float[] colors = new float[3];
            colors[0] = postItUI.GetComponent<Image>().color.r;
            colors[1] = postItUI.GetComponent<Image>().color.g;
            colors[2] = postItUI.GetComponent<Image>().color.b;

            pV.RPC("ModificationItem", RpcTarget.All, modificationItem.index, postItInput.text, colors);
        }
        CancelButton();
    }
    #endregion

    #region 포스트잇 정보 RPC
    [PunRPC]
    void CreateItem(float[] randPos, float[] colorInfo, string text)
    {
        GameObject newItem = Instantiate(itemObj, board, false);
        newItem.transform.localPosition = new Vector2(randPos[0], randPos[1]);

        PostItItem itemScript = newItem.GetComponent<PostItItem>();
        itemScript.ItemColor = new Color(colorInfo[0], colorInfo[1], colorInfo[2]);
        itemScript.ExampleStr = text;
        itemScript.board = board;
        itemScript.panel = this;
        itemScript.index = itemList.Count;
        itemList.Add(itemScript);

        SyncNewItem(itemScript.index);
    }

    /// <summary>
    /// Item 2개가 합쳐져 새로운 그룹을 생성하는 RPC 함수
    /// </summary>
    public void CreateGroup(int dropObjIndex, int dragObjIndex) => pV.RPC("CreateGroupRPC", RpcTarget.All, dropObjIndex, dragObjIndex);

    [PunRPC]
    void CreateGroupRPC(int dropObjIndex, int dragObjIndex)
    {
        GameObject groupObject = Instantiate(groupObj, board, false);
        groupObject.transform.localPosition = itemList[dropObjIndex].transform.localPosition;
        itemList[dropObjIndex].transform.SetParent(groupObject.transform, false);
        itemList[dropObjIndex].groupTr = groupObject.transform;

        itemList[dragObjIndex].transform.SetParent(groupObject.transform, false);
        itemList[dragObjIndex].groupTr = groupObject.transform;

        PostItGroup groupScript = groupObject.GetComponent<PostItGroup>();
        groupScript.index = groupList.Count;
        groupScript.panel = this;
        groupList.Add(groupScript);
        groupScript.InvokeInfo();

        SyncNewGroup(groupScript.index);
        SyncAddItem(dropObjIndex);
        SyncAddItem(dragObjIndex);
    }

    /// <summary>
    /// 생성된 그룹에 추가하는 RPC 함수
    /// </summary>
    public void AddGroup(int groupIndex, int dragObjIndex) => pV.RPC("AddGroupRPC", RpcTarget.All, groupIndex, dragObjIndex);

    [PunRPC]
    void AddGroupRPC(int groupIndex, int dragObjIndex)
    {
        itemList[dragObjIndex].transform.SetParent(groupList[groupIndex].transform, false);
        itemList[dragObjIndex].groupTr = groupList[groupIndex].transform;

        itemList[dragObjIndex].groupTr.GetComponent<PostItGroup>().InvokeInfo();
        SyncAddItem(dragObjIndex);
    }

    /// <summary>
    /// 아이템 객체를 드래그 시작, 종료할 때 처리를 진행하는 RPC 함수
    /// </summary>
    public void PostItDrag(int index) => pV.RPC("DragInfoRPC", RpcTarget.All, index);

    [PunRPC]
    void DragInfoRPC(int index)
    {
        if (!itemList[index].isDrag)
        {
            itemList[index].isDrag = true;
            itemList[index].transform.SetParent(transform, false);
            itemList[index].gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            itemList[index].gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            itemList[index].gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            if (itemList[index].groupTr != null)
            {
                if (itemList[index].groupTr.childCount <= 1)
                {
                    for (int i = 0; i < groupList.Count; i++)
                    {
                        if (groupList[i].index.Equals(itemList[index].groupTr.GetComponent<PostItGroup>().index))
                            Destroy(groupList[i].gameObject);
                    }
                }
            }
            itemList[index].groupTr = null;
        }
        else
        {
            itemList[index].isDrag = false;

            if (itemList[index].groupTr == null)
                itemList[index].transform.SetParent(board, false);
            else
                itemList[index].transform.SetParent(itemList[index].groupTr, false);

            //싱크 오브젝트 세팅
            SyncAddItem(index);
        }
    }

    /// <summary>
    /// 그룹 객체를 드래그 시작, 종료할 때 처리를 진행하는 RPC 함수
    /// </summary>
    public void GroupDrag(int index) => pV.RPC("GroupDragRPC", RpcTarget.All, index);

    [PunRPC]
    void GroupDragRPC(int index)
    {
        if (!groupList[index].isDrag)
        {
            groupList[index].isDrag = true;
            groupList[index].transform.SetParent(transform, false);
            for (int i = 1; i < groupList[index].transform.childCount; i++)
            {
                groupList[index].transform.GetChild(i).GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        else
        {
            groupList[index].isDrag = false;
            groupList[index].transform.SetParent(board, false);
            for (int i = 1; i < groupList[index].transform.childCount; i++)
            {
                groupList[index].transform.GetChild(i).GetComponent<BoxCollider2D>().enabled = true;
            }

            //싱크 오브젝트 세팅
            SyncAddGroup(index);
        }
    }

    /// <summary>
    /// 아이템의 RPC 이동 처리를 진행하는 함수
    /// </summary>
    public void PostItMove(int index, float x, float y) => pV.RPC("MovePostItRPC", RpcTarget.All, index, x, y);

    [PunRPC]
    void MovePostItRPC(int index, float x, float y)
    {
        Vector2 pos = new Vector3(x, y);
        itemList[index].transform.localPosition = pos;
    }

    /// <summary>
    /// 그룹의 RPC 이동 처리를 진행하는 함수
    /// </summary>
    public void GroupMove(int index, float x, float y) => pV.RPC("MoveGroupRPC", RpcTarget.All, index, x, y);

    [PunRPC]
    void MoveGroupRPC(int index, float x, float y)
    {
        Vector2 pos = new Vector2(x, y);
        groupList[index].transform.localPosition = pos;
    }

    public void ModificationProcess(int index)
    {
        pV.RPC("StartModification", RpcTarget.All, index);
        modificationItem = itemList[index];
        AddButton();
    }

    [PunRPC]
    void StartModification(int index) => itemList[index].isModification = true;

    [PunRPC]
    void ModificationItem(int index, string text, float[] colorInfo)
    {
        itemList[index].ItemColor = new Color(colorInfo[0], colorInfo[1], colorInfo[2]);
        itemList[index].ExampleStr = text;
        EndModification(index);

        //싱크 오브젝트 세팅
        SyncAddItem(index);
    }

    [PunRPC]
    void EndModification(int index) => itemList[index].isModification = false;

    /// <summary>
    ///Group이 Destroy될 때 본인의 정보를 List에서 삭제하는 함수
    /// </summary>
    public void DeleteGroup(int index)
    {
        groupList.RemoveAt(index);        
        for (int i = index; i < groupList.Count; i++)
        {
            groupList[i].index--;
        }
        SyncDelGroup(index);
    }

    /// <summary>
    /// Item이 Destroy될 때 본인의 정보를 List에서 삭제하는 함수
    /// </summary>
    public void DeleteItem(int index)
    {
        itemList.RemoveAt(index);
        for (int i = index; i < itemList.Count; i++)
        {
            itemList[i].index--;
        }
        SyncDelItem(index);
    }

    public void GroupNaming(int index, string name) => pV.RPC("GroupNamingRPC", RpcTarget.All, index, name);

    [PunRPC]
    void GroupNamingRPC(int index, string name)
    {
        groupList[index].groupNameField.text = name;
        SyncAddGroup(index);
    }

    #endregion

    #region 싱크 관련 함수
    void SyncNewItem(int index)
    {
        if (DataManager.isMaster)
            RoomManager.Instance.chatRoom.syncObject.postItItemInfo.Add(itemList[index].GetItemInfo());
    }

    void SyncNewGroup(int index)
    {
        if (DataManager.isMaster)
            RoomManager.Instance.chatRoom.syncObject.postItGroupInfo.Add(groupList[index].GetGroupInfo());
    }

    void SyncAddItem(int index)
    {
        //싱크 오브젝트 세팅
        if (DataManager.isMaster)
        {
            Debug.Log("ADD 아이템 인덱스 : " + index);
            for (int i = 0; i < RoomManager.Instance.chatRoom.syncObject.postItItemInfo.Count; i++)
            {
                string[] syncInfo = RoomManager.Instance.chatRoom.syncObject.postItItemInfo[i].Split('_');
                int syncIndex = int.Parse(syncInfo[0]);
                Debug.Log("인덱스 : " + index + ", 싱크 인덱스 : " + syncIndex + ", i : " + i);

                if (index.Equals(syncIndex))
                {
                    RoomManager.Instance.chatRoom.syncObject.postItItemInfo[i] = itemList[index].GetItemInfo();
                    Debug.Log("(ADD)싱크 아이템 리스트 개수 : " + RoomManager.Instance.chatRoom.syncObject.postItItemInfo.Count);
                    Debug.Log(itemList[index].GetItemInfo());
                    return;
                }
            }

            RoomManager.Instance.chatRoom.syncObject.postItItemInfo.Add(itemList[index].GetItemInfo());
            Debug.Log("(NEW)싱크 아이템 리스트 개수 : " + RoomManager.Instance.chatRoom.syncObject.postItItemInfo.Count);
            Debug.Log(itemList[index].GetItemInfo());
        }
    }

    void SyncAddGroup(int index)
    {
        //싱크 오브젝트 세팅
        if (DataManager.isMaster)
        {
            for (int i = 0; i < RoomManager.Instance.chatRoom.syncObject.postItGroupInfo.Count; i++)
            {
                string[] syncInfo = RoomManager.Instance.chatRoom.syncObject.postItGroupInfo[i].Split('_');
                int syncIndex = int.Parse(syncInfo[0]);

                if (index.Equals(syncIndex))
                {
                    RoomManager.Instance.chatRoom.syncObject.postItGroupInfo[i] = groupList[index].GetGroupInfo();
                    return;
                }
            }

            RoomManager.Instance.chatRoom.syncObject.postItGroupInfo.Add(groupList[index].GetGroupInfo());
        }
    }

    void SyncDelItem(int index)
    {        
        if (DataManager.isMaster)
        {
            RoomManager.Instance.chatRoom.syncObject.postItItemInfo.RemoveAt(index);
            for (int i = index; i < RoomManager.Instance.chatRoom.syncObject.postItItemInfo.Count; i++)
            {
                string[] syncInfo = RoomManager.Instance.chatRoom.syncObject.postItItemInfo[i].Split('_');
                int syncIndex = int.Parse(syncInfo[0]);

                syncIndex--;
                syncInfo[0] = syncIndex.ToString();
                string info = "";
                for (int j = 0; j < syncInfo.Length; j++)
                {
                    info += syncInfo[j];
                    if (j < syncInfo.Length - 1) info += "_";
                }

                RoomManager.Instance.chatRoom.syncObject.postItItemInfo[i] = info;                
            }
        }
    }

    void SyncDelGroup(int index)
    {
        if (DataManager.isMaster)
        {
            RoomManager.Instance.chatRoom.syncObject.postItGroupInfo.RemoveAt(index);
            for (int i = index; i < RoomManager.Instance.chatRoom.syncObject.postItGroupInfo.Count; i++)
            {
                string[] syncInfo = RoomManager.Instance.chatRoom.syncObject.postItGroupInfo[i].Split('_');
                int syncIndex = int.Parse(syncInfo[0]);

                syncIndex--;
                syncInfo[0] = syncIndex.ToString();
                string info = "";
                for (int j = 0; j < syncInfo.Length; j++)
                {
                    info += syncInfo[j];
                    if (j < syncInfo.Length - 1) info += "_";
                }

                RoomManager.Instance.chatRoom.syncObject.postItGroupInfo[i] = info;                
            }
        }
    }
    #endregion
}
