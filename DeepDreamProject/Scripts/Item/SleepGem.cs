using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepGem : MonoBehaviour
{
    public float GemPower = 1f;

    Collider2D _collider;
    MMFeedbacks _getFeedbacks;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _getFeedbacks = GetComponent<MMFeedbacks>();
        _getFeedbacks.Initialization(gameObject);
        Invoke("ActiveCollider", 0.5f);
    }

    void ActiveCollider() => _collider.enabled = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.gameObject.GetComponentInParent<Character>();
        if (character != null)
        {
            if (character.ControlType.Equals(CharacterControlType.AI)) return;
            PlayerData.Instance.SleepPower += GemPower;
            _getFeedbacks.PlayFeedbacks(transform.position);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Character character = other.gameObject.GetComponentInParent<Character>();
        if (character != null)
        {
            if (character.ControlType.Equals(CharacterControlType.AI)) return;
            PlayerData.Instance.SleepPower += GemPower;
            _getFeedbacks.PlayFeedbacks(transform.position);
            gameObject.SetActive(false);
        }
    }
}
