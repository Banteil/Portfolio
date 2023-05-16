using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///<summary>
///페이드 인, 아웃 등 각종 이펙트 효과를 관리하는 매니저 클래스
///</summary>
public class EffectManager : Singleton<EffectManager>
{
    ///<summary>
    ///RawImage객체에 페이드 아웃 효과를 주는 함수
    ///</summary>
    public IEnumerator FadeOut(RawImage fadeImage, float fadeTime)
    {
        fadeImage.raycastTarget = true;
        Color fadecolor = fadeImage.color;
        float timer = 0f;

        while (fadecolor.a < 1f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(0, 1, timer);
            fadeImage.color = fadecolor;
            yield return null;
        }
    }

    ///<summary>
    ///RawImage객체에 페이드 인 효과를 주는 함수
    ///</summary>
    public IEnumerator FadeIn(RawImage fadeImage, float fadeTime)
    {
        Color fadecolor = fadeImage.color;
        float timer = 0f;

        while (fadecolor.a > 0f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(1, 0, timer);
            fadeImage.color = fadecolor;
            yield return null;
        }
        fadeImage.raycastTarget = false;
    }
    public IEnumerator FadeIn(SpriteRenderer fadesprite, float fadeTime)
    {
        Color fadecolor = fadesprite.color;
        float timer = 0f;

        while (fadecolor.a > 0f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(1, 0, timer);
            fadesprite.color = fadecolor;
            yield return null;
        }
    }

    ///<summary>
    ///Image객체에 페이드 아웃 효과를 주는 함수
    ///</summary>
    public IEnumerator FadeOut(Image fadeImage, float fadeTime)
    {
        fadeImage.raycastTarget = true;
        Color fadecolor = fadeImage.color;
        float timer = 0f;

        while (fadecolor.a < 1f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(0, 1, timer);
            fadeImage.color = fadecolor;
            yield return null;
        }
    }

    ///<summary>
    ///Image객체에 페이드 인 효과를 주는 함수
    ///</summary>
    public IEnumerator FadeIn(Image fadeImage, float fadeTime)
    {
        Color fadecolor = fadeImage.color;
        float timer = 0f;

        while (fadecolor.a > 0f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(1, 0, timer);
            fadeImage.color = fadecolor;
            yield return null;
        }
        fadeImage.raycastTarget = false;
    }

    ///<summary>
    ///Text객체에 페이드 아웃 효과를 주는 함수
    ///</summary>
    public IEnumerator FadeOut(Text fadeText, float fadeTime)
    {
        Color fadecolor = fadeText.color;
        float timer = 0f;

        while (fadecolor.a < 1f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(0, 1, timer);
            fadeText.color = fadecolor;
            yield return null;
        }
    }

    ///<summary>
    ///Text객체에 페이드 인 효과를 주는 함수
    ///</summary>
    public IEnumerator FadeIn(Text fadeText, float fadeTime)
    {
        Color fadecolor = fadeText.color;
        float timer = 0f;

        while (fadecolor.a > 0f)
        {
            timer += Time.deltaTime / fadeTime;
            fadecolor.a = Mathf.Lerp(1, 0, timer);
            fadeText.color = fadecolor;
            yield return null;
        }
    }

    public IEnumerator BattleStartEffect(Camera cam)
    {
        while (cam.fieldOfView > 30f)
        {
            cam.fieldOfView -= 200 * Time.deltaTime;
            yield return null;
        }

        while (cam.fieldOfView < 100f)
        {
            cam.fieldOfView += 150 * Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator BattleEndEffect(Camera cam)
    {
        while (cam.fieldOfView > 62f + float.Epsilon)
        {
            cam.fieldOfView -= 200 * Time.deltaTime;
            yield return null;
        }
        cam.fieldOfView = 62f;
    }

    public IEnumerator ScaleUpDown(GameObject gameObject, float speed, float minscale, float maxscale, Vector3 startscale)
    {
        while (gameObject.transform.localScale.x < maxscale && gameObject.transform.localScale.y < maxscale)
        {
            gameObject.transform.localScale += new Vector3(speed * Time.deltaTime, speed * Time.deltaTime, 0);
            yield return null;
        }
        while (gameObject.transform.localScale.x > minscale && gameObject.transform.localScale.y > minscale)
        {
            gameObject.transform.localScale -= new Vector3(speed * Time.deltaTime, speed * Time.deltaTime, 0);
            yield return null;
        }

        gameObject.transform.localScale = startscale;
    }

    public IEnumerator EnemyEvade(Text missText)
    {
        RectTransform rect = missText.GetComponent<RectTransform>();
        rect.localScale = new Vector2(0.5f, 0.5f);
        yield return StartCoroutine(Miss(missText));
        rect.localScale = Vector2.one;
    }

    public IEnumerator PlayerEvade(Text missText)
    { 
        yield return StartCoroutine(Miss(missText));
    }

    public IEnumerator EnemyHitEffect(SpriteRenderer enemySp, float time)
    {
        enemySp.color = Color.red;
        yield return new WaitForSeconds(time);
        enemySp.color = Color.white;
    }

    public IEnumerator ShakeEffect(Transform targetTr, float shakeAmount, float shakeTime)
    {
        Vector3 initalPostion = targetTr.position;

        while (shakeTime > 0)
        {
            targetTr.position = Random.insideUnitSphere * shakeAmount + initalPostion;
            shakeTime -= Time.deltaTime;
            yield return null;
        }
        targetTr.position = initalPostion;
    }

    public IEnumerator Miss(Text text)
    {
        text.color = Color.white;
        RectTransform rect = text.GetComponent<RectTransform>();
        float speed = 300;

        while (rect.anchoredPosition.y < 400)
        {
            rect.anchoredPosition += new Vector2(0, speed * Time.deltaTime);
            text.color -= new Color(0, 0, 0, 0.05f);
            yield return null;
        }
        text.color = Color.clear;
        rect.anchoredPosition = Vector3.zero;
    }

    public IEnumerator Fade(Text text)
    {
        yield return StartCoroutine(FadeOut(text, 1f));
        yield return StartCoroutine(FadeIn(text, 1f));
    }
}
