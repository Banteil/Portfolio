using UnityEngine;

namespace starinc.io.gallaryx
{
    public class ArrivalSign : MonoBehaviour
    {
        public float rotationSpeed = 100f; // 회전 속도를 조절하는 변수

        void Update()
        {
            // Z축을 기준으로 시계방향 회전
            transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }
}
