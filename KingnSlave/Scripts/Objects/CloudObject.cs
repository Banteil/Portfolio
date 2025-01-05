using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class CloudObject : MonoBehaviour
    {
        private RectTransform _rectTr, _canvasRectTr;
        private Image _image;

        [SerializeField] private float _distance = 1f;
        [SerializeField] private float _duration = 10f;
        [SerializeField] private int _baseSize = 500;

        private void Awake()
        {
            _rectTr = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
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
            _distance = Random.Range(1f, 3f);
            _image.sprite = ResourceManager.Instance.GetSprite(Random.Range(0, 2));
            var sizeResult = _baseSize / _distance;
            _rectTr.sizeDelta = new Vector2(sizeResult, sizeResult);            

            var posX = ((_canvasRectTr.sizeDelta.x * 0.5f) + (sizeResult * 0.5f) + Random.Range(10f, 500f)) * -1;
            var posY = Random.Range(500f, (_canvasRectTr.sizeDelta.y * 0.5f) - 100f);
            _rectTr.anchoredPosition = new Vector2(posX, posY);

            _rectTr.DOAnchorPosX(-posX, _duration * _distance)
            .SetEase(Ease.Linear)
            .OnComplete(() => Initialize());
        }
    }
}
