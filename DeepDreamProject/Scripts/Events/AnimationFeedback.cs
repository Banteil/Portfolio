using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationFeedback : MonoBehaviour
{
    public List<MMFeedbacks> FeedbacksList;

    void Start()
    {
        for (int i = 0; i < FeedbacksList.Count; i++)
        {
            FeedbacksList[i].Initialization(gameObject);
        }
    }

    public void PlayFeedbacks()
    {
        for (int i = 0; i < FeedbacksList.Count; i++)
        {
            FeedbacksList[i].PlayFeedbacks(transform.position);
        }
    }
}
