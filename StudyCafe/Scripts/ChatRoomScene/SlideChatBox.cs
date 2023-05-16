using UnityEngine;
using UnityEngine.UI;

public class SlideChatBox : MonoBehaviour
{
    public RectTransform boxRect, msgRect;
    public Text msg, userName;
    float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 5f)
        {
            Destroy(gameObject);
        }
    }
}
