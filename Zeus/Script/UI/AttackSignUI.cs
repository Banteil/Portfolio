using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Zeus
{
    public class AttackSignUI : MonoBehaviour
    {
        Image _image;
        Animator _ownerAnimator;
        Transform _visiblePointTr;
        Coroutine _process;

        private void Awake()
        {
            var zeusAI = GetComponentInParent<ZeusAIController>();
            if (zeusAI == null)
            {
                Destroy(gameObject);
                return;
            }

            _ownerAnimator = GetComponentInParent<Animator>();
            if (_ownerAnimator == null)
            {
                Destroy(gameObject);
                return;
            }

            zeusAI.AttackSign = this;
            _image = GetComponentInChildren<Image>(true);
            _image.transform.localScale = Vector3.zero;
            _image.gameObject.SetActive(false);
        }

        public void StartDisplay(HumanBodyBones signPoint, Color hdrColor, int soundID, float duration = 0.5f)
        {
            //UI 마테리얼 개별 프로퍼티 적용 
            Material mat = Instantiate(_image.material);
            mat.SetColor("_Color", hdrColor);
            _image.material = mat;

            if (_ownerAnimator.isHuman)
                _visiblePointTr = _ownerAnimator.GetBoneTransform(signPoint);
            else
            {
                _visiblePointTr = _ownerAnimator.transform;
                var combatManager = _ownerAnimator.GetComponent<CombatManager>();
                if (combatManager != null)
                {
                    foreach (var member in combatManager.Members)
                    {
                        if (member.HitCollider.HitColliderName.Equals("Head"))
                        {
                            _visiblePointTr = member.HitCollider.transform;
                            break;
                        }
                    }
                }
            }

            if (_process != null) StopCoroutine(_process);
            _process = StartCoroutine(DisplayProcess(duration));
        }

        IEnumerator DisplayProcess(float duration)
        {
            _image.transform.localScale = Vector3.zero;
            _image.gameObject.SetActive(true);

            float currentTime = 0f;
            while (currentTime < duration)
            {
                currentTime += GameTimeManager.Instance.DeltaTime;
                _image.transform.localScale = Vector3.Lerp(_image.transform.localScale, Vector3.one, GameTimeManager.Instance.DeltaTime);
                SetUIPosition();
                _image.gameObject.SetActive(IsTargetVisible(Camera.main, transform.position));
                yield return null;
            }

            _image.gameObject.SetActive(false);
            _process = null;
        }

        void SetUIPosition()
        {
            var dir = Camera.main.transform.position - _visiblePointTr.position;
            dir = (Mathf.Abs(1f - dir.magnitude) < float.Epsilon) ? dir.normalized : dir;
            transform.position = _visiblePointTr.position + (dir * 0.5f);
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.back, Camera.main.transform.rotation * Vector3.up);
        }

        bool IsTargetVisible(Camera camera, Vector3 pos)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            foreach (var plane in planes)
            {
                if (plane.GetDistanceToPoint(pos) < 0)
                    return false;
            }
            return true;
        }
    }
}
