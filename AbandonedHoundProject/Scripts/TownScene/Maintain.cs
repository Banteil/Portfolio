using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Maintain : Singleton<Maintain>
{
    public Transform gemContent;
    public Transform redGemSlot, blueGemSlot, greenGemSlot, yellowGemSlot;
    public Text statusText;

    void Start()
    {
        ResetMaintainInfo();
    }

    public void ResetMaintainInfo()
    {
        for(int i = 0; i < gemContent.childCount; i++)
        {
            Destroy(gemContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < PlayerState.Instance.getGemList.Count; i++)
        {
            Gem gem = new Gem();
            gem = PlayerState.Instance.getGemList[i];

            GameObject gemSlot = Instantiate(Resources.Load("GemSlot") as GameObject);
            gemSlot.AddComponent<MaintainSelectGem>();
            gemSlot.GetComponent<MaintainSelectGem>().gem = gem;
            gemSlot.transform.SetParent(gemContent);
            gemSlot.transform.localScale = Vector3.one;
        }

        if (PlayerState.Instance.redGem != null && redGemSlot.childCount.Equals(0))
        {
            Gem insertGem = new Gem();
            insertGem = PlayerState.Instance.redGem;

            GameObject gemSlot = Instantiate(Resources.Load("GemSlot") as GameObject);
            gemSlot.AddComponent<MaintainSelectGem>();
            gemSlot.GetComponent<MaintainSelectGem>().gem = insertGem;
            gemSlot.transform.SetParent(redGemSlot);
            gemSlot.transform.localScale = Vector3.one;
        }

        if (PlayerState.Instance.blueGem != null && blueGemSlot.childCount.Equals(0))
        {
            Gem insertGem = new Gem();
            insertGem = PlayerState.Instance.blueGem;

            GameObject gemSlot = Instantiate(Resources.Load("GemSlot") as GameObject);
            gemSlot.AddComponent<MaintainSelectGem>();
            gemSlot.GetComponent<MaintainSelectGem>().gem = insertGem;
            gemSlot.transform.SetParent(blueGemSlot);
            gemSlot.transform.localScale = Vector3.one;
        }

        if (PlayerState.Instance.greenGem != null && greenGemSlot.childCount.Equals(0))
        {
            Gem insertGem = new Gem();
            insertGem = PlayerState.Instance.greenGem;

            GameObject gemSlot = Instantiate(Resources.Load("GemSlot") as GameObject);
            gemSlot.AddComponent<MaintainSelectGem>();
            gemSlot.GetComponent<MaintainSelectGem>().gem = insertGem;
            gemSlot.transform.SetParent(greenGemSlot);
            gemSlot.transform.localScale = Vector3.one;
        }

        if (PlayerState.Instance.yellowGem != null && yellowGemSlot.childCount.Equals(0))
        {
            Gem insertGem = new Gem();
            insertGem = PlayerState.Instance.yellowGem;

            GameObject gemSlot = Instantiate(Resources.Load("GemSlot") as GameObject);
            gemSlot.AddComponent<MaintainSelectGem>();
            gemSlot.GetComponent<MaintainSelectGem>().gem = insertGem;
            gemSlot.transform.SetParent(yellowGemSlot);
            gemSlot.transform.localScale = Vector3.one;
        }

        statusText.text = PlayerState.Instance.GetPlayerStatus();
    }

}
