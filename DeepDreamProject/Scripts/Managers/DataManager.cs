using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataManager : DontDestorySingleton<DataManager>
{
    [SerializeField]
    List<SceneData> _sceneDataList;
    public List<SceneData> SceneDataList { get { return _sceneDataList; } }
    [SerializeField]
    List<ItemData> _itemDataList;
    public List<ItemData> ItemDataList { get { return _itemDataList; } }
    [SerializeField]
    List<RoomData> _roomDataList;
    public List<RoomData> RoomDataList { get { return _roomDataList; } }
    [SerializeField]
    List<RoomEventData> _roomEventDataList;
    public List<RoomEventData> RoomEventDataList { get { return _roomEventDataList; } }

    public Material BasicMaterial;

    protected override void Awake()
    {
        base.Awake();
        LoadData();
    }

    void LoadData()
    {
        _sceneDataList = Resources.LoadAll<SceneData>("Data/SceneData").ToList();
        _itemDataList = Resources.LoadAll<ItemData>("Data/ItemData").ToList();
        _roomDataList = Resources.LoadAll<RoomData>("Data/RoomData").ToList();
        _roomEventDataList = Resources.LoadAll<RoomEventData>("Data/RoomEventData").ToList();        
    }

    public SceneData GetSceneDataFromName(string sceneName)
    {
        foreach (SceneData sceneData in _sceneDataList)
        {
            if (sceneData.SceneName.Equals(sceneName))
                return sceneData;
        }
        return null;
    }

    public ItemData GetItemDataFromID(int id)
    {
        foreach (ItemData itemData in _itemDataList)
        {
            if (itemData.ID.Equals(id))
                return itemData;
        }
        return null;
    }

    public RoomData GetRandomRoomData()
    {
        return RoomDataList[Random.Range(0, RoomDataList.Count)];
    }

    public Rect GetAlphaCroppedSpriteRect(Sprite sprite)
    {
        Rect croppedRect = new Rect(
            (sprite.textureRectOffset.x + sprite.textureRect.width / 2f) / sprite.pixelsPerUnit,
            (sprite.textureRectOffset.y + sprite.textureRect.height / 2f) / sprite.pixelsPerUnit,
            sprite.textureRect.width / sprite.pixelsPerUnit,
            sprite.textureRect.height / sprite.pixelsPerUnit);

        return croppedRect;
    }

    public Vector3 GetAlphaCroppedSpriteOffset(Sprite sprite)
    {
        Vector3 offset = GetAlphaCroppedSpriteRect(sprite).position - sprite.pivot / sprite.pixelsPerUnit;
        return offset;
    }

    public Vector2 GetAlphaCroppedSpriteSize(Sprite sprite)
    {
        Vector2 size = GetAlphaCroppedSpriteRect(sprite).size;
        return size;
    }

    public int GetRandomValue(float[] percentage)
    {
        float maxPercent = 0f;
        for (int i = 0; i < percentage.Length; i++)
        {
            maxPercent += percentage[i];
        }

        float rand = Random.Range(0, maxPercent);
        float currentValue = 0f;
        for (int i = 0; i < percentage.Length; i++)
        {            
            currentValue += percentage[i];
            if (rand <= currentValue)
                return i;
        }
        return -1;
    }    
}
