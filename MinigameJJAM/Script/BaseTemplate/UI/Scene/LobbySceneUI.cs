using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class LobbySceneUI : SceneUI, IListUI
    {
        #region Cache
        private enum LobbyButton
        {
            SettingButton,
            RemoveAdsButton,
        }

        private enum LobbyScrollRect
        {
            MinigameScrollView,
        }
        #endregion

        protected override void BindInitialization()
        {
            Bind<Button>(typeof(LobbyButton));
            Bind<ScrollRect>(typeof(LobbyScrollRect));

            var settingButton = GetButton((int)LobbyButton.SettingButton);
            settingButton.gameObject.BindEvent(ShowSettingPopup);
            var removeAdsButton = GetButton((int)LobbyButton.RemoveAdsButton);
            if (Manager.User.IsRemoveAds)
                removeAdsButton.gameObject.SetActive(false);
            else
            {
                removeAdsButton.gameObject.BindEvent(ShowRemoveAdsPopup);
                Manager.User.OnActiveRemoveAds += () => removeAdsButton.gameObject.SetActive(false);
            }

            var scrollRect = GetScrollRect((int)LobbyScrollRect.MinigameScrollView) as InfinityScrollRect;            
            scrollRect.OnCreatePoolingList += () =>
            {
                for (int i = 0; i < scrollRect.PoolingObjectList.Count; i++)
                {
                    var list = scrollRect.PoolingObjectList[i] as MinigameListUI;
                    list.OnFocusing += MinigameListFocusing;
                    list.OnDefocusing += MinigameListDefocusing;
                }
            };

            Manager.Game.CurrentGameAddress = -1;
        }

        protected override void EscapeAction()
        {
            var message = Util.GetLocalizedString(Define.LOCALIZATION_TABLE_MESSAGE, "quitGame");
            Manager.UI.ShowMessage(message, Util.QuitApplication);
        }

        protected override void OnStart()
        {
            base.OnStart();
            Manager.Sound.PlayBGM("lobbybgm");
            SettingMinigameList();            
        }

        private async void SettingMinigameList()
        {
            await UniTask.WaitUntil(() => Manager.Game.CompletePreparedData);
            var scrollRect = GetScrollRect((int)LobbyScrollRect.MinigameScrollView) as InfinityScrollRect;
            scrollRect.MaxCount = Manager.Game.Minigames.GetEntriesCount();
            scrollRect.CreatePoolingList<MinigameListUI>();
        }

        #region BindEvent
        private void ShowSettingPopup(PointerEventData data)
        {
            Manager.UI.ShowPopupUI<SettingPopupUI>();
        }       

        private void MinigameListFocusing(int index)
        {
            var scrollRect = GetScrollRect((int)LobbyScrollRect.MinigameScrollView) as InfinityScrollRect;
            for (int i = 0; i < scrollRect.PoolingObjectList.Count; i++)
            {
                var list = scrollRect.PoolingObjectList[i];
                var listIndex = list.GetIndex();
                var button = list.GetComponent<Button>();
                button.interactable = listIndex == index;

                if (listIndex <= index) continue;
                var height = MinigameListUI.DETAIL_HEIGHT - MinigameListUI.BASIC_HEIGHT;
                var rectTr = (RectTransform)list.transform;
                rectTr.anchoredPosition = new Vector2(rectTr.anchoredPosition.x, rectTr.anchoredPosition.y - height);
            } 
        }

        private void MinigameListDefocusing(int index)
        {
            var scrollRect = GetScrollRect((int)LobbyScrollRect.MinigameScrollView) as InfinityScrollRect;
            for (int i = 0; i < scrollRect.PoolingObjectList.Count; i++)
            {
                var list = scrollRect.PoolingObjectList[i];
                var button = list.GetComponent<Button>();
                button.interactable = true;

                if (list.GetIndex() <= index) continue;
                var height = MinigameListUI.DETAIL_HEIGHT - MinigameListUI.BASIC_HEIGHT;
                var rectTr = (RectTransform)list.transform;
                rectTr.anchoredPosition = new Vector2(rectTr.anchoredPosition.x, rectTr.anchoredPosition.y + height);
            }
        }

        private void ShowRemoveAdsPopup(PointerEventData data)
        {
            Manager.UI.ShowPopupUI<RemoveAdsPopupUI>();
        }
        #endregion

        public void SetListData(ListUI listUI)
        {
            var minigameList = listUI as MinigameListUI;
            var index = minigameList.GetIndex();
            var minigameEntry = Manager.Game.Minigames.GetEntryByIndex(index);
            var highScore = Manager.Game.Scores.GetScoreByAddress(minigameEntry.address);
            minigameList.SetListData(minigameEntry, highScore);
        }
    }
}
