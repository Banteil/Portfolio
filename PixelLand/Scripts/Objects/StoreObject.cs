using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreObject : MonoBehaviour
{
    [Range(1, 8)]
    public int storeIndex = 1;
    [Range(1, 13)]
    public int tentIndex = 1;

    SpriteRenderer storeSR;
    SpriteRenderer tentSR;

    private void Awake()
    {
        storeSR = GetComponent<SpriteRenderer>();
        tentSR = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Sprite store = Resources.Load<Sprite>("Sprites/Tiles/Object/store_" + storeIndex);
        storeSR.sprite = store;
        Sprite tent = Resources.Load<Sprite>("Sprites/Tiles/Object/tent_" + tentIndex);
        tentSR.sprite = tent;
    }
}
