using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    [ClassHeader("HealthController", IconName = "HealthControllerIcon")]
    public class HealthController : zMonoBehaviour, IHealthController
    {
        #region Variables
        [EditorToolbar("Health", order = 0)]
        [SerializeField][ReadOnly] protected bool _isDead;
        [BarDisplay("maxHealth")][SerializeField] protected float _currentHealth;
        public bool IsImmortal = false;
        public bool IsInvincibility = false;
        public bool FillHealthOnStart = true;
        [SerializeField]
        protected int _maxHealth = 100;
        public int MaxHealth
        {
            get
            {
                return _maxHealth;
            }
            protected set
            {
                _maxHealth = value;
            }
        }

        public float CurrentHealth
        {
            get
            {
                return _currentHealth;
            }
            protected set
            {
                if (_currentHealth != value)
                {
                    _currentHealth = value;
                    _currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
                    onChangeHealth.Invoke(_currentHealth);
                    HandleCheckHealthEvents();
                }

                var newDeathState = _currentHealth <= 0;
                if (newDeathState != IsDead)
                {
                    IsDead = newDeathState;
                }
            }
        }

        public virtual bool IsDead
        {
            get
            {
                return _isDead;
            }
            set
            {
                if (_isDead != value)
                {
                    _isDead = value;
                    if (value) OnDeadEvent.Invoke(gameObject);
                }
            }
        }

        public string LastHitObjectName { get; set; }
        public float healthRecovery = 0f;
        public float healthRecoveryDelay = 0f;
        [HideInInspector]
        public float currentHealthRecoveryDelay;
        [EditorToolbar("Events", order = 100)]
        public List<CheckHealthEvent> checkHealthEvents = new List<CheckHealthEvent>();
        [SerializeField] protected OnReceiveDamage _onStartReceiveDamage = new OnReceiveDamage();
        [SerializeField] protected OnReceiveDamage _onReceiveDamage = new OnReceiveDamage();
        [SerializeField] protected OnDead _onDead = new OnDead();
        public ValueChangedEvent onChangeHealth;
        public OnReceiveDamage onStartReceiveDamage { get { return _onStartReceiveDamage; } protected set { _onStartReceiveDamage = value; } }
        public OnReceiveDamage onReceiveDamage { get { return _onReceiveDamage; } protected set { _onReceiveDamage = value; } }
        public OnDead OnDeadEvent { get { return _onDead; } protected set { _onDead = value; } }
        public UnityEvent onResetHealth;
        internal bool inHealthRecovery;
        #endregion

        private void Awake()
        {
            AwakeInit();
        }

        protected virtual void AwakeInit() { }

        private void Start()
        {
            Init();
        }

        protected virtual void Init()
        {
            if (FillHealthOnStart)
            {
                _currentHealth = MaxHealth;
                Debug.Log("FillHealthOnStart");
            }

            currentHealthRecoveryDelay = healthRecoveryDelay;
        }

        protected bool CanRecoverHealth
        {
            get
            {
                return (_currentHealth >= 0 && healthRecovery > 0 && _currentHealth < MaxHealth);
            }
        }

        protected IEnumerator RecoverHealth()
        {
            inHealthRecovery = true;
            while (CanRecoverHealth && !IsDead)
            {
                HealthRecovery();
                yield return null;
            }
            inHealthRecovery = false;
        }

        protected void HealthRecovery()
        {
            if (!CanRecoverHealth || IsDead) return;
            if (currentHealthRecoveryDelay > 0)
                currentHealthRecoveryDelay -= GameTimeManager.Instance.DeltaTime;
            else
            {
                if (CurrentHealth > MaxHealth)
                    CurrentHealth = MaxHealth;
                if (CurrentHealth < MaxHealth)
                    CurrentHealth += healthRecovery * GameTimeManager.Instance.DeltaTime;
            }
        }

        public void AddHealth(int value)
        {
            CurrentHealth += value;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
            if (!IsDead && CurrentHealth <= 0)
            {
                IsDead = true;
                OnDeadEvent.Invoke(gameObject);
            }
            HandleCheckHealthEvents();
        }

        public void ChangeHealth(int value)
        {
            CurrentHealth = value;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
            if (!IsDead && CurrentHealth <= 0)
            {
                IsDead = true;
                OnDeadEvent.Invoke(gameObject);
            }
            HandleCheckHealthEvents();
        }

        public void ChangeMaxHealth(int value)
        {
            healthRecovery = value;
            StartCoroutine(RecoverHealth());
        }

        public void ResetHealth(float health)
        {
            CurrentHealth = health;
            if (IsDead)
            {
                IsDead = false;
            }
            onResetHealth.Invoke();
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            if (IsDead)
            {
                IsDead = false;
            }
            onResetHealth.Invoke();
        }

        public virtual void TakeDamage(Damage damage)
        {
            if (damage != null)
            {
                onStartReceiveDamage.Invoke(damage);
                currentHealthRecoveryDelay = CurrentHealth <= 0 ? 0 : healthRecoveryDelay;

                if (CurrentHealth > 0 && !IsImmortal)
                {
                    CurrentHealth -= damage.DamageValue;
                }
                if (damage.DamageValue > 0)
                {
                    Debug.Log(gameObject.name + " 받은 데미지 : " + damage.DamageValue);
                    LastHitObjectName = damage.AttackObjectName;
                    onReceiveDamage.Invoke(damage);
                }
                HandleCheckHealthEvents();
            }
        }

        protected void HandleCheckHealthEvents()
        {
            var events = checkHealthEvents.FindAll(e => (e.healthCompare == CheckHealthEvent.HealthCompare.Equals && CurrentHealth.Equals(e.healthToCheck)) ||
                                                        (e.healthCompare == CheckHealthEvent.HealthCompare.HigherThan && CurrentHealth > (e.healthToCheck)) ||
                                                        (e.healthCompare == CheckHealthEvent.HealthCompare.LessThan && CurrentHealth < (e.healthToCheck)));

            for (int i = 0; i < events.Count; i++)
            {
                events[i].OnCheckHealth.Invoke();
            }
            if (CurrentHealth < MaxHealth && this.gameObject.activeInHierarchy && !inHealthRecovery)
                StartCoroutine(RecoverHealth());
        }

        [System.Serializable]
        public class CheckHealthEvent
        {
            public int healthToCheck;
            public bool disableEventOnCheck;

            public enum HealthCompare
            {
                Equals,
                HigherThan,
                LessThan
            }

            public HealthCompare healthCompare = HealthCompare.Equals;

            public UnityEvent OnCheckHealth;
        }

        [System.Serializable]
        public class ValueChangedEvent : UnityEvent<float>
        {

        }
    }
}

