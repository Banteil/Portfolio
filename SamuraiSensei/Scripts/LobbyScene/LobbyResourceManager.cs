using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyResourceManager : MonoBehaviour
{
    private static LobbyResourceManager instance = null;
    public static LobbyResourceManager Instance
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
            SettingResources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SettingResources()
    {
        stageButtonObj = Resources.Load<GameObject>("Prefabs/UI/StageSelectButton");
        lineObj = Resources.Load<GameObject>("Prefabs/UI/Line");
        worldBackgroundSprite = Resources.LoadAll<Sprite>("Sprites/UI/Lobby/StageMenu/StageBackground");
        SetListSpriteArray(worldDecorationSprites, "Sprites/UI/Lobby/StageMenu/WorldDecoration/;_World", true);
    }

    void SetListSpriteArray(List<Sprite[]> array, string path, bool isSplit)
    {
        int i = 0;        
        while (true)
        {
            Sprite[] sprites = null;
            if (isSplit)
            {
                string[] split = path.Split(';');
                sprites = Resources.LoadAll<Sprite>(split[0] + i + split[1]);
            }
            else
                sprites = Resources.LoadAll<Sprite>(path + i);

            if (sprites == null || sprites.Length.Equals(0)) break;
            array.Add(sprites);
            i++;
        }
    }

    GameObject stageButtonObj;
    public GameObject StageButtonObj { get { return stageButtonObj; } }
    GameObject lineObj;
    public GameObject LineObj { get { return lineObj; } }

    Sprite[] worldBackgroundSprite;
    public Sprite[] WorldBackgroundSprite { get { return worldBackgroundSprite; } }
    List<Sprite[]> worldDecorationSprites = new List<Sprite[]>();
    public List<Sprite[]> WorldDecorationSprites { get { return worldDecorationSprites; } }
}
