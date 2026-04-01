using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    CharacterBasic cB;
    SpriteRenderer sR;
    BoxCollider2D box;
    Animator aT;
    TrailRenderer tR;
    List<CharacterHitBox> hitList = new List<CharacterHitBox>();
    Transform effectTr;

    public float Damage 
    { 
        get
        {
            if (cB.Items.Equipments[EquipmentType.Weapon] == null)
                return cB.Info.Power;
            else
                return cB.Items.Equipments[EquipmentType.Weapon].Info.Value + cB.Info.Power;
        } 
    }
    [SerializeField]
    float staminaCost = 0;
    public float StaminaCost { get { return staminaCost; } }

    bool isAttack;
    public bool IsAttack { get { return isAttack; } }

    private void Awake()
    {
        cB = GetComponentInParent<CharacterBasic>();
        sR = GetComponent<SpriteRenderer>();
        aT = GetComponent<Animator>();
        box = GetComponent<BoxCollider2D>();
        tR = transform.GetChild(0).GetComponent<TrailRenderer>();
        effectTr = transform.GetChild(1);
    }

    private void LateUpdate()
    {
        ChangeSprite();
    }

    public void Attack()
    {
        if (isAttack) return;
        else if (cB.Items.Equipments[EquipmentType.Weapon] == null) return;

        tR.transform.localPosition = new Vector3(0, sR.sprite.bounds.size.y - 0.5f);
        tR.enabled = true;
        sR.enabled = true;
        isAttack = true;
        switch (cB.Items.Equipments[EquipmentType.Weapon].Info.ItemType)
        {
            case ItemType.SWORD_ONEHAND:
                SoundManager.Instance.PlaySFX(SoundResource.Instance.SwordSwingSFX);
                aT.SetTrigger("Cut");
                break;
            case ItemType.SPEAR_ONEHAND:
                aT.SetTrigger("Sting");
                break;
            case ItemType.AXE_ONEHAND:
                Vector3 middlePos = cB.transform.position + new Vector3(0f, cB.SR.bounds.size.y / 2);
                cB.HandPos.transform.position = middlePos;
                aT.SetTrigger("Swing");
                break;
            default:
                break;
        }
        UseWeaponSkill();
    }

    public void Working(ItemType type)
    {
        if (cB.IsActing) return;

        cB.InteractUI.gameObject.SetActive(false);
        cB.IsActing = true;
        StartCoroutine(WorkingProcess(type));
    }

    IEnumerator WorkingProcess(ItemType type)
    {
        cB.HandPos.position = new Vector2(cB.transform.position.x, cB.HandPos.position.y);
        SyncBodyAnimation(type, true);

        while (cB.IsActing) { yield return null; }
        SyncBodyAnimation(type, false);

        cB.State = CharacterState.IDLE;
    }

    void SyncBodyAnimation(ItemType type, bool isPlay)
    {
        switch (type)
        {
            case ItemType.AXE_ONEHAND:
                cB.AT.SetBool("IsFelling", isPlay);
                aT.SetBool("IsFelling", isPlay);
                break;
            //case ItemType.MINING:
            //    cB.AT.SetBool("IsMining", isPlay);
            //    break;
            //case ItemType.HARVEST:
            //    cB.AT.SetBool("IsHarvest", isPlay);
            //    break;
            //case ItemType.BUILDING:
            //    cB.AT.SetBool("IsBuild", isPlay);
            //    aT.SetBool("IsBuild", isPlay);
            //    break;
            default:
                break;
        }
    }

    void ChangeSprite()
    {
        if (cB.Items.Equipments[EquipmentType.Weapon] != null)
        {
            sR.sprite = cB.Items.Equipments[EquipmentType.Weapon].Info.Sprite;
            box.offset = new Vector2(0, sR.sprite.bounds.size.y / 2f);
            box.size = new Vector3(sR.sprite.bounds.size.x / transform.lossyScale.x,
                                         sR.sprite.bounds.size.y / transform.lossyScale.y,
                                         sR.sprite.bounds.size.z / transform.lossyScale.z);
            effectTr.transform.localPosition = new Vector2(0, sR.sprite.bounds.size.y);
        }
        else
            sR.sprite = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<CharacterHitBox>() != null)
        {
            CollisionCheck(collision.GetComponent<CharacterHitBox>());
        }
    }

    void CollisionCheck(CharacterHitBox hitBox)
    {
        if (hitBox.InvincibilityCheck || hitList.Contains(hitBox)) return;

        hitList.Add(hitBox);
        cB.InteractUI.gameObject.SetActive(false);
        aT.speed = 0;
        Invoke("ReleaseStiffness", 0.1f);
        Camera.main.GetComponent<CameraController>().StartVibrate(0.5f, 0.05f);
        float totalDamage = Damage;
        if (totalDamage <= 0) totalDamage = 1;
        hitBox.Hit(totalDamage, 150, transform.position, cB);
    }

    public void UseWeaponSkill()
    {
        if(cB.Items.Equipments[EquipmentType.Weapon].Info.ItemSkill != null)
        {
            cB.Items.Equipments[EquipmentType.Weapon].Info.ItemSkill.UseSkill(effectTr);
        }
    }

    void ReleaseStiffness()
    {
        aT.speed = 1;
    }

    public void EndAttack()
    {
        tR.enabled = false;
        sR.enabled = false;
        isAttack = false;
        hitList = new List<CharacterHitBox>();
    }
}
