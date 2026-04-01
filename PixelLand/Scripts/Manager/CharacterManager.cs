using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour
{
    private static CharacterManager instance = null;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            TextAsset json = Resources.Load<TextAsset>("Json/CharacterTable");
            characterTable = JsonUtility.FromJson<CharacterTable>(json.ToString());
            for (int i = 0; i < characterTable.tableList.Count; i++)
            {
                switch (characterTable.tableList[i].Type)
                {
                    case CharacterType.PLAYERBLE:
                        playerbleInfo.Add(characterTable.tableList[i]);
                        break;
                    case CharacterType.NPC:
                        npcInfo.Add(characterTable.tableList[i]);
                        break;
                    case CharacterType.MONSTER:
                        monsterInfo.Add(characterTable.tableList[i]);
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static CharacterManager Instance
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

    //캐릭터 정보 테이블
    CharacterTable characterTable;
    public CharacterTable CharacterTable { get { return characterTable; } }

    List<CharacterInfo> playerbleInfo = new List<CharacterInfo>();
    public List<CharacterInfo> PlayerbleInfo { get { return playerbleInfo; } }

    List<CharacterInfo> monsterInfo = new List<CharacterInfo>();
    public List<CharacterInfo> MonsterInfo { get { return monsterInfo; } }

    List<CharacterInfo> npcInfo = new List<CharacterInfo>();
    public List<CharacterInfo> NPCInfo { get { return npcInfo; } }

    //맵 객체 리스트
    [SerializeField]
    List<CharacterBasic> npcList = new List<CharacterBasic>();
    public List<CharacterBasic> NPCList { get { return npcList; } }

    [SerializeField]
    List<CharacterBasic> monsterList = new List<CharacterBasic>();
    public List<CharacterBasic> MonsterList { get { return monsterList; } }

    [SerializeField]
    List<CharacterBasic> playerbleList = new List<CharacterBasic>();
    public List<CharacterBasic> PlayerbleList { get { return playerbleList; } } 

    public void ResetMapObject()
    {
        npcList.Clear();
        monsterList.Clear();
        playerbleList.Clear();

        for (int i = 0; i < MapManager.Instance.NPCTr.childCount; i++)
        {
            CharacterBasic cB = MapManager.Instance.NPCTr.GetChild(i).GetComponent<CharacterBasic>();
            if(cB.ObjectID == null)
                cB.ObjectID = SceneManager.GetActiveScene().name + "NPC" + i;
            else if(cB.ObjectID.Equals(""))
                cB.ObjectID = SceneManager.GetActiveScene().name + "NPC" + i;            
            npcList.Add(cB);
        }

        for (int i = 0; i < MapManager.Instance.MonsterTr.childCount; i++)
        {
            CharacterBasic cB = MapManager.Instance.MonsterTr.GetChild(i).GetComponent<CharacterBasic>();
            if (cB.ObjectID == null)
                cB.ObjectID = SceneManager.GetActiveScene().name + "Monster" + i;
            else if (cB.ObjectID.Equals(""))
                cB.ObjectID = SceneManager.GetActiveScene().name + "Monster" + i;
            monsterList.Add(cB);
        }

        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < player.Length; i++)
        {
            CharacterBasic cB = player[i].GetComponent<CharacterBasic>();
            cB.ObjectID = SceneManager.GetActiveScene().name + "Playerble" + i;
            playerbleList.Add(cB);
        }
    }
}
