using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "FoodDataTable", menuName = "Scriptable Objects/Minigame/FoodDataTable")]
    public class FoodDataTable : ScriptableObject
    {
        [SerializeField]
        private List<FoodData> _foodDatas;

        public FoodData GetFoodData(string foodName)
        {
            return _foodDatas.FirstOrDefault(food => food.Name == foodName);
        }

        public FoodData GetRandomFoodData()
        {
            var index = Random.Range(0, _foodDatas.Count);
            return _foodDatas[index];
        }
    }
}
