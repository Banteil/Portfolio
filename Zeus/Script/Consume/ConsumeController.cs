using UnityEngine;

namespace Zeus
{
    public class ConsumeController : MonoBehaviour
    {
        [SerializeField] private Transform _objectAttachTarget;

        [SerializeField] private AnimStateEventSO _onConsumeAnimStateEvent;

        private ConsumeTableData _curConsumeData;

        private bool _inputLeftShoulder;
        private bool _consumable;

        private void OnEnable()
        {
            InputReader.Instance.CallLeftShoulder += OnLeftShoulder;
            InputReader.Instance.CallPotion += OnButtonSouth;

            _onConsumeAnimStateEvent.Regist(OnUpdatAnimState);

            _consumable = true;
        }
        private void OnDisable()
        {
            if (!InputReader.HasInstance)
                return;

            InputReader.Instance.CallLeftShoulder -= OnLeftShoulder;
            InputReader.Instance.CallPotion -= OnButtonSouth;

            _onConsumeAnimStateEvent.Unregist(OnUpdatAnimState);
            
            _consumable = false;
        }
        private void OnLeftShoulder(bool enabled)
        {
            _inputLeftShoulder = enabled;
        }
        private void OnButtonSouth()
        {
            if (!_consumable) return;
            if (!_inputLeftShoulder) return;
            if (_curConsumeData != null) return;
            OnEnter();
        }
        private void OnUpdatAnimState(TypeAnimState state)
        {
            if (state == TypeAnimState.EXIT)
            {
                _curConsumeData = null;
                _consumable = true;

                ConsumeEffectExit();
            }
        }

        private void OnEnter()
        {
            var playerData = TableManager.CurrentPlayerData;
            if (playerData == null) return;

            var tableID = playerData.GetEquipConsumeID();
            if (tableID == 0) return;

            var consumeData = playerData.GetConsumeData(tableID);
            if (consumeData.Amount == 0) return;

            // 쿨다운여부
            ConsumeCoolTimer cooltime = GameTimeManager.Instance.GetCoolTime(tableID) as ConsumeCoolTimer;
            if (cooltime != null) return;

            var tableData = TableManager.GetConsumeTableData(tableID);
            if (tableData == null) return;

            _curConsumeData = tableData;

            var player = CharacterObjectManager.Get().GetPlayerbleCharacter();
            if (player == null) return;

            player.Animator.SetTrigger(AnimatorParameters.UsePotion);
            _consumable = false;
        }
        private void OnRelease(int tableID)
        {
            var tableData = TableManager.GetConsumeTableData(tableID);

            // 사용효과 해제
            foreach (var effect in tableData.Effects)
                effect.Function.OnRelease();
        }

        public void ConsumeObjectCreate()
        {
            if (_curConsumeData == null) return;

            if (_objectAttachTarget != null)
            {
                var objectPos = _objectAttachTarget.position;
                var objectRot = _objectAttachTarget.rotation;
                var consumeObject = TableManager.GetGameObject(_curConsumeData.ObjectPath);
                if (consumeObject != null)
                {
                    consumeObject.transform.SetParent(_objectAttachTarget);
                    consumeObject.transform.SetPositionAndRotation(objectPos, objectRot);
                }
            }
        }
        public void ConsumeEffectEnter()
        {
            if (_curConsumeData == null) return;

            var playerData = TableManager.CurrentPlayerData;
            if (playerData == null) return;

            var player = CharacterObjectManager.Get().GetPlayerbleCharacter();
            if (player == null) return;

            // 인벤에서 해당 아이템 삭제
            if (!playerData.RemoveConsume(_curConsumeData.ID)) return;

            // 사용효과 적용
            foreach (var effect in _curConsumeData.Effects)
                effect.Function.OnEffect(effect.Value);

            var effectPos = player.transform.position;
            effectPos.y += 1.5f;
            EffectsManager.Get().SetEffect(_curConsumeData.VfxID, effectPos, player.transform.forward, null, 4f);

            var consumeUI = PlayerUIManager.Get().GetUI<PlayerConsumeUI>(TypePlayerUI.CONSUMABLE);
            if (consumeUI != null)
            {
                var consumeData = playerData.GetConsumeData(_curConsumeData.ID);
                var timer = new ConsumeCoolTimer(_curConsumeData.ID, _curConsumeData.Cooldown);
                timer.ConsumeUI = consumeUI;
                consumeUI.RefreshConsume(consumeData.Amount > 0);
            }

            var qauickTabUI = PlayerUIManager.Get().GetUI<PlayerQuickTabUI>(TypePlayerUI.QUICKTAB);
            if (qauickTabUI != null)
            {
                qauickTabUI.RefreshCurrentTab();
            }
        }
        public void ConsumeEffectExit()
        {
            if (_objectAttachTarget != null)
            {
                for (int i = _objectAttachTarget.childCount - 1; i > -1; i--)
                {
                    var child = _objectAttachTarget.GetChild(i);
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
