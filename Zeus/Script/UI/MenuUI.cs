using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zeus
{
    public class MenuUI : UIBase
    {
        protected List<Menu> _menuList;
        protected int _currentMenuIndex;

        private void Awake()
        {
            _menuList = GetComponentsInChildren<Menu>().ToList();
            _currentMenuIndex = 0;
        }
 
        protected virtual void MoveMenuFocus(Vector2 dir) { }        
    }
}
