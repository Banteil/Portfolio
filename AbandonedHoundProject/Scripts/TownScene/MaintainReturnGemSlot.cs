using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaintainReturnGemSlot : ReturnGemSlot
{
    void Start()
    {
        content = GameObject.Find("GemContent").transform;
    }

    public override void OnDrop(PointerEventData eventData)
    {
        if (SelectGem.parentSlot == null || SelectGem.selectedGem == null) return;

        if (SelectGem.parentSlot.Equals(transform.GetChild(0).GetChild(0).transform)) return;

        Gem insertGem = new Gem();
        insertGem = SelectGem.selectedGem.GetComponent<SelectGem>().gem;

        switch (insertGem.GemKind)
        {
            case GemKind.RED:
                PlayerState.Instance.redGem = null;
                break;
            case GemKind.BLUE:
                PlayerState.Instance.blueGem = null;
                break;
            case GemKind.GREEN:
                PlayerState.Instance.greenGem = null;
                break;
            case GemKind.YELLOW:
                PlayerState.Instance.yellowGem = null;
                break;
        }

        PlayerState.Instance.getGemList.Add(insertGem);

        Text stateText = GameObject.Find("StateText").GetComponent<Text>();
        stateText.text = PlayerState.Instance.GetPlayerStatus();

        SelectGem.selectedGem.transform.SetParent(content);
    }
}
