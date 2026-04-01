using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;

public class NPCKnight : NPCBasic
{
    Transform saveTr;
    AIDestinationSetter desnination;
    AIPath aiPath;

    int calculatePercent = 40;
    float huntingTime = 20f;
    [SerializeField]
    bool isHunting;
    [SerializeField]
    bool calculateMode;

    private void Start()
    {
        rB = GetComponent<Rigidbody2D>();
        sR = transform.GetChild(1).GetComponent<SpriteRenderer>();
        aT = transform.GetChild(1).GetComponent<Animator>();
        hitBox = transform.GetChild(1).GetComponent<CharacterHitBox>();

        handPos = hitBox.transform.GetChild(0);
        wC = handPos.GetChild(0).GetComponent<WeaponController>();

        desnination = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();

        for (int i = 0; i < CharacterManager.Instance.NPCInfo.Count; i++)
        {
            if (characterID.Equals(CharacterManager.Instance.NPCInfo[i].CharacterID))
            {
                Info = CharacterManager.Instance.NPCInfo[i];
                break;
            }
        }

        actDescription = "고용";
        aiPath.maxSpeed = Info.Speed;

        Items.Equipments[EquipmentType.Weapon] = ItemManager.Instance.GetItemSameID(Info.WeaponID);

        saveTr = new GameObject("KnightBasePos").transform;
        saveTr.position = transform.position;

        if (interUI == null)
        {
            GameObject obj = Instantiate(ResourceManager.Instance.InterUI, MapManager.Instance.MapCanvas, false);
            interUI = obj.GetComponent<InteractionUI>();
            interUI.Target = this;
        }
    }

    void Update()
    {
        desnination.target = target;
        h = v = 0;
        if (target != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            if (dir.x > 0) h = 1;
            else if (dir.x < 0) h = -1;
            if (dir.y > 0) v = 1;
            else if (dir.y < 0) v = -1;
            LookDirection();
            SetHandState();
        }

        switch (state)
        {
            case CharacterState.IDLE:
                Idle();
                break;
            case CharacterState.SEARCHING:
                Searching();
                break;
            case CharacterState.TRACKING:
                Tracking();
                break;
            case CharacterState.ATTACK:
                Attack();
                break;
            case CharacterState.DIE:
                Die();
                break;
            default:
                break;
        }

        AnimationCheck();
    }

    void Idle()
    {
        sight = 1.5f;
        h = v = 0;
        rB.bodyType = RigidbodyType2D.Kinematic;
        aiPath.canMove = false;
    }

    void Searching()
    {
        sight = 8f;
        aiPath.canMove = true;
        rB.bodyType = RigidbodyType2D.Dynamic;

        if (isHunting)
        {
            if (target == null)
                StartCoroutine(SearchingProcess());
        }
        else
        {
            target = saveTr;
            float dis = (target.position - transform.position).magnitude;
            if (dis <= 0.1f)
                state = CharacterState.IDLE;
        }
    }

    IEnumerator SearchingProcess()
    {
        int rand = Random.Range(0, MapManager.Instance.SearchingPosList.Count);
        target = MapManager.Instance.SearchingPosList[rand];
        while (true)
        {
            if (state != CharacterState.SEARCHING || target == null) break;

            float dis = (target.position - transform.position).magnitude;
            if (dis <= 0.1f)
            {
                target = null;
                break;
            }
            yield return null;
        }
    }

    void Tracking()
    {
        sight = 6f;
        aiPath.canMove = true;
        rB.bodyType = RigidbodyType2D.Dynamic;

        if (target == null)
        {
            state = CharacterState.SEARCHING;
            return;
        }

        float dis = (target.position - transform.position).magnitude;
        if (target.CompareTag("DropItem"))
        {
            if (dis <= 1.5f)
            {
                target.GetComponentInParent<DropItem>().Interaction(this);
                target = null;
                state = CharacterState.SEARCHING;
            }
        }
        else
        {
            if (dis <= Info.AttackRange)
            {
                state = CharacterState.ATTACK;
            }
        }
    }

    void Attack()
    {
        if (IsActing) return;
        wC.Attack();
        state = CharacterState.TRACKING;
    }

    void Die()
    {
        StopAllCoroutines();
        if (enemyHpUI != null)
            Destroy(enemyHpUI.gameObject);
        CharacterManager.Instance.NPCList.Remove(this);
        Destroy(gameObject);
    }

    public override void Interaction(CharacterBasic character)
    {
        character.HitBox.InteractOff(this);
        interUI.gameObject.SetActive(false);
        if (state.Equals(CharacterState.IDLE))
        {
            if (!calculateMode)
                StartCoroutine(HuntingModeProcess());
            else
                CalculateProcess();
        }
    }

