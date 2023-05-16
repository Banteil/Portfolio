using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class ConsumeManager : BaseObject<ConsumeManager>
    {
        [SerializeField] private string _objectAttachTarget;
        struct ConsumeCooldownInfo
        {
            public int TableID;
            public float Time;
        }

        private List<ConsumeCooldownInfo> _cooldowns = new List<ConsumeCooldownInfo>();

        private void Update()
        {
            if (_cooldowns.Count == 0) return;

            for (int i = _cooldowns.Count - 1; i > -1; i--)
            {
                var cooldown = _cooldowns[i];
                cooldown.Time -= GameTimeManager.Instance.DeltaTime;
                if (cooldown.Time <= 0)
                {
                    OnRelease(cooldown.TableID);
                    _cooldowns.RemoveAt(i);
                }
                else
                    _cooldowns[i] = cooldown;
            }
        }

        public void OnConsume(int tableID)
        {
            // 쿨다운여부
            if (_cooldowns.Exists(x => x.TableID == tableID))
            {
                //CallFailed?.Invoke(this, TypeFailed.COOLDOWN);
                return;
            }

            var tableData = TableManager.GetConsumeTableData(tableID);
            if (tableData == null)
            {
                //CallFailed?.Invoke(this, TypeFailed.NOT_FOUND_TABLE_ID);
                return;
            }

            var player = CharacterObjectManager.Get().GetPlayerbleCharacter();
            if (player == null)
            {
                return;
            }
            player.Animator.SetTrigger(AnimatorParameters.UsePotion);

            var attachTarget = player.transform.Find(_objectAttachTarget);
            if (attachTarget != null)
            {
                var consumeObject = TableManager.GetGameObject(tableData.ObjectPath);
                if (consumeObject != null)
                {
                    consumeObject.transform.SetParent(attachTarget);
                    consumeObject.transform.SetPositionAndRotation(attachTarget.transform.position, attachTarget.transform.rotation);
                }
            }

            var playerData = TableManager.CurrentPlayerData;
            if (playerData == null)
            {
                //CallFailed?.Invoke(this, TypeFailed.NOT_FOUNT_SAVEDATA);
                return;
            }

            // 인벤에서 해당 아이템 삭제
            if (!playerData.RemoveConsume(tableID))
            {
                //CallFailed?.Invoke(this, TypeFailed.NOT_ENOUGH_AMOUNT);
                return;
            }

            // 사용효과 적용
            foreach (var effect in tableData.Effects)
                effect.Function.OnEffect(effect.Value);

            var effectPos = player.transform.position;
            effectPos.y += 1.5f;
            EffectsManager.Get().SetEffect(tableData.VfxID, effectPos, player.transform.forward, null, 4f);

            // 쿨타임 등록
            _cooldowns.Add(new ConsumeCooldownInfo()
            {
                TableID = tableID,
                Time = tableData.Cooldown,
            });
        }
        public void OnRelease(int tableID)
        {
            var tableData = TableManager.GetConsumeTableData(tableID);

            // 사용효과 해제
            foreach (var effect in tableData.Effects)
                effect.Function.OnRelease();
        }
    }
}