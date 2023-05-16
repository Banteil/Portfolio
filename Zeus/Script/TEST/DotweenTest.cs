using DG.Tweening;
using UnityEngine;
using Zeus;
using System.Collections;

public class DotweenTest : MonoBehaviour
{
    public Transform start;
    public Transform end;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
        TweenMove();
    }

    private void TweenMove()
    {
        var nextPostion = transform.position.CompaerEpsilon(start.position, 0.1f) == true ? end.position : start.position;
        var tween = transform.DOMove(nextPostion, 1f).SetEase(Ease.Linear);
        tween.Pause();
    
        tween.timeScale = DOTween.unscaledTimeScale / DOTween.timeScale;
        //tween.DOTimeScale(1f / DOTween.timeScale, 0f);
        tween.Play();
        tween.onComplete = () =>
        {
            TweenMove();
        };
    }
}
