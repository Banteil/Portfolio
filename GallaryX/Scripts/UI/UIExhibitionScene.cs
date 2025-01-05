using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.gallaryx
{
    public class UIExhibitionScene : UIScene
    {
        enum ExhibitionButton
        {
            ShortcutMenuButton,
            EditOrderInfoButton,
            GuideInfoButton,
            HideUIInfoButton,
            AutoMoveInfoButton,
        }

        enum ExhibitionText
        {
            AutoViewingInfoText,
        }

        enum ExhibitionCanvasGroup
        {
            PCShortcutMenu,
            MobileLandscapeShortcutMenu,
            MobilePortraitShortcutMenu,
        }

        private CanvasGroup _mainMenuGroup;
        private UIEditExhibitsPopup _uIEditPopup;
        private bool _isAutoMove;
        private bool _onShortcutMenu = true;
        private Tween _slideTween;
        private Coroutine _textAnimateRoutine;
        private LayerMask _exhibitsLayer;

        protected override void OnAwake()
        {
            base.OnAwake();
            BindInitialization();
            var cameraController = FindFirstObjectByType<CameraController>();
            cameraController.ClickCallback += Call_ExhibitionClick;

            GameManager.Instance.RequireObjectsSpawnCallback += () => InputManager.Instance.ControlKeyInputCallback += Call_EndAutomaticViewing;
            GameManager.Instance.RequireObjectsSpawnCallback += () =>
            {
                var auto = MainSceneManager.Instance.Player.GetAbility<AutomaticViewingAbility>();
                auto.StartAutomaticProcessing += () =>
                {
                    if (_textAnimateRoutine == null)
                    {
                        _textAnimateRoutine = StartCoroutine(AnimateText());
                    }
                    _isAutoMove = true;
                };

                auto.EndAutomaticProcessing += () =>
                {
                    if (_textAnimateRoutine != null)
                    {
                        StopCoroutine(_textAnimateRoutine);
                        _textAnimateRoutine = null;
                        GetText((int)ExhibitionText.AutoViewingInfoText).text = "";
                    }
                    _isAutoMove = false;
                };
            };

            MainSceneManager.Instance.ShowGuideCallback += ShowGuide;
            _exhibitsLayer = LayerMask.NameToLayer("Exhibits");
        }

        private void BindInitialization()
        {
            Bind<TextMeshProUGUI>(typeof(ExhibitionText));
            Bind<CanvasGroup>(typeof(ExhibitionCanvasGroup));

            _mainMenuGroup = Get<CanvasGroup>((int)(!Util.IsMobileWebPlatform ? ExhibitionCanvasGroup.PCShortcutMenu :
                (Util.IsLandscape ? ExhibitionCanvasGroup.MobileLandscapeShortcutMenu : ExhibitionCanvasGroup.MobilePortraitShortcutMenu)));

            var shortcutUI = transform.GetChild(0);
            for (int i = 0; i < shortcutUI.childCount; i++)
            {
                if (shortcutUI.GetChild(i).gameObject == _mainMenuGroup.gameObject)
                {
                    _mainMenuGroup.alpha = 1;
                    _mainMenuGroup.interactable = true;
                    _mainMenuGroup.blocksRaycasts = true;
                }
                else
                {
                    shortcutUI.GetChild(i).gameObject.SetActive(false);
                }
            }

            var shortcutMenuButton = _mainMenuGroup.transform.Find(ExhibitionButton.ShortcutMenuButton.ToString()).gameObject;
            shortcutMenuButton.BindEvent(ShowShortcutMenu);
            var editButton = _mainMenuGroup.transform.GetChild(1).Find(ExhibitionButton.EditOrderInfoButton.ToString()).gameObject;
            if (GameManager.Instance.UID == "guest" || GameManager.Instance.UID != GameManager.Instance.ExhibitionUID)
                editButton.SetActive(false);
            else
                editButton.BindEvent(EditOrderInfo);
            var guideButton = _mainMenuGroup.transform.GetChild(1).Find(ExhibitionButton.GuideInfoButton.ToString()).gameObject;
            guideButton.BindEvent(GuideInfo);
            var hideButton = _mainMenuGroup.transform.GetChild(1).Find(ExhibitionButton.HideUIInfoButton.ToString()).gameObject;
            hideButton.BindEvent(HideUIInfo);
            var autoButton = _mainMenuGroup.transform.GetChild(1).Find(ExhibitionButton.AutoMoveInfoButton.ToString()).gameObject;
            autoButton.BindEvent(AutoMoveInfo);

            if (Util.IsMobileWebPlatform)
            {
                var autoText = GetText((int)ExhibitionText.AutoViewingInfoText);
                autoText.fontSize = 30;
                var rectText = (RectTransform)autoText.transform;
                rectText.anchorMax = new Vector2(0.5f, 1);
                rectText.anchorMin = new Vector2(0.5f, 1);
                rectText.pivot = new Vector2(0.5f, 1);
                rectText.anchoredPosition = new Vector2(0f, -20f);
            }

            _uIEditPopup = GetComponentInChildren<UIEditExhibitsPopup>();
            _uIEditPopup.EnableCallback += (isEnable) =>
            {
                _mainMenuGroup.alpha = isEnable ? 0 : 1;
                _mainMenuGroup.interactable = !isEnable;
                _mainMenuGroup.blocksRaycasts = !isEnable;
            };
        }

        protected override void OnStart()
        {
            InputManager.Instance.KeyInputEvent.InputUpCallback += StartAutomaticView;
            InputManager.Instance.KeyInputEvent.InputUpCallback += ActiveEditExhibitsUI;
        }

        protected override void ScreenOrientationChanged(bool isLandscape)
        {
            base.ScreenOrientationChanged(isLandscape);
            var prevGroup = _mainMenuGroup;

            _mainMenuGroup = Get<CanvasGroup>((int)(isLandscape ? ExhibitionCanvasGroup.MobileLandscapeShortcutMenu : ExhibitionCanvasGroup.MobilePortraitShortcutMenu));
            _mainMenuGroup.alpha = prevGroup.alpha;
            _mainMenuGroup.interactable = prevGroup.interactable;
            _mainMenuGroup.blocksRaycasts = prevGroup.blocksRaycasts;
            _mainMenuGroup.gameObject.SetActive(prevGroup.gameObject.activeSelf);
            prevGroup.gameObject.SetActive(false);

            var shortcutMenuRect = (RectTransform)_mainMenuGroup.transform;
            var shortcutButton = _mainMenuGroup.transform.Find(ExhibitionButton.ShortcutMenuButton.ToString());
            if (!isLandscape)
                shortcutMenuRect.anchoredPosition = new Vector2(_onShortcutMenu ? 80f * shortcutMenuRect.localScale.x : 0f, shortcutMenuRect.anchoredPosition.y);
            else
                shortcutMenuRect.anchoredPosition = new Vector2(shortcutMenuRect.anchoredPosition.x, _onShortcutMenu ? 0f : 120f * shortcutMenuRect.localScale.y);
            var childImage = shortcutButton.GetChild(0);
            childImage.localRotation = Quaternion.Euler(0f, _onShortcutMenu ? 180f : 0f, 0f);

            var shortcutMenuButton = _mainMenuGroup.transform.Find(ExhibitionButton.ShortcutMenuButton.ToString()).gameObject;
            shortcutMenuButton.BindEvent(ShowShortcutMenu);
            var editButton = _mainMenuGroup.transform.GetChild(1).Find(ExhibitionButton.EditOrderInfoButton.ToString()).gameObject;
            if (GameManager.Instance.UID == "guest" || GameManager.Instance.UID != GameManager.Instance.ExhibitionUID)
                editButton.SetActive(false);
            else
                editButton.BindEvent(EditOrderInfo);
            var guideButton = _mainMenuGroup.transform.GetChild(1).Find(ExhibitionButton.GuideInfoButton.ToString()).gameObject;
            guideButton.BindEvent(GuideInfo);
            var hideButton = _mainMenuGroup.transform.GetChild(1).Find(ExhibitionButton.HideUIInfoButton.ToString()).gameObject;
            hideButton.BindEvent(HideUIInfo);
            var autoButton = _mainMenuGroup.transform.GetChild(1).Find(ExhibitionButton.AutoMoveInfoButton.ToString()).gameObject;
            autoButton.BindEvent(AutoMoveInfo);
        }

        private void EditOrderInfo(PointerEventData data) => ShowEditExhibitsUI();

        private void GuideInfo(PointerEventData data) => MainSceneManager.Instance.ShowGuide();

        private void HideUIInfo(PointerEventData data) => UIManager.Instance.HideUIProcess();

        private void AutoMoveInfo(PointerEventData data) => AutomaticViewing(!_isAutoMove);

        private void ActiveEditExhibitsUI()
        {
            if (InputManager.Instance.KeyInputEvent.InputEditUIKey() == InputState.Up)
            {
                ShowEditExhibitsUI();
            }
        }

        private void ShowEditExhibitsUI()
        {
            if (!_uIEditPopup.IsEnable)
            {
                UIManager.Instance.AddPopupUI(_uIEditPopup);
                _uIEditPopup.IsEnable = true;

            }
            else
            {
                _uIEditPopup.IsEnable = false;
            }
        }

        private void StartAutomaticView()
        {
            if (Util.IsUIFocusing) return;
            if (InputManager.Instance.KeyInputEvent.InputAutoMoveKey() == InputState.Up)
            {
                AutomaticViewing(!_isAutoMove);
            }
        }

        private void ShowShortcutMenu(PointerEventData eventData)
        {
            if (_slideTween != null && _slideTween.IsPlaying()) return;
            var shortcutMenuRect = (RectTransform)_mainMenuGroup.transform;
            var shortcutButton = _mainMenuGroup.transform.Find(ExhibitionButton.ShortcutMenuButton.ToString());

            var widthUI = Util.IsMobileWebPlatform && Util.IsLandscape;
            if (!widthUI)
            {
                _slideTween = shortcutMenuRect.DOAnchorPosX(_onShortcutMenu ? 0f : 80f * shortcutMenuRect.localScale.x, 0.3f).SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            _onShortcutMenu = !_onShortcutMenu;
                            var childImage = shortcutButton.GetChild(0);
                            childImage.localRotation = Quaternion.Euler(0f, _onShortcutMenu ? 180f : 0f, 0f);
                            _slideTween = null;
                        });
            }
            else
            {
                _slideTween = shortcutMenuRect.DOAnchorPosY(_onShortcutMenu ? 120f * shortcutMenuRect.localScale.y : 0f, 0.3f).SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            _onShortcutMenu = !_onShortcutMenu;
                            var childImage = shortcutButton.GetChild(0);
                            childImage.localRotation = Quaternion.Euler(0f, _onShortcutMenu ? 180f : 0f, 0f);
                            _slideTween = null;
                        });
            }
        }

        private void ShowGuide()
        {
            if (_slideTween != null && _slideTween.IsPlaying())
            {
                _slideTween.Kill();
                _slideTween = null;
            }

            var shortcutMenuRect = (RectTransform)_mainMenuGroup.transform;
            var shortcutButton = _mainMenuGroup.transform.Find(ExhibitionButton.ShortcutMenuButton.ToString());
            var widthUI = Util.IsMobileWebPlatform && Util.IsLandscape;

            if (!widthUI)
            {
                shortcutMenuRect.anchoredPosition = new Vector2(0f, shortcutMenuRect.anchoredPosition.y);
                var childImage = shortcutButton.GetChild(0);
                childImage.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                shortcutMenuRect.anchoredPosition = new Vector2(shortcutMenuRect.anchoredPosition.x, 120f);
                var childImage = shortcutButton.GetChild(0);
                childImage.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            _onShortcutMenu = false;
        }

        /// <summary>
        /// 자동 이동 여부 체크
        /// </summary>
        /// <param name="isMove"></param>
        private async void AutomaticViewing(bool isMove)
        {
            if (MainSceneManager.Instance.Player == null || isMove == _isAutoMove) return;

            if (MainSceneManager.Instance.Player.HasAbility<AutomaticViewingAbility>())
            {
                var auto = MainSceneManager.Instance.Player.GetAbility<AutomaticViewingAbility>();
                if (isMove)
                {
                    if (Util.IsMobileWebPlatform)
                    {
                        InputManager.Instance.MobileVirtualController.InactiveRunButton();
                        await UniTask.Yield();
                    }

                    auto.StartAutomaticViewing();
                }
                else
                {
                    auto.EndAutomaticViewing();
                }
            }
        }

        IEnumerator AnimateText()
        {
            int dotCount = 0;
            var infoText = GetText((int)ExhibitionText.AutoViewingInfoText);
            var baseText = Util.GetLocalizedString("UITable", "scene_autoviewing");
            var interval = 0.5f;
            while (true)
            {
                infoText.text = baseText + new string('.', dotCount);
                dotCount = (dotCount + 1) % 4;  // 0, 1, 2, 3 (점 개수는 3까지 반복)
                yield return new WaitForSeconds(interval);
            }
        }

        private void ShowExhibitsInfoPopup(Exhibits exhibits)
        {
            var texture = exhibits.GetTexture();
            if (texture == null) return;

            MainSceneManager.Instance.Player.IsInteraction = true;
            exhibits.SetInfoVideo();
            var popupUI = UIManager.Instance.ShowPopupUI<UIExhibitsInfoPopup>("ExhibitsInfoUI", exhibits.Call_MuteVideo);
            popupUI.SetInfo(exhibits);
        }

        #region Callback
        private void Call_ExhibitionClick(RaycastHit hit)
        {
            if (hit.collider.gameObject.layer == _exhibitsLayer)
            {
                var exhibits = hit.collider.GetComponent<Exhibits>();
                if (exhibits.ExhibitsType == Define.FileType.EMPTY) return;
                ShowExhibitsInfoPopup(exhibits);
            }
        }

        private void Call_EndAutomaticViewing() => AutomaticViewing(false);
        #endregion

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (InputManager.HasInstance)
                InputManager.Instance.ControlKeyInputCallback -= Call_EndAutomaticViewing;
        }
    }
}
