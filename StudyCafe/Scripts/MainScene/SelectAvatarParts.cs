using System.Collections.Generic;
using UnityEngine;

public class SelectAvatarParts : MonoBehaviour
{
    const int Skin = 0;
    const int Eyes = 1;
    const int Pants = 2;
    const int Top = 3;
    const int Hair = 4;
    const int Accessory = 5;
    public AvatarImage avatarImage;
    public GameObject[] itemListPanel;
    public Transform[] contents;
    public int[] partsType;

    /// <summary>
    /// 리스트 뷰에 아바타 파츠 item을 생성, 추가하는 함수<br />
    /// </summary>
    public void AddPartsList()
    {
        List<Sprite[]>[] spritesList = new List<Sprite[]>[partsType.Length];
        for(int i = 0; i < spritesList.Length; i++)
        {
            spritesList[i] = new List<Sprite[]>();
        }

        for (int i = 0; i < partsType.Length; i++)
        {
            Sprite[] sprites = null;
            int j = 1;

            while(true)
            {                
                switch (partsType[i])
                {
                    case Hair:
                        sprites = Resources.LoadAll<Sprite>("AvatarSprite/Hair/hair_" + j);
                        if (!sprites.Length.Equals(0))
                            spritesList[i].Add(sprites);
                        break;
                    case Eyes:
                        sprites = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + j);
                        if (!sprites.Length.Equals(0))
                            spritesList[i].Add(sprites);
                        break;
                    case Top:
                        sprites = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + j);
                        if (!sprites.Length.Equals(0))
                            spritesList[i].Add(sprites);
                        break;
                    case Pants:
                        sprites = Resources.LoadAll<Sprite>("AvatarSprite/Pants/pants_" + j);
                        if (!sprites.Length.Equals(0))
                            spritesList[i].Add(sprites);
                        break;
                    case Skin:
                        sprites = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + j);
                        if (!sprites.Length.Equals(0))
                            spritesList[i].Add(sprites);
                        break;
                    case Accessory:
                        sprites = Resources.LoadAll<Sprite>("AvatarSprite/Accessory/acc_" + j);
                        if (!sprites.Length.Equals(0))
                            spritesList[i].Add(sprites);
                        break;
                }

                if (sprites == null) break;
                else if (sprites.Length.Equals(0)) break;
                else
                    j++;
            }
        }

        for (int i = 0; i < contents.Length; i++)
        {
            for (int j = 0; j < spritesList[i].Count; j++)
            {                
                GameObject item = Instantiate(Resources.Load<GameObject>("Prefabs/UI/PartsItem"), contents[i], false);
                PartsItem partsItem = item.GetComponent<PartsItem>();
                Sprite[] sprite = spritesList[i][j];
                partsItem.SetPartsData(partsType[i], j + 1, sprite[0]);
                partsItem.avatarImage = avatarImage;
            }
        }
    }

    /// <summary>
    /// 아바타 파츠의 Tap UI를 선택했을 때 실행되는 함수<br />
    /// 선택한 파츠 리스트를 표시함
    /// </summary>
    public void SelectPartsTap(int type)
    {
        for(int i = 0; i < itemListPanel.Length; i++)
        {
            if(partsType[i].Equals(type))
                itemListPanel[i].SetActive(true);
            else
                itemListPanel[i].SetActive(false);
        }
    }

    public void DestroyAllParts()
    {
        for (int i = 0; i < contents.Length; i++)
        {
            for (int j = 0; j < contents[i].childCount; j++)
            {
                Destroy(contents[i].GetChild(j).gameObject);
            }
        }
    }
}
