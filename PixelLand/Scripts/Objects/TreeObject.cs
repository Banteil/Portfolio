using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeObject : Work
{
    [SerializeField]
    int minIndex, maxIndex;
    int currentIndex;
    Collider2D bodyCollider;

    void Start()
    {
        sR = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();
        sightAct = transform.GetChild(0).GetComponent<SightAct>();
        objectName = "나무";
        actDescription = "나무 베기";
        maxTime = 3;
        interactableType = ItemType.AXE_ONEHAND;
        //sR.sprite = ResourceManager.Instance.TreeSprite[maxIndex];
        outline = GetComponent<SpriteOutline>();
        currentIndex = maxIndex;
    }

    public override IEnumerator WorkProcess(CharacterBasic character)
    {
        float workload = 0;
        float value = 0;
        if (character.Items.Equipments[EquipmentType.Weapon] != null)
        {
            if (character.Items.Equipments[EquipmentType.Weapon].Info.ItemType.Equals(interactableType))
                value = character.Items.Equipments[EquipmentType.Weapon].Info.Value;
            else
                value = character.Items.Equipments[EquipmentType.Weapon].Info.Value * 0.5f;
        }
        else
            value = 0.25f;

        GameObject obj = Instantiate(ResourceManager.Instance.WorkGaugeObject, MapManager.Instance.MapCanvas, false);
        WorkGauge workgauge = obj.GetComponent<WorkGauge>();
        workgauge.Target = character;
        obj.SetActive(true);
        while (workload < maxTime)
        {
            if (!character.State.Equals(CharacterState.WORK))
            {
                character.IsActing = false;
                Destroy(obj);
                yield break;
            }

            workgauge.GaugeImage.fillAmount = workload / maxTime;
            workload += value * Time.deltaTime;
            yield return null;
        }

        Destroy(obj);
        character.IsActing = false;

        WorkComplete();
    }

    public override void WorkComplete()
    {
        sR.sprite = null;
        bodyCollider.enabled = false;
        sightAct.SightCollider.enabled = false;
        selfSwitch = true;
        ItemManager.Instance.DropOfItem(transform.position, _dropInfo, 3);
        currentIndex = minIndex;
        //InvokeRepeating("Growth", 5f, 5f);
    }

    void Growth()
    {
        sR.sprite = ResourceManager.Instance.TreeSprite[currentIndex];
        if (currentIndex.Equals(maxIndex))
        {
            sightAct.SightCollider.enabled = true;
            selfSwitch = false;
            CancelInvoke("Growth");
            return;
        }
        currentIndex++;
        bodyCollider.enabled = true;
    }

    public override void ObjectDetection(GameObject detectionObj)
    {
        if (selfSwitch) return;

        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (!cH.CB.IsActing && cH.CB.gameObject.CompareTag("Player"))
            {
                cH.InteractOn(this);
            }
        }
    }
    public override void ObjectDetecting(GameObject detectionObj)
    {
        if (selfSwitch) return;

        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (!cH.CB.IsActing && cH.CB.gameObject.CompareTag("Player"))
            {
                cH.InteractOn(this);
            }
        }
    }

    public override void OutOfDetection(GameObject detectionObj)
    {
        if (selfSwitch) return;

        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.gameObject.CompareTag("Player"))
            {
                cH.InteractOff(this);
                outline.enabled = false;
            }
        }
    }
}
