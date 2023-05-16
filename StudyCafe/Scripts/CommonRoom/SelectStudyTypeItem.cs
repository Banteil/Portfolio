using UnityEngine;

public class SelectStudyTypeItem : MonoBehaviour
{
    public int index;
    public NPCAct npcAct;
    public GameObject selectCanvas, uiCanvas;

    public void SettingOpenStudyButton()
    {
        npcAct.NPCTypeInfo = (NPCAct.NPCType)index;
        selectCanvas.SetActive(false);
        uiCanvas.SetActive(true);
    }
}
