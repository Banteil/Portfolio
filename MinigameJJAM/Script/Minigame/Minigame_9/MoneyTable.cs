using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "MoneyTable", menuName = "Scriptable Objects/Minigame/MoneyTable")]
    public class MoneyTable : ScriptableObject
    {
        [SerializeField]
        private List<MoneyData> _moneyDatas;

        public MoneyData GetMoneyData(int count)
        {
            foreach(var data in  _moneyDatas)
            {
                if (data.IsMatchTheCount(count)) return data;
            }
            return null;
        }
    }
}
