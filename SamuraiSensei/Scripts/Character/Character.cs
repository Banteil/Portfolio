using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ElementalProperties { FIRE, WATER, TREE, METAL, EARTH, NONE }
public enum CharacterState { IDLE, MOVE, HIT, ATTACK, DIE, STOP }

public abstract class Character : MonoBehaviour
{
    [SerializeField]
    protected CharacterState state;
    public CharacterState State { get { return state; } set { state = value; } }

    [SerializeField]
    protected int currentLine;
    public int CurrentLine { get { return currentLine; } set { currentLine = value; } }

    [SerializeField]
    protected float hp;
    public float HP { get { return hp; } set { hp = value; } }

    [SerializeField]
    protected Character target;
    public Character Target { get { return target; } }

    protected Animator aT;
    protected SpriteRenderer sR;
    public SpriteRenderer SR { get { return sR; } }

    protected bool isFlash;
    protected float attackDelayTime;

    protected abstract void TargetSearch();
    protected abstract void Idle();
    protected abstract void Attack();
    protected abstract void Die();
    public abstract void Hit(float damage, ElementalProperties attackElement, float rigidTime, float knockBackPower);

    protected virtual IEnumerator RecoveryRigid(float time)
    {
        yield return new WaitForSeconds(time);
        state = CharacterState.IDLE;
    }   

    protected void DisplayDamage(int damage)
    {
        DamageTextObject damageTextObject = null;
        if (BattleUIManager.Instance.DamageTexts.Count <= 0)
        {
            GameObject obj = Instantiate(BattleResourceManager.Instance.DamageTextObject, BattleUIManager.Instance.ObjectCanvasTr, false);
            damageTextObject = obj.GetComponent<DamageTextObject>();
        }
        else
            damageTextObject = BattleUIManager.Instance.DamageTexts.Dequeue();
        Vector3 pos = new Vector3(transform.position.x , transform.position.y + sR.sprite.rect.height * 0.5f * 0.01f);
        damageTextObject.transform.position = pos;
        damageTextObject.DamageText.text = damage.ToString();
        damageTextObject.gameObject.SetActive(true);
    }

    protected IEnumerator Flash()
    {
        isFlash = true;
        float amount = 0f;
        while (amount < 1f)
        {
            sR.material.SetFloat("_FlashAmount", amount);
            amount += 20f * Time.deltaTime;
            yield return null;
        }

        while (amount >= 0f)
        {
            sR.material.SetFloat("_FlashAmount", amount);
            amount -= 20f * Time.deltaTime;
            yield return null;
        }
        sR.material.SetFloat("_FlashAmount", 0f);
        isFlash = false;
    }

    public void StopCharacter()
    {
        aT.StopPlayback();

    }
}
