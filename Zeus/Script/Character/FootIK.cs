using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zeus
{
    public class FootIK : MonoBehaviour
    {
        private Animator anim;
        [Header("Feet IK")]

        public LayerMask GroundLayer;

        [SerializeField] private float heightFromGroundRaycast = 0.7f;
        [Range(0, 2f)] [SerializeField] private float raycastDownDistance = 1.5f;
        [SerializeField] private float pelvisOffset = 0f;

        [Range(0, 1f)] [SerializeField] private float pelvisUpDownSpeed = 0.25f;
        [Range(0, 1f)] [SerializeField] private float feetToIKPositionSpeed = 0.25f;

        public string leftFootAnim = "LeftFootCurve";
        public string rightFootAnim = "RightFootCurve";

        private Vector3 leftFootPosition, leftFootIKPosition, rightFootPosition, rightFootIKPosition;
        private Quaternion leftFootIKRotation, rightFootIKRotation;
        private float lastPelvisPositionY, lastLeftFootPosition, lastRightFootPosition;

        public bool showDebug;

        // Start is called before the first frame update
        void Start()
        {
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (GameTimeManager.Instance.WorldTimeScale.CompaerEpsilon(0.000001f, 0.0001f))
                return;

            AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
            AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

            //Raycast to Ground
            FeetPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation);
            FeetPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation);

        }

        private void OnAnimatorIK(int layerIndex)
        {

            MovePelvisHeight();

            //Left Foot IK Position and Rotation
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnim));
            MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPosition);

            //Right Foot IK Position and Rotation
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnim));
            MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPosition);
        }

        void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationHolder, ref float lastFootPositionY)
        {
            Vector3 targetIKPosition = anim.GetIKPosition(foot);

            if (positionIKHolder != Vector3.zero)
            {
                targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
                positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

                float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed);
                lastFootPositionY = yVariable;
                targetIKPosition.y += yVariable;
                targetIKPosition = transform.TransformPoint(targetIKPosition);
            }

            anim.SetIKRotation(foot, rotationHolder);
            anim.SetIKPosition(foot, targetIKPosition);
        }

        private void MovePelvisHeight()
        {
            if (rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
            {
                lastPelvisPositionY = anim.bodyPosition.y;
                return;
            }

            float leftOffsetPosition = leftFootIKPosition.y - transform.position.y;
            float rightOffsetPosition = rightFootIKPosition.y - transform.position.y;
            float totalOffset = leftOffsetPosition < rightOffsetPosition ? leftOffsetPosition : rightOffsetPosition;

            Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset;
            newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpDownSpeed);
            anim.bodyPosition = newPelvisPosition;

            lastPelvisPositionY = anim.bodyPosition.y;
        }

        //Raycast handler to find feet position on ground
        private void FeetPositionSolver(Vector3 fromRaycastPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations)
        {
            RaycastHit feetHit;

            if (showDebug)
                Debug.DrawLine(fromRaycastPosition, fromRaycastPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.green);

            if (ThrowRayOnDirection(fromRaycastPosition, Vector3.down, raycastDownDistance + heightFromGroundRaycast, out feetHit))
            {
                feetIKPositions = fromRaycastPosition;
                feetIKPositions.y = feetHit.point.y + pelvisOffset;
                feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetHit.normal) * transform.rotation;
                return;
            }

            feetIKPositions = Vector3.zero;
        }

        private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
        {
            feetPositions = anim.GetBoneTransform(foot).position;
            feetPositions.y = transform.position.y + heightFromGroundRaycast;
        }

        public bool ThrowRayOnDirection(Vector3 origin, Vector3 direction, float length, out RaycastHit hit)
        {
            if (showDebug)
            {
                Debug.DrawLine(origin, origin + direction * length, Color.green);
            }

            return Physics.Raycast(origin, direction, out hit, length, GroundLayer);
        }
    }
}

