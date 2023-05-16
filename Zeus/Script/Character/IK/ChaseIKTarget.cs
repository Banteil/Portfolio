using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zeus
{
    public class ChaseIKTarget : MonoBehaviour
    {
        public Transform target;
        void FixedUpdate()
        {
            if (target != null)
            {
                transform.position = target.transform.position;
            }
        }
    }
}