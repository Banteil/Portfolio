using UnityEngine;
using UnityEngine.UI;

public class EmoteItem : MonoBehaviour
{
    public int index;
    public Sprite EmoteSprite
    {
        set
        {
            transform.GetChild(0).GetComponent<Image>().sprite = value;
        }        
    }
    public IntFunction displayEmote;

    public void DisplayEmoteButton() => displayEmote?.Invoke(index);
}
