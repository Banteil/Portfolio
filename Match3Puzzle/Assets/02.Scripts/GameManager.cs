using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    int max_row = 6;
    int max_column = 8;
    int max_block_type = 5;
    Tile[,] tiles;

    void Awake()
    {
        InitTilePosition();
    }

    void InitTilePosition()
    {
        tiles = new Tile[max_column, max_row];
        Transform tiles_tr = new GameObject("Tiles").transform;

        for (int i = 0; i < max_column; i++)
        {
            for (int j = 0; j < max_row; j++)
            {
                GameObject tile_obj = Instantiate(Resources.Load<GameObject>("Prefabs/Tile"), tiles_tr);
                tile_obj.transform.position = new Vector3(j * 0.75f, i * -0.75f, 0f);
                tiles[i, j] = tile_obj.GetComponent<Tile>();
                tiles[i, j].position = tile_obj.transform.position;
                tiles[i, j].block = SettingRandomBlock(i, j);
            }
        }

        SettingTileNode();
    }

    void SettingTileNode()
    {
        for (int i = 0; i < max_column; i++)
        {
            for (int j = 0; j < max_row; j++)
            {
                if (j != 0) tiles[i, j].node_left = tiles[i, j - 1];
                if (j != max_row - 1) tiles[i, j].node_right = tiles[i, j + 1];
                if (i != 0) tiles[i, j].node_up = tiles[i - 1, j];
                if (i != max_column - 1) tiles[i, j].node_down = tiles[i + 1, j];
            }
        }
    }

    Block SettingRandomBlock(int column, int row)
    {
        int sel = Random.Range(0, max_block_type);
        GameObject block_obj = Instantiate(Resources.Load<GameObject>("Prefabs/Block"), tiles[column, row].position, Quaternion.identity);
        Block block = block_obj.GetComponent<Block>();
        block.Type = (BlockType)sel;

        return block;
    }
}
