using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class MultiOpponentExpression : UIBase
    {
        private Image speechBubble;
        private Image speechBubbleContent;
        private bool isPlaying = false;
        private float timer;

        enum ExpressionImage
        {
            SpeechBubble,
            SpeechBubbleContent
        }

        private void Start()
        {
            Initialized();
            InGameManager.Instance.MultiOpponentShowExpression.AddPersistentListener(OccurExpressionEvent);
        }

        protected override void InitializedProcess()
        {
            Bind<Image>(typeof(ExpressionImage));

            speechBubble = GetImage((int)ExpressionImage.SpeechBubble);
            speechBubbleContent = GetImage((int)ExpressionImage.SpeechBubbleContent);
        }

        private void OccurExpressionEvent(int expressionItemSeq)
        {
            if (isPlaying)
            {
                ChangeSpeechBubbleContent(expressionItemSeq);
                return;
            }
            ShowSpeechBubble(expressionItemSeq);
        }

        private async void ShowSpeechBubble(int expressionItemSeq)
        {
            isPlaying = true;
            speechBubbleContent.sprite = await NetworkManager.Instance.GetSpriteTask(ShopManager.Instance.GetItemTypeList(Define.ItemType.Expression).Find((data) => data.seq == expressionItemSeq).image_url);
            speechBubble.gameObject.SetActive(true);

            timer = Define.EXPESSION_REMAINING_TIME;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                await UniTask.Yield();
            }
            speechBubble.transform.gameObject.SetActive(false);
            isPlaying = false;
        }

        private async void ChangeSpeechBubbleContent(int expressionItemSeq)
        {
            speechBubbleContent.sprite = await NetworkManager.Instance.GetSpriteTask(ShopManager.Instance.GetItemTypeList(Define.ItemType.Expression).Find((data) => data.seq == expressionItemSeq).image_url);
            timer = Define.EXPESSION_REMAINING_TIME;
        }
    }
}