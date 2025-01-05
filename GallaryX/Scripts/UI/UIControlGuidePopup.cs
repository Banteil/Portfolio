using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.gallaryx
{
    public class UIControlGuidePopup : UIPopup
    {
        enum ControlGuideGameObject
        {
            Background,
            PCFrame,
            MobileLandscapeFrame,
            MobilePortraitFrame,
        }

        private GameObject _mainFrame;
        private List<GameObject> _guideFrames = new List<GameObject>();

        private int _index = 0;

        protected override void OnAwake()
        {
            UICanvas.sortingOrder = UIManager.Instance.GlobalUI.UICanvas.sortingOrder + 1;
            UIManager.Instance.SetCanvas(gameObject, true);
            Bind<GameObject>(typeof(ControlGuideGameObject));
            if(Util.IsMobileWebPlatform)
                _mainFrame = Get<GameObject>((int)(Util.IsLandscape ? ControlGuideGameObject.MobileLandscapeFrame : ControlGuideGameObject.MobilePortraitFrame));
            else
                _mainFrame = Get<GameObject>((int)ControlGuideGameObject.PCFrame);

            var subFrames = _mainFrame.transform.Find("SubFrames");
            for (int i = 0; i < subFrames.childCount; i++)
            {
                _guideFrames.Add(subFrames.GetChild(i).gameObject);
            }

            var frames = transform.GetChild(1);
            for (int i = 0; i < frames.childCount; i++)
            {
                var frameObj = frames.GetChild(i).gameObject;
                if(frameObj == _mainFrame)
                {
                    var canvasGroup = frameObj.GetComponent<CanvasGroup>();
                    canvasGroup.alpha = 1;
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.interactable = true;
                }
                else
                {
                    frameObj.SetActive(false);
                }
            }

            Get<GameObject>((int)ControlGuideGameObject.Background).BindEvent(OnCloseButtonClicked);            
        }

        protected override void OnStart()
        {
            MainSceneManager.Instance.Player.IsInteraction = true;
        }

        protected override void ScreenOrientationChanged(bool isLandscape)
        {
            base.ScreenOrientationChanged(isLandscape); 
            _guideFrames.Clear();
            var prevCanvasGroup = _mainFrame.GetComponent<CanvasGroup>();
            prevCanvasGroup.alpha = 0;
            prevCanvasGroup.blocksRaycasts = false;
            prevCanvasGroup.interactable = false;
            _mainFrame.SetActive(false);

            _mainFrame = Get<GameObject>((int)(isLandscape ? ControlGuideGameObject.MobileLandscapeFrame : ControlGuideGameObject.MobilePortraitFrame));
            var subFrames = _mainFrame.transform.Find("SubFrames");
            for (int i = 0; i < subFrames.childCount; i++)
            {
                var subFrame = subFrames.GetChild(i).gameObject;
                _guideFrames.Add(subFrame);
                subFrame.SetActive(i == _index);
            }
            var currentCanvasGroup = _mainFrame.GetComponent<CanvasGroup>();
            currentCanvasGroup.alpha = 1;
            currentCanvasGroup.blocksRaycasts = true;
            currentCanvasGroup.interactable = true;
            _mainFrame.SetActive(true);
        }

        public void LeftButton()
        {
            _guideFrames[_index].SetActive(false);
            _index--;
            if(_index < 0)
                _index = _guideFrames.Count - 1;

            _guideFrames[_index].SetActive(true);
        }

        public void RightButton()
        {
            _guideFrames[_index].SetActive(false);
            _index++;
            if (_index >= _guideFrames.Count)
                _index = 0;

            _guideFrames[_index].SetActive(true);
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            UIManager.Instance.CloseSpecificPopupUI<UIControlGuidePopup>();
        }

        public void CloseButton() => OnCloseButtonClicked(null);

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (MainSceneManager.HasInstance)
                MainSceneManager.Instance.Player.IsInteraction = false;
        }
    }
}
