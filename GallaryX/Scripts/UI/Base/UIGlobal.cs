using UnityEngine;

namespace starinc.io.gallaryx
{
    public class UIGlobal : UIBase
    {
        protected const int GLOBAL_ORDER = 1000;

        protected override void OnAwake()
        {
            Util.DontDestroyObject(gameObject);
            GetComponent<Canvas>().sortingOrder = GLOBAL_ORDER;
        }


    }
}
