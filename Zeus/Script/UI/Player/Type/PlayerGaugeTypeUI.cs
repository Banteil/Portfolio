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
                Debug.Log("Current���� MAx���� �����ϴ�");
                current = max;
            }

            //������ �ϳ��� ��
            var gaugeValue = max / GaugeList.Count;

            //������ ������ ����
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
