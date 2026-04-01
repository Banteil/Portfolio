using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectEffect : MonoBehaviour
{
    Transform lu, ru, ld, rd;
    Rect selectObjectRect;
    public Rect SelectObjectRect { set { selectObjectRect = value; } }   

    void Awake()
    {
        lu = transform.GetChild(0);
        ru = transform.GetChild(1);
        ld = transform.GetChild(2);
        rd = transform.GetChild(3);
    }

    void SettingEffectPos(bool isUI)
    {
        if (!isUI)
        {
            lu.localPosition = new Vector2((selectObjectRect.width / 32f) * -1f, selectObjectRect.height / 32f);
            ru.localPosition = new Vector2((selectObjectRect.width / 32f), selectObjectRect.height / 32f);
            ld.localPosition = new Vector2((selectObjectRect.width / 32f) * -1f, selectObjectRect.height / 32f * -1f);
            rd.localPosition = new Vector2((selectObjectRect.width / 32f), selectObjectRect.height / 32f * -1f);
        }
        else
        {
            lu.localPosition = new Vector2((selectObjectRect.width / 2f) * -1f, selectObjectRect.height / 2f);
            ru.localPosition = new Vector2((selectObjectRect.width / 2f), selectObjectRect.height / 2f);
            ld.localPosition = new Vector2((selectObjectRect.width / 2f) * -1f, selectObjectRect.height / 2f * -1f);
            rd.localPosition = new Vector2((selectObjectRect.width / 2f), selectObjectRect.height / 2f * -1f);
        }
    }

    public void EffectOn()
    {
        Vector2 pos = transform.localPosition;
        pos.y = -2 + (selectObjectRect.height / 32f);
        transform.localPosition = pos;

        SettingEffectPos(false);
        lu.gameObject.SetActive(true);
        ru.gameObject.SetActive(true);
        ld.gameObject.SetActive(true);
        rd.gameObject.SetActive(true);
    }

    public void EffectOn(Vector3 pos)
    {
        transform.position = pos;
        SettingEffectPos(true);
        lu.gameObject.SetActive(true);
        ru.gameObject.SetActive(true);
        ld.gameObject.SetActive(true);
        rd.gameObject.SetActive(true);
    }

    public void EffectOff()
    {
        lu.gameObject.SetActive(false);
        ru.gameObject.SetActive(false);
        ld.gameObject.SetActive(false);
        rd.gameObject.SetActive(false);
    }
}
