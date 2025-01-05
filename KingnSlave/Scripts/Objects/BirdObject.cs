using DG.Tweening;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class BirdObject : MonoBehaviour
    {
        private RectTransform _rectTr, _canvasRectTr;

        [SerializeField] private float _duration = 10f;

        private void Awake()
        {
            _rectTr = GetComponent<RectTransform>();
        }

        // Start is called before the first frame update
        void Start()
        {
            var lobby = FindObjectOfType<UILobbyScene>();
            if (lobby == null) return;
            _canvasRectTr = lobby.GetComponent<RectTransform>();
            Initialize();
        }

        private void Initialize()
        {
            _duration = Random.Range(5f, 10f);
            var posX = ((_canvasRectTr.sizeDelta.x * 0.5f) + Random.Range(100f, 1000f)) * -1;
            var posY = Random.Range(-(_canvasRectTr.sizeDelta.y * 0.25f), _canvasRectTr.sizeDelta.y * 0.25f);
            _rectTr.anchoredPosition = new Vector2(posX, posY);

            _rectTr.DOAnchorPos(new Vector2(-posX, posY + Random.Range(100f, 600f)), _duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => Initialize());
        }
    }
}
