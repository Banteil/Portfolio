using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogInfo : MonoBehaviour
{
    Text logText;
    Coroutine directRoutine;
    void Start()
    {
        logText = GetComponent<Text>();
    }

    public void DisplayLogInfo(string info)
    {
        if (directRoutine != null) StopCoroutine(directRoutine);
        directRoutine = StartCoroutine(Directing(info));
    }

    IEnumerator Directing(string info)
    {
        logText.text = info;
        logText.color = new Color32(255, 255, 255, 255);        
        yield return new WaitForSeconds(2f);

        byte alpha = 255;
        while(alpha > 0)
        {
            alpha--;
            logText.color = new Color32(255, 255, 255, alpha);            
            yield return null;
        }

        logText.text = "";
        directRoutine = null;
    }
}
