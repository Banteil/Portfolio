using UnityEngine;
using UnityEngine.EventSystems;

public class Btn_OpenCloseHistory : MonoBehaviour, IPointerClickHandler
{
    private const int ADDITION_X_POS = 910;

    private RectTransform historyUI;
    private Vector3 originPos;
    private Vector3 openedPos;

    private bool isOpen;
    public bool IsOpen
    {
        get
        {
            return isOpen;
        }
        set
        {
            isOpen = value;
            historyUI.anchoredPosition = isOpen ? openedPos : originPos;
        }
    }

    private void Awake()
    {
        historyUI = transform.parent.GetComponent<RectTransform>();
        originPos = historyUI.anchoredPosition;
        openedPos = historyUI.anchoredPosition + new Vector2(ADDITION_X_POS, 0);
        isOpen = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("CLICKED");
        IsOpen = !IsOpen;
    }
}