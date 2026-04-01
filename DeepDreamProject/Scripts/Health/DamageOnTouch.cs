using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnTouch : MonoBehaviour
{
    /// the possible ways to add knockback : noKnockback, which won't do nothing, set force, or add force
    public enum KnockbackStyles { NoKnockback, AddForce }

    [Header("Binding")]
    public Transform OwnerTr;

    [Header("Targets")]
    public LayerMask TargetLayerMask;

    [Header("Damage Caused")]
    public float DamageCaused = 1f;
    public virtual float Damage
    {
        get
        {
            float ownerDamage = 0f;
            float statDamage = 0f;
            if (_item != null)
            {
                if (_item.ActState.Equals(ItemActStates.BEINGTHROWN))
                    ownerDamage = _item.Data.ThrowDamage;
                else
                    ownerDamage = _item.Data.AttackDamage;

                if (_item.Owner != null)
                {
                    statDamage = _item.Owner.CharacterStat.StrengthStat.Value;
                }
            }
            float damage = ownerDamage + statDamage + DamageCaused;
            return damage;
        }
        set
        {
            DamageCaused = value;
        }
    }
    /// the type of knockback to apply when causing damage
    [Tooltip("the type of knockback to apply when causing damage")]
    public KnockbackStyles DamageCausedKnockbackType = KnockbackStyles.AddForce;
    /// The force to apply to the object that gets damaged
    [Tooltip("The force to apply to the object that gets damaged")]
    public Vector3 DamageCausedKnockbackForce = new Vector3(10, 10, 0);
    /// The duration of the invincibility frames after the hit (in seconds)
    [Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
    public float InvincibilityDuration = 0.5f;
    /// if this is true, damage will only be applied on TriggerEnter, and not on TriggerStay
    [Tooltip("if this is true, damage will only be applied on TriggerEnter, and not on TriggerStay")]
    public bool DamageOnTriggerEnterOnly = false;

    [Header("Damage Taken")]
    /// The amount of damage taken every time, whether what we collide with is damageable or not
    [Tooltip("The amount of damage taken every time, whether what we collide with is damageable or not")]
    public int DamageTakenEveryTime = 0;
    /// The amount of damage taken when colliding with a damageable object
    [Tooltip("The amount of damage taken when colliding with a damageable object")]
    public int DamageTakenDamageable = 0;
    /// The amount of damage taken when colliding with something that is not damageable
    [Tooltip("The amount of damage taken when colliding with something that is not damageable")]
    public int DamageTakenNonDamageable = 0;
    /// the type of knockback to apply when taking damage
    [Tooltip("the type of knockback to apply when taking damage")]
    public KnockbackStyles DamageTakenKnockbackType = KnockbackStyles.NoKnockback;
    /// The force to apply to the object that gets damaged
    [Tooltip("The force to apply to the object that gets damaged")]
    public Vector3 DamageTakenKnockbackForce = Vector3.zero;
    /// The duration of the invincibility frames after the hit (in seconds)
    [Tooltip("The duration of the invincibility frames after the hit (in seconds)")]
    public float DamageTakenInvincibilityDuration = 0.5f;

    [Header("Feedbacks")]
    /// the feedback to play when hitting a Damageable
    [Tooltip("the feedback to play when hitting a Damageable")]
    public MMFeedbacks HitDamageableFeedback;
    /// the feedback to play when hitting a non Damageable
    [Tooltip("the feedback to play when hitting a non Damageable")]
    public MMFeedbacks HitNonDamageableFeedback;

    // storage		
    protected Vector3 _lastPosition, _lastDamagePosition, _velocity, _knockbackForce, _damageDirection;
    protected float _startTime = 0f;
    protected Rigidbody _colliderRigidBody;
    protected Rigidbody2D _colliderRigidBody2D;
    protected Health _health;
    protected Health _colliderHealth;
    protected List<GameObject> _ignoredGameObjects;
    protected Vector3 _knockbackForceApplied;
    protected BoxCollider2D _collider2D;
    public BoxCollider2D Collider2D { get { return _collider2D; } }
    protected BoxCollider _collider;
    public BoxCollider Collider { get { return _collider; } }
    protected Color _gizmosColor;
    protected Vector3 _gizmoSize;
    protected Vector3 _gizmoOffset;
    protected Transform _gizmoTransform;
    protected bool _twoD = false;
    protected Vector3 _positionLastFrame;
    protected Vector3 _scriptDirection;
    protected Item _item;
    protected Character _character;

    /// <summary>
    /// Initialization
    /// </summary>
    protected virtual void Awake()
    {
        InitializeIgnoreList();
        _item = GetComponentInParent<Item>();
        _character = GetComponentInParent<Character>();
        _collider2D = GetComponent<BoxCollider2D>();
        _collider = GetComponent<BoxCollider>();
        _health = GetComponentInParent<Health>();
        _lastDamagePosition = transform.position;
        _twoD = _collider2D != null;

        _gizmosColor = Color.red;
        _gizmosColor.a = 0.25f;
        if (_collider2D != null) { SetGizmoOffset(_collider2D.offset); _collider2D.isTrigger = true; }
        if (_collider != null) { SetGizmoOffset(_collider.center); _collider.isTrigger = true; }
        InitializeFeedbacks();
    }

    public virtual void InitializeFeedbacks()
    {
        HitDamageableFeedback?.Initialization(this.gameObject);
        HitNonDamageableFeedback?.Initialization(this.gameObject);
    }

    public virtual void SetGizmoSize(Vector3 newGizmoSize)
    {
        _collider2D = GetComponent<BoxCollider2D>();
        _collider = GetComponent<BoxCollider>();
        _gizmoSize = newGizmoSize;
    }

    public virtual void SetGizmoOffset(Vector3 newOffset)
    {
        _gizmoOffset = newOffset;
    }

    /// <summary>
    /// OnEnable we set the start time to the current timestamp
    /// </summary>
    protected virtual void OnEnable()
    {
        _startTime = Time.time;
        _lastPosition = this.transform.position;
        _lastDamagePosition = this.transform.position;
    }

    /// <summary>
    /// During last update, we store the position and velocity of the object
    /// </summary>
    protected virtual void Update()
    {
        ComputeVelocity();
    }

    protected void LateUpdate()
    {
        _positionLastFrame = this.transform.position;
    }

    /// <summary>
    /// Initializes the _ignoredGameObjects list if needed
    /// </summary>
    protected virtual void InitializeIgnoreList()
    {
        if (_ignoredGameObjects == null)
        {
            _ignoredGameObjects = new List<GameObject>();
        }
    }

    /// <summary>
    /// Adds the gameobject set in parameters to the ignore list
    /// </summary>
    /// <param name="newIgnoredGameObject">New ignored game object.</param>
    public virtual void IgnoreGameObject(GameObject newIgnoredGameObject)
    {
        InitializeIgnoreList();
        _ignoredGameObjects.Add(newIgnoredGameObject);
    }

    /// <summary>
    /// Removes the object set in parameters from the ignore list
    /// </summary>
    /// <param name="ignoredGameObject">Ignored game object.</param>
    public virtual void StopIgnoringObject(GameObject ignoredGameObject)
    {
        if (_ignoredGameObjects != null)
        {
            _ignoredGameObjects.Remove(ignoredGameObject);
        }
    }

    /// <summary>
    /// Clears the ignore list.
    /// </summary>
    public virtual void ClearIgnoreList()
    {
        InitializeIgnoreList();
        _ignoredGameObjects.Clear();
    }

    /// <summary>
    /// Computes the velocity based on the object's last position
    /// </summary>
    protected virtual void ComputeVelocity()
    {
        if (Time.deltaTime != 0f)
        {
            _velocity = (_lastPosition - (Vector3)transform.position) / Time.deltaTime;

            if (Vector3.Distance(_lastDamagePosition, this.transform.position) > 0.5f)
            {
                _damageDirection = this.transform.position - _lastDamagePosition;
                _lastDamagePosition = this.transform.position;
            }

            _lastPosition = this.transform.position;
        }
    }

    /// <summary>
    /// When a collision with the player is triggered, we give damage to the player and knock it back
    /// </summary>
    /// <param name="collider">what's colliding with the object.</param>
    public virtual void OnTriggerStay2D(Collider2D collider)
    {
        if (DamageOnTriggerEnterOnly)
        {
            return;
        }

        Colliding(collider.gameObject);
    }

    /// <summary>
    /// On trigger enter 2D, we call our colliding endpoint
    /// </summary>
    /// <param name="collider"></param>S
    public virtual void OnTriggerEnter2D(Collider2D collider)
    {
        Colliding(collider.gameObject);
    }

    /// <summary>
    /// On trigger stay, we call our colliding endpoint
    /// </summary>
    /// <param name="collider"></param>
    public virtual void OnTriggerStay(Collider collider)
    {
        if (DamageOnTriggerEnterOnly)
        {
            return;
        }
        Colliding(collider.gameObject);
    }

    /// <summary>
    /// On trigger enter, we call our colliding endpoint
    /// </summary>
    /// <param name="collider"></param>
    public virtual void OnTriggerEnter(Collider collider)
    {
        Colliding(collider.gameObject);
    }

    /// <summary>
    /// 충돌 시 데미지 입힘
    /// </summary>
    /// <param name="collider"></param>
    protected virtual void Colliding(GameObject collider)
    {
        if (!this.isActiveAndEnabled)
        {
            return;
        }

        // if the object we're colliding with is part of our ignore list, we do nothing and exit
        if (_ignoredGameObjects.Contains(collider))
        {
            return;
        }

        // if what we're colliding with isn't part of the target layers, we do nothing and exit
        if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
        {
            return;
        }

        // if we're on our first frame, we don't apply damage
        if (Time.time == 0f)
        {
            return;
        }

        if (FriendlyFactionCheck(collider)) return;

        if (_item != null)
        {
            //아이템이 Drop 상태일 때 체크하지 않음 
            if (_item.State.Equals(ItemStates.DROP)) return;

            if (_item.Owner != null)
            {
                //충돌체가 아이템의 Owner 캐릭터거나 그 자식일 경우 체크하지 않음
                if ((collider == _item.Owner.gameObject) || (collider.transform.IsChildOf(_item.Owner.transform)))
                    return;                
            }

            _item.Reflect();
        }

        _colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();
        if (_colliderHealth == null) _colliderHealth = collider.transform.GetComponentInParent<Health>();

        // 충돌체에 체력이 존재하면 데미지 객체 취급, 아니면 논데미지 객체 취급
        if (_colliderHealth != null)
        {
            if (_colliderHealth.CurrentHP > 0)
            {
                OnCollideWithDamageable(_colliderHealth);
            }
        }
        else
        {
            OnCollideWithNonDamageable();
        }
    }

    protected virtual bool FriendlyFactionCheck(GameObject collider)
    {
        Character colliderCharacter = collider.GetComponentInParent<Character>();
        if (colliderCharacter == null) return false;

        if (_character != null)
        {
            if (_character.Faction.CheckFriendly(colliderCharacter.Faction))
                return true;
        }

        if (_item != null)
        {
            if (_item.Owner.Faction.CheckFriendly(colliderCharacter.Faction))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Describes what happens when colliding with a damageable object
    /// </summary>
    /// <param name="health">Health.</param>
    protected virtual void OnCollideWithDamageable(Health health)
    {
        if ((DamageCausedKnockbackForce != Vector3.zero) && (!_colliderHealth.Invulnerable) && (!_colliderHealth.ImmuneToKnockback))
        {
            _knockbackForce = DamageCausedKnockbackForce;

            if (_twoD) // if we're in 2D
            {
                // if what we're colliding with is a TopDownController, we apply a knockback force
                _colliderRigidBody2D = health.gameObject.MMGetComponentNoAlloc<Rigidbody2D>();
                if (_colliderRigidBody2D != null)
                {
                    Transform ownerTr = transform;
                    if (_item != null) { ownerTr = _item.transform; }
                    Vector3 relativePosition = _colliderRigidBody2D.transform.position - ownerTr.position;
                    _knockbackForce = Vector3.RotateTowards(DamageCausedKnockbackForce, relativePosition.normalized, 10f, 0f);
                }
            }
            else // if we're in 3D
            {
                _colliderRigidBody = health.gameObject.MMGetComponentNoAlloc<Rigidbody>();
                if (_colliderRigidBody != null)
                {
                    Transform ownerTr = transform;
                    if (_item != null) { ownerTr = _item.transform; }
                    Vector3 relativePosition = _colliderRigidBody.transform.position - ownerTr.position;
                    _knockbackForce.x = relativePosition.normalized.x * DamageCausedKnockbackForce.x;
                    _knockbackForce.y = DamageCausedKnockbackForce.y;
                    _knockbackForce.z = relativePosition.normalized.z * DamageCausedKnockbackForce.z;
                }
            }

            if (DamageCausedKnockbackType == KnockbackStyles.AddForce)
            {
                Character character = health.gameObject.GetComponent<Character>();
                if (character != null)
                    character.Impact = _knockbackForce.normalized * _knockbackForce.magnitude;
            }
        }

        HitDamageableFeedback?.PlayFeedbacks(this.transform.position);

        _colliderHealth.Damage(Damage, gameObject, InvincibilityDuration, InvincibilityDuration, _damageDirection);
        if ((DamageTakenEveryTime + DamageTakenDamageable > 0) && !_colliderHealth.PreventTakeSelfDamage)
        {
            SelfDamage(DamageTakenEveryTime + DamageTakenDamageable);
        }
    }

    /// <summary>
    /// Describes what happens when colliding with a non damageable object
    /// </summary>
    protected virtual void OnCollideWithNonDamageable()
    {
        if (DamageTakenEveryTime + DamageTakenNonDamageable > 0)
        {
            SelfDamage(DamageTakenEveryTime + DamageTakenNonDamageable);
        }

        HitNonDamageableFeedback?.PlayFeedbacks(this.transform.position);
    }

    /// <summary>
    /// Applies damage to itself
    /// </summary>
    /// <param name="damage">Damage.</param>
    protected virtual void SelfDamage(int damage)
    {
        if (_health != null)
        {
            _damageDirection = Vector3.up;
            _health.Damage(damage, gameObject, 0f, DamageTakenInvincibilityDuration, _damageDirection);
        }
    }

    public void ActiveCollider(bool active)
    {
        if (_collider != null) _collider.enabled = active;
        if (_collider2D != null) _collider2D.enabled = active;
    }

    /// <summary>
    /// draws a cube or sphere around the damage area
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;

        if (_collider2D != null)
        {

            if (_collider2D.enabled)
            {

                MMDebug.DrawGizmoCube(this.transform,
                                        _gizmoOffset,
                                        _collider2D.size,
                                        false);
            }
            else
            {
                MMDebug.DrawGizmoCube(this.transform,
                                        _gizmoOffset,
                                        _collider2D.size,
                                        true);
            }
        }

        if (_collider != null)
        {
            if (_collider.enabled)
            {
                MMDebug.DrawGizmoCube(this.transform,
                                        _gizmoOffset,
                                        _collider.size,
                                        false);
            }
            else
            {
                MMDebug.DrawGizmoCube(this.transform,
                                        _gizmoOffset,
                                        _collider.size,
                                        true);
            }
        }
    }
}

