using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Zeus
{
    public class RigController : MonoBehaviour
    {
        [Header("Character")]
        public ThirdPersonController Character;

        [Header("LeftHand")]
        public TwoBoneIKConstraint LeftHandIK;
        public ChaseIKTarget LeftHandTarget;

        [Header("RightHand")]
        public TwoBoneIKConstraint RightHandIK;
        public ChaseIKTarget RightHandTarget;

        [Header("Other")]
        public float WeightSpeed = 2.5f;

        private Rig _rig;

        private bool _ikCondition = true;

        public bool IKCondition
        {
            get
            {
                return _ikCondition && Character._useHandIK && !Character._ignoreHandIK;
            }

            set
            {
                _ikCondition = value;
            }
        }

        private void Start()
        {
            //_rig = GetComponent<Rig>();
            //DeActiveIK();
        }

        public void ActiveIK(string hand, Transform target)
        {
            //_rig.weight = 1;
            //if (hand.Equals("LeftLowerArm"))
            //{
            //    if (LeftHandIK == null) { return; }
            //    LeftHandIK.weight = 1;
            //    LeftHandTarget.target = target;
            //}
            //else if (hand.Equals("RightLowerArm"))
            //{
            //    RightHandIK.weight = 1;
            //    RightHandTarget.target = target;
            //}
        }

        public void DeActiveIK()
        {
            //_rig.weight = 0;
            //LeftHandIK.weight = 0;
            //LeftHandTarget.target = null;
            //RightHandIK.weight = 0;
            //RightHandTarget.target = null;
        }
    }
}
