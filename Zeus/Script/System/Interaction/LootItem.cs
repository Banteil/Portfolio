using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Zeus
{
    public class LootItem : InteractionTrigger
    {
        private int _effectID;
        private GameObject _lootObject;
        private LootItemData _lootItemData;

        private bool _isInteractable = false;

        private void Start()
        {
            float x = Random.Range(0f, 1f);
            float z = Random.Range(0f, 1f);
            GetComponent<Rigidbody>()?.AddForce(new Vector3(x, 1f, z) * 500f);
        }
        public void Initialize(LootItemData itemData)
        {
            _lootItemData = itemData;

            // LootItem ������Ʈ ����
            var lootObjectPath = _lootItemData.LootObject;
            if (!string.IsNullOrEmpty(lootObjectPath))
            {
                _lootObject = TableManager.GetGameObject(lootObjectPath);
                if (_lootObject != null)
                {
                    _lootObject.transform.SetParent(this.transform);
                    _lootObject.transform.SetPositionAndRotation(this.transform.position, Random.rotation);
                }
            }

            // ����Ʈ ����
            var effectObject = TableManager.GetEffectGameObject((int)_lootItemData.ItemType);

            // ������ ����Ʈ�� ������Ʈ�� ����ٴϵ���
            if (effectObject != null)
            {
                _effectID = EffectsManager.Get().SetEffect(effectObject, this.transform.position);
                var chaseTransform = effectObject.AddComponent<ChaseTransform>();
                if (chaseTransform != null)
                    chaseTransform.FollowTarget = _lootObject.transform;
            }
        }
        public void Take()
        {
            var player = CharacterObjectManager.Get().GetPlayerbleCharacter();
            if (player == null) return;

            GetReward();

            // �ִϸ��̼�
            player.Animator.SetTrigger(AnimatorParameters.PickUp);

            //if (!string.IsNullOrEmpty(_lootTableData.InteractionAnim))
            //    CharacterObjectManager.Get().GetPlayerbleCharacter().Animator.Play(_lootTableData.InteractionAnim);

            // ����Ʈ ��ȯ
            EffectsManager.Get().ReleaseEffect(_effectID);
            // ������Ʈ �ı�
            Destroy(this.gameObject);
        }

        public override void OnEnter(InteractionActor actor)
        {
            base.OnEnter(actor);

            if (_isInteractable) return;

            // show prompt
            PlayerInteractionTypeUI interactionUI = PlayerUIManager.Get().GetUI<PlayerInteractionTypeUI>(TypePlayerUI.INTERACTION);
            interactionUI.OnEnter(this.gameObject, Take);

            _isInteractable = true;
        }
        public override void OnLeave()
        {
            base.OnLeave();

            if (!_isInteractable) return;

            // hide prompt
            PlayerInteractionTypeUI interactionUI = PlayerUIManager.Get().GetUI<PlayerInteractionTypeUI>(TypePlayerUI.INTERACTION);
            interactionUI.OnLeave(this.gameObject);

            _isInteractable = false;
        }

        private void GetReward()
        {
            var playerData = TableManager.CurrentPlayerData;
            if (playerData == null) return;

            var value = Random.Range(_lootItemData.ValueRange.x, _lootItemData.ValueRange.y);

            switch (_lootItemData.ItemType)
            {
                case TypeItem.COIN:
                    // ����� ���ιۿ� ����.. ��
                    // ���� ����
                    playerData.IncreaseCoin(value);
                    break;
                case TypeItem.WEAPON:
                    break;
                case TypeItem.CONSUME:
                    playerData.AddConsume(_lootItemData.ItemID, value);
                    PlayerUIManager.Get().GetUI<PlayerConsumeUI>(TypePlayerUI.CONSUMABLE).RefreshConsume();
                    break;
            }
        }
    }
}
