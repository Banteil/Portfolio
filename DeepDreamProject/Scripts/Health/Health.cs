using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Binding")]
    public Health MasterHealth;
    public Animator TargetAnimator;
    public HealthBar BindHealthBar;

    [Header("Status")]
    public bool Invulnerable = false;
    public bool ImmuneToDamage = false;
    public bool ImmuneToKnockback = false;
    public bool DestroyOnDeath = true;
    public bool NoneAnimation = false;
    public float DelayBeforeDestruction = 0f;

    [Header("Health")]
    public float InitialHP = 10;
    public virtual float MaximumHP
    {
        get { return InitialHP; }
        set { InitialHP = value; }
    }
    [SerializeField]
    protected float _currentHP;
    public float CurrentHP
    {
        get { return _currentHP; }
        set
        {
            _currentHP = value;
        }
    }
    public bool ResetHealthOnEnable = true;

    [Header("Feedbacks")]
    public MMFeedbacks DamageMMFeedbacks;
    public MMFeedbacks DeathMMFeedbacks;
    public bool FeedbackIsProportionalToDamage = false;
    public bool PreventTakeSelfDamage = false;

    public float LastDamage { get; set; }
    public Vector3 LastDamageDirection { get; set; }

    // hit delegate
    public delegate void OnHitDelegate();
    public OnHitDelegate OnHit;

    // death delegate
    public delegate void OnDeathDelegate();
    public OnDeathDelegate OnDeath;

    protected LayerMask _initialLayer;
    protected Renderer _renderer;
    protected Collider2D _collider2D;
    protected Collider _collider3D;
    protected bool _initialized = false;

    /// <summary>
    /// On Start, we initialize our health
    /// </summary>
    protected virtual void Awake()
    {
        Initialization();
    }

    protected virtual void Start()
    {
        GrabAnimator();
        SetInitialHealth();
        UpdateHealthBar();
    }

    public virtual void SetInitialHealth()
    {
        if (MasterHealth == null)
        {
            SetHealth(MaximumHP);
        }
        else
        {
            MaximumHP = MasterHealth.MaximumHP;
        }
    }

    /// <summary>
    /// Grabs useful components, enables damage and gets the inital color
    /// </summary>
    public virtual void Initialization()
    {
        _initialLayer = gameObject.layer;
        _renderer = GetComponentInChildren<Renderer>();
        _collider2D = GetComponentInChildren<Collider2D>();
        _collider3D = GetComponentInChildren<Collider>();

        DamageMMFeedbacks?.Initialization(gameObject);
        DeathMMFeedbacks?.Initialization(gameObject);

        DamageEnabled();
        _initialized = true;
    }

    protected virtual void GrabAnimator()
    {
        if (TargetAnimator == null)
        {
            BindAnimator();
        }
    }

    protected virtual void BindAnimator()
    {
        TargetAnimator = GetComponentInChildren<Animator>();
    }

    protected virtual void OnEnable()
    {
        if (ResetHealthOnEnable)
        {
            SetInitialHealth();
        }
        DamageEnabled();
    }

    /// <summary>
    /// 오브젝트에 데미지 처리
    /// </summary>
    /// <param name="damage">The amount of health points that will get lost.</param>
    /// <param name="instigator">The object that caused the damage.</param>
    /// <param name="flickerDuration">The time (in seconds) the object should flicker after taking the damage.</param>
    /// <param name="invincibilityDuration">The duration of the short invincibility following the hit.</param>
    public virtual void Damage(float damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector3 damageDirection)
    {
        //무적이거나 데미지 이뮨이면 패스
        if (Invulnerable || ImmuneToDamage)
        {
            return;
        }

        if (!this.enabled)
        {
            return;
        }

        //이미 HP가 0이하면 패스
        if ((CurrentHP <= 0) && (InitialHP != 0))
        {
            return;
        }

        //데미지 만큼 현재 HP 감소
        float previousHealth = CurrentHP;
        if (MasterHealth != null)
        {
            previousHealth = MasterHealth.CurrentHP;
            MasterHealth.SetHealth(MasterHealth.CurrentHP - damage);
        }
        else
        {
            SetHealth(CurrentHP - damage);
        }

        LastDamage = damage;
        LastDamageDirection = damageDirection;
        OnHit?.Invoke();

        // we prevent the character from colliding with Projectiles, Player and Enemies
        if (invincibilityDuration > 0)
        {
            DamageDisabled();
            if (gameObject.activeSelf)
                StartCoroutine(DamageEnabled(invincibilityDuration));
        }

        if (!NoneAnimation)
        {
            if (TargetAnimator != null)
            {
                TargetAnimator.SetTrigger("Damage");
            }
        }

        if (FeedbackIsProportionalToDamage)
        {
            DamageMMFeedbacks?.PlayFeedbacks(this.transform.position, damage);
        }
        else
        {
            DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);
        }

        // if health has reached zero we set its health to zero (useful for the healthbar)
        if (MasterHealth != null)
        {
            if (MasterHealth.CurrentHP <= 0)
            {
                MasterHealth.CurrentHP = 0;
                MasterHealth.Kill();
            }
        }
        else
        {
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                Kill();
            }

        }
    }

    /// <summary>
    /// 오브젝트 킬 처리
    /// </summary>
    public virtual void Kill()
    {
        if (ImmuneToDamage)
        {
            return;
        }

        SetHealth(0);

        // we prevent further damage
        DamageDisabled();

        DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);

        if (!NoneAnimation)
        {
            if (TargetAnimator != null)
            {
                TargetAnimator.SetTrigger("Death");
            }
        }

        OnDeath?.Invoke();

        if (DestroyOnDeath)
        {
            if (DelayBeforeDestruction > 0f)
            {
                Invoke("DestroyObject", DelayBeforeDestruction);
            }
            else
            {
                // finally we destroy the object
                DestroyObject();
            }
        }
    }

    /// <summary>
    /// Destroys the object, or tries to, depending on the character's settings
    /// </summary>
    protected virtual void DestroyObject()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Resets the character's health to its max value
    /// </summary>
    public virtual void ResetHealthToMaxHealth()
    {
        SetHealth(MaximumHP);
    }

    /// <summary>
    /// 현재 체력 세팅
    /// </summary>
    /// <param name="newValue"></param>
    public virtual void SetHealth(float newValue)
    {
        CurrentHP = newValue;
        UpdateHealthBar();
    }

    /// <summary>
    /// HP바 업데이트 처리 (작업 중)
    /// </summary>
    public virtual void UpdateHealthBar()
    {
        if (BindHealthBar == null) return;

        BindHealthBar.UpdateBar(_currentHP, MaximumHP);
    }

    /// <summary>
    /// Prevents the character from taking any damage
    /// </summary>
    public virtual void DamageDisabled()
    {
        Invulnerable = true;
    }

    /// <summary>
    /// Allows the character to take damage
    /// </summary>
    public virtual void DamageEnabled()
    {
        Invulnerable = false;
    }

    /// <summary>
    /// makes the character able to take damage again after the specified delay
    /// </summary>
    /// <returns>The layer collision.</returns>
    public virtual IEnumerator DamageEnabled(float delay)
    {
        yield return new WaitForSeconds(delay);
        Invulnerable = false;
    }

    /// <summary>
    /// On Disable, we prevent any delayed destruction from running
    /// </summary>
    protected virtual void OnDisable()
    {
        CancelInvoke();
    }
}
