using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterState { IDLE, WORK, SEARCHING, TRACKING, ATTACK, HIT, DIE }

public abstract class CharacterBasic : InteractionObject
{
    //캐릭터 리지드바디
    protected Rigidbody2D rB;

    //캐릭터 히트박스(바디)
    protected CharacterHitBox hitBox;
    public CharacterHitBox HitBox { get { return hitBox; } }

    //캐릭터 사용 무기 컨트롤러
    protected WeaponController wC;
    public WeaponController WC { get { return wC; } }

    //캐릭터 상태 enum
    [SerializeField]
    protected CharacterState state;
    public CharacterState State { get { return state; } set { state = value; } }
    protected CharacterState prevState;
    public CharacterState PrevState { set { prevState = value; } }

    //현재 좌, 우를 판단하기 위한 수치 정보
    protected float h, v;
    public float characterH { get { return h; } }
    public float characterV { get { return v; } }

    //캐릭터 ID(코드) 정보, 해당 정보로 테이블에서 스팩 관련 정보 불러옴
    [SerializeField]
    protected int characterID;
    public int CharacterID { get { return characterID; } }

    [SerializeField]
    private string[] _targetTags;

    //캐릭터 스팩 관련 정보
    [SerializeField]
    CharacterInfo info;
    public CharacterInfo Info
    {
        get { return info; }
        set
        {
            info = value;
            objectName = info.CharacterName;
            hp = info.MaxHP;
            sight = info.Sight;
        }
    }

    //현재 HP
    [SerializeField]
    protected float hp;
    public float HP
    {
        get { return hp; }
        set
        {
            hp = value;
            if (hp <= 0)
                state = CharacterState.DIE;
        }
    }


    //현재 스태미나
    [SerializeField]
    protected float stamina;
    public float Stamina { get { return stamina; } set { stamina = value; } }
    protected bool staminaRecoveryCheck = true;

    //무기 기본 위치
    protected Transform handPos;
    public Transform HandPos { get { return handPos; } }
    
    //현재 특정 행동 중인지 여부
    protected bool isActing;
    public bool IsActing { get { return isActing; } set { isActing = value; } }

    //인식하는 상호작용 객체 리스트
    [SerializeField]
    protected List<InteractionObject> recognizedObjects = new List<InteractionObject>();
    public List<InteractionObject> RecognizedObjects { get { return recognizedObjects; } }
    //인식하는 상호작용 객체
    protected InteractionObject interactionObject;

    //캐릭터가 목표로 하는 장소 or 객체
    [SerializeField]
    protected Transform target;
    public Transform Target { set { target = value; } }

    //캐릭터 장비, 소지품 정보
    protected CharacterItem _characterItem;
    public CharacterItem Items { get { return _characterItem; } }

    //캐릭터 UI
    [SerializeField]
    protected InteractionUI interUI;
    public InteractionUI InteractUI { get { return interUI; } set { interUI = value; } }
    protected EnemyHpUI enemyHpUI = null;

    //잔상
    protected AfterImage afterImage;

    int saveH, saveV;

    protected override void Awake()
    {
        base.Awake();

        rB = GetComponent<Rigidbody2D>();
        _characterItem = GetComponent<CharacterItem>();
    }

    protected override void Update()
    {
        base.Update();

        if (isActing) return;

        aT.SetFloat("Horizontal", h);
        aT.SetFloat("Vertical", v);
    }

    protected override void OnDestroy()
    {
        if (interUI != null)
            Destroy(interUI.gameObject);
    }

    protected virtual void LookDirection()
    {
        if (isActing) return;

        int intH = (int)h;
        int intV = (int)v;
        aT.SetFloat("Horizontal", intH);
        aT.SetFloat("Vertical", intV);
        if ((intH.Equals(0) && !intV.Equals(0)) || (!intH.Equals(0) && intV.Equals(0)))
        {
            saveH = intH;
            saveV = intV;
            aT.SetFloat("SaveH", saveH);
            aT.SetFloat("SaveV", saveV);
        }
    }

