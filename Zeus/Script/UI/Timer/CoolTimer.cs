namespace Zeus
{
    public class CoolTimer : ZeusTimer
    {
        internal PlayerSkillUI UpdateUI;

        internal float FillAmount
        {
            get
            {
                var amount = RemainTime > 0f ? 1f - (RemainTime / Duration) : 1f;
                return amount;
            }
        }

        public CoolTimer(int id, float duration) : base(id, duration) { }

        internal void UpdateElapsedTime(float elapsedTime)
        {
            ElapsedTime = elapsedTime;  
        }

        internal override void Done()
        {
            if (UpdateUI != null)
                UpdateUI.SetGauge(FillAmount);
        }

        internal override void Tick()
        {
            if (UpdateUI != null)
                UpdateUI.SetGauge(FillAmount);
        }
    }
}
