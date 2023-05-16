using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GemInsertCheck : MonoBehaviour, IDropHandler
{
    Transform content;
    public Gem gem = new Gem();
    public GemKind slotType;

    void Start()
    {
        content = GameObject.Find("GemContent").transform;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (SelectGem.parentSlot == null || SelectGem.selectedGem == null) return;

        Gem insertGem = new Gem();
        insertGem = SelectGem.selectedGem.GetComponent<MaintainSelectGem>().gem;
        if (transform.childCount == 0 && slotType.Equals(insertGem.GemKind))
        {
            switch (insertGem.GemKind)
            {
                case GemKind.RED:
                    PlayerState.Instance.redGem = insertGem;
                    break;
                case GemKind.BLUE:
                    PlayerState.Instance.blueGem = insertGem;
                    break;
                case GemKind.GREEN:
                    PlayerState.Instance.greenGem = insertGem;
                    break;
                case GemKind.YELLOW:
                    PlayerState.Instance.yellowGem = insertGem;
                    break;
            }

            for(int i = 0; i < PlayerState.Instance.getGemList.Count; i++)
            {
                if(PlayerState.Instance.getGemList[i].Equals(insertGem))
                    PlayerState.Instance.getGemList.RemoveAt(i);
            }

            SelectGem.selectedGem.transform.SetParent(transform);

            Text stateText = GameObject.Find("StateText").GetComponent<Text>();
            stateText.text = PlayerState.Instance.GetPlayerStatus();
        }
        else SelectGem.selectedGem.transform.SetParent(content);
    }
}
