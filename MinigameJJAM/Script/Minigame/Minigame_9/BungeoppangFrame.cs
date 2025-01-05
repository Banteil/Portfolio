using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class BungeoppangFrame : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IBungeoppang
    {
        private const string NONE = "bun_1_empty";
        private const string BUTTER = "bun_2_butter";
        private const string BATTER = "bun_3_batter";
        private const string REDBEAN = "bun_4_paste";
        private const string COOKING = "bun_5_uncooked";
        private const string DONE = "bun_6_cooked";
        private const string OVERCOOK = "bun_7_overcooked";

        private const float COOKING_DONE_TIME = 10f;
        private const float OVERCOOK_TIME = 15f;

        private enum BungeoppangFrameState
        {
            None,
            Butter,
            Batter,
            RedBean,
            Cooking,
            Done,
            Overcook,
        }

        [SerializeField]
        private Image _leftFrame, _rightFrame;
        [SerializeField]
        private GameObject _open, _close;
        [SerializeField]
        private ParticleSystem _smokeEffect;

        private BungeoppangGame _baseGame;
        public BungeoppangGame BaseGame { set { _baseGame = value; } }
        private BungeoppangFrameState _currentState = BungeoppangFrameState.None;

        private float _cookingTime = 0f;
        private Coroutine _cookingRoutine;

        public bool IsDrag { get; set; }
        public IBungeoppang.BungeoppangTool ToolType { get; set; }
        public int Count { get; set; } = 1;

        private Vector3 _initialRightFramePosition;

        private event Action<PointerEventData> _onPointerClick, _onBeginDrag, _onDrag, _onEndDrag;

        private void Awake()
        {
            _initialRightFramePosition = _rightFrame.rectTransform.localPosition;
            ToolType = IBungeoppang.BungeoppangTool.Frame;
        }

        public void SetActionFrame(bool active)
        {
            if (active)
            {
                _onPointerClick = PointerClickProcess;
                _onBeginDrag = BeginDragProcess;
                _onDrag = DragProcess;
                _onEndDrag = EndDragProcess;
            }
            else
            {
                _onPointerClick = null;
                _onBeginDrag = null;
                _onDrag = null;
                _onEndDrag = null;
            }
        }

        public void Reset()
        {
            _rightFrame.sprite = _baseGame.GetSprite(NONE);
            _currentState = BungeoppangFrameState.None;
        }

        public Sprite DragSprite() => _rightFrame.sprite;


        public void OnPointerClick(PointerEventData eventData)
        {
            _onPointerClick?.Invoke(eventData);
        }

        private void PointerClickProcess(PointerEventData eventData)
        {
            var rectTransform = (RectTransform)transform;
            var frameCenter = rectTransform.position;
            Vector3 screenPosition = eventData.position;
            screenPosition.z = Camera.main.nearClipPlane;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            bool isRightSide = worldPosition.x >= frameCenter.x;
            CookingFrame(isRightSide);
        }

        private void CookingFrame(bool isRightSide)
        {
            switch (_currentState)
            {
                case BungeoppangFrameState.None:
                    if (_baseGame.CurrentToggle == BungeoppangGame.BungeoppangGameToggle.ButterToggle)
                    {
                        var rightName = Util.RemoveCloneSuffix(_rightFrame.sprite.name);
                        var leftName = Util.RemoveCloneSuffix(_leftFrame.sprite.name);

                        if (isRightSide)
                        {
                            if (rightName == BUTTER) return;
                            _rightFrame.sprite = _baseGame.GetSprite(BUTTER);
                        }
                        else
                        {
                            if (leftName == BUTTER) return;
                            _leftFrame.sprite = _baseGame.GetSprite(BUTTER);
                        }

                        Manager.Sound.PlaySFX("m9sfx_butter");
                        rightName = Util.RemoveCloneSuffix(_rightFrame.sprite.name);
                        leftName = Util.RemoveCloneSuffix(_leftFrame.sprite.name);
                        if (rightName == BUTTER && leftName == BUTTER)
                            _currentState = BungeoppangFrameState.Butter;
                    }
                    break;
                case BungeoppangFrameState.Butter:
                    if (_baseGame.CurrentToggle == BungeoppangGame.BungeoppangGameToggle.BatterToggle)
                    {
                        var rightName = Util.RemoveCloneSuffix(_rightFrame.sprite.name);
                        var leftName = Util.RemoveCloneSuffix(_leftFrame.sprite.name);

                        if (isRightSide)
                        {
                            if (rightName == BATTER) return;
                            _rightFrame.sprite = _baseGame.GetSprite(BATTER);
                        }
                        else
                        {
                            if (leftName == BATTER) return;
                            _leftFrame.sprite = _baseGame.GetSprite(BATTER);
                        }

                        Manager.Sound.PlaySFX("m9sfx_batter");
                        rightName = Util.RemoveCloneSuffix(_rightFrame.sprite.name);
                        leftName = Util.RemoveCloneSuffix(_leftFrame.sprite.name);
                        if (rightName == BATTER && leftName == BATTER)
                            _currentState = BungeoppangFrameState.Batter;
                    }
                    break;
                case BungeoppangFrameState.Batter:
                    if (_baseGame.CurrentToggle == BungeoppangGame.BungeoppangGameToggle.RedBeanToggle)
                    {
                        Manager.Sound.PlaySFX("m9sfx_redBean");
                        _rightFrame.sprite = _baseGame.GetSprite(REDBEAN);
                        _currentState = BungeoppangFrameState.RedBean;
                    }
                    break;
                case BungeoppangFrameState.RedBean:                    
                    _leftFrame.sprite = _baseGame.GetSprite(NONE);
                    _rightFrame.sprite = _baseGame.GetSprite(COOKING);
                    _currentState = BungeoppangFrameState.Cooking;
                    _cookingRoutine = StartCoroutine(CookingProcess());
                    break;
                case BungeoppangFrameState.Cooking:
                    if (_cookingRoutine != null)
                    {
                        Manager.Sound.PlaySFX("m9sfx_closeFrame");
                        StopCoroutine(_cookingRoutine);
                        _cookingRoutine = null;

                        _cookingTime -= 2f;
                        if (_cookingTime < 0) _cookingTime = 0;
                        _open.SetActive(true);
                        _close.SetActive(false);
                    }
                    else
                    {
                        _cookingRoutine = StartCoroutine(CookingProcess());
                    }
                    break;
                case BungeoppangFrameState.Done:
                    if (_cookingRoutine != null)
                    {
                        Manager.Sound.PlaySFX("m9sfx_closeFrame");
                        StopCoroutine(_cookingRoutine);
                        _cookingRoutine = null;
                        _smokeEffect.Stop();
                        _open.SetActive(true);
                        _close.SetActive(false);
                        _cookingTime = 0f;
                    }
                    break;
                default:
                    if (_cookingRoutine != null)
                    {
                        Manager.Sound.PlaySFX("m9sfx_closeFrame");
                        StopCoroutine(_cookingRoutine);
                        _cookingRoutine = null;
                        _smokeEffect.Stop();
                        var main = _smokeEffect.main;
                        main.startColor = Color.white;
                        _open.SetActive(true);
                        _close.SetActive(false);
                        _cookingTime = 0f;
                    }
                    else
                    {
                        Manager.Sound.PlaySFX("m9sfx_bunClick");
                        Reset();
                    }
                    break;
            }
        }

        private IEnumerator CookingProcess()
        {
            Manager.Sound.PlaySFX("m9sfx_closeFrame");
            _open.SetActive(false);
            _close.SetActive(true);
            while (_currentState == BungeoppangFrameState.Cooking)
            {
                _cookingTime += Time.deltaTime;
                yield return null;
                if (_cookingTime >= COOKING_DONE_TIME)
                {
                    Manager.Sound.PlaySFX("m9sfx_cook");
                    _smokeEffect.Play();
                    _rightFrame.sprite = _baseGame.GetSprite(DONE);
                    _currentState = BungeoppangFrameState.Done;
                    break;
                }
            }

            while (_currentState == BungeoppangFrameState.Done)
            {
                _cookingTime += Time.deltaTime;
                yield return null;
                if (_cookingTime >= OVERCOOK_TIME)
                {
                    Manager.Sound.PlaySFX("m9sfx_overcook");
                    var main = _smokeEffect.main;
                    main.startColor = Color.black;
                    _rightFrame.sprite = _baseGame.GetSprite(OVERCOOK);
                    _currentState = BungeoppangFrameState.Overcook;
                    break;
                }
            }

            while (true)
            {
                yield return null;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _onBeginDrag?.Invoke(eventData);
        }

        private void BeginDragProcess(PointerEventData eventData)
        {
            if (_currentState != BungeoppangFrameState.Done) return;
            IsDrag = true;
            Manager.Sound.PlaySFX("m9sfx_bunClick");
            _rightFrame.GetComponent<Canvas>().sortingOrder = 5;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _onDrag?.Invoke(eventData);
        }

        private void DragProcess(PointerEventData eventData)
        {
            if (!IsDrag) return;
            Vector3 screenPosition = eventData.position;
            screenPosition.z = Camera.main.nearClipPlane;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            _rightFrame.rectTransform.position = worldPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _onEndDrag?.Invoke(eventData);
        }

        private void EndDragProcess(PointerEventData eventData)
        {
            if (!IsDrag) return;
            _rightFrame.rectTransform.localPosition = _initialRightFramePosition;
            _rightFrame.GetComponent<Canvas>().sortingOrder = 1;
            IsDrag = false;
        }
    }
}
