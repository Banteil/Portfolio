using UnityEngine;

namespace starinc.io.gallaryx
{
    public static class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void GameInitialize()
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}
