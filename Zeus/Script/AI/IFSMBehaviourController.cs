using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public partial interface IFSMBehaviourController
    {
        MessageReceiver MessageReceiver
        {
            get;
        }
    }
}