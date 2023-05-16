using UnityEngine;

namespace Zeus
{
    [ClassHeader("BossHealth", "Assign your canvas object in the 'healthBar' field to hide and only display when receive damage - leave it empty if you want to display the healthbar all the time.", OpenClose = false)]
    public class BossHealth : zMonoBehaviour
    {
        [Tooltip("UI to show on receive damage")]
        [Header("UI properties")]
        public string BossName;
        PlayerGaugeTypeUI _bossHpUI;
        ZeusAIController _zeusAI;

        bool _isInitialized = false;

        void Start()
        {
            _zeusAI = transform.GetComponentInParent<ZeusAIController>();
            if (_zeusAI == null)
            {
                Debug.LogWarning("The character must have a ICharacter Interface");
                Destroy(this.gameObject);
                return;
            }
            _zeusAI.onChangeHealth.AddListener(HPChange);
            _zeusAI.OnDeadEvent.AddListener(OnDeath);
            _bossHpUI = PlayerUIManager.Get().GetUI<PlayerGaugeTypeUI>(TypePlayerUI.BOSSHP);
            _isInitialized = true;
        }

        private void OnEnable()
        {
            if (!_isInitialized)
            {
                _bossHpUI = PlayerUIManager.Get().GetUI<PlayerGaugeTypeUI>(TypePlayerUI.BOSSHP);
                if (_bossHpUI == null) return;
                _zeusAI = transform.GetComponentInParent<ZeusAIController>();
            }

            _bossHpUI.SetValue(_zeusAI.MaxHealth, _zeusAI.FillHealthOnStart ? _zeusAI.MaxHealth : _zeusAI.CurrentHealth);
            _bossHpUI.SetGaugeName(BossName);
            _bossHpUI.SetVisible(true);
            Debug.Log($"OnEnable : {_zeusAI.CurrentHealth} / {_zeusAI.MaxHealth}");
        }

        public void OnDeath(GameObject obj)
        {
            _bossHpUI.SetValue(_zeusAI.MaxHealth, 0f);
            _bossHpUI.SetVisible(false);
        }

        public void Damage(Damage damage)
        {
            try
            {
                _bossHpUI.SetValue(_zeusAI.MaxHealth, _zeusAI.CurrentHealth);
            }
            catch
            {
                Destroy(this);
            }
        }

        public void HPChange(float currentHP)
        {
            try
            {
                _bossHpUI.SetValue(_zeusAI.MaxHealth, _zeusAI.CurrentHealth);
            }
            catch
            {
                Destroy(this);
            }
        }

        private void OnDisable()
        {
            _bossHpUI.SetValue(1f, 1f);
            _bossHpUI.SetVisible(false);
        }
    }
}