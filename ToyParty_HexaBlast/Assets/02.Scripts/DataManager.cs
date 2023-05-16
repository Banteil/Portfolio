using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct Location
{
    public int row;
    public int column;

    public Location(int row, int column)
    {
        this.row = row;
        this.column = column;
    }

    public static Location operator +(Location c1, Location c2)
    {
        return new Location(c1.row + c2.row, c1.column + c2.column);
    }

    public static Location operator -(Location c1, Location c2)
    {
        return new Location(c1.row - c2.row, c1.column - c2.column);
    }
}

public class DataManager : MonoBehaviour
{
    public readonly int maxRow = 5;
    public readonly int maxColumn = 18;

    GameObject tilePrefab;
    GameObject blockPrefab;
    List<Dictionary<string, object>> csvData;    
    Tile createBlockTile;
    Transform tiles, blocks;
    Location[] evenAdjacentLocation;
    Location[] oddAdjacentLocation;

    public List<Tile> tileList;
    public Vector2[,] tilePosition;
    public int moveCount;
    public int targetCount;
    public Text moveCountText;
    public Text targetCountText;
    public Text resultText;

    ///<summary>
    ///필요 데이터를 세팅하는 함수
    ///</summary>
    public void SettingVariable(int stage)
    {
        tileList = new List<Tile>();
        tilePosition = new Vector2[maxRow, maxColumn];
        tilePrefab = Resources.Load("Prefabs/Tile") as GameObject;
        blockPrefab = Resources.Load("Prefabs/Block") as GameObject;
        csvData = CSVReader.Read("StageData_" + stage);
        moveCount = (int)csvData[0]["Info"];
        targetCount = (int)csvData[1]["Info"];

        for (int i = 0; i < maxRow; i++)
        {
            for (int j = 0; j < maxColumn; j++)
            {
                SettingTilePosition(i, j);
            }
        }

        evenAdjacentLocation = new Location[]
        {
        new Location(0, -2), new Location(0, -1),
        new Location(0, 1), new Location(0, 2),
        new Location(-1, 1), new Location(-1, -1)
        };

        oddAdjacentLocation = new Location[]
        {
        new Location(0, -2), new Location(1, -1),
        new Location(1, 1), new Location(0, 2),
        new Location(0, 1), new Location(0, -1)
        };
    }

    void SettingTilePosition(int row, int column)
    {
        if ((column % 2).Equals(0))
            tilePosition[row, column] = new Vector2(-6f + (row * 3), 5.5f - ((column / 2) * 1.7f));
        else
        {
            tilePosition[row, column] = new Vector2(-4.5f + (row * 3), 4.65f - ((column / 2) * 1.7f));
        }        
    }

    ///<summary>
    ///csv 데이터를 토대로 타일 객체를 세팅, 생성하는 함수
    ///</summary>
    void SettingTileData()
    {
        for (int i = 0; i < maxRow; i++)
        {
            string key = "Row_" + i;

            for (int j = 0; j < maxColumn; j++)
            {
                string data = csvData[j][key].ToString();
                if (data.Equals("0"))
                {
                    continue;
                }

                GameObject tileObj = Instantiate(tilePrefab, tilePosition[i, j], Quaternion.identity);
                tileObj.transform.SetParent(tiles);
                tileObj.name = "tile(" + i + "," + j + ")";

                Tile tempTile = tileObj.GetComponent<Tile>();
                tempTile.location.row = i;
                tempTile.location.column = j;

                if (data.Equals("1"))
                {
                    tempTile.blockObject = null;
                    continue;
                }
                else
                    tempTile.blockObject = SettingBlockData(i, j, data);

                tileList.Add(tempTile);

                if (i.Equals(maxRow / 2) && j.Equals(0))
                    createBlockTile = tempTile;
            }
        }

        return;
    }

    ///<summary>
    ///csv 데이터를 토대로 블록 객체를 세팅, 생성하는 함수
    ///</summary>
    GameObject SettingBlockData(int row, int column, string data)
    {
        //맵 설정에서 블록이 Empty로 되어있다면 null 
        if (data.Equals("1")) return null;

        GameObject blockObj = Instantiate(blockPrefab, tilePosition[row, column], Quaternion.identity);
        blockObj.transform.SetParent(blocks);

        Block block = blockObj.GetComponent<Block>();

        //맵 설정에서 블록이 2_ == Basic으로 되어있을 경우와 Special일 경우 처리
        if (data.Contains("2_"))
        {
            string[] blockData = data.Split('_');
            block.blockType = Block.BlockType.BASIC;
            block.blockColor = (Block.BlockColor)int.Parse(blockData[1]);            
        }
        else
        {
            block.blockType = (Block.BlockType)(int.Parse(data) - 2);
            block.blockColor = Block.BlockColor.NONE;
        }

        return blockObj;
    }

    void TileConnection()
    {
        for (int i = 0; i < tileList.Count; i++)
        {
            int tileColumn = tileList[i].location.column;

            if ((tileColumn % 2).Equals(0))
            {
                for (int j = 0; j < evenAdjacentLocation.Length; j++)
                {
                    Location adjacentLocation = tileList[i].location + evenAdjacentLocation[j];
                    tileList[i].adjacentTile[j] = TileSearch(adjacentLocation);
                }
            }
            else
            {
                for (int j = 0; j < oddAdjacentLocation.Length; j++)
                {
                    Location adjacentLocation = tileList[i].location + oddAdjacentLocation[j];
                    tileList[i].adjacentTile[j] = TileSearch(adjacentLocation);
                }
            }
        }
    }

    Tile TileSearch(Location location)
    {
        for (int i = 0; i < tileList.Count; i++)
        {
            if (tileList[i].location.Equals(location))
                return tileList[i];
        }

        return null;
    }

    ///<summary>
    ///csv 데이터를 토대로 맵 객체를 생성하는 함수
    ///</summary>
    public void CreateStage()
    {
        tiles = new GameObject("Tiles").transform;
        blocks = new GameObject("Blocks").transform;

        SettingTileData();
        TileConnection();
        UpdateUIInfo();
    }

    ///<summary>
    ///UI 정보를 업데이트하는 함수
    ///</summary>
    public void UpdateUIInfo()
    {
        moveCountText.text = moveCount.ToString();
        targetCountText.text = targetCount.ToString();

        if(GameManager.Instance.stageEnd)
        {
            if(targetCount > 0)
            {
                resultText.text = "Stage Fail...";
            }
            else
            {
                resultText.text = "Stage Clear!";
            }
        }
    }

    ///<summary>
    ///매개변수로 받은 타일의 벡터 포지션을 반환하는 함수
    ///</summary>
    public Vector2 GetTilePosition(Tile tile)
    {
        Vector2 position = tilePosition[tile.location.row, tile.location.column];
        return position;
    }

    ///<summary>
    ///블록 랜덤 생성 및 생성된 블록이 있는 타일 정보를 반환하는 함수
    ///</summary>
    public Tile CreateBlock()
    {
        Vector2 CreatePosition = GetTilePosition(createBlockTile);
        GameObject blockObj = Instantiate(blockPrefab, CreatePosition, Quaternion.identity);
        blockObj.transform.SetParent(blocks);

        Block block = blockObj.GetComponent<Block>();
        block.blockType = Block.BlockType.BASIC;

        int rand = Random.Range(0, 6);
        block.blockColor = (Block.BlockColor)rand;

        createBlockTile.blockObject = blockObj;
        return createBlockTile;
    }
}
