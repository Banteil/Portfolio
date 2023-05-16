using UnityEngine;
using UnityEngine.UI;

public class KeywordItem : MonoBehaviour
{
    public Text keyword;
    public float max;
    public KeywordPanel panel = null;

    void Update()
    {
        float speed = 150f * Time.deltaTime;
        GetComponent<RectTransform>().localPosition -= new Vector3(0f, speed, 0f);
        if (GetComponent<RectTransform>().localPosition.y < max) 
            DestroyItem();
    }

    public void DestroyItem()
    {
        Destroy(gameObject);
        if (panel == null)
            MiniGameManager.Instance.keywordDestroy -= DestroyItem;
        else
            panel.keywordDestroy -= DestroyItem;
    }
}
