using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class CardHistoryData : MonoBehaviour
    {
        public Define.CardType Card { get; private set; }
        public int CardOrderAtRound { get; private set; }
        public int Round { get; private set; }
    }
}