using UnityEngine;

namespace starinc.io
{
    public class UIDestroyEvent : MonoBehaviour
    {
        private void OnDestroy()
        {
            Util.UnfocusUI();
        }
    }
}
