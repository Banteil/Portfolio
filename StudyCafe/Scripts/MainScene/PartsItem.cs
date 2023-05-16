using UnityEngine;
using UnityEngine.UI;

public class PartsItem : MonoBehaviour
{
    int type;
    int num;
    public AvatarImage avatarImage;

    public void SetPartsData(int type, int num, Sprite sprite)
    {
        transform.GetChild(0).GetComponent<Image>().sprite = sprite;
        this.type = type;
        this.num = num;
    }

    public void SelectParts() => avatarImage.SetInfo(type, num);  
}
