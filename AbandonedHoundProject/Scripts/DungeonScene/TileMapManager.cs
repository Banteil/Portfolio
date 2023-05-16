using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

///<summary>
///스테이지, 구역 입장 시 XML에 저장된 맵 구조를 읽어 객체를 배치하는 매니저 클래스 
///</summary>
public class TileMapManager : Singleton<TileMapManager>
{
    const int up = 0;
    const int right = 1;
    const int down = 2;
    const int left = 3;
    const int maxTileAry = 30;

    GameObject floor;
    GameObject[] wall = new GameObject[4];
    GameObject[] door = new GameObject[4];
    GameObject[] entrance = new GameObject[4];
    GameObject[] exit = new GameObject[4];
    GameObject[] broken = new GameObject[4];
    GameObject underground;
    GameObject switchObj;
    GameObject chest;
    GameObject eventObj;
    GameObject boss;
    GameObject monster;
    GameObject player;

    Tile[,] tileMap = new Tile[maxTileAry, maxTileAry];
    XmlDocument xmlDoc;
    List<int> tileEventPercentageList = new List<int>();

    private void Awake()
    {
        for (int i = 0; i < maxTileAry; i++)
        {
            for (int j = 0; j < maxTileAry; j++)
            {
                tileMap[i, j] = new Tile();
            }
        }
        //maxTileAry x maxTileAry 크기의 타일맵 세팅

        int temp = 1;
        while(true)
        {
            string tempFilePath = Application.persistentDataPath + "/Map/Stage" + DataPassing.stageNum + "_" + temp + "_Temp.xml";
 
            if (System.IO.File.Exists(tempFilePath))
                System.IO.File.Delete(tempFilePath);
            else break;

            temp++;
        }

        LoadResources(DataPassing.stageNum); //정비 씬에서 넘겨받은 stageNum을 토대로 리소르를 로딩
        LoadXMLData(DataPassing.stageNum, 1); //stageNum과 동일한 stage XML을 로딩, 첫 시작이기에 구역은 1 고정
    }

    ///<summary>
    ///불러올 스테이지에서 쓰일 리소스들을 미리 로딩하는 함수
    ///</summary>
    void LoadResources(int stageNum)
    {
        floor = Resources.Load("DungeonObject/Floor_" + stageNum) as GameObject;
        for (int i = 0; i < 4; i++)
        {
            wall[i] = Resources.Load("DungeonObject/Wall_" + stageNum + "_" + (i + 1)) as GameObject;
            door[i] = Resources.Load("DungeonObject/Door_" + stageNum + "_" + (i + 1)) as GameObject;
            entrance[i] = Resources.Load("DungeonObject/Entrance_" + stageNum + "_" + (i + 1)) as GameObject;
            exit[i] = Resources.Load("DungeonObject/Exit_" + stageNum + "_" + (i + 1)) as GameObject;
            broken[i] = Resources.Load("DungeonObject/BrokenWall_" + stageNum + "_" + (i + 1)) as GameObject;
        }
        underground = Resources.Load("DungeonObject/Underground_" + stageNum) as GameObject;
        chest = Resources.Load("DungeonObject/Chest_" + stageNum) as GameObject;
        switchObj = Resources.Load("DungeonObject/Switch_" + stageNum) as GameObject;
        eventObj = Resources.Load("DungeonObject/EventObj") as GameObject;
        boss = Resources.Load("Boss") as GameObject;
        boss.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprite/Boss_" + DataPassing.stageNum);
        monster = Resources.Load("Monster") as GameObject;
        player = GameObject.FindGameObjectWithTag("Player");

        TextAsset textAsset = Resources.Load("XML/TileEventDB") as TextAsset;
        XmlDocument tileEvnetDB = new XmlDocument();
        tileEvnetDB.LoadXml(textAsset.text);
        XmlNodeList node_Table = tileEvnetDB.SelectNodes("rows/row");
        XmlNodeList per_table = tileEvnetDB.GetElementsByTagName("percentage");

        for (int i = 0; i < node_Table.Count; i++)
        {
            int perCount = int.Parse(per_table[i].InnerText);
            for (int j = 0; j < perCount; j++)
            {
                tileEventPercentageList.Add(i);
            }
        }
    }

