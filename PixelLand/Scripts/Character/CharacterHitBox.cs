using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHitBox : MonoBehaviour
{
    Collider2D hitCollider;

    CharacterBasic cB;
    public CharacterBasic CB { get { return cB; } }

    [SerializeField]
    bool invincibilityCheck;
    public bool InvincibilityCheck { get { return invincibilityCheck; } set { invincibilityCheck = value; } }

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        cB = transform.parent.GetComponent<CharacterBasic>();
    }

    public void Hit(float damage, int knockbackPower, Vector3 attackerPos, CharacterBasic attacker)
    {
        //죽었을 때나 무적일 땐 패스
        if (cB.State.Equals(CharacterState.DIE) || invincibilityCheck)
            return;
        //일 중에 맞으면 상태 idle로 전환
        else if (cB.State.Equals(CharacterState.WORK))
            cB.State = CharacterState.IDLE;

        //맞는 대상의 상호작용 UI가 존재할 경우 꺼줌
        if (cB.InteractUI != null)
            cB.InteractUI.gameObject.SetActive(false);

        //캐릭터 HP를 데미지만큼 처리
        if (cB.State.Equals(CharacterState.HIT)) Debug.Log("이전 상태 HIT");
        cB.PrevState = cB.State;
        cB.State = CharacterState.HIT;
        cB.HP -= damage;
        //데미지 UI 표시
        Vector3 damagePos = transform.position + (Vector3.up * cB.SR.sprite.bounds.size.y);
        UIManager.Instance.DisplayDamageValue(damagePos, damage);
        //피격 이펙트 생성
        Vector2 middlePos = transform.position + new Vector3(0, cB.SR.bounds.size.y / 2);
        Instantiate(ResourceManager.Instance.HitEffectObj, middlePos + (Random.insideUnitCircle * 0.1f), Quaternion.identity);

        //공격자가 캐릭터이며, 존재하면 공격자에 대한 처리 진행
        if (attacker != null)
        {
            cB.Target = attacker.transform;
            if (cB.InteractUI != null)
                cB.InteractUI.gameObject.SetActive(false);
            InteractOff(attacker);
        }

        StartCoroutine(cB.HitInvincibility());
        //캐릭터 고유 히트 판정 실행
        cB.HitHandling(damage, knockbackPower, attackerPos);
    }

    public void InteractOn(InteractionObject iO)
    {
        if (cB.RecognizedObjects.Contains(iO)) return;
        cB.RecognizedObjects.Add(iO);
        cB.CheckForNearbyObjects();
    }

    public void InteractOff(InteractionObject iO)
    {
        cB.RecognizedObjects.Remove(iO);
        cB.CheckForNearbyObjects();
    }

    public void CreateRunDust()
    {
        RunDust dust = Instantiate(ResourceManager.Instance.RunDustEffect, transform.position, Quaternion.identity).GetComponent<RunDust>();
        Vector3 dir = new Vector3(-cB.characterH, -cB.characterV);
        dust.Direction = dir;
    }

    public void DieEvent()
    {
        Destroy(cB.gameObject);
    }
}