    void CalculateProcess()
    {
        var loginfo = UIManager.Instance.GetUI("LogInfoUI").GetComponent<LogInfo>();
        List<Item> calculateItemList = new List<Item>();
        for (int i = 0; i < _characterItem.Belongings.Count; i++)
        {
            if (_characterItem.Belongings[i] == null) break;
            calculateItemList.Add(_characterItem.Belongings[i]);
            _characterItem.Belongings[i] = null;
        }

        int numberToShare = (calculateItemList.Count * calculatePercent) / 100;
        if(numberToShare < 1)
            loginfo.DisplayLogInfo("정산 받을 아이템이 없습니다...");
        else
        {
            List<Item> giveItemList = new List<Item>();            
            List<int> randIndexList = new List<int>();
            for (int i = 0; i < calculateItemList.Count; i++) { randIndexList.Add(i); }

            for (int i = 0; i < numberToShare; i++)
            {
                int index = Random.Range(0, randIndexList.Count);

                giveItemList.Add(calculateItemList[index]);
                randIndexList.RemoveAt(index);
            }

            for (int i = 0; i < giveItemList.Count; i++)
            {
                ItemManager.Instance.ItemAcquisition(GameManager.Instance.Player, giveItemList[i]);
            }

            loginfo.DisplayLogInfo("고용에 대한 정산을 완료하였습니다!");
        }
        actDescription = "고용";
        calculateMode = false;
        itemGiveAction = null;
    }

    IEnumerator HuntingModeProcess()
    {
        isHunting = true;
        target = null;
        state = CharacterState.SEARCHING;

        yield return new WaitForSeconds(huntingTime);

        isHunting = false;
        target = saveTr;
        while (!aiPath.reachedEndOfPath)
        {
            target = saveTr;
            yield return null;
        }

        target = null;
        calculateMode = true;
        actDescription = "정산";
        state = CharacterState.IDLE;
    }

    public override void ObjectDetection(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (state.Equals(CharacterState.IDLE))
            {
                if (cH.CB.transform.CompareTag("Player"))
                {
                    cH.InteractOn(this);
                    target = detectionObj.transform;
                    interUI.gameObject.SetActive(true);
                }
            }
            else
            {
                if (cH.CB.transform.CompareTag("Monster") && isHunting)
                {
                    if (target == null || target.gameObject.CompareTag("SearchingPos"))
                    {
                        target = detectionObj.transform;
                        state = CharacterState.TRACKING;
                    }
                }
            }
        }
        else if (detectionObj.CompareTag("DropItem") && isHunting)
        {
            if (_characterItem.Belongings[_characterItem.Belongings.Count - 1] != null) return;

            DropItem dropItem = detectionObj.GetComponentInParent<DropItem>();
            if (!dropItem.InterPossible) return;
            else if (dropItem.Item.Info.ItemType.Equals(ItemType.GOODS)) return;

            if (target == null || target.gameObject.CompareTag("SearchingPos"))
            {
                target = detectionObj.transform;
                state = CharacterState.TRACKING;
            }
        }
    }

    public override void ObjectDetecting(GameObject detectionObj)
    {
        if (state.Equals(CharacterState.TRACKING) || state.Equals(CharacterState.SEARCHING))
        {
            if (detectionObj.CompareTag("Character"))
            {
                CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
                if (cH.CB.transform.CompareTag("Monster") && isHunting)
                {
                    if (target == null || target.gameObject.CompareTag("SearchingPos"))
                    {
                        target = detectionObj.transform;
                        state = CharacterState.TRACKING;
                    }
                }
            }
            else if (detectionObj.CompareTag("DropItem") && isHunting)
            {
                if (_characterItem.Belongings[_characterItem.Belongings.Count - 1] != null) return;

                DropItem dropItem = detectionObj.GetComponentInParent<DropItem>();
                if (dropItem == null) return;
                else if (!dropItem.InterPossible || dropItem.Item.Info.ItemType.Equals(ItemType.GOODS)) return;

                if (target == null || target.gameObject.CompareTag("SearchingPos"))
                {
                    target = detectionObj.transform;
                    state = CharacterState.TRACKING;
                }
            }
        }
    }

    public override void OutOfDetection(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (state.Equals(CharacterState.IDLE))
            {
                if (cH.CB.transform.CompareTag("Player"))
                {
                    cH.InteractOff(this);
                    target = null;
                    interUI.gameObject.SetActive(false);
                }
            }
            else
            {
                cH.InteractOff(this);
                interUI.gameObject.SetActive(false);
            }
        }
    }
}
