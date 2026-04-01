using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbySpace : MonoBehaviour
{
    [SerializeField]
    Transform content;

    private void Start()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform rT = content.GetChild(i).GetComponent<RectTransform>();
            rT.sizeDelta = new Vector2(1080f, rT.sizeDelta.y);
        }
    }
}
