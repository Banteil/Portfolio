using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class VerticalPauseMenuUI : VerticalMenuUI
    {
        private bool _isFocus = false;
        public bool IsFocus
        {
            get => _isFocus;
            set { _isFocus = value;  EnableUI(_isFocus); }
        }
        protected override void MoveMenuFocus(Vector2 dir)
        {
            if (!IsFocus) { return; }
            base.MoveMenuFocus(dir);
        }

        public List<Menu> GetUIList()
        {
            return _menuList;
        }
    }
}