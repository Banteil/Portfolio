using UnityEngine;

namespace Zeus
{
    public class VerticalMenuUI : MenuUI
    {
        private void Start()
        {
            InitIndex();
        }

        public void InitIndex()
        {
            _currentMenuIndex = 0;
            if(_menuList.Count <=0) { return; }
            _menuList[_currentMenuIndex].IsOn = true;
        }

        public override void OnNavigate(Vector2 value)
        {
            base.OnNavigate(value);
            MoveMenuFocus(value);
        }

        protected override void MoveMenuFocus(Vector2 dir)
        {
            if (_menuList.Count <= 0) { return; }

            if (dir.x > 0 || dir.y < 0)
            {
                _currentMenuIndex = _currentMenuIndex + 1 >= _menuList.Count ? 0 : _currentMenuIndex + 1;
                _menuList[_currentMenuIndex].IsOn = true;
            }
            else if (dir.x < 0 || dir.y > 0)
            {
                _currentMenuIndex = _currentMenuIndex - 1 < 0 ? _menuList.Count - 1 : _currentMenuIndex - 1;
                _menuList[_currentMenuIndex].IsOn = true;
            }
        }

        public override void OnSubmit()
        {
            if (!IsEnabled)
                return;

            if (_menuList.Count <= 0) { return; }

            _menuList[_currentMenuIndex].Confirm();
            base.OnSubmit();
        }
    }
}