    protected void SetHandState()
    {
        if (handPos == null || wC.IsAttack) return;

        float rotaion = 0;
        if (saveH > 0) rotaion = 270f;
        else if (saveH < 0) rotaion = 90f;
        else if (saveV < 0) rotaion = 180f;

        Vector3 middlePos = transform.position + new Vector3(0f, sR.bounds.size.y / 2);
        handPos.transform.position = middlePos + new Vector3(0.5f * saveH, 0.5f * saveV);
        handPos.eulerAngles = saveH < 0 ? new Vector3(180f, 0, rotaion) : new Vector3(0, 0, rotaion);
    }

    protected void AnimationCheck()
    {
        if (h != 0 || v != 0)
            aT.SetBool("IsRun", true);
        else
            aT.SetBool("IsRun", false);
    }

    public void DisplayEnemyHp()
    {
        if (enemyHpUI == null)
        {
            GameObject obj = Instantiate(ResourceManager.Instance.EnemyHpUI, MapManager.Instance.MapCanvas, false);
            enemyHpUI = obj.GetComponent<EnemyHpUI>();
            enemyHpUI.Target = this;
        }
    }

    abstract public void HitHandling(float damage, int knockbackPower, Vector3 attackerPos);

    public IEnumerator HitInvincibility()
    {
        if (hitBox.InvincibilityCheck) yield break;
        hitBox.InvincibilityCheck = true;   
        yield return StartCoroutine(Flash());
        hitBox.InvincibilityCheck = false;
        state = prevState;
    }

    IEnumerator Flash()
    {
        sR.material = ResourceManager.Instance.FlashWhite;
        float amount = 0f;
        while (amount < 1f)
        {
            sR.material.SetFloat("_FlashAmount", amount);
            amount += 10f * Time.deltaTime;
            yield return null;
        }

        while (amount >= 0f)
        {
            sR.material.SetFloat("_FlashAmount", amount);
            amount -= 10f * Time.deltaTime;
            yield return null;
        }
        sR.material.SetFloat("_FlashAmount", 0f);
        sR.material = ResourceManager.Instance.URPSprite;
    }

    public void CheckForNearbyObjects()
    {
        if (recognizedObjects.Count.Equals(0) || isActing)
        {
            interactionObject = null;
            if (interUI != null)
                interUI.gameObject.SetActive(false);
            return;
        }

        float dis = 999999999f;
        int saveIndex = 0;
        for (int i = 0; i < recognizedObjects.Count; i++)
        {
            if (recognizedObjects[i] == null)
            {
                recognizedObjects.RemoveAt(i);
                if (recognizedObjects.Count.Equals(0))
                {
                    interactionObject = null;
                    if (interUI != null)
                        interUI.gameObject.SetActive(false);
                    return;
                }
                i--;
                continue;
            }

            float tempDis = Vector2.Distance(transform.position, recognizedObjects[i].transform.position);
            if (tempDis < dis)
            {
                dis = tempDis;
                saveIndex = i;
            }
        }
        interactionObject = recognizedObjects[saveIndex];
        interactionObject.EnalbeOutline();
        interUI.SetTextInfo(interactionObject.InteractionKey, interactionObject.ActDescription);
        interUI.gameObject.SetActive(true);
    }


    public override void Interaction(CharacterBasic character) { }
    public override void ObjectDetection(GameObject detectionObj) { }
    public override void ObjectDetecting(GameObject detectionObj) { }
    public override void OutOfDetection(GameObject detectionObj) { }

    public class HitInfo
    {
        //public CharacterBasic Attacker;
        public float Damage;
        public float KnockbackPower;
    }


    public System.Action<HitInfo> OnHit;

    public void SetSight(float sight)
    {
        this.sight = sight;
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (rB != null)
            rB.velocity = velocity;
    }

    public void AddForce(Vector2 force)
    {
        if (rB != null)
            rB.AddForce(force);
    }

    public virtual Transform Detection()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, sight);
        for (int i = 0; i < colliders.Length; i++)
        {
            var collider = colliders[i];
            var str = _targetTags.FirstOrDefault(x => collider.CompareTag(x));

            if (str != null)
                return collider.transform;
        }
        return null;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Hit"))
        {
            OnHit?.Invoke(null);
        }
    }
}
