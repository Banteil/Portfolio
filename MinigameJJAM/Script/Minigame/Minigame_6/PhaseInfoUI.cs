using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace starinc.io
{
    public class PhaseInfoUI : MonoBehaviour
    {
        private List<PhaseInfo> _phaseInfos;

        private void Awake()
        {
            _phaseInfos = GetComponentsInChildren<PhaseInfo>().ToList();
        }

        public void SetInfos(List<DrinkData> drinkDatas)
        {
            for (int i = 0; i < _phaseInfos.Count; i++)
            {
                var compareCount = i < drinkDatas.Count;
                _phaseInfos[i].gameObject.SetActive(compareCount);
                if (compareCount)
                {
                    _phaseInfos[i].SetInfo(drinkDatas[i].PhaseUISprite, drinkDatas[i].RequiredIceCount);
                }
            }
        }
    }
}
