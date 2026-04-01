using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : CharacterBasic
{
    bool buildMode;
    public bool BuildMode
    {
        get { return buildMode; }
        set
        {
            buildMode = value;
        }
    }

    Dictionary<KeyCode, Action> keyDictionary;

    void Start()
    {
        if (GameManager.Instance.Player != null)
        {
            Debug.Log("이미 플레이어 캐릭터가 있음");
            Destroy(gameObject);
            return;
        }

        GameManager.Instance.Player = this;
        DontDestroyOnLoad(gameObject);

        rB = GetComponent<Rigidbody2D>();
        Transform body = transform.GetChild(0);
        sR = body.GetComponent<SpriteRenderer>();
        aT = body.GetComponent<Animator>();
        hitBox = body.GetComponent<CharacterHitBox>();
        handPos = hitBox.transform.GetChild(0);
        wC = handPos.GetChild(0).GetComponent<WeaponController>();
        afterImage = body.GetChild(2).GetComponent<AfterImage>();

        //플레이어블 캐릭터 정보 리스트에서 캐릭터 정보 받아옴
        for (int i = 0; i < CharacterManager.Instance.PlayerbleInfo.Count; i++)
        {
            if (characterID.Equals(CharacterManager.Instance.PlayerbleInfo[i].CharacterID))
            {
                Info = CharacterManager.Instance.PlayerbleInfo[i];
                break;
            }
        }
        stamina = Info.MaxStamina;
        //기본 소지 무기 초기화 (추후엔 세이브 데이터로 교체)
        Items.Equipments[EquipmentType.Weapon] = ItemManager.Instance.GetItemSameID(Info.WeaponID);
        UIManager.Instance.GetUI("WeaponInfo").GetComponent<WeaponInfoUI>().SetWeaponInfo(Items.Equipments[EquipmentType.Weapon]);
        //HP 정보 UI에 세팅
        UIManager.Instance.PlayerHpFillAmount();
        //상호작용 UI 생성
        if (interUI == null)
        {
            GameObject obj = Instantiate(ResourceManager.Instance.InterUI, MapManager.Instance.MapCanvas, false);
            interUI = obj.GetComponent<InteractionUI>();
            interUI.Target = this;
        }

        keyDictionary = new Dictionary<KeyCode, Action>
        {
            { KeyCode.UpArrow, () => { v = 1; h = 0; } },
            { KeyCode.DownArrow, () => { v = -1; h = 0; } },
            { KeyCode.LeftArrow, () => { v = 0; h = -1; } },
            { KeyCode.RightArrow, () =>{ v = 0; h = 1; } }
        };

        UIManager.Instance.GetUI("Inventory").GetComponent<Inventory>().SetInventoryInfo(); //임시로 여기에
    }

    void Update()
    {
        if (GameManager.Instance.IsDirecting || GameManager.Instance.IsLoading || state.Equals(CharacterState.DIE)) return;
                
        switch (state)
        {
            case CharacterState.IDLE:
                Idle();
                break;
        }
        PlayerUI();
        StaminaRecovery();
    }

    void PlayerUI()
    {
        if (Input.GetKeyUp(KeyCode.I)) 
            UIManager.Instance.ToggleWindow("Inventory");

        if (Input.GetKeyUp(KeyCode.Q))
            UIManager.Instance.ToggleWindow("QuestUI");

        if (Input.GetKeyUp(KeyCode.Tab))
            UIManager.Instance.ToggleWindow("Minimap");
    }

    void Idle()
    {
        UseShortcutItem();

        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        AnimationCheck();
        Interaction(this);
        Move();
        Attack();
    }

    public override void Interaction(CharacterBasic me)
    {
        if (interactionObject == null) return;

        if (Input.GetKeyUp(interactionObject.InteractionKey.ToLower()))
        {
            interactionObject.Interaction(me);
            h = v = 0;
            rB.velocity = Vector2.zero;
            aT.SetBool("IsRun", false);
        }
    }

    void Move()
    {
        Vector2 dir = new Vector2(h, v).normalized;
        bool isDash = Input.GetKey(KeyCode.LeftShift) && !dir.Equals(Vector2.zero) && stamina > 0;
        if (isDash)
        {
            stamina -= 0.5f;
            if (stamina < 0) stamina = 0;
            UIManager.Instance.PlayerStaminaFillAmount();
            staminaRecoveryCheck = false;
            CancelInvoke("SetRecoveryCheck");
            Invoke("SetRecoveryCheck", 1f);
            rB.velocity = dir * Info.Speed * 2;
        }
        else
            rB.velocity = dir * Info.Speed;
        if(!wC.IsAttack)
            LookDirection();
        afterImage.gameObject.SetActive(isDash);
    }

    void Attack()
    {
        if (Input.anyKeyDown)
        {
            foreach (var dic in keyDictionary)
            {
                if (Input.GetKeyDown(dic.Key))
                {
                    if (stamina < wC.StaminaCost || IsActing || wC.IsAttack || Items.Equipments[EquipmentType.Weapon] == null) return;

                    dic.Value();
                    LookDirection();
                    SetHandState();

                    stamina -= wC.StaminaCost;
                    UIManager.Instance.PlayerStaminaFillAmount();
                    staminaRecoveryCheck = false;
                    CancelInvoke("SetRecoveryCheck");
                    Invoke("SetRecoveryCheck", 0.5f);
                    wC.Attack();
                }
            }
        }
    }

    void StaminaRecovery()
    {
        if (stamina < Info.MaxStamina && staminaRecoveryCheck)
        {
            stamina += 50f * Time.deltaTime;
            if (stamina > Info.MaxStamina) stamina = Info.MaxStamina;
            UIManager.Instance.PlayerStaminaFillAmount();
        }
    }

    void SetRecoveryCheck() => staminaRecoveryCheck = true;

    public void InPortal(Vector3 pos)
    {
        isActing = true;
        hitBox.InvincibilityCheck = true;
        state = CharacterState.WORK;
        transform.position = pos;
        wC.gameObject.SetActive(false);
        aT.SetTrigger("UsePortal");
    }

    public void OutPortal()
    {
        isActing = false;
        hitBox.InvincibilityCheck = false;
        state = CharacterState.IDLE;
        wC.gameObject.SetActive(true);
        aT.SetTrigger("OutPortal");
    }

    void UseShortcutItem()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyUp((i + 1).ToString()))
            {
                UIManager.Instance.GetUI("Inventory").GetComponent<Inventory>().ShortcutBoxes[i].ItemObject.Item.UseItem();
            }
        }
    }

    public override void HitHandling(float damage, int knockbackPower, Vector3 attackerPos)
    {
        UIManager.Instance.PlayerHpFillAmount();

        if (!state.Equals(CharacterState.DIE))
        {
            Vector3 hitbackDir = (transform.position - attackerPos).normalized;
            rB.velocity = Vector2.zero;
            rB.AddForce(hitbackDir * knockbackPower);            
            Camera.main.GetComponent<CameraController>().StartVibrate(0.5f, 0.05f);
        }
        else
            Die();
    }

    void Die()
    {
        StopAllCoroutines();
        //aT.SetTrigger("IsDie");
        //aT.speed = 1;
        rB.velocity = Vector2.zero;
        Camera.main.GetComponent<CameraController>().enabled = false;
        UIManager.Instance.GetUI("LogInfoUI").GetComponent<LogInfo>().DisplayLogInfo("사망하였습니다...");
        GameManager.Instance.RestartGame();
        gameObject.SetActive(false);
    }

    public override void ObjectDetection(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Monster"))
        {
            //Debug.Log("몬스터 감지");
        }
    }

    public override void ObjectDetecting(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Monster"))
        {
            //Debug.Log("몬스터 감지 중");
        }
    }

    public override void OutOfDetection(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Monster"))
        {
            //Debug.Log("몬스터 시야 벗어남");
        }
    }
}
