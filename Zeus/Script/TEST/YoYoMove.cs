using DG.Tweening;
using UnityEngine;

public class YoYoMove : MonoBehaviour
{
    void Start()
    {
        var startPos = transform.position;
        transform.DOMoveY(startPos.y + 30f, 2).SetLoops(-1, LoopType.Yoyo);
    }
}
