using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomUI : MonoBehaviour
{
    [SerializeField]
    float selectSize = 400f;
    RectTransform rT;
    HorizontalLayoutGroup hLG;
    List<Toggle> taps = new List<Toggle>();
    public List<Toggle> Taps { get { return taps; } }

    void Awake()
    {
        rT = GetComponent<RectTransform>();
        hLG = GetComponent<HorizontalLayoutGroup>();
        for (int i = 0; i < transform.childCount; i++)
        {
            taps.Add(transform.GetChild(i).GetComponent<Toggle>());
        }
        TapSorting();
    }

    public void TapSorting()
    {
        float width = rT.rect.width - selectSize;
        for (int i = 0; i < taps.Count; i++)
        {
            if (taps[i].isOn)
            {
                taps[i].GetComponent<RectTransform>().sizeDelta = new Vector2(selectSize, rT.rect.height);
                LobbyManager.Instance.SelectMenu(i);
            }
            else
                taps[i].GetComponent<RectTransform>().sizeDelta = new Vector2(width / (taps.Count - 1), rT.rect.height);
        }
        hLG.childForceExpandWidth = false;
        hLG.childForceExpandWidth = true;
    }
}
