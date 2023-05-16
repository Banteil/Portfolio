using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCSwitchText : MonoBehaviour
{
    public Text switchText;
    public NPCAct npc;
    public int index;

    public void MouseEnter()
    {
        switchText.color = Color.red;
    }

    public void MouseExit()
    {
        switchText.color = Color.black;
    }

    public void MousePointerDown()
    {
        switchText.color = Color.gray;
    }

    public void MousePointerUp()
    {
        Debug.Log("선택지 선택");
        switchText.color = Color.black;
        npc.CheckChoice(index);
    }
}
