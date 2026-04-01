using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightAct : MonoBehaviour
{
    InteractionObject interactionObject;
    CircleCollider2D sightCollider;
    public CircleCollider2D SightCollider { get { return sightCollider; } }

    private void Awake()
    {
        interactionObject = GetComponentInParent<InteractionObject>();
        sightCollider = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        sightCollider.radius = interactionObject.Sight;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        interactionObject.ObjectDetection(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        interactionObject.ObjectDetecting(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        interactionObject.OutOfDetection(collision.gameObject);
    }
}
