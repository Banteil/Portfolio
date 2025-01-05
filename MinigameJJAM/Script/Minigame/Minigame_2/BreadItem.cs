using DG.Tweening;
using UnityEngine;

namespace starinc.io
{
    public class BreadItem : BaseItem
    {
        #region Cache
        private const string DROPLET_NAME = "item_droplet";
        private const string ICE_NAME = "item_ice";
        private const string SUGER_NAME = "item_sugar";

        private const float ICE_BUFF_TIME = 5f;

        private enum BreadItemType
        {
            Droplet,
            Ice,
            Sugar,
        }

        private BreadItemType _itemType;

        private SpriteRenderer _spriteRenderer;

        private Sequence _spriteSequence;
        #endregion
        protected override void BindInitialization()
        {
            base.BindInitialization();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected override void Reset()
        {
            if (_spriteSequence != null)
                _spriteSequence.Kill();
            _spriteSequence = DOTween.Sequence();
            _spriteSequence.Append(_spriteRenderer.transform.DOLocalMoveY(0.5f, 1f).SetEase(Ease.InQuad))
                    .Append(_spriteRenderer.transform.DOLocalMoveY(0f, 1f).SetEase(Ease.OutQuad))
                    .SetLoops(-1);

            var type = (BreadItemType)Random.Range(0, 2); //현재는 설탕 임시 배제
            SetItemInfo(type);
        }

        protected override void OnDisable()
        {
            if (_spriteSequence != null)
                _spriteSequence.Kill();
            _spriteRenderer.transform.localPosition = Vector3.zero;
        }

        private void SetItemInfo(BreadItemType type)
        {
            var minigameScene = Manager.UI.SceneUI as MinigameSceneUI;
            _itemType = type;
            switch (_itemType)
            {
                case BreadItemType.Droplet:
                    _spriteRenderer.sprite = minigameScene.Data.GetSprite(DROPLET_NAME);
                    break;
                case BreadItemType.Ice:
                    _spriteRenderer.sprite = minigameScene.Data.GetSprite(ICE_NAME);
                    break;
                case BreadItemType.Sugar:
                    _spriteRenderer.sprite = minigameScene.Data.GetSprite(SUGER_NAME);
                    break;
                default:
                    Debug.LogError("BreadItem Setting Error");
                    break;
            }
        }

        protected override void Interact(CreatureController character)
        {
            var minigameScene = Manager.UI.SceneUI as MinigameSceneUI;
            switch (_itemType)
            {
                case BreadItemType.Droplet:
                    character.HP++;
                    minigameScene.Data.GetPrefabObject("DropletEffect", character.transform, false);
                    break;
                case BreadItemType.Ice:
                    character.SetInvisibleState(ICE_BUFF_TIME);
                    var iceObj = minigameScene.Data.GetPrefabObject("IceEffect", character.transform, false);
                    var iceEffect = iceObj.GetComponent<TimeLimitEffect>();
                    iceEffect.Limit = ICE_BUFF_TIME;
                    iceEffect.CloseTime = 1f;
                    break;
                case BreadItemType.Sugar:

                    break;
                default:
                    Debug.LogError("BreadItem Setting Error");
                    break;
            }
        }
    }
}
