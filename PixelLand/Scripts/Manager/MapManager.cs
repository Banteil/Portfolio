using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    private static MapManager instance = null;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;

            objectsTr = transform.GetChild(2);
            npcTr = transform.GetChild(3);
            monsterTr = transform.GetChild(4);
            for (int i = 0; i < transform.GetChild(5).childCount; i++)
            {
                searchingPosList.Add(transform.GetChild(5).GetChild(i));
            }
            spawnPos = transform.GetChild(6);
            mapCanvas = transform.GetChild(7);
            villageArea = transform.GetChild(8).GetComponent<VillageArea>();
            dropItemTr = transform.GetChild(9);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static MapManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    Transform spawnPos;
    public Transform SpawnPos { get { return spawnPos; } }
    Transform objectsTr;
    public Transform ObjectsTr { get { return objectsTr; } }
    Transform npcTr;
    public Transform NPCTr { get { return npcTr; } }
    Transform monsterTr;
    public Transform MonsterTr { get { return monsterTr; } }
    List<Transform> searchingPosList = new List<Transform>();
    public List<Transform> SearchingPosList { get { return searchingPosList; } }
    public Tilemap biggerTilemap;

    Transform mapCanvas;
    public Transform MapCanvas { get { return mapCanvas; } }

    VillageArea villageArea;
    public VillageArea Villages { get { return villageArea; } }

    Transform dropItemTr;
    public Transform DropItemTr { get { return dropItemTr; } }
    Queue<GameObject> dropItemObjects = new Queue<GameObject>();
    public Queue<GameObject> DropItemObjects { get { return dropItemObjects; } }

    [SerializeField]
    string _mapName;
    public string MapName { get { return _mapName; } }

    [SerializeField]
    string _subInfo;
    public string SubInfo { get { return _subInfo; } }

    [SerializeField]
    AudioClip bgm;

    void Start()
    {
        Setting();
        LoadMapData();
        CharacterManager.Instance.ResetMapObject();
        SoundManager.Instance.BGM.clip = bgm;
        SoundManager.Instance.BGM.Play();
    }

    void Setting()
    {
        //리소스 세팅이 안됐으면 리소스부터 다 로딩
        if (!ResourceManager.Instance.SettingComplete)
            ResourceManager.Instance.SettingResource();

        //인벤토리가 열려있으면 닫음
        if (UIManager.Instance.GetUI("Inventory").gameObject.activeSelf)
            UIManager.Instance.GetWindow("Inventory");

        for (int i = 0; i < 300; i++)
        {
            GameObject drop = Instantiate(ResourceManager.Instance.DropItemObj, dropItemTr, false);
            drop.SetActive(false);
            DropItemObjects.Enqueue(drop);
        }

        //게임 매니저가 관리하는 플레이어(나) 캐릭터가 없으면 생성 후 세팅
        if (GameManager.Instance.Player == null)
        {
            GameObject playerObj = Instantiate(Resources.Load<GameObject>("Prefabs/Character/Hero"));
            playerObj.transform.position = spawnPos.position;
            Camera.main.GetComponent<CameraController>().Target = playerObj.transform;
            Camera.main.transform.position = playerObj.transform.position;
            GameObject petObj = Instantiate(Resources.Load<GameObject>("Prefabs/Character/Pet"));
        }
        else //있는데 포탈을 탔는지, 아닌지 여부에 따라 위치 세팅
        {
            if (!GameManager.Instance.PortalMove)
                GameManager.Instance.Player.transform.position = spawnPos.position;
            else
            {
                GameManager.Instance.Player.transform.position = GameManager.Instance.TransferPos;
                GameManager.Instance.PortalMove = false;
            }
            Camera.main.GetComponent<CameraController>().Target = GameManager.Instance.Player.transform;
            Camera.main.transform.position = GameManager.Instance.Player.transform.position;

            if (GameManager.Instance.Player.InteractUI == null)
            {
                GameObject obj = Instantiate(ResourceManager.Instance.InterUI, mapCanvas, false);
                GameManager.Instance.Player.InteractUI = obj.GetComponent<InteractionUI>();
                GameManager.Instance.Player.InteractUI.Target = GameManager.Instance.Player;
            }
        }
        UIManager.Instance.GetUI("MapInfo").GetComponent<MapInfoUI>().SetMapInfo(_mapName, _subInfo);
    }

    public void SaveMapData()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        for (int i = 0; i < villageArea.VillageList.Count; i++)
        {
            for (int j = 0; j < villageArea.VillageList[i].VillageLandList.Count; j++)
            {
                bool search = false;
                for (int w = 0; w < TemporaryDB.Instance.DataList.personalSpaceDataList.Count; w++)
                {
                    if (TemporaryDB.Instance.DataList.personalSpaceDataList[w].objectID.Equals(villageArea.VillageList[i].VillageLandList[j].ObjectID))
                    {
                        search = true;
                        TemporaryDB.Instance.DataList.personalSpaceDataList[w] = villageArea.VillageList[i].VillageLandList[j].Data;
                        break;
                    }
                }
                if (!search)
                {
                    TemporaryDB.Instance.DataList.personalSpaceDataList.Add(villageArea.VillageList[i].VillageLandList[j].Data);
                }
            }
        }
        sw.Stop();
        Debug.Log("저장 시간 : " + sw.ElapsedMilliseconds.ToString() + "ms");
    }

    public void LoadMapData()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        for (int i = 0; i < TemporaryDB.Instance.DataList.personalSpaceDataList.Count; i++)
        {
            for (int j = 0; j < villageArea.VillageList.Count; j++)
            {
                bool search = false;
                for (int w = 0; w < villageArea.VillageList[j].VillageLandList.Count; w++)
                {
                    if (TemporaryDB.Instance.DataList.personalSpaceDataList[i].objectID.Equals(villageArea.VillageList[j].VillageLandList[w].ObjectID))
                    {
                        search = true;
                        villageArea.VillageList[j].VillageLandList[w].Data = TemporaryDB.Instance.DataList.personalSpaceDataList[i];
                        break;
                    }
                }
                if (search) break;
            }
        }
        sw.Stop();
        Debug.Log("로드 시간 : " + sw.ElapsedMilliseconds.ToString() + "ms");
    }
}
