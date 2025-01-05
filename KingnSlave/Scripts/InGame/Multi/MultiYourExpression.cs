using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class MultiYourExpression : UIBase
    {
        private List<ItemData> myExpressionList;
        private Image speechBubble;
        private Image speechBubbleContent;
        private Image expressionSelection;
        private InfinityScrollRect expressionScrollView;
        private bool isPlaying;
        private float timer;

        enum ExpressionUIScrollRect
        {
            ExpressionScrollView
        }

        enum ExpressionUIImages
        {
            SpeechBubble,
            SpeechBubbleContent,
            ExpressionSelection
        }

        private void Start() => Initialized();

        protected override void InitializedProcess() 
        {
            myExpressionList = UserDataManager.Instance.GetInGameExpressionList();

            myExpressionList.Sort((item1, item2) =>
            {
                if (item1.order_no < 0 && item2.order_no >= 0)
                {
                    return -1;
                }
                else if (item1.order_no >= 0 && item2.order_no < 0)
                {
                    return 1;
                }
                // 둘 다 order_no의 부호가 같을 때
                else
                {
                    return -(item1.order_no.CompareTo(item2.order_no));
                }
            });

            Bind<Image>(typeof(ExpressionUIImages));
            Bind<ScrollRect>(typeof(ExpressionUIScrollRect));

            speechBubble = GetImage((int)ExpressionUIImages.SpeechBubble);
            speechBubbleContent = GetImage((int)ExpressionUIImages.SpeechBubbleContent);
            expressionSelection = GetImage((int)ExpressionUIImages.ExpressionSelection);
            expressionScrollView = GetScrollRect((int)ExpressionUIScrollRect.ExpressionScrollView) as InfinityScrollRect;
            expressionSelection.gameObject.BindEvent(ClickExpressionSelectionButton);

            Debug.Log("scrollView?" + expressionScrollView + ", myList? " + myExpressionList);
            expressionScrollView.MaxCount = myExpressionList.Count;
            expressionScrollView.CreatePoolingList<ExpressionButton>("ExpressionButton");

        }

        public override void SetListData(UIList uiList)
        {
            var button = uiList as ExpressionButton;
            var index = button.GetIndex();
            button.SetListData(myExpressionList[index]);
        }

        private void ClickExpressionSelectionButton(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            expressionScrollView.gameObject.SetActive(!expressionScrollView.gameObject.activeInHierarchy);
        }

        public void ClickExpression(int index)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));

            expressionScrollView.gameObject.SetActive(false);

            if (isPlaying)
            {
                ChangeSpeechBubbleContent(index);
                return;
            }
            ShowSpeechBubble(index);
        }

        private async void ShowSpeechBubble(int index)
        {
            isPlaying = true;
            speechBubbleContent.sprite = await NetworkManager.Instance.GetSpriteTask(myExpressionList[index].image_url);
            speechBubble.gameObject.SetActive(true);

            InGameManager.Instance.ClickExpression?.Invoke(myExpressionList[index].item_seq);
            // POST
            try
            {
                StartCoroutine(CallAPI.APIGameRoomInsertGameAction(UserDataManager.Instance.MyData, Define.APIActionCd.expression.ToString(), Define.CardType.None, -1, myExpressionList[index].item_seq.ToString()));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            timer = Define.EXPESSION_REMAINING_TIME;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                await UniTask.Yield();
            }
            speechBubble.transform.gameObject.SetActive(false);
            isPlaying = false;
        }

        private async void ChangeSpeechBubbleContent(int index)
        {
            speechBubbleContent.sprite = await NetworkManager.Instance.GetSpriteTask(myExpressionList[index].image_url);
            timer = Define.EXPESSION_REMAINING_TIME;

            InGameManager.Instance.ClickExpression?.Invoke(myExpressionList[index].item_seq);
            // POST
            try
            {
                StartCoroutine(CallAPI.APIGameRoomInsertGameAction(UserDataManager.Instance.MyData, Define.APIActionCd.expression.ToString(), Define.CardType.None, -1, myExpressionList[index].item_seq.ToString()));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}