using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Zeus
{
    public class PlayerGoodsTypeUI : PlayerUIType
    {
        public TextMeshProUGUI Amount;
        
        public void SetAmount(int value)
        {
            Amount.text = value.ToString();
        }
    }
}

