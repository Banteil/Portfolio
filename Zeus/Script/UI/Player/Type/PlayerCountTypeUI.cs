using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Zeus
{
    public class PlayerCountTypeUI : PlayerUIType
    {
        public List<Image> Symbol;
        private int _currentCount;

        public void SetValue(int value)
        {
            _currentCount = value;

            if (_currentCount <= 0)
            {
                Debug.Log("사용할 수 없는 상태입니다");
            }

            for (int i = 0; i < Symbol.Count; i++)
            {
                Symbol[i].gameObject.SetActive(_currentCount > i ? true : false);
            }
        }
    }
}