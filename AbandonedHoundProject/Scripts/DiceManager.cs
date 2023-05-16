using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

///<summary>
///주사위 굴림을 호출받을 때 주사위 굴림 애니메이션 실행 및 결과값을 반환하는 클래스
///</summary>
public class DiceManager : Singleton<DiceManager>
{
    const int minDiceScale = 1; //최소 주사위 눈금
    const int maxDiceScale = 6; //최대 주사위 눈금
    public int dicePoint; //주사위 점수

    bool isTouch;
    GameObject dice; //주사위 오브젝트
    Transform diceSpawnTr; //주사위 스폰 위치
    Image dicebackImage; //주사위 카메라 배경 이미지
    Text dicePointtext; //주사위 눈금 표시 텍스트
    GameObject folder; //주사위 UI 프리팹 리소스
    GameObject tempDiceUI; //Instantiate될 주사위 UI 프리팹 리소스를 저장해둘 전역 GameObject

    void Awake()
    {
        dice = Resources.Load("Dice") as GameObject;
        folder = Resources.Load("DiceUIObject") as GameObject;
    }

    public void InputTouch(BaseEventData eventData)
    {
        isTouch = true;
    }

    ///<summary>
    ///주사위 개수를 입력받아 그에 맞는 주사위 굴림을 실행하는 함수
    ///</summary>
    ///<param name="count">몇 개의 주사위를 돌릴 것인지 확인하는 매개변수</param> 
    public int DiceRoll(int count)
    {
        //주사위 관련 UI를 주사위 생성과 동시에 같이 생성
        tempDiceUI = Instantiate(folder);

        diceSpawnTr = tempDiceUI.transform.GetChild(2).GetComponent<Transform>();
        dicebackImage = tempDiceUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
        dicePointtext = tempDiceUI.transform.GetChild(0).transform.GetChild(1).GetComponent<Text>();
        dicePointtext.gameObject.SetActive(false);
               
        StartCoroutine(DiceUIFadeIn());

        switch (count)
        {
            case 1:
                dicePoint = Random.Range(minDiceScale, maxDiceScale + 1);
                StartCoroutine(DiceRotate1(dicePoint));
                break;
            case 2:
                int point1 = Random.Range(minDiceScale, maxDiceScale + 1);
                int point2 = Random.Range(minDiceScale, maxDiceScale + 1);
                StartCoroutine(DiceRotate2(point1, point2));
                dicePoint = point1 + point2;
                break;
        }

        return dicePoint;
    }

    ///<summary>
    ///주사위 1개를 굴릴 때 사용되는 함수
    ///</summary>
    IEnumerator DiceRotate1(int point)
    {
        GameObject dicetemp = Instantiate(dice, diceSpawnTr.position, Quaternion.identity);

        iTween.MoveTo(dicetemp, iTween.Hash("y", -0.2f, "time", 2, "delay", 0.2f));

        yield return new WaitForSeconds(3.0f);

        //주사위가 굴러갈 때 터치 이벤트 트리거 동적 생성
        EventTrigger eventTrigger = dicebackImage.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry_PointerDown = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        entry_PointerDown.callback.AddListener((data) => { InputTouch((PointerEventData)data); });
        eventTrigger.triggers.Add(entry_PointerDown);

        while (true)
        {
            dicetemp.transform.Rotate(new Vector3(15f, Random.Range(0f, 20f), 0));

            if (isTouch)
            {
                switch (point)
                {
                    case 1:
                        iTween.RotateTo(dicetemp, new Vector3(0, 180, 0), 0.1f);
                        break;
                    case 2:
                        iTween.RotateTo(dicetemp, new Vector3(0, 90, 270), 0.1f);
                        break;
                    case 3:
                        iTween.RotateTo(dicetemp, new Vector3(0, 90, 0), 0.1f);
                        break;
                    case 4:
                        iTween.RotateTo(dicetemp, new Vector3(0, 270, 0), 0.1f);
                        break;
                    case 5:
                        iTween.RotateTo(dicetemp, new Vector3(0, 90, 90), 0.1f);
                        break;
                    case 6:
                        iTween.RotateTo(dicetemp, new Vector3(0, 0, 180), 0.1f);
                        break;
                }
                break;
            }

            yield return null;
        }

        dicePointtext.gameObject.SetActive(true);
        dicePointtext.text = dicePoint.ToString();

        yield return new WaitForSeconds(3.0f);

        Destroy(dicetemp);
        dicePointtext.gameObject.SetActive(false);

        StartCoroutine(DiceUIFadeOut());
    }

