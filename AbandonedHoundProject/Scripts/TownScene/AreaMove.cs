using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AreaMove : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject arrowPanel;
    Vector2 touchBeganPositon;
    Vector2 touchEndPosition;
 
    public void OnPointerDown(PointerEventData eventData)
    {
        if (TownManager.Instance.openUI) return;

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            touchBeganPositon = Input.mousePosition;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            touchBeganPositon = Input.GetTouch(0).position;
        }

        if (TownManager.Instance.location.Equals(0))
        {
            arrowPanel.transform.GetChild(0).gameObject.SetActive(false);
            arrowPanel.transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (TownManager.Instance.location.Equals(4))
        {
            arrowPanel.transform.GetChild(0).gameObject.SetActive(true);
            arrowPanel.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            arrowPanel.transform.GetChild(0).gameObject.SetActive(true);
            arrowPanel.transform.GetChild(1).gameObject.SetActive(true);
        }

        arrowPanel.SetActive(true);
        arrowPanel.transform.localPosition = touchBeganPositon - new Vector2(Screen.width / 2, Screen.height / 2);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (TownManager.Instance.openUI) return;

        arrowPanel.SetActive(false);

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            touchEndPosition = Input.mousePosition;
            if (Vector2.Distance(touchBeganPositon, touchEndPosition) < 400f) return;

            Vector2 swipeDirection = (touchEndPosition - touchBeganPositon).normalized;

            if (swipeDirection.y >= 0.5f || swipeDirection.y <= -0.5f) return;

            if (swipeDirection.x < 0) TownManager.Instance.MoveLeft(); 
            else if (swipeDirection.x > 0) TownManager.Instance.MoveRight();
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            touchEndPosition = Input.GetTouch(0).position;
            if (Vector2.Distance(touchBeganPositon, touchEndPosition) < 400f) return;

            Vector2 swipeDirection = (touchEndPosition - touchBeganPositon).normalized;

            if (swipeDirection.y >= 0.5f || swipeDirection.y <= -0.5f) return;

            if (swipeDirection.x < 0) TownManager.Instance.MoveLeft(); 
            else if (swipeDirection.x > 0) TownManager.Instance.MoveRight();
        }        
    }
}
