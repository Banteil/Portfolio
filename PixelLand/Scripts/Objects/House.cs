using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Work
{
    PersonalSpace pS;
    SelectEffect selectEffect;
    SpriteRenderer outerWallImage, roofImage, doorImage;
    BoxCollider2D bodyCollider, siteCollider;
    [SerializeField]
    Camera myHomeCamera;
    bool buildComplete;
    
    void Awake()
    {
        pS = transform.parent.GetComponent<PersonalSpace>();
        sR = GetComponent<SpriteRenderer>();
        sightAct = transform.GetChild(0).GetComponent<SightAct>();
        outerWallImage = transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();
        roofImage = transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        doorImage = transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>();

        bodyCollider = transform.GetChild(1).GetChild(0).GetComponent<BoxCollider2D>();
        siteCollider = GetComponent<BoxCollider2D>();
        selectEffect = transform.GetChild(2).GetChild(0).GetComponent<SelectEffect>();

        maxTime = 10f;
        objectName = "집터";
        actDescription = "집 짓기";
    }

    private void Start()
    {
        if(selfSwitch)
        {
            Vector3 pos = roofImage.transform.position;
            pos.y += outerWallImage.sprite.rect.size.y / 16f;
            roofImage.transform.position = pos;
            DoorPositionAdjustment(outerWallImage.sprite.name);
            WorkComplete();
        }
    }

    public override void Interaction(CharacterBasic character)
    {
        if (character.gameObject.CompareTag("Player"))
        {            
            character.HitBox.InteractOff(this);
            if (!buildComplete)
                StartCoroutine(PickOuterWall(character));
            else
                Debug.Log("집 메뉴");
        }
    }

    IEnumerator PickOuterWall(CharacterBasic character)
    {
        List<Sprite> wallList = new List<Sprite>();
        List<Sprite> commonList = new List<Sprite>();
        List<Sprite> bigList = new List<Sprite>();
        for (int i = 0; i < ResourceManager.Instance.HouseOuterWallSprites.Length; i++)
        {
            string[] splitName = ResourceManager.Instance.HouseOuterWallSprites[i].name.Split('_');
            switch (splitName[1])
            {
                case "Small":
                    wallList.Add(ResourceManager.Instance.HouseOuterWallSprites[i]);
                    break;
                case "Common":
                    commonList.Add(ResourceManager.Instance.HouseOuterWallSprites[i]);
                    break;
                case "Big":
                    bigList.Add(ResourceManager.Instance.HouseOuterWallSprites[i]);
                    break;
                default:
                    break;
            }
        }
        wallList.AddRange(commonList);
        wallList.AddRange(bigList);

        int num = 0;
        character.State = CharacterState.WORK;
        outerWallImage.sprite = wallList[num];
        DoorPositionAdjustment(outerWallImage.sprite.name);
        outerWallImage.gameObject.SetActive(true);
        doorImage.gameObject.SetActive(true);

        selectEffect.SelectObjectRect = outerWallImage.sprite.rect;
        selectEffect.EffectOn();

        yield return new WaitForSeconds(0.1f);
        while (true)
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                character.State = CharacterState.IDLE;
                selectEffect.EffectOff();
                outerWallImage.sprite = null;
                outerWallImage.gameObject.SetActive(false);
                break;
            }

            if (Input.GetKeyUp(KeyCode.D))
            {
                num++;
                if(num >= ResourceManager.Instance.HouseOuterWallSprites.Length - 1) num = 0;

                outerWallImage.sprite = wallList[num];
                DoorPositionAdjustment(outerWallImage.sprite.name);
                selectEffect.EffectOff();
                selectEffect.SelectObjectRect = outerWallImage.sprite.rect;
                selectEffect.EffectOn();
            }

            if (Input.GetKeyUp(KeyCode.A))
            {
                num--;
                if (num < 0) num = wallList.Count - 2;

                outerWallImage.sprite = wallList[num];
                DoorPositionAdjustment(outerWallImage.sprite.name);
                selectEffect.EffectOff();
                selectEffect.SelectObjectRect = outerWallImage.sprite.rect;
                selectEffect.EffectOn();
            }

            if (Input.GetKeyUp(KeyCode.F))
            {
                Vector3 pos = roofImage.transform.position;
                pos.y += outerWallImage.sprite.rect.size.y / 16f;
                roofImage.transform.position = pos;
                StartCoroutine(PickRoof(character));
                break;
            }
            yield return null;
        }
    }

    void DoorPositionAdjustment(string wallName)
    {
        Vector3 pos = doorImage.transform.localPosition;
        string[] splitName = wallName.Split('_');
        string size = splitName[1];
        string type = splitName[2];
        switch (size)
        {
            case "Small":
                switch (type)
                {
                    case "Type2":
                        pos.x = 0f;
                        break;
                    default:
                        pos.x = -0.5f;
                        break;
                }
                break;
            case "Common":
                switch (type)
                {
                    case "Type3":
                        pos.x = 0f;
                        break;
                    default:
                        pos.x = -0.5f;
                        break;
                }
                break;
            case "Big":
                switch (type)
                {
                    default:
                        pos.x = -0.5f;
                        break;
                }
                break;
        }

        doorImage.transform.localPosition = pos;
    }

    IEnumerator PickRoof(CharacterBasic character)
    {
        List<Sprite> roofList = new List<Sprite>();
        for (int i = 0; i < ResourceManager.Instance.HouseRoofSprites.Length; i++)
        {
            string[] splitName = ResourceManager.Instance.HouseRoofSprites[i].name.Split('_');
            string checkName = splitName[1] + "_" + splitName[2];
            if(outerWallImage.sprite.name.Contains(checkName))
                roofList.Add(ResourceManager.Instance.HouseRoofSprites[i]);
        }
        
        int num = 0;
        roofImage.sprite = roofList[num];
        roofImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.1f);
        while (true)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                character.State = CharacterState.IDLE;
                selectEffect.EffectOff();
                outerWallImage.sprite = null;
                outerWallImage.gameObject.SetActive(false);
                roofImage.sprite = null;
                roofImage.gameObject.SetActive(false);
                doorImage.gameObject.SetActive(false);
                break;
            }

            if (Input.GetKeyUp(KeyCode.D))
            {
                num++;
                if (num >= roofList.Count - 1) num = 0;

                roofImage.sprite = roofList[num];
            }

            if (Input.GetKeyUp(KeyCode.A))
            {
                num--;
                if (num < 0) num = roofList.Count - 2;

                roofImage.sprite = roofList[num];
            }

            if (Input.GetKeyUp(KeyCode.F))
            {
                character.State = CharacterState.WORK;
                character.WC.Working(interactableType);
                yield return StartCoroutine(WorkProcess(character));
                break;
            }
            yield return null;
        }
    }

    public override IEnumerator WorkProcess(CharacterBasic character)
    {
        float workload = 0;
        float value = 0;
        if (character.Items.Equipments[EquipmentType.Weapon] != null)
            value = character.Items.Equipments[EquipmentType.Weapon].Info.Value;
        else
            value = 0.25f;

        outerWallImage.color = new Color32(255, 255, 255, 0);
        roofImage.color = new Color32(255, 255, 255, 0);
        doorImage.color = new Color32(255, 255, 255, 0);
        GameObject obj = Instantiate(ResourceManager.Instance.WorkGaugeObject, MapManager.Instance.MapCanvas, false);
        WorkGauge workgauge = obj.GetComponent<WorkGauge>();
        workgauge.Target = character;
        obj.SetActive(true);

        while (workload < maxTime)
        {
            if (!character.State.Equals(CharacterState.WORK))
            {
                selectEffect.EffectOff();
                outerWallImage.sprite = null;
                outerWallImage.color = new Color32(255, 255, 255, 200);
                outerWallImage.gameObject.SetActive(false);
                roofImage.sprite = null;
                roofImage.color = new Color32(255, 255, 255, 200);
                roofImage.gameObject.SetActive(false);
                doorImage.color = new Color32(255, 255, 255, 200);
                doorImage.gameObject.SetActive(false);
                yield break;
            }

            workgauge.GaugeImage.fillAmount = workload / maxTime;
            outerWallImage.color = new Color(1f,1f,1f, workload / maxTime);
            roofImage.color = new Color(1f,1f,1f, workload / maxTime);
            doorImage.color = new Color(1f, 1f, 1f, workload / maxTime);
            workload += value * Time.deltaTime;
            yield return null;
        }

        Destroy(obj);
        character.IsActing = false;
        WorkComplete();
    }


    public override void WorkComplete()
    {
        buildComplete = true;

        outerWallImage.color = new Color(1f, 1f, 1f, 1f);
        roofImage.color = new Color(1f, 1f, 1f, 1f);
        doorImage.color = new Color(1f, 1f, 1f, 1f);
        sR.sprite = null;
        sR.enabled = false;

        Vector2 size = outerWallImage.sprite.rect.size / 16f;
        size.y += (roofImage.sprite.rect.size / 16f).y;
        size.y = size.y * 0.5f;
        bodyCollider.enabled = true;
        bodyCollider.size = size;
        Vector2 offset = bodyCollider.offset;
        Vector2 sightOffset = offset;
        sightOffset.y -= size.y * 0.5f;
        offset.y = size.y * 0.5f;
        bodyCollider.offset = offset;        
        sightAct.SightCollider.offset = sightOffset;
        siteCollider.enabled = false;

        pS.Data.houseOuterWall = outerWallImage.sprite.name;
        pS.Data.houseRoof = roofImage.sprite.name;

        objectName = "집";
        actDescription = "집 메뉴";

        selectEffect.EffectOff();
    }

    public void SetSaveData(PersonalSpaceData data)
    {
        for (int i = 0; i < ResourceManager.Instance.HouseOuterWallSprites.Length; i++)
        {
            if(data.houseOuterWall.Equals(ResourceManager.Instance.HouseOuterWallSprites[i].name))
            {
                outerWallImage.sprite = ResourceManager.Instance.HouseOuterWallSprites[i];
                break;
            }
        }
        DoorPositionAdjustment(data.houseOuterWall);

        Vector3 pos = roofImage.transform.position;
        pos.y += outerWallImage.sprite.rect.size.y / 16f;
        roofImage.transform.position = pos;

        for (int i = 0; i < ResourceManager.Instance.HouseRoofSprites.Length; i++)
        {
            if (data.houseRoof.Equals(ResourceManager.Instance.HouseRoofSprites[i].name))
            {
                roofImage.sprite = ResourceManager.Instance.HouseRoofSprites[i];
                break;
            }
        }

        outerWallImage.gameObject.SetActive(true);
        roofImage.gameObject.SetActive(true);
        doorImage.gameObject.SetActive(true);

        WorkComplete();
    }

    public override void ObjectDetection(GameObject detectionObj)
    {
        if (pS.OwnerID.Equals("null")) return;

        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.CompareTag("Player") && pS.OwnerID.Equals(cH.CB.ObjectID))
                cH.InteractOn(this);
        }
    }

    public override void ObjectDetecting(GameObject detectionObj) { }
    public override void OutOfDetection(GameObject detectionObj)
    {
        if (pS.OwnerID.Equals("null")) return;

        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.CompareTag("Player"))
                cH.InteractOff(this);
        }
    }
}
