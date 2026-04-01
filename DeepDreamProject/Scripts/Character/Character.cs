using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CharacterControlType { PLAYER, AI }
public enum CharacterStates { IDLE, ACT, DIE }

public class Character : MonoBehaviour
{
    [Header("Character Info")]
    public CharacterControlType ControlType;
    public DimensionTypes DimensionTypes;
    public CharacterStates State;
    public Faction Faction;

    [Header("Bind Info")]
    public Transform BindModel;
    public Transform BindAbility;
    public Transform BindAIBrain;
    public Transform BindWeaponAttachment;

    public Vector3 CurrentMovement { get; set; }
    public float VelocityMagnitude
    {
        get
        {
            if (DimensionTypes.Equals(DimensionTypes.DIMENSION2D))
                return _rigidbody2D.velocity.magnitude;
            else
                return _rigidbody.velocity.magnitude;
        }
    }
    protected CharacterStat _characterStat;
    public CharacterStat CharacterStat { get { return _characterStat; } }
    protected Health _health;
    public Health Health { get { return _health; } }
    protected List<CharacterAbility> _abilities;
    public List<CharacterAbility> Abilities { get { return _abilities; } }
    protected Animator _animator;
    public Animator Animator { get { return _animator; } }
    protected Renderer _renderer;
    public Renderer Renderer { get { return _renderer; } }
    protected Rigidbody2D _rigidbody2D;
    public Rigidbody2D Rigidbody2D { get { return _rigidbody2D; } }
    protected Rigidbody _rigidbody;
    public Rigidbody Rigidbody { get { return _rigidbody; } }
    protected Vector3 _impact = Vector3.zero;
    public Vector3 Impact { set { _impact = value; } }

    protected virtual void Awake()
    {
        PreInitialization();
    }

    protected virtual void PreInitialization()
    {
        DimensionTypes = GetType().Equals(typeof(Character2D)) ? DimensionTypes.DIMENSION2D : DimensionTypes.DIMENSION3D;
        _characterStat = GetComponent<CharacterStat>();
        _health = GetComponent<Health>();
        if(_health != null) _health.OnDeath += OnDeath;
        _animator = GetComponentInChildren<Animator>();
        InitializeAbilityList();
    }

    protected virtual void InitializeAbilityList()
    {        
        _abilities = BindAbility != null ? BindAbility.GetComponentsInChildren<CharacterAbility>().ToList() : transform.GetComponentsInChildren<CharacterAbility>().ToList();
    }

    protected virtual void Start()
    {
        Initialization();
    }

    protected virtual void Initialization()
    {
        if (ControlType.Equals(CharacterControlType.PLAYER))
        {
            if (DimensionTypes.Equals(DimensionTypes.DIMENSION2D))
            {
                FollowCamera2D followCamera = Camera.main.GetComponent<FollowCamera2D>();
                if (followCamera != null)
                {
                    followCamera.Target = transform;
                    SpriteRenderer sR = (SpriteRenderer)_renderer;
                    Vector3 offset = DataManager.Instance.GetAlphaCroppedSpriteOffset(sR.sprite);
                    offset.x = 0f;
                    offset.z = 0f;
                    followCamera.Offset = offset;
                }
            }
            else
            {
                //3D일 경우 Offset 세팅
            }

            _characterStat.StrengthStat.BaseValue = 5f;
            _characterStat.AgilityStat.BaseValue = 5f;
            _characterStat.VitalityStat.BaseValue = 5f;
        }
    }

    protected virtual void Update()
    {
        if (State.Equals(CharacterStates.DIE)) return;

        EarlyProcessAbilities();
        ProcessAbilities();
        LateProcessAbilities();
        UpdateAnimators();
    }

    protected virtual void EarlyProcessAbilities()
    {
        foreach (CharacterAbility ability in _abilities)
        {
            if (ability.enabled && ability.AbilityInitialized)
            {
                ability.EarlyProcessAbility();
            }
        }
    }

    protected virtual void ProcessAbilities()
    {
        foreach (CharacterAbility ability in _abilities)
        {
            if (ability.enabled && ability.AbilityInitialized)
            {
                ability.ProcessAbility();
            }
        }
    }

    protected virtual void LateProcessAbilities()
    {
        foreach (CharacterAbility ability in _abilities)
        {
            if (ability.enabled && ability.AbilityInitialized)
            {
                ability.LateProcessAbility();
            }
        }
    }

    protected virtual void UpdateAnimators()
    {
        if (Animator != null)
        {
            //기본 애니메이션 재생

            foreach (CharacterAbility ability in _abilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                {
                    ability.UpdateAnimator();
                }
            }
        }
    }

    public T GetAbility<T>() where T : CharacterAbility
    {
        Type searchedAbilityType = typeof(T);

        foreach (CharacterAbility ability in _abilities)
        {
            if (ability is T characterAbility)
            {
                return characterAbility;
            }
        }

        return null;
    }

    #region 캐릭터 행동 함수

    protected virtual void FixedUpdate()
    {
        if (State.Equals(CharacterStates.DIE)) return;

        ApplyImpact();
        CharacterMovement();
    }

    protected virtual void CharacterMovement() { }

    protected virtual void ApplyImpact()
    {
        _impact = Vector3.Lerp(_impact, Vector3.zero, 5f * Time.deltaTime);
    }

    protected virtual void OnDeath()
    {
        State = CharacterStates.DIE;
        Reset();
    }

    public virtual void Reset()
    {
        if (DimensionTypes.Equals(DimensionTypes.DIMENSION2D))
        {
            _rigidbody2D.velocity = Vector3.zero;
        }
        else
        {
            _rigidbody.velocity = Vector3.zero;
        }
    }
    #endregion
}
