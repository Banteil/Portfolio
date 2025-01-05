using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public string TooltipKey; // 툴팁에 표시할 텍스트
        public enum TooltipDirection { Left, Right, Up, Down }
        public TooltipDirection Direction = TooltipDirection.Right; // 툴팁이 표시될 방향
        public float Spacing = 10f; // 간격 설정

        [SerializeField]
        private float TooltipDelay = 0.5f; // 툴팁이 나타나는 지연 시간
        private GameObject _tooltipInstance; // 현재 툴팁 인스턴스
        private bool _isHovering = false;
        private Coroutine _showTooltipCoroutine;

        private IEnumerator ShowTooltipAfterDelay()
        {
            yield return new WaitForSeconds(TooltipDelay);

            if (_isHovering)
            {
                // 툴팁 프리팹 인스턴스화
                _tooltipInstance = UIManager.Instance.CreateHoveringUI();
                // 툴팁 텍스트 설정
                var hoveringUI = _tooltipInstance.GetComponent<HoveringUI>();
                hoveringUI.InfoText = Util.GetLocalizedString("UITable", TooltipKey);

                yield return null;

                // 툴팁의 크기와 위치를 조정
                var myRect = (RectTransform)transform;
                var targetRect = (RectTransform)_tooltipInstance.transform;
                var canvas = _tooltipInstance.GetComponentInParent<Canvas>();
                var scaler = canvas.GetComponent<CanvasScaler>();

                // 각각의 축에 대해 비율 계산
                float scaleFactorX = Screen.width / scaler.referenceResolution.x;
                float scaleFactorY = Screen.height / scaler.referenceResolution.y;

                // 기본 위치 설정
                Vector3 tooltipPosition = myRect.position;

                // 좌표 변환 (월드 좌표에서 로컬 좌표로 변환)
                Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, tooltipPosition);
                RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform.parent, screenPoint, null, out Vector3 worldPoint);

                // 비율에 맞게 위치 조정
                if (Direction == TooltipDirection.Right)
                {
                    worldPoint += new Vector3((targetRect.rect.width * 0.5f + myRect.rect.width * 0.5f + Spacing) * scaleFactorX, 0, 0);
                }
                else if (Direction == TooltipDirection.Left)
                {
                    worldPoint -= new Vector3((targetRect.rect.width * 0.5f + myRect.rect.width * 0.5f + Spacing) * scaleFactorX, 0, 0);
                }
                else if (Direction == TooltipDirection.Up)
                {
                    worldPoint += new Vector3(0, (targetRect.rect.height * 0.5f + myRect.rect.height * 0.5f + Spacing) * scaleFactorY, 0);
                }
                else if (Direction == TooltipDirection.Down)
                {
                    worldPoint -= new Vector3(0, (targetRect.rect.height * 0.5f + myRect.rect.height * 0.5f + Spacing) * scaleFactorY, 0);
                }

                targetRect.position = worldPoint;
                hoveringUI.ActiveUI();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // 모바일 터치 입력이면 툴팁 표시하지 않음
            if (Util.IsMobileWebPlatform) return;

            _isHovering = true;

            if (_showTooltipCoroutine != null)
            {
                StopCoroutine(_showTooltipCoroutine);
            }

            _showTooltipCoroutine = StartCoroutine(ShowTooltipAfterDelay());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;

            if (_showTooltipCoroutine != null)
            {
                StopCoroutine(_showTooltipCoroutine);
            }

            if (_tooltipInstance != null)
            {
                Destroy(_tooltipInstance);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _isHovering = false;

            if (_showTooltipCoroutine != null)
            {
                StopCoroutine(_showTooltipCoroutine);
            }

            if (_tooltipInstance != null)
            {
                Destroy(_tooltipInstance);
            }
        }
    }
}