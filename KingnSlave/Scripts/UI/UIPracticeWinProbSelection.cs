using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIPracticeWinProbSelection : MonoBehaviour
    {
        private List<Button> buttonList = new List<Button>();
        private int[] percentValues;

        private enum Percent
        {
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Ten = 10,
            Twenty = 20,
            Thirty = 30,
            Fourty = 40,
            Fifty = 50
        }

        private void Awake()
        {
            if (GameManager.Instance.CurrentGameMode != Define.GamePlayMode.Practice)
                return;

            percentValues = (int[])Enum.GetValues(typeof(Percent));

            for (int i = 0; i < transform.childCount; i++)
            {
                if (percentValues.Length <= i)
                    break;

                var childCom = transform.GetChild(i).GetComponent<Button>();
                if (childCom == null)
                    continue;

                buttonList.Add(childCom);

                childCom.onClick.AddListener(delegate { OnProbabilitySelected(childCom.transform.GetSiblingIndex()); });
                childCom.transform.GetChild(0).GetComponent<TMP_Text>().text = $"{percentValues[childCom.transform.GetSiblingIndex()]}%";

                childCom.image.color = (Mathf.Abs(GameManager.Instance.PracticeGameWinProbPercent - (float)percentValues[i]) < float.Epsilon) ? Color.gray : Color.white;
                //Debug.Log(childCom.transform.GetSiblingIndex() + "?: " + (int)percentValues.GetValue(childCom.transform.GetSiblingIndex()));
            }
        }

        public void OnProbabilitySelected(int index)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            foreach (var button in buttonList)
                button.image.color = Color.white;

            Debug.Log("index? " + index);
            buttonList[index].image.color = Color.gray;
            GameManager.Instance.PracticeGameWinProbPercent = (float)percentValues[index];
        }

        private void OnDestroy()
        {
            foreach (var button in buttonList)
                button.onClick.RemoveAllListeners();
        }
    }
}