using System;
using UnityEngine;
using Zeus;
using Random = UnityEngine.Random;

namespace Zeus
{
    public class AE_PhysicsMotion : MonoBehaviour
    {
        public bool UseCollisionDetect = true;
        public float Mass = 1;
        public float Speed = 10;
        public float RandomSpeedOffset = 0f;
        public float AirDrag = 0.1f;
        public bool UseGravity = true;
        public ForceMode ForceMode = ForceMode.Impulse;
        public float ColliderRadius = 0.05f;
        public bool FreezeRotation;

        public bool UseTargetPositionAfterCollision;
        public GameObject EffectOnCollision;
        public bool CollisionEffectInWorldSpace = true;
        public bool LookAtNormal = true;
        public float CollisionEffectDestroyAfter = 5;

        public GameObject[] DeactivateObjectsAfterCollision;

        [HideInInspector] public float HUE = -1;

        public event EventHandler<AE_CollisionInfo> CollisionEnter;

        Rigidbody rigid;
        SphereCollider collid;
        ContactPoint lastContactPoint;
        Collider lastCollider;
        Vector3 offsetColliderPoint;
        bool isCollided;
        GameObject targetAnchor;
        bool isInitializedForce;
        float currentSpeedOffset;

        //Zeus
        public CombatManager Owner;
        private GameObject _target;
        public bool hold = true;
        private GameTimeManager _gameTimeManager;

        public delegate void HitAfterEvent(Vector3 rot);
        public HitAfterEvent HitAfterEvents;
        public bool IgnoreHitLayer = false;
        public Transform ArrowModel;
        private Rigidbody _arrowModelRigid;
        public Vector3 HitAfterOffset;

        //Sound
        private const int _arrowHitSoundID = 115;

        void OnEnable()
        {
            foreach (var obj in DeactivateObjectsAfterCollision)
            {
                if (obj != null)
                {
                    if (obj.GetComponent<ParticleSystem>() != null) obj.SetActive(false);
                    obj.SetActive(true);
                }
            }
            currentSpeedOffset = Random.Range(-RandomSpeedOffset * 10000f, RandomSpeedOffset * 10000f) / 10000f;
            InitializeRigid();
            if (_gameTimeManager == null)
            {
                _gameTimeManager = GameTimeManager.Instance;
            }

            HitAfterEvents += SetHitAfterArrowModel;
        }


        private void OnDisable()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = new Quaternion();
            isCollided = false;
            HitAfterEvents -= SetHitAfterArrowModel;
            if (rigid != null) Destroy(rigid);
            if (collid != null) Destroy(collid);
        }

        void InitializeRigid()
        {
            if (UseCollisionDetect)
            {
                collid = gameObject.AddComponent<SphereCollider>();
                collid.radius = ColliderRadius;
            }

            isInitializedForce = false;
        }

        void InitializeForce()
        {
            if (hold) { return; }
            rigid = gameObject.AddComponent<Rigidbody>();
            rigid.mass = Mass;
            rigid.drag = AirDrag;
            rigid.useGravity = UseGravity;
            if (FreezeRotation) rigid.constraints = RigidbodyConstraints.FreezeRotation;
            rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigid.interpolation = RigidbodyInterpolation.Interpolate;
            rigid.AddForce(transform.forward * (Speed + currentSpeedOffset), ForceMode);

            if (ArrowModel != null)
            {
                _arrowModelRigid = ArrowModel.gameObject.AddComponent<Rigidbody>();
                if (_arrowModelRigid != null)
                {
                    _arrowModelRigid.mass = Mass;
                    _arrowModelRigid.drag = AirDrag;
                    _arrowModelRigid.useGravity = UseGravity;
                    if (FreezeRotation) _arrowModelRigid.constraints = RigidbodyConstraints.FreezeRotation;
                    _arrowModelRigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    _arrowModelRigid.interpolation = RigidbodyInterpolation.Interpolate;
                    _arrowModelRigid.AddForce(transform.forward * (Speed + currentSpeedOffset), ForceMode);
                }
            }
            isInitializedForce = true;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (isCollided && !UseCollisionDetect || hold) return;
            if (collision.gameObject == Owner.gameObject || collision.transform.tag.Equals(Owner.tag) || (IgnoreHitLayer && collision.gameObject.layer == LayerMask.NameToLayer("Hit"))) { return; }
            foreach (ContactPoint contact in collision.contacts)
            {
                if (!isCollided)
                {
                    isCollided = true;
                    offsetColliderPoint = contact.otherCollider.transform.position - contact.point;
                    lastCollider = contact.otherCollider;
                    lastContactPoint = contact;
                    if (UseTargetPositionAfterCollision)
                    {
                        if (targetAnchor != null) Destroy(targetAnchor);

                        targetAnchor = new GameObject();
                        targetAnchor.transform.parent = contact.otherCollider.transform;
                        targetAnchor.transform.position = contact.point;
                        targetAnchor.transform.rotation = transform.rotation;
                        _target = collision.gameObject;
                        //targetAnchor.transform.LookAt(contact.normal);
                    }

                }
                var handler = CollisionEnter;
                if (handler != null)
                    handler(this, new AE_CollisionInfo { ContactPoint = contact });

                //if (EffectOnCollision != null)
                //{
                //    var instance = Instantiate(EffectOnCollision, contact.point, new Quaternion()) as GameObject;

                //    if (HUE > -0.9f)
                //    {
                        //var color = instance.AddComponent<AE_EffectSettingColor>();
                        //var hsv = AE_ColorHelper.ColorToHSV(color.Color);
                        //hsv.H = HUE;
                        //color.Color = AE_ColorHelper.HSVToColor(hsv);
                //    }

                //    if (LookAtNormal) instance.transform.LookAt(contact.point + contact.normal);
                //    else instance.transform.rotation = transform.rotation;
                //    if (!CollisionEffectInWorldSpace) instance.transform.parent = contact.otherCollider.transform.parent;
                //    HitAfterEvents.Invoke((targetAnchor.transform.position - transform.position));

                //    instance.DestroyTimer(CollisionEffectDestroyAfter);
                //}
            }

            foreach (var obj in DeactivateObjectsAfterCollision)
            {
                if (obj != null)
                {
                    var ps = obj.GetComponent<ParticleSystem>();
                    if (ps != null) ps.Stop();
                    else obj.SetActive(false);
                }
            }

            if (rigid != null) Destroy(rigid);
            if (_arrowModelRigid != null) Destroy(_arrowModelRigid);
            if (collid != null) Destroy(collid);
        }

