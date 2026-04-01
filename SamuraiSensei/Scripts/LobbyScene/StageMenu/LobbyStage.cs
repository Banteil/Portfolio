using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyStage : LobbyMenu
{
    private static LobbyStage instance = null;
    public static LobbyStage Instance
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

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            for (int i = 0; i < worldMap.childCount; i++)
            {
                worldMapObjects.Add(worldMap.GetChild(i).GetComponent<WorldMapObject>());
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField]
    Transform worldMap;
    List<WorldMapObject> worldMapObjects = new List<WorldMapObject>();
    
    [SerializeField]
    StageSelectPopup stageSelectPopup;
    public StageSelectPopup SelectPopup { get { return stageSelectPopup; } }

    [SerializeField]
    StageInfoPopup stageInfoPopup;
    public StageInfoPopup StageInfoPopup { get { return stageInfoPopup; } }
      
    public void OpenStageList(int worldIndex, string name, int bossID)
    {
        stageSelectPopup.BossID = bossID;
        stageSelectPopup.WorldIndex = worldIndex;        
        stageSelectPopup.WorldNameText.text = name;        
        stageSelectPopup.gameObject.SetActive(true);        
    }

    public override void ExitMenu()
    {
        stageInfoPopup.gameObject.SetActive(false);
        stageSelectPopup.gameObject.SetActive(false);        
    }
}
