using System.Collections;
using UnityEngine;

public class Grayscale : MonoBehaviour
{
    public SpriteRenderer[] sR;

    IEnumerator GrayscaleRoutine(float duration, bool isGrayscale)
    {
        float time = 0;
        while (duration > time)
        {
            float durationFrame = Time.deltaTime;
            float ratio = time / duration;
            float grayAmount = isGrayscale ? ratio : 1 - ratio;
            SetGrayscale(grayAmount);
            time += durationFrame;
            yield return null;
        }
        SetGrayscale(isGrayscale ? 1 : 0);
    }

    void SetGrayscale(float amount = 1)
    {
        for (int i = 0; i < sR.Length; i++)
        {
            sR[i].material.SetFloat("_GrayscaleAmount", amount);
        }
    }

    ///<summary>
    ///Sprite를 흑백으로 만듦(Grayscale Material 필요)
    ///</summary>
    public void StartGrayScale(float duration) => StartCoroutine(GrayscaleRoutine(duration, true));

    ///<summary>
    ///흑백으로 된 Sprite를 원래대로 되돌림(Grayscale Material 필요)
    ///</summary>
    public void ResetGrayScale(float duration) => StartCoroutine(GrayscaleRoutine(duration, false));
}
