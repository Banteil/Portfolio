using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AvatarImage : MonoBehaviour
{
    const int Skin = 0;
    const int Eyes = 1;
    const int Pants = 2;
    const int Top = 3;
    const int Hair = 4;    
    const int Accessory = 5;
    [HideInInspector]
    public CreateAvatarManager createAvatarManager;

    [Header("AvatarParts")]
    public Image[] parts;    
    public string[] partsInfo = new string[5];
    Sprite[][] partsSprites = new Sprite[6][];
    static float animTimer = 0f;
    int direction, eyeDirection;
    int spriteIndex = 1;
    bool isMoving = true;

    public void SetInfo()
    {
        partsSprites[Skin] = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_1");
        if (partsSprites[Skin].Length != 0) parts[Skin].sprite = partsSprites[Skin][0];
        partsSprites[Eyes] = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_1");
        if (partsSprites[Eyes].Length != 0) parts[Eyes].sprite = partsSprites[Eyes][0];
        partsSprites[Top] = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_1");
        if(partsSprites[Top].Length != 0) parts[Top].sprite = partsSprites[Top][0];
        partsSprites[Hair] = Resources.LoadAll<Sprite>("AvatarSprite/Hair/hair_1");
        if (partsSprites[Hair].Length != 0) parts[Hair].sprite = partsSprites[Hair][0];
        partsSprites[Pants] = Resources.LoadAll<Sprite>("AvatarSprite/Pants/pants_1");
        if (partsSprites[Pants].Length != 0) parts[Pants].sprite = partsSprites[Pants][0];

        partsInfo[Hair] = "h_1";
        partsInfo[Eyes] = "e_1";
        partsInfo[Top] = "t_1";
        partsInfo[Pants] = "p_1";
        partsInfo[Skin] = "sk_1";
        partsInfo[Accessory] = "a_0";
    }

    public void SetInfo(int parts, int num)
    {
        switch(parts)
        {
            case Hair:
                partsSprites[Hair] = Resources.LoadAll<Sprite>("AvatarSprite/Hair/hair_" + num);
                if(partsSprites[Hair].Length != 0)
                    this.parts[Hair].sprite = partsSprites[Hair][0];
                partsInfo[Hair] = "h_" + num;
                break;
            case Eyes:
                partsSprites[Eyes] = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + num);
                if (partsSprites[Eyes].Length != 0)
                    this.parts[Eyes].sprite = partsSprites[Eyes][0];
                partsInfo[Eyes] = "e_" + num;
                break;
            case Top:
                partsSprites[Top] = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num);
                if (partsSprites[Top].Length != 0)
                    this.parts[Top].sprite = partsSprites[Top][0];
                partsInfo[Top] = "t_" + num;
                break;
            case Pants:
                partsSprites[Pants] = Resources.LoadAll<Sprite>("AvatarSprite/Pants/pants_" + num);
                if (partsSprites[Pants].Length != 0)
                    this.parts[Pants].sprite = partsSprites[Pants][0];
                partsInfo[Pants] = "p_" + num;
                break;
            case Skin:
                partsSprites[Skin] = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num);
                if (partsSprites[Skin].Length != 0)
                    this.parts[Skin].sprite = partsSprites[Skin][0];
                partsInfo[Skin] = "sk_" + num;
                break;
            case Accessory:
                if (!num.Equals(0))
                {
                    partsSprites[Accessory] = Resources.LoadAll<Sprite>("AvatarSprite/Accessory/acc_" + num);
                    if (partsSprites[Accessory].Length != 0)
                        this.parts[Accessory].sprite = partsSprites[Accessory][0];
                    partsInfo[Accessory] = "a_" + num;
                }
                break;
        }
    }

    public void SetInfo(string avatarInfo)
    {
        string[] splitInfo = avatarInfo.Split(',');
        for (int i = 0; i < splitInfo.Length; i++)
        {
            string[] info = splitInfo[i].Split('_');
            int num = int.Parse(info[1]);
            switch (info[0])
            {
                case "h":
                    partsSprites[Hair] = Resources.LoadAll<Sprite>("AvatarSprite/Hair/hair_" + num);
                    if (partsSprites[Hair].Length != 0)
                        this.parts[Hair].sprite = partsSprites[Hair][0];
                    partsInfo[Hair] = "h_" + num;
                    break;
                case "e":
                    partsSprites[Eyes] = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + num);
                    if (partsSprites[Eyes].Length != 0)
                        this.parts[Eyes].sprite = partsSprites[Eyes][0];
                    partsInfo[Eyes] = "e_" + num;
                    break;
                case "t":
                    partsSprites[Top] = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num);
                    if (partsSprites[Top].Length != 0)
                        this.parts[Top].sprite = partsSprites[Top][0];
                    partsInfo[Top] = "t_" + num;
                    break;
                case "p":
                    partsSprites[Pants] = Resources.LoadAll<Sprite>("AvatarSprite/Pants/pants_" + num);
                    if (partsSprites[Pants].Length != 0)
                        this.parts[Pants].sprite = partsSprites[Pants][0];
                    partsInfo[Pants] = "p_" + num;
                    break;
                case "sk":
                    partsSprites[Skin] = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num);
                    if (partsSprites[Skin].Length != 0)
                        this.parts[Skin].sprite = partsSprites[Skin][0];
                    partsInfo[Skin] = "sk_" + num;
                    break;
                case "a":
                    if (!num.Equals(0))
                    {
                        partsSprites[Accessory] = Resources.LoadAll<Sprite>("AvatarSprite/Accessory/acc_" + num);
                        if (partsSprites[Accessory].Length != 0)
                            this.parts[Accessory].sprite = partsSprites[Accessory][0];
                        partsInfo[Accessory] = "a_" + num;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// DB에 저장할 아바타 파츠의 string 정보를 반환하는 함수
    /// </summary>
    public string GetPartsInfo()
    {
        string info = "";
        for (int i = 0; i < partsInfo.Length; i++)
        {
            info += partsInfo[i];
            if (i < partsInfo.Length - 1) info += ",";
        }

        return info;
    }

    #region 아바타 행동(아바타 생성 시에만 사용됨)
    public void AvatarMove()
    {
        StartCoroutine(AvatarAnimationProcess());
        iTween.MoveTo(gameObject, iTween.Hash("islocal", true, "x", 0f, "time", 2f, "oncomplete", "EndAvatarMove", "oncompletetarget", gameObject));
    }

    void EndAvatarMove() => isMoving = false;

    IEnumerator AvatarAnimationProcess()
    {
        while(isMoving)
        {
            Anim();
            yield return null;
        }

        for (int i = 0; i < partsSprites.Length; i++)
        {
            if (partsSprites[i].Length.Equals(0) || i.Equals(Accessory)) continue;
            parts[i].sprite = partsSprites[i][0];
        }

        createAvatarManager.DisplayCreateAvatarMenu();
        InputControl.Instance.cancel = null;
        InputControl.Instance.enterKey = createAvatarManager.ConfirmCreateAvatarButton;
    }

    void Anim()
    {
        animTimer += Time.deltaTime;
        direction = 36;
        eyeDirection = 20;
        //설정된 방향에 맞는 애니메이션 실행

        //눈, 악세사리 제외 행동
        for (int i = 0; i < partsSprites.Length; i++)
        {
            if (partsSprites[i].Length.Equals(0) || i.Equals(Eyes) || i.Equals(Accessory)) continue;
            int index = direction + spriteIndex;
            parts[i].sprite = partsSprites[i][index];
        }

        //눈 행동
        int eyeIndex = eyeDirection + spriteIndex;
        parts[Eyes].sprite = partsSprites[Eyes][eyeIndex];

        if (animTimer >= 0.1f)
        {
            spriteIndex++;
            if (spriteIndex > 8)
                spriteIndex = 1;
            animTimer = 0f;
        }
    }
    #endregion
}
