using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private static ResourceManager instance = null;
    public static ResourceManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ResourceSetting();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //스킬 아이콘 스프라이트
    Sprite[] skillIconList;
    public Sprite[] SkillIconList { get { return skillIconList; } }

    //스킬 프레임 스프라이트
    Sprite[][] skillFrameList = new Sprite[6][];
    public Sprite[][] SkillFrameList { get { return skillFrameList; } }

    //캐릭터 로고 스프라이트
    Sprite[] characterLogoList;
    public Sprite[] CharacterLogoList { get { return characterLogoList; } }

    Sprite[] characterBoxBackgroundSprite;
    public Sprite[] CharacterBoxBackgroundSprite { get { return characterBoxBackgroundSprite; } }

    Sprite[] characterBoxStandingSprite;
    public Sprite[] CharacterBoxStandingSprite { get { return characterBoxStandingSprite; } }

    //재화 아이콘 스프라이트
    Sprite goldIcon;
    public Sprite GoldIcon { get { return goldIcon; } }
    Sprite gemIcon;
    public Sprite GemIcon { get { return gemIcon; } }

    void ResourceSetting()
    {
        characterBoxBackgroundSprite = Resources.LoadAll<Sprite>("Sprites/UI/Lobby/StageMenu/BoxBackground");
        characterBoxStandingSprite = Resources.LoadAll<Sprite>("Sprites/UI/Lobby/StageMenu/BoxCharacterStanding");
        goldIcon = Resources.Load<Sprite>("Sprites/UI/Icon/Gold");
        gemIcon = Resources.Load<Sprite>("Sprites/UI/Icon/Gem");
        skillIconList = Resources.LoadAll<Sprite>("Sprites/UI/Icon/Skill");
        for (int i = 0; i < 6; i++)
        {
            string folderName = "";
            switch (i)
            {
                case 0:
                    folderName = "Fire";
                    break;
                case 1:
                    folderName = "Water";
                    break;
                case 2:
                    folderName = "Tree";
                    break;
                case 3:
                    folderName = "Metal";
                    break;
                case 4:
                    folderName = "Earth";
                    break;
                default:
                    folderName = "None";
                    break;
            }
            skillFrameList[i] = Resources.LoadAll<Sprite>("Sprites/UI/Frame/" + folderName);
        }
        characterLogoList = Resources.LoadAll<Sprite>("Sprites/UI/Logo/Character");
    }

    public float GetMaxExp(int level)
    {
        float max = (25 * (level - 1)) + 100;
        return max;
    }
}
