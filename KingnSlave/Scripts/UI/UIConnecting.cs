using UnityEngine;

namespace starinc.io.kingnslave
{
    public class UIConnecting : MonoBehaviour
    {
        [SerializeField] private RectTransform processImage;
        private float rotationDuration = 120f;

        private void Start() => Initialized();

        protected void Initialized()
        {
            GetComponent<Canvas>().sortingOrder = 9999;
        }

        private void Update()
        {
            processImage.Rotate(Vector3.forward * -rotationDuration * Time.deltaTime);
        }
    }
}
