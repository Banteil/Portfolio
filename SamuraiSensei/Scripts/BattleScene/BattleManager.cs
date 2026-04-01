using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private static BattleManager instance = null;
    public static BattleManager Instance
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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StageSetting();
        PlayerbleSetting();
        EnemySetting();
        //적 스폰 시작
        StartCoroutine(SpawnRoutine());
    }

    void StageSetting()
    {
        //선택된 스테이지가 없을때 == 배틀 씬에서 테스트로 실행할 때 1-1 스테이지에서 시작되도록 함
        if (GameManager.Instance.SelectStage == null)
            GameManager.Instance.SelectStage = TableDatabase.Instance.StageTable.infoList[0];

        cost = maxCost;
        //현재 스테이지의 스폰 이벤트 정보 세팅
        for (int i = 0; i < TableDatabase.Instance.SpawnTable.eventList.Count; i++)
        {
            int worldID = TableDatabase.Instance.SpawnTable.eventList[i].worldID;
            int stageID = TableDatabase.Instance.SpawnTable.eventList[i].stageID;
            if (worldID > GameManager.Instance.SelectStage.world) break;
            else if (stageID > GameManager.Instance.SelectStage.stage) break;

            if (worldID.Equals(TableDatabase.Instance.SpawnTable.eventList[i].worldID) && stageID.Equals(GameManager.Instance.SelectStage.stage))
                spawnEvents.Enqueue(TableDatabase.Instance.SpawnTable.eventList[i]);
        }
        mapLeft.position = new Vector2(-BattleUIManager.Instance.MainCanvas.rect.width * 0.01f, 0f);
        mapRight.position = new Vector2(BattleUIManager.Instance.MainCanvas.rect.width * 0.01f, 0f);
    }

    void PlayerbleSetting()
    {
        //플레이어블 캐릭터 세팅
        for (int i = 0; i < characterPos.Length; i++)
        {
            GameObject characterObj = Instantiate(BattleResourceManager.Instance.PlayerbleObjectList[GameManager.Instance.SelectCharacter[i]], characterPos[i], false);
            playerbles[i] = characterObj.GetComponent<Playerble>();
            playerbles[i].CurrentLine = i;
            BattleUIManager.Instance.CharacterLogo[i].sprite = ResourceManager.Instance.CharacterLogoList[GameManager.Instance.SelectCharacter[i]];
            BattleUIManager.Instance.CharacterHpUI[i].HPText.text = playerbles[i].Info.info.maxHP.ToString();
        }

        //캐릭터가 소지한 스킬 리스트를 스킬 덱에 세팅 (현재는 테이블 전체 끌어옴)
        for (int i = 0; i < TableDatabase.Instance.AllSkillTable.skillList.Count; i++)
        {
            skillDeck.Add(TableDatabase.Instance.AllSkillTable.skillList[i]);
            skillDeck.Add(TableDatabase.Instance.AllSkillTable.skillList[i]);
        }
        skillDeck = ShuffleSkillDeck(skillDeck); //셔플

        //스킬 오브젝트 세팅
        for (int i = 0; i < skillDeck.Count; i++)
        {
            if (i.Equals(BattleUIManager.Instance.SkillItems.Length)) break;
            BattleUIManager.Instance.SkillItems[i].gameObject.SetActive(true);
            BattleUIManager.Instance.SkillItems[i].SkillInfo = skillDeck[i];
        }
    }

    void EnemySetting()
    {
        //적 라인 별 리스트 초기화
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i] = new List<Enemy>();
        }
    }

    public const float maxCost = 100f;
    [SerializeField]
    float cost;
    public float Cost
    {
        get { return cost; }
        set
        {
            if (value < cost)
            {
                costRecovery = false;
                if (costRecoveryRoutine != null)
                    StopCoroutine(costRecoveryRoutine);
                costRecoveryRoutine = StartCoroutine(CostRecoveryPossible());
            }
            cost = value;
            BattleUIManager.Instance.SkillCostGauge.fillAmount = cost / maxCost;

        }
    }
    bool costRecovery = false;
    Coroutine costRecoveryRoutine;

    [SerializeField]
    Transform mapLeft, mapRight;

    [SerializeField]
    Transform objectsTr;
    public Transform ObjectTr { get { return objectsTr; } }

    //플레이어블 캐릭터
    [SerializeField]
    Transform[] characterPos;
    public Transform[] CharacterPos { get { return characterPos; } }

    Playerble[] playerbles = new Playerble[3];
    public Playerble[] Playerbles { get { return playerbles; } }

    //적 캐릭터
    [SerializeField]
    Transform[] enemySpawnPos;
    public Transform[] EnemySpawnPos { get { return enemySpawnPos; } }
    //라인 별 적 관리
    List<Enemy>[] enemies = new List<Enemy>[3];
    public List<Enemy>[] Enemies { get { return enemies; } }

    //스폰 이벤트
    Queue<SpawnEvent> spawnEvents = new Queue<SpawnEvent>();
    SpawnEvent currentEvent;

    //사용 스킬 리스트(덱, 묘지)
    List<Skill> skillDeck = new List<Skill>();
    public List<Skill> SkillDeck { get { return skillDeck; } }

    List<Skill> usedSkills = new List<Skill>();
    public List<Skill> UsedSkills { get { return usedSkills; } }

    private void Update()
    {
        if (costRecovery)
        {
            Cost += 10f * Time.deltaTime;
            if (cost > maxCost) cost = maxCost;
        }

        if (Application.platform.Equals(RuntimePlatform.Android))
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                BattleUIManager.Instance.Pause.gameObject.SetActive(true);
            }
        }
        DebugInput();
    }

    /// <summary>
    /// 리스트의 스킬을 전부 셔플
    /// </summary>
    List<Skill> ShuffleSkillDeck(List<Skill> list)
    {
        int random1, random2;
        Skill temp;

        for (int i = 0; i < list.Count; ++i)
        {
            random1 = Random.Range(0, list.Count);
            random2 = Random.Range(0, list.Count);

            temp = list[random1];
            list[random1] = list[random2];
            list[random2] = temp;
        }

        return list;
    }

    /// <summary>
    /// 특정 캐릭터의 스킬만 셔플
    /// </summary>
    List<Skill> ShuffleSkillDeck(List<Skill> list, int characterID)
    {
        Skill temp;

        for (int i = 0; i < list.Count; ++i)
        {
            if (list[i].characterID.Equals(characterID))
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[j].characterID.Equals(characterID))
                    {
                        int check = Random.Range(0, 2);
                        if (check.Equals(1))
                        {
                            temp = list[i];
                            list[i] = list[j];
                            list[j] = temp;
                        }
                    }
                }
            }
        }

        return list;
    }

    public void DeleteSkillDeck(int characterID)
    {
        for (int i = 0; i < skillDeck.Count; i++)
        {
            if (skillDeck[i].characterID.Equals(characterID))
            {
                usedSkills.Add(skillDeck[i]);
                skillDeck.RemoveAt(i);
                i--;
            }
        }

        RearrangementSkillItem();
    }

    void RearrangementSkillItem()
    {
        for (int i = 0; i < BattleUIManager.Instance.SkillItems.Length; i++)
        {
            if (i >= skillDeck.Count)
                BattleUIManager.Instance.SkillItems[i].SkillInfo = null;
            else
                BattleUIManager.Instance.SkillItems[i].SkillInfo = skillDeck[i];
        }
    }

    /// <summary>
    /// 코스트 회복 타이밍을 체크하는 루틴
    /// </summary>
    IEnumerator CostRecoveryPossible()
    {
        yield return new WaitForSeconds(1f);
        costRecovery = true;
        costRecoveryRoutine = null;
    }

    /// <summary>
    /// spawnEvents 큐의 정보를 기반으로 적을 스폰시키는 루틴
    /// </summary>
    IEnumerator SpawnRoutine()
    {
        float currentTime = 0;
        while (true)
        {
            if (spawnEvents.Count.Equals(0)) break;

            currentEvent = spawnEvents.Dequeue();
            while (true)
            {
                currentTime += Time.deltaTime;
                if (currentTime >= currentEvent.spawnTime)
                {
                    GameObject enemyObj = Instantiate(BattleResourceManager.Instance.EnemyObjectList[currentEvent.enemyID], enemySpawnPos[currentEvent.line], false);
                    enemyObj.transform.position = enemySpawnPos[currentEvent.line].position + new Vector3(Random.Range(-1f, 1f), 0);
                    Enemy enemy = enemyObj.GetComponent<Enemy>();
                    enemy.Info = TableDatabase.Instance.EnemyTable.infoList[currentEvent.enemyID];
                    enemy.CurrentLine = currentEvent.line;
                    enemies[currentEvent.line].Add(enemy);
                    enemy.DisplayHpBar();
                    break;
                }
                yield return null;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 속성에 따른 데미지를 계산, 결과 수치를 반환하는 함수
    /// </summary>
    public float DamageElementCalculation(ElementalProperties objectElement, ElementalProperties attackElement, float damage)
    {
        float saveDamage = damage;
        int percent = Random.Range(0, 36);
        switch (objectElement)
        {
            case ElementalProperties.FIRE:
                switch (attackElement)
                {
                    case ElementalProperties.WATER:
                        saveDamage = saveDamage * (1 + percent / 100);
                        break;
                    case ElementalProperties.METAL:
                        saveDamage = saveDamage * (1 - percent / 100);
                        break;
                }
                break;
            case ElementalProperties.WATER:
                switch (attackElement)
                {
                    case ElementalProperties.EARTH:
                        saveDamage = saveDamage * (1 + percent / 100);
                        break;
                    case ElementalProperties.FIRE:
                        saveDamage = saveDamage * (1 - percent / 100);
                        break;
                }
                break;
            case ElementalProperties.TREE:
                switch (attackElement)
                {
                    case ElementalProperties.METAL:
                        saveDamage = saveDamage * (1 + percent / 100);
                        break;
                    case ElementalProperties.EARTH:
                        saveDamage = saveDamage * (1 - percent / 100);
                        break;
                }
                break;
            case ElementalProperties.METAL:
                switch (attackElement)
                {
                    case ElementalProperties.FIRE:
                        saveDamage = saveDamage * (1 + percent / 100);
                        break;
                    case ElementalProperties.TREE:
                        saveDamage = saveDamage * (1 - percent / 100);
                        break;
                }
                break;
            case ElementalProperties.EARTH:
                switch (attackElement)
                {
                    case ElementalProperties.TREE:
                        saveDamage = saveDamage * (1 + percent / 100);
                        break;
                    case ElementalProperties.WATER:
                        saveDamage = saveDamage * (1 - percent / 100);
                        break;
                }
                break;
            default:
                return saveDamage;
        }
        return saveDamage;
    }

    /// <summary>
    /// 문자로 된 스킬 계산식의 결과를 float 타입 수치로 반환하는 함수
    /// </summary>
    public float GetSkillValue(string data, Skill info)
    {
        float result = 0;
        string[] splitData = data.Split(';');
        string operatorSymbol = "+";
        for (int i = 0; i < splitData.Length; i++)
        {
            if ((i % 2).Equals(0)) //결과 수치
            {
                float value = GetStrValue(splitData[i], info);
                result = GetCalculateResult(operatorSymbol, result, value);
            }
            else //연산자
            {
                operatorSymbol = splitData[i];
            }
        }
        return result;
    }

    /// <summary>
    /// 스킬 계산식 내의 문자를 수치로 변환하여 반환하는 함수
    /// </summary>
    float GetStrValue(string data, Skill info)
    {
        float value = 0;
        switch (data)
        {
            case "level":
                for (int i = 0; i < skillDeck.Count; i++)
                {
                    if (skillDeck[i].id.Equals(info.id))
                    {
                        value = skillDeck[i].level;
                        break;
                    }
                }
                break;
            case "damage":
                for (int i = 0; i < playerbles.Length; i++)
                {
                    if (playerbles[i].Info.info.id.Equals(info.characterID))
                    {
                        value = playerbles[i].Damage;
                        break;
                    }
                }
                break;
            default:
                value = float.Parse(data);
                break;
        }

        return value;
    }

    /// <summary>
    /// 매개변수로 전달된 연산기호 문자에 따라 결과를 반환하는 함수
    /// </summary>
    float GetCalculateResult(string operatorSymbol, float result, float value)
    {
        switch (operatorSymbol)
        {
            case "+":
                return result + value;
            case "-":
                return result - value;
            case "*":
                return result * value;
            case "/":
                return result / value;
            default:
                return result;
        }
    }

    public void Buff(Playerble playerble, Skill info)
    {
        string[] splitData = info.formula.Split(';');
        string stat = splitData[0];
        string operatorSymbol = splitData[1];
        float value = float.Parse(splitData[2]);
        float resetTime = GetSkillValue(info.rigidTime, info);
        switch (stat)
        {
            case "attackSpeed":
                switch (operatorSymbol)
                {
                    case "+":
                        playerble.PlusBuffAttackSpeed = value;
                        break;
                    case "*":
                        playerble.MultiplyBuffAttackSpeed = value;
                        break;
                    default:
                        return;
                }
                break;
            case "damage":
                switch (operatorSymbol)
                {
                    case "+":
                        playerble.PlusBuffDamage = value;
                        break;
                    case "*":
                        playerble.MultiplyBuffDamage = value;
                        break;
                    default:
                        return;
                }
                break;
        }

        StartCoroutine(ResetBuff(playerble, stat, resetTime));
    }

    IEnumerator ResetBuff(Playerble playerble, string stat, float time)
    {
        yield return new WaitForSeconds(time);
        switch (stat)
        {
            case "attackSpeed":
                playerble.PlusBuffAttackSpeed = 0;
                playerble.MultiplyBuffAttackSpeed = 1f;
                break;
            case "damage":
                playerble.PlusBuffDamage = 0;
                playerble.MultiplyBuffDamage = 1f;
                break;
        }
    }

    public void Recovery(Playerble playerble, Skill info)
    {
        float value = GetSkillValue(info.formula, info);
        playerble.HP += value;
        if (playerble.HP > playerble.Info.info.maxHP)
            playerble.HP = playerble.Info.info.maxHP;
        BattleUIManager.Instance.PlayerHpInfoUpdate(playerble);
    }

    public void Functional(Skill info)
    {
        string[] splitData = info.formula.Split(';');
        string effect = splitData[0];
        string target = splitData[1];
        switch (effect)
        {
            case "refresh": //스킬 패, 덱 섞기
                switch (target)
                {
                    case "all":
                        //모두에게 해당
                        skillDeck = ShuffleSkillDeck(skillDeck);
                        break;
                    case "user":
                        //스킬 소유 캐릭터에게만 해당
                        skillDeck = ShuffleSkillDeck(skillDeck, info.characterID);
                        break;
                    default:
                        return;
                }
                RearrangementSkillItem();
                break;
            default:
                return;
        }
    }

    void AllCharacterStop()
    {
        for (int i = 0; i < playerbles.Length; i++)
        {
            playerbles[i].State = CharacterState.STOP;
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            for (int j = 0; j < enemies[i].Count; j++)
            {
                enemies[i][j].State = CharacterState.STOP;
            }
        }
    }

    /// <summary>
    /// 스테이지 클리어 성공 체크 함수 (적을 모두 잡았을 시)
    /// </summary>
    public void StageClearCheck()
    {
        if (enemies[0].Count.Equals(0) && enemies[1].Count.Equals(0) && enemies[2].Count.Equals(0))
        {
            StopAllCoroutines();
            costRecovery = false;
            AllCharacterStop();

            SoundManager.Instance.BGMPlayer.clip = SoundResourceManager.Instance.GameClear;
            SoundManager.Instance.BGMPlayer.Play();

            BattleUIManager.Instance.Result.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 스테이지 클리어 실패 체크 함수 (캐릭터가 모두 사망 시)
    /// </summary>
    public void StageFailCheck()
    {
        if (playerbles[0].State.Equals(CharacterState.DIE) && playerbles[1].State.Equals(CharacterState.DIE) && playerbles[2].State.Equals(CharacterState.DIE))
        {
            StopAllCoroutines();
            costRecovery = false;
            AllCharacterStop();

            SoundManager.Instance.BGMPlayer.clip = SoundResourceManager.Instance.GameOver;
            SoundManager.Instance.BGMPlayer.Play();

            BattleUIManager.Instance.Result.isDefeat = true;
            BattleUIManager.Instance.Result.gameObject.SetActive(true);
        }
    }

    void DebugInput()
    {
        if(Input.GetKeyUp(KeyCode.P))
        {
            StopAllCoroutines();
            costRecovery = false;
            AllCharacterStop();

            SoundManager.Instance.BGMPlayer.clip = SoundResourceManager.Instance.GameOver;
            SoundManager.Instance.BGMPlayer.Play();

            BattleUIManager.Instance.Result.isDefeat = true;
            BattleUIManager.Instance.Result.gameObject.SetActive(true);
        }
    }
}
