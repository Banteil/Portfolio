using UnityEngine;

namespace starinc.io.kingnslave
{
    public static class CoroutineUtilities
    {
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    }
}