    ///<summary>
    ///주사위 2개를 굴릴 때 사용되는 함수
    ///</summary>
    IEnumerator DiceRotate2(int point1, int point2)
    {
        GameObject dicetemp1 = Instantiate(dice, diceSpawnTr.position + new Vector3(-2, 0, 0), Quaternion.identity);
        GameObject dicetemp2 = Instantiate(dice, diceSpawnTr.position + new Vector3(2, 0, 0), Quaternion.identity);

        iTween.MoveTo(dicetemp1, iTween.Hash("y", -0.2f, "time", 3, "delay", 0.2f));
        iTween.MoveTo(dicetemp2, iTween.Hash("y", -0.2f, "time", 3, "delay", 0.2f));

        yield return new WaitForSeconds(3.0f);

        EventTrigger eventTrigger = dicebackImage.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry_PointerDown = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        entry_PointerDown.callback.AddListener((data) => { InputTouch((PointerEventData)data); });
        eventTrigger.triggers.Add(entry_PointerDown);

        while (true)
        {
            dicetemp1.transform.Rotate(new Vector3(15f, Random.Range(0f, 20f), 0));
            dicetemp2.transform.Rotate(new Vector3(15f, Random.Range(0f, 20f), 0));

            if (isTouch)
            {
                int[] diceScale = new int[2];
                diceScale[0] = point1;
                diceScale[1] = point2;

                for (int i = 0; i < 2; i++)
                {
                    switch (diceScale[i])
                    {
                        case 1:
                            if (i == 0)
                                iTween.RotateTo(dicetemp1, new Vector3(0, 180, 0), 0.1f);
                            else
                                iTween.RotateTo(dicetemp2, new Vector3(0, 180, 0), 0.1f);
                            break;
                        case 2:
                            if (i == 0)
                                iTween.RotateTo(dicetemp1, new Vector3(0, 90, 270), 0.1f);
                            else
                                iTween.RotateTo(dicetemp2, new Vector3(0, 90, 270), 0.1f);
                            break;
                        case 3:
                            if (i == 0)
                                iTween.RotateTo(dicetemp1, new Vector3(0, 90, 0), 0.1f);
                            else
                                iTween.RotateTo(dicetemp2, new Vector3(0, 90, 0), 0.1f);
                            break;
                        case 4:
                            if (i == 0)
                                iTween.RotateTo(dicetemp1, new Vector3(0, 270, 0), 0.1f);
                            else
                                iTween.RotateTo(dicetemp2, new Vector3(0, 270, 0), 0.1f);
                            break;
                        case 5:
                            if (i == 0)
                                iTween.RotateTo(dicetemp1, new Vector3(0, 90, 90), 0.1f);
                            else
                                iTween.RotateTo(dicetemp2, new Vector3(0, 90, 90), 0.1f);
                            break;
                        case 6:
                            if (i == 0)
                                iTween.RotateTo(dicetemp1, new Vector3(0, 0, 180), 0.1f);
                            else
                                iTween.RotateTo(dicetemp2, new Vector3(0, 0, 180), 0.1f);
                            break;
                    }
                }

                break;
            }

            yield return null;
        }

        dicePointtext.gameObject.SetActive(true);
        dicePointtext.text = dicePoint.ToString();

        yield return new WaitForSeconds(1.0f);

        Destroy(dicetemp1);
        Destroy(dicetemp2);
        dicePointtext.gameObject.SetActive(false);

        StartCoroutine(DiceUIFadeOut());
    }

    ///<summary>
    ///페이드 인 효과 함수
    ///</summary>
    IEnumerator DiceUIFadeIn()
    {
        float FadeTime = 1f; // Fade효과 재생시간
        float start = 0;
        float end = 0.6f;
        float time = 0f;
        Color fadecolor = dicebackImage.color;        

        fadecolor.a = Mathf.Lerp(start, end, time);

        while (fadecolor.a < 1f)
        {
            time += Time.deltaTime / FadeTime;
            fadecolor.a = Mathf.Lerp(start, end, time);
            dicebackImage.color = fadecolor;
            yield return null;  
        }
    }

    ///<summary>
    ///페이드 아웃 효과 함수
    ///</summary>
    IEnumerator DiceUIFadeOut()
    {
        float FadeTime = 1f; // Fade효과 재생시간
        float start = 0.6f;
        float end = 0f;
        float time = 0f;
        Color fadecolor = dicebackImage.color;

        fadecolor.a = Mathf.Lerp(start, end, time);

        while (fadecolor.a > 0f)
        {
            time += Time.deltaTime / FadeTime;
            fadecolor.a = Mathf.Lerp(start, end, time);
            dicebackImage.color = fadecolor;
            yield return null;

        }

        Destroy(tempDiceUI);
        dicePoint = 0;

        switch(SceneManager.GetActiveScene().name)
        {
            case "TownScene":
                Combination.Instance.diceEnd = true;
                break;
            case "DungeonScene":
                DungeonManager.Instance.dungeonMainUICanvas.gameObject.SetActive(true);
                break;
            case "BattleScene":
                BattleManager.Instance.battleSceneCanvas.gameObject.SetActive(true);
                break;
        }
        
        isTouch = false;
    }
}