        public void HitBoxTriggerOn(Vector3 position, Collider collision)
        {
            if (isCollided && !UseCollisionDetect || hold) return;
            if (collision.gameObject == Owner.gameObject) { return; }
            if (collision.gameObject == Owner.gameObject || collision.transform.tag.Equals(Owner.tag) || (IgnoreHitLayer && collision.gameObject.layer == LayerMask.NameToLayer("Hit"))) { return; }
            var collisionPoint = collision.ClosestPoint(transform.position);
            if (!isCollided)
            {

                if (UseTargetPositionAfterCollision)
                {
                    if (targetAnchor != null) Destroy(targetAnchor);

                    targetAnchor = new GameObject();
                    targetAnchor.transform.parent = collision.transform;
                    targetAnchor.transform.position = collisionPoint;
                    targetAnchor.transform.rotation = transform.rotation;
                    _target = collision.gameObject;
                }
                HitAfterEvents.Invoke((targetAnchor.transform.position - transform.position));
                if (rigid != null) Destroy(rigid);
                if (collid != null) Destroy(collid);
                isCollided = true;
            }

            foreach (var obj in DeactivateObjectsAfterCollision)
            {
                if (obj != null)
                {
                    var ps = obj.GetComponent<ParticleSystem>();
                    if (ps != null) ps.Stop();
                    else obj.SetActive(false);
                }
            }


            if (rigid != null) Destroy(rigid);
            if (_arrowModelRigid != null) Destroy(_arrowModelRigid);
            if (collid != null) Destroy(collid);

            transform.parent.gameObject.DestroyTimer(CollisionEffectDestroyAfter);
        }

        private void FixedUpdate()
        {
            if (!isInitializedForce) InitializeForce();
            if (UseTargetPositionAfterCollision && isCollided && targetAnchor != null)
            {
                transform.position = targetAnchor.transform.position;
                transform.rotation = targetAnchor.transform.rotation;
            }
            if (isCollided)//투사체 충돌 후
            {
                CheckTargetDestroy();
            }

        }

        // 히트 후 화살 모델 위치 이동
        private void SetHitAfterArrowModel(Vector3 rot)
        {
            SoundManager.Instance.Play(_arrowHitSoundID);
            ArrowModel.SetParent(transform);
            ArrowModel.localPosition = HitAfterOffset;
        }

        private void CheckTargetDestroy()
        {
            if (_target == null)//타겟이 사라진 경우
            {
                Destroy(transform.parent.gameObject);
            }
        }

        public class AE_CollisionInfo : EventArgs
        {
            public ContactPoint ContactPoint;
        }

        //private void Update()
        //{
        //    var kinetic = rigid.mass* Mathf.Pow(rigid.velocity.magnitude, 2) * 0.5f;
        //    Debug.Log(transform.localPosition.magnitude + "   time" + (Time.time - startTime) + "  speed" + (transform.localPosition.magnitude/ (Time.time - startTime)));
        //}


        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
                return;

            var t = transform;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(t.position, ColliderRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(t.position, t.position + t.forward * 100);
        }
    }
}