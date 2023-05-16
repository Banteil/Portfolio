using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType { RED, BLUE, GREEN, YELLOW, BLACK }

public class Block : MonoBehaviour
{
    BlockType type;
    Color[] colors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.black };

    public BlockType Type
    {
        get { return type; }
        set
        {
            type = value;
            GetComponent<SpriteRenderer>().color = colors[(int)value];
        }
    }    
}
