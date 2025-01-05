using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class SingleStoryGameBackground : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Image>().sprite = ResourceManager.Instance.GetStageInfoData(GameManager.Instance.ChallengingStageInCycle).InGameBackground;
        }
    }
}