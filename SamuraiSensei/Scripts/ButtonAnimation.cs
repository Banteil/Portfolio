using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Animation anim;

    void Awake()
    {
        anim = GetComponent<Animation>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        anim.Play("ButtonMouseEnter");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        anim.Play("ButtonMouseExit");
    }
}
