using UnityEngine;

namespace starinc.io.gallaryx
{
    public class UISubmenu : UIBase
    {
        private CanvasGroup _group;

        protected override void OnAwake()
        {
            base.OnAwake();
            _group = gameObject.GetOrAddComponent<CanvasGroup>();
            IsEnable = false;
        }

        public bool IsEnable
        {
            get { return _group.alpha == 1; }
            set
            {
                if (value)
                {
                    _group.alpha = 1;
                    _group.blocksRaycasts = true;
                    _group.interactable = true;
                }
                else
                {
                    _group.alpha = 0;
                    _group.blocksRaycasts = false;
                    _group.interactable = false;
                }
            }
        }
    }
}
