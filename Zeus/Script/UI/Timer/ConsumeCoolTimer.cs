using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class ConsumeCoolTimer : ZeusTimer
    {
        internal PlayerConsumeUI ConsumeUI;
        internal Action<int> OnFinish;

        internal float FillAmount
        {
            get
            {
                var amount = RemainTime > 0f ? 1f - (RemainTime / Duration) : 1f;
                return amount;
            }
        }

        public ConsumeCoolTimer(int id, float duration) : base(id, duration) { }

        internal override void Done()
        {
            OnFinish?.Invoke(ID);
            //if (ConsumeUI != null)
            //    ConsumeUI.SetCooldown(0f);
        }

        internal override void Tick()
        {
            var consumeID = TableManager.CurrentPlayerData.GetEquipConsumeID();
            if (consumeID != ID) return;

            if (ConsumeUI != null)
                ConsumeUI.SetCooldown(FillAmount);
        }
    }
}
