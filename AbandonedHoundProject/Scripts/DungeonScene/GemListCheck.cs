using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GemListCheck : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Coroutine touchCheck;
    public List<GemOption> gemOptionList = new List<GemOption>();

    public void OnPointerDown(PointerEventData eventData)
    {
        touchCheck = StartCoroutine(Touching());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopCoroutine(touchCheck);
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    IEnumerator Touching()
    {
        float timer = 0f;

        while(timer < 0.5f)
        {
            if(transform.localScale.x > 0.95f) 
            transform.localScale -= new Vector3(0.01f, 0.01f, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        GameObject gemOptionPanel = GameObject.Find("ResultAccountCanvas").transform.GetChild(2).gameObject;
        gemOptionPanel.SetActive(true);

        Transform content = gemOptionPanel.transform.GetChild(0).GetChild(0).GetChild(0).transform;

        for (int i = 0; i < gemOptionList.Count; i++)
        {
            GameObject gemOption = Instantiate(Resources.Load("GemOptionList") as GameObject);
            gemOption.transform.GetChild(0).GetComponent<Text>().text = gemOptionList[i].dialog;
            gemOption.transform.SetParent(content);
            yield return null;
        }

        RawImage fadeImage = GameObject.Find("ResultAccountCanvas").transform.GetChild(1).GetComponent<RawImage>();
        fadeImage.raycastTarget = true;
        EventTrigger.Entry interactionUI_Entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        interactionUI_Entry.callback.AddListener((data) => { CancleTouch(); });
        fadeImage.gameObject.GetComponent<EventTrigger>().triggers.Add(interactionUI_Entry);
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void CancleTouch()
    {
        GameObject gemOptionPanel = GameObject.Find("ResultAccountCanvas").transform.GetChild(2).gameObject;
        Transform content = gemOptionPanel.transform.GetChild(0).GetChild(0).GetChild(0).transform;
        for (int i = 0; i < gemOptionList.Count; i++) Destroy(content.GetChild(i).gameObject);
        gemOptionPanel.SetActive(false);
        RawImage fadeImage = GameObject.Find("ResultAccountCanvas").transform.GetChild(1).GetComponent<RawImage>();
        fadeImage.raycastTarget = false;
        fadeImage.gameObject.GetComponent<EventTrigger>().triggers.Clear();
    }
}