    ///<summary>
    ///매개변수와 동일한 숫자의 XML(맵 구조 정보)를 불러와 xmlDoc에 저장하는 함수
    ///</summary>
    public void LoadXMLData(int stageNum, int areaNum)
    {
        xmlDoc = new XmlDocument();
        string tempFilePath = Application.persistentDataPath + "/Map/Stage" + stageNum + "_" + areaNum + "_Temp.xml";
        string filePath = Application.persistentDataPath + "/Map/Stage" + stageNum + "_" + areaNum + ".xml";

        if (System.IO.File.Exists(tempFilePath)) xmlDoc.Load(tempFilePath);
        else if (System.IO.File.Exists(filePath)) xmlDoc.Load(filePath);
        else
        {
            TextAsset textAsset = Resources.Load("XML/Stage" + stageNum + "_" + areaNum + "_Origin") as TextAsset;
            xmlDoc.LoadXml(textAsset.text);
        }

        LoadMapData();
    }

    public bool CheckNextAreaData(int stageNum, int areaNum)
    {
        TextAsset textAsset = Resources.Load("XML/Stage" + stageNum + "_" + areaNum + "_Origin") as TextAsset;
        if (textAsset == null) return false;
        else return true;
    }

    ///<summary>
    ///플레이어가 상호작용하여 변경된 맵 정보를 임시로 저장하는 함수
    ///</summary>
    ///<param name="stageNum">스테이지 숫자</param> 
    ///<param name="areaNum">스테이지 내 구역 숫자</param> 
    public void SaveTempMapData(int stageNum, int areaNum)
    {
        string filePath = Application.persistentDataPath + "/Map/Stage" + stageNum + "_" + areaNum + "_Temp.xml";
        XmlDocument saveXmlDoc = new XmlDocument();

        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/Map/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Map/");

        saveXmlDoc.AppendChild(saveXmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
        //xml의 버전과 인코딩 방식을 정해준다.

        // 루트 노드 생성
        XmlNode root = saveXmlDoc.CreateNode(XmlNodeType.Element, "rows", string.Empty);
        saveXmlDoc.AppendChild(root);

        for (int i = 0; i < maxTileAry; i++)
        {
            // 자식 노드 생성
            XmlNode child = saveXmlDoc.CreateNode(XmlNodeType.Element, "row", string.Empty);
            root.AppendChild(child);

            for (int j = 0; j < maxTileAry; j++)
            {
                // 자식 노드에 들어갈 속성 생성
                XmlElement name = saveXmlDoc.CreateElement("w" + j);
                string tileInfo = DungeonManager.Instance.CurrentTileInfo(i, j);
                name.InnerText = tileInfo;
                child.AppendChild(name);
            }
        }

        saveXmlDoc.Save(filePath);

        Debug.Log("저장 완료");
    }

    public void SaveMapData(int stageNum, int maxAreaNum)
    {
        for (int i = 1; i <= maxAreaNum; i++)
        {
            string filePath = Application.persistentDataPath + "/Map/Stage" + stageNum + "_" + i + ".xml";
            string tempFilePath = Application.persistentDataPath + "/Map/Stage" + stageNum + "_" + i + "_Temp.xml";
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath); //경로에 이미 저장된 파일이 있으면 삭제

            if (System.IO.File.Exists(tempFilePath)) //임시 파일이 있다면
                System.IO.File.Move(tempFilePath, filePath); //Temp 파일을 기존 파일로 대체
            else
            {
                XmlDocument saveXmlDoc = new XmlDocument();

                if (!System.IO.Directory.Exists(Application.persistentDataPath + "/Map/"))
                    System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Map/");

                saveXmlDoc.AppendChild(saveXmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                //xml의 버전과 인코딩 방식을 정해준다.

                // 루트 노드 생성
                XmlNode root = saveXmlDoc.CreateNode(XmlNodeType.Element, "rows", string.Empty);
                saveXmlDoc.AppendChild(root);

                for (int j = 0; j < maxTileAry; j++)
                {
                    // 자식 노드 생성
                    XmlNode child = saveXmlDoc.CreateNode(XmlNodeType.Element, "row", string.Empty);
                    root.AppendChild(child);

                    for (int w = 0; w < maxTileAry; w++)
                    {
                        // 자식 노드에 들어갈 속성 생성
                        XmlElement name = saveXmlDoc.CreateElement("w" + w);
                        string tileInfo = DungeonManager.Instance.CurrentTileInfo(j, w);
                        name.InnerText = tileInfo;
                        child.AppendChild(name);
                    }
                }

                saveXmlDoc.Save(filePath);
            }
        }

        Debug.Log("저장 완료");
    }

    public void DeleteMapData(bool allData)
    { 
        if (!allData) DungeonManager.Instance.areaNum++;
        for (int i = 1; i < DungeonManager.Instance.areaNum; i++)
        {
            string filePath = Application.persistentDataPath + "/Map/Stage" + DataPassing.stageNum + "_" + i + ".xml";
            string tempFilePath = Application.persistentDataPath + "/Map/Stage" + DataPassing.stageNum + "_" + i + "_Temp.xml";
            if (allData && System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            if (System.IO.File.Exists(tempFilePath))
                System.IO.File.Delete(tempFilePath);           
        }

        int temp = 1;
        while (true)
        {
            string tempFilePath = Application.persistentDataPath + "/Map/Stage" + DataPassing.stageNum + "_" + temp + "_Temp.xml";
            if (System.IO.File.Exists(tempFilePath))
                System.IO.File.Delete(tempFilePath);
            else break;

            temp++;
        }

        Debug.Log("스테이지 데이터 초기화 완료");
    }

    ///<summary>
    ///현재 생성된 맵 객체를 삭제한 후 타일맵 정보, xml정보를 초기화하는 함수<br/>
    ///새로운 구역으로 넘어갈 때 호출 필요
    ///</summary>
    public void DeleteCurrntMap()
    {
        Destroy(GameObject.Find("TileMap"));
        xmlDoc = null;
    }

    public string GetSDConnectInfo(int i, int j, int w)
    {
        XmlNodeList cost_Table = xmlDoc.GetElementsByTagName("w" + j); //w + i를 기준으로 테이블을 나눔
        string[] tileInfo = cost_Table.Item(i).InnerText.Split('\x020');

        return tileInfo[w];
    }

    ///<summary>
    ///xmlDoc에 저장된 맵 구조를 토대로 [maxTileAry][maxTileAry] 크기의 tileMap 변수에 정보를 각각 대입하는 함수
    ///</summary>
    void LoadMapData()
    {
        for (int i = 0; i < maxTileAry; i++)
        {
            int j = 0; //foreach문 안에서 이중 for문을 쓰듯이 사용할 임시 int 변수
            string[] tileInfo; //타일 한 칸의 정보를 나눠서 저장할 string 배열 변수 (ex. 1 1 1 1 1)
            XmlNodeList cost_Table = xmlDoc.GetElementsByTagName("w" + i); //w + i를 기준으로 테이블을 나눔
            foreach (XmlNode cost in cost_Table)
            {
                tileInfo = cost.InnerText.Split('\x020');
                //temp string 변수에 w~ cost_Table 내 cost 정보를 띄어쓰기 기준으로 분류하여 저장
                //순서대로 tile, wall 상하좌우 정보로 분류

                if (tileInfo[0].Contains("6_")) tileMap[j, i].tile = TileType.SWITCH;
                else if (tileInfo[0].Contains("4_T")) tileMap[j, i].tile = TileType.TREASURE;
                else if (tileInfo[0].Contains("7_")) tileMap[j, i].tile = TileType.EVENT;
                else tileMap[j, i].tile = (TileType)int.Parse(tileInfo[0]);

                for (int w = 0; w < 4; w++)
                {
                    if (tileInfo[w + 1].Contains("2_")) tileMap[j, i].wall[w] = WallType.DOOR;
                    else tileMap[j, i].wall[w] = (WallType)int.Parse(tileInfo[w + 1]);
                } //벽 정보 상,우,하,좌 순서로 대입
                switch (tileInfo[5])
                {
                    case "T":
                        tileMap[j, i].miniMapCheck = true;
                        break;
                    case "F":
                        tileMap[j, i].miniMapCheck = false;
                        break;
                } //미니맵 체크 정보 대입

                j++;
            }
        } //w0 ~ w29까지의 정보를 대입

        DungeonManager.Instance.currTileInfo = (Tile[,])tileMap.Clone();
        //맵의 정보를 모두 읽어왔다면, 던전 매니저에게 타일 맵 정보를 넘겨줌
        MapSetting(); //로딩된 정보를 토대로 맵 세팅 시작
    }

    ///<summary>
    ///tileMap 구조를 토대로 맵 객체를 생성, 배치하는 함수
    ///</summary>
    void MapSetting()
    {
        Transform tileHolder = new GameObject("TileMap").transform;
        //Instantiate될 객체들을 모두 묶어 정리할 TileMap 객체를 생성하여 tileHolder로 분류
        //Hierarchy 상위 위치 겸 SetParent 사용이 용이하도록 하기 위함

        int maxRespawnMonster = 5 + DataPassing.stageNum;

        for (int i = 0; i < maxTileAry; i++)
        {
            for (int j = 0; j < maxTileAry; j++)
            {
                if (tileMap[i, j].tile == TileType.EMPTY) continue;
                //타일의 타입이 EMPTY라면 생성과정 생략
                else
                {
                    TileLocation tileLocation = new TileLocation();
                    tileLocation.x = j;
                    tileLocation.y = i;
                    if (tileMap[i, j].tile == TileType.START) tileLocation.isStart = true;
                    else tileLocation.isStart = false;
                    DungeonManager.Instance.tileLocationList.Add(tileLocation);
                }


                Transform tile = new GameObject(i + "," + j).transform;
                //tileHolder를 부모로 할 i,j 객체를 생성하여 tile로 분류
                //tile은 해당 객체 내의 floor, wall 등 객체들을 자식으로 묶어 관리함
                //이름을 i,j로 지어 캐릭터 이동 좌표를 알아내기 용이하게 함

                GameObject instance = Instantiate(floor, floor.transform.position + new Vector3(j * 4f, 0f, i * -4f), Quaternion.identity);
                instance.transform.SetParent(tile);

                switch (tileMap[i, j].tile)
                {
                    case TileType.NORMAL:
                        {
                            if (DataPassing.stageNum == 0) break;

                            if (maxRespawnMonster > 0 && Random.Range(0, 10) == 0)
                            {
                                Instantiate(monster, instance.transform.position + new Vector3(-2f, 1.5f, 2f), Quaternion.identity);
                                maxRespawnMonster--;
                            }
                        }
                        break;
                    case TileType.START:
                        {
                            if (!DungeonManager.Instance.isNext) break;

                            player.transform.position = new Vector3(j * 4f, 1.5f, i * -4f);
                            player.GetComponent<PlayerMovement>().x = j;
                            player.GetComponent<PlayerMovement>().y = i;
                            DungeonManager.Instance.x = j;
                            DungeonManager.Instance.y = i;
                        }
                        break;
                    case TileType.END:
                        {
                            if (DungeonManager.Instance.isNext) break;

                            player.transform.position = new Vector3(j * 4f, 1.5f, i * -4f);
                            player.GetComponent<PlayerMovement>().x = j;
                            player.GetComponent<PlayerMovement>().y = i;
                            DungeonManager.Instance.x = j;
                            DungeonManager.Instance.y = i;
                        }
                        break;
                    case TileType.TREASURE:
                        {
                            XmlNodeList cost_Table = xmlDoc.GetElementsByTagName("w" + j);
                            string[] tileInfo = cost_Table.Item(i).InnerText.Split('\x020');

                            if (!tileInfo[0].Contains("4_T"))
                            {
                                GameObject tempChest = Instantiate(chest, chest.transform.position + new Vector3(j * 4f, 0f, i * -4f), Quaternion.identity);
                                tempChest.transform.SetParent(tile);
                                GameObject chestMark = Instantiate(Resources.Load("DungeonObject/ChestMark") as GameObject);
                                chestMark.transform.SetParent(instance.transform.GetChild(0));
                                chestMark.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                            }
                        }
                        break;
                    case TileType.BOSS:
                        {
                            if (!DataPassing.stageClear[DataPassing.stageNum])
                                Instantiate(boss, instance.transform.position + new Vector3(-2f, 1.5f, 2f), Quaternion.identity);
                        }
                        break;
                    case TileType.SWITCH:
                        {
                            GameObject tempSwitch = Instantiate(switchObj, switchObj.transform.position + new Vector3(j * 4f, 0f, i * -4f), Quaternion.identity);
                            tempSwitch.transform.SetParent(tile);
                            SwitchAct switchAct = tempSwitch.GetComponent<SwitchAct>();
                            XmlNodeList cost_Table = xmlDoc.GetElementsByTagName("w" + j);
                            string[] tileInfo = cost_Table.Item(i).InnerText.Split('\x020');

                            if (tileInfo[0].Contains("6_"))
                            {
                                string[] temp;
                                temp = tileInfo[0].Split('_');
                                switchAct.switchIndex = int.Parse(temp[1]);
                            }
                            else
                            {
                                switchAct.active = true;
                            }
                            GameObject swtichMark = Instantiate(Resources.Load("DungeonObject/SwitchMark") as GameObject);
                            swtichMark.transform.SetParent(instance.transform.GetChild(0));
                            swtichMark.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                        }
                        break;
                    case TileType.EVENT:
                        {
                            GameObject tempEvent = Instantiate(eventObj, eventObj.transform.position + new Vector3(j * 4f, 0f, i * -4f), Quaternion.identity);
                            tempEvent.transform.SetParent(tile);
                            TriggerEvent triggerEvent = tempEvent.GetComponent<TriggerEvent>();
                            XmlNodeList cost_Table = xmlDoc.GetElementsByTagName("w" + j);
                            string[] tileInfo = cost_Table.Item(i).InnerText.Split('\x020');

                            if (tileInfo[0].Contains("7_"))
                            {
                                string[] temp;
                                temp = tileInfo[0].Split('_');
                                triggerEvent.eventNum = int.Parse(temp[1]);
                            }
                        }
                        break;
                }
                //tileMap[i,j]의 tile 종류에 따라 floor 객체 생성 및 타일에 맞는 객체 추가 배치 등에 차이를 둠
                if (tileMap[i, j].miniMapCheck) instance.transform.GetChild(0).gameObject.SetActive(true);
                //해당 좌표 타일의 미니맵 체크가 true일 경우, 타일이 미니맵에서 보이도록 조정

                for (int w = 0; w < 4; w++)
                {
                    switch (tileMap[i, j].wall[w])
                    {
                        case WallType.NORMAL:
                            {
                                instance = Instantiate(wall[w], wall[w].transform.position + new Vector3(j * 4f, 0f, i * -4f), wall[w].transform.rotation);
                                instance.transform.SetParent(tile);
                            }
                            break;
                        case WallType.DOOR:
                            {
                                instance = Instantiate(door[w], door[w].transform.position + new Vector3(j * 4f, 0f, i * -4f), door[w].transform.rotation);
                                instance.transform.SetParent(tile);
                                DoorAct doorAct = instance.GetComponent<DoorAct>();
                                doorAct.direction = w;

                                XmlNodeList cost_Table = xmlDoc.GetElementsByTagName("w" + j);
                                string[] tileInfo = cost_Table.Item(i).InnerText.Split('\x020');

                                if (tileInfo[w + 1].Contains("2_"))
                                {
                                    string[] temp;
                                    temp = tileInfo[w + 1].Split('_');
                                    doorAct.doorIndex = int.Parse(temp[1]);
                                    doorAct.doorLock = true;
                                }
                            }
                            break;
                        case WallType.ENTRANCE:
                            {
                                instance = Instantiate(entrance[w], entrance[w].transform.position + new Vector3(j * 4f, 0f, i * -4f), entrance[w].transform.rotation);
                                instance.transform.SetParent(tile);
                                if (DungeonManager.Instance.isNext)
                                {
                                    player.GetComponent<PlayerMovement>().currDirection = (w + 2) % 4;
                                    int direction = player.GetComponent<PlayerMovement>().currDirection;

                                    switch (direction)
                                    {
                                        case 0:
                                            player.transform.rotation = Quaternion.Euler(0, 0, 0);
                                            break;
                                        case 1:
                                            player.transform.rotation = Quaternion.Euler(0, 90f, 0);
                                            break;
                                        case 2:
                                            player.transform.rotation = Quaternion.Euler(0, -180f, 0);
                                            break;
                                        case 3:
                                            player.transform.rotation = Quaternion.Euler(0, -90f, 0);
                                            break;
                                    }
                                }
                            }
                            break;
                        case WallType.EXIT:
                            {
                                instance = Instantiate(exit[w], exit[w].transform.position + new Vector3(j * 4f, 0f, i * -4f), exit[w].transform.rotation);
                                instance.transform.SetParent(tile);
                                if (!DungeonManager.Instance.isNext)
                                {
                                    player.GetComponent<PlayerMovement>().currDirection = (w + 2) % 4;
                                    int direction = player.GetComponent<PlayerMovement>().currDirection;

                                    switch (direction)
                                    {
                                        case 0:
                                            player.transform.rotation = Quaternion.Euler(0, 0, 0);
                                            break;
                                        case 1:
                                            player.transform.rotation = Quaternion.Euler(0, 90f, 0);
                                            break;
                                        case 2:
                                            player.transform.rotation = Quaternion.Euler(0, -180f, 0);
                                            break;
                                        case 3:
                                            player.transform.rotation = Quaternion.Euler(0, -90f, 0);
                                            break;
                                    }
                                }
                            }
                            break;
                        case WallType.BROKEN:
                            {
                                instance = Instantiate(broken[w], broken[w].transform.position + new Vector3(j * 4f, 0f, i * -4f), broken[w].transform.rotation);
                                instance.transform.SetParent(tile);
                            }
                            break;
                        case WallType.TERRAIN:
                            {
                                instance = Instantiate(wall[w], wall[w].transform.position + new Vector3(j * 4f, 0f, i * -4f), wall[w].transform.rotation);
                                instance.transform.SetParent(tile);                                
                                Destroy(instance.GetComponent<Wall>());
                                instance.layer = 0;
                            }
                            break;
                    }
                    if (tileMap[i, j].miniMapCheck) instance.transform.GetChild(0).gameObject.SetActive(true);
                    //해당 좌표 타일의 미니맵 체크가 true일 경우, 벽이 미니맵에서 보이도록 조정
                }
                //tileMap[i,j]의 wall 종류에 따라 wall 객체 생성 및 배치에 차이를 둠

                int rand = Random.Range(0, 1000);
                tileMap[i, j].eventType = tileEventPercentageList[rand];

                tile.SetParent(tileHolder);
            }
        }
        GameObject under = Instantiate(underground);
        under.transform.SetParent(tileHolder);

        Invoke("ConnectSwitchDoor", 1f);
        //Destroy와 동시에 실행될 경우 프레임이 끝나기 전에 삭제되지 않은 객체도 검색이 되므로 Invoke로 딜레이를 줌        
    }

    ///<summary>
    ///스위치 - 문 객체 index 확인하여 연결하는 함수
    ///</summary>
    void ConnectSwitchDoor()
    {
        GameObject[] switches = GameObject.FindGameObjectsWithTag("SWITCH");
        GameObject[] doors = GameObject.FindGameObjectsWithTag("DOOR");

        for (int i = 0; i < doors.Length; i++)
        {
            DoorAct door = doors[i].GetComponent<DoorAct>();
            if (door.doorIndex == 0) continue;

            door.connectSwitchActList.Clear();

            for (int j = 0; j < switches.Length; j++)
            {
                SwitchAct swit = switches[j].GetComponent<SwitchAct>();

                if (door.doorIndex == swit.switchIndex)
                {
                    door.connectSwitchActList.Add(swit);
                    swit.door = door;
                }
            }
        }
    }

    /////<summary>
    /////타일 맵 객체 배치 시 혹시나 동일한 위치에 벽이 세워지는것을 방지하는 함수(미사용)
    /////</summary>
    //void TileWallReadjust(int i, int j)
    //{
    //    if (i != 0) if (tileMap[i - 1, j].wall[down] != WallType.EMPTY) tileMap[i, j].wall[up] = WallType.EMPTY;
    //    if (i != maxTileAry - 1) if (tileMap[i + 1, j].wall[up] != WallType.EMPTY) tileMap[i, j].wall[down] = WallType.EMPTY;
    //    if (j != 0) if (tileMap[i, j - 1].wall[right] != WallType.EMPTY) tileMap[i, j].wall[left] = WallType.EMPTY;
    //    if (j != maxTileAry - 1) if (tileMap[i, j + 1].wall[left] != WallType.EMPTY) tileMap[i, j].wall[right] = WallType.EMPTY;
    //}
}
