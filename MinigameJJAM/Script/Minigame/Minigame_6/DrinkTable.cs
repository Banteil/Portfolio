using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(fileName = "DrinkTable", menuName = "Scriptable Objects/Minigame/DrinkTable")]
    public class DrinkTable : ScriptableObject
    {
        [SerializeField]
        private List<DrinkData> _drinkDatas;
        [SerializeField]
        private Drink _drinkPrefab;

        public GameObject CreateDrink(int phase, Action failAction, Vector3 position)
        {
            var validDrinks = _drinkDatas.Where(data => data.Phase <= phase).ToList();
            if (validDrinks.Count == 0)
            {
                Debug.LogWarning("No drink data matches the specified phase.");
                return null;
            }
            var selectedData = validDrinks[UnityEngine.Random.Range(0, validDrinks.Count)];
            var drinkObj = Instantiate(_drinkPrefab.gameObject, position, Quaternion.identity);

            var drink = drinkObj.GetComponent<Drink>();
            drink.InitializeWithData(selectedData);
            drink.OnFailure += failAction;

            return drinkObj;
        }

        public List<DrinkData> GetPhaseDrinks(int phase)
        {
            var validDrinks = _drinkDatas.Where(data => data.Phase == phase).ToList();
            return validDrinks;
        }
    }
}
