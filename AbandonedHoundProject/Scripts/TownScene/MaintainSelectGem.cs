using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaintainSelectGem : SelectGem, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform maintainCanvas;
    Coroutine touchCheck = null;
    int sibling;

    void Start()
    {
        maintainCanvas = GameObject.Find("MaintainCanvas").transform;
        transform.GetChild(0).GetComponent<Image>().sprite = gem.gemSprite;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        touchCheck = StartCoroutine(Touching());
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        StopCoroutine(touchCheck);
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        StopCoroutine(touchCheck);
        transform.localScale = new Vector3(1f, 1f, 1f);

        selectedGem = gameObject;
        parentSlot = transform.parent;
        sibling = transform.GetSiblingIndex();
        RectTransform rect = GetComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        Image image = GetComponent<Image>();
        image.raycastTarget = false;
        transform.SetParent(maintainCanvas);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            Vector3 touchPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            transform.position = touchPosition;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            Vector3 touchPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 0);
            transform.position = touchPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        selectedGem = null;
        if (transform.parent.Equals(maintainCanvas))
        {
            transform.SetParent(parentSlot);
            transform.SetSiblingIndex(sibling);
            parentSlot = null;
        }
        Image image = GetComponent<Image>();
        image.raycastTarget = true;
    }

    public IEnumerator Touching()
    {
        float timer = 0f;

        while (timer < 0.5f)
        {
            if (transform.localScale.x > 0.95f)
                transform.localScale -= new Vector3(0.01f, 0.01f, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        GameObject gemOptionPanel = GameObject.Find("MaintainCanvas").transform.GetChild(2).gameObject;
        gemOptionPanel.SetActive(true);

        Text GemTypeText = gemOptionPanel.transform.GetChild(0).GetComponent<Text>();
        GemTypeText.text = gem.gemKindName;
        Text GemValueText = gemOptionPanel.transform.GetChild(1).GetComponent<Text>();

        switch (gem.GemKind)
        {
            case GemKind.RED:
                GemValueText.text = "Str + " + gem.statValue;
                break;
            case GemKind.BLUE:
                GemValueText.text = "Int + " + gem.statValue;
                break;
            case GemKind.GREEN:
                GemValueText.text = "Dex + " + gem.statValue;
                break;
            case GemKind.YELLOW:
                GemValueText.text = "Vit + " + gem.statValue;
                break;
        }

        Transform content = gemOptionPanel.transform.GetChild(3).GetChild(0).GetChild(0).transform;

        for (int i = 0; i < gem.gemOptionList.Count; i++)
        {
            GameObject gemOption = Instantiate(Resources.Load("GemOptionList") as GameObject);
            gemOption.transform.GetChild(0).GetComponent<Text>().text = gem.gemOptionList[i].dialog;
            gemOption.transform.SetParent(content);
            yield return null;
        }

        RawImage cancleImage = gemOptionPanel.transform.GetChild(2).GetComponent<RawImage>();
        cancleImage.raycastTarget = true;
        EventTrigger.Entry interaction_Entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        interaction_Entry.callback.AddListener((data) => { CancleTouch(); });
        cancleImage.gameObject.GetComponent<EventTrigger>().triggers.Add(interaction_Entry);
        gemOptionPanel.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public override void CancleTouch()
    {
        GameObject gemOptionPanel = GameObject.Find("MaintainCanvas").transform.GetChild(2).gameObject;
        Transform content = gemOptionPanel.transform.GetChild(3).GetChild(0).GetChild(0).transform;
        for (int i = 0; i < gem.gemOptionList.Count; i++) Destroy(content.GetChild(i).gameObject);
        RawImage cancleImage = gemOptionPanel.transform.GetChild(2).GetComponent<RawImage>();
        cancleImage.raycastTarget = false;
        cancleImage.gameObject.GetComponent<EventTrigger>().triggers.Clear();
        gemOptionPanel.SetActive(false);
    }
}
