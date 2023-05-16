using UnityEngine;
using Unity.Burst;


namespace Zeus
{
    [BurstCompile]
    public abstract class ZeusTimer
    {
        internal int ID;

        internal bool UnScaled;
        internal float StartTime { get; private set; }
        internal float RemainTime { get; private set; }
        internal float Duration { get; private set; }

        protected float ElapsedTime = 0f;

        public ZeusTimer(int id, float duration)
        {
            ID = id;
            RemainTime = Duration = duration;
            if (UnScaled)
            {
                StartTime = Time.realtimeSinceStartup;
            }

            GameTimeManager.Instance.AddTimer(this);
        }

        internal void Update(float step)
        {
            if (ElapsedTime > Duration)
                return;

            ElapsedTime += step;
            RemainTime = UnScaled ? Duration - (Time.realtimeSinceStartup - StartTime) : Duration - ElapsedTime;
            if (RemainTime <= 0f)
            {
                RemainTime = 0f;
                Done();
            }

            Tick();
        }

        internal abstract void Done();
        internal abstract void Tick();
    }
}