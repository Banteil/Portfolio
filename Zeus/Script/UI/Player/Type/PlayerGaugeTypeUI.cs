using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Zeus
{
    public class PlayerGaugeTypeUI : PlayerUIType
    {
        public TextMeshProUGUI NameText;
        public List<GaugeUI> GaugeList;
        public int GaugeCount;

        protected override void Start()
        {
            base.Start();
            Init();
        }

        private void Init()
        {
            if(GaugeCount == 0) { return; }
            //for(int i = 0; i < TestGaugeCount; i++)
            //{
            //    var gauge = Instantiate(GaugePrefab, GaugeParent);
            //    GaugeList.Add(gauge);
            //}
        }

        public void SetValue(float max, float current)
        {
            if (max < current)
            {
                Debug.Log("Current값이 MAx보다 높습니다");
                current = max;
            }

            //게이지 하나당 값
            var gaugeValue = max / GaugeList.Count;

            //가득찬 게이지 개수
            var fullGauge = Mathf.FloorToInt(current / gaugeValue);
            var result = (current - (gaugeValue * fullGauge)) / gaugeValue;
            for (int i =0; i < GaugeList.Count; i++)
            {
                if (i < fullGauge)
                {
                    GaugeList[i].Value = 1;
                }
                else if(i == fullGauge)
                {
                    GaugeList[fullGauge].Value = result;
                }
                else if (i > fullGauge)
                {
                    GaugeList[i].Value = 0;
                }
            }
        }

        public void SetGaugeName(string name)
        {
            NameText.text = name;
        }
    }
}
