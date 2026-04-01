using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapCalculator : MonoBehaviour
{
    List<Tilemap> tilemaps = new List<Tilemap>();

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Tilemap temp = transform.GetChild(i).GetComponent<Tilemap>();
            if(temp != null)
                tilemaps.Add(transform.GetChild(i).GetComponent<Tilemap>());
        }

        for (int i = 0; i < tilemaps.Count; i++)
        {
            Debug.Log("'" + tilemaps[i].gameObject.name + "'의 총 타일 개수 : " + GetTileAmount(i));
            Debug.Log("'" + tilemaps[i].gameObject.name + "'의 가로 길이 : " + tilemaps[i].localBounds.size.x);
            Debug.Log("'" + tilemaps[i].gameObject.name + "'의 세로 길이 : " + tilemaps[i].localBounds.size.y);
        }
    }

    public int GetTileAmountSprite(Sprite targetSprite, int index)
    {
        int amount = 0;

        // loop through all of the tiles        
        BoundsInt bounds = tilemaps[index].cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = tilemaps[index].GetTile<Tile>(pos);
            if (tile != null)
            {
                if (tile.sprite == targetSprite)
                {
                    amount += 1;
                }
            }
        }

        return amount;
    }

    public int GetTileAmount(int index)
    {
        int amount = 0;

        // loop through all of the tiles        
        BoundsInt bounds = tilemaps[index].cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = tilemaps[index].GetTile<Tile>(pos);
            if (tile != null)
            {
                amount += 1;
            }
        }

        return amount;
    }
}